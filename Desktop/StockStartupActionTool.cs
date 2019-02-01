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
using ClearCanvas.Desktop.Tools;
using NewLife.Log;
using System;
using System.Linq;
using LogLevel = ClearCanvas.Common.LogLevel;

namespace ClearCanvas.Desktop
{
    /// <summary>定义应用程序启动时要执行的操作。
    /// Defines the action to be performed at application startup.
    /// </summary>
    /// <see cref="StartupActionProviderExtensionPoint"/>
    public interface IStartupActionProvider
    {
        /// <summary>获取启动操作的简短标签。
        /// Gets a short label for the startup action.
        /// </summary>
        string Name { get; }

        /// <summary>获取有关启动操作的简短描述。
        /// Gets a short description about the startup action.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// 对指定的<see cref ="IDesktopWindow"/>执行启动操作，这是应用程序中的第一个（“主”）窗口。
        /// Performs the startup action for the specified <see cref="IDesktopWindow"/>, which is the first ("main") window in the application.
        /// </summary>
        /// <param name="mainDesktopWindow">
        /// 第一个（“主”）<see cref  = "IDesktopWindow"/>。
        /// The first ("main") <see cref="IDesktopWindow"/>.
        /// </param>
        void Startup(IDesktopWindow mainDesktopWindow);
    }

    /// <summary>
    /// 扩展点，用于提供在应用程序启动时执行的操作。
    /// Extension point to provide an action to be performed at application startup.
    /// </summary>
    /// <see cref="IStartupActionProvider"/>
    [ExtensionPoint]
    public class StartupActionProviderExtensionPoint : ExtensionPoint<IStartupActionProvider> { }

    partial class StockDesktopTools
    {
        [ExtensionOf(typeof(DesktopToolExtensionPoint))]
        internal sealed class StartupActionTool : Tool<IDesktopToolContext>
        {
            private bool _isMainWindowTool = false;
            private static int _toolCount = 0;

            public override void Initialize()
            {
                if (_toolCount == 0)
                    _isMainWindowTool = true;

                ++_toolCount;

                if (_isMainWindowTool)
                    DoStartup(Context.DesktopWindow);

                base.Initialize();
            }

            private static void DoStartup(IDesktopWindow desktopWindow)
            {
                try
                {
                    var xp = new StartupActionProviderExtensionPoint();
                    var xt = xp.CreateExtension() as IStartupActionProvider;
                    if (xt != null)
                        xt.Startup(desktopWindow);
                }
                catch (NotSupportedException)
                {
                    // no extensions defined - not an error
                }
                catch (Exception ex)
                {
                    Platform.Log(LogLevel.Debug, ex, "Unexpected error loading startup actions.");
                    XTrace.WriteLine("Unexpected error loading startup actions." +
                                     $"{Environment.NewLine}加载启动操作时出现意外错误。");
                    XTrace.WriteException(ex);
                }
            }

            protected override void Dispose(bool disposing)
            {
                --_toolCount;

                if (_isMainWindowTool)
                    CloseChildDesktopWindows();

                base.Dispose(disposing);
            }

            private void CloseChildDesktopWindows()
            {
                // We can't just iterate through the collection and close them,
                // because closing a window changes the collection.  So instead,
                // we create a list of the child windows then iterate through
                // that list and close them.
                // 我们不能只是遍历集合并关闭它们，因为关闭窗口会更改集合。所以与其，我们创建一个子窗口列表然后迭代列出并关闭它们。
                var childWindowsToClose = Application.DesktopWindows.Where(window => window != Context.DesktopWindow).ToList();
                foreach (var window in childWindowsToClose)
                    window.Close();
            }
        }
    }
}