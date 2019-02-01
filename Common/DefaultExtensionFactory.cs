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
using NewLife.Log;
using System;
using System.Collections.Generic;

namespace ClearCanvas.Common
{
    /// <summary><see cref ="IExtensionFactory"/>的默认实现，它从中创建扩展在运行时发现的插件集。</summary>
    /// <remarks>这个类可以安全地用于多个并发线程。</remarks>
    internal class DefaultExtensionFactory : IExtensionFactory
    {
        private volatile IDictionary<TypeRef, List<ExtensionInfo>> _extensionMap;
        private readonly object _syncLock = new object();

        internal DefaultExtensionFactory()
        {
        }

        #region IExtensionFactory Members

        /// <summary>创建扩展指定扩展点并匹配指定过滤器的可用扩展实例。
        /// Creates instances of available extensions that extend the specified extension point and match the specified filter.
        /// </summary>
        /// <param name="extensionPoint">要为其创建扩展的扩展点。
        /// The extension point for which to create extensions.</param>
        /// <param name="filter">An <see cref="ExtensionFilter"/>用于将结果集限制为具有特定特征的扩展。
        /// used to limit the result set to extensions with particular characteristics.</param>
        /// <param name="justOne">指示是否仅返回找到的第一个匹配扩展名。
        /// Indicates whether or not to return only the first matching extension that is found.</param>
        /// <returns>一组扩展实例。
        /// A set of extension instances.</returns>
        /// <remarks>可用扩展是启用和许可的扩展。如果<paramref name ="justOne"/>为true，则返回成功实例化的第一个匹配扩展，没有其他扩展实例化。
        /// 
        /// Available extensions are those which are both enabled and licensed.
        /// If <paramref name="justOne"/> is true, the first matching extension that is successfully instantiated is returned,
        /// an no other extensions are instantiated.
        /// </remarks>
        public object[] CreateExtensions(ExtensionPoint extensionPoint, ExtensionFilter filter, bool justOne)
        {
            // get subset of applicable extensions 获取适用扩展的子集
            var extensions = ListExtensionsHelper(extensionPoint, filter);

            // attempt to instantiate the extension classes 尝试实例化扩展类
            var createdObjects = new List<object>();
            foreach (var extension in extensions)
            {
                if (justOne && createdObjects.Count > 0)
                    break;

                try
                {
                    // instantiate 实例
                    var o = Activator.CreateInstance(extension.ExtensionClass.Resolve());
                    createdObjects.Add(o);
                }
                catch (Exception e)
                {
                    // 实例化失败
                    // 这不应被视为特殊情况
                    // 在某些情况下，实例化可能会因设计而失败（例如，扩展仅设计为在特定平台上运行）

                    // instantiation failed
                    // this should not be considered an exceptional circumstance
                    // instantiation may fail by design in some cases (e.g extension is designed only to run on a particular platform)
                    Platform.Log(LogLevel.Debug, e);
                    XTrace.WriteException(e);
                    XTrace.WriteLine("===================");
                }
            }

            return createdObjects.ToArray();
        }

        /// <summary>列出与指定的<paramref name ="filter"/>匹配的指定<paramref name ="extensionPoint"/>的所有可用扩展名。</summary>
        /// 
        /// <param name="extensionPoint">要检索扩展列表的扩展点。
        /// The extension point for which to retrieve a list of extensions.</param>
        /// 
        /// <param name="filter"><see cref ="ExtensionFilter"/>用于将结果集限制为具有特定特征的扩展。
        /// An <see cref="ExtensionFilter"/> used to limit the result set to extensions with particular characteristics.</param>
        /// 
        /// <returns><see cref ="ExtensionInfo"/>对象的列表，描述可用的扩展名。
        /// A list of <see cref="ExtensionInfo"/> objects describing available extensions.</returns>
        /// 
        /// <remarks>可用扩展是启用和许可的扩展。
        /// Available extensions are those which are both enabled and licensed.
        /// </remarks>
        public ExtensionInfo[] ListExtensions(ExtensionPoint extensionPoint, ExtensionFilter filter)
        {
            return ListExtensionsHelper(extensionPoint, filter).ToArray();
        }

        #endregion

        private List<ExtensionInfo> ListExtensionsHelper(ExtensionPoint extensionPoint, ExtensionFilter filter)
        {
            // ensure extension map has been constructed 确保已构建扩展映射
            BuildExtensionMapOnce();

            List<ExtensionInfo> extensions;
            if (_extensionMap.TryGetValue(extensionPoint.GetType(), out extensions))
            {
                return CollectionUtils.Select(extensions,
                 extension => extension.Enabled && extension.Authorized && (filter == null || filter.Test(extension)));
            }
            return new List<ExtensionInfo>();
        }

        private void BuildExtensionMapOnce()
        {
            // build extension map if not already built
            // note that this is the only place where we need to lock, because once built, map is safe for concurrent readers
            // 如果尚未构建，则构建扩展映射
            // 请注意，这是我们需要锁定的唯一地方，因为一旦构建，地图对于并发读者来说是安全的
            if (_extensionMap != null)
                return;

            lock (_syncLock)
            {
                if (_extensionMap == null)
                {
                    // group extensions by extension point
                    // (note that grouping preserves the order of the original Extensions list)
                    // 按扩展点分组扩展
                    //（请注意，分组会保留原始扩展名列表的顺序）
                    _extensionMap = CollectionUtils.GroupBy(Platform.PluginManager.Extensions, ext => ext.PointExtended);
                }
            }
        }
    }
}
