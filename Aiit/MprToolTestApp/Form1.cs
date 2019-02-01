using ClearCanvas.ImageViewer.StudyManagement;
using ClearCanvas.ImageViewer.Volume.Mpr;
using System;
using System.Linq;
using System.Windows.Forms;

namespace MprToolTestApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        MprTool.MprTools MT = null;
        //打开目录中的dicom文件
        private void toolStripButton1_Click(object sender, EventArgs e)
        {

            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                MT = new MprTool.MprTools(fbd.SelectedPath);
                MT.CreateStudyTree();
                if (MT.GetStudyTree.Patients[0].Studies[0].Series.Count() > 1)
                {
                    选择原始序列 xzxlF = new 选择原始序列();
                    xzxlF.listBox1.Items.Clear();
                    Int32 index = 0;
                    foreach (Series series in MT.GetStudyTree.Patients[0].Studies[0].Series)
                    {
                        xzxlF.listBox1.Items.Add(index + "-" + series.Sops.Count() + "-" + series.SeriesDescription);
                        index++;
                    }
                    xzxlF.Tag = "";
                    xzxlF.ShowDialog();
                    if (xzxlF.Tag.ToString() != "")
                    {
                        MT.CreateMprSlices(Convert.ToInt32(xzxlF.Tag));
                        listBox1.Items.Clear();
                        foreach (MprSliceSet MSS in MT.GetMprSliceSet)
                        {
                            listBox1.Items.Add(listBox1.Items.Count + "-" + MSS.SliceSops.Count() + "-" + MSS.Description);
                        }
                    }
                }
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem == null)
            {
                MessageBox.Show("请选择可用mpr序列！");
                return;
            }
            listBox2.Items.Clear();
            foreach (MprSliceSop msop in MT.GetMprSliceSet[listBox1.SelectedIndex].SliceSops)
            {
                listBox2.Items.Add(msop.SopInstanceUid);
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {

            if (listBox1.SelectedItem == null)
            {
                MessageBox.Show("请选择可用mpr序列！");
                return;
            }

            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                MT.CreateMprSliceSopDicomFiles(listBox1.SelectedIndex, fbd.SelectedPath);
            }
            MessageBox.Show("保存完成！");
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem == null)
            {
                MessageBox.Show("请选择可用mpr序列！");
                return;
            }

            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                MT.CreateMprSliceSopDicomImage(listBox1.SelectedIndex, fbd.SelectedPath);
            }
            MessageBox.Show("保存完成！");
        }
    }
}
