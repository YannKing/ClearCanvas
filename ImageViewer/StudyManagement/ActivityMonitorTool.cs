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
using ClearCanvas.Desktop;
using ClearCanvas.Desktop.Actions;
using ClearCanvas.Desktop.Tools;
using ClearCanvas.ImageViewer.Common.WorkItem;
using NewLife.Log;
using System;

namespace ClearCanvas.ImageViewer.StudyManagement
{
    /// <summary>活动监视器菜单
    /// Yann 2018年12月29日 11:45:00
    /// </summary>
    [MenuAction("show", "global-menus/MenuTools/MenuActivityMonitor", "Show")]
    [Tooltip("show", "TooltipActivityMonitor")]
    [ViewerActionPermission("show", AuthorityTokens.ActivityMonitor.View)]
    [ExtensionOf(typeof(DesktopToolExtensionPoint))]
    public class ActivityMonitorTool : Tool<IDesktopToolContext>
    {
        /// <summary>观察者安装</summary>
        private static bool _watchersInstalled;

        private ActivityMonitorFailureWatcher _failureWatcher;
        private LocalServerWatcher _localServerWatcher;
        private bool _maxDiskSpaceExceeded;

        public override void Initialize()
        {
            base.Initialize();

            // install watchers only once, in the first desktop window (the main window)
            // 在第一个桌面窗口（主窗口）中只安装一次观察者
            if (!_watchersInstalled)
            {
                _watchersInstalled = true;
                _failureWatcher = new ActivityMonitorFailureWatcher(this.Context.DesktopWindow, Show);
                _failureWatcher.Initialize();

                _localServerWatcher = LocalServerWatcher.Instance;
                _localServerWatcher.DiskSpaceUsageChanged += LocalServerWatcherOnDiskSpaceUsageChanged;
                CheckDiskspaceUsageExceeded();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_failureWatcher != null)
                {
                    _failureWatcher.Dispose();
                    _failureWatcher = null;
                }
                if (_localServerWatcher != null)
                {
                    _localServerWatcher.DiskSpaceUsageChanged -= LocalServerWatcherOnDiskSpaceUsageChanged;
                    _localServerWatcher = null;
                }
            }
            base.Dispose(disposing);
        }

        public override IActionSet Actions
        {
            get
            {
                if (!WorkItemActivityMonitor.IsSupported)
                    return new ActionSet();

                return base.Actions;
            }
        }

        public void Show()
        {
            XTrace.WriteLine($"调用：{this.GetType().FullName}  Show()");
            ActivityMonitorManager.Show(Context.DesktopWindow);
        }

        /// <summary>磁盘空间使用的本地服务器观察器已更改</summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void LocalServerWatcherOnDiskSpaceUsageChanged(object sender, EventArgs eventArgs)
        {
            XTrace.WriteLine("磁盘空间使用的本地服务器观察器已更改=>：触发事件：LocalServerWatcherOnDiskSpaceUsageChanged");
            CheckDiskspaceUsageExceeded();
        }

        private void CheckDiskspaceUsageExceeded()
        {
            if (_localServerWatcher.IsMaximumDiskspaceUsageExceeded == _maxDiskSpaceExceeded)
                return;

            _maxDiskSpaceExceeded = _localServerWatcher.IsMaximumDiskspaceUsageExceeded;
            if (_maxDiskSpaceExceeded)
            {
                this.Context.DesktopWindow.ShowAlert(AlertLevel.Warning,
                                                     SR.WarningMaximumDiskUsageExceeded,
                                                     SR.LinkOpenStorageConfiguration,
                                                     delegate
                                                     {
                                                         ActivityMonitorQuickLink.LocalStorageConfiguration.Invoke(
                                                             this.Context.DesktopWindow);
                                                     },
                                                     true);
            }
        }
    }
}
