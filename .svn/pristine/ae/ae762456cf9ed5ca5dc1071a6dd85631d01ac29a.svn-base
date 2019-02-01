using System;
using System.Windows.Forms;

namespace MprToolTestApp
{
    public partial class 选择原始序列 : Form
    {
        public 选择原始序列()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem == null)
            {
                MessageBox.Show("请选择需要进行MPR操作的序列");
                return;
            }
            this.Tag = listBox1.SelectedIndex.ToString();
            this.Close();
        }
    }
}
