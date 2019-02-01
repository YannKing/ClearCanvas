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


using ClearCanvas.Desktop.View.WinForms;

namespace ClearCanvas.Desktop.ExtensionBrowser.View.WinForms
{
    /// <summary>
    /// WinForms实现了一个视图到<see cref ="ExtensionBrowserComponent"/>。这个类
    /// 将所有实际工作委托给<see cref ="ExtensionBrowserControl"/>类。
    /// 
    /// WinForms implemenation of a view onto <see cref="ExtensionBrowserComponent"/>.  This class
    /// delegates all of the real work to the <see cref="ExtensionBrowserControl"/> class.
    /// </summary>
    [ClearCanvas.Common.ExtensionOf(typeof(ExtensionBrowserComponentViewExtensionPoint))]
    public class ExtensionBrowserView : WinFormsView, IApplicationComponentView
    {
        private ExtensionBrowserComponent _browser;
        private ExtensionBrowserControl _browserControl;

        public ExtensionBrowserView()
        {
        }

        /// <summary>
        /// 执行<see cref ="IApplicationComponentView.SetComponent"/>
        /// Implementation of <see cref="IApplicationComponentView.SetComponent"/>
        /// </summary>
        /// <param name="component"></param>
        public void SetComponent(IApplicationComponent component)
        {
            _browser = (ExtensionBrowserComponent)component;
        }

        /// <summary>
        /// 执行<see cref ="IView.GuiElement"/>。获取WinForms为此视图提供UI的元素。
        /// 
        /// Implementation of <see cref="IView.GuiElement"/>.  Gets the WinForms
        /// element that provides the UI for this view.
        /// </summary>
        public override object GuiElement
        {
            get
            {
                if (_browserControl == null)
                {
                    _browserControl = new ExtensionBrowserControl(_browser);
                }
                return _browserControl;
            }
        }
    }
}
