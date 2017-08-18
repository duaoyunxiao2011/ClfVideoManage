using System;
using System.Runtime.InteropServices;
using DShowNET;

namespace ICameraDll.DirectX.Capture
{
    public class AudioSource : Source
    {
        internal IPin Pin;

        internal AudioSource(IPin pin)
        {
            if (!(pin is IAMAudioInputMixer))
            {
                throw new NotSupportedException("The input pin does not support the IAMAudioInputMixer interface");
            }
            this.Pin = pin;
            base.name = this.getName(pin);
        }

        public override void Dispose()
        {
            if (this.Pin != null)
            {
                Marshal.ReleaseComObject(this.Pin);
            }
            this.Pin = null;
            base.Dispose();
        }

        private string getName(IPin pin)
        {
            string str = "Unknown pin";
            PinInfo pInfo = new PinInfo();
            int errorCode = pin.QueryPinInfo(out pInfo);
            if (errorCode == 0)
            {
                str = pInfo.name ?? "";
            }
            else
            {
                Marshal.ThrowExceptionForHR(errorCode);
            }
            if (pInfo.filter != null)
            {
                Marshal.ReleaseComObject(pInfo.filter);
            }
            pInfo.filter = null;
            return str;
        }

        public override bool Enabled
        {
            get
            {
                bool flag;
                ((IAMAudioInputMixer) this.Pin).get_Enable(out flag);
                return flag;
            }
            set
            {
                ((IAMAudioInputMixer) this.Pin).put_Enable(value);
            }
        }
    }
}

