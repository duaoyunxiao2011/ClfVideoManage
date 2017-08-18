using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using DShowNET;

namespace ICameraDll.DirectX.Capture
{
    public class Capture : Form, ISampleGrabberCB
    {
        protected AudioCapabilities audioCaps;
        protected Filter audioCompressor;
        protected IBaseFilter audioCompressorFilter;
        protected Filter audioDevice;
        protected IBaseFilter audioDeviceFilter;
        protected SourceCollection audioSources;
        protected IAMStreamConfig audioStreamConfig;
        protected IBaseFilter baseGrabFlt;
        protected int bufferedSize;
        protected bool captured = true;
        protected bool capturedFrame;
        protected ICaptureGraphBuilder2 captureGraphBuilder;
        protected string filename = "";
        protected IFileSinkFilter fileWriterFilter;
        protected bool firstFrame = true;
        protected IGraphBuilder graphBuilder;
        protected GraphState graphState;
        protected PictureBox ImageCaptured;
        protected Image image;
        protected bool isCaptureRendered;
        protected bool isPreviewRendered;
        protected IMediaControl mediaControl;
        private IMediaEventEx mediaEvt;
        protected IBaseFilter muxFilter;
        protected Control previewWindow;
        protected PropertyPageCollection propertyPages;
        protected bool renderStream;
        protected int rotCookie;
        protected ISampleGrabber sampGrabber;
        protected byte[] savedArray;
        protected Tuner tuner;
        protected VideoCapabilities videoCaps;
        protected Filter videoCompressor;
        protected IBaseFilter videoCompressorFilter;
        protected Filter videoDevice;
        protected IBaseFilter videoDeviceFilter;
        protected VideoInfoHeader videoInfoHeader;
        protected SourceCollection videoSources;
        protected IAMStreamConfig videoStreamConfig;
        protected IVideoWindow videoWindow;
        protected bool wantCaptureFrame;
        protected bool wantCaptureRendered;
        protected bool wantPreviewRendered;
        private const int WM_GRAPHNOTIFY = 0x8001;

        public event EventHandler CaptureComplete;

        public event FrameCapHandler FrameCaptureComplete;

        public Capture(Filter videoDevice, Filter audioDevice)
        {
            if ((videoDevice == null) && (audioDevice == null))
            {
                throw new ArgumentException("The videoDevice and/or the audioDevice parameter must be set to a valid Filter.\n");
            }
            this.videoDevice = videoDevice;
            this.audioDevice = audioDevice;
            this.Filename = this.getTempFilename();
            this.ImageCaptured = new PictureBox();
            this.createGraph();
        }

        protected void assertStopped()
        {
            if (!this.Stopped)
            {
                throw new InvalidOperationException("This operation not allowed while Capturing. Please Stop the current capture.");
            }
        }

        private void Capture_Load(object sender, EventArgs e)
        {
        }

        [STAThread]
        public void CaptureFrame()
        {
            int num;
            if (this.firstFrame)
            {
                this.assertStopped();
                this.renderStream = true;
                this.renderGraph();
                num = this.mediaControl.Run();
                if (num != 0)
                {
                    Marshal.ThrowExceptionForHR(num);
                }
                this.firstFrame = false;
            }
            this.captured = false;
            if (this.savedArray == null)
            {
                int imageSize = this.videoInfoHeader.BmiHeader.ImageSize;
                if ((imageSize < 0x3e8) || (imageSize > 0xf42400))
                {
                    return;
                }
                this.savedArray = new byte[imageSize + 0xfa00];
            }
            num = this.sampGrabber.SetCallback(this, 1);
        }

        protected void createGraph()
        {
            System.Type typeFromCLSID = null;
            object obj2 = null;
            if ((this.videoDevice == null) && (this.audioDevice == null))
            {
                throw new ArgumentException("The video and/or audio device have not been set. Please set one or both to valid capture devices.\n");
            }
            if (this.graphState < GraphState.Created)
            {
                object obj3;
                GC.Collect();
                this.graphBuilder = (IGraphBuilder) Activator.CreateInstance(System.Type.GetTypeFromCLSID(Clsid.FilterGraph, true));
                Guid clsid = Clsid.CaptureGraphBuilder2;
                Guid gUID = typeof(ICaptureGraphBuilder2).GUID;
                this.captureGraphBuilder = (ICaptureGraphBuilder2) DsBugWO.CreateDsInstance(ref clsid, ref gUID);
                typeFromCLSID = System.Type.GetTypeFromCLSID(Clsid.SampleGrabber, true);
                if (typeFromCLSID == null)
                {
                    throw new NotImplementedException("DirectShow SampleGrabber not installed/registered");
                }
                obj2 = Activator.CreateInstance(typeFromCLSID);
                this.sampGrabber = (ISampleGrabber) obj2;
                obj2 = null;
                int errorCode = this.captureGraphBuilder.SetFiltergraph(this.graphBuilder);
                if (errorCode < 0)
                {
                    Marshal.ThrowExceptionForHR(errorCode);
                }
                AMMediaType pmt = new AMMediaType();
                pmt.majorType = MediaType.Video;
                pmt.subType = MediaSubType.RGB24;
                pmt.formatType = FormatType.VideoInfo;
                errorCode = this.sampGrabber.SetMediaType(pmt);
                if (errorCode < 0)
                {
                    Marshal.ThrowExceptionForHR(errorCode);
                }
                if (this.VideoDevice != null)
                {
                    this.videoDeviceFilter = (IBaseFilter) Marshal.BindToMoniker(this.VideoDevice.MonikerString);
                    errorCode = this.graphBuilder.AddFilter(this.videoDeviceFilter, "Video Capture Device");
                    if (errorCode < 0)
                    {
                        Marshal.ThrowExceptionForHR(errorCode);
                    }
                    this.mediaEvt = (IMediaEventEx) this.graphBuilder;
                    this.baseGrabFlt = (IBaseFilter) this.sampGrabber;
                    errorCode = this.graphBuilder.AddFilter(this.baseGrabFlt, "DS.NET Grabber");
                    if (errorCode < 0)
                    {
                        Marshal.ThrowExceptionForHR(errorCode);
                    }
                }
                if (this.AudioDevice != null)
                {
                    this.audioDeviceFilter = (IBaseFilter) Marshal.BindToMoniker(this.AudioDevice.MonikerString);
                    errorCode = this.graphBuilder.AddFilter(this.audioDeviceFilter, "Audio Capture Device");
                    if (errorCode < 0)
                    {
                        Marshal.ThrowExceptionForHR(errorCode);
                    }
                }
                if (this.VideoCompressor != null)
                {
                    this.videoCompressorFilter = (IBaseFilter) Marshal.BindToMoniker(this.VideoCompressor.MonikerString);
                    errorCode = this.graphBuilder.AddFilter(this.videoCompressorFilter, "Video Compressor");
                    if (errorCode < 0)
                    {
                        Marshal.ThrowExceptionForHR(errorCode);
                    }
                }
                if (this.AudioCompressor != null)
                {
                    this.audioCompressorFilter = (IBaseFilter) Marshal.BindToMoniker(this.AudioCompressor.MonikerString);
                    errorCode = this.graphBuilder.AddFilter(this.audioCompressorFilter, "Audio Compressor");
                    if (errorCode < 0)
                    {
                        Marshal.ThrowExceptionForHR(errorCode);
                    }
                }
                Guid capture = PinCategory.Capture;
                Guid interleaved = MediaType.Interleaved;
                Guid riid = typeof(IAMStreamConfig).GUID;
                if (this.captureGraphBuilder.FindInterface(ref capture, ref interleaved, this.videoDeviceFilter, ref riid, out obj3) != 0)
                {
                    interleaved = MediaType.Video;
                    if (this.captureGraphBuilder.FindInterface(ref capture, ref interleaved, this.videoDeviceFilter, ref riid, out obj3) != 0)
                    {
                        obj3 = null;
                    }
                }
                this.videoStreamConfig = obj3 as IAMStreamConfig;
                obj3 = null;
                capture = PinCategory.Capture;
                interleaved = MediaType.Audio;
                riid = typeof(IAMStreamConfig).GUID;
                if (this.captureGraphBuilder.FindInterface(ref capture, ref interleaved, this.audioDeviceFilter, ref riid, out obj3) != 0)
                {
                    obj3 = null;
                }
                this.audioStreamConfig = obj3 as IAMStreamConfig;
                this.mediaControl = (IMediaControl) this.graphBuilder;
                if (this.videoSources != null)
                {
                    this.videoSources.Dispose();
                }
                this.videoSources = null;
                if (this.audioSources != null)
                {
                    this.audioSources.Dispose();
                }
                this.audioSources = null;
                if (this.propertyPages != null)
                {
                    this.propertyPages.Dispose();
                }
                this.propertyPages = null;
                this.videoCaps = null;
                this.audioCaps = null;
                obj3 = null;
                capture = PinCategory.Capture;
                interleaved = MediaType.Interleaved;
                riid = typeof(IAMTVTuner).GUID;
                if (this.captureGraphBuilder.FindInterface(ref capture, ref interleaved, this.videoDeviceFilter, ref riid, out obj3) != 0)
                {
                    interleaved = MediaType.Video;
                    if (this.captureGraphBuilder.FindInterface(ref capture, ref interleaved, this.videoDeviceFilter, ref riid, out obj3) != 0)
                    {
                        obj3 = null;
                    }
                }
                IAMTVTuner tuner = obj3 as IAMTVTuner;
                if (tuner != null)
                {
                    this.tuner = new Tuner(tuner);
                }
                this.graphState = GraphState.Created;
            }
        }

        public void Cue()
        {
            this.assertStopped();
            this.wantCaptureRendered = true;
            this.renderGraph();
            int errorCode = this.mediaControl.Pause();
            if (errorCode != 0)
            {
                Marshal.ThrowExceptionForHR(errorCode);
            }
        }
        public string[] GetDrivers()
        {
            Filters file = new Filters();
            FilterCollection fc = file.VideoInputDevices;
            string[] str = new string[fc.Count];
            for (int i = 0; i < fc.Count; i++)
            {
                str[i] = fc[i].Name;
            }
            return str;
        }
        protected void derenderGraph()
        {
            if (this.mediaControl != null)
            {
                this.mediaControl.Stop();
            }
            if (this.videoWindow != null)
            {
                this.videoWindow.put_Visible(0);
                this.videoWindow.put_Owner(IntPtr.Zero);
                this.videoWindow = null;
            }
            if (this.PreviewWindow != null)
            {
                this.previewWindow.Resize -= new EventHandler(this.onPreviewWindowResize);
            }
            if (this.graphState >= GraphState.Rendered)
            {
                this.graphState = GraphState.Created;
                this.isCaptureRendered = false;
                this.isPreviewRendered = false;
                if (this.videoDeviceFilter != null)
                {
                    this.removeDownstream(this.videoDeviceFilter, this.videoCompressor == null);
                }
                if (this.audioDeviceFilter != null)
                {
                    this.removeDownstream(this.audioDeviceFilter, this.audioCompressor == null);
                }
                this.muxFilter = null;
                this.fileWriterFilter = null;
                this.baseGrabFlt = null;
            }
        }

        protected void destroyGraph()
        {
            try
            {
                this.derenderGraph();
            }
            catch
            {
            }
            this.graphState = GraphState.Null;
            this.isCaptureRendered = false;
            this.isPreviewRendered = false;
            if (this.rotCookie != 0)
            {
                DsROT.RemoveGraphFromRot(ref this.rotCookie);
                this.rotCookie = 0;
            }
            if (this.muxFilter != null)
            {
                this.graphBuilder.RemoveFilter(this.muxFilter);
            }
            if (this.baseGrabFlt != null)
            {
                this.graphBuilder.RemoveFilter(this.baseGrabFlt);
            }
            if (this.videoCompressorFilter != null)
            {
                this.graphBuilder.RemoveFilter(this.videoCompressorFilter);
            }
            if (this.audioCompressorFilter != null)
            {
                this.graphBuilder.RemoveFilter(this.audioCompressorFilter);
            }
            if (this.videoDeviceFilter != null)
            {
                this.graphBuilder.RemoveFilter(this.videoDeviceFilter);
            }
            if (this.audioDeviceFilter != null)
            {
                this.graphBuilder.RemoveFilter(this.audioDeviceFilter);
            }
            if (this.videoSources != null)
            {
                this.videoSources.Dispose();
            }
            this.videoSources = null;
            if (this.audioSources != null)
            {
                this.audioSources.Dispose();
            }
            this.audioSources = null;
            if (this.propertyPages != null)
            {
                this.propertyPages.Dispose();
            }
            this.propertyPages = null;
            if (this.tuner != null)
            {
                this.tuner.Dispose();
            }
            this.tuner = null;
            if (this.graphBuilder != null)
            {
                Marshal.ReleaseComObject(this.graphBuilder);
            }
            this.graphBuilder = null;
            if (this.captureGraphBuilder != null)
            {
                Marshal.ReleaseComObject(this.captureGraphBuilder);
            }
            this.captureGraphBuilder = null;
            if (this.muxFilter != null)
            {
                Marshal.ReleaseComObject(this.muxFilter);
            }
            this.muxFilter = null;
            if (this.baseGrabFlt != null)
            {
                Marshal.ReleaseComObject(this.baseGrabFlt);
            }
            this.baseGrabFlt = null;
            if (this.fileWriterFilter != null)
            {
                Marshal.ReleaseComObject(this.fileWriterFilter);
            }
            this.fileWriterFilter = null;
            if (this.videoDeviceFilter != null)
            {
                Marshal.ReleaseComObject(this.videoDeviceFilter);
            }
            this.videoDeviceFilter = null;
            if (this.audioDeviceFilter != null)
            {
                Marshal.ReleaseComObject(this.audioDeviceFilter);
            }
            this.audioDeviceFilter = null;
            if (this.videoCompressorFilter != null)
            {
                Marshal.ReleaseComObject(this.videoCompressorFilter);
            }
            this.videoCompressorFilter = null;
            if (this.audioCompressorFilter != null)
            {
                Marshal.ReleaseComObject(this.audioCompressorFilter);
            }
            this.audioCompressorFilter = null;
            this.mediaControl = null;
            this.videoWindow = null;
            GC.Collect();
        }

        public void DisposeCapture()
        {
            this.wantPreviewRendered = false;
            this.wantCaptureRendered = false;
            this.CaptureComplete = null;
            try
            {
                this.destroyGraph();
            }
            catch
            {
            }
            if (this.videoSources != null)
            {
                this.videoSources.Dispose();
            }
            this.videoSources = null;
            if (this.audioSources != null)
            {
                this.audioSources.Dispose();
            }
            this.audioSources = null;
        }

        int ISampleGrabberCB.BufferCB(double SampleTime, IntPtr pBuffer, int BufferLen)
        {
            if (!this.captured && (this.savedArray != null))
            {
                this.captured = true;
                this.bufferedSize = BufferLen;
                if (((pBuffer != IntPtr.Zero) && (BufferLen > 0x3e8)) && (BufferLen <= this.savedArray.Length))
                {
                    Marshal.Copy(pBuffer, this.savedArray, 0, BufferLen);
                }
                try
                {
                    base.BeginInvoke(new CaptureDone(this.OnCaptureDone));
                }
                catch (ThreadInterruptedException exception)
                {
                    MessageBox.Show(exception.Message);
                }
                catch (Exception exception2)
                {
                    MessageBox.Show(exception2.Message);
                }
            }
            return 0;
        }

        int ISampleGrabberCB.SampleCB(double SampleTime, IMediaSample pSample)
        {
            return 0;
        }

        ~Capture()
        {
            base.Dispose();
        }

        protected object getStreamConfigSetting(IAMStreamConfig streamConfig, string fieldName)
        {
            if (streamConfig == null)
            {
                throw new NotSupportedException();
            }
            this.assertStopped();
            this.derenderGraph();
            object obj2 = null;
            IntPtr zero = IntPtr.Zero;
            AMMediaType structure = new AMMediaType();
            try
            {
                object obj3;
                int format = streamConfig.GetFormat(out zero);
                if (format != 0)
                {
                    Marshal.ThrowExceptionForHR(format);
                }
                Marshal.PtrToStructure(zero, structure);
                if (structure.formatType == FormatType.WaveEx)
                {
                    obj3 = new WaveFormatEx();
                }
                else if (structure.formatType == FormatType.VideoInfo)
                {
                    obj3 = new VideoInfoHeader();
                }
                else
                {
                    if (structure.formatType != FormatType.VideoInfo2)
                    {
                        throw new NotSupportedException("This device does not support a recognized format block.");
                    }
                    obj3 = new VideoInfoHeader2();
                }
                Marshal.PtrToStructure(structure.formatPtr, obj3);
                FieldInfo field = obj3.GetType().GetField(fieldName);
                if (field == null)
                {
                    throw new NotSupportedException("Unable to find the member '" + fieldName + "' in the format block.");
                }
                obj2 = field.GetValue(obj3);
            }
            finally
            {
                DsUtils.FreeAMMediaType(structure);
                Marshal.FreeCoTaskMem(zero);
            }
            this.renderStream = false;
            this.renderGraph();
            this.startPreviewIfNeeded();
            return obj2;
        }

        protected string getTempFilename()
        {
            string str;
            try
            {
                int num = 0;
                Random random = new Random();
                string tempPath = Path.GetTempPath();
                do
                {
                    str = Path.Combine(tempPath, random.Next().ToString("X") + ".avi");
                    num++;
                    if (num > 100)
                    {
                        throw new InvalidOperationException("Unable to find temporary file.");
                    }
                }
                while (File.Exists(str));
            }
            catch
            {
                str = "c:\temp.avi";
            }
            return str;
        }

        public void GoOn()
        {
            int errorCode = this.mediaControl.Run();
            if (errorCode != 0)
            {
                Marshal.ThrowExceptionForHR(errorCode);
            }
        }

        private void InitializeComponent()
        {
            base.SuspendLayout();
            base.ClientSize = new Size(0x124, 0x10a);
            base.Name = "Capture";
            base.Load += new EventHandler(this.Capture_Load);
            base.ResumeLayout(false);
        }

        private void OnCaptureDone()
        {
            if (this.sampGrabber != null)
            {
                this.sampGrabber.SetCallback(null, 0);
                int width = this.videoInfoHeader.BmiHeader.Width;
                int height = this.videoInfoHeader.BmiHeader.Height;
                if ((((width & 3) == 0) && (width >= 0x20)) && (((width <= 0x1000) && (height >= 0x20)) && (height <= 0x1000)))
                {
                    int num3 = width * 3;
                    GCHandle handle = GCHandle.Alloc(this.savedArray, GCHandleType.Pinned);
                    int num4 = (int) handle.AddrOfPinnedObject();
                    num4 += (height - 1) * num3;
                    Bitmap bitmap = new Bitmap(width, height, -num3, PixelFormat.Format24bppRgb, (IntPtr) num4);
                    handle.Free();
                    this.savedArray = null;
                    //this.ImageCaptured.Image = bitmap;
                    //this.FrameCaptureComplete(this.ImageCaptured);
                    this.image = bitmap;
                    this.FrameCaptureComplete(this.image);
                    
                }
            }
        }

        private void OnGraphNotify()
        {
            DsEvCode code;
            int num;
            int num2;
            while ((this.mediaEvt.GetEvent(out code, out num, out num2, 0) >= 0) && (this.mediaEvt.FreeEventParams(code, num, num2) == 0))
            {
            }
        }

        protected void onPreviewWindowResize(object sender, EventArgs e)
        {
            if (this.videoWindow != null)
            {
                Rectangle clientRectangle = this.previewWindow.ClientRectangle;
                this.videoWindow.SetWindowPosition(0, 0, clientRectangle.Right, clientRectangle.Bottom);
            }
        }

        public void Pause()
        {
            int errorCode = this.mediaControl.Pause();

            if (errorCode != 0)
            {
                Marshal.ThrowExceptionForHR(errorCode);
            }
        }

        protected void removeDownstream(IBaseFilter filter, bool removeFirstFilter)
        {
            IEnumPins pins;
            int num = filter.EnumPins(out pins);
            pins.Reset();
            if ((num == 0) && (pins != null))
            {
                IPin[] ppPins = new IPin[1];
                do
                {
                    int num2;
                    num = pins.Next(1, ppPins, out num2);
                    if ((num == 0) && (ppPins[0] != null))
                    {
                        IPin ppPin = null;
                        ppPins[0].ConnectedTo(out ppPin);
                        if (ppPin != null)
                        {
                            PinInfo pInfo = new PinInfo();
                            num = ppPin.QueryPinInfo(out pInfo);
                            if ((num == 0) && (pInfo.dir == PinDirection.Input))
                            {
                                this.removeDownstream(pInfo.filter, true);
                                this.graphBuilder.Disconnect(ppPin);
                                this.graphBuilder.Disconnect(ppPins[0]);
                                if ((pInfo.filter != this.videoCompressorFilter) && (pInfo.filter != this.audioCompressorFilter))
                                {
                                    this.graphBuilder.RemoveFilter(pInfo.filter);
                                }
                            }
                            Marshal.ReleaseComObject(pInfo.filter);
                            Marshal.ReleaseComObject(ppPin);
                        }
                        Marshal.ReleaseComObject(ppPins[0]);
                    }
                }
                while (num == 0);
                Marshal.ReleaseComObject(pins);
                pins = null;
            }
        }

        protected void renderGraph()
        {
            Guid capture;
            Guid interleaved;
            int connectedMediaType;
            bool flag = false;
            this.assertStopped();
            if (this.filename == null)
            {
                throw new ArgumentException("The Filename property has not been set to a file.\n");
            }
            if (this.mediaControl != null)
            {
                this.mediaControl.Stop();
            }
            this.createGraph();
            if (!(this.wantPreviewRendered || !this.isPreviewRendered))
            {
                this.derenderGraph();
            }
            if ((!this.wantCaptureRendered && this.isCaptureRendered) && this.wantPreviewRendered)
            {
                this.derenderGraph();
                this.graphState = GraphState.Null;
                this.createGraph();
            }
            if (this.wantCaptureRendered && !this.isCaptureRendered)
            {
                Guid avi = MediaSubType.Avi;
                connectedMediaType = this.captureGraphBuilder.SetOutputFileName(ref avi, this.Filename, out this.muxFilter, out this.fileWriterFilter);
                if (connectedMediaType < 0)
                {
                    Marshal.ThrowExceptionForHR(connectedMediaType);
                }
                if (this.VideoDevice != null)
                {
                    capture = PinCategory.Capture;
                    interleaved = MediaType.Interleaved;
                    if (this.captureGraphBuilder.RenderStream(ref capture, ref interleaved, this.videoDeviceFilter, this.videoCompressorFilter, this.muxFilter) < 0)
                    {
                        interleaved = MediaType.Video;
                        connectedMediaType = this.captureGraphBuilder.RenderStream(ref capture, ref interleaved, this.videoDeviceFilter, this.videoCompressorFilter, this.muxFilter);
                        if (connectedMediaType == -2147220969)
                        {
                            throw new DeviceInUseException("Video device", connectedMediaType);
                        }
                        if (connectedMediaType < 0)
                        {
                            Marshal.ThrowExceptionForHR(connectedMediaType);
                        }
                    }
                }
                if (this.AudioDevice != null)
                {
                    capture = PinCategory.Capture;
                    interleaved = MediaType.Audio;
                    connectedMediaType = this.captureGraphBuilder.RenderStream(ref capture, ref interleaved, this.audioDeviceFilter, this.audioCompressorFilter, this.muxFilter);
                    if (connectedMediaType < 0)
                    {
                        Marshal.ThrowExceptionForHR(connectedMediaType);
                    }
                }
                this.isCaptureRendered = true;
                flag = true;
            }
            if ((this.wantPreviewRendered && this.renderStream) && !this.isPreviewRendered)
            {
                capture = PinCategory.Preview;
                interleaved = MediaType.Video;
                connectedMediaType = this.captureGraphBuilder.RenderStream(ref capture, ref interleaved, this.videoDeviceFilter, this.baseGrabFlt, null);
                if (connectedMediaType < 0)
                {
                    Marshal.ThrowExceptionForHR(connectedMediaType);
                }
                this.videoWindow = (IVideoWindow) this.graphBuilder;
                connectedMediaType = this.videoWindow.put_Owner(this.previewWindow.Handle);
                if (connectedMediaType < 0)
                {
                    Marshal.ThrowExceptionForHR(connectedMediaType);
                }
                connectedMediaType = this.videoWindow.put_WindowStyle(0x46000000);
                if (connectedMediaType < 0)
                {
                    Marshal.ThrowExceptionForHR(connectedMediaType);
                }
                this.previewWindow.Resize += new EventHandler(this.onPreviewWindowResize);
                this.onPreviewWindowResize(this, null);
                connectedMediaType = this.videoWindow.put_Visible(-1);
                if (connectedMediaType < 0)
                {
                    Marshal.ThrowExceptionForHR(connectedMediaType);
                }
                connectedMediaType = this.mediaEvt.SetNotifyWindow(base.Handle, 0x8001, IntPtr.Zero);
                if (connectedMediaType < 0)
                {
                    Marshal.ThrowExceptionForHR(connectedMediaType);
                }
                this.isPreviewRendered = true;
                flag = true;
                AMMediaType pmt = new AMMediaType();
                connectedMediaType = this.sampGrabber.GetConnectedMediaType(pmt);
                if (connectedMediaType < 0)
                {
                    Marshal.ThrowExceptionForHR(connectedMediaType);
                }
                if ((pmt.formatType != FormatType.VideoInfo) || (pmt.formatPtr == IntPtr.Zero))
                {
                    throw new NotSupportedException("Unknown Grabber Media Format");
                }
                this.videoInfoHeader = (VideoInfoHeader) Marshal.PtrToStructure(pmt.formatPtr, typeof(VideoInfoHeader));
                Marshal.FreeCoTaskMem(pmt.formatPtr);
                pmt.formatPtr = IntPtr.Zero;
                connectedMediaType = this.sampGrabber.SetBufferSamples(false);
                if (connectedMediaType == 0)
                {
                    connectedMediaType = this.sampGrabber.SetOneShot(false);
                }
                if (connectedMediaType == 0)
                {
                    connectedMediaType = this.sampGrabber.SetCallback(null, 0);
                }
                if (connectedMediaType < 0)
                {
                    Marshal.ThrowExceptionForHR(connectedMediaType);
                }
            }
            if (flag)
            {
                this.graphState = GraphState.Rendered;
            }
        }

        public void RenderPreview()
        {
            this.assertStopped();
            this.renderStream = true;
            this.renderGraph();
            int errorCode = this.mediaControl.Run();
            if (errorCode != 0)
            {
                Marshal.ThrowExceptionForHR(errorCode);
            }
        }

        protected object setStreamConfigSetting(IAMStreamConfig streamConfig, string fieldName, object newValue)
        {
            if (streamConfig == null)
            {
                throw new NotSupportedException();
            }
            this.assertStopped();
            this.derenderGraph();
            IntPtr zero = IntPtr.Zero;
            AMMediaType structure = new AMMediaType();
            try
            {
                object obj2;
                int format = streamConfig.GetFormat(out zero);
                if (format != 0)
                {
                    Marshal.ThrowExceptionForHR(format);
                }
                Marshal.PtrToStructure(zero, structure);
                if (structure.formatType == FormatType.WaveEx)
                {
                    obj2 = new WaveFormatEx();
                }
                else if (structure.formatType == FormatType.VideoInfo)
                {
                    obj2 = new VideoInfoHeader();
                }
                else
                {
                    if (structure.formatType != FormatType.VideoInfo2)
                    {
                        throw new NotSupportedException("This device does not support a recognized format block.");
                    }
                    obj2 = new VideoInfoHeader2();
                }
                Marshal.PtrToStructure(structure.formatPtr, obj2);
                FieldInfo field = obj2.GetType().GetField(fieldName);
                if (field == null)
                {
                    throw new NotSupportedException("Unable to find the member '" + fieldName + "' in the format block.");
                }
                field.SetValue(obj2, newValue);
                Marshal.StructureToPtr(obj2, structure.formatPtr, false);
                format = streamConfig.SetFormat(structure);
                if (format != 0)
                {
                    Marshal.ThrowExceptionForHR(format);
                }
            }
            finally
            {
                DsUtils.FreeAMMediaType(structure);
                Marshal.FreeCoTaskMem(zero);
            }
            this.renderStream = false;
            this.renderGraph();
            this.startPreviewIfNeeded();
            return null;
        }

        public void Start()
        {
            this.Stop();
            this.firstFrame = false;
            this.assertStopped();
            this.wantCaptureRendered = true;
            this.renderStream = true;
            this.renderGraph();
            int errorCode = this.mediaControl.Run();
            if (errorCode != 0)
            {
                Marshal.ThrowExceptionForHR(errorCode);
            }
            this.graphState = GraphState.Capturing;
        }

        protected void startPreviewIfNeeded()
        {
            if (!((!this.wantPreviewRendered || !this.isPreviewRendered) || this.isCaptureRendered))
            {
                this.mediaControl.Run();
            }
        }

        public void Stop()
        {
            if (this.mediaControl != null)
            {
                this.mediaControl.Stop();
            }
            this.wantCaptureRendered = false;
            this.wantPreviewRendered = true;
            if (this.graphState == GraphState.Capturing)
            {
                this.graphState = GraphState.Rendered;
                if (this.CaptureComplete != null)
                {
                    this.CaptureComplete(this, null);
                }
            }
            this.firstFrame = true;
            this.renderStream = false;
            try
            {
                this.renderGraph();
            }
            catch
            {
            }
            try
            {
                this.startPreviewIfNeeded();
            }
            catch
            {
            }
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
        }

        public AudioCapabilities AudioCaps
        {
            get
            {
                if ((this.audioCaps == null) && (this.audioStreamConfig != null))
                {
                    try
                    {
                        this.audioCaps = new AudioCapabilities(this.audioStreamConfig);
                    }
                    catch (Exception)
                    {
                    }
                }
                return this.audioCaps;
            }
        }

        public short AudioChannels
        {
            get
            {
                return (short) this.getStreamConfigSetting(this.audioStreamConfig, "nChannels");
            }
            set
            {
                this.setStreamConfigSetting(this.audioStreamConfig, "nChannels", value);
            }
        }

        public Filter AudioCompressor
        {
            get
            {
                return this.audioCompressor;
            }
            set
            {
                this.assertStopped();
                this.destroyGraph();
                this.audioCompressor = value;
                this.renderGraph();
                this.startPreviewIfNeeded();
            }
        }

        public Filter AudioDevice
        {
            get
            {
                return this.audioDevice;
            }
        }

        public short AudioSampleSize
        {
            get
            {
                return (short) this.getStreamConfigSetting(this.audioStreamConfig, "wBitsPerSample");
            }
            set
            {
                this.setStreamConfigSetting(this.audioStreamConfig, "wBitsPerSample", value);
            }
        }

        public int AudioSamplingRate
        {
            get
            {
                return (int) this.getStreamConfigSetting(this.audioStreamConfig, "nSamplesPerSec");
            }
            set
            {
                this.setStreamConfigSetting(this.audioStreamConfig, "nSamplesPerSec", value);
            }
        }

        public Source AudioSource
        {
            get
            {
                return this.AudioSources.CurrentSource;
            }
            set
            {
                this.AudioSources.CurrentSource = value;
            }
        }

        public SourceCollection AudioSources
        {
            get
            {
                if (this.audioSources == null)
                {
                    try
                    {
                        if (this.audioDevice != null)
                        {
                            this.audioSources = new SourceCollection(this.captureGraphBuilder, this.audioDeviceFilter, false);
                        }
                        else
                        {
                            this.audioSources = new SourceCollection();
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
                return this.audioSources;
            }
        }

        public bool Capturing
        {
            get
            {
                return (this.graphState == GraphState.Capturing);
            }
        }

        public bool Cued
        {
            get
            {
                return (this.isCaptureRendered && (this.graphState == GraphState.Rendered));
            }
        }

        public string Filename
        {
            get
            {
                return this.filename;
            }
            set
            {
                this.assertStopped();
                if (this.Cued)
                {
                    throw new InvalidOperationException("The Filename cannot be changed once cued. Use Stop() before changing the filename.");
                }
                this.filename = value;
                if (this.fileWriterFilter != null)
                {
                    string str;
                    AMMediaType pmt = new AMMediaType();
                    int curFile = this.fileWriterFilter.GetCurFile(out str, pmt);
                    if (curFile < 0)
                    {
                        Marshal.ThrowExceptionForHR(curFile);
                    }
                    if (pmt.formatSize > 0)
                    {
                        Marshal.FreeCoTaskMem(pmt.formatPtr);
                    }
                    curFile = this.fileWriterFilter.SetFileName(this.filename, pmt);
                    if (curFile < 0)
                    {
                        Marshal.ThrowExceptionForHR(curFile);
                    }
                }
            }
        }

        public double FrameRate
        {
            get
            {
                long num = (long) this.getStreamConfigSetting(this.videoStreamConfig, "AvgTimePerFrame");
                return (10000000.0 / ((double) num));
            }
            set
            {
                long newValue = (long) (10000000.0 / value);
                this.setStreamConfigSetting(this.videoStreamConfig, "AvgTimePerFrame", newValue);
            }
        }

        public Size FrameSize
        {
            get
            {
                BitmapInfoHeader header = (BitmapInfoHeader) this.getStreamConfigSetting(this.videoStreamConfig, "BmiHeader");
                return new Size(header.Width, header.Height);
            }
            set
            {
                BitmapInfoHeader newValue = (BitmapInfoHeader) this.getStreamConfigSetting(this.videoStreamConfig, "BmiHeader");
                newValue.Width = value.Width;
                newValue.Height = value.Height;
                this.setStreamConfigSetting(this.videoStreamConfig, "BmiHeader", newValue);
            }
        }

        public Control PreviewWindow
        {
            get
            {
                return this.previewWindow;
            }
            set
            {
                this.assertStopped();
                this.derenderGraph();
                this.previewWindow = value;
                this.wantPreviewRendered = (this.previewWindow != null) && (this.videoDevice != null);
                this.renderStream = false;
                this.renderGraph();
                this.startPreviewIfNeeded();
            }
        }

        public PropertyPageCollection PropertyPages
        {
            get
            {
                if (this.propertyPages == null)
                {
                    try
                    {
                        this.propertyPages = new PropertyPageCollection(this.captureGraphBuilder, this.videoDeviceFilter, this.audioDeviceFilter, this.videoCompressorFilter, this.audioCompressorFilter, this.VideoSources, this.AudioSources);
                    }
                    catch (Exception)
                    {
                    }
                }
                return this.propertyPages;
            }
        }

        public bool Stopped
        {
            get
            {
                return (this.graphState != GraphState.Capturing);
            }
        }

        public Tuner Tuner
        {
            get
            {
                return this.tuner;
            }
        }

        public VideoCapabilities VideoCaps
        {
            get
            {
                if ((this.videoCaps == null) && (this.videoStreamConfig != null))
                {
                    try
                    {
                        this.videoCaps = new VideoCapabilities(this.videoStreamConfig);
                    }
                    catch (Exception)
                    {
                    }
                }
                return this.videoCaps;
            }
        }

        public Filter VideoCompressor
        {
            get
            {
                return this.videoCompressor;
            }
            set
            {
                this.assertStopped();
                this.destroyGraph();
                this.videoCompressor = value;
                this.renderGraph();
                this.startPreviewIfNeeded();
            }
        }

        public Filter VideoDevice
        {
            get
            {
                return this.videoDevice;
            }
        }

        public Source VideoSource
        {
            get
            {
                return this.VideoSources.CurrentSource;
            }
            set
            {
                this.VideoSources.CurrentSource = value;
            }
        }

        public SourceCollection VideoSources
        {
            get
            {
                if (this.videoSources == null)
                {
                    try
                    {
                        if (this.videoDevice != null)
                        {
                            this.videoSources = new SourceCollection(this.captureGraphBuilder, this.videoDeviceFilter, true);
                        }
                        else
                        {
                            this.videoSources = new SourceCollection();
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
                return this.videoSources;
            }
        }

        private delegate void CaptureDone();

        public delegate void FrameCapHandler(Image image);

        protected enum GraphState
        {
            Null,
            Created,
            Rendered,
            Capturing
        }
    }
}

