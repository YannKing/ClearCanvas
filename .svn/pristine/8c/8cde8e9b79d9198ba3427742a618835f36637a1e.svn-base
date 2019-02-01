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
using System.Reflection;

namespace ClearCanvas.Desktop.Actions
{
    /// <summary>用于声明“单击”操作的属性集的抽象基类。
    /// Abstract base class for the set of attributes that are used to declare "click" actions.
    /// </summary>
    public abstract class ClickActionAttribute : ActionInitiatorAttribute
    {
        private readonly string _path;
        private readonly string _clickHandler;
        private bool _initiallyAvailable = true;
        private ClickActionFlags _flags;
        private XKeys _keyStroke;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="actionID">逻辑操作ID。
        /// The logical action ID.</param>
        /// 
        /// <param name="path">行动路径。
        /// The action path.</param>
        /// 
        /// <param name="clickHandler">单击操作时将调用的方法的名称。
        /// The name of the method that will be invoked when the action is clicked.</param>
        public ClickActionAttribute(string actionID, string path, string clickHandler) : base(actionID)
        {
            _path = path;
            _clickHandler = clickHandler;
            _flags = ClickActionFlags.None; //默认值，如果指定了named参数，将覆盖 // default value, will override if named parameter is specified  
        }

        /// <summary>构造函数。</summary>
        /// <param name="actionID">逻辑操作ID。</param>
        /// <param name="path">The action path.</param>
        public ClickActionAttribute(string actionID, string path) : this(actionID, path, null)
        {
        }

        /// <summary>获取单击操作时将调用的方法的名称。
        /// Gets the name of the method that will be invoked when the action is clicked.
        /// </summary>
        public string ClickHandler
        {
            get { return _clickHandler; }
        }

        /// <summary>获取或设置一个值，该值指示在未被操作模型覆盖时操作是否应该可用。
        /// Gets or sets a value indicating whether or not the action should be available by default when not overriden by the action model.
        /// </summary>
        public bool InitiallyAvailable
        {
            get { return _initiallyAvailable; }
            set { _initiallyAvailable = value; }
        }

        /// <summary>获取或设置自定义操作行为的标志。
        /// Gets or sets the flags that customize the behaviour of the action.
        /// </summary>
        public ClickActionFlags Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        /// <summary>获取或设置应从键盘调用操作的键击。
        /// Gets or sets the key-stroke that should invoke the action from the keyboard.
        /// </summary>
		public XKeys KeyStroke
        {
            get { return _keyStroke; }
            set { _keyStroke = value; }
        }

        /// <summary>动作模型中动作的建议位置。
        /// The suggested location of the action in the action model.
        /// </summary>
        public string Path { get { return _path; } }

        /// <summary>通过指定的<see cref ="IActionBuildingContext"/>将此属性应用于<see cref ="IAction"/>实例。
        /// Applies this attribute to an <see cref="IAction"/> instance, via the specified <see cref="IActionBuildingContext"/>.
        /// </summary>
        /// <remarks>
        /// 因为这个动作是<see cref ="ActionInitiatorAttribute"/>，实际上是这个方法创建关联的<see cref ="ClickAction"/>。  
        /// <see cref="ActionDecoratorAttribute"/>s只是修改动作的属性。
        /// 
        /// Because this action is an <see cref="ActionInitiatorAttribute"/>, this method actually
        /// creates the associated <see cref="ClickAction"/>.  <see cref="ActionDecoratorAttribute"/>s
        /// merely modify the properties of the action.
        /// </remarks>
        public override void Apply(IActionBuildingContext builder)
        {
            // assert _action == null
            ActionPath path = new ActionPath(this.Path, builder.ResourceResolver);
            builder.Action = CreateAction(builder.ActionID, path, this.Flags, builder.ResourceResolver);
            builder.Action.Available = this.InitiallyAvailable;
            builder.Action.Persistent = true;
            ((ClickAction)builder.Action).KeyStroke = this.KeyStroke;
            builder.Action.Label = path.LastSegment.LocalizedText;

            if (!string.IsNullOrEmpty(_clickHandler))
            {
                // check that the method exists, etc //检查方法是否存在等
                ValidateClickHandler(builder.ActionTarget, _clickHandler);

                ClickHandlerDelegate clickHandler =
                    (ClickHandlerDelegate)Delegate.CreateDelegate(typeof(ClickHandlerDelegate), builder.ActionTarget, _clickHandler);
                ((ClickAction)builder.Action).SetClickHandler(clickHandler);
            }
        }

        /// <summary>创建此属性表示的<see cref ="ClickAction"/>。
        /// Creates the <see cref="ClickAction"/> represented by this attribute.
        /// </summary>
        /// <param name="actionID">逻辑操作ID。
        /// The logical action ID.</param>
        /// 
        /// <param name="path">行动路径。
        /// The action path.</param>
        /// 
        /// <param name="flags">用于指定操作的单击行为的标志。
        /// Flags that specify the click behaviour of the action.</param>
        /// 
        /// <param name="resolver">用于解析操作路径和图标的对象。
        /// The object used to resolve the action path and icons.</param>
        protected abstract ClickAction CreateAction(string actionID, ActionPath path, ClickActionFlags flags, IResourceResolver resolver);

        private static void ValidateClickHandler(object target, string methodName)
        {
            MethodInfo info = target.GetType().GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                Type.EmptyTypes,
                null);

            if (info == null)
            {
                throw new ActionBuilderException(
                    string.Format(SR.ExceptionActionBuilderMethodDoesNotExist, methodName, target.GetType().FullName));
            }
        }
    }
}
