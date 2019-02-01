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
using ClearCanvas.Common.Shreds;
using NewLife.Log;
using System;
using System.Collections.Generic;
using LogLevel = ClearCanvas.Common.LogLevel;

namespace ClearCanvas.Server.ShredHost
{
    public abstract class WcfShred : Shred, IWcfShred
    {
        public WcfShred()
        {
            _serviceEndpointDescriptions = new Dictionary<string, ServiceEndpointDescription>();
        }


        public override object InitializeLifetimeService()
        {
            // I can't find any documentation yet, that says that returning null 
            // means that the lifetime of the object should not expire after a timeout
            // but the initial solution comes from this page: http://www.dotnet247.com/247reference/msgs/13/66416.aspx
            // 我还找不到任何文档，也就是说返回null 表示超时后对象的生命周期不应过期 但最初的解决方案来自此页面：http：www.dotnet247.com/247reference/msgs/13/66416.aspx
            return null;
        }


        public ServiceEndpointDescription StartHttpHost<TServiceType, TServiceInterfaceType>(string name, string description)
        {
            Platform.Log(LogLevel.Info, "Starting WCF Shred {0}...", name);
            XTrace.WriteLine($"启动WCF Shred {name} ...");

            if (_serviceEndpointDescriptions.ContainsKey(name))
                throw new Exception(String.Format($"The service endpoint '{name}' already exists." +
                                                  $"服务端点“{name}”已存在。"));

            ServiceEndpointDescription sed =
                WcfHelper.StartHttpHost<TServiceType, TServiceInterfaceType>(name, description, SharedHttpPort, ServiceAddressBase);
            _serviceEndpointDescriptions[name] = sed;

            Platform.Log(LogLevel.Info, "WCF Shred {0} is listening at {1}.", name, sed.ServiceHost.Description.Endpoints[0].Address);
            XTrace.WriteLine($"WCF Shred {name} 正在监听： {sed.ServiceHost.Description.Endpoints[0].Address}.");

            return sed;
        }

        public ServiceEndpointDescription StartBasicHttpHost<TServiceType, TServiceInterfaceType>(string name, string description)
        {
            Platform.Log(LogLevel.Info, "Starting WCF Shred {0}...", name);
            XTrace.WriteLine($"启动WCF Shred {name} ...");

            if (_serviceEndpointDescriptions.ContainsKey(name))
                throw new Exception(String.Format($"The service endpoint '{name}' already exists." +
                                                  $"服务端点“{name}”已存在。"));

            ServiceEndpointDescription sed =
                WcfHelper.StartBasicHttpHost<TServiceType, TServiceInterfaceType>(name, description, SharedHttpPort, ServiceAddressBase);
            _serviceEndpointDescriptions[name] = sed;

            Platform.Log(LogLevel.Info, "WCF Shred {0} is listening at {1}.", name, sed.ServiceHost.Description.Endpoints[0].Address);
            XTrace.WriteLine($"WCF Shred {name} 正在监听： { sed.ServiceHost.Description.Endpoints[0].Address}.");
            return sed;
        }

        public ServiceEndpointDescription StartHttpDualHost<TServiceType, TServiceInterfaceType>(string name, string description)
        {
            Platform.Log(LogLevel.Info, "Starting WCF Shred {0}...", name);

            if (_serviceEndpointDescriptions.ContainsKey(name))
                throw new Exception(String.Format("The service endpoint '{0}' already exists.", name));

            ServiceEndpointDescription sed =
                WcfHelper.StartHttpDualHost<TServiceType, TServiceInterfaceType>(name, description, SharedHttpPort, ServiceAddressBase);
            _serviceEndpointDescriptions[name] = sed;

            Platform.Log(LogLevel.Info, "WCF Shred {0} is listening at {1}.", name, sed.ServiceHost.Description.Endpoints[0].Address);

            return sed;
        }

        public ServiceEndpointDescription StartNetTcpHost<TServiceType, TServiceInterfaceType>(string name, string description)
        {
            Platform.Log(LogLevel.Info, "Starting WCF Shred {0}...", name);

            if (_serviceEndpointDescriptions.ContainsKey(name))
                throw new Exception(String.Format("The service endpoint '{0}' already exists.", name));

            ServiceEndpointDescription sed =
                WcfHelper.StartNetTcpHost<TServiceType, TServiceInterfaceType>(name, description, SharedTcpPort, SharedHttpPort, ServiceAddressBase);
            _serviceEndpointDescriptions[name] = sed;

            Platform.Log(LogLevel.Info, "WCF Shred {0}is listening at {1}.", name, sed.ServiceHost.Description.Endpoints[0].Address);


            return sed;
        }

        /// <summary>启动Net Pipe Host
        /// (启动网络管道宿主？)
        /// Yann 2018年12月28日 16:21:12
        /// </summary>
        /// <typeparam name="TServiceType"></typeparam>
        /// <typeparam name="TServiceInterfaceType"></typeparam>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public ServiceEndpointDescription StartNetPipeHost<TServiceType, TServiceInterfaceType>(string name, string description)
        {
            Platform.Log(LogLevel.Info, "Starting WCF Shred {0}...", name);
            XTrace.WriteLine($"启动WCF Shred {name} ...");

            if (_serviceEndpointDescriptions.ContainsKey(name))
                throw new Exception(String.Format($"The service endpoint '{name}' already exists." +
                                                  $"服务端点“{name}”已存在。"));

            ServiceEndpointDescription sed = WcfHelper.StartNetPipeHost<TServiceType, TServiceInterfaceType>(name, description, SharedHttpPort, ServiceAddressBase);
            _serviceEndpointDescriptions[name] = sed;

            Platform.Log(LogLevel.Info, "WCF Shred {0} is listening at {1}.", name, sed.ServiceHost.Description.Endpoints[0].Address);
            XTrace.WriteLine($"WCF Shred {name} 正在监听： {sed.ServiceHost.Description.Endpoints[0].Address}.");

            return sed;
        }

        /// <summary>停止宿主
        /// Yann 2018年12月28日 16:20:55</summary>
        /// <param name="name"></param>
        protected void StopHost(string name)
        {
            Platform.Log(LogLevel.Info, "Stopping WCF Shred {0}...", name);
            XTrace.WriteLine($"停止WCF Shred {name} ...");

            if (_serviceEndpointDescriptions.ContainsKey(name))
            {
                _serviceEndpointDescriptions[name].ServiceHost.Close();
                _serviceEndpointDescriptions.Remove(name);
                Platform.Log(LogLevel.Info, "WCF Shred {0} Stopped", name);
                XTrace.WriteLine($"WCF Shred {name}已停止");
            }
            else
            {
                // TODO: throw an exception, since a name of a service endpoint that is
                // passed in here that doesn't exist should be considered a programming error
                // TODO：抛出异常，因为服务端点的名称是
                // 在这里传递的不存在应该被认为是编程错误
                Platform.Log(LogLevel.Debug, "Attempt to stop WCF Shred {0} failed: shred doesn't exist.", name);
                XTrace.WriteLine($"尝试停止WCF Shred {name}失败：shred不存在。");
            }
        }

        #region Private Members

        private Dictionary<string, ServiceEndpointDescription> _serviceEndpointDescriptions;

        #endregion

        #region IWcfShred Members

        private int _httpPort;
        private int _tcpPort;
        private string _serviceAddressBase;

        public int SharedHttpPort
        {
            get { return _httpPort; }
            set { _httpPort = value; }
        }

        public int SharedTcpPort
        {
            get { return _tcpPort; }
            set { _tcpPort = value; }
        }

        public string ServiceAddressBase
        {
            get { return _serviceAddressBase; }
            set { _serviceAddressBase = value; }
        }

        #endregion
    }
}
