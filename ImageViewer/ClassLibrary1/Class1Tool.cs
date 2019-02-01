using ClearCanvas.Common;
using ClearCanvas.Common.Utilities;
using ClearCanvas.Desktop;
using ClearCanvas.Desktop.Actions;
using ClearCanvas.Desktop.Tools;
using ClearCanvas.Desktop.View.WinForms;
using System;

namespace ClassLibrary1
{
    [MenuAction("apply", "global-menus/MenuTools/MenuStandard/MenuMyDesktopTool", "Apply")]
    [ButtonAction("apply", "global-toolbars/ToolbarStandard/ToolbarMyDesktopTool", "Apply")]
    [Tooltip("apply", "Class1Tool")]

    [IconSet("apply", "Icons.TrueSmall.png", "Icons.TrueMedium.png", "Icons.TrueLarge.png")]
    [EnabledStateObserver("apply", "Enabled", "EnabledChanged")]
    // ... (other action attributes here)
    [ExtensionOf(typeof(DesktopToolExtensionPoint))]
    public class Class1Tool : Tool<IDesktopToolContext>
    {
        private bool _enabled;
        private event EventHandler _enabledChanged;

        /// <summary>默认构造函数。一个no-args构造函数是必需的框架。不要删除。</summary>
        public Class1Tool()
        {
            _enabled = true;
        }
        /// <summary>由框架调用以初始化此工具。</summary>
        public override void Initialize()
        {
            base.Initialize();
            // TODO: add any significant initialization code here rather than in the constructor
        }
        /// <summary>调用以确定是否在UI中启用/禁用此工具。</summary>
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
        /// <summary>通知此工具的“已启用”状态已更改。</summary>
        public event EventHandler EnabledChanged
        {
            add { _enabledChanged += value; }
            remove { _enabledChanged -= value; }
        }

        /// <summary>当用户单击“应用”菜单项或工具栏按钮时由框架调用。</summary>
        public void Apply()
        {
            var ms = new MessageBox();
            ms.Show($"{this.GetType().FullName}");
            // Add code here to implement the functionality of the tool
        }

    }

}