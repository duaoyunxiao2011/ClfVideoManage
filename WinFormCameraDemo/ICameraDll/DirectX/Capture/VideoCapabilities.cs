using System;
using System.Drawing;
using System.Runtime.InteropServices;
using DShowNET;

namespace ICameraDll.DirectX.Capture
{
    public class VideoCapabilities
    {
        public int FrameSizeGranularityX;
        public int FrameSizeGranularityY;
        public Size InputSize;
        public double MaxFrameRate;
        public Size MaxFrameSize;
        public double MinFrameRate;
        public Size MinFrameSize;

        internal VideoCapabilities(IAMStreamConfig videoStreamConfig)
        {
            if (videoStreamConfig == null)
            {
                throw new ArgumentNullException("videoStreamConfig");
            }
            AMMediaType mediaType = null;
            VideoStreamConfigCaps caps = null;
            IntPtr zero = IntPtr.Zero;
            try
            {
                IntPtr ptr2;
                int num;
                int num2;
                int numberOfCapabilities = videoStreamConfig.GetNumberOfCapabilities(out num, out num2);
                if (numberOfCapabilities != 0)
                {
                    Marshal.ThrowExceptionForHR(numberOfCapabilities);
                }
                if (num <= 0)
                {
                    throw new NotSupportedException("This video device does not report capabilities.");
                }
                if (num2 > Marshal.SizeOf(typeof(VideoStreamConfigCaps)))
                {
                    throw new NotSupportedException("Unable to retrieve video device capabilities. This video device requires a larger VideoStreamConfigCaps structure.");
                }
                zero = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(VideoStreamConfigCaps)));
                numberOfCapabilities = videoStreamConfig.GetStreamCaps(0, out ptr2, zero);
                if (numberOfCapabilities != 0)
                {
                    Marshal.ThrowExceptionForHR(numberOfCapabilities);
                }
                mediaType = (AMMediaType) Marshal.PtrToStructure(ptr2, typeof(AMMediaType));
                caps = (VideoStreamConfigCaps) Marshal.PtrToStructure(zero, typeof(VideoStreamConfigCaps));
                this.InputSize = caps.InputSize;
                this.MinFrameSize = caps.MinOutputSize;
                this.MaxFrameSize = caps.MaxOutputSize;
                this.FrameSizeGranularityX = caps.OutputGranularityX;
                this.FrameSizeGranularityY = caps.OutputGranularityY;
                this.MinFrameRate = 10000000.0 / ((double) caps.MaxFrameInterval);
                this.MaxFrameRate = 10000000.0 / ((double) caps.MinFrameInterval);
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(zero);
                }
                zero = IntPtr.Zero;
                if (mediaType != null)
                {
                    DsUtils.FreeAMMediaType(mediaType);
                }
                mediaType = null;
            }
        }
    }
}

