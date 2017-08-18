using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DShowNET;

namespace ICameraDll.DirectX.Capture
{
    public class DirectShowPropertyPage : PropertyPage
    {
        protected DShowNET.ISpecifyPropertyPages specifyPropertyPages;

        public DirectShowPropertyPage(string name, DShowNET.ISpecifyPropertyPages specifyPropertyPages)
        {
            base.Name = name;
            base.SupportsPersisting = false;
            this.specifyPropertyPages = specifyPropertyPages;
        }

        public void Dispose()
        {
            if (this.specifyPropertyPages != null)
            {
                Marshal.ReleaseComObject(this.specifyPropertyPages);
            }
            this.specifyPropertyPages = null;
        }

        [DllImport("olepro32.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]
        private static extern int OleCreatePropertyFrame(IntPtr hwndOwner, int x, int y, string lpszCaption, int cObjects, [In, MarshalAs(UnmanagedType.Interface)] ref object ppUnk, int cPages, IntPtr pPageClsID, int lcid, int dwReserved, IntPtr pvReserved);
        public override void Show(Control owner)
        {
            DsCAUUID pPages = new DsCAUUID();
            try
            {
                int pages = this.specifyPropertyPages.GetPages(out pPages);
                if (pages != 0)
                {
                    Marshal.ThrowExceptionForHR(pages);
                }
                object specifyPropertyPages = this.specifyPropertyPages;
                pages = OleCreatePropertyFrame(owner.Handle, 30, 30, null, 1, ref specifyPropertyPages, pPages.cElems, pPages.pElems, 0, 0, IntPtr.Zero);
            }
            finally
            {
                if (pPages.pElems != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(pPages.pElems);
                }
            }
        }
    }
}

