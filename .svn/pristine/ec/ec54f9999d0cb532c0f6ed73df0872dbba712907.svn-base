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
using NewLife.Log;
using System;
using System.Security.Principal;
using System.ServiceModel;
using LogLevel = ClearCanvas.Common.LogLevel;

namespace ClearCanvas.ImageViewer.Shreds
{
    /// <summary>模拟上下文的包装器，表示在此类实例的生命周期内WCF客户端的Windows凭据。
    /// Wrapper for an impersonation context representing the Windows credentials of the WCF client during the lifetime of an instance of this class.
    /// </summary>
    /// <remarks>
    /// 要模拟WCF客户端的Windows凭据，请先构造<see cref ="ServiceClientImpersonationContext"/>的新实例执行需要凭据的语句。
    /// 语句执行后，<see cref ="Dispose"/>实例以恢复上下文。
    /// 
    /// To impersonate the Windows credentials of the WCF client, construct a new instance of <see cref="ServiceClientImpersonationContext"/> before
    /// executing the statements that require the credentials. After the statements have executed, <see cref="Dispose"/> the instance to revert the context.
    /// </remarks>
    internal sealed class ServiceClientImpersonationContext : IDisposable
    {
        private WindowsImpersonationContext _windowsImpersonationContext;

        /// <summary>如果可能，初始化模拟WCF客户端的Windows凭据。
        /// Initializes impersonation of the WCF client's Windows credentials, if possible.
        /// </summary>
        public ServiceClientImpersonationContext()
        {
            try
            {
                var securityContext = ServiceSecurityContext.Current;
                _windowsImpersonationContext = securityContext != null && securityContext.WindowsIdentity != null ? securityContext.WindowsIdentity.Impersonate() : null;
            }
            catch (Exception ex)
            {
                Platform.Log(LogLevel.Warn, ex, "Exception thrown when accessing the security context of the service call for identity impersonation.");
                XTrace.WriteLine($"警告：访问用于身份模拟的服务调用的安全上下文时抛出异常。");
                XTrace.WriteException(ex);
            }
        }

        /// <summary>结束模拟WCF客户端的Windows凭据。
        /// Ends impersonation of the WCF client's Windows credentials.
        /// </summary>
        public void Dispose()
        {
            if (_windowsImpersonationContext != null)
            {
                _windowsImpersonationContext.Dispose();
                _windowsImpersonationContext = null;
            }
        }
    }
}