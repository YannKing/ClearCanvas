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
using ClearCanvas.Common;
using ClearCanvas.ImageViewer;

namespace ClearCanvas.ImageViewer.StudyManagement
{
    /// <summary>
    /// Interface defining a 'transient reference' to a <see cref="Sop"/>.
    /// 界定<transient cref =“Sop”/>的“瞬态参考”的接口。
    /// </summary>
    /// <remarks>
    /// <para>
    /// In a Framework or SDK, managed objects often implement the
    /// <see cref="IDisposable"/> pattern even when they don't explicitly contain
    /// any unmanaged resources.  This is done in cases where it is possible, or perhaps very likely,
    /// that a derived class would in fact have unmanaged resources that need
    /// to be disposed or cleaned up.  Such is the case with classes like <see cref="ISopDataSource"/>.
    /// ///在框架或SDK中，托管对象经常实现
    /// <看到cref =“IDisposable”/>模式，即使它们没有明确包含
    ///任何非托管资源。 这是在有可能或很可能的情况下完成的
    ///派生类实际上需要非托管资源
    ///要处理或清理 像<see cref =“ISopDataSource”/>这样的类就是这种情况。
    /// </para>
    /// <para>
    /// Also, occasionally, the clear 'owner' of an object, particularly one that may be
    /// passed around from object to object, is not easy to determine, or simply doesn't exist.
    /// For managed objects that contain no unmanaged resources, this doesn't matter because
    /// the object(s) can simply be discarded and left for the garbage collector to
    /// clean up.  But what about objects that must be disposed, but have no clear owner?
    /// One might argue that this points to a design flaw, and that may very well be correct in most cases.
    /// However, one totally valid case of this is objects that are cached for reasons of
    /// memory conservation, like <see cref="ISopDataSource"/>, that must also implement
    /// <see cref="IDisposable"/>.  How do you determine when to properly dispose these objects
    /// when there is no one parent container that disposes them when it is itself disposed
    /// (for example, the <see cref="ImageViewerComponent"/> or it's <see cref="StudyTree"/>)?
    /// The only plausible way is to implement reference counting on the cached objects and only truly perform
    /// the disposal when the reference count goes to zero.
    /// ///此外，偶尔也是对象的明确“所有者”，特别是可能的对象
        ///从对象传递到对象，不容易确定，或者根本不存在。
        ///对于不包含非托管资源的托管对象，这无关紧要，因为
        ///可以简单地丢弃对象并将其留给垃圾收集器
        /// 清理。但是必须处理的对象呢，但没有明确的所有者呢？
        ///有人可能认为这指出了一个设计缺陷，在大多数情况下这可能是正确的。
        ///然而，一个完全有效的情况是由于原因而被缓存的对象
        ///内存保护，如<see cref =“ISopDataSource”/>，也必须实现
        /// <see cref =“IDisposable”/>。如何确定何时正确处理这些对象
        ///当没有一个父容器在它自己处理时处理它们时
        ///（例如，<see cref =“ImageViewerComponent”/>或者<see cref =“StudyTree”/>）？
        ///唯一合理的方法是对缓存的对象实现引用计数，并且只能真正执行
        ///当引用计数变为零时的处理。
    /// </para>
    /// <para>
    /// To solve all of these issues, enter the 'transient reference'.  So, how does it work?
    /// Basically, each transient reference object can itself be 'owned' by another object and disposed
    /// at the time the owning object is disposed.  This essentially allows many objects to reference
    /// the same shared/cached object, while also solving the problem of ownership.  Each object owns
    /// its reference object, and the object it points to doesn't actually have to be explicitly owned at all!
    /// Instead, all the entities that own a transient reference essentially share ownership of the referenced
    /// object, and only once all the transient references are disposed is the underlying referenced object
    /// disposed.  It's reference counting, but way better.  Because the 'reference count' is always equal
    /// to the total number of transient reference objects, things are suddenly much easier to manage.
    /// ///要解决所有这些问题，请输入“瞬态参考”。 那么它是怎样工作的？
         ///基本上，每个瞬态参考对象本身可以被另一个对象“拥有”并处理掉
         ///在处置拥有对象时。 这基本上允许许多对象引用
         ///同一个共享/缓存对象，同时也解决了所有权问题。 每个对象拥有
         ///它的引用对象，它指向的对象实际上根本不必显式拥有！
         ///相反，拥有瞬态引用的所有实体基本上共享引用的所有权
         /// object，并且只有在处理完所有瞬态引用后才是底层引用对象
         ///处置 它是引用计数，但方式更好。 因为'引用计数'总是相等的
         ///对于瞬态参考对象的总数，事情突然变得更容易管理。
    /// </para>
    /// <para>
    /// In the viewer framework, we don't typically work directly with <see cref="ISopDataSource"/>s because
    /// the interface is far too basic to have to work with directly.  Instead, we work with <see cref="Sop"/>,
    /// <see cref="ImageSop"/> and <see cref="Frame"/> objects that are simply bridges (see Bridge Design Pattern)
    /// to <see cref="ISopDataSource"/> and <see cref="ISopFrameData"/>.  The <see cref="Sop"/> class is also the entity responsible for
    /// transparently managing the caching of the <see cref="ISopDataSource"/>s passed to its constructor,
    /// therefore we work with 'transient references' to <see cref="Sop"/>s and <see cref="Frame"/>s instead
    /// of <see cref="ISopDataSource"/> and <see cref="ISopFrameData"/>.
    /// ///在查看器框架中，我们通常不直接使用<see cref =“ISopDataSource”/> s，因为
         ///界面太基本了，无法直接使用。 相反，我们使用<see cref =“Sop”/>，
         /// <see cref =“ImageSop”/>和<see cref =“Frame”/>只是桥梁的对象（参见桥梁设计模式）
         ///到<see cref =“ISopDataSource”/>和<see cref =“ISopFrameData”/>。 <see cref =“Sop”/>类也是负责的实体
         ///透明地管理传递给它的构造函数的<see cref =“ISopDataSource”/>的缓存，
         ///因此我们使用'transient references'来<see cref =“Sop”/> s和<see cref =“Frame”/> s代替
         /// <see cref =“ISopDataSource”/>和<see cref =“ISopFrameData”/>。
    /// </para>
    /// <para>
    /// The recommended practice, when taking a reference to a <see cref="Sop"/> outside of it's current owner
    /// and holding a reference to it (like in the Clipboard, for example) is to get a <see cref="ISopReference">transient reference</see>
    /// to the <see cref="Sop"/> or <see cref="Frame"/>.  When you are done with the transient reference, call Dispose on it.
    /// The object that owns the <see cref="Sop"/>, normally the <see cref="ImageViewerComponent"/> will dispose the
    /// <see cref="Sop"/>, but internally, the <see cref="Sop"/> will not do any cleanup until all transient references to it
    /// are also disposed.  If you create a <see cref="Sop"/> yourself, it is good practice to dispose the <see cref="Sop"/>
    /// <b>and</b> all it's transient references in order to ensure the <see cref="ISopDataSource"/> is released
    /// from the cache.
    /// ///建议的做法，当参考<see cref =“Sop”/>以外的当前所有者
         ///并保持对它的引用（例如在剪贴板中）是获取<see cref =“ISopReference”>瞬态引用</ see>
         ///到<see cref =“Sop”/>或<see cref =“Frame”/>。 完成瞬态参考后，请在其上调用Dispose。
         ///拥有<see cref =“Sop”/>的对象，通常<see cref =“ImageViewerComponent”/>将处置
         /// <see cref =“Sop”/>，但在内部，<see cref =“Sop”/>在所有瞬态引用之前都不会进行任何清理
         ///也被处理掉了。 如果你自己创建一个<see cref =“Sop”/>，最好配置<see cref =“Sop”/>
         /// <b>和</ b>所有它的瞬态引用，以确保<see cref =“ISopDataSource”/>被释放
         ///来自缓存。
    /// </para>
    /// </remarks>
    public interface ISopReference : ISopProvider, IDisposable
	{
		/// <summary>
		/// Clones an existing <see cref="ISopReference"/>, creating a new transient reference.
		/// </summary>
		ISopReference Clone();
	}

	public partial class Sop
	{
		private class SopReference : ISopReference
		{
			private Sop _sop;

			public SopReference(Sop sop)
			{
				_sop = sop;
				_sop.OnReferenceCreated();
			}

			#region ISopProvider Members

			public Sop Sop
			{
				get { return _sop; }
			}

			#endregion

			#region ICachedSop Members

			public ISopReference Clone()
			{
				return _sop.CreateTransientReference();
			}

			#endregion

			#region IDisposable Members

			public void Dispose()
			{
				if (_sop != null)
				{
					_sop.OnReferenceDisposed();
					_sop = null;
				}
			}

			#endregion
		}

		#region Sop Stuff for Transient References

		private readonly object _syncLock = new object();
		private int _transientReferenceCount = 0;
		private bool _selfDisposed = false;

		private void OnReferenceDisposed()
		{
			lock(_syncLock)
			{
				if (_transientReferenceCount > 0)
					--_transientReferenceCount;

				if (_transientReferenceCount == 0 && _selfDisposed)
					DisposeInternal();
			}
		}
		
		private void OnReferenceCreated()
		{
			lock(_syncLock)
			{
				if (_transientReferenceCount == 0 && _selfDisposed)
					throw new ObjectDisposedException("The underlying sop data source has already been disposed.");

				++_transientReferenceCount;
			}
		}

		private void DisposeInternal()
		{
			try
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}
			catch(Exception e)
			{
				Platform.Log(LogLevel.Warn, e);
			}
		}

		/// <summary>
		/// Creates a new 'transient reference' to this <see cref="Sop"/>.
		/// </summary>
		/// <remarks>See <see cref="ISopReference"/> for a detailed explanation of 'transient references'.</remarks>
		public ISopReference CreateTransientReference()
		{
			return new SopReference(this);
		}

		/// <summary>
		/// Implementation of the <see cref="IDisposable"/> pattern.
		/// </summary>
		public void Dispose()
		{
			lock(_syncLock)
			{
				_selfDisposed = true;

				//Only dispose for real when self has been disposed and all the transient references have been disposed.
				if (_transientReferenceCount == 0)
					DisposeInternal();
			}
		}

		#endregion
	}
}