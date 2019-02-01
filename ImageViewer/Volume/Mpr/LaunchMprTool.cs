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
using System.Collections.Generic;
using System.Threading;
using ClearCanvas.Common;
using ClearCanvas.Common.Utilities;
using ClearCanvas.Desktop;
using ClearCanvas.Desktop.Actions;
using ClearCanvas.Dicom.Iod;
using ClearCanvas.ImageViewer.BaseTools;
using ClearCanvas.ImageViewer.Configuration;
using ClearCanvas.ImageViewer.StudyManagement;
using ClearCanvas.ImageViewer.Volume.Mpr.Utilities;
using ClearCanvas.ImageViewer.Volumes;
using NewLife.Agent;
using NewLife.Log;
using ClearCanvas.ImageViewer.Volume.Mpr;
using ClearCanvas.ImageViewer.PresentationStates.Dicom;
using ClearCanvas.Dicom;
using ClearCanvas.ImageViewer.Graphics;
using ClearCanvas.Dicom.Iod.Iods;

namespace ClearCanvas.ImageViewer.Volume.Mpr
{
    [ButtonAction("open", "global-toolbars/ToolbarMpr/ToolbarOpenSelectionWithMpr", "LaunchMpr")]
    [MenuAction("open", "imageviewer-contextmenu/MenuOpenWithMpr", "LaunchMpr")]
    [MenuAction("open", "global-menus/MenuTools/MenuMpr/MenuOpenSelectionWithMpr", "LaunchMpr")]
    [IconSet("open", "Icons.LaunchMprToolSmall.png", "Icons.LaunchMprToolMedium.png", "Icons.LaunchMprToolLarge.png")]

    [MenuAction("open1", "imageviewer-contextmenu/Menu测试MPR", "LaunchMprTest")]
    [MenuAction("open1", "global-menus/MenuTools/MenuMpr/Menu测试MPR", "LaunchMprTest")]

    [EnabledStateObserver("open", "Enabled", "EnabledChanged")]
    [VisibleStateObserver("open", "Visible", null)]
    [ViewerActionPermission("open", AuthorityTokens.ViewerClinical)]
    [GroupHint("open", "Tools.Volume.MPR")]
    //
    [ExtensionOf(typeof(ImageViewerToolExtensionPoint))]
    public class LaunchMprTool : ImageViewerTool
    {
        private MprViewerComponent _viewer;

        public bool Visible { get; private set; }

        public override void Initialize()
        {
            base.Initialize();

            Visible = !(ImageViewer is MprViewerComponent);

            Context.Viewer.EventBroker.ImageBoxSelected += OnImageBoxSelected;
            Context.Viewer.EventBroker.DisplaySetSelected += OnDisplaySetSelected;
        }

        protected override void Dispose(bool disposing)
        {
            Context.Viewer.EventBroker.ImageBoxSelected -= OnImageBoxSelected;
            Context.Viewer.EventBroker.DisplaySetSelected -= OnDisplaySetSelected;

            base.Dispose(disposing);
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
            }
        }

        //测试独立创建MPR方法
        public void LaunchMprTest()
        {
            DicomFile DF = new DicomFile();
            DF.Load("d:\\000000EC");

            DF.Save("d:\\test.dcm");

            ImageViewerComponent _mviewer = new ImageViewerComponent(); //准备打开文件的viewer对象
            //准备打开文件
            System.Windows.Forms.OpenFileDialog OFD = new System.Windows.Forms.OpenFileDialog();
            OFD.Title = "打开dicom文件";
            OFD.Filter = "*.dcm|dicom文件|*.*|所有文件";
            OFD.Multiselect = true;
            try
            {
                if (OFD.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    foreach (String fs in OFD.FileNames)
                    {
                        _mviewer.StudyTree.AddSop(ImageSop.Create(fs));
                    }
                }
            }
            catch (Exception ex)
            {
                XTrace.WriteLine(ex.Message + "\n\r" + ex.StackTrace);
            }


            //执行任务以创建MPR组件。 报告异常（通过任务抛出或传递），但必须处理任何创建的组件
            try
            {
                List<Frame> frames = new List<Frame>();
                foreach (ImageSop iss in _mviewer.StudyTree.Patients[0].Studies[0].Series[4].Sops)
                {
                    frames.Add(iss.CreateFrame(1));
                }
                Volumes.Volume volume = Volumes.Volume.Create(frames);
                //_viewer = new MprViewerComponent(volume);
                //_viewer.Layout();
                MprVolume mprVolume = new MprVolume(volume);//获取MPR卷

                

                //创建MprSliceSet对象
                MprDisplaySet mds = new MprDisplaySet("测试", mprVolume.SliceSets[0]);

                IDicomPresentationImage testimage;
                testimage = new DicomGrayscalePresentationImage(mprVolume.SliceSets[0].SliceSops[0].Frames[1]);
                //testimage.DrawToBitmap()
                var frameReference= mprVolume.SliceSets[3].SliceSops[100].Frames[1].CreateTransientReference();
                //GrayscaleImageGraphic gig = new GrayscaleImageGraphic(frameReference.Frame.Rows,
                //   frameReference.Frame.Columns,
                //   frameReference.Frame.BitsAllocated,
                //   frameReference.Frame.BitsStored,
                //   frameReference.Frame.HighBit,
                //   frameReference.Frame.PixelRepresentation != 0,
                //   frameReference.Frame.PhotometricInterpretation == PhotometricInterpretation.Monochrome1,
                //   frameReference.Frame.RescaleSlope,
                //   frameReference.Frame.RescaleIntercept,
                //   frameReference.Frame.GetNormalizedPixelData);

                GrayscalePresentationImage gpi = new GrayscalePresentationImage(
                    frameReference.Frame.Rows,
                   frameReference.Frame.Columns,
                   frameReference.Frame.BitsAllocated,
                   frameReference.Frame.BitsStored,
                   frameReference.Frame.HighBit,
                   frameReference.Frame.PixelRepresentation != 0,
                   frameReference.Frame.PhotometricInterpretation == PhotometricInterpretation.Monochrome1,
                   frameReference.Frame.RescaleSlope,
                   frameReference.Frame.RescaleIntercept,
                   frameReference.Frame.NormalizedPixelSpacing.Column,
                   frameReference.Frame.NormalizedPixelSpacing.Row,
                   frameReference.Frame.PixelAspectRatio.Column,
                   frameReference.Frame.PixelAspectRatio.Row,
                   frameReference.Frame.GetNormalizedPixelData
                    );

                


                DicomFile dfs = new DicomFile();
                DicomAttributeCollection dac = new DicomAttributeCollection();

                
      
             
          


                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(mds.PresentationImages[0].ClientRectangle.Width, mds.PresentationImages[0].ClientRectangle.Height);
                //testimage.DrawToBitmap();
                gpi.DrawToBitmap(bitmap);
                bitmap.Save("d:\\aaa.bmp");

                ImageSop iss1 = (ImageSop)_mviewer.StudyTree.Patients[0].Studies[0].Series[4].Sops[10];

                DicomSoftcopyPresentationState presentationState = DicomSoftcopyPresentationState.Load(mprVolume.SliceSets[0].SliceSops[0].Frames[1]);

               

               // DicomSoftcopyPresentationState presentationState = DicomSoftcopyPresentationState.Load(mprVolume.SliceSets[3].SliceSops[200].DataSource);
                presentationState.DicomFile.Save("d:\\aaaa.dcm");
                // DicomAttributeCollection dac = new DicomAttributeCollection();

              //  DicomFile df = new DicomFile("", presentationState.DicomFile.MetaInfo.Copy(), );


                //DicomFile dfs = new DicomFile("", CreateMetaInfo(dataSource), dataSource);
            }
            catch (Exception ex)
            {
                XTrace.WriteLine(ex.Message + "\n\r" + ex.StackTrace);
            }
            //LaunchImageViewerArgs args = new LaunchImageViewerArgs(ViewerLaunchSettings.WindowBehaviour);
            //args.Title = _viewer.Title;
            //ImageViewerComponent.Launch(_viewer, args);
        }


        public void LaunchMpr()
        {
            Exception exception = null;

            IPresentationImage currentImage = Context.Viewer.SelectedPresentationImage;
            if (currentImage == null)
                return;

            // gather the source frames which MPR will operate on. exceptions are reported.
            //收集MPR将要操作的源帧。 报告例外情况。
            BackgroundTaskParams @params;
            try
            {
                @params = new BackgroundTaskParams(FilterSourceFrames(currentImage.ParentDisplaySet, currentImage));
            }
            catch (Exception ex)
            {
                ReportException(ex);
                return;
            }

            // execute the task to create an MPR component. exceptions (either thrown or passed via task) are reported, but any created component must be disposed
            //执行任务以创建MPR组件。 报告异常（通过任务抛出或传递），但必须处理任何创建的组件
            BackgroundTask task = new BackgroundTask(LoadVolume, true, @params);
            task.Terminated += (sender, e) => exception = e.Exception;

            try
            {
                ProgressDialog.Show(task, Context.DesktopWindow);
                //task.Run();
            }
            catch (Exception ex)
            {
                if (_viewer != null)
                {
                    _viewer.Dispose();
                    _viewer = null;
                }

                ReportException(ex);
                return;
            }
            finally
            {
                task.Dispose();
            }

            if (exception != null)
            {
                ReportException(exception);
                return;
            }

            // launch the created MPR component as a workspace. any exceptions here are just reported.
            //将创建的MPR组件作为工作空间启动。 这里的任何例外都只是报道。
            try
            {
                LaunchImageViewerArgs args = new LaunchImageViewerArgs(ViewerLaunchSettings.WindowBehaviour);
                args.Title = _viewer.Title;
                ImageViewerComponent.Launch(_viewer, args);
            }
            catch (Exception ex)
            {
                ReportException(ex);
            }
            finally
            {
                _viewer = null;
            }
        }

        private void Task_Terminated(object sender, BackgroundTaskTerminatedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void ReportException(Exception ex)
        {
            ExceptionHandler.Report(ex, SR.ExceptionMprLoadFailure, Context.DesktopWindow);
        }

        private static IEnumerable<Frame> FilterSourceFrames(IDisplaySet displaySet, IPresentationImage currentImage)
        {
            // this method tries to filter the source display set based on the currently selected image before passing it to MPR
            // we need this because sometimes MPR-able content is found in a series concatenated with other frames (e.g. 3-plane loc)
            //此方法尝试在将源显示集传递给MPR之前根据当前选定的图像过滤源显示集
            //我们需要这个，因为有时可以在与其他帧（例如3平面loc）连接的系列中找到可以MPR的内容
            if (currentImage is IImageSopProvider)
            {
                Frame currentFrame = ((IImageSopProvider)currentImage).Frame;
                string studyInstanceUid = currentFrame.StudyInstanceUid;
                string seriesInstanceUid = currentFrame.SeriesInstanceUid;
                string frameOfReferenceUid = currentFrame.FrameOfReferenceUid;
                ImageOrientationPatient imageOrientationPatient = currentFrame.ImageOrientationPatient;

                // 如果当前帧缺少任何匹配参数，那么它总是一个错误,检查UID、序列UID。
                if (string.IsNullOrEmpty(studyInstanceUid) || string.IsNullOrEmpty(seriesInstanceUid))
                    throw new NullSourceSeriesException();
                if (string.IsNullOrEmpty(frameOfReferenceUid))
                    throw new NullFrameOfReferenceException();
                if (imageOrientationPatient == null || imageOrientationPatient.IsNull)
                    throw new NullImageOrientationException();

                // 如果当前帧不是支持的像素格式，那么它总是一个错误
                if (currentFrame.BitsAllocated != 16)
                    throw new UnsupportedPixelFormatSourceImagesException();

                // perform a very basic filtering of the selected display set based on the currently selected image
                //基于当前选择的图像执行对所选显示组的非常基本的过滤
                var filteredFrames = new List<Frame>();
                foreach (IPresentationImage image in displaySet.PresentationImages)
                {
                    if (image == currentImage)
                    {
                        filteredFrames.Add(currentFrame);
                    }
                    else if (image is IImageSopProvider)
                    {
                        Frame frame = ((IImageSopProvider)image).Frame;
                        if (frame.StudyInstanceUid == studyInstanceUid
                            && frame.SeriesInstanceUid == seriesInstanceUid
                            && frame.FrameOfReferenceUid == frameOfReferenceUid
                            && !frame.ImageOrientationPatient.IsNull
                            && frame.ImageOrientationPatient.EqualsWithinTolerance(imageOrientationPatient, .01f))
                            filteredFrames.Add(frame);
                    }
                }

                // if we found at least 3 frames matching the current image, then return those to MPR
                //如果我们发现至少3帧与当前图像匹配，则将其返回到MPR
                if (filteredFrames.Count > 3)
                    return filteredFrames;

                // JY: #6164 - Error message not accurate for MPR with no location information
                // if we don't find 3 matching frames, then MPR fails on the minimum frames error
                // which masks the fact that there *were* enough frames, just not enough frames matching some aspect filter criteria
                // we don't know what was the specific failed criterion, so we'll just pass all frames unfiltered
                // this lets MPR decide what's wrong with the display set here and throw the correct exception
                // JY：＃6164  - 如果我们没有找到3个匹配的帧，错误信息对于没有位置信息的MPR不准确，
                //那么MPR在最小帧错误上失败，这掩盖了*有*足够帧的事实，
                //只是不够 匹配某些方面过滤条件的帧我们不知道具体的失败标准是什么，
                //所以我们只是传递未经过滤的所有帧，这样MPR就能决定这里的显示集有什么问题并抛出正确的异常

                return CollectionUtils.Map<IPresentationImage, Frame>(displaySet.PresentationImages, img => img is IImageSopProvider ? ((IImageSopProvider)img).Frame : null);
            }
            else
            {
                throw new UnsupportedSourceImagesException();
            }
        }

        private void LoadVolume(IBackgroundTaskContext context)
        {
            try
            {
                ProgressTask mainTask = new ProgressTask();
                mainTask.AddSubTask("BUILD", 90);
                mainTask.AddSubTask("LAYOUT", 10);

                context.ReportProgress(new BackgroundTaskProgress(mainTask.IntPercent, string.Format(SR.MessageInitializingMpr, mainTask.Progress)));

                BackgroundTaskParams @params = (BackgroundTaskParams)context.UserState;
                Volumes.Volume volume = Volumes.Volume.Create(@params.Frames,
                                                              delegate (int i, int count)
                                                                  {
                                                                      if (context.CancelRequested)
                                                                          throw new BackgroundTaskCancelledException();
                                                                      if (i == 0)
                                                                          mainTask["BUILD"].AddSubTask("", count);
                                                                      mainTask["BUILD"][""].Increment();
                                                                      string message = string.Format(SR.MessageBuildingMprVolumeProgress, mainTask.Progress, i + 1, count, mainTask["BUILD"].Progress);
                                                                      context.ReportProgress(new BackgroundTaskProgress(mainTask.IntPercent, message));
                                                                  });

                mainTask["BUILD"].MarkComplete();
                context.ReportProgress(new BackgroundTaskProgress(mainTask.IntPercent, string.Format(SR.MessagePerformingMprWorkspaceLayout, mainTask.Progress)));

                //call layout here b/c it could take a while
                @params.SynchronizationContext.Send(delegate
                                                        {
                                                            _viewer = new MprViewerComponent(volume);
                                                            _viewer.Layout();
                                                        }, null);

                mainTask["LAYOUT"].MarkComplete();
                context.ReportProgress(new BackgroundTaskProgress(mainTask.IntPercent, string.Format(SR.MessageDone, mainTask.Progress)));

                context.Complete();
            }
            catch (BackgroundTaskCancelledException)
            {
                context.Cancel();
            }
            catch (Exception ex)
            {
                context.Error(ex);
            }
        }

        private sealed class BackgroundTaskCancelledException : Exception { }

        private class BackgroundTaskParams
        {
            public readonly IEnumerable<Frame> Frames;
            public readonly SynchronizationContext SynchronizationContext;

            public BackgroundTaskParams(IEnumerable<Frame> frames)
            {
                Frames = frames;
                SynchronizationContext = SynchronizationContext.Current;
            }
        }

        protected override void OnPresentationImageSelected(object sender, PresentationImageSelectedEventArgs e)
        {
            if (e.SelectedPresentationImage != null)
                UpdateEnabled(e.SelectedPresentationImage.ParentDisplaySet);
            else
                UpdateEnabled(null);
        }

        private void OnImageBoxSelected(object sender, ImageBoxSelectedEventArgs e)
        {
            if (e.SelectedImageBox.DisplaySet == null)
                UpdateEnabled(null);
        }

        private void OnDisplaySetSelected(object sender, DisplaySetSelectedEventArgs e)
        {
            UpdateEnabled(e.SelectedDisplaySet);
        }

        private void UpdateEnabled(IDisplaySet selectedDisplaySet)
        {
            Enabled = selectedDisplaySet != null && selectedDisplaySet.PresentationImages.Count > 1;
        }
    }
}