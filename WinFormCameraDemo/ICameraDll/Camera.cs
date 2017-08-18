using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using ICameraDll.DirectX.Capture;
namespace ICameraDll
{
    public  class Camera : ICamera
    {
        private Capture capture;//摄像头录像操作
        private Filters filters = new Filters();//Filter集合
        public string stauts = "NoThing";//当前状态，默认

        #region 属性

        private string logFilePath;
        private string logFileName;
        /// <summary>
        /// 日志文件路径
        /// </summary>
        public string LogFilePath
        {
            get
            {
                return logFilePath;
            }
            set
            {
                logFilePath = value;
            }
        }
        /// <summary>
        /// 日志文件名
        /// </summary>
        public string LogFileName
        {
            get
            {
                return logFileName;
            }
            set
            {
                logFileName = value;
            }
        }
        private string ImageFilePath;
        private string ImageFileName;
        #endregion

        #region 摄像头开始录制视频
        /// <summary>
        /// 摄像头开始录制视频
        /// </summary>
        /// <param name="VideoContro">展现控件</param>
        /// <param name="Path">视频存储路径</param>
        /// <param name="FileName">视频存储文件名</param>
        /// <returns>录像开启成功：1 录像开启失败：0，系统程序：-1，ffshow视频解码器不存在:2,视像头录像正在录制:3</returns>
        public int StartRecording(Control videoControl, string filePath, string fileName)
        {
            var state = 0;
            //开始录制前判断摄像头是否在进行录像工作，工作时关掉它
            if (this.capture != null)
            {
                this.capture.Stop();
                this.capture.DisposeCapture();
                this.stauts = "NoThing";
            }
            CameraManage cameraManage = new CameraManage(logFilePath, logFileName);
            //当前不为录像状态时则开始录像
            if (!this.stauts.Equals("Recing"))
            {
                //获取ffshow视频解码器索引
                var ffshowIndex = cameraManage.GetffshowIndex();
                if (ffshowIndex > 0)//解码器1判断，ffshowIndex
                {
                    try
                    {
                        var Flie = filePath + fileName;
                        cameraManage.CreatFile(Flie);
                        this.capture = new Capture(new Filters().VideoInputDevices[0], null);//实例化视像头对象
                        this.capture.PreviewWindow = videoControl;//设置承载控件
                        this.capture.VideoCompressor = this.filters.VideoCompressors[ffshowIndex];//设置视频解码器
                        this.capture.Filename = Flie;//设置要保存的文件路径和文件名，格式例如d:\\ssss.avi
                        this.capture.FrameRate = 15;//设置帧
                        
                        this.capture.FrameSize = new Size(320, 240);//设置视频分辨率
                        this.capture.Start();//开启录制
                        
                        //封面cover
                        this.stauts = "Recing";
                        //拍照事件
                        Capture.FrameCapHandler f = new Capture.FrameCapHandler(GetNewImage);
                        this.capture.FrameCaptureComplete += new DirectX.Capture.Capture.FrameCapHandler(f.Invoke);
                       
                        state = 1;
                    }
                    catch (Exception ex)
                    {
                        cameraManage.RecordErrorLog(ex.Message + "当前状态:" + this.stauts);
                        this.stauts = "Error";
                        StopRecord();
                        state = -1;
                    }
                }
                else
                {
                    cameraManage.RecordErrorLog("ffshow视频解码器不存在，没安装,当前状态：" + this.stauts);
                    state = 2;

                }
            }
            else
            {
                cameraManage.RecordErrorLog("视像头录像正在录制,无法调用。当前状态：" + this.stauts);
                state = 3;
            }
            return state;

        }
        #endregion

        #region 拍照保存图片
        /// <summary>
        /// 保存图片
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="fileName"></param>
        /// <returns>保存图片成功：1 保存图片失败：0，系统程序程序错误：-1</returns>
        public int Photograph(string filePath, string fileName)
        {
            ImageFilePath = filePath;
            ImageFileName = fileName;
            CameraManage cameraManage = new CameraManage(ImageFilePath, ImageFileName);
            var state = 0;
            if (this.capture == null)
            {

                PictureBox pic = new PictureBox();
                try
                {
                    Capture.FrameCapHandler f = new Capture.FrameCapHandler(GetNewImage);
                    this.capture = new Capture(new Filters().VideoInputDevices[0], null);//实例化视像头对象
                    this.capture.PreviewWindow = pic;
                    this.capture.FrameRate = 15;//设置帧
                    this.capture.FrameSize = new Size(320, 240);//设置分辨率
                    this.capture.RenderPreview();
                    this.capture.FrameCaptureComplete += new DirectX.Capture.Capture.FrameCapHandler(f.Invoke);
                    this.stauts = "Viewing";
                }
                catch (Exception ex)
                {

                    cameraManage.RecordErrorLog(ex.Message + "当前状态:" + this.stauts);
                    this.stauts = "Error";
                    StopRecord();
                    state = -1;
                }
            }
            this.capture.CaptureFrame();
            state = 1;

            return state;
        }
        
        /// <summary>
        /// 获取当前帧图片
        /// </summary>
        /// <param name="image"></param>
        public void GetNewImage(Image image)
        {
            CameraManage cameraManage = new CameraManage(logFilePath, logFileName);
            var Flie = ImageFilePath + ImageFileName;
            cameraManage.CreatFile(Flie);
            image.Save(Flie, System.Drawing.Imaging.ImageFormat.Jpeg);
            if (this.capture != null && !this.stauts.Equals("Recing"))
            {
                this.capture.DisposeCapture();
                this.capture.Stop();
            }
        }
        #endregion

        #region 暂停录像
        /// <summary>
        /// 暂停录像
        /// </summary>
        /// <returns>暂停成功：1 暂停失败：0<</returns>
        public int PauseRecord()
        {
            var state = 0;
            if (this.stauts.Equals("Recing") && this.capture != null)
            {
                this.capture.Pause();
                this.stauts = "Pausing";
                state = 1;
            }
            return state;
        }
        #endregion

        #region 继续录制
        /// <summary>
        /// 继续录制
        /// </summary>
        /// <returns>继续成功：1 继续失败：0</returns>
        public int Continue()
        {
            var state = 0;
            if (this.stauts.Equals("Pausing") && this.capture != null)
            {
                this.capture.GoOn();
                this.stauts = "Recing";
                state = 1;
            }
            return state;
        }
        #endregion

        #region 停止录制
        /// <summary>
        /// 停止录制
        /// </summary>
        /// <returns>停止成功：1 停止失败：0</returns>
        public int StopRecord()
        {
            var state = 0;
            if ((this.capture != null) && !this.stauts.Equals("NoThing"))
            {
                this.capture.Stop();
                this.stauts = "NoThing";
                this.capture.DisposeCapture();
                this.capture = null;
                state = 1;
            }
            return state;
        }
        #endregion
    }
}
