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
using ClearCanvas.ImageViewer.StudyManagement.Core.Storage.DicomQuery;
using NewLife.Log;
using System;
using System.Data;
//using System.Data.Linq;
//using System.Data.Linq.Mapping;
using Devart.Data.Linq;
using Devart.Data.Linq.Mapping;
using System.IO;
using LogLevel = ClearCanvas.Common.LogLevel;

namespace ClearCanvas.ImageViewer.StudyManagement.Core.Storage
{
    /// <summary>管理数据访问工作单元，对单个事务有效。
    /// 
    /// Manages a data-access unit of work, valid for a single transaction.
    /// </summary>
    /// <remarks>根据这篇博客文章（http：matthewmanela.com/blog/sql-ce-3-5-with-linq-to-sql/），
    /// 有在使用带有sql-ce的linq-to-sql时要考虑的一些性能注意事项。
    /// 基本上我们不希望允许L2S管理连接，而是我们自己管理它。
    ///
    /// As per this blog post (http://matthewmanela.com/blog/sql-ce-3-5-with-linq-to-sql/), there are
    /// some performance considerations to take into account when using linq-to-sql with sql-ce. Basically
    /// we don't want to allow L2S to manage the connection, but rather we manage it ourselves.
    /// </remarks>
    public class DataAccessContext : IDisposable
    {
        private static IDbConnection _staticConnection;
        private static readonly object _syncLock = new object();

        public const string WorkItemMutex = "WorkItem";

        private const string _defaultDatabaseFileName = "dicom_store.sdf";

        private readonly string _databaseFilename;
        private readonly DicomStoreDataContext _context;
        private readonly IDbConnection _connection;
        private readonly IDbTransaction _transaction;
        private bool _transactionCommitted;
        private bool _disposed;
        private ExclusiveLock _mutex;

        public DataAccessContext() : this(null)
        {

        }

        public DataAccessContext(string mutexName) : this(mutexName, _defaultDatabaseFileName)
        {

        }

        internal DataAccessContext(string mutexName, string databaseFilename)
        {
            if (!string.IsNullOrEmpty(mutexName))
            {
                _mutex = ExclusiveLock.CreateFileSystemLock(SqlCeDatabaseHelper<DicomStoreDataContext>.GetDatabaseFilePath(mutexName + databaseFilename));
                _mutex.Lock();
            }

            try
            {
                // initialize a connection and transaction
                // 初始化连接和事务
                _databaseFilename = databaseFilename;
                _connection = CreateConnection();
                _transaction = _connection.BeginTransaction(IsolationLevel.ReadCommitted);
                _context = new DicomStoreDataContext(_connection);
                //_context.Log = Console.Out;
            }
            catch
            {
                _mutex.Unlock();
                _mutex.Dispose();
                _mutex = null;

                throw;
            }

            lock (_syncLock)
            {
                if (_staticConnection == null)
                {
                    // This is done for performance reasons.  It forces a connection to remain open while the 
                    // the app domain is running, so that the database is kept in memory.
                    // 这是出于性能原因而完成的。它迫使连接保持打开状态应用程序域正在运行，因此数据库保留在内存中。
                    try
                    {
                        _staticConnection = CreateConnection();
                    }
                    catch (Exception ex)
                    {
                        Platform.Log(LogLevel.Debug, ex, "Failed to initialize static connection to data store database");
                        XTrace.WriteLine($"无法初始化到数据存储数据库的静态连接");
                        XTrace.WriteException(ex);
                    }
                }
            }
        }

        public TextWriter Logger
        {
            get { return _context.Log; }
            set { _context.Log = value; }
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            if (_disposed)
                throw new InvalidOperationException("Already disposed.");

            _disposed = true;

            if (!_transactionCommitted && _transaction != null)
            {
                _transaction.Rollback();
            }

            _context.Dispose();
            _connection.Close();
            _connection.Dispose();

            if (_mutex != null)
            {
                _mutex.Unlock();
                _mutex.Dispose();
                _mutex = null;
            }
        }

        #endregion

        public DeviceBroker GetDeviceBroker()
        {
            return new DeviceBroker(_context);
        }

        public WorkItemBroker GetWorkItemBroker()
        {
            return new WorkItemBroker(_context);
        }

        public WorkItemUidBroker GetWorkItemUidBroker()
        {
            return new WorkItemUidBroker(_context);
        }

        public StudyBroker GetStudyBroker()
        {
            return new StudyBroker(_context);
        }

        public StudyStoreQuery GetStudyStoreQuery()
        {
            return new StudyStoreQuery(_context);
        }

        /// <summary>提交交易。
        /// Commits the transaction.
        /// </summary>
        /// <remarks>成功调用此方法后，应该处理此上下文实例。
        /// After a successful call to this method, this context instance should be disposed.
        /// </remarks>
        public void Commit()
        {
            try
            {
                if (_transactionCommitted)
                    throw new InvalidOperationException("Transaction already committed.");
                _context.SubmitChanges();
                if (_transaction != null)
                    _transaction.Commit();
                _transactionCommitted = true;
            }
            catch (ChangeConflictException)
            {
                foreach (ObjectChangeConflict occ in _context.ChangeConflicts)
                {
                    MetaTable metatable = _context.Mapping.GetTable(occ.Object.GetType());
                    Platform.Log(LogLevel.Warn, "Change Conflict with update to table: {0}", metatable.TableName);
                    XTrace.WriteLine($"通过更新到表更改冲突：{metatable.TableName}");
                }
                throw;
            }
            finally
            {
                if (_mutex != null)
                {
                    _mutex.Unlock();
                    _mutex.Dispose();
                    _mutex = null;
                }
            }
        }

        private IDbConnection CreateConnection()
        {
            return CreateConnection(_databaseFilename);
        }

        /// <summary>创建与指定数据库文件的连接，从而创建数据库如果它不存在。
        /// Dicom Store数据上下文
        /// 
        /// Creates a connection to the specified database file, creating the database
        /// if it does not exist.
        /// </summary>
        /// <param name="databaseFile"></param>
        /// <returns></returns>
        private IDbConnection CreateConnection(string databaseFile)
        {
            return MySqlDatabaseHelper<DicomStoreDataContext>.CreateConnection();
        }
    }
}