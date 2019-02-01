#region License

// Copyright (c) 2014, ClearCanvas Inc.
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

using System.Data.Linq;

namespace ClearCanvas.ImageViewer.StudyManagement.Core.Storage
{
    partial class DicomStoreDataContext
    {
        #region LINQ to SQL Overrides   // LINQ to SQL覆盖

        // Updates and Inserts to certain tables have been explicitly implemented here in order to reduce the overhead
        // of the LINQ to SQL dynamic query generation which can greatly impact overall performance of the data store.
        // These overrides have been implemented in accordance with the requirements defined at
        // http://msdn.microsoft.com/en-us/library/vstudio/bb546188%28v=vs.100%29.aspx
        // In particular, updates should throw a ChangeConflictException if the specific update version was not found,
        // and inserts should update the entity with the inserted row's identity. Both updates and inserts should
        // update the entity with the row's version.


        /*
         * 此处已明确实现对某些表的更新和插入，以减少开销
         * LINQ to SQL动态查询生成可以极大地影响数据存储的整体性能。
         * 这些覆盖已根据定义的要求实施
         * HTTP：msdn.microsoft.com/en-us/library/vstudio/bb546188%28v=vs.100%29.aspx
         * 特别是，如果找不到特定的更新版本，更新应抛出ChangeConflictException，
         * 和插入应该使用插入的行标识更新实体。更新和插入都应该
         * 使用行的版本更新实体。
         */


        /// <summary>插入WorkItem</summary>
        /// <param name="instance"></param>
        partial void InsertWorkItem(WorkItem instance)
        {
            const string commandText = "INSERT INTO [WorkItem]"
                                       + " ([DeleteTime], [ExpirationTime], [FailureCount], [Priority], [ProcessTime], [RequestedTime],"
                                       + "	[ScheduledTime], [SerializedProgress], [SerializedRequest], [Status], [StudyInstanceUid], [Type])"
                                       + " VALUES (@deleteTime, @expirationTime, @failureCount, @priority, @processTime, @requestedTime,"
                                       + "  @scheduledTime, @progress, @request, @status, @studyInstanceUid, @type)";
            using (var cmd = Connection.CreateCommand(commandText, Transaction))
            {
                cmd.SetParameter("deleteTime", instance.DeleteTime);
                cmd.SetParameter("expirationTime", instance.ExpirationTime);
                cmd.SetParameter("failureCount", instance.FailureCount);
                cmd.SetParameter("priority", (int)instance.Priority);
                cmd.SetParameter("processTime", instance.ProcessTime);
                cmd.SetParameter("requestedTime", instance.RequestedTime);
                cmd.SetParameter("scheduledTime", instance.ScheduledTime);
                cmd.SetParameter("progress", instance.SerializedProgress);
                cmd.SetParameter("request", instance.SerializedRequest);
                cmd.SetParameter("status", (int)instance.Status);
                cmd.SetParameter("studyInstanceUid", instance.StudyInstanceUid);
                cmd.SetParameter("type", instance.Type);

                cmd.ExecuteNonQuery();

                // this is an INSERT, so we must retrieve and update the row id and version
                // 这是一个INSERT，因此我们必须检索并更新行ID和版本
                cmd.ReadInsertedRowIdentity(instance, "WorkItem");
            }
        }

        partial void UpdateWorkItem(WorkItem instance)
        {
            const string commandText = "UPDATE [WorkItem]"
                                       + "	SET [DeleteTime] = @deleteTime,"
                                       + "	[ExpirationTime] = @expirationTime,"
                                       + "	[FailureCount] = @failureCount,"
                                       + "	[Priority] = @priority,"
                                       + "	[ProcessTime] = @processTime,"
                                       + "	[RequestedTime] = @requestedTime,"
                                       + "	[ScheduledTime] = @scheduledTime,"
                                       + "	[SerializedProgress] = @progress,"
                                       + "	[SerializedRequest] = @request,"
                                       + "	[Status] = @status,"
                                       + "	[StudyInstanceUid] = @studyInstanceUid,"
                                       + "	[Type] = @type"
                                       + "	WHERE [Oid] = @oid"
                                       + "  AND [Version] = @version";
            using (var cmd = Connection.CreateCommand(commandText, Transaction))
            {
                cmd.SetParameter("oid", instance.Oid);
                cmd.SetParameter("deleteTime", instance.DeleteTime);
                cmd.SetParameter("expirationTime", instance.ExpirationTime);
                cmd.SetParameter("failureCount", instance.FailureCount);
                cmd.SetParameter("priority", (int)instance.Priority);
                cmd.SetParameter("processTime", instance.ProcessTime);
                cmd.SetParameter("requestedTime", instance.RequestedTime);
                cmd.SetParameter("scheduledTime", instance.ScheduledTime);
                cmd.SetParameter("progress", instance.SerializedProgress);
                cmd.SetParameter("request", instance.SerializedRequest);
                cmd.SetParameter("status", (int)instance.Status);
                cmd.SetParameter("studyInstanceUid", instance.StudyInstanceUid);
                cmd.SetParameter("type", instance.Type);
                cmd.SetParameter("version", instance.Version);

                // if update doesn't affect any rows, then a change conflict has occurred and we must inform LINQ-to-SQL about it
                // 如果更新不影响任何行，则发生更改冲突，我们必须通知LINQ-to-SQL
                if (cmd.ExecuteNonQuery() == 0)
                    throw new ChangeConflictException();

                // this is an UPDATE, so we must retrieve and update the row version
                // 这是一个更新，所以我们必须检索并更新行版本
                cmd.ReadUpdatedRowVersion(instance, "WorkItem");
            }
        }

        partial void InsertWorkItemUid(WorkItemUid instance)
        {
            const string commandText = "INSERT INTO [WorkItemUid]"
                                       + " ([Complete], [Failed], [FailureCount], [File], [SeriesInstanceUid], [SopInstanceUid], [WorkItemOid])"
                                       + " VALUES (@p1, @p2, @p3, @p4, @p5, @p6, @p7)";
            using (var cmd = Connection.CreateCommand(commandText, Transaction))
            {
                cmd.SetParameter("p1", instance.Complete);
                cmd.SetParameter("p2", instance.Failed);
                cmd.SetParameter("p3", instance.FailureCount);
                cmd.SetParameter("p4", instance.File);
                cmd.SetParameter("p5", instance.SeriesInstanceUid);
                cmd.SetParameter("p6", instance.SopInstanceUid);
                cmd.SetParameter("p7", instance.WorkItemOid);

                cmd.ExecuteNonQuery();

                // this is an INSERT, so we must retrieve and update the row id and version
                // 这是一个INSERT，因此我们必须检索并更新行ID和版本
                cmd.ReadInsertedRowIdentity(instance, "WorkItemUid");
            }
        }

        partial void UpdateWorkItemUid(WorkItemUid instance)
        {
            const string commandText = "UPDATE [WorkItemUid]"
                                       + " SET [Complete]=@p1, [Failed]=@p2, [FailureCount]=@p3, [File]=@p4, [SeriesInstanceUid]=@p5, [SopInstanceUid]=@p6, [WorkItemOid]=@p7"
                                       + " WHERE [Oid]=@oid AND [Version]=@ver";
            using (var cmd = Connection.CreateCommand(commandText, Transaction))
            {
                cmd.SetParameter("p1", instance.Complete);
                cmd.SetParameter("p2", instance.Failed);
                cmd.SetParameter("p3", instance.FailureCount);
                cmd.SetParameter("p4", instance.File);
                cmd.SetParameter("p5", instance.SeriesInstanceUid);
                cmd.SetParameter("p6", instance.SopInstanceUid);
                cmd.SetParameter("p7", instance.WorkItemOid);
                cmd.SetParameter("oid", instance.Oid);
                cmd.SetParameter("ver", instance.Version);

                // if update doesn't affect any rows, then a change conflict has occurred and we must inform LINQ-to-SQL about it
                // 如果更新不影响任何行，则发生更改冲突，我们必须通知LINQ-to-SQL
                if (cmd.ExecuteNonQuery() == 0)
                    throw new ChangeConflictException();

                // this is an UPDATE, so we must retrieve and update the row version
                // 这是一个更新，所以我们必须检索并更新行版本
                cmd.ReadUpdatedRowVersion(instance, "WorkItemUid");
            }
        }

        #endregion
    }
}