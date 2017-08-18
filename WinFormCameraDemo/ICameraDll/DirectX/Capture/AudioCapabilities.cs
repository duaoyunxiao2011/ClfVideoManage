using DShowNET;
using System;
using System.Runtime.InteropServices;

namespace ICameraDll.DirectX.Capture
{
    public class AudioCapabilities
    {
        public int ChannelsGranularity;
        public int MaximumChannels;
        public int MaximumSampleSize;
        public int MaximumSamplingRate;
        public int MinimumChannels;
        public int MinimumSampleSize;
        public int MinimumSamplingRate;
        public int SampleSizeGranularity;
        public int SamplingRateGranularity;

        internal AudioCapabilities(IAMStreamConfig audioStreamConfig)
        {
            if (audioStreamConfig == null)
            {
                throw new ArgumentNullException("audioStreamConfig");
            }
            AMMediaType mediaType = null;
            AudioStreamConfigCaps caps = null;
            IntPtr zero = IntPtr.Zero;
            try
            {
                IntPtr ptr2;
                int num;
                int num2;
                int numberOfCapabilities = audioStreamConfig.GetNumberOfCapabilities(out num, out num2);
                if (numberOfCapabilities != 0)
                {
                    Marshal.ThrowExceptionForHR(numberOfCapabilities);
                }
                if (num <= 0)
                {
                    throw new NotSupportedException("This audio device does not report capabilities.");
                }
                if (num2 > Marshal.SizeOf(typeof(AudioStreamConfigCaps)))
                {
                    throw new NotSupportedException("Unable to retrieve audio device capabilities. This audio device requires a larger AudioStreamConfigCaps structure.");
                }
                zero = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(AudioStreamConfigCaps)));
                numberOfCapabilities = audioStreamConfig.GetStreamCaps(0, out ptr2, zero);
                if (numberOfCapabilities != 0)
                {
                    Marshal.ThrowExceptionForHR(numberOfCapabilities);
                }
                mediaType = (AMMediaType) Marshal.PtrToStructure(ptr2, typeof(AMMediaType));
                caps = (AudioStreamConfigCaps) Marshal.PtrToStructure(zero, typeof(AudioStreamConfigCaps));
                this.MinimumChannels = caps.MinimumChannels;
                this.MaximumChannels = caps.MaximumChannels;
                this.ChannelsGranularity = caps.ChannelsGranularity;
                this.MinimumSampleSize = caps.MinimumBitsPerSample;
                this.MaximumSampleSize = caps.MaximumBitsPerSample;
                this.SampleSizeGranularity = caps.BitsPerSampleGranularity;
                this.MinimumSamplingRate = caps.MinimumSampleFrequency;
                this.MaximumSamplingRate = caps.MaximumSampleFrequency;
                this.SamplingRateGranularity = caps.SampleFrequencyGranularity;
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

