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
using NewLife.Log;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LogLevel = ClearCanvas.Common.LogLevel;

namespace ClearCanvas.Desktop.Tools
{
    /// <summary>
    /// Default implementation of <see cref="IToolSet"/>.
    /// </summary>
    public class ToolSet : IToolSet
    {
        private List<ITool> _tools;

        /// <summary>这构造了包含指定工具的工具集。 <see cref ="IToolContext"/>在每个工具上设置，并调用每个工具的Initialize方法。
        /// 
        /// This contructs a tool set containing the specified tools.  The <see cref="IToolContext"/>
        /// is set on each tool and each tool's Initialize method is called.
        /// </summary>
        /// 
        /// <param name="context">传递给每个工具的工具上下文。
        /// The tool context to pass to each tool.</param>
        /// 
        /// <param name="tools">一组工具，用于在此ToolSet中进行分组并进行初始化使用相同的工具上下文设置。
        /// A set of tools to group in this ToolSet and be initialized and 
        /// set with the same tool context.</param>
        public ToolSet(IEnumerable tools, IToolContext context)
        {
            _tools = new List<ITool>();

            foreach (ITool tool in tools)
            {
                try
                {
                    tool.SetContext(context);
                    tool.Initialize();
                    _tools.Add(tool);
                }
                catch (Exception e)
                {
                    // a tool failed to initialize - log and continue
                    // (this tool will not be included in the set)
                    Platform.Log(LogLevel.Error, e);
                    //工具无法初始化 - 记录并继续（此工具不会包含在套装中）
                    XTrace.WriteLine("工具无法初始化 - 记录并继续（此工具不会包含在套装中）");
                    XTrace.WriteException(e);
                }
            }
        }

        /// <summary>根据指定的扩展点和上下文构造工具集。
        /// Constructs a toolset based on the specified extension point and context.
        /// </summary>
        /// <remarks>该工具集将尝试实例化并初始化所有指定工具扩展点的扩展名。
        /// 
        /// The toolset will attempt to instantiate and initialize all 
        /// extensions of the specified tool extension point.
        /// </remarks>
        /// 
        /// <param name="toolExtensionPoint">提供工具的工具扩展点。
        /// The tool extension point that provides the tools.</param>
        /// 
        /// <param name="context">传递给每个工具的工具上下文。
        /// The tool context to pass to each tool.</param>
        public ToolSet(IExtensionPoint toolExtensionPoint, IToolContext context) : this(toolExtensionPoint, context, null) { }

        /// <summary>根据指定的扩展点和上下文构造工具集。
        /// Constructs a toolset based on the specified extension point and context.
        /// </summary>
        /// <remarks>该工具集将尝试实例化并初始化所有通过的指定工具扩展点的扩展名指定过滤器。
        /// 
        /// The toolset will attempt to instantiate and initialize all 
        /// extensions of the specified tool extension point that pass the 
        /// specified filter.
        /// </remarks>
        /// <param name="toolExtensionPoint">提供工具的工具扩展点。
        /// The tool extension point that provides the tools.</param>
        /// 
        /// <param name="context">传递给每个工具的工具上下文。 
        /// The tool context to pass to each tool.</param>
        /// 
        /// <param name="filter">只有与指定扩展名过滤器匹配的工具才会加载到工具集。如果为null，则加载扩展点的所有工具。
        /// Only tools that match the specified extension filter are loaded into the 
        /// tool set.  If null, all tools extending the extension point are loaded.</param>
        public ToolSet(IExtensionPoint toolExtensionPoint, IToolContext context, ExtensionFilter filter) : this(
            toolExtensionPoint.CreateExtensions(filter), context)
        {

        }

        /// <summary>
        /// Disposes of all the <see cref="ITool"/>s in the tool set.
        /// </summary>
        /// <param name="disposing">True if this object is being disposed, false if it is being finalized.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && _tools != null)
            {
                foreach (ITool tool in _tools)
                {
                    try
                    {
                        tool.Dispose();
                    }
                    catch (Exception e)
                    {
                        // log and continue disposing of other tools
                        Platform.Log(LogLevel.Error, e);
                        XTrace.WriteLine("记录并继续处理其他工具");
                        XTrace.WriteException(e);
                    }
                }
                _tools = null;
            }
        }

        #region IToolSet members

        /// <summary>
        /// Gets the tools contained in this tool set.
        /// </summary>
        public ITool[] Tools
        {
            get { return _tools.ToArray(); }
        }

        /// <summary>
        /// Finds the tool of the specified type.
        /// </summary>
        /// <typeparam name="TTool"></typeparam>
        /// <returns>The instance of the tool of the specified type, or null if no such exists.</returns>
        public TTool Find<TTool>() where TTool : ITool
        {
            return (TTool)_tools.FirstOrDefault(t => t is TTool);
        }

        /// <summary>
        /// Returns the union of all actions defined by all tools in this tool set.
        /// </summary>
        public IActionSet Actions
        {
            get
            {
                var actionList = new List<IAction>();
                foreach (ITool tool in _tools)
                {
                    actionList.AddRange(tool.Actions);
                }
                return new ActionSet(actionList);
            }
        }

        #endregion

        #region IDisposable Members

        /// <summary>
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
                Platform.Log(LogLevel.Error, e);
            }
        }

        #endregion
    }
}