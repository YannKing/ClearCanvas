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

namespace ClearCanvas.Desktop.Actions
{
    /// <summary>用于以声明方式指定操作的属性集的抽象基类。
    /// Abstract base class for the set of attributes that are used to specify actions declaratively.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public abstract class ActionAttribute : Attribute
    {
        private readonly string _actionID;

        /// <summary>属性构造函数。</summary>
        /// <param name="actionID">逻辑操作标识符。
        /// A logical action identifier.</param>
        protected ActionAttribute(string actionID)
        {
            if (actionID.Contains("Class1Tool"))
            {


            }
            _actionID = actionID;
        }

        /// <summary>返回由指定目标对象的类型名称限定的逻辑操作ID。
        /// Returns the logical action ID qualified by the type name of the specified target object.
        /// </summary>
        /// <param name="target">应使用其类型的对象来限定操作ID。
        /// The object whose type should be used to qualify the action ID.</param>
        public string QualifiedActionID(object target)
        {
            // create a fully qualified action ID // 创建一个完全限定的操作ID
            return string.Format("{0}:{1}", target.GetType().FullName, _actionID);
        }

        /// <summary>通过指定的<see cref ="IActionBuildingContext"/>将此属性应用于<see cref ="IAction"/>实例。
        /// Applies this attribute to an <see cref="IAction"/> instance, via the specified <see cref="IActionBuildingContext"/>.
        /// </summary>
        public abstract void Apply(IActionBuildingContext builder);
    }
}
