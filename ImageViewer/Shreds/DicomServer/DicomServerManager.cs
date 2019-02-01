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
using ClearCanvas.ImageViewer.Common;
using ClearCanvas.ImageViewer.Common.DicomServer;
using NewLife.Log;
using System;
using System.Diagnostics;
using System.Threading;
using LogLevel = ClearCanvas.Common.LogLevel;

namespace ClearCanvas.ImageViewer.Shreds.DicomServer
{
    internal class DicomServerManager
    {
        public static readonly DicomServerManager Instance = new DicomServerManager();

        #region Private Fields

        private readonly object _syncLock = new object();
        private DicomServer _server;
        private ServiceStateEnum _serviceState;

        private bool _active;
        private bool _restart;

        #endregion

        private DicomServerManager()
        {
        }

        #region Private Methods

        private void StartServerAsync(object nothing)
        {
            DicomServerConfiguration serverConfiguration;
            lock (_syncLock)
            {
                serverConfiguration = Common.DicomServer.DicomServer.GetConfiguration();
                _restart = false;
            }

            DicomServer server = null;

            try
            {
                Trace.WriteLine("Starting Dicom server.");
                XTrace.WriteLine("启动Dicom服务器。");
                server = new DicomServer(serverConfiguration);
                server.Start();
            }
            catch (Exception e)
            {
                Platform.Log(LogLevel.Error, e, "Failed to start dicom server ({0}/{1}:{2}", serverConfiguration.HostName, serverConfiguration.AETitle, serverConfiguration.Port);

                XTrace.WriteLine($"无法启动dicom服务器({serverConfiguration.HostName}/{ serverConfiguration.AETitle}:{ serverConfiguration.Port}");
                XTrace.WriteException(e);

                server = null;
            }
            finally
            {
                lock (_syncLock)
                {
                    //the server may be null here, we are just reflecting the state based on the method calls.
                    //这里的服务器可能为null，我们只是根据方法调用反映状态。
                    _server = server;
                    _serviceState = ServiceStateEnum.Started;
                    OnServerStarted();
                }
            }
        }

        private void StopServerAsync(object nothing)
        {
            lock (_syncLock)
            {
                DicomServer server = _server;
                _server = null;

                if (server != null)
                {
                    Trace.WriteLine("Stopping Dicom server.");
                    XTrace.WriteLine("正在停止Dicom服务器。");
                    server.Stop();
                }

                _serviceState = ServiceStateEnum.Stopped;
                OnServerStopped();
            }
        }

        private void OnServerStarted()
        {
            lock (_syncLock)
            {
                Monitor.Pulse(_syncLock);
                if (!_active)
                    _restart = false;

                Trace.WriteLine("Started Dicom server.");
                XTrace.WriteLine("已启动Dicom服务器。");
                if (_restart)
                    StopServer(false);
            }
        }

        private void OnServerStopped()
        {
            lock (_syncLock)
            {
                Monitor.Pulse(_syncLock);
                if (!_active)
                    _restart = false;

                Trace.WriteLine("Stopped Dicom server.");
                XTrace.WriteLine("停止了Dicom服务器。");
                if (_restart)
                    StartServer(false);
            }
        }

        private void StartServer(bool wait)
        {
            lock (_syncLock)
            {
                if (_serviceState == ServiceStateEnum.Stopped)
                {
                    _serviceState = ServiceStateEnum.Starting;
                    ThreadPool.QueueUserWorkItem(StartServerAsync);
                }

                if (wait)
                {
                    while (_serviceState != ServiceStateEnum.Started)
                        Monitor.Wait(_syncLock, 50);
                }
            }
        }

        private void StopServer(bool wait)
        {
            lock (_syncLock)
            {
                if (_serviceState == ServiceStateEnum.Started)
                {
                    _serviceState = ServiceStateEnum.Stopping;
                    ThreadPool.QueueUserWorkItem(StopServerAsync);
                }

                if (wait)
                {
                    while (_serviceState != ServiceStateEnum.Stopped)
                        Monitor.Wait(_syncLock, 50);
                }
            }
        }

        #endregion

        #region Public Methods

        public ServiceStateEnum State
        {
            get
            {
                lock (_syncLock)
                {
                    return _serviceState;
                }
            }
        }

        public void Start()
        {
            lock (_syncLock)
            {
                _active = true;
                StartServer(true);
            }
        }

        public void Stop()
        {
            lock (_syncLock)
            {
                _active = false;
                _restart = false;
                StopServer(true);
            }
        }

        public void Restart()
        {
            lock (_syncLock)
            {
                if (_active)
                {
                    Trace.WriteLine("DICOM server listener restart requested.");
                    XTrace.WriteLine("请求重新启动DICOM服务器侦听器。");
                    _restart = true;
                    StopServer(false);
                }
            }
        }

        #endregion
    }
}
