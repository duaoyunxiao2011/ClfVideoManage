using System;
using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;
using DShowNET;
using DShowNET.Device;

namespace ICameraDll.DirectX.Capture
{
    public class FilterCollection : CollectionBase
    {
        internal FilterCollection(Guid category)
        {
            this.getFilters(category);
        }

        protected void getFilters(Guid category)
        {
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
                if (enum2.CreateClassEnumerator(ref category, out ppEnumMoniker, 0) != 0)
                {
                    throw new NotSupportedException("No devices of the category");
                }
                while ((ppEnumMoniker.Next(1, rgelt, out num) == 0) && (rgelt[0] != null))
                {
                    Filter filter = new Filter(rgelt[0]);
                    base.InnerList.Add(filter);
                    Marshal.ReleaseComObject(rgelt[0]);
                    rgelt[0] = null;
                }
                base.InnerList.Sort();
            }
            finally
            {
                enum2 = null;
                if (rgelt[0] != null)
                {
                    Marshal.ReleaseComObject(rgelt[0]);
                }
                rgelt[0] = null;
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
        }

        public Filter this[int index]
        {
            get
            {
                return (Filter) base.InnerList[index];
            }
        }
    }
}

