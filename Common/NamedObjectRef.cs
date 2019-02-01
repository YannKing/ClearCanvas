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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;

namespace ClearCanvas.Common
{
    /// <summary>
    /// Base class for <see cref="TypeRef"/> and <see cref="AssemblyRef"/>.
    /// </summary>
    /// <typeparam name="T">The type of the referenced object.</typeparam>
    /// <remarks>
    /// Instances of this class are immutable and safe for concurrent access by multiple threads.
    /// </remarks>
    public abstract class NamedObjectRef<T> where T : class
    {
        private readonly string _name;
        private readonly object _syncObj = new object();
        private T _obj;

        protected NamedObjectRef(string name, T obj)
        {
            _name = name;
            _obj = obj;
        }

        protected NamedObjectRef(string name)
        {
            _name = name;
        }

        public override string ToString()
        {
            return _name;
        }

        /// <summary>
        /// Gets a value indicating whether the referenced object has been resolved.
        /// </summary>
        public bool IsResolved
        {
            get { return _obj != null; }
        }

        /// <summary>
        /// Resolves the reference and returns the referenced object.
        /// </summary>
        /// <returns></returns>
        public T Resolve()
        {
            if (_obj != null)
                return _obj;

            lock (_syncObj)
            {
                return _obj ?? (_obj = ResolveObject(_name));
            }
        }

        protected bool EqualsHelper(NamedObjectRef<T> other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (ReferenceEquals(null, other)) return false;

            return _name == other._name;
        }

        protected bool EqualsHelper(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;

            var other = obj as NamedObjectRef<T>;
            if (ReferenceEquals(null, other)) return false;

            return _name == other._name;
        }

        protected int HashCodeHelper()
        {
            return _name.GetHashCode();
        }

        public string Name
        {
            get { return _name; }
        }

        protected abstract T ResolveObject(string name);
    }

    /// <summary>
    /// Represents a reference to a <see cref="Type"/> object.
    /// </summary>
    /// <remarks>
    /// <para>
    /// To obtain a <see cref="TypeRef"/> instance, use an overload of <see cref="TypeRef.Get"/>.
    /// Alternatively, an existing <see cref="Type"/> object can be implicitly cast to a
    /// <see cref="TypeRef"/> instance.
    /// </para>
    /// <para>
    /// Instances of this class are immutable and safe for concurrent access by multiple threads.
    /// </para>
    /// </remarks>
    public sealed class TypeRef : NamedObjectRef<Type>, IEquatable<TypeRef>
    {
        #region SerializationSurrogate

        /// <summary>序列化助手类。</summary>
        public class SerializationSurrogate : ISerializationSurrogate
        {
            public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
            {
                var r = (TypeRef)obj;
                info.AddValue("name", r.Name, typeof(string));
            }

            public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
            {
                var name = (string)info.GetValue("name", typeof(string));
                return Get(name);
            }
        }

        #endregion

        private static readonly Dictionary<string, TypeRef> _interns = new Dictionary<string, TypeRef>();
        private static readonly ConcurrentDictionary<Type, string> _assemblyQualifiedTypeNameCache = new ConcurrentDictionary<Type, string>();

        /// <summary>获取与指定类型对应的<see cref ="TypeRef"/>。</summary>
        public static TypeRef Get(string assemblyQualifiedTypeName)
        {
            lock (_interns)
            {
                TypeRef r;
                if (!_interns.TryGetValue(assemblyQualifiedTypeName, out r))
                {
                    _interns.Add(assemblyQualifiedTypeName, r = new TypeRef(assemblyQualifiedTypeName));
                }
                return r;
            }
        }

        /// <summary>获取与指定类型对应的<see cref ="TypeRef"/>。 </summary>
        public static TypeRef Get(Type type)
        {
            var name = GetAssemblyQualifiedName(type);
            lock (_interns)
            {
                TypeRef r;
                if (!_interns.TryGetValue(name, out r))
                {
                    _interns.Add(name, r = new TypeRef(name, type));
                }
                return r;
            }
        }

        private readonly object _syncObj = new object();
        private string _typeFullName;
        private string _assemblyName;

        /// <summary>私人构造函数。</summary>
        private TypeRef(string assemblyQualifiedTypeName, Type type)
         : base(assemblyQualifiedTypeName, type)
        {
        }

        /// <summary>私有构造函数。</summary>
        private TypeRef(string assemblyQualifiedTypeName) : base(assemblyQualifiedTypeName)
        {
        }

        /// <summary>获取引用类型的全名。</summary>
        public string FullName
        {
            get
            {
                if (_typeFullName != null)
                    return _typeFullName;

                lock (_syncObj)
                {
                    if (_typeFullName == null)
                        ParseName();
                    return _typeFullName;
                }
            }
        }

        /// <summary>获取定义类型的程序集的名称。</summary>
        public string AssemblyName
        {
            get
            {
                if (_assemblyName != null)
                    return _assemblyName;

                lock (_syncObj)
                {
                    if (_assemblyName == null)
                        ParseName();
                    return _assemblyName;
                }
            }
        }

        /// <summary>将<see cref ="Type"/>转换为<see cref ="TypeRef"/>。</summary>
        public static implicit operator TypeRef(Type type)
        {
            return Get(type);
        }

        public override bool Equals(object obj)
        {
            return EqualsHelper(obj);
        }

        public override int GetHashCode()
        {
            return HashCodeHelper();
        }

        public bool Equals(TypeRef other)
        {
            return EqualsHelper(other);
        }

        public static bool operator ==(TypeRef left, TypeRef right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(TypeRef left, TypeRef right)
        {
            return !Equals(left, right);
        }

        protected override Type ResolveObject(string name)
        {
            //确保在尝试解析类型之前加载程序集
            AssemblyRef.Get(this.AssemblyName).Resolve();

            return Type.GetType(name, true);
        }

        private void ParseName()
        {
            var parts = this.Name.Split(',');
            _typeFullName = parts[0].Trim();
            _assemblyName = parts[1].Trim();
        }

        private static string GetAssemblyQualifiedName(Type type)
        {
            return _assemblyQualifiedTypeNameCache.GetOrAdd(type, t => string.Format("{0}, {1}", t.FullName, t.Assembly.GetName().Name));
        }
    }

    /// <summary>表示对<see cref ="Assembly"/>对象的引用。</summary>
    /// <remarks>
    /// <para>
    /// 要获得<see cref ="AssemblyRef"/>实例，请使用<see cref ="AssemblyRef.Get"/>的重载。
    /// 或者，可以将现有的<see cref ="Assembly"/>对象隐式地转换为<see cref ="AssemblyRef"/>实例。
    ///
    /// To obtain an <see cref="AssemblyRef"/> instance, use an overload of <see cref="AssemblyRef.Get"/>.
    /// Alternatively, an existing <see cref="Assembly"/> object can be implicitly cast to an
    /// <see cref="AssemblyRef"/> instance.
    /// </para>
    /// <para>此类的实例是不可变的，并且对多个线程的并发访问是安全的。
    /// 
    /// Instances of this class are immutable and safe for concurrent access by multiple threads.
    /// </para>
    /// </remarks>
    public class AssemblyRef : NamedObjectRef<Assembly>, IEquatable<AssemblyRef>
    {
        #region SerializationSurrogate

        /// <summary>序列化助手类。</summary>
        public class SerializationSurrogate : ISerializationSurrogate
        {
            public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
            {
                var r = (AssemblyRef)obj;
                info.AddValue("name", r.Name, typeof(string));
            }

            public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
            {
                var name = (string)info.GetValue("name", typeof(string));
                return Get(name);
            }
        }

        #endregion

        private static readonly Dictionary<string, AssemblyRef> _interns = new Dictionary<string, AssemblyRef>();
        private static Func<string, Assembly> _assemblyResolver;

        /// <summary>获取与指定程序集对应的<see cref ="AssemblyRef"/>。</summary>
        public static AssemblyRef Get(string assemblyName)
        {
            lock (_interns)
            {
                AssemblyRef r;
                if (!_interns.TryGetValue(assemblyName, out r))
                {
                    _interns.Add(assemblyName, r = new AssemblyRef(assemblyName));
                }
                return r;
            }
        }

        /// <summary>获取与指定程序集对应的<see cref ="AssemblyRef"/>。</summary>
        public static AssemblyRef Get(Assembly assembly)
        {
            var name = assembly.GetName().Name;
            lock (_interns)
            {
                AssemblyRef r;
                if (!_interns.TryGetValue(name, out r))
                {
                    _interns.Add(name, r = new AssemblyRef(name, assembly));
                }
                return r;
            }
        }

        /// <summary>设置全局程序集解析程序。</summary>
        /// <param name="assemblyResolver"></param>
        internal static void SetResolver(Func<string, Assembly> assemblyResolver)
        {
            if (_assemblyResolver != null)
                throw new InvalidOperationException("装配解析器已经设置好了。 The assembly resolver has already been set.");

            _assemblyResolver = assemblyResolver;
        }

        /// <summary>私人构造函数。</summary>
        private AssemblyRef(string assemblyName, Assembly assembly) : base(assemblyName, assembly)
        {
        }

        /// <summary>私人构造函数。</summary>
        private AssemblyRef(string assemblyName) : base(assemblyName)
        {
        }

        /// <summary>将<see cref ="Assembly"/>转换为<see cref ="AssemblyRef"/>。</summary>
        public static implicit operator AssemblyRef(Assembly assembly)
        {
            return Get(assembly);
        }

        public override bool Equals(object obj)
        {
            return EqualsHelper(obj);
        }

        public override int GetHashCode()
        {
            return HashCodeHelper();
        }

        public bool Equals(AssemblyRef other)
        {
            return EqualsHelper(other);
        }

        public static bool operator ==(AssemblyRef left, AssemblyRef right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(AssemblyRef left, AssemblyRef right)
        {
            return !Equals(left, right);
        }

        protected override Assembly ResolveObject(string name)
        {
            return _assemblyResolver(name);
        }
    }

}
