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
using ClearCanvas.ImageViewer.Common.DicomServer;
using ClearCanvas.Server.ShredHost;
using NewLife.Log;
using System;
using LogLevel = ClearCanvas.Common.LogLevel;

namespace ClearCanvas.ImageViewer.Shreds.DicomServer
{
    /// <summary>Dicom服务器扩展</summary>
    [ShredIsolation(Level = ShredIsolationLevel.None)]
    [ExtensionOf(typeof(ShredExtensionPoint))]
    public class DicomServerExtension : WcfShred
    {
        private readonly string _dicomServerEndpointName = "DicomServer";
        private bool _dicomServerWcfInitialized;

        public DicomServerExtension()
        {
            _dicomServerWcfInitialized = false;

            LicenseInformation.LicenseChanged += OnLicenseInformationChanged;
        }

        /// <summary>Dicom服务启动
        /// Yann 2018年12月28日 15:29:08
        /// </summary>
        public override void Start()
        {
            XTrace.WriteLine("Dicom服务启动");
            try
            {
                StartNetPipeHost<DicomServerServiceType, IDicomServer>(_dicomServerEndpointName, SR.DicomServer);
                _dicomServerWcfInitialized = true;
                string message = String.Format(SR.FormatWCFServiceStartedSuccessfully, SR.DicomServer);
                Platform.Log(LogLevel.Info, message);
                Console.WriteLine(message);
                XTrace.WriteLine(message);
            }
            catch (Exception e)
            {
                Platform.Log(LogLevel.Error, e);
                var msg = String.Format(SR.FormatWCFServiceFailedToStart, SR.DicomServer);
                Console.WriteLine(msg);
                XTrace.WriteLine(msg);
                XTrace.WriteException(e);
            }

            //NOTE: in a lot of cases, we start all the internal services before the WCF service,
            //but in this case, the (shared/offline) DICOM service configuration will call RestartListener
            //right after a change is made in the database, so we want to start the internal services
            //after the WCF service us up and running. That way, although unlikely, if the server configuration
            //were changed just as this service were starting up, we will always start up the listener
            //with the right AE title and Port, even if the listener starts then restarts in quick succession.
            //These internal services all nicely handle the possibility of a service calling into them when
            //they're not running yet, anyway.

            /*注意：在很多情况下，我们在WCF服务之前启动所有内部服务，
             * 但在这种情况下，（共享/脱机）DICOM服务配置将调用RestartListener
             * 在数据库中进行更改后，我们就要启动内部服务
             * 在WCF服务我们之后运行。这样，虽然不太可能，但如果是服务器配置
             * 当这项服务启动时我们就会被改变，我们将始终启动听众
             * 使用正确的AE标题和端口，即使监听器启动，也会快速连续重启。
             * 这些内部服务都可以很好地处理服务调用时的可能性
             * 无论如何，它们还没有运行。
             */
            try
            {
                DicomServerManager.Instance.Start();

                string message = String.Format(SR.FormatServiceStartedSuccessfully, SR.DicomServer);
                Platform.Log(LogLevel.Info, message);
                Console.WriteLine(message);
                XTrace.WriteLine(message);
            }
            catch (Exception e)
            {
                Platform.Log(LogLevel.Error, e);
                var message = String.Format(SR.FormatServiceFailedToStart, SR.DicomServer);
                Console.WriteLine(message);
                XTrace.WriteLine(message);
                XTrace.WriteException(e);
            }
        }

        public override void Stop()
        {
            if (_dicomServerWcfInitialized)
            {
                try
                {
                    StopHost(_dicomServerEndpointName);
                    var message = String.Format(SR.FormatWCFServiceStoppedSuccessfully, SR.DicomServer);
                    Platform.Log(LogLevel.Info, message);
                    XTrace.WriteLine(message);
                }
                catch (Exception e)
                {
                    Platform.Log(LogLevel.Error, e);
                    XTrace.WriteException(e);
                }
            }

            try
            {
                DicomServerManager.Instance.Stop();
                var message = String.Format(SR.FormatServiceStoppedSuccessfully, SR.DicomServer);
                Platform.Log(LogLevel.Info, message);
                XTrace.WriteLine(message);

            }
            catch (Exception e)
            {
                Platform.Log(LogLevel.Error, e);
                XTrace.WriteException(e);
            }
        }

        public override string GetDisplayName()
        {
            return SR.DicomServer;
        }

        public override string GetDescription()
        {
            return SR.DicomServerDescription;
        }

        private void OnLicenseInformationChanged(object sender, EventArgs e)
        {
            Platform.Log(LogLevel.Info, @"Restarting {0} due to application licensing status change.", SR.DicomServer);
            XTrace.WriteLine($@"由于应用程序许可状态更改而重新启动{SR.DicomServer}。");
            DicomServerManager.Instance.Restart();
        }
    }
}
