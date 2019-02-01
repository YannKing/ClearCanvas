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
using ClearCanvas.Common.Shreds;
using NewLife.Log;
using NewLife.Serialization;
using System;
using System.Threading;
using LogLevel = ClearCanvas.Common.LogLevel;

namespace ClearCanvas.Server.ShredHost
{
    internal class ShredController : MarshalByRefObject
    {

        public ShredController(ShredStartupInfo startupInfo)
        {
            Platform.CheckForNullReference(startupInfo, "startupInfo");

            _startupInfo = startupInfo;
            _id = ShredController.NextId;
            _runningState = RunningState.Stopped;
            XTrace.WriteLine($"ShredController构造函数属性：{_startupInfo.ToJson()}");

        }

        static ShredController()
        {
            _nextId = 101;
        }

        public bool Start()
        {
            XTrace.WriteLine($"启动线程{Environment.NewLine}");
            XTrace.WriteLine($" _startupInfo.ShredTypeName {_startupInfo.ShredTypeName}");
            XTrace.WriteLine($"_lockRunningState {_lockRunningState}");
            XTrace.WriteLine($"_startupInfo.IsolationLevel {_startupInfo.IsolationLevel.ToString()}");

            lock (_lockRunningState)
            {
                if (RunningState.Running == _runningState || RunningState.Transition == _runningState)
                    return (RunningState.Running == _runningState);

                _runningState = RunningState.Transition;
            }

            if (_startupInfo.IsolationLevel == ShredIsolationLevel.OwnAppDomain)
            {
                _domain = AppDomain.CreateDomain(_startupInfo.ShredTypeName);

                XTrace.WriteLine($"START：{_domain.ToJson()}");
                _shredObject = (IShred)_domain.CreateInstanceFromAndUnwrap(_startupInfo.AssemblyPath.LocalPath, _startupInfo.ShredTypeName);
            }
            else
            {
                _shredObject = (IShred)AppDomain.CurrentDomain.CreateInstanceFromAndUnwrap(_startupInfo.AssemblyPath.LocalPath, _startupInfo.ShredTypeName);
            }

            // cache the shred's details so that even if the shred is stopped and unloaded
            // we still have it's display name 
            //缓存Shred的详细信息，以便即使Shred被停止和卸载也是如此我们还有它的显示名称
            _shredCacheObject = new ShredCacheObject(_shredObject.GetDisplayName(), _shredObject.GetDescription());

            // create the thread and start it
            _thread = new Thread(StartupShred)
            {
                Name = $"{_shredCacheObject.GetDisplayName()}"
            };

            XTrace.WriteLine($"启动线程：Thread.Start：Name：  {_thread.Name}");

            _thread.Start(this);

            lock (_lockRunningState)
            {
                _runningState = RunningState.Running;
            }

            return (RunningState.Running == _runningState);
        }


        public bool Stop()
        {
            XTrace.WriteLine($"停止线程");
            lock (_lockRunningState)
            {
                if (RunningState.Stopped == _runningState || RunningState.Transition == _runningState)
                    return (RunningState.Running == _runningState);

                _runningState = RunningState.Transition;
            }

            _shredObject.Stop();
            _thread.Join();
            if (_domain != null)
            {
                AppDomain.Unload(_domain);
                _domain = null; // need to explicity set to null, otherwise any references to it in the future will throw an exception
                //需要将explicity设置为null，否则将来对它的任何引用都会抛出异常
            }

            _shredObject = null;
            _thread = null;

            lock (_lockRunningState)
            {
                _runningState = RunningState.Stopped;
            }

            return (RunningState.Running == _runningState);
        }

        private void StartupShred(object threadData)
        {

            XTrace.WriteLine($"StartupShred：");

            ShredController shredController = threadData as ShredController;
            XTrace.WriteLine($"{shredController?.StartupInfo.ShredName}");

            IWcfShred wcfShred = shredController.Shred as IWcfShred;
            XTrace.WriteLine($"{wcfShred.ToJson()}");
            try
            {
                if (wcfShred != null)
                {
                    wcfShred.SharedHttpPort = ShredHostServiceSettings.Instance.SharedHttpPort;
                    wcfShred.SharedTcpPort = ShredHostServiceSettings.Instance.SharedTcpPort;
                    wcfShred.ServiceAddressBase = ShredHostServiceSettings.Instance.ServiceAddressBase;
                }

                BeginStartupTimer();

                shredController.Shred.Start();
            }
            catch (Exception e)
            {
                Platform.Log(LogLevel.Error, e, "Unexpected exception when starting up Shred {0}", shredController.Shred.GetDescription());
                XTrace.WriteLine($"启动Shred：{shredController.Shred.GetDescription()}" +
                                 $"{Environment.NewLine} 时出现意外异常");

                XTrace.WriteException(e);
            }
            finally
            {
                EndStartupTimer();
            }
        }

        private void BeginStartupTimer()
        {
            XTrace.WriteLine($"BeginStartupTimer=>开启启动30秒计时");

            _startingUp = true;
            TimerCallback callback = o =>
            {
                var shred = _shredObject;
                if (_startingUp && shred != null)
                {
                    Platform.Log(LogLevel.Warn, "The shred '{0}' has not returned from its Start() method after 30 seconds; shreds should start up and return quickly and should never block until Stop() is called.", shred.GetType().FullName);

                    XTrace.WriteLine($"30秒后，shred'{shred.GetType().FullName}'没有从其Start()方法返回;" +
                                     $"碎片应该启动并快速返回，并且在调用Stop()之前永远不应该阻塞。");

                }
                EndStartupTimer();
            };

            //Use a timer on the thread pool to check that the shred's Start method returned after a reasonable startup time.
            //使用线程池上的计时器检查在合理的启动时间后返回的Shred的Start方法。
            var thirtySeconds = TimeSpan.FromSeconds(300);
            _startupTimer = new Timer(callback, null, thirtySeconds, thirtySeconds);
        }

        private void EndStartupTimer()
        {
            try
            {
                _startingUp = false;
                _startupTimer.Dispose();
                _startupTimer = null;
                XTrace.WriteLine($"释放Timer定时器");
            }
            catch { /**/ }
        }

        private class ShredCacheObject : IShred
        {
            public ShredCacheObject(string displayName, string description)
            {
                _displayName = displayName;
                _description = description;
            }

            #region Private fields

            private string _displayName;
            private string _description;

            #endregion

            #region IShred Members

            public void Start()
            {
                throw new Exception("该方法或操作未实现。" +
                                    "The method or operation is not implemented.");
            }

            public void Stop()
            {
                throw new Exception("该方法或操作未实现。" +
                                    "The method or operation is not implemented.");
            }

            public string GetDisplayName()
            {
                return _displayName;
            }

            public string GetDescription()
            {
                return _description;
            }

            #endregion
        }

        #region Private fields

        private readonly object _lockRunningState = new object();
        private RunningState _runningState;

        #endregion

        #region Properties

        private Thread _thread;
        private IShred _shredObject;
        private ShredCacheObject _shredCacheObject;
        private AppDomain _domain;
        private ShredStartupInfo _startupInfo;
        private int _id;
        private static int _nextId;

        private bool _startingUp;
        private Timer _startupTimer;

        protected static int NextId
        {
            get { return _nextId++; }
        }

        public int Id
        {
            get { return _id; }
        }

        public ShredStartupInfo StartupInfo
        {
            get { return _startupInfo; }
        }

        public IShred Shred
        {
            get
            {
                if (null == _shredObject)
                    return _shredCacheObject;
                else
                    return _shredObject;
            }
        }

        public WcfDataShred WcfDataShred
        {
            get
            {
                return new WcfDataShred(this.Id, this.Shred.GetDisplayName(), this.Shred.GetDescription(), (RunningState.Running == _runningState) ? true : false);
            }
        }

        #endregion
    }
}