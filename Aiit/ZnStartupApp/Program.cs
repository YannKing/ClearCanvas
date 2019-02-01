using ClearCanvas.Common;
using ClearCanvas.Common.Utilities;
using ClearCanvas.Desktop;
using ClearCanvas.Desktop.Actions;
using ClearCanvas.Desktop.Tools;
using ClearCanvas.ImageViewer.Configuration;
using ClearCanvas.ImageViewer.StudyManagement;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ZnStartupApp
{

    [MenuAction("openfiles", "global-menus/MenuFile/打开文件", "OpenFiles")]
    [MenuAction("openfolder", "global-menus/MenuFile/打开目录", "OpenFolder")]
    [MenuAction("closeapp", "global-menus/MenuFile/关闭应用", "CloseApp")]

    [ExtensionOf(typeof(DesktopToolExtensionPoint))]
    public class ToolsMenu : Tool<IDesktopToolContext>
    {
        #region 框架代码
        private bool _enabled;
        private event EventHandler _enabledChanged;
        /// <summary>
        ///默认无参构造函数。CC框架所必须的，不可删除。
        /// </summary>
        public ToolsMenu()
        {
            _enabled = true;
        }
        /// <summary>
        /// 框架调用的初始化方法，必须重写。
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            // TODO: 在此处添加任何重要初始化代码，而不是在构造函数
        }
        /// <summary>
        /// 框架调用此属性，用于获取或设置在UI中 启用/禁用状态。
        /// </summary>
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
        /// <summary>
        /// 通知这个工具的启用状态发生了变化。
        /// </summary>
        public event EventHandler EnabledChanged
        {
            add { _enabledChanged += value; }
            remove { _enabledChanged -= value; }
        }
        #endregion

        #region 菜单命令
        /// <summary>
        /// 打开文件
        /// </summary>
        public void OpenFiles()
        {
            System.Windows.Forms.OpenFileDialog OFD = new System.Windows.Forms.OpenFileDialog();
            OFD.Title = "打开Dicom文件";
            OFD.Filter = "*.dcm|*.dcm|*.*|*.*";
            OFD.Multiselect = true;
            if (OFD.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                new OpenFilesHelper(OFD.FileNames) { WindowBehaviour = ViewerLaunchSettings.WindowBehaviour }.OpenFiles();
            }
        }

        /// <summary>
        /// 打开目录
        /// </summary>
        public void OpenFolder()
        {
            System.Windows.Forms.FolderBrowserDialog FBD = new System.Windows.Forms.FolderBrowserDialog();
            FBD.Description = "选择包含Dicom文件的目录。";
            if (FBD.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                new OpenFilesHelper(System.IO.Directory.EnumerateFiles(FBD.SelectedPath).ToArray()) { WindowBehaviour = ViewerLaunchSettings.WindowBehaviour }.OpenFiles();
            }
        }

        /// <summary>
        /// 关闭应用
        /// </summary>
        public void CloseApp()
        {
            if (System.Windows.Forms.MessageBox.Show("确定要退出图像浏览器吗？", "问题", System.Windows.Forms.MessageBoxButtons.OKCancel, System.Windows.Forms.MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.OK)
            {
                Application.Quit();
            }
        }
        #endregion
    }


    [ExtensionOf(typeof(StartupActionProviderExtensionPoint))]
    internal sealed class StartupActionProvider : IStartupActionProvider
    {
        public string Name
        {
            get { return "ZnStartupApp"; }
        }

        public string Description
        {
            get { return "附加工具组件，命令行工具和菜单工具。"; }
        }

        public void Startup(IDesktopWindow mainDesktopWindow)
        {
            List<string> args = new List<string>(Environment.GetCommandLineArgs());
            new OpenFilesHelper(System.IO.Directory.EnumerateFiles(args[1]).ToArray()) { WindowBehaviour = ViewerLaunchSettings.WindowBehaviour }.OpenFiles();
        }
    }
}
