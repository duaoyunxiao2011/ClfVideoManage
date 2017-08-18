using System;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using DShowNET;

namespace ICameraDll.DirectX.Capture
{
    public class PropertyPageCollection : CollectionBase, IDisposable
    {
        internal PropertyPageCollection()
        {
            base.InnerList.Capacity = 1;
        }

        internal PropertyPageCollection(ICaptureGraphBuilder2 graphBuilder, IBaseFilter videoDeviceFilter, IBaseFilter audioDeviceFilter, IBaseFilter videoCompressorFilter, IBaseFilter audioCompressorFilter, SourceCollection videoSources, SourceCollection audioSources)
        {
            this.addFromGraph(graphBuilder, videoDeviceFilter, audioDeviceFilter, videoCompressorFilter, audioCompressorFilter, videoSources, audioSources);
        }

        protected void addFromGraph(ICaptureGraphBuilder2 graphBuilder, IBaseFilter videoDeviceFilter, IBaseFilter audioDeviceFilter, IBaseFilter videoCompressorFilter, IBaseFilter audioCompressorFilter, SourceCollection videoSources, SourceCollection audioSources)
        {
            object ppint = null;
            Trace.Assert(graphBuilder != null);
            this.addIfSupported(videoDeviceFilter, "Video Capture Device");
            Guid capture = PinCategory.Capture;
            Guid interleaved = MediaType.Interleaved;
            Guid gUID = typeof(IAMStreamConfig).GUID;
            if (graphBuilder.FindInterface(ref capture, ref interleaved, videoDeviceFilter, ref gUID, out ppint) != 0)
            {
                interleaved = MediaType.Video;
                if (graphBuilder.FindInterface(ref capture, ref interleaved, videoDeviceFilter, ref gUID, out ppint) != 0)
                {
                    ppint = null;
                }
            }
            this.addIfSupported(ppint, "Video Capture Pin");
            capture = PinCategory.Preview;
            interleaved = MediaType.Interleaved;
            gUID = typeof(IAMStreamConfig).GUID;
            if (graphBuilder.FindInterface(ref capture, ref interleaved, videoDeviceFilter, ref gUID, out ppint) != 0)
            {
                interleaved = MediaType.Video;
                if (graphBuilder.FindInterface(ref capture, ref interleaved, videoDeviceFilter, ref gUID, out ppint) != 0)
                {
                    ppint = null;
                }
            }
            this.addIfSupported(ppint, "Video Preview Pin");
            ArrayList list = new ArrayList();
            int num = 1;
            for (int i = 0; i < videoSources.Count; i++)
            {
                CrossbarSource source = videoSources[i] as CrossbarSource;
                if ((source != null) && (list.IndexOf(source.Crossbar) < 0))
                {
                    list.Add(source.Crossbar);
                    if (this.addIfSupported(source.Crossbar, "Video Crossbar " + ((num == 1) ? "" : num.ToString())))
                    {
                        num++;
                    }
                }
            }
            list.Clear();
            this.addIfSupported(videoCompressorFilter, "Video Compressor");
            capture = PinCategory.Capture;
            interleaved = MediaType.Interleaved;
            gUID = typeof(IAMTVTuner).GUID;
            if (graphBuilder.FindInterface(ref capture, ref interleaved, videoDeviceFilter, ref gUID, out ppint) != 0)
            {
                interleaved = MediaType.Video;
                if (graphBuilder.FindInterface(ref capture, ref interleaved, videoDeviceFilter, ref gUID, out ppint) != 0)
                {
                    ppint = null;
                }
            }
            this.addIfSupported(ppint, "TV Tuner");
            IAMVfwCompressDialogs compressDialogs = videoCompressorFilter as IAMVfwCompressDialogs;
            if (compressDialogs != null)
            {
                VfwCompressorPropertyPage page = new VfwCompressorPropertyPage("Video Compressor", compressDialogs);
                base.InnerList.Add(page);
            }
            this.addIfSupported(audioDeviceFilter, "Audio Capture Device");
            capture = PinCategory.Capture;
            interleaved = MediaType.Audio;
            gUID = typeof(IAMStreamConfig).GUID;
            if (graphBuilder.FindInterface(ref capture, ref interleaved, audioDeviceFilter, ref gUID, out ppint) != 0)
            {
                ppint = null;
            }
            this.addIfSupported(ppint, "Audio Capture Pin");
            capture = PinCategory.Preview;
            interleaved = MediaType.Audio;
            gUID = typeof(IAMStreamConfig).GUID;
            if (graphBuilder.FindInterface(ref capture, ref interleaved, audioDeviceFilter, ref gUID, out ppint) != 0)
            {
                ppint = null;
            }
            this.addIfSupported(ppint, "Audio Preview Pin");
            num = 1;
            for (int j = 0; j < audioSources.Count; j++)
            {
                CrossbarSource source2 = audioSources[j] as CrossbarSource;
                if ((source2 != null) && (list.IndexOf(source2.Crossbar) < 0))
                {
                    list.Add(source2.Crossbar);
                    if (this.addIfSupported(source2.Crossbar, "Audio Crossbar " + ((num == 1) ? "" : num.ToString())))
                    {
                        num++;
                    }
                }
            }
            list.Clear();
            this.addIfSupported(audioCompressorFilter, "Audio Compressor");
        }

        protected bool addIfSupported(object o, string name)
        {
            ISpecifyPropertyPages specifyPropertyPages = null;
            DsCAUUID pPages = new DsCAUUID();
            bool flag = false;
            try
            {
                specifyPropertyPages = o as ISpecifyPropertyPages;
                if ((specifyPropertyPages != null) && ((specifyPropertyPages.GetPages(out pPages) != 0) || (pPages.cElems <= 0)))
                {
                    specifyPropertyPages = null;
                }
            }
            finally
            {
                if (pPages.pElems != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(pPages.pElems);
                }
            }
            if (specifyPropertyPages != null)
            {
                DirectShowPropertyPage page = new DirectShowPropertyPage(name, specifyPropertyPages);
                base.InnerList.Add(page);
                flag = true;
            }
            return flag;
        }

        public void Clear()
        {
            for (int i = 0; i < base.InnerList.Count; i++)
            {
                this[i].Dispose();
            }
            base.InnerList.Clear();
        }

        public void Dispose()
        {
            this.Clear();
            base.InnerList.Capacity = 1;
        }

        ~PropertyPageCollection()
        {
            this.Dispose();
        }

        public PropertyPage this[int index]
        {
            get
            {
                return (PropertyPage) base.InnerList[index];
            }
        }
    }
}

