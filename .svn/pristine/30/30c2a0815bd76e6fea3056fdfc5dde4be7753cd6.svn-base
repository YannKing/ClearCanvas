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
using ClearCanvas.Common.Utilities;
using ClearCanvas.ImageViewer.Common.DicomServer;
using ClearCanvas.ImageViewer.Common.StudyManagement;
using NewLife.Log;
using System;
using LogLevel = ClearCanvas.Common.LogLevel;

namespace ClearCanvas.ImageViewer.Common
{
    /// <summary>为安装程序设置应用程序以设置DICOM服务器的AE标题和端口。
    /// Setup application for the installer to set the AE Title and port of the DICOM Server.
    /// </summary>
    [ExtensionOf(typeof(ApplicationRootExtensionPoint))]
    internal class ConfigureLocalServerApplication : IApplicationRoot
    {
        private class CommandLine : ClearCanvas.Common.Utilities.CommandLine
        {
            [CommandLineParameter("ae", "Sets the AE title of the local DICOM server." +
                                        "设置本地DICOM服务器的AE标题。", Required = true)]
            public string AETitle { get; set; }

            [CommandLineParameter("host", "Sets the host name of the local DICOM server." +
                                          "设置本地DICOM服务器的主机名。", Required = false)]
            public string HostName { get; set; }

            [CommandLineParameter("port", "Sets the listening port of the local DICOM server." +
                                          "设置本地DICOM服务器的侦听端口。", Required = true)]
            public int Port { get; set; }

            [CommandLineParameter("filestore", "Sets the location of the file store." +
                                               "设置文件存储的位置。", Required = false)]
            public string FileStoreDirectory { get; set; }

            [CommandLineParameter("minspacepercent", "Sets the minimum used space required on the file store volume for the server to continue accepting studies." +
                                                     "设置文件存储卷上所需的最小已用空间，以便服务器继续接受研究。", Required = false)]
            public string MinimumFreeSpacePercent { get; set; }
        }

        #region Implementation of IApplicationRoot

        public void RunApplication(string[] args)
        {
            var commandLine = new CommandLine();
            try
            {
                commandLine.Parse(args);
            }
            catch (Exception e)
            {
                Platform.Log(LogLevel.Info, e);
                XTrace.WriteLine($"{e.Message}");
                Console.WriteLine(e.Message);

                commandLine.PrintUsage(Console.Out);
                Environment.Exit(-1);
            }

            try
            {
                DicomServer.DicomServer.UpdateConfiguration(new DicomServerConfiguration
                {
                    HostName = commandLine.HostName,
                    AETitle = commandLine.AETitle,
                    Port = commandLine.Port
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                XTrace.WriteLine($"{e.Message}");
                Platform.Log(LogLevel.Warn, e);
                Environment.Exit(-1);
            }

            try
            {
                if (!String.IsNullOrEmpty(commandLine.FileStoreDirectory))
                {
                    StudyStore.UpdateConfiguration(new StorageConfiguration
                    {
                        FileStoreDirectory = commandLine.FileStoreDirectory,
                        MinimumFreeSpacePercent = commandLine.MinimumFreeSpacePercent != null ?
                        double.Parse(commandLine.MinimumFreeSpacePercent) : StorageConfiguration.AutoMinimumFreeSpace
                    });
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Platform.Log(LogLevel.Warn, e);
                XTrace.WriteLine($"{e.Message}");
                Environment.Exit(-1);
            }
        }

        #endregion
    }
}
