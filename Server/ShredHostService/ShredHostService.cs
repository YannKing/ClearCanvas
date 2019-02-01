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

using NewLife.Log;
using System;
using System.IO;
using System.ServiceProcess;

namespace ClearCanvas.Server.ShredHostService
{
    public partial class ShredHostService : ServiceBase
    {
        /// <summary>服务操作超时</summary>
        private const int _serviceOperationTimeout = 3 * 60 * 1000;

        public static void InternalStart()
        {
            // the default startup path is in the system folder
            // we need to change this to be able to scan for plugins and to log

            //默认启动路径位于系统文件夹中 我们需要更改它以便能够扫描插件和记录
            string startupPath = AppDomain.CurrentDomain.BaseDirectory;
            Directory.SetCurrentDirectory(startupPath);
            ShredHost.ShredHost.Start();
        }

        public static void InternalStop()
        {
            ShredHost.ShredHost.Stop();
        }

        public ShredHostService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            XTrace.WriteLine($" ShredHostService.OnStart {args.Join(Environment.NewLine)} ");
            RequestAdditionalTime(_serviceOperationTimeout);

            InternalStart();
        }

        protected override void OnStop()
        {
            XTrace.WriteLine($" override  OnStop");
            RequestAdditionalTime(_serviceOperationTimeout);

            InternalStop();
        }
    }
}