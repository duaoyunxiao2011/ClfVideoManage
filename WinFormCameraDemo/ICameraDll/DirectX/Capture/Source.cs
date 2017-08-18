using System;

namespace ICameraDll.DirectX.Capture
{
    public class Source : IDisposable
    {
        protected string name;

        public virtual void Dispose()
        {
            this.name = null;
        }

        ~Source()
        {
            this.Dispose();
        }

        public override string ToString()
        {
            return this.Name;
        }

        public virtual bool Enabled
        {
            get
            {
                throw new NotSupportedException("This method should be overriden in derrived classes.");
            }
            set
            {
                throw new NotSupportedException("This method should be overriden in derrived classes.");
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }
    }
}

