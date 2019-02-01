using ClearCanvas.Common;
using ClearCanvas.Server.ShredHostService;
using NewLife.Agent;
using NewLife.Log;
using System;

namespace Aiit.DicomReceiveService
{
    class Program
    {
        /// <summary>以服务方式启动</summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            XTrace.UseConsole();
            DicomAgentService.ServiceMain();
        }
    }

    /// <summary>Windows服务代理</summary>
    class DicomAgentService : AgentServiceBase<DicomAgentService>
    {
        public DicomAgentService()
        {
            var set = Setting.Current;
            if (!set.ServiceName.IsNullOrEmpty())//设置名称不为空
                ServiceName = set.ServiceName.Trim();
            set.ServiceName = ServiceName;
            XTrace.WriteLine($"服务初始化 服务名称：{ServiceName}");
        }

        protected override void OnStart(string[] args)
        {
            XTrace.WriteLine($"**********DicomAgentService---OnStart------");
            var message = ProductInformation.GetNameAndVersion(true, true, true, true);
            XTrace.WriteLine($"版本{message}");
            XTrace.WriteLine($"**********DicomAgentService---OnStart--END---------");

            base.OnStart(args);
        }

        protected override void OnStop()
        {
            XTrace.WriteLine($"**********DicomAgentService---OnStop------");

            XTrace.WriteLine($"**********DicomAgentService---OnStop--END---------");
            base.OnStop();
        }

        public override bool Work(int index)
        {
            XTrace.WriteLine($"Work{index}");
            return false;
        }
        protected override void StartWork(string reason)
        {
            XTrace.WriteLine($"-DicomAgentService---StartWork------");
            ShredHostService.InternalStart();
            XTrace.WriteLine($"-DicomAgentService---StartWork--END---------");
            base.StartWork(reason);
        }
        protected override void StopWork(string reason)
        {
            XTrace.WriteLine($"-DicomAgentService---StopWork------");
            ShredHostService.InternalStop();
            XTrace.WriteLine($"-DicomAgentService---StopWork--END---------");
            base.StopWork(reason);
        }

        #region 服务活动周期
        /// <summary>系统关闭时</summary>
        protected override void OnShutdown()
        {
            XTrace.WriteLine($"OnShutdown关机");
            base.OnShutdown();
        }

        /// <summary>服务进程被销毁</summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            XTrace.WriteLine($"服务进程被销毁");
            base.Dispose(disposing);
        }
        #endregion
    }

}