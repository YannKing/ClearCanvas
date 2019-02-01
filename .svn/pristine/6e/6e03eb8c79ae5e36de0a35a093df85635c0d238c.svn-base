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

using ClearCanvas.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace ClearCanvas.Common
{
    /// <summary>
    /// Loads plugin assemblies dynamically from disk and exposes meta-data about the set of installed
    /// plugins, extension points, and extensions to the application.
    /// </summary>
    public sealed class PluginManager
    {
        #region BackgroundAssemblyLoader

        private class BackgroundAssemblyLoader
        {
            private readonly PluginManager _owner;
            private bool _running;
            private volatile bool _cancelRequested;

            public BackgroundAssemblyLoader(PluginManager owner)
            {
                _owner = owner;
            }

            public void Run()
            {
                if (_running)
                    return;

                // if all plugin assemblies are loaded, nothing to do
                if (_owner.Plugins.All(p => p.Assembly.IsResolved))
                    return;

                Platform.Log(LogLevel.Debug, "PluginManager: Starting background assembly loading.");
                ThreadPool.QueueUserWorkItem(state => LoadAssemblies());
                _running = true;
            }

            public void Cancel()
            {
                if (!_running || _cancelRequested)
                    return;

                _cancelRequested = true;
                _running = false;

                Platform.Log(LogLevel.Debug, "PluginManager: Suspending background assembly loading.");
            }

            private void LoadAssemblies()
            {
                foreach (var plugin in _owner.Plugins.Where(p => !p.Assembly.IsResolved))
                {
                    if (_cancelRequested)
                        return;

                    try
                    {
                        plugin.Assembly.Resolve();
                    }
                    catch (Exception e)
                    {
                        Platform.Log(LogLevel.Error, e);
                    }
                }
            }
        }

        #endregion

        private readonly List<PluginInfo> _plugins = new List<PluginInfo>();
        private readonly List<ExtensionInfo> _extensions = new List<ExtensionInfo>();
        private readonly List<ExtensionPointInfo> _extensionPoints = new List<ExtensionPointInfo>();
        private readonly string _pluginDir;

        private readonly object _syncLock = new object();
        private readonly PluginLoader _loader;
        private volatile bool _pluginsLoaded;

        private readonly BackgroundAssemblyLoader _backgroundAssemblyLoader;

        /// <summary>插件管理器
        /// Yann 2018年12月25日 10:10:40
        /// </summary>
        /// <param name="pluginDir">插件目录</param>
        internal PluginManager(string pluginDir)
        {
            _pluginDir = pluginDir;

            string[] alternateCacheLocations;
            _loader = new PluginLoader(pluginDir, GetMetadataCacheFilePath(out alternateCacheLocations), alternateCacheLocations);
            _backgroundAssemblyLoader = new BackgroundAssemblyLoader(this);
        }

        /// <summary>
        /// Gets the plugin manager instance in use by the framework.
        /// </summary>
        public static PluginManager Instance
        {
            get { return Platform.PluginManager; }
        }

        #region Public API

        /// <summary>
        /// Gets information about the set of all installed plugins.
        /// </summary>
        /// <remarks>
        /// If plugins have not yet been loaded into memory, querying this property will cause them to be loaded.
        /// </remarks>
        public IList<PluginInfo> Plugins
        {
            get
            {
                EnsurePluginInfoLoaded();
                return _plugins.AsReadOnly();
            }
        }

        /// <summary>
        /// Gets information about the set of extensions defined across all installed plugins,
        /// including disabled and unlicensed extensions.
        /// </summary>
        /// <remarks>
        /// If plugins have not yet been loaded into memory, querying this property will cause them to be loaded.
        /// </remarks>
        public IList<ExtensionInfo> Extensions
        {
            get
            {
                EnsurePluginInfoLoaded();
                return _extensions.AsReadOnly();
            }
        }

        /// <summary>
        /// Gets information about the set of extension points defined across all installed plugins.  
        /// </summary>
        /// <remarks>
        /// If plugins have not yet been loaded into memory, querying this property will cause them to be loaded.
        /// </remarks>
        public IList<ExtensionPointInfo> ExtensionPoints
        {
            get
            {
                EnsurePluginInfoLoaded();
                return _extensionPoints.AsReadOnly();
            }
        }

        /// <summary>
        /// Occurs when a plugin is loaded.
        /// </summary>
        public event EventHandler<PluginLoadedEventArgs> PluginLoaded
        {
            add
            {
                lock (_syncLock)
                {
                    _loader.PluginLoaded += value;
                }
            }
            remove
            {
                lock (_syncLock)
                {
                    _loader.PluginLoaded -= value;
                }
            }
        }

        /// <summary>
        /// Enables or disables loading of any outstanding plugin assemblies on a background thread.
        /// </summary>
        public void EnableBackgroundAssemblyLoading(bool enable)
        {
            lock (_syncLock)
            {
                if (enable)
                    _backgroundAssemblyLoader.Run();
                else
                    _backgroundAssemblyLoader.Cancel();
            }
        }

        #endregion

        #region Helpers

        private void EnsurePluginInfoLoaded()
        {
            if (!_pluginsLoaded)
            {
                lock (_syncLock)
                {
                    if (!_pluginsLoaded)
                    {
                        LoadPluginInfo();
                    }
                }
            }
        }

        /// <summary>加载插件信息</summary>
        private void LoadPluginInfo()
        {
            if (!Directory.Exists(_pluginDir))
                throw new PluginException(SR.ExceptionPluginDirectoryNotFound);

            _plugins.AddRange(_loader.LoadPluginInfo());
            if (_plugins.Count == 0)
            {
                // If there are no plugins, there's nothing left to do ... but they were still loaded.
                //如果没有插件，就没有什么可做的......但它们仍然被加载。
                _pluginsLoaded = true;
                return;
            }

            // compile lists of all extension points and extensions
            //编译所有扩展点和扩展的列表
            var extensions = new List<ExtensionInfo>(_plugins.SelectMany(p => p.Extensions));
            var points = new List<ExtensionPointInfo>(_plugins.SelectMany(p => p.ExtensionPoints));

            // hack: add points and extensions from ClearCanvas.Common, which isn't technically a plugin
            //hack：从ClearCanvas.Common添加点和扩展，这在技术上不是一个插件
            PluginInfo.DiscoverExtensionPointsAndExtensions(GetType().Assembly, points, extensions);

            // #742: order the extensions according to the XML configuration
            //＃742：根据XML配置对扩展进行排序
            var ordered = ExtensionSettings.Default.OrderExtensions(extensions);

            // create global extension list
            //创建全局扩展列表
            _extensions.AddRange(ordered);

            // points do not need to be ordered 
            _extensionPoints.AddRange(points);

            _pluginsLoaded = true;
        }

        /// <summary>获取元数据缓存文件路径
        /// Yann 2018年12月25日 10:11:23
        /// </summary>
        /// <param name="alternates"></param>
        /// <returns></returns>
        private static string GetMetadataCacheFilePath(out string[] alternates)
        {
            var exePath = Process.GetCurrentProcess().MainModule.FileName;
            using (var sha = new SHA256CryptoServiceProvider2())
            {
                // since this is used to generate a file path, we must limit the length of the generated name so it doesn't exceed max path length
                // we don't simply use MD5 because it throws an exception if the OS has strict cryptographic policies in place (e.g. FIPS)
                // note: truncation of SHA256 seems to be an accepted method of producing a shorter hash - see notes in HashUtilities

                //因为这用于生成文件路径，所以我们必须限制生成的名称的长度，使其不超过最大路径长度,
                //我们不是简单地使用MD5，因为如果操作系统具有严格的加密策略（例如FIPS），它会引发异常 
                //注意：截断SHA256似乎是生成较短哈希的可接受方法 - 请参阅HashUtilities中的注释
                var hash = StringUtilities.ToHexString(sha.ComputeHash(Encoding.Unicode.GetBytes(exePath)), 0, 16);

                // alternate locations are treated as read-only pre-generated cache files (e.g. for Portable workstation)
                //备用位置被视为只读的预生成缓存文件（例如，用于便携式工作站）
                alternates = new[] { Path.Combine(Platform.PluginDirectory, "pxpx", hash) };

                // return the main location, which must be writable
                // 返回主要位置，必须是可写的
                var resultVal = Path.Combine(Platform.ApplicationDataDirectory, "pxpx", hash);
                return resultVal;
            }
        }

        #endregion
    }
}