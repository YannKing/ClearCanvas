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
using ClearCanvas.Common.Utilities;
using ClearCanvas.Desktop.Actions;
using ClearCanvas.Desktop.Tools;
using System;

namespace ClearCanvas.Desktop.Applets.WebBrowser
{
    [MenuAction("apply", "global-menus/MenuTools/MenuStandard/MenuWebBrowser", "Apply")]
    [ButtonAction("apply", "global-toolbars/ToolbarStandard/ToolbarWebBrowser", "Apply")]
    [Tooltip("apply", "TooltipWebBrowser")]
    [IconSet("apply", "Icons.WebBrowserToolSmall.png", "Icons.WebBrowserToolMedium.png", "Icons.WebBrowserToolLarge.png")]
    [EnabledStateObserver("apply", "Enabled", "EnabledChanged")]
    //
    [ExtensionOf(typeof(DesktopToolExtensionPoint))]
    public class WebBrowserTool : Tool<IDesktopToolContext>
    {
        private bool _enabled;
        private event EventHandler _enabledChanged;

        /// <summary>默认构造函数。一个no-args构造函数是必需的框架。不要删除。 </summary>
        public WebBrowserTool()
        {
            _enabled = true;
        }

        /// <summary>由框架调用以初始化此工具。</summary>
        public override void Initialize()
        {
            base.Initialize();

            // TODO: add any significant initialization code here rather than in the constructor
            // TODO:在这里添加任何重要的初始化代码而不是在构造函数中
        }

        /// <summary>调用以确定是否在UI中启用/禁用此工具。 </summary>
        public bool Enabled
        {
            get { return _enabled; }
            protected set
            {
                if (_enabled != value)
                {
                    _enabled = value;
                    EventsHelper.Fire(_enabledChanged, this, EventArgs.Empty);
                }
            }
        }

        /// <summary>通知此工具的“已启用”状态已更改。
        /// Notifies that the Enabled state of this tool has changed.
        /// </summary>
        public event EventHandler EnabledChanged
        {
            add { _enabledChanged += value; }
            remove { _enabledChanged -= value; }
        }

        /// <summary>当用户单击“应用”菜单项或工具栏按钮时由框架调用。
        /// Called by the framework when the user clicks the "apply" menu item or toolbar button.
        /// </summary>
        public void Apply()
        {
            // TODO
            // Add code here to implement the functionality of the tool
            // If this tool is associated with a workspace, you can access the workspace
            // using the Workspace property

            // TODO
            //在此处添加代码以实现该工具的功能 如果此工具与工作空间关联，则可以访问工作空间 使用Workspace属性
            WebBrowserComponent component = new WebBrowserComponent();

            ApplicationComponent.LaunchAsWorkspace(this.Context.DesktopWindow, component, SR.WorkspaceName);
        }
    }
}