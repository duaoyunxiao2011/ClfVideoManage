using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;

namespace ICameraDll
{
    public interface ICamera
    {
        /// <summary>
        /// 日志文件路径
        /// </summary>
        string LogFilePath { get; set; }

        /// <summary>
        /// 日志文件名
        /// </summary>
        string LogFileName { get; set; }

        /// <summary>
        /// 摄像头开始录制视频
        /// </summary>
        /// <param name="VideoContro">展现控件</param>
        /// <param name="Path">视频存储路径</param>
        /// <param name="FileName">视频存储文件名</param>
        /// <returns>录像开启成功：1， 录像开启失败：0，系统程序出错：-1，ffshow视频解码器不存在:2，视像头录像正在录制:3</returns>
        int StartRecording(Control videoControl, string filePath, string fileName);

        /// <summary>
        /// 拍摄照片
        /// </summary>
        /// <param name="FilePath">照片存储路径</param>
        /// <param name="FileName">照片存储文件名</param>
        /// <returns>保存图片成功：1 保存图片失败：0，系统程序程序错误：-1</returns>
        int Photograph(string filePath, string fileName);

      

        /// <summary>
        /// 暂停录像
        /// </summary>
        /// <returns>暂停成功：1 暂停失败：0<</returns>
        int PauseRecord();

        /// <summary>
        /// 继续录像
        /// </summary>
        /// <returns>继续成功：1 继续失败：0</returns>
        int Continue();

        /// <summary>
        /// 停止录像
        /// </summary>
        /// <returns>停止录像成功：1 停止录像失败：0</returns>
        int StopRecord();

    }
}
