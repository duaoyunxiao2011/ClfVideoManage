using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DShowNET;

namespace ICameraDll.DirectX.Capture
{
    public class VfwCompressorPropertyPage : PropertyPage
    {
        protected IAMVfwCompressDialogs vfwCompressDialogs;

        public VfwCompressorPropertyPage(string name, IAMVfwCompressDialogs compressDialogs)
        {
            base.Name = name;
            base.SupportsPersisting = true;
            this.vfwCompressDialogs = compressDialogs;
        }

        public override void Show(Control owner)
        {
            this.vfwCompressDialogs.ShowDialog(VfwCompressDialogs.Config, owner.Handle);
        }

        public override byte[] State
        {
            get
            {
                byte[] pState = null;
                int pcbState = 0;
                if ((this.vfwCompressDialogs.GetState(null, ref pcbState) == 0) && (pcbState > 0))
                {
                    pState = new byte[pcbState];
                    if (this.vfwCompressDialogs.GetState(pState, ref pcbState) != 0)
                    {
                        pState = null;
                    }
                }
                return pState;
            }
            set
            {
                int errorCode = this.vfwCompressDialogs.SetState(value, value.Length);
                if (errorCode != 0)
                {
                    Marshal.ThrowExceptionForHR(errorCode);
                }
            }
        }
    }
}

