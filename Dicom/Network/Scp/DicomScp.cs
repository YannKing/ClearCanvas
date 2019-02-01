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
using ClearCanvas.Dicom.Network.Scu;
using NewLife.Log;
using System.Collections.Generic;
using System.Net;
using LogLevel = ClearCanvas.Common.LogLevel;

namespace ClearCanvas.Dicom.Network.Scp
{
    /// <summary>实现DICOM SCP的基类。
    /// Base class implementing a DICOM SCP.
    /// </summary>
    /// <remarks>
    /// <para>该类使用ClearCanvas.Dicom程序集来实现DICOM SCP。它处理与DICOM库的大多数基本交互。
    /// This class uses the ClearCanvas.Dicom assembly to implement a DICOM SCP.  
    /// It handles most of the basic interactions with the DICOM library.
    /// </para>
    /// 
    /// <para>该类依赖于<see cref ="ExtensionPoint"/>来处理动作DICOM服务。
    /// 该类将加载实现<see cref ="IDicomScp {TContext}"/>接口的插件。
    /// 它将查询这些插件以确定它们支持的DICOM服务，然后构建基于插件支持的传输语法和DICOM服务的列表。
    /// 
    /// The class depends on an <see cref="ExtensionPoint"/> for handling action DICOM 
    /// services.  The class will load plugins that implement the <see cref="IDicomScp{TContext}"/> interface.
    /// It will query these plugins to determine what DICOM Servies they support, and then 
    /// construct a list of transfer syntaxes and DICOM services supported based on the plugins.
    /// </para>
    /// <para>当请求消息到达时，将调用相应的插件来处理传入的消息。
    /// 请注意，不同的插件可以支持相同的DICOM服务，但是不同的传输语法。
    /// 
    /// When a request message arrives, the appropriate plugin will be called to process the
    /// incoming message.  Note that different plugins can support the same DICOM service, but 
    /// different transfer syntaxes.
    /// </para>
    /// </remarks>
    public class DicomScp<TContext>
    {
        /// <summary>代表打电话来核实是否应接受或拒绝协会。 
        /// Delegate called to verify if an association should be accepted or rejected.
        /// </summary>
        /// <remarks>如果在构造函数中指定<see cref ="DicomScp {TContext}"/>，则通过<see cref ="DicomScp {TContext}"/>调用此委托检查是否应拒绝或接受关联。
        /// If assigned in the constructor to <see cref="DicomScp{TContext}"/>, this delegate is called by <see cref="DicomScp{TContext}"/>
        /// to check if an association should be rejected or accepted.  
        /// </remarks>
        /// <param name="context">传递给构造函数的用户参数<see cref ="DicomScp {TContext}"/>
        /// User parameters passed to the constructor to <see cref="DicomScp{TContext}"/></param>
        /// <param name="assocParms">关联的参数。
        /// Parameters for the association.</param>
        /// <param name="result">如果委托返回false，则返回DICOM拒绝结果。
        /// If the delegate returns false, the DICOM reject result is returned here.</param>
        /// <param name="reason">如果委托返回false，则返回DICOM拒绝原因。
        /// If the delegate returns false, the DICOM reject reason is returned here.</param>
        /// <returns>如果应该接受关联，则为true;如果拒绝，则为false。
        /// true if the association should be accepted, false if rejected.</returns>
        public delegate bool AssociationVerifyCallback(TContext context, ServerAssociationParameters assocParms, out DicomRejectResult result, out DicomRejectReason reason);

        /// <summary>在关联完成后调用代理，其中包含已传输的存储映像列表。
        /// Delegate called after an association is complete with a list of Storage images transferred.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="assocParams"></param>
        /// <param name="instances"></param>
        public delegate void AssociationComplete(TContext context, ServerAssociationParameters assocParams, List<StorageInstance> instances);
        #region Constructors
        /// <summary>DICOM SCP的构造函数。
        /// Constructor for the DICOM SCP.
        /// </summary>
        /// 
        /// <remarks>
        /// <para>构造函数允许用户将对象传递给实现该对象的插件<请参阅cref ="IDicomScp {TContext}"/>界面。
        /// The constructor allows the user to pass an object to plugins that implement the 
        /// <see cref="IDicomScp{TContext}"/> interface. 
        /// </para>
        /// </remarks>
        /// 
        /// <param name="context">要传递给实现<see cref ="IDicomScp {TContext}"/>接口的插件的对象。
        /// An object to be passed to plugins implementing the <see cref="IDicomScp{TContext}"/> interface.</param>
        /// 
        /// <param name="verifier">当新协会到达时，代表会打电话来验证是否应该接受或拒绝该协会。可以设置为null。
        /// 
        /// Delegate called when a new association arrives to verify if it should be accepted or rejected.  Can be set to null.</param>
        public DicomScp(TContext context, AssociationVerifyCallback verifier)
        {
            _context = context;
            _verifier = verifier;
        }

        /// <summary>
        /// Constructor for the DICOM SCP.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The constructor allows the user to pass an object to plugins that implement the 
        /// <see cref="IDicomScp{TContext}"/> interface. 
        /// </para>
        /// </remarks>
        /// <param name="context">An object to be passed to plugins implementing the <see cref="IDicomScp{TContext}"/> interface.</param>
        /// <param name="verifier">Delegate called when a new association arrives to verify if it should be accepted or rejected.  Can be set to null.</param>
        /// <param name="complete">Delegate called when the association is complete/released relaying storage instance information.  Typically used for audit log purposes.</param>
        public DicomScp(TContext context, AssociationVerifyCallback verifier, AssociationComplete complete)
        {
            _context = context;
            _verifier = verifier;
            _complete = complete;
        }
        #endregion

        #region Private Members

        private ServerAssociationParameters _assocParameters;
        private readonly TContext _context;
        private readonly AssociationVerifyCallback _verifier;
        private readonly AssociationComplete _complete;
        #endregion

        #region Properties

        /// <summary>DICOM SCP的本地应用程序实体标题。 
        /// The local Application Entity Title of the DICOM SCP.
        /// </summary>
        public string AeTitle { get; set; }

        /// <summary>DICOM SCP的监听端口。
        /// The listen port of the DICOM SCP. 
        /// </summary>
        public int ListenPort { get; set; }

        /// <summary>DICOM SCP的监听端口。
        /// The listen port of the DICOM SCP. 
        /// </summary>
        public IPAddress ListenAddress { get; set; }


        /// <summary>用于协商关联的Association参数。
        /// The Association parameters used to negotiate the association.
        /// </summary>
        public ServerAssociationParameters AssociationParameters
        {
            get { return _assocParameters; }
        }

        /// <summary>与组件关联的上下文。
        /// The context associated with component.
        /// </summary>
        public TContext Context
        {
            get { return _context; }
        }
        #endregion

        #region Private Methods
        /// <summary>为DICOM SCP创建表示上下文列表。
        /// Create the list of presentation contexts for the DICOM SCP.
        /// </summary>
        /// <remarks>该方法加载DICOM Scp插件，然后查询它们构造支持的表示上下文列表。
        /// The method loads the DICOM Scp plugins, and then queries them
        /// to construct a list of presentation contexts that are supported.
        /// </remarks>
		private void CreatePresentationContexts()
        {
            var ep = new DicomScpExtensionPoint<TContext>();
            object[] scps = ep.CreateExtensions();
            foreach (object obj in scps)
            {
                var scp = obj as IDicomScp<TContext>;
                scp.SetContext(_context);

                IList<SupportedSop> sops = scp.GetSupportedSopClasses();
                foreach (SupportedSop sop in sops)
                {
                    byte pcid = _assocParameters.FindAbstractSyntax(sop.SopClass);
                    if (pcid == 0)
                        pcid = _assocParameters.AddPresentationContext(sop.SopClass);

                    // Now add all the transfer syntaxes, if necessary
                    //现在添加所有传输语法，如有必要
                    foreach (TransferSyntax syntax in sop.SyntaxList)
                    {
                        //检查语法是否已经注册
                        if (0 == _assocParameters.FindAbstractSyntaxWithTransferSyntax(sop.SopClass, syntax))
                        {
                            _assocParameters.AddTransferSyntax(pcid, syntax);
                        }
                    }
                }
            }

            // Sort the presentation contexts, and put them in the order that we prefer them.
            // Favor Explicit over Implicit transfer syntaxes, lossless compression over lossy
            // compression, and lossless compressed over uncompressed.

            // 对表示上下文进行排序，并按照我们喜欢的顺序进行排序。隐藏明确的隐式传输语法，有损无损压缩 压缩，无压缩压缩。
            foreach (DicomPresContext serverContext in _assocParameters.GetPresentationContexts())
            {
                serverContext.SortTransfers(
                    delegate (TransferSyntax s1, TransferSyntax s2)
                    {
                        if (s1.Equals(s2))
                            return 0;
                        if (s1.ExplicitVr && !s2.ExplicitVr)
                            return -1;
                        if (!s1.ExplicitVr && s2.ExplicitVr)
                            return 1;
                        if (s1.Encapsulated && s2.Encapsulated)
                        {
                            if (s1.LosslessCompressed == s2.LosslessCompressed)
                                return 0;
                            if (s1.LosslessCompressed && s2.LossyCompressed)
                                return -1;
                            return 1;
                        }
                        if (s1.Encapsulated)
                        {
                            if (s1.LossyCompressed)
                                return 1;
                            return -1;
                        }

                        if (s2.Encapsulated)
                        {
                            if (s2.LossyCompressed)
                                return -1;
                            return 1;

                        }
                        return 0;
                    });
            }
        }

        #endregion

        #region Public Methods
        /// <summary>委托与<see cref ="DicomServer"/>一起使用以创建处理程序为新的传入关联实现<see cref ="IDicomServerHandler"/>接口。
        /// 
        /// Delegate for use with <see cref="DicomServer"/> to create a handler
        /// that implements the <see cref="IDicomServerHandler"/> interface for a new incoming association.
        /// </summary>
        /// 
        /// <param name="assoc">协商关联的关联参数。
        /// The association parameters for the negotiated association.</param>
        /// 
        /// <param name="server">服务器。 The server.</param>
        /// <returns>一个新的<see cref ="DicomScpHandler {TContext}"/>实例。</returns>
        public IDicomServerHandler StartAssociation(DicomServer server, ServerAssociationParameters assoc)
        {
            return new DicomScpHandler<TContext>(server, assoc, _context, _verifier, _complete);
        }


        /// <summary>开始侦听关联。
        /// Start listening for associations.
        /// </summary>
        /// <returns>成功时为真，失败时为假。</returns>
        public bool Start()
        {
            if (ListenAddress != null)
                return Start(ListenAddress);

            Platform.Log(LogLevel.Fatal, "Attempted to listen on AE {0} with no Listening IP Address set.", AeTitle);
            XTrace.WriteLine($"尝试在没有侦听IP地址集的情况下收听AE：{AeTitle}。");
            return false;
        }


        /// <summary>开始侦听关联。
        /// Start listening for associations.
        /// </summary>
        /// <returns>成功时为真，失败时为假。</returns>
        public bool Start(IPAddress addr)
        {

            XTrace.WriteLine($"DicomScp=>Start方法：");
            XTrace.WriteLine($"IPADDRESS：{addr.ToString()}");
            try
            {
                ListenAddress = addr;

                _assocParameters = new ServerAssociationParameters(AeTitle, new IPEndPoint(addr, ListenPort));

                // Load our presentation contexts from all the extensions
                // 从所有扩展中加载我们的演示文稿上下文
                CreatePresentationContexts();

                if (_assocParameters.GetPresentationContextIDs().Count == 0)
                {
                    Platform.Log(LogLevel.Fatal, "No configured presentation contexts for AE: {0}", AeTitle);
                    XTrace.WriteLine($"没有配置AE的演示文稿上下文：{AeTitle}");
                    return false;
                }

                return DicomServer.StartListening(_assocParameters, StartAssociation);
            }
            catch (DicomException ex)
            {
                Platform.Log(LogLevel.Fatal, ex, "Unexpected exception when starting listener on port {0})", ListenPort);
                XTrace.WriteLine($"在端口{ListenPort}上启动侦听器时出现意外异常");
                return false;
            }
        }

        /// <summary>停止关联监听器。
        /// Stop the association listener.
        /// </summary>
        public void Stop()
        {
            try
            {
                DicomServer.StopListening(_assocParameters);
            }
            catch (DicomException e)
            {
                Platform.Log(LogLevel.Error, e, "Unexpected exception when stopping listening on port {0}", ListenPort);
                XTrace.WriteLine($"停止侦听端口{ListenPort}时出现意外异常");
            }
        }
        #endregion
    }
}
