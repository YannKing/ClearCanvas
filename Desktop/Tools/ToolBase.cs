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
using ClearCanvas.Desktop.Actions;
using System;

namespace ClearCanvas.Desktop.Tools
{
    /// <summary>提供<see cref ="ITool"/>的默认实现的抽象基类。
    /// Abstract base class providing a default implementation of <see cref="ITool"/>.
    /// </summary>
    /// <remarks>工具类可以继承此类，但继承从<see cref ="Tool{TContextInterface}"/>建议。
    /// Tool classes may inherit this class, but inheriting 
    /// from <see cref="Tool{TContextInterface}"/> is recommended.
    /// </remarks>
    public abstract class ToolBase : ITool
    {
        private IToolContext _context;
        private IActionSet _actions;

        /// <summary>构造函数。</summary>
        protected ToolBase()
        {
        }

        /// <summary>提供对工具运行的上下文的无类型引用。
        /// Provides an untyped reference to the context in which the tool is operating.
        /// </summary>
        /// <remarks>在<see cref =“SetContext”/>之前尝试访问此属性已被调用（例如在此工具的构造函数中）将返回null。
        /// Attempting to access this property before <see cref="SetContext"/> 
        /// has been called (e.g in the constructor of this tool) will return null.
        /// </remarks>
        protected IToolContext ContextBase
        {
            get { return _context; }
        }

        #region ITool members

        /// <summary>由框架调用以设置工具上下文。
        /// Called by the framework to set the tool context.
        /// </summary>
        public void SetContext(IToolContext context)
        {
            _context = context;
        }

        /// <summary>由框架调用以允许工具初始化自身。
        /// Called by the framework to allow the tool to initialize itself.
        /// </summary>
        /// 
        /// <remarks>调用<see cref ="SetContext"/>之后将调用此方法，这保证了在调用此方法时该工具可以访问其上下文。
        /// This method will be called after <see cref="SetContext"/> has been called, 
        /// which guarantees that the tool will have access to its context when this method is called.
        /// </remarks>
        public virtual void Initialize()
        {
            // nothing to do  //没事做
        }

        /// <summary>获取作用于此工具的一组操作。 </summary>
        /// <remarks><see cref ="ITool.Actions"/>提到不应将此属性视为动态。
        /// 此实现假定通过延迟初始化动作<b>不</b>动态行动和存储它们。
        /// 如果您希望动态返回操作，则必须覆盖这个性质。
        /// 
        /// <see cref="ITool.Actions"/> mentions that this property should not be considered dynamic.
        /// This implementation assumes that the actions are <b>not</b> dynamic by lazily initializing
        /// the actions and storing them.  If you wish to return actions dynamically, you must override
        /// this property.
        /// </remarks>
        public virtual IActionSet Actions
        {
            get { return _actions ?? (_actions = new ActionSet(ActionAttributeProcessor.Process(this))); }
            protected set { _actions = value; }
        }

        #endregion

        /// <summary>处置此对象;覆盖此方法以执行任何必要的清理。
        /// Disposes of this object; override this method to do any necessary cleanup.
        /// </summary>
        /// <param name="disposing">如果正在处理此对象，则为true;如果正在最终确定，则为false。
        /// True if this object is being disposed, false if it is being finalized.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context = null;
            }
        }

        #region IDisposable Members IDisposable会员

        /// <summary>执行<see cref ="IDisposable"/>模式。
        /// Implementation of the <see cref="IDisposable"/> pattern.
        /// </summary>
        public void Dispose()
        {
            try
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
            catch (Exception e)
            {
                // shouldn't throw anything from inside Dispose() 
                //不应该从Dispose（）内部抛出任何东西
                Platform.Log(LogLevel.Error, e);
            }
        }

        #endregion
    }
}
