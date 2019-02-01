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
using ClearCanvas.Desktop.View.WinForms;
using NewLife.Log;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClearCanvas.Desktop.Executable
{
    internal class Program
    {
        /// <summary>应用程序的主要入口点。</summary>
        [STAThread]
        private static void Main(string[] args)
        {
            //始终至少尝试让我们的应用程序代码处理异常。
            //将此设置为“catch”表示Application.ThreadException事件
            //将首先开火，实际上导致应用程序立即崩溃并关闭。
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
            XTrace.UseWinForm();
            XTrace.WriteLine($"Main输入参数：{args.Join(Environment.NewLine)}");

#if !MONO
            if (args.Length == 0)
            {
                SplashScreenManager.DisplaySplashScreen();//显示启动画面
            }
                
#endif
            Platform.PluginManager.PluginLoaded += new EventHandler<PluginLoadedEventArgs>(OnPluginProgress);


            /*
             修改日期：2019年1月23日
             修改人：李海川
             目的：支持命令行参数调用，提供三种参数形式
                1. CC.exe
                2. CC.exe 图像路径
                3. CC.exe 用户名 图像路径
             */
            if (args.Length == 0)
            {
                Platform.StartApp(@"ClearCanvas.Desktop.Application", new string[0]);
            }
            else if (args.Length == 1)
            {
                //一个参数，参数为图像文件目录路径。
                Platform.StartApp(@"ClearCanvas.Desktop.Application", args);
            }
            else if (args.Length == 2)
            {
                string[] args1 = new string[args.Length - 1];
                Array.Copy(args, 1, args1, 0, args1.Length);
                //设置用户名
                ClearCanvas.Common.Configuration.SettingsStoreSettingsProvider.UserName = args[0];

                //两个参数,第一个为用户名称，第二个图像文件目录路径。
                Platform.StartApp(@"ClearCanvas.Desktop.Application", args1);
            }


            ////检查命令行参数
            //if (args.Length > 0)
            //{
            //    //为简单起见，这是一个天真的实现（可能需要在将来改变）
            //    //   如果有> 0个参数，则假设第一个参数是类名
            //    //   并将后续参数捆绑到辅助数组中
            //    //   转发到应用程序根类
            //    string[] args1 = new string[args.Length - 1];
            //    Array.Copy(args, 1, args1, 0, args1.Length);

            //    Platform.StartApp(args[0], args1);
            //}
            //else
            //{
            //    Platform.StartApp(@"ClearCanvas.Desktop.Application", new string[0]);
            //}
        }


        private static int count = 0;
        /// <summary>关于插件进度</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnPluginProgress(object sender, PluginLoadedEventArgs e)
        {
            //XTrace.WriteLine($"插件加载次数{count++}");
            Platform.CheckForNullReference(e, "e");
#if !MONO
            SplashScreenManager.SetStatus(e.Message);
            if (e.PluginAssembly != null)
            {
                SplashScreenManager.AddAssemblyIcon(e.PluginAssembly);
                //XTrace.WriteLine($"加载插件名称{e.PluginAssembly.FullName}");
            }
#endif
        }
    }
}