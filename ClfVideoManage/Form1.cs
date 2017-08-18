using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace ClfVideoManage
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public cVideo video = null;

        private void Form1_Load(object sender, EventArgs e)
        {
            video = new cVideo(this.panel1.Handle, 640, 480);  
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (video.StartWebCam(320, 240))
            {
                video.get();
                video.Capparms.fYield = true;
                video.Capparms.fAbortLeftMouse = false;
                video.Capparms.fAbortRightMouse = false;
                video.Capparms.fCaptureAudio = false;
                video.Capparms.dwRequestMicroSecPerFrame = 0x9C40; // 设定帧率25fps： 1*1000000/25 = 0x9C40  
                video.set();
            }  
        }

        private void button2_Click(object sender, EventArgs e)
        {
            
        }

     
        private void button3_Click(object sender, EventArgs e)
        {
            video.StarKinescope(System.IO.Path.Combine(@"d:\\", "temp.avi"));
        }



        private void button4_Click(object sender, EventArgs e)
        {
            video.StopKinescope(); 
        }


        public void ys()
        {
            string file_name = @"d:/temp.avi";
            //string command_line = " -i " + file_name + " -vcodec libx264 -cqp 25 -y " + file_name.Replace(".avi", "_264") + ".avi";
            string command_line = " -i " + file_name + " -y -qscale 10 -s 640*480 -r 15 " + "f.flv";
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo.WorkingDirectory = Application.StartupPath;
            proc.StartInfo.UseShellExecute = false; //use false if you want to hide the window    
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.FileName = "ffmpeg";
            proc.StartInfo.Arguments = command_line;
            proc.Start();
            proc.WaitForExit();
            proc.Close();

            //// 删除原始avi文件    
            //FileInfo file = new FileInfo(file_name);
            //if (file.Exists)
            //{
            //    try
            //    {
            //        file.Delete(); //删除单个文件    
            //    }
            //    catch (Exception e)
            //    {
            //        //Common.writeLog("删除视频文件“" + file_name + "”出错！" + e.Message);
            //    }
            //}
        }

        private void button5_Click(object sender, EventArgs e)
        {
            ys();
        }
    }
}
