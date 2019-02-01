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
using System;
using System.Linq;
using LogLevel = ClearCanvas.Common.LogLevel;

namespace ClearCanvas.Server.ShredHost
{
    internal class ExtensionScanner : MarshalByRefObject
    {
        /// <summary>使用ExtensionPoint类型扫描插件文件夹以查找扩展名。
        /// Uses the ExtensionPoint type to scan the plugins folder for extensions.
        /// </summary>
        /// <returns>一个可枚举的集合，其中包含有助于加载器的每个扩展的信息将它们加载到AppDomain中。如果未找到扩展名，则返回null。
        /// 
        /// An enumerable collection that contains information on each extension that will help the loader
        /// load them into the AppDomain. If no extensions are found, null is returned.</returns>
        public ShredStartupInfoList ScanExtensions()
        {
            Platform.Log(LogLevel.Debug, this.GetType().ToString() + ":" + this.GetType().Name + " in AppDomain [" + AppDomain.CurrentDomain.FriendlyName + "]");
            XTrace.WriteLine($"{ this.GetType()}：{this.GetType().Name} in AppDomain [\"{ AppDomain.CurrentDomain.FriendlyName }\"]");

            var shredInfoList = new ShredStartupInfoList();
            var xp = new ShredExtensionPoint();
            var shredObjects = xp.CreateExtensions();
            foreach (var shredObject in shredObjects)
            {
                var shredType = shredObject.GetType();
                var asShred = shredObject as IShred;
                if (asShred == null)
                {
                    Platform.Log(LogLevel.Debug, "Shred extension '{0}' does not implement IShred.", shredType.FullName);
                    XTrace.WriteLine($"Shred扩展名“{shredType.FullName}”未实现IShred。");
                    continue;
                }

                var shredIsolation = shredType.GetCustomAttributes(typeof(ShredIsolationAttribute), true).OfType<ShredIsolationAttribute>().FirstOrDefault();
                var shredIsolationLevel = shredIsolation == null ? ShredIsolationLevel.OwnAppDomain : shredIsolation.Level;

                if (shredIsolationLevel != ShredIsolationLevel.None)
                {
                    Platform.Log(LogLevel.Info, "Shred {0} is running in a seperate app domain", shredType.Name);
                    XTrace.WriteLine($"Shred {shredType.Name}正在单独的应用程序域中运行");
                }

                var assemblyPath = new Uri(shredType.Assembly.CodeBase);
                var startupInfo = new ShredStartupInfo(assemblyPath, ((IShred)shredObject).GetDisplayName(), shredType.FullName, shredIsolationLevel);
                shredInfoList.Add(startupInfo);
            }

            return shredInfoList;
        }
    }
}
