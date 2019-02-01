using ClearCanvas.Dicom;
using ClearCanvas.Dicom.Iod;
using ClearCanvas.ImageViewer;
using ClearCanvas.ImageViewer.PresentationStates.Dicom;
using ClearCanvas.ImageViewer.StudyManagement;
using ClearCanvas.ImageViewer.Volume.Mpr;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MprTool
{
    /*
    创建日期：2019年1月17日
    创建人：李海川
    功能说明：
        1.根据提供的目录地址，加载其中的所有dicom文件。
        2.对dicom文件进行标准序列分组。
        3.提供标准序列的基本信息获取功能。
        4.执行Mpr序列创建。
        5.提供Mpr序列的基本信息获取功能。
        6.提供指定Dicom、Mpr中指定的Sop保存功能（Dicom文件，Bmp图像文件）。
        7.提供指定Dicom、Mpr中的指定序列整体保存到指定目录功能(Dicom文件,Bmp图像文件)。
    */
    public class MprTools
    {
        String _sourceDirPath;
        Exception _exception = null;
        IEnumerable<String> _dicomFilePaths = null;
        StudyTree _studyTree = null;
        MprVolume _mprVolume = null;
        IList<IMprSliceSet> _mprSliceSets = null;
        ImageViewerComponent _mviewer = null;
        ClearCanvas.ImageViewer.Volumes.Volume _volume = null;

        /// <summary>
        /// 获取执行过程中的错误信息，默认为null，表示没有错误。
        /// </summary>
        public Exception GetException
        {
            get
            {
                return _exception;
            }
        }

        /// <summary>
        /// 获取检查树
        /// </summary>
        public StudyTree GetStudyTree
        {
            get
            {
                return _studyTree;
            }
        }

        /// <summary>
        /// 获取Mpr序列集合对象。
        /// </summary>
        public IList<IMprSliceSet> GetMprSliceSet
        {
            get
            {
                return _mprSliceSets;
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="SourceDirPath">包含需要执行Mpr重建的dicom文件目录地址。</param>
        /// <param name="ExpName">需要加载文件的扩展名,默认为空，表示任何文件,格式：*.dcm *.dicom</param>
        public MprTools(String SourceDirPath, String ExpName = "*.*")
        {

            _sourceDirPath = SourceDirPath;
            _dicomFilePaths = System.IO.Directory.EnumerateFiles(_sourceDirPath, ExpName);
            //检查文件数量是否符合要求，如果执行mpr重建，原始序列图像数量应大于16。
            if (_dicomFilePaths.Count() < 16)
            {
                _exception = new Exception("原始序列中图像数量小于16，不能完成后续的mpr重建工作。");
            }
        }


        /// <summary>
        /// 创建检查数据树，对普通影像中的序列进行分组。
        /// </summary>
        /// <returns></returns>
        public StudyTree CreateStudyTree()
        {
            _mviewer = new ImageViewerComponent(); //准备打开文件的viewer对象
            try
            {
                foreach (string fs in _dicomFilePaths)
                {
                    _mviewer.StudyTree.AddSop(Sop.Create(fs));
                }
                _studyTree = _mviewer.StudyTree;
                return _studyTree;
            }
            catch (Exception ex)
            {
                _exception = ex;
                return null;
            }
        }

        List<Frame> frames = null;

        /// <summary>
        /// 创建检查MPR序列集
        /// </summary>
        /// <param name="SeriesNum"></param>
        /// <param name="StudyNum"></param>
        /// <returns></returns>
        public IList<IMprSliceSet> CreateMprSlices(Int32 SeriesNum, Int32 StudyNum = 0)
        {
            try
            {
                frames = new List<Frame>();
                foreach (ImageSop iss in _studyTree.Patients[0].Studies[StudyNum].Series[SeriesNum].Sops)
                {
                    frames.Add(iss.CreateFrame(1));
                }
                _volume = ClearCanvas.ImageViewer.Volumes.Volume.Create(frames);//创建普通卷
                _mprVolume = new MprVolume(_volume);//获取MPR卷
                _mprSliceSets = _mprVolume.SliceSets;
                return _mprVolume.SliceSets;
            }
            catch (Exception ex)
            {
                _exception = ex;
                return null;
            }
        }

        /// <summary>
        /// 获取一个Mpr序列中的Sop DicomFile对象。
        /// </summary>
        /// <param name="MprSliceIndex"></param>
        /// <returns></returns>
        public List<DicomFile> GetMprSliceSopDicomFiles(Int32 MprSliceIndex)
        {
            try
            {
                List<DicomFile> DF = new List<DicomFile>();
                foreach (ImageSop fs in _mprSliceSets[MprSliceIndex].SliceSops)
                {
                    //此代码为了对实现对DicomFile对象自动初始化，未找到手工初始化方法。
                    DicomSoftcopyPresentationState presentationState = DicomSoftcopyPresentationState.Load(fs.Frames[1]);//Frames 序号由1开始。
                    SupplyDicomFileImageTag(presentationState.DicomFile, fs.Frames[1]);
                    DF.Add(presentationState.DicomFile);
                }
                return DF;
            }
            catch (Exception ex)
            {
                _exception = ex;
                return null;
            }
        }



        /// <summary>
        /// 为指定的mprSlice 在指定的目录中创建dicom文件。
        /// </summary>
        /// <param name="MprSliceIndex">序列序号</param>
        /// <param name="DesDirPath">目标路径地址</param>
        /// <returns></returns>
        public bool CreateMprSliceSopDicomFiles(Int32 MprSliceIndex, String DesDirPath)
        {
            try
            {
                foreach (MprSliceSop MSS in _mprSliceSets[MprSliceIndex].SliceSops)
                {
                    //此代码为了对实现对DicomFile对象自动初始化，未找到手工初始化方法。
                    DicomSoftcopyPresentationState presentationState = DicomSoftcopyPresentationState.Load(MSS.Frames[1]);//Frames 序号由1开始。
                    SupplyDicomFileImageTag(presentationState.DicomFile, MSS.Frames[1]);
                    presentationState.DicomFile.Save(DesDirPath + "\\" + presentationState.DicomFile.MediaStorageSopInstanceUid + ".dcm");
                }
                return true;
            }
            catch (Exception ex)
            {
                _exception = ex;
                return false;
            }
        }


        /// <summary>
        /// 为指定的mprSlice 在指定的目录中创建 bmp 文件。
        /// </summary>
        /// <param name="MprSliceIndex">序列序号</param>
        /// <param name="DesDirPath">目标路径地址</param>
        /// <returns></returns>
        public bool CreateMprSliceSopDicomImage(Int32 MprSliceIndex, String DesDirPath)
        {
            try
            {
                foreach (ImageSop MSS in _mprSliceSets[MprSliceIndex].SliceSops)
                {
                    var frameReference = MSS.Frames[1].CreateTransientReference();
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
                    System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(gpi.ClientRectangle.Width, gpi.ClientRectangle.Height);
                    gpi.DrawToBitmap(bitmap);
                    bitmap.Save(DesDirPath + "\\" + frameReference.Frame.SopInstanceUid + ".bmp");
                    bitmap.Dispose();
                }
                return true;
            }
            catch (Exception ex)
            {
                _exception = ex;
                return false;
            }
        }


        /// <summary>
        /// 补充DicomFile对象中缺少影像数据元素
        /// </summary>
        /// <param name="DDF">需要补充元素的DicomFile对象</param>
        /// <param name="SF">包含影像元素的Frame对象</param>
        private void SupplyDicomFileImageTag(DicomFile DDF, Frame SF)
        {
            DDF.DataSet[0x280002] = SF[0x280002];
            DDF.DataSet[0x280004] = SF[0x280004];
            DDF.DataSet[0x280010] = SF[0x280010];
            DDF.DataSet[0x280011] = SF[0x280011];
            DDF.DataSet[0x280030] = SF[0x280030];
            DDF.DataSet[0x280100] = SF[0x280100];
            DDF.DataSet[0x280101] = SF[0x280101];
            DDF.DataSet[0x280102] = SF[0x280102];
            DDF.DataSet[0x280103] = SF[0x280103];
            DDF.DataSet[0x280106] = SF[0x280106];
            DDF.DataSet[0x280107] = SF[0x280107];
            DDF.DataSet[0x281050] = SF[0x281050];
            DDF.DataSet[0x281051] = SF[0x281051];
            DDF.DataSet[0x281052] = SF[0x281052];
            DDF.DataSet[0x281053] = SF[0x281053];
            DDF.DataSet[0x281054] = SF[0x281054];
            DDF.DataSet[0x281055] = SF[0x281055];
            DDF.DataSet[0x7fe00010] = SF[0x7fe00010];
            DDF.DataSet[0x7fe00010].Values = SF.GetNormalizedPixelData();
        }
    }
}
