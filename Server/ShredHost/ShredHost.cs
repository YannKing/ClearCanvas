#region License

// Copyright (c) 2013, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This file is part of the ClearCanvas RIS/PACS open source project.
//
// The ClearCanvas RIS/PACS open source project is free software: you can
// redistribute it and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
//
// The ClearCanvas RIS/PACS open source project is distributed in the hope that it
// will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General
// Public License for more details.
//
// You should have received a copy of the GNU General Public License along with
// the ClearCanvas RIS/PACS open source project.  If not, see
// <http://www.gnu.org/licenses/>.

#endregion

using ClearCanvas.Common;
using NewLife.Log;
using System;
using System.Reflection;
using LogLevel = ClearCanvas.Common.LogLevel;


namespace ClearCanvas.Server.ShredHost
{
    public static class ShredHost
    {
        #region Private Members 私有成员
        private static ShredControllerList _shredInfoList;
        private static ServiceEndpointDescription _sed;
        private static RunningState _runningState;
        private static object _lockObject = new object();
        private static bool _shredHostWCFInitialized = false;
        #endregion

        /// <summary>启动ShredHost例程。
        /// Starts the ShredHost routine.
        /// </summary>
        /// <returns>true  - 如果ShredHost当前正在运行，则为false  - 如果ShredHost已停止。
        /// true - if the ShredHost is currently running, false - if ShredHost is stopped.</returns>
        public static bool Start()
        {
            XTrace.WriteLine($"ShredHost.Start()被执行");
            // install the unhandled exception event handler //安装未处理的异常事件处理程序
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += MyUnhandledExceptionEventHandler;//未处理异常捕获

            XTrace.WriteLine("为当前应用程序域添加未处理异常捕获处理函数：MyUnhandledExceptionEventHandler ");

            lock (_lockObject)
            {
                if (RunningState.Running == _runningState || RunningState.Transition == _runningState)
                    return (RunningState.Running == _runningState);

                _runningState = RunningState.Transition;
            }

            Platform.Log(LogLevel.Info, "Starting up in AppDomain [" + AppDomain.CurrentDomain.FriendlyName + "]");
            XTrace.WriteLine("在AppDomain中启动当前应用程序域[" + currentDomain.FriendlyName + "]");


            // the ShredList and shreds objects are proxy objects that actually exist
            // in the secondary AppDomain
            //ShredList和Shreds对象是实际存在的代理对象在辅助AppDomain中
            //AppDomain stagingDomain = AppDomain.CreateDomain("StagingDomain");
            ExtensionScanner scanner = (ExtensionScanner)AppDomain.CurrentDomain.CreateInstanceFromAndUnwrap(Assembly.GetExecutingAssembly().Location,
                "ClearCanvas.Server.ShredHost.ExtensionScanner");
            ShredStartupInfoList shredStartupInfoList = null;

            try
            {
                shredStartupInfoList = scanner.ScanExtensions();
                XTrace.WriteLine($"扫描扩展 {shredStartupInfoList.Join(Environment.NewLine)}");
            }
            catch (PluginException pluginException)
            {
                // There was a problem loading the plugins, including if there were no plugins found
                // This is an innocuous problem, and just means that there are no shreds to run
                // 加载插件时出现问题，包括是否找不到插件这是一个无害的问题，只是意味着没有可以运行的碎片
                Platform.Log(LogLevel.Warn, pluginException);
                XTrace.WriteLine("警告：插件异常");
                XTrace.WriteException(pluginException);
            }

            StartShreds(shredStartupInfoList);

            // all the shreds have been created, so we can dismantle the secondary domain that was used 
            // for scanning for all Extensions that are shreds
            //所有碎片都已创建，因此我们可以拆除使用的二级域用于扫描所有碎片的扩展
            //AppDomain.Unload(stagingDomain);

            //try
            //{
            //    _sed = WcfHelper.StartHttpHost<ShredHostServiceType, IShredHost>(
            //        "ShredHost", "Host program of multiple independent service-like sub-programs", ShredHostServiceSettings.Instance.ShredHostHttpPort);
            //    _shredHostWCFInitialized = true;
            //    string message = String.Format("The ShredHost WCF service has started on port {0}.", ShredHostServiceSettings.Instance.ShredHostHttpPort);
            //    Platform.Log(LogLevel.Info, message);
            //    Console.WriteLine(message);
            //}
            //catch(Exception e)
            //{
            //    Platform.Log(LogLevel.Error, e);
            //    Console.WriteLine("The ShredHost WCF service has failed to start.  Please check the log for more details.");
            //}

            lock (_lockObject)
            {
                _runningState = RunningState.Running;
            }

            return (RunningState.Running == _runningState);
        }

        /// <summary>停止正在运行的ShredHost。
        /// Stops the running ShredHost.
        /// </summary>
        /// <returns>true  - 如果ShredHost正在运行，则为false  - 如果ShredHost已停止。
        /// true - if the ShredHost is running, false - if the ShredHost is stopped.</returns>
        public static bool Stop()
        {
            lock (_lockObject)
            {
                if (RunningState.Stopped == _runningState || RunningState.Transition == _runningState)
                    return (RunningState.Running == _runningState);

                _runningState = RunningState.Transition;
            }

            // correct sequence should be to stop the WCF host so that we don't
            // receive any more incoming requests
            //正确的顺序应该是停止WCF主机，这样我们就不会接收任何更多的传入请求
            Platform.Log(LogLevel.Info, "ShredHost stop request received");

            XTrace.WriteLine("已收到ShredHost停止请求");

            if (_shredHostWCFInitialized)
            {
                try
                {
                    WcfHelper.StopHost(_sed);
                    Platform.Log(LogLevel.Info, "The ShredHost WCF service has stopped.");
                    XTrace.WriteLine("ShredHost WCF服务已停止。");
                }
                catch (Exception e)
                {
                    Platform.Log(LogLevel.Error, e);
                    XTrace.WriteException(e);
                }
            }

            StopShreds();
            Platform.Log(LogLevel.Info, "Completing ShredHost stop.");
            XTrace.WriteLine("完成ShredHost停止。");

            _shredInfoList.Clear();
            lock (_lockObject)
            {
                _runningState = RunningState.Stopped;
            }

            return (RunningState.Running == _runningState);
        }

        public static bool IsShredHostRunning
        {
            get
            {
                bool isRunning;
                lock (_lockObject)
                {
                    isRunning = (RunningState.Running == _runningState);
                }

                return isRunning;
            }
        }

        public static bool StartShred(WcfDataShred shred)
        {
            Platform.Log(LogLevel.Info, "Attempting to start shred: " + shred.Name);
            XTrace.WriteLine("(Attempting to start shred:)试图开始粉碎：" + shred.Name);
            return ShredControllerList[shred.Id].Start();
        }

        public static bool StopShred(WcfDataShred shred)
        {
            Platform.Log(LogLevel.Info, "Attempting to stop shred: " + shred.Name);
            XTrace.WriteLine("(Attempting to stop shred:)试图阻止碎片： " + shred.Name);
            return ShredControllerList[shred.Id].Stop();
        }

        static ShredHost()
        {
            _shredInfoList = new ShredControllerList();
            _sed = null;
            _runningState = RunningState.Stopped;
        }

        private static void StartShreds(ShredStartupInfoList shredStartupInfoList)
        {
            XTrace.WriteLine($"启动StartShreds {shredStartupInfoList.AllShredStartupInfo.Join(Environment.NewLine)}");
            if (null != shredStartupInfoList)
            {
                // create the data structure that will hold the shreds and their thread, etc. related objects
                // 创建将保存Shred及其线程等相关对象的数据结构
                foreach (ShredStartupInfo shredStartupInfo in shredStartupInfoList)
                {
                    if (null != shredStartupInfo)
                    {
                        // clone the shredStartupInfo structure into the current AppDomain, otherwise, once the StagingDomain 
                        // has been unloaded, the shredStartupInfo structure will be destroyed
                        // 将shredStartupInfo结构克隆到当前AppDomain中，否则，一旦StagingDomain已被卸载，shredStartupInfo结构将被销毁
                        var newShredStartupInfo = new ShredStartupInfo(shredStartupInfo.AssemblyPath, shredStartupInfo.ShredName,
                                                                       shredStartupInfo.ShredTypeName, shredStartupInfo.IsolationLevel);

                        // create the controller that will allow us to start and stop the shred
                        //创建允许我们启动和停止碎片的控制器
                        var shredController = new ShredController(newShredStartupInfo);
                        _shredInfoList.Add(shredController);
                    }
                }
            }

            foreach (ShredController shredController in _shredInfoList)
            {
                XTrace.WriteLine($"shredController =>>{shredController.Id} >> {shredController.StartupInfo.ShredName}");
                shredController.Start();
            }
        }

        private static void StopShreds()
        {
            Exception savedException = null;

            foreach (ShredController shredController in _shredInfoList)
            {
                try
                {
                    string displayName = shredController.Shred.GetDisplayName();
                    Platform.Log(LogLevel.Info, displayName + ": Signalling stop");
                    XTrace.WriteLine(displayName + "：信号停止");
                    shredController.Stop();
                    Platform.Log(LogLevel.Info, displayName + ": Stopped");
                    XTrace.WriteLine(displayName + "：已停止");
                }
                catch (Exception e)
                {
                    Platform.Log(LogLevel.Fatal, e, "Unexepected exception stopping Shred (shred still running): {0}", shredController.Shred.GetDisplayName());
                    XTrace.WriteLine($"意外的异常停止Shred（碎片仍在运行）： {shredController.Shred.GetDisplayName()}");
                    XTrace.WriteException(e);
                    savedException = e;
                }
            }

            if (savedException != null) throw savedException;
        }

        #region Print asms in AD helper f(x)  Print asms in AD helper f(x)
        public static void PrintAllAssembliesInAppDomain(AppDomain ad)
        {
            Assembly[] loadedAssemblies = ad.GetAssemblies();
            Console.WriteLine(@"***** Here are the assemblies loaded in {0} *****\n", ad.FriendlyName);
            XTrace.WriteLine($"***** 这是加载的程序集：{ad.FriendlyName} *****{Environment.NewLine}");
            foreach (Assembly a in loadedAssemblies)
            {
                //Console.WriteLine(@"-> Name: {0}", a.GetName().Name);
                //Console.WriteLine(@"-> Version: {0}", a.GetName().Version);
                XTrace.WriteLine($@"-> Name: {a.GetName().Name}");
                XTrace.WriteLine($@"-> Version: { a.GetName().Version}");
            }
        }
        #endregion

        /// <summary>未处理异常捕获
        /// Yann 2018年12月28日 14:29:49
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        internal static void MyUnhandledExceptionEventHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            Platform.Log(LogLevel.Fatal, e, "Fatal error - unhandled exception in running Shred; ShredHost must terminate");

            XTrace.WriteLine("致命错误 - 运行Shred时出现未处理的异常; ShredHost必须终止");
            XTrace.WriteException(e);
        }

        internal static ShredControllerList ShredControllerList
        {
            get { return _shredInfoList; }
        }
    }
}
