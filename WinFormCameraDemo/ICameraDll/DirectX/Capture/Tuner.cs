using DShowNET;
using System;
using System.Runtime.InteropServices;

namespace ICameraDll.DirectX.Capture
{
    public class Tuner : IDisposable
    {
        protected IAMTVTuner tvTuner;

        public Tuner(IAMTVTuner tuner)
        {
            this.tvTuner = tuner;
        }

        public void Dispose()
        {
            if (this.tvTuner != null)
            {
                Marshal.ReleaseComObject(this.tvTuner);
            }
            this.tvTuner = null;
        }

        public int Channel
        {
            get
            {
                int num;
                int num2;
                int num3;
                int errorCode = this.tvTuner.get_Channel(out num, out num2, out num3);
                if (errorCode != 0)
                {
                    Marshal.ThrowExceptionForHR(errorCode);
                }
                return num;
            }
            set
            {
                int errorCode = this.tvTuner.put_Channel(value, AMTunerSubChannel.Default, AMTunerSubChannel.Default);
                if (errorCode != 0)
                {
                    Marshal.ThrowExceptionForHR(errorCode);
                }
            }
        }

        public  TunerInputType InputType
        {
            get
            {
                DShowNET.TunerInputType type;
                int errorCode = this.tvTuner.get_InputType(0, out type);
                if (errorCode != 0)
                {
                    Marshal.ThrowExceptionForHR(errorCode);
                }
                return (TunerInputType) type;
            }
            set
            {
                DShowNET.TunerInputType inputType = (DShowNET.TunerInputType) value;
                int errorCode = this.tvTuner.put_InputType(0, inputType);
                if (errorCode != 0)
                {
                    Marshal.ThrowExceptionForHR(errorCode);
                }
            }
        }

        public bool SignalPresent
        {
            get
            {
                AMTunerSignalStrength strength;
                int errorCode = this.tvTuner.SignalPresent(out strength);
                if (errorCode != 0)
                {
                    Marshal.ThrowExceptionForHR(errorCode);
                }
                if (strength == AMTunerSignalStrength.NA)
                {
                    throw new NotSupportedException("Signal strength not available.");
                }
                return (strength == AMTunerSignalStrength.SignalPresent);
            }
        }
    }
}

