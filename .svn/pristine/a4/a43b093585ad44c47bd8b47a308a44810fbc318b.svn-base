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

using JetBrains.Annotations;
using System;

namespace ClearCanvas.Common.Utilities
{
    /// <summary>当放置在派生自<see cref ="CommandLine"/>的类的字段/属性时，指示尝试根据解析的命令行参数设置字段/属性的基类。
    /// When placed on a field/property of a class derived from <see cref="CommandLine"/>, instructs
    /// the base class to attempt to set the field/property according to the parsed command line arguments.
    /// </summary>
    /// 
    /// <remarks>
    /// 如果field/property的类型为string，int或enum，则将其视为命名参数，
    /// 除非设置属性的<see cref ="Position"/>属性，在这种情况下，它被视为位置参数。
    /// 如果field/property的类型为boolean，则将其视为switch。
    /// 
    /// If the field/property is of type string, int, or enum, it is treated as a named parameter, unless
    /// the <see cref="Position"/> property of the attribute is set, in which case it is treated as a positional
    /// parameter.  If the field/property is of type boolean, it is treated as a switch.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false), MeansImplicitUse]
    public class CommandLineParameterAttribute : Attribute
    {
        private readonly int _position = -1;
        private bool _required;
        private readonly string _key;
        private readonly string _keyShortForm;
        private readonly string _usage;
        private readonly string _displayName;

        /// <summary>用于声明位置参数的构造函数。
        /// Constructor for declaring a positional parameter.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="displayName"></param>
        public CommandLineParameterAttribute(int position, string displayName)
        {
            _position = position;
            _displayName = displayName;
        }

        /// <summary>用于声明位置参数的构造函数。
        /// Constructor for declaring a positional parameter.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="displayName"></param>
        /// <param name="usage"> </param>
        public CommandLineParameterAttribute(int position, string displayName, string usage)
        {
            _position = position;
            _displayName = displayName;
            _usage = usage;
        }

        /// <summary>用于声明命名参数或布尔开关的构造函数。
        /// Constructor for declaring a named parameter or boolean switch.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="usage"></param>
        public CommandLineParameterAttribute(string key, string usage)
        {
            _key = key;
            _usage = usage;
        }

        /// <summary>用于声明命名参数或布尔开关的构造函数。
        /// Constructor for declaring a named parameter or boolean switch.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="keyShortForm"></param>
        /// <param name="usage"></param>
        public CommandLineParameterAttribute(string key, string keyShortForm, string usage)
        {
            _key = key;
            _keyShortForm = keyShortForm;
            _usage = usage;
        }

        /// <summary>获取或设置一个值，该值指示此参数是否为必需参数。
        /// Gets or sets a value indicating whether this parameter is a required parameter.
        /// </summary>
        public bool Required
        {
            get { return _required; }
            set { _required = value; }
        }

        /// <summary>获取位置参数的位置。
        /// Gets the position of a positional parameter.
        /// </summary>
        internal int Position
        {
            get { return _position; }
        }

        /// <summary>获取位置参数的显示名称。
        /// Gets the display name for a positional parameter.
        /// </summary>
        internal string DisplayName
        {
            get { return _displayName; }
        }

        /// <summary>获取命名参数的键（参数名称）。
        /// Gets the key (parameter name) for a named parameter.
        /// </summary>
        internal string Key
        {
            get { return _key; }
        }

        /// <summary>获取命名参数的关键短格式。
        /// Gets the key short-form for a named parameter.
        /// </summary>
        internal string KeyShortForm
        {
            get { return _keyShortForm; }
        }

        /// <summary>获取描述此参数用法的消息。
        /// Gets a message describing the usage of this parameter.
        /// </summary>
        internal string Usage
        {
            get { return _usage; }
        }

    }
}
