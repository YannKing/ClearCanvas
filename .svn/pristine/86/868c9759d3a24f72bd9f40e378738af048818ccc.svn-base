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
    /// <summary>扩展过滤器的抽象基类。
    /// An abstract base class for extension filters.  
    /// </summary>
    /// <remarks>扩展过滤器用于过滤返回的扩展点<b> CreateExtensions </b>方法之一。这个的子类class实现特定类型的过滤器。
    /// Extension filters are used to filter the extension points returned by 
    /// one of the <b>CreateExtensions</b> methods.  Subclasses of this
    /// class implement specific types of filters.
    /// </remarks>
    public abstract class ExtensionFilter
    {
        /// <summary>根据此过滤器的条件测试指定的扩展名。
        /// Tests the specified extension against the criteria of this filter.
        /// </summary>
        /// <param name="extension">要测试的扩展名。</param>
        /// <returns>如果扩展名符合条件，则为True，否则为false。</returns>
        public abstract bool Test(ExtensionInfo extension);
    }
}
