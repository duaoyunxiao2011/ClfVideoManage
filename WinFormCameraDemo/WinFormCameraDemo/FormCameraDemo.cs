using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ICameraDll;
namespace WinFormCameraDemo
{
    public partial class FormCameraDemo : Form
    {
        public FormCameraDemo()
        {
            InitializeComponent();
        }
        ICamera  camera;
        public string fileName = "";
        /// <summary>
        /// 开始录像
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void but_Start_Click(object sender, EventArgs e)
        {
            //弹出框，命名文件名
            FmFileName form = new FmFileName();
            form.Owner = this;
            form.ShowDialog();
            string ad = DateTime.Now.TimeOfDay.ToString("hhmmss")+ "";
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = DateTime.Now.ToString("yyyyMMdd")+"-"+DateTime .Now .TimeOfDay.ToString ("hhmmss")+".avi";
              
            }
            else
            {
                fileName += ".avi";
            }

            camera = new Camera();
            var date = DateTime.Now.ToString("yyyy-MM-dd");
           
            //有名字，就不用这个了
            camera.LogFileName = "log1.txt";//log文件
            camera.LogFilePath = string.Format(@"d:\\Camera\\{0}\\Log\\", date);//日志和录像保存d盘camera文件里
            camera.StartRecording(picbox_Video, string.Format(@"d:\\Camera\\{0}\\Video\\", date),fileName);
        }

        /// <summary>
        /// 暂停录像
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void but_Pause_Click(object sender, EventArgs e)
        {
            if (but_Pause.Text.Equals("暂停录像"))
            {
                camera.PauseRecord();
                but_Pause.Text = "继续录像";
            }
            else
            {
                camera.Continue();
                but_Pause.Text = "暂停录像";
            }
        }
        /// <summary>
        /// 停止录像
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void but_Stop_Click(object sender, EventArgs e)
        {
            if (camera != null)
            {
                camera.StopRecord();
            }
            //停止播放的时候，拍照保存
            //var date = DateTime.Now.ToString("yyyy-MM-dd");
            //string filename = date + "~" + DateTime.Now.TimeOfDay.ToString("hhmmss");
            //if (camera == null)
            //{
            //    camera = new Camera();
            //}
            //camera.Photograph(string.Format(@"d:\\Camera\\{0}\\Image\\", date), filename + ".jpg");
        }

        /// <summary>
        /// 拍照
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void but_Photograph_Click(object sender, EventArgs e)
        {
            var date = DateTime.Now.ToString("yyyy-MM-dd");
            string filename = date+"~"+DateTime.Now.TimeOfDay.ToString("hhmmss");
            if (camera == null)
            {
                camera = new Camera();
            }
            camera.Photograph(string.Format(@"d:\\Camera\\{0}\\Image\\", date), filename+".jpg");


        }

        /// <summary>
        /// 退出程序
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void but_Exit_Click(object sender, EventArgs e)
        {
            if (camera != null)//如果不点停止录制，点击退出，就停止了，否则视频无法播放。
            {
                camera.StopRecord();
            }

            Application.Exit();//退出程序
        }

        private void btnVideo_Click(object sender, EventArgs e)
        {
            VideoPlayer form = new VideoPlayer();
            form.Show();
        }
    }
}
