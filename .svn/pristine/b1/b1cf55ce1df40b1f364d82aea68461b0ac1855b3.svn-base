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
using ClearCanvas.Desktop;
using System;
using System.Collections.Generic;
using System.IO;

namespace ClearCanvas.ImageViewer.StudyManagement
{
    /// <summary>Helper类用于从本地文件创建，填充和启动<see cref ="ImageViewerComponent"/>。
    /// Helper class to create, populate and launch an <see cref="ImageViewerComponent"/> from local files.
    /// </summary>
    public class OpenFilesHelper
    {
        private readonly List<string> _filenames = new List<string>();

        /// <summary>
        /// Constructs a new instance of <see cref="OpenFilesHelper"/>.
        /// </summary>
        public OpenFilesHelper()
        {
            WindowBehaviour = WindowBehaviour.Auto;
            HandleErrors = true;
        }

        /// <summary>构造<see cref ="OpenFilesHelper"/>的新实例，并将指定的文件添加到要打开的图像列表中。
        /// Constructs a new instance of <see cref="OpenFilesHelper"/> and adds the specified files to the list of images to open.
        /// </summary>
        /// <param name="filenames">A list of paths to the files to be opened.</param>
        public OpenFilesHelper(IEnumerable<string> filenames) : this()
        {
            AddFiles(filenames);
        }

        /// <summary>获取或设置<see cref ="WindowBehaviour"/>以启动<see cref ="ImageViewerComponent"/>。
        /// Gets or sets the <see cref="WindowBehaviour"/> for launching the <see cref="ImageViewerComponent"/>.
        /// </summary>
        public WindowBehaviour WindowBehaviour { get; set; }

        /// <summary>获取或设置<see cref ="ImageViewerComponent"/>的工作空间标题。
        /// Gets or sets the workspace title for the <see cref="ImageViewerComponent"/>.
        /// </summary>
        /// <remarks>此值可以为null，从而指示组件应自动生成适当的标题。
        /// This value may be null, thus indicating that the component should automatically generate an appropriate title.
        /// </remarks>
        public string Title { get; set; }

        /// <summary>获取或设置是否允许打开空查看器（例如，未加载图像）。
        /// Gets or sets whether or not to allow an empty viewer to be opened (e.g. with no images loaded).
        /// </summary>
        public bool AllowEmptyViewer { get; set; }

        /// <summary>指定是否应自动处理错误（例如，向用户显示错误）。
        /// Specifies whether or not errors should be handled automatically (e.g. error shown to the user).
        /// </summary>
        public bool HandleErrors { get; set; }

        /// <summary>获取用户是否取消了图像的打开。
        /// Gets whether or not the user cancelled the opening of the images.
        /// </summary>
        public bool UserCancelled { get; private set; }

        /// <summary>获取或设置所有者<see cref ="IDesktopWindow"/>。
        /// Gets or sets the owner <see cref="IDesktopWindow"/>.
        /// </summary>
        /// 
        /// <remarks>
        /// <para>
        /// 这是<see cref ="IDesktopWindow"/>，将在其上显示错误消息如果没有在新窗口中启动，<see cref ="ImageViewerComponent"/>将启动。
        /// 
        /// This is the <see cref="IDesktopWindow"/> on which error messages will be shown and
        /// on which the <see cref="ImageViewerComponent"/> will launch if not launching in a new window.
        /// </para>
        /// 
        /// <para>此值可以为null，因此表示应使用当前活动的<see cref ="IDesktopWindow"/>。
        /// This value may be null, thus indicating that the currently active <see cref="IDesktopWindow"/> should be used.
        /// </para>
        /// 
        /// </remarks>
        public IDesktopWindow DesktopWindow { get; set; }

        /// <summary>将文件添加到要打开的图像列表中。
        /// Adds a file to the list of images to be opened.
        /// </summary>
        /// <param name="filename">要打开的文件的路径。
        /// The path to the file to be opened.</param>
        public void AddFile(string filename)
        {
            Platform.CheckForNullReference(filename, "filename");
            if (!File.Exists(filename))
                throw new ArgumentException("Invalid file path.", "filename");
            _filenames.Add(filename);
        }

        /// <summary>将文件添加到要打开的图像列表中。
        /// Adds files to the list of images to be opened.
        /// </summary>
        /// <param name="filenames">要打开的文件的路径列表。
        /// A list of paths to the files to be opened.</param>
        public void AddFiles(IEnumerable<string> filenames)
        {
            Platform.CheckForNullReference(filenames, "filenames");
            foreach (var filename in filenames)
            {
                // validate all filenames before adding any to the list, so we don't get any weird partially added situations
                // 在将任何文件名添加到列表之前验证所有文件名，因此我们不会得到任何奇怪的部分添加情况
                if (!File.Exists(filename))
                    throw new ArgumentException("Invalid file path.", "filenames");
            }
            _filenames.AddRange(filenames);
        }

        /// <summary>将目录的内容添加到要打开的图像列表中。
        /// Adds the contents of a directory to the list of images to be opened.
        /// </summary>
        /// <param name="directory">包含要打开的文件的目录。
        /// The directory containing files to be opened.</param>
        /// <param name="recursive">如果应递归添加子目录的文件，则为True;否则为false。否则就错了。
        /// True if the files of subdirectories should be recursively added; False otherwise.</param>
        public void AddDirectory(string directory, bool recursive)
        {
            Platform.CheckForNullReference(directory, "directory");
            if (!Directory.Exists(directory))
                throw new ArgumentException("Invalid directory path.", "directory");

            FileProcessor.Process(directory, "*.*", _filenames.Add, recursive);
        }

        /// <summary>
        /// Recursively adds the contents of a directory to the list of images to be opened.
        /// </summary>
        /// <param name="directory">The directory containing files to be opened.</param>
        public void AddDirectory(string directory)
        {
            AddDirectory(directory, true);
        }

        /// <summary>创建<see cref ="ImageViewerComponent"/>，加载指定的图像，并启动<see cref ="ImageViewerComponent"/>。
        /// Creates the <see cref="ImageViewerComponent"/>, loads the specified images,
        /// and launches the <see cref="ImageViewerComponent"/>.
        /// </summary>
        public ImageViewerComponent OpenFiles()
        {
            ImageViewerComponent viewer = null;
            BlockingOperation.Run(delegate { viewer = LoadAndOpenFiles(); });
            return viewer;
        }

        /// <summary>加载并打开文件</summary>
        /// <returns></returns>
        private ImageViewerComponent LoadAndOpenFiles()
        {
            var codeClock = new CodeClock();
            codeClock.Start();

            // don't find priors for files loaded off the local disk.
            var viewer = CreateViewer(false); //找不到从本地磁盘加载的文件的先验。
            var desktopWindow = DesktopWindow ?? Application.ActiveDesktopWindow;

            try
            {
                UserCancelled = false;
                bool cancelled;
                viewer.LoadImages(_filenames.ToArray(), desktopWindow, out cancelled);
                UserCancelled = cancelled;
            }
            catch (Exception e)
            {
                if (!HandleErrors)
                    throw;

                ExceptionHandler.Report(e, SR.MessageFailedToOpenImages, desktopWindow);
            }

            if (UserCancelled || (!AnySopsLoaded(viewer) && !AllowEmptyViewer))
            {
                viewer.Dispose();
                return null;
            }

            var args = new LaunchImageViewerArgs(WindowBehaviour) { Title = Title };
            ImageViewerComponent.Launch(viewer, args);

            codeClock.Stop();
            Platform.Log(LogLevel.Debug, string.Format("TTFI: {0}", codeClock));

            return viewer;
        }

        private static ImageViewerComponent CreateViewer(bool loadPriors)
        {
            if (loadPriors)
                return new ImageViewerComponent(LayoutManagerCreationParameters.Extended);
            else
                return new ImageViewerComponent(LayoutManagerCreationParameters.Extended, PriorStudyFinder.Null);
        }

        private static bool AnySopsLoaded(IImageViewer imageViewer)
        {
            return CollectionUtils.Contains(imageViewer.StudyTree.Patients, p => CollectionUtils.Contains(p.Studies, d => CollectionUtils.Contains(d.Series, r => r.Sops.Count > 0)));
        }
    }
}