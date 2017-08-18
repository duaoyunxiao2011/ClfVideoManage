using System;

namespace ICameraDll.DirectX.Capture
{
    public class DeviceInUseException : SystemException
    {
        public DeviceInUseException(string deviceName, int hResult) : base(string.Concat(new object[] { deviceName, " is in use or cannot be rendered. (", hResult, ")" }))
        {
        }
    }
}

