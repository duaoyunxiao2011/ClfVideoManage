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
    public class CPlay : Form, ISampleGrabberCB
    {
        protected AudioCapabilities audioCaps;
        protected IAMStreamConfig audioStreamConfig;
        protected IBaseFilter baseGrabFlt;
        protected int bufferedSize;
        protected bool captured = true;
        protected bool capturedFrame;
        protected string filename = "";
        protected bool firstFrame = true;
        protected IGraphBuilder graphBuilder;
        protected GraphState graphState;
        protected PictureBox ImageCaptured;
        protected bool isCaptureRendered;
        protected bool isPreviewRendered;
        protected IMediaControl mediaControl;
        private IMediaEventEx mediaEvt;
        protected IBaseFilter muxFilter;
        protected Control previewWindow;
        protected bool renderStream;
        protected int rotCookie;
        protected ISampleGrabber sampGrabber;
        protected byte[] savedArray;
        protected VideoCapabilities videoCaps;
        protected VideoInfoHeader videoInfoHeader;
        protected IAMStreamConfig videoStreamConfig;
        protected IVideoWindow videoWindow;
        protected bool wantCaptureFrame;
        protected bool wantCaptureRendered;
        protected bool wantPreviewRendered;
        private const int WM_GRAPHNOTIFY = 0x8001;

        public event EventHandler CaptureComplete;

        public event FrameCapHandler FrameCaptureComplete;

        public CPlay()
        {
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

        private void CPlay_Load(object sender, EventArgs e)
        {
        }

        protected void createGraph()
        {
            System.Type typeFromCLSID = null;
            object obj2 = null;
            if (this.graphState < GraphState.Created)
            {
                GC.Collect();
                this.graphBuilder = (IGraphBuilder) Activator.CreateInstance(System.Type.GetTypeFromCLSID(Clsid.FilterGraph, true));
                typeFromCLSID = System.Type.GetTypeFromCLSID(Clsid.SampleGrabber, true);
                if (typeFromCLSID == null)
                {
                    throw new NotImplementedException("DirectShow SampleGrabber not installed/registered");
                }
                obj2 = Activator.CreateInstance(typeFromCLSID);
                this.sampGrabber = (ISampleGrabber) obj2;
                obj2 = null;
                AMMediaType pmt = new AMMediaType();
                pmt.majorType = MediaType.Video;
                pmt.subType = MediaSubType.RGB24;
                pmt.formatType = FormatType.VideoInfo;
                int errorCode = this.sampGrabber.SetMediaType(pmt);
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
                Guid capture = PinCategory.Capture;
                Guid interleaved = MediaType.Interleaved;
                Guid gUID = typeof(IAMStreamConfig).GUID;
                if (errorCode != 0)
                {
                    Guid video = MediaType.Video;
                }
                object obj3 = null;
                Guid guid5 = PinCategory.Capture;
                Guid audio = MediaType.Audio;
                Guid guid7 = typeof(IAMStreamConfig).GUID;
                this.audioStreamConfig = obj3 as IAMStreamConfig;
                this.mediaControl = (IMediaControl) this.graphBuilder;
                this.videoCaps = null;
                this.audioCaps = null;
                obj3 = null;
                Guid guid8 = PinCategory.Capture;
                Guid guid9 = MediaType.Interleaved;
                Guid guid10 = typeof(IAMTVTuner).GUID;
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
                this.muxFilter = null;
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
            if (this.graphBuilder != null)
            {
                Marshal.ReleaseComObject(this.graphBuilder);
            }
            this.graphBuilder = null;
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

        ~CPlay()
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
                str = @"c:\temp.avi";
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
            base.Name = "CPlay";
            base.Load += new EventHandler(this.CPlay_Load);
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
                    this.ImageCaptured.Image = bitmap;
                    this.FrameCaptureComplete(this.ImageCaptured);
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
            if (!(!this.wantCaptureRendered || this.isCaptureRendered))
            {
                this.graphBuilder.RenderFile(this.Filename, null);
            }
            if ((this.wantPreviewRendered && this.renderStream) && !this.isPreviewRendered)
            {
                Guid preview = PinCategory.Preview;
                Guid video = MediaType.Video;
                this.videoWindow = (IVideoWindow) this.graphBuilder;
                int errorCode = this.videoWindow.put_Owner(this.previewWindow.Handle);
                if (errorCode < 0)
                {
                    Marshal.ThrowExceptionForHR(errorCode);
                }
                errorCode = this.videoWindow.put_WindowStyle(0x46000000);
                if (errorCode < 0)
                {
                    Marshal.ThrowExceptionForHR(errorCode);
                }
                this.previewWindow.Resize += new EventHandler(this.onPreviewWindowResize);
                this.onPreviewWindowResize(this, null);
                errorCode = this.videoWindow.put_Visible(-1);
                if (errorCode < 0)
                {
                    Marshal.ThrowExceptionForHR(errorCode);
                }
                errorCode = this.mediaEvt.SetNotifyWindow(base.Handle, 0x8001, IntPtr.Zero);
                if (errorCode < 0)
                {
                    Marshal.ThrowExceptionForHR(errorCode);
                }
                this.isPreviewRendered = true;
                flag = true;
                AMMediaType pmt = new AMMediaType();
                errorCode = this.sampGrabber.GetConnectedMediaType(pmt);
                if (errorCode < 0)
                {
                    Marshal.ThrowExceptionForHR(errorCode);
                }
                if ((pmt.formatType != FormatType.VideoInfo) || (pmt.formatPtr == IntPtr.Zero))
                {
                    throw new NotSupportedException("Unknown Grabber Media Format");
                }
                this.videoInfoHeader = (VideoInfoHeader) Marshal.PtrToStructure(pmt.formatPtr, typeof(VideoInfoHeader));
                Marshal.FreeCoTaskMem(pmt.formatPtr);
                pmt.formatPtr = IntPtr.Zero;
                errorCode = this.sampGrabber.SetBufferSamples(false);
                if (errorCode == 0)
                {
                    errorCode = this.sampGrabber.SetOneShot(false);
                }
                if (errorCode == 0)
                {
                    errorCode = this.sampGrabber.SetCallback(null, 0);
                }
                if (errorCode < 0)
                {
                    Marshal.ThrowExceptionForHR(errorCode);
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
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x8001)
            {
                if (this.mediaEvt != null)
                {
                    this.OnGraphNotify();
                }
            }
            else
            {
                base.WndProc(ref m);
            }
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
                this.wantPreviewRendered = this.previewWindow != null;
                this.renderStream = false;
                this.renderGraph();
                this.startPreviewIfNeeded();
            }
        }

        public bool Stopped
        {
            get
            {
                return (this.graphState != GraphState.Capturing);
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

        private delegate void CaptureDone();

        public delegate void FrameCapHandler(PictureBox Frame);

        protected enum GraphState
        {
            Null,
            Created,
            Rendered,
            Capturing
        }
    }
}

