using System;
using System.Runtime.InteropServices;
using DShowNET;
using DShowNET.Device;
namespace ICameraDll.DirectX.Capture
{
    public class Filter : IComparable
    {
        public string MonikerString;
        public string Name;

        internal Filter(UCOMIMoniker moniker)
        {
            this.Name = this.getName(moniker);
            this.MonikerString = this.getMonikerString(moniker);
        }

        public Filter(string monikerString)
        {
            this.Name = this.getName(monikerString);
            this.MonikerString = monikerString;
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }
            Filter filter = (Filter) obj;
            return this.Name.CompareTo(filter.Name);
        }

        protected UCOMIMoniker getAnyMoniker()
        {
            UCOMIMoniker moniker;
            Guid videoCompressorCategory = FilterCategory.VideoCompressorCategory;
            object o = null;
            ICreateDevEnum enum2 = null;
            UCOMIEnumMoniker ppEnumMoniker = null;
            UCOMIMoniker[] rgelt = new UCOMIMoniker[1];
            try
            {
                int num;
                Type typeFromCLSID = Type.GetTypeFromCLSID(Clsid.SystemDeviceEnum);
                if (typeFromCLSID == null)
                {
                    throw new NotImplementedException("System Device Enumerator");
                }
                o = Activator.CreateInstance(typeFromCLSID);
                enum2 = (ICreateDevEnum) o;
                if (enum2.CreateClassEnumerator(ref videoCompressorCategory, out ppEnumMoniker, 0) != 0)
                {
                    throw new NotSupportedException("No devices of the category");
                }
                if (ppEnumMoniker.Next(1, rgelt, out num) != 0)
                {
                    rgelt[0] = null;
                }
                moniker = rgelt[0];
            }
            finally
            {
                enum2 = null;
                if (ppEnumMoniker != null)
                {
                    Marshal.ReleaseComObject(ppEnumMoniker);
                }
                ppEnumMoniker = null;
                if (o != null)
                {
                    Marshal.ReleaseComObject(o);
                }
                o = null;
            }
            return moniker;
        }

        protected string getMonikerString(UCOMIMoniker moniker)
        {
            string str;
            moniker.GetDisplayName(null, null, out str);
            return str;
        }

        protected string getName(UCOMIMoniker moniker)
        {
            object ppvObj = null;
            IPropertyBag bag = null;
            string str;
            try
            {
                Guid gUID = typeof(IPropertyBag).GUID;
                moniker.BindToStorage(null, null, ref gUID, out ppvObj);
                bag = (IPropertyBag) ppvObj;
                object pVar = "";
                int errorCode = bag.Read("FriendlyName", ref pVar, IntPtr.Zero);
                if (errorCode != 0)
                {
                    Marshal.ThrowExceptionForHR(errorCode);
                }
                string str2 = pVar as string;
                if ((str2 == null) || (str2.Length < 1))
                {
                    throw new NotImplementedException("Device FriendlyName");
                }
                str = str2;
            }
            catch (Exception)
            {
                str = "";
            }
            finally
            {
                bag = null;
                if (ppvObj != null)
                {
                    Marshal.ReleaseComObject(ppvObj);
                }
                ppvObj = null;
            }
            return str;
        }

        protected string getName(string monikerString)
        {
            UCOMIMoniker moniker = null;
            UCOMIMoniker ppmkOut = null;
            string str;
            try
            {
                int num;
                moniker = this.getAnyMoniker();
                moniker.ParseDisplayName(null, null, monikerString, out num, out ppmkOut);
                str = this.getName(moniker);
            }
            finally
            {
                if (moniker != null)
                {
                    Marshal.ReleaseComObject(moniker);
                }
                moniker = null;
                if (ppmkOut != null)
                {
                    Marshal.ReleaseComObject(ppmkOut);
                }
                ppmkOut = null;
            }
            return str;
        }
    }
}

