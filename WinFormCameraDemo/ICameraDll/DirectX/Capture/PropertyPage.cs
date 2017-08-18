using System;
using System.Windows.Forms;

namespace ICameraDll.DirectX.Capture
{
    public class PropertyPage : IDisposable
    {
        public string Name;
        public bool SupportsPersisting;

        public void Dispose()
        {
        }

        public virtual void Show(Control owner)
        {
            throw new NotSupportedException("Not implemented. Use a derived class. ");
        }

        public virtual byte[] State
        {
            get
            {
                throw new NotSupportedException("This property page does not support persisting state.");
            }
            set
            {
                throw new NotSupportedException("This property page does not support persisting state.");
            }
        }
    }
}

