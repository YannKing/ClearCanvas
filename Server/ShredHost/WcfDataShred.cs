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
using System;

namespace ClearCanvas.Server.ShredHost
{
    /// <summary>创建的类仅允许在服务操作中进行WCF序列化和使用
    /// Class created only to allow WCF serialization and usage in service operations
    /// </summary>
    [Serializable]
    public partial class WcfDataShred
    {
        public WcfDataShred(int id, string name, string description, bool isRunning)
        {
            Platform.CheckForNullReference(name, "name");
            Platform.CheckForNullReference(description, "description");
            Platform.CheckForEmptyString(name, "name");

            _id = id;
            _name = name;
            _description = description;
            _isRunning = isRunning;
        }

        #region Properties
        private string _name;
        private string _description;
        private bool _isRunning;
        private int _id;

        public int Id
        {
            get { return _id; }
            set
            {
                _id = value;
            }
        }


        public bool IsRunning
        {
            get { return _isRunning; }
            set
            {
                _isRunning = value;
            }
        }

        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
            }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
            }
        }

        #endregion
    }
}
