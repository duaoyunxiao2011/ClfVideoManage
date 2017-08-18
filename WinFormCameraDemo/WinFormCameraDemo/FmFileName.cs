using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WinFormCameraDemo
{
    public partial class FmFileName : Form
    {
        public FmFileName()
        {
            InitializeComponent();
        }


        private void FmFileName_Load(object sender, EventArgs e)
        {
            string date = DateTime.Now.ToString("yyyyMMdd") + "-" + DateTime.Now.TimeOfDay.ToString("hhmmss");
            txtFileName.Text = date;
            
        }
        private void btnOK_Click(object sender, EventArgs e)
        {
            FormCameraDemo frm1 = (FormCameraDemo)this.Owner; //注意 如果textBox1是放在panel1中的 则先找panel1 再找textBox1
            frm1.fileName = txtFileName.Text.Trim();
            this.Close();

        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtFileName.Text = "";
        }
    }
}
