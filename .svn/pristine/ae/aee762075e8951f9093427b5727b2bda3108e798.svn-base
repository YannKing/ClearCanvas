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

using System.Runtime.Serialization;

namespace ClearCanvas.Common.Authorization
{
    /// <summary>Helper类，用于提供在部署时导入的权限组定义。
    /// Helper class for providing authority group definitions to be imported at deployment time.
    /// </summary>
    /// <seealso cref="AuthorityTokenAttribute"/>
    [DataContract]
    public class AuthorityGroupDefinition
    {
        /// <summary>默认构造函数。
        /// Default constructor.
        /// </summary>
        // Required for JSML deserialization - do not remove
        // JSML反序列化所必需的 - 不要删除
        public AuthorityGroupDefinition()
        {
            this.Tokens = new string[0];
        }


        /// <summary>构造函数。
        /// Constructor.
        /// </summary>
        /// <param name="name">权限组的名称。
        /// The name of the authority group.</param>
        /// <param name="tokens">关联的权限组令牌。
        /// The associated authority group tokens.</param>
        public AuthorityGroupDefinition(string name, string[] tokens) : this(name, name, false, tokens, false)
        {
        }

        /// <summary>构造函数。</summary>
        /// <param name="name">权限组的名称。
        /// The name of the authority group.</param>
        /// <param name="tokens">关联的权限组令牌。
        /// The associated authority group tokens.</param>
        /// <param name="dataGroup">判断该组是否是用于控制数据访问的权限组。
        /// Tells if the group is an authority group for controlling access to data.</param>
        /// <param name="description">权限组的描述。
        /// The description of the authority group.</param>
        public AuthorityGroupDefinition(string name, string description, bool dataGroup, string[] tokens) : this(name, description, dataGroup, tokens, false)
        {
        }

        /// <summary>构造函数。</summary>
        /// <param name="name">权限组的名称。
        /// The name of the authority group.</param>
        /// <param name="tokens">关联的权限组令牌。
        /// The associated authority group tokens.</param>
        /// <param name="dataGroup">判断该组是否是用于控制数据访问的权限组。
        /// Tells if the group is an authority group for controlling access to data.</param>
        /// <param name="description">权限组的描述。
        /// The description of the authority group.</param>
        /// <param name="builtIn"> </param>
        public AuthorityGroupDefinition(string name, string description, bool dataGroup, string[] tokens, bool builtIn)
        {
            Name = name;
            Tokens = tokens;
            Description = description;
            DataGroup = dataGroup;
            BuiltIn = builtIn;
        }

        /// <summary>获取权限组的名称。
        /// Gets the name of the authority group.
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>获取权限组的名称。
        /// Gets the name of the authority group.
        /// </summary>
        [DataMember]
        public string Description { get; set; }

        /// <summary>如果权限组是内置组，则获取bool信号。
        /// Gets a bool signaling if the authority group is a built-in group.
        /// </summary>
        [DataMember]
        public bool BuiltIn { get; set; }


        /// <summary>如果权限组用于数据访问，则获取bool信号。
        /// Gets a bool signaling if the authority group is for Data access.
        /// </summary>
        [DataMember]
        public bool DataGroup { get; set; }

        /// <summary>获取分配给该组的标记集。
        /// Gets the set of tokens that are assigned to the group.
        /// </summary>
        [DataMember]
        public string[] Tokens { get; set; }
    }
}
