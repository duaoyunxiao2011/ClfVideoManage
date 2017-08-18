using System;
using DShowNET;

namespace ICameraDll.DirectX.Capture
{
    public class Filters
    {
        public FilterCollection AudioCompressors = new FilterCollection(FilterCategory.AudioCompressorCategory);
        public FilterCollection AudioInputDevices = new FilterCollection(FilterCategory.AudioInputDevice);
        public FilterCollection VideoCompressors = new FilterCollection(FilterCategory.VideoCompressorCategory);
        public FilterCollection VideoInputDevices = new FilterCollection(FilterCategory.VideoInputDevice);
    }
}

