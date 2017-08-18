namespace WinFormCameraDemo
{
    partial class FormCameraDemo
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.but_Start = new System.Windows.Forms.Button();
            this.but_Stop = new System.Windows.Forms.Button();
            this.but_Pause = new System.Windows.Forms.Button();
            this.but_Photograph = new System.Windows.Forms.Button();
            this.picbox_Video = new System.Windows.Forms.PictureBox();
            this.but_Exit = new System.Windows.Forms.Button();
            this.btnVideo = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.picbox_Video)).BeginInit();
            this.SuspendLayout();
            // 
            // but_Start
            // 
            this.but_Start.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.but_Start.Location = new System.Drawing.Point(348, 46);
            this.but_Start.Name = "but_Start";
            this.but_Start.Size = new System.Drawing.Size(75, 23);
            this.but_Start.TabIndex = 4;
            this.but_Start.Text = "开始录像";
            this.but_Start.UseVisualStyleBackColor = true;
            this.but_Start.Click += new System.EventHandler(this.but_Start_Click);
            // 
            // but_Stop
            // 
            this.but_Stop.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.but_Stop.Location = new System.Drawing.Point(348, 178);
            this.but_Stop.Name = "but_Stop";
            this.but_Stop.Size = new System.Drawing.Size(75, 23);
            this.but_Stop.TabIndex = 6;
            this.but_Stop.Text = "停止录像";
            this.but_Stop.UseVisualStyleBackColor = true;
            this.but_Stop.Click += new System.EventHandler(this.but_Stop_Click);
            // 
            // but_Pause
            // 
            this.but_Pause.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.but_Pause.Location = new System.Drawing.Point(348, 129);
            this.but_Pause.Name = "but_Pause";
            this.but_Pause.Size = new System.Drawing.Size(75, 23);
            this.but_Pause.TabIndex = 7;
            this.but_Pause.Text = "暂停录像";
            this.but_Pause.UseVisualStyleBackColor = true;
            this.but_Pause.Click += new System.EventHandler(this.but_Pause_Click);
            // 
            // but_Photograph
            // 
            this.but_Photograph.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.but_Photograph.Location = new System.Drawing.Point(348, 86);
            this.but_Photograph.Name = "but_Photograph";
            this.but_Photograph.Size = new System.Drawing.Size(75, 23);
            this.but_Photograph.TabIndex = 8;
            this.but_Photograph.Text = "拍摄照片";
            this.but_Photograph.UseVisualStyleBackColor = true;
            this.but_Photograph.Click += new System.EventHandler(this.but_Photograph_Click);
            // 
            // picbox_Video
            // 
            this.picbox_Video.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.picbox_Video.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picbox_Video.Location = new System.Drawing.Point(12, 12);
            this.picbox_Video.Name = "picbox_Video";
            this.picbox_Video.Size = new System.Drawing.Size(313, 351);
            this.picbox_Video.TabIndex = 9;
            this.picbox_Video.TabStop = false;
            // 
            // but_Exit
            // 
            this.but_Exit.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.but_Exit.Location = new System.Drawing.Point(348, 274);
            this.but_Exit.Name = "but_Exit";
            this.but_Exit.Size = new System.Drawing.Size(75, 23);
            this.but_Exit.TabIndex = 10;
            this.but_Exit.Text = "退出程序";
            this.but_Exit.UseVisualStyleBackColor = true;
            this.but_Exit.Click += new System.EventHandler(this.but_Exit_Click);
            // 
            // btnVideo
            // 
            this.btnVideo.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnVideo.Location = new System.Drawing.Point(348, 224);
            this.btnVideo.Name = "btnVideo";
            this.btnVideo.Size = new System.Drawing.Size(75, 23);
            this.btnVideo.TabIndex = 11;
            this.btnVideo.Text = "播放视频";
            this.btnVideo.UseVisualStyleBackColor = true;
            this.btnVideo.Click += new System.EventHandler(this.btnVideo_Click);
            // 
            // FormCameraDemo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(464, 375);
            this.Controls.Add(this.btnVideo);
            this.Controls.Add(this.but_Exit);
            this.Controls.Add(this.picbox_Video);
            this.Controls.Add(this.but_Photograph);
            this.Controls.Add(this.but_Pause);
            this.Controls.Add(this.but_Stop);
            this.Controls.Add(this.but_Start);
            this.Name = "FormCameraDemo";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "CameraDemo";
            ((System.ComponentModel.ISupportInitialize)(this.picbox_Video)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button but_Start;
        private System.Windows.Forms.Button but_Stop;
        private System.Windows.Forms.Button but_Pause;
        private System.Windows.Forms.Button but_Photograph;
        private System.Windows.Forms.PictureBox picbox_Video;
        private System.Windows.Forms.Button but_Exit;
        private System.Windows.Forms.Button btnVideo;
    }
}

