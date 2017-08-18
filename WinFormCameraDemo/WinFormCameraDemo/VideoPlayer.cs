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
    public partial class VideoPlayer : Form
    {
        public VideoPlayer()
        {
            InitializeComponent();
        }

        private void VideoPlayer_Load(object sender, EventArgs e)
        {
            this.axWindowsMediaPlayer1.stretchToFit = true;
            axWindowsMediaPlayer1.settings.mute = true;//静音
            this.axWindowsMediaPlayer1.settings.volume = 0;//音量设置
        }
        private void 打开ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.OpenFile();
        }
        public void OpenFile()
        {
            this.openFileDialog1.Title = "请选择要播放的媒体文件";
            this.openFileDialog1.Filter = "Media File(*.mpg,*.dat,*.avi,*.wmv,*.wav,*.mp3,*rm,*rmvb)|*.wav;*.mp3;*.mpg;*.dat;*.avi;*.wmv;*.rm;*.rmvb|All(*.*)|*.*";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                this.axWindowsMediaPlayer1.URL = openFileDialog1.FileName;
                this.toolStripStatusLabel1.Text = "当前正在播放：" + this.axWindowsMediaPlayer1.currentMedia.name;
                this.axWindowsMediaPlayer1.Ctlcontrols.play();
            }
        }

        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void 播放ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.axWindowsMediaPlayer1.Ctlcontrols.play();
        }

        private void 暂停ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.axWindowsMediaPlayer1.Ctlcontrols.pause();
        }

        private void 停止ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.axWindowsMediaPlayer1.Ctlcontrols.stop();
        }

        //private void 上一首ToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    this.axWindowsMediaPlayer1.Ctlcontrols.previous();
        //}

        //private void 下一首ToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    this.axWindowsMediaPlayer1.Ctlcontrols.next();
        //}

        private void 全屏播放ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.axWindowsMediaPlayer1.fullScreen = true;
        }

     
    }
}
