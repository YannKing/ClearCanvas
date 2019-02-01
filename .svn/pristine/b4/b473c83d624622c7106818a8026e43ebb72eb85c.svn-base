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
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using LogLevel = ClearCanvas.Common.LogLevel;

namespace ClearCanvas.Dicom.Network
{
    internal struct ListenerInfo
    {
        public StartAssociation StartDelegate;
        public ServerAssociationParameters Parameters;
    }

    /// <summary>用于为传入的DICOM关联创建后台侦听线程的类。 </summary>
    internal class Listener : IDisposable
    {
        #region Members

        static private readonly Dictionary<IPEndPoint, Listener> _listeners = new Dictionary<IPEndPoint, Listener>();
        private readonly IPEndPoint _ipEndPoint = null;
        private readonly Dictionary<String, ListenerInfo> _applications = new Dictionary<String, ListenerInfo>();
        private TcpListener _tcpListener = null;
        private Thread _theThread = null;
        private volatile bool _stop = false;
        private static readonly object _syncLock = new object();
        #endregion

        #region Public Static Methods

        public static bool Listen(ServerAssociationParameters parameters, StartAssociation acceptor)
        {
            XTrace.WriteLine("执行监听方法在：Listener.Listen");
            //XTrace.WriteLine($"ServerAssociationParameters参数：{parameters}");

            lock (_syncLock)
            {
                Listener theListener;
                if (_listeners.TryGetValue(parameters.LocalEndPoint, out theListener))
                {

                    ListenerInfo info = new ListenerInfo();

                    info.StartDelegate = acceptor;
                    info.Parameters = parameters;

                    if (theListener._applications.ContainsKey(parameters.CalledAE))
                    {
                        Platform.Log(LogLevel.Error, "Already listening with AE {0} on {1}", parameters.CalledAE, parameters.LocalEndPoint.ToString());
                        XTrace.WriteLine($"已经在{parameters.LocalEndPoint.ToString()}上收听了AE：{parameters.CalledAE}");
                        return false;
                    }

                    theListener._applications.Add(parameters.CalledAE, info);
                    Platform.Log(LogLevel.Info, "Starting to listen with AE {0} on existing port {1}", parameters.CalledAE, parameters.LocalEndPoint.ToString());
                    XTrace.WriteLine($"开始在现有端口{parameters.LocalEndPoint.ToString()}上使用AE：{parameters.CalledAE} 进行收听");
                }
                else
                {
                    theListener = new Listener(parameters, acceptor);
                    if (!theListener.StartListening())
                    {
                        Platform.Log(LogLevel.Error, "Unexpected error starting to listen on {0}", parameters.LocalEndPoint.ToString());
                        XTrace.WriteLine($"开始侦听{parameters.LocalEndPoint.ToString()}时出现意外错误");
                        return false;
                    }

                    _listeners[parameters.LocalEndPoint] = theListener;
                    theListener.StartThread();

                    Platform.Log(LogLevel.Info, "Starting to listen with AE {0} on port {1}", parameters.CalledAE, parameters.LocalEndPoint.ToString());
                    XTrace.WriteLine($"开始在端口{parameters.LocalEndPoint.ToString()}上收听AE {parameters.CalledAE}");
                }

                return true;
            }
        }

        public bool StartListening()
        {

            _tcpListener = new TcpListener(_ipEndPoint);

            XTrace.WriteLine($"Listener.StartListening =>{_tcpListener}");

            try
            {
                _tcpListener.Start(50);
            }
            catch (SocketException e)
            {
                Platform.Log(LogLevel.Error, e, "Unexpected exception when starting TCP listener");
                Platform.Log(LogLevel.Error, "Shutting down listener on {0}", _ipEndPoint.ToString());

                XTrace.WriteLine($"启动TCP侦听器时出现意外异常");
                XTrace.WriteLine($"在{_ipEndPoint.ToString()}上关闭监听器");
                XTrace.WriteException(e);

                _tcpListener = null;
                return false;
            }
            return true;
        }

        public static bool StopListening(ServerAssociationParameters parameters)
        {
            XTrace.WriteLine("执行停止监听方法：Listener.StopListening");
            lock (_syncLock)
            {
                Listener theListener;

                if (_listeners.TryGetValue(parameters.LocalEndPoint, out theListener))
                {
                    if (theListener._applications.ContainsKey(parameters.CalledAE))
                    {
                        theListener._applications.Remove(parameters.CalledAE);

                        if (theListener._applications.Count == 0)
                        {
                            // Cleanup the listener  清理监听器
                            _listeners.Remove(parameters.LocalEndPoint);
                            theListener.StopThread();
                            theListener.Dispose();
                        }
                        Platform.Log(LogLevel.Info, "Stopping listening with AE {0} on {1}", parameters.CalledAE, parameters.LocalEndPoint.ToString());
                        XTrace.WriteLine($"在{parameters.LocalEndPoint.ToString()}上停止使用AE {parameters.CalledAE}进行收听");
                    }
                    else
                    {
                        Platform.Log(LogLevel.Error, "Unable to stop listening on AE {0}, assembly was not listening with this AE.", parameters.CalledAE);
                        XTrace.WriteLine($"无法停止收听AE {parameters.CalledAE}，程序集没有收听此AE。");
                        return false;
                    }
                }
                else
                {
                    Platform.Log(LogLevel.Error, "Unable to stop listening, assembly was not listening on end point {0}.", parameters.LocalEndPoint.ToString());
                    XTrace.WriteLine($"无法停止侦听，程序集没有侦听终点{parameters.LocalEndPoint.ToString()}。");
                    return false;
                }

                return true;
            }
        }
        #endregion

        #region Constructors
        internal Listener(ServerAssociationParameters parameters, StartAssociation acceptor)
        {
            ListenerInfo info = new ListenerInfo();

            info.Parameters = parameters;
            info.StartDelegate = acceptor;

            _applications.Add(parameters.CalledAE, info);

            _ipEndPoint = parameters.LocalEndPoint;
        }
        #endregion

        private void StartThread()
        {

            _theThread = new Thread(Listen);
            _theThread.Name = "Association Listener on port " + _ipEndPoint.Port;

            XTrace.WriteLine($"Listener => StartThread => { _theThread.Name} ：端口上的关联侦听器 {_ipEndPoint.Port}");
            _theThread.Start();
        }

        private void StopThread()
        {
            _stop = true;

            _theThread.Join();
        }

        public void Listen()
        {
            while (_stop == false)
            {
                // Tried Async i/o here, but had some weird problems with connections not getting
                // through.

                // 在这里试过Async i / o，但是有一些奇怪的问题，连接没有得到
                // 通过。  (Yann  这里这句 可能是另一个人的注释)
                if (_tcpListener.Pending())
                {
                    Socket theSocket = _tcpListener.AcceptSocket();

                    //DicomServer将自动开始在后台运行
                    new DicomServer(theSocket, _applications);
                    continue;
                }
                Thread.Sleep(10);
            }
            try
            {
                _tcpListener.Stop();
            }
            catch (SocketException e)
            {
                //Yann 怀疑这里打错了，应该是.ToString()
                Platform.Log(LogLevel.Error, e, "Unexpected exception when stoppinging TCP listener on {0}", _ipEndPoint, ToString());
                XTrace.WriteLine($"在{_ipEndPoint.ToString()}上停止TCP侦听器时出现意外异常");
                XTrace.WriteException(e);
            }
        }

        #region IDisposable Implementation
        public void Dispose()
        {
            StopThread();
            if (_tcpListener != null)
            {
                _tcpListener.Stop();
                _tcpListener = null;
            }
        }
        #endregion
    }
}
