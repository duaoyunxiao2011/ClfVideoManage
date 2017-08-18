using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using ICameraDll.DirectX.Capture;
namespace ICameraDll
{
    /// <summary>
    /// 摄像头后台操作
    /// </summary>
    public class CameraManage
    {
        private Filters filters = new Filters();
        private string LogFilePath { get; set; } //日志文件路径
        private string LogFileName { get; set; }//日志文件路径
        public CameraManage(string LogFilePath, string LogFileName)
        {
            this.LogFilePath = LogFilePath;
            this.LogFileName = LogFileName;
        }

        #region 获取视频编码器集合
        /// <summary>
        /// 获取视频编码器集合
        /// </summary>
        /// <returns>Filter 集合</returns>
        public Filter[] GetCompressorFilter()
        {
            Filter[] filterArray = new Filter[this.filters.VideoCompressors.Count];
            for (var i = 0; i < filterArray.Length; i++)
            {
                filterArray[i] = this.filters.VideoCompressors[i];
            }
            return filterArray;
        }
        #endregion

        #region 获取视频输入设备集合(摄像头)
        /// <summary>
        /// 获取视频输入设备集合(摄像头)
        /// </summary>
        /// <returns>Filter 集合</returns>
        public Filter[] GetDriversFilter()
        {
            Filter[] filterArray = new Filter[this.filters.VideoInputDevices.Count];
            for (var i = 0; i < filterArray.Length; i++)
            {
                filterArray[i] = this.filters.VideoInputDevices[i];
            }
            return filterArray;
        }
        #endregion

        #region 获取ffdshow video encoder视频编码器索引
        /// <summary>
        /// 获取ffdshow video encoder视频编码器索引   视频解码器
        /// </summary>
        /// <returns>存在返回索引，否则返回-1</returns>
        public int GetffshowIndex()
        {
            FilterCollection videoCompressors = this.filters.VideoCompressors;
            for (var i = 0; i < videoCompressors.Count; i++)
            {
                if ((videoCompressors[i] != null) && videoCompressors[i].Name.Equals("ffdshow video encoder"))
                {
                    return i;
                }
            }
            return -1;
        }
        #endregion

        #region 创建文件路径
        /// <summary>
        /// 创建文件路径
        /// </summary>
        /// <param name="FilePath">文件路径</param>
        public void CreatFile(string FilePath)
        {
            var dir = FilePath.Remove(FilePath.LastIndexOf("\\\\"));
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }
        #endregion

        #region 记录错误日志
        /// <summary>
        /// 记录错误日志
        /// </summary>
        /// <param name="content">错误内容</param>
        public void RecordErrorLog(string content)
        {
            var LogFlie = LogFilePath + LogFileName;
            if (!string.IsNullOrEmpty(LogFlie))
            {
                CreatFile(LogFlie);
                File.AppendAllText(LogFlie, DateTime.Now.ToString() + ",Error:" + content + "\r\n");
            }
        }
        #endregion
    }
}
