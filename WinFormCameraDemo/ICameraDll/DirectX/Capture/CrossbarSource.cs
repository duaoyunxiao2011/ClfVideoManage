using System;
using System.Runtime.InteropServices;
using DShowNET;

namespace ICameraDll.DirectX.Capture
{
    public class CrossbarSource : Source
    {
        internal PhysicalConnectorType ConnectorType;
        internal IAMCrossbar Crossbar;
        internal int InputPin;
        internal int OutputPin;

        internal CrossbarSource(IAMCrossbar crossbar, int outputPin, int inputPin, PhysicalConnectorType connectorType)
        {
            this.Crossbar = crossbar;
            this.OutputPin = outputPin;
            this.InputPin = inputPin;
            this.ConnectorType = connectorType;
            base.name = this.getName(connectorType);
        }

        public override void Dispose()
        {
            if (this.Crossbar != null)
            {
                Marshal.ReleaseComObject(this.Crossbar);
            }
            this.Crossbar = null;
            base.Dispose();
        }

        private string getName(PhysicalConnectorType connectorType)
        {
            switch (connectorType)
            {
                case PhysicalConnectorType.Video_Tuner:
                    return "Video Tuner";

                case PhysicalConnectorType.Video_Composite:
                    return "Video Composite";

                case PhysicalConnectorType.Video_SVideo:
                    return "Video S-Video";

                case PhysicalConnectorType.Video_RGB:
                    return "Video RGB";

                case PhysicalConnectorType.Video_YRYBY:
                    return "Video YRYBY";

                case PhysicalConnectorType.Video_SerialDigital:
                    return "Video Serial Digital";

                case PhysicalConnectorType.Video_ParallelDigital:
                    return "Video Parallel Digital";

                case PhysicalConnectorType.Video_SCSI:
                    return "Video SCSI";

                case PhysicalConnectorType.Video_AUX:
                    return "Video AUX";

                case PhysicalConnectorType.Video_1394:
                    return "Video Firewire";

                case PhysicalConnectorType.Video_USB:
                    return "Video USB";

                case PhysicalConnectorType.Video_VideoDecoder:
                    return "Video Decoder";

                case PhysicalConnectorType.Video_VideoEncoder:
                    return "Video Encoder";

                case PhysicalConnectorType.Video_SCART:
                    return "Video SCART";

                case PhysicalConnectorType.Audio_Tuner:
                    return "Audio Tuner";

                case PhysicalConnectorType.Audio_Line:
                    return "Audio Line In";

                case PhysicalConnectorType.Audio_Mic:
                    return "Audio Mic";

                case PhysicalConnectorType.Audio_AESDigital:
                    return "Audio AES Digital";

                case PhysicalConnectorType.Audio_SPDIFDigital:
                    return "Audio SPDIF Digital";

                case PhysicalConnectorType.Audio_SCSI:
                    return "Audio SCSI";

                case PhysicalConnectorType.Audio_AUX:
                    return "Audio AUX";

                case PhysicalConnectorType.Audio_1394:
                    return "Audio Firewire";

                case PhysicalConnectorType.Audio_USB:
                    return "Audio USB";

                case PhysicalConnectorType.Audio_AudioDecoder:
                    return "Audio Decoder";
            }
            return "Unknown Connector";
        }

        public override bool Enabled
        {
            get
            {
                int num;
                return ((this.Crossbar.get_IsRoutedTo(this.OutputPin, out num) == 0) && (this.InputPin == num));
            }
            set
            {
                if (value)
                {
                    int errorCode = this.Crossbar.Route(this.OutputPin, this.InputPin);
                    if (errorCode < 0)
                    {
                        Marshal.ThrowExceptionForHR(errorCode);
                    }
                }
                else
                {
                    int num2 = this.Crossbar.Route(this.OutputPin, -1);
                    if (num2 < 0)
                    {
                        Marshal.ThrowExceptionForHR(num2);
                    }
                }
            }
        }
    }
}

