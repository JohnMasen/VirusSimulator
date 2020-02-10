using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Text;

namespace VirusSimulator.ImageSharpOutput
{
    public interface IImageProcessorPlugIn<TPixel> where TPixel:struct,IPixel<TPixel>
    {
        public void OnDraw(Image<TPixel> image);

        public void Init(Image<TPixel> image);

        public void Close(Image<TPixel> image);
    }
}
