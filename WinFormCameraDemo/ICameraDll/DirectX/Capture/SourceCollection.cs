using System;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using DShowNET;

namespace ICameraDll.DirectX.Capture
{
    public class SourceCollection : CollectionBase, IDisposable
    {
        internal SourceCollection()
        {
            base.InnerList.Capacity = 1;
        }

        internal SourceCollection(ICaptureGraphBuilder2 graphBuilder, IBaseFilter deviceFilter, bool isVideoDevice)
        {
            this.addFromGraph(graphBuilder, deviceFilter, isVideoDevice);
        }

        protected void addFromGraph(ICaptureGraphBuilder2 graphBuilder, IBaseFilter deviceFilter, bool isVideoDevice)
        {
            Trace.Assert(graphBuilder != null);
            foreach (IAMCrossbar crossbar in this.findCrossbars(graphBuilder, deviceFilter))
            {
                ArrayList c = this.findCrossbarSources(graphBuilder, crossbar, isVideoDevice);
                base.InnerList.AddRange(c);
            }
            if (!(isVideoDevice || (base.InnerList.Count != 0)))
            {
                ArrayList list2 = this.findAudioSources(graphBuilder, deviceFilter);
                base.InnerList.AddRange(list2);
            }
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

        ~SourceCollection()
        {
            this.Dispose();
        }

        protected ArrayList findAudioSources(ICaptureGraphBuilder2 graphBuilder, IBaseFilter deviceFilter)
        {
            ArrayList list = new ArrayList();
            if (deviceFilter is IAMAudioInputMixer)
            {
                IEnumPins pins;
                int num = deviceFilter.EnumPins(out pins);
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
                            PinDirection output = PinDirection.Output;
                            num = ppPins[0].QueryDirection(out output);
                            if ((num == 0) && (output == PinDirection.Input))
                            {
                                AudioSource source = new AudioSource(ppPins[0]);
                                list.Add(source);
                            }
                            ppPins[0] = null;
                        }
                    }
                    while (num == 0);
                    Marshal.ReleaseComObject(pins);
                    pins = null;
                }
            }
            if (list.Count == 1)
            {
                list.Clear();
            }
            return list;
        }

        protected ArrayList findCrossbars(ICaptureGraphBuilder2 graphBuilder, IBaseFilter deviceFilter)
        {
            ArrayList list = new ArrayList();
            Guid upstreamOnly = FindDirection.UpstreamOnly;
            Guid pType = new Guid();
            Guid gUID = typeof(IAMCrossbar).GUID;
            object ppint = null;
            object obj3 = null;
            int num = graphBuilder.FindInterface(ref upstreamOnly, ref pType, deviceFilter, ref gUID, out ppint);
            while ((num == 0) && (ppint != null))
            {
                if (ppint is IAMCrossbar)
                {
                    list.Add(ppint as IAMCrossbar);
                    num = graphBuilder.FindInterface(ref upstreamOnly, ref pType, ppint as IBaseFilter, ref gUID, out obj3);
                    ppint = obj3;
                }
                else
                {
                    ppint = null;
                }
            }
            return list;
        }

        protected ArrayList findCrossbarSources(ICaptureGraphBuilder2 graphBuilder, IAMCrossbar crossbar, bool isVideoDevice)
        {
            int num;
            int num2;
            ArrayList list = new ArrayList();
            int errorCode = crossbar.get_PinCounts(out num, out num2);
            if (errorCode < 0)
            {
                Marshal.ThrowExceptionForHR(errorCode);
            }
            for (int i = 0; i < num; i++)
            {
                for (int j = 0; j < num2; j++)
                {
                    if (crossbar.CanRoute(i, j) == 0)
                    {
                        int num6;
                        PhysicalConnectorType type;
                        errorCode = crossbar.get_CrossbarPinInfo(true, j, out num6, out type);
                        if (errorCode < 0)
                        {
                            Marshal.ThrowExceptionForHR(errorCode);
                        }
                        CrossbarSource source = new CrossbarSource(crossbar, i, j, type);
                        if (type < PhysicalConnectorType.Audio_Tuner)
                        {
                            if (isVideoDevice)
                            {
                                list.Add(source);
                            }
                            else if (!isVideoDevice)
                            {
                                list.Add(source);
                            }
                        }
                    }
                }
            }
            int index = 0;
            while (index < list.Count)
            {
                bool flag = false;
                CrossbarSource source2 = (CrossbarSource) list[index];
                for (int k = 0; k < list.Count; k++)
                {
                    CrossbarSource source3 = (CrossbarSource) list[k];
                    if ((source2.OutputPin == source3.OutputPin) && (index != k))
                    {
                        flag = true;
                        break;
                    }
                }
                if (flag)
                {
                    index++;
                }
                else
                {
                    list.RemoveAt(index);
                }
            }
            return list;
        }

        internal Source CurrentSource
        {
            get
            {
                foreach (Source source in base.InnerList)
                {
                    if (source.Enabled)
                    {
                        return source;
                    }
                }
                return null;
            }
            set
            {
                if (value == null)
                {
                    foreach (Source source in base.InnerList)
                    {
                        source.Enabled = false;
                    }
                }
                else if (value is CrossbarSource)
                {
                    value.Enabled = true;
                }
                else
                {
                    foreach (Source source2 in base.InnerList)
                    {
                        source2.Enabled = false;
                    }
                    value.Enabled = true;
                }
            }
        }

        public Source this[int index]
        {
            get
            {
                return (Source) base.InnerList[index];
            }
        }
    }
}

