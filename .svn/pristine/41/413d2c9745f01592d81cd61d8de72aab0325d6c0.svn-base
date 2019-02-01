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

using ClearCanvas.Dicom;
using System;
using ClearCanvas.Dicom.Iod.Modules;
using ClearCanvas.ImageViewer.Common;

namespace ClearCanvas.ImageViewer.StudyManagement
{
    /// <summary>
    /// Defines the methods and properties of the data source for a <see cref="Sop"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A sop 'data source' is merely an abstraction of what is essentially just
    /// a <see cref="DicomFile"/>.  However, because the data is not always a simple
    /// local file, we have to account for the fact that there may be parts
    /// of the implementation that must be specialized because, for example,
    /// the data exists on a remote server and might be retrieved on demand
    /// or in stages.  An example of this is the ClearCanvas Streaming implementation
    /// where:
    /// <list type="bullet">
    /// <item>A <see cref="DicomFile"/> is initially constructed from an xml
    /// document retrieved from the server.</item>
    /// <item>The xml document often will contain the entire image header, but
    /// unknown, private and large tags are excluded.  When it is determined that
    /// a tag being requested is one of these tags, the <see cref="ISopDataSource"/>
    /// will silently retrieve the entire image header on-demand.</item>
    /// <item>Pixel data is always loaded on-demand in streaming and local cases.</item>
    /// </list>
    /// </para>
    /// <para>
    /// Note that there is a significant shift in the design from previous versions
    /// of the framework, where <see cref="ISopDataSource"/> did not exist and
    /// any specializations had to be done by deriving from <see cref="ImageSop"/>.
    /// This caused a number of problems, the main one being that,
    /// when we added support for non-image <see cref="Sop"/>s (like Key Objects) and
    /// <see cref="Sop"/> became non-abstract, all of a sudden we had to create 2 specialized
    /// classes, one for <see cref="Sop"/> and one for <see cref="ImageSop"/>; there
    /// was no way around it because client code depended on checking the type (<see cref="Sop"/> or <see cref="ImageSop"/>)
    /// in order to know whether or not it was an image.  Also, <see cref="Sop"/> and <see cref="ImageSop"/>
    /// are rather large classes, and ultimately we realized that all they do
    /// is provide a convenient interface to a <see cref="DicomFile"/>.  So, why
    /// make developers inherit from <see cref="Sop"/>, <see cref="ImageSop"/> and <see cref="Frame"/>
    /// and force them to have to figure out which of the many properties and methods to override when
    /// it's really not necessary?  Also, now that we support non-image <see cref="Sop"/>s, how come
    /// there's just <see cref="Sop"/> and <see cref="ImageSop"/>?  Where's KeyImageSop and PresentationStateSop?
    /// All of a sudden, we were headed down a very bad road.  Such is development.
    /// </para>
    /// <para>
    /// So how does <see cref="ISopDataSource"/> make it easier?  The biggest way
    /// has already been mentioned, which is that it unifies all sop instances, image or otherwise, under
    /// a single interface, so you only have to implement <see cref="ISopDataSource"/> (and possibly <see cref="ISopFrameData"/>).
    /// <see cref="ISopDataSource"/> has a few specialized bits of functionality for handling of <b>large</b> image data,
    /// such as pixel data and overlays, but that's it - everything else is the same as for non-images.  The other
    /// nice thing about <see cref="ISopDataSource"/> is that it can be used with the various convenient
    /// wrapper classes in <see cref="ClearCanvas.Dicom.Iod.Modules"/>, such as <see cref="KeyObjectDocumentModuleIod"/>.
    /// The answer to the question "why isn't there a Key Object Sop?" is: there doesn't need to be.  You
    /// can simply wrap the <see cref="ISopDataSource"/> in whichever convenient IOD wrapper you need for
    /// a particular <see cref="SopClass">SOP class</see>.
    /// sop'数据源'仅仅是对基本上正义的抽象
    /// a <see cref =“DicomFile”/>。但是，因为数据并不总是很简单
    ///本地文件，我们必须考虑到可能存在部分的事实
    ///必须专门化的实现，例如，
    ///数据存在于远程服务器上，可能会按需检索
    ///或分阶段一个例子是ClearCanvas Streaming实现
    ///其中：
    /// <list type =“bullet”>
    /// <item> <see cref =“DicomFile”/>最初是从xml构造的
    ///从服务器检索的文档。</ item>
    /// <item> xml文档通常包含整个图像标题，但是
    ///未知，私有和大型标签被排除在外。当确定时
    ///被请求的标签是这些标签之一，<see cref =“ISopDataSource”/>
    ///将按需静默检索整个图像标题。</ item>
    /// <item>在流媒体和本地案例中始终按需加载像素数据。</ item>
    /// </ list>
    /// </ para>
    /// <para>
    ///请注意，设计与以前的版本有重大差异
    ///框架，其中<see cref =“ISopDataSource”/>不存在
    ///必须通过派生<see cref =“ImageSop”/>来完成任何专业化。
    ///这引起了许多问题，主要问题是，
    ///当我们添加对非图像的支持<see cref =“Sop”/> s（如密钥对象）和
    /// <see cref =“Sop”/>变得非抽象，突然间我们不得不创建2个专门的
    ///类，一个用于<see cref =“Sop”/>，另一个用于<see cref =“ImageSop”/>;那里
    ///没办法，因为客户端代码依赖于检查类型（<see cref =“Sop”/>或<see cref =“ImageSop”/>）
    ///以便知道它是否是图像。另外，<see cref =“Sop”/>和<see cref =“ImageSop”/>
    ///是相当大的类，最终我们意识到他们所做的一切
    ///为<see cref =“DicomFile”/>提供了一个方便的界面。所以为什么
    ///使开发人员继承自<see cref =“Sop”/>，<see cref =“ImageSop”/>和<see cref =“Frame”/>
    ///并强制他们必须弄清楚要覆盖的许多属性和方法中的哪一个
    ///这真的没必要吗？此外，现在我们支持非图像<see cref =“Sop”/> s，为什么呢？
    ///只有<see cref =“Sop”/>和<see cref =“ImageSop”/>？ KeyImageSop和PresentationStateSop在哪里？
    ///突然之间，我们走上了一条非常糟糕的道路。这就是发展。
    /// </ para>
    /// <para>
    ///那么<see cref =“ISopDataSource”/>如何让它变得更容易？最大的方式
    ///已被提及，即它统一了所有sop实例，图像或其他内容
    ///单个接口，所以你只需要实现<see cref =“ISopDataSource”/>（可能<see cref =“ISopFrameData”/>）。
    /// <see cref =“ISopDataSource”/>有一些专门用于处理<b>大</ b>图像数据的功能，
    ///，例如像素数据和叠加层，但就是这样 - 其他所有内容都与非图像相同。另一个
    ///关于<see cref =“ISopDataSource”/>的好处是它可以与各种方便使用
    /// <cref =“ClearCanvas.Dicom.Iod.Modules”/>中的包装类，例如<see cref =“KeyObjectDocumentModuleIod”/>。
    ///问题的答案“为什么没有Key Object Sop？”是：没有必要。您
    ///可以简单地将<see cref =“ISopDataSource”/>包装在您需要的任何方便的IOD包装器中
    ///特定的<see cref =“SopClass”> SOP类</ see>。
    /// </para>
    /// </remarks>
    public interface ISopDataSource : IDicomAttributeProvider, IDisposable
	{
		/// <summary>
		/// Gets the Patient ID.
		/// </summary>
		string PatientId { get; }

		/// <summary>
		/// Gets the study instance UID.
		/// </summary>
		string StudyInstanceUid { get; }

		/// <summary>
		/// Gets the series instance UID.
		/// </summary>
		string SeriesInstanceUid { get; }

		/// <summary>
		/// Gets the SOP instance UID.
		/// </summary>
		string SopInstanceUid { get; }

		/// <summary>
		/// Gets the instance number.
		/// </summary>
		int InstanceNumber { get; }

		/// <summary>
		/// Gets the SOP class UID.
		/// </summary>
		string SopClassUid { get; }

		/// <summary>
		/// Gets the transfer syntax UID.
		/// </summary>
		string TransferSyntaxUid { get; }

		/// <summary>
		/// Gets a value indicating whether or not the SOP instance is 'stored',
		/// for example in the local store or on a remote PACS server.
		/// </summary>
		/// <remarks>
		/// This would normally be used to determine whether an <see cref="ISopDataSource">data source</see>
		/// is one that is generated and only exists in memory, as the treatment of such a sop
		/// might be different in some cases.
		/// </remarks>
		bool IsStored { get; }

		/// <summary>
		/// Gets the source server where this data source was loaded from.
		/// </summary>
		IDicomServiceNode Server { get; }

		/// <summary>
		/// Gets a value indicating whether or not the SOP instance is an image.
		/// </summary>
		bool IsImage { get; }

		/// <summary>
		/// Gets the number of frames in this SOP instance.
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown if the SOP instance is not an image.</exception>
		int NumberOfFrames { get; }

		/// <summary>
		/// Gets the data for a particular frame in the SOP instance, if it is an image.
		/// </summary>
		/// <param name="frameNumber">The 1-based number of the frame for which the data is to be retrieved.</param>
		/// <returns>An <see cref="ISopFrameData"/> containing frame-specific data.</returns>
		/// <exception cref="InvalidOperationException">Thrown if this <see cref="ISopDataSource"/> is not an image
		/// (e.g. <see cref="IsImage"/> returns false).</exception>
		ISopFrameData GetFrameData(int frameNumber);
	}
}
