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

using System;

namespace ClearCanvas.Common
{
    /// <summary>用于将类标记为定义扩展点的属性。</summary>
    /// <remarks>使用此属性将类标记为定义扩展点。此属性必须只是应用于<see cref ="ExtensionPoint"/>的子类。</remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ExtensionPointAttribute : Attribute
    {
        private string _name;
        private string _description;

        /// <summary>属性构造函数。</summary>
        public ExtensionPointAttribute()
        {
        }

        /// <summary>扩展点的友好名称。</summary>
        /// <remarks>这是可选的，可以作为命名参数提供。</remarks>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>扩展点的友好描述。</summary>
        /// <remarks>这是可选的，可以作为命名参数提供。</remarks>
        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }
    }
}
