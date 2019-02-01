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

#region Additional permission to link with SQL Server Compact Edition

// Additional permission under GNU GPL version 3 section 7
// 
// If you modify this Program, or any covered work, by linking or combining it
// with SQL Server Compact Edition (or a modified version of that library),
// containing parts covered by the terms of the SQL Server Compact Edition
// EULA, the licensors of this Program grant you additional permission to
// convey the resulting work.

#endregion

using ClearCanvas.Common;
using ClearCanvas.Common.Utilities;
using NewLife.Log;
using System;
using System.Data;
using System.Data.SqlServerCe;
using System.IO;
using System.Threading;
using LogLevel = ClearCanvas.Common.LogLevel;

namespace ClearCanvas.ImageViewer.StudyManagement.Core.Storage
{
    public static class SqlCeDatabaseHelper<TContext>
    {
        public static void CreateDatabase(string fileName)
        {
            var filePath = GetDatabaseFilePath(fileName);
            XTrace.WriteLine($"数据库文件路径：{filePath}");
             
            // ensure the parent directory exists before trying to create database
            //在尝试创建数据库之前，请确保父目录存在
            //Directory.CreateDirectory(GetDatabaseDirectory());

            var dbDir = GetDatabaseDirectory();
            XTrace.WriteLine($"数据库目录：{dbDir}");
            Directory.CreateDirectory(dbDir);

            //NOTE: Since we're using CE 4.0, the LINQ CreateDatabase function won't work because it creates a 3.5 database.
            //注意：由于我们使用CE 4.0，LINQ CreateDatabase函数将无法工作，因为它创建了一个3.5数据库。
            var resourceResolver = new ResourceResolver(typeof(TContext).Assembly);
            using (Stream resourceStream = resourceResolver.OpenResource(fileName))
            {
                using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    var buffer = new byte[1024];
                    int bytesRead = resourceStream.Read(buffer, 0, buffer.Length);
                    // write the required bytes 写所需的字节
                    while (bytesRead > 0)
                    {
                        fileStream.Write(buffer, 0, bytesRead);
                        bytesRead = resourceStream.Read(buffer, 0, buffer.Length);
                    }

                    fileStream.Close();
                }

                resourceStream.Close();
            }
        }

        /// <summary>创建数据库连接
        /// Yann 2019年1月3日 11:25:23创建数据库链接
        /// 这里经过输出的日志发现，程序会每2S进行一次数据库链接。 由于频繁链接数据库操作会产生大量日志输出，进而影响阅读，所以暂时关闭这里的日志。
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="timeoutMilliseconds"></param>
        /// <returns></returns>
        public static IDbConnection CreateConnection(string fileName, int timeoutMilliseconds = 2000)
        {
            //XTrace.WriteLine("创建数据库连接 CreateConnection");//Yann 2019年1月3日 11:26:57 调用频繁，暂时关闭这里的日志
            int retryCount = 0;//发生异常时的，记录的重试次数
            int startTickCount = Environment.TickCount;
            while (true)
            {
                try
                {
                    //While the database is being created (via CreateDatabase), other threads can be trying to either
                    //create the database, or create a connection to it before it's finished being written out.
                    //Rather than using an ExclusiveLock around CreateConnection all the time, we just put in a retry
                    //mechanism for the odd case where this collision actually occurs. Much less expensive, given
                    //how infrequently the database files are created.
                    //Note that an ExclusiveLock just around CreateDatabase doesn't work because the SqlCeConnection
                    //can still fail to open because it can't access the file while it's being created.
                    /**
                     * 在创建数据库时（通过CreateDatabase），其他线程也可以尝试创建数据库，或在写完数据库之前创建一个连接。
                     * 我们不是一直在CreateConnection周围使用ExclusiveLock，而是重新进行重试这种碰撞实际发生的奇怪情况的机制。
                     * 给定的便宜得多数据库文件创建的频率很低。
                     * 请注意，CreateDatabase周围的ExclusiveLock不起作用，因为SqlCeConnection仍然无法打开，因为它在创建时无法访问该文件。
                     */
                    var connection = CreateConnectionInternal(fileName);//Yann 2019年1月3日 11:28:36 由于本函数会频繁调用所以调用的函数内的日志也注释掉
                    if (retryCount > 0)
                    {
                        Platform.Log(LogLevel.Info, "Successfully opened database connection to '{0}' after {1} retries.", fileName, retryCount);
                        XTrace.WriteLine($"在{retryCount}重试后成功打开了与'{fileName}'的数据库连接。");
                    }

                    return connection;
                }
                catch (Exception e)
                {
                    //No timeout. 没有超时。
                    if (timeoutMilliseconds <= 0)
                        throw;

                    var elapsed = Environment.TickCount - startTickCount;
                    var remaining = timeoutMilliseconds - elapsed;

                    //retry if there's some time left before the timeout, or there hasn't been at least one retry.
                    //如果在超时之前还剩一段时间，或者至少没有重试，则重试。
                    if (remaining <= 0 && retryCount != 0)
                        throw;

                    ++retryCount;
                    var waitMilliseconds = Math.Min(50, remaining);
                    Platform.Log(LogLevel.Warn, e, "Failed to create database connection for '{0}'" +
                                                   "; waiting {1}ms before trying again.", fileName, waitMilliseconds);

                    XTrace.WriteLine($"无法为“{fileName}”创建数据库连接;在再次尝试之前等待{waitMilliseconds}ms。");
                    Thread.Sleep(waitMilliseconds);
                }
            }
        }

        /// <summary>获取数据库文件路径</summary>
        public static string GetDatabaseFilePath(string fileName)
        {
            var getDatabaseFilePath = Path.Combine(GetDatabaseDirectory(), fileName);
            //XTrace.WriteLine($"GetDatabaseFilePath 获取数据库文件路径：{getDatabaseFilePath}");
            return getDatabaseFilePath;
        }

        /// <summary>获取数据库目录</summary>
        public static string GetDatabaseDirectory()
        {
            var getDatabaseDirectory = Platform.ApplicationDataDirectory;
            //XTrace.WriteLine($"GetDatabaseDirectory 获取数据库目录：{getDatabaseDirectory}");
            return getDatabaseDirectory;
        }

        /// <summary>创建内部连接</summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static IDbConnection CreateConnectionInternal(string fileName)
        {
            string filePath = GetDatabaseFilePath(fileName);
            if (!File.Exists(filePath))
            {
                XTrace.WriteLine($"数据库文件不存在，创建数据库：{filePath}");
                CreateDatabase(fileName);
            }

            // TODO (CR Jun 2012): Why are we limiting the database size? 为什么我们限制数据库大小？
            var connectString = string.Format("Data Source = {0}; Default Lock Timeout = 10000;Max Database Size=2048;", filePath);
            //XTrace.WriteLine($"CreateConnectionInternal => CE数据库链接串： {connectString}"); //Yann 由于这里的数据链接调用频繁，暂时关掉日志输出
            // now we can create a long-lived connection 现在我们可以创建一个长期连接
            var connection = new SqlCeConnection(connectString);
            connection.Open();
            return connection;
        }


    }
}
