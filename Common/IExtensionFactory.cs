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

namespace ClearCanvas.Common
{
    /// <summary>接口定义工厂以扩展任意<see cref ="ExtensionPoint"/> s。</summary>
    /// <remarks>此接口的实现应该是线程安全的。</remarks>
    public interface IExtensionFactory
    {
        /// <summary>创建扩展指定扩展点并匹配指定过滤器的可用扩展实例。</summary>
        /// <param name="extensionPoint">要为其创建扩展的扩展点。</param>
        /// <param name="filter">An <see cref="ExtensionFilter"/>用于将结果集限制为具有特定特征的扩展。</param>
        /// <param name="justOne">指示是否仅返回找到的第一个匹配扩展名。</param>
        /// <returns>一组扩展实例。</returns>
        /// <remarks>
        /// Available extensions are those which are both enabled and licensed.
        /// If <paramref name="justOne"/> is true, the first matching extension that is successfully instantiated is returned,
        /// an no other extensions are instantiated.
        /// </remarks>
        object[] CreateExtensions(ExtensionPoint extensionPoint, ExtensionFilter filter, bool justOne);

        /// <summary>列出与指定的<paramref name ="filter"/>匹配的指定<paramref name ="extensionPoint"/>的所有可用扩展名。
        /// Lists all available extensions for the specified <paramref name="extensionPoint"/> that match the specified <paramref name="filter"/>.
        /// </summary>
        /// <param name="extensionPoint">The extension point for which to retrieve a list of extensions.</param>
        /// <param name="filter">An <see cref="ExtensionFilter"/> used to limit the result set to extensions with particular characteristics.</param>
        /// <returns>A list of <see cref="ExtensionInfo"/> objects describing available extensions.</returns>
        /// <remarks>
        /// 可用扩展是启用和许可的扩展。
        /// 
        /// Available extensions are those which are both enabled and licensed.
        /// </remarks>
        ExtensionInfo[] ListExtensions(ExtensionPoint extensionPoint, ExtensionFilter filter);
    }
}
