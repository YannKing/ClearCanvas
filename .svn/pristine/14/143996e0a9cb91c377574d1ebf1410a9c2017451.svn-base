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
using ClearCanvas.ImageViewer.StudyManagement;
using System;

namespace ClearCanvas.ImageViewer
{
    public partial class ImageViewerComponent
    {
        internal class LocalSopLoader : IDisposable
        {
            private readonly IImageViewer _viewer;

            private int _total;
            private int _failed;

            public LocalSopLoader(IImageViewer viewer)
            {
                _viewer = viewer;
            }

            public int Total
            {
                get { return _total; }
            }

            public int Failed
            {
                get { return _failed; }
            }

            public void Load(string[] files, IDesktopWindow desktop, out bool cancelled)
            {
                Platform.CheckForNullReference(files, "files");

                _total = 0;
                _failed = 0;

                bool userCancelled = false;

                if (desktop != null)
                {
                    BackgroundTask task = new BackgroundTask(
                     delegate (IBackgroundTaskContext context)
                     {
                         for (int i = 0; i < files.Length; i++)
                         {
                             LoadSop(files[i]);

                             int percentComplete = (int)(((float)(i + 1) / files.Length) * 100);
                             string message = String.Format(SR.MessageFormatOpeningImages, i, files.Length);

                             BackgroundTaskProgress progress = new BackgroundTaskProgress(percentComplete, message);
                             context.ReportProgress(progress);

                             if (context.CancelRequested)
                             {
                                 userCancelled = true;
                                 break;
                             }
                         }

                         context.Complete(null);

                     }, true);

                    ProgressDialog.Show(task, desktop, true, ProgressBarStyle.Blocks);
                    cancelled = userCancelled;
                }
                else
                {
                    foreach (string file in files)
                        LoadSop(file);

                    cancelled = false;
                }

                if (Failed > 0)
                    throw new LoadSopsException(Total, Failed);
            }

            private void LoadSop(string file)
            {
                try
                {
                    Sop sop = Sop.Create(file);
                    try
                    {
                        _viewer.StudyTree.AddSop(sop);
                    }
                    catch (SopValidationException)
                    {
                        sop.Dispose();
                        throw;
                    }
                }
                catch (Exception e)
                {
                    // Things that could go wrong in which an exception will be thrown:
                    // 1) file is not a valid DICOM image
                    // 2) file is a valid DICOM image, but its image parameters are invalid
                    // 3) file is a valid DICOM image, but we can't handle this type of DICOM image
                    //可能出错的事情会抛出异常：
                    // 1）文件不是有效的DICOM图像
                    // 2）file是有效的DICOM图像，但其图像参数无效
                    // 3）file是一个有效的DICOM图像，但我们无法处理这种类型的DICOM图像

                    _failed++;
                    Platform.Log(LogLevel.Error, e);
                }

                _total++;
            }

            #region IDisposable Members

            public void Dispose()
            {
                GC.SuppressFinalize(this);
            }

            #endregion
        }
    }
}