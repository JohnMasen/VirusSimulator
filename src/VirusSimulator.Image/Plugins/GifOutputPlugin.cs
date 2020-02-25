using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VirusSimulator.Image.Plugins
{
    public class GifOutputPlugin<TPixel> : IImageProcessorPlugIn<TPixel> where TPixel : struct, IPixel<TPixel>
    {
        private int w, h;
        private Image<TPixel> gif;
        public string OutputPath { get; private set; }
        public GifOutputPlugin(int width, int height, string outputPath=null)
        {
            w = width;
            h = height;
            if (string.IsNullOrWhiteSpace(outputPath))
            {
                OutputPath = "Output.gif";
            }
            else
            {
                OutputPath = outputPath;
            }
        }

        
            
        public void Close(Image<TPixel> image)
        {
            gif.Frames.RemoveFrame(0);//remove first empty frame
            using (FileStream fs=new FileStream(OutputPath,FileMode.Create))
            {
                gif.SaveAsGif(fs,new SixLabors.ImageSharp.Formats.Gif.GifEncoder() { ColorTableMode = SixLabors.ImageSharp.Formats.Gif.GifColorTableMode.Local });
            }
        }

        public void Init(Image<TPixel> image)
        {
            gif = new Image<TPixel>(w, h);
        }

        public void OnDraw(Image<TPixel> image)
        {
            var f=image.Frames.CloneFrame(0);
            if (w!=f.Width || h!=f.Height)
            {
                f.Mutate(op =>
                {
                    op.Resize(w, h);
                });
            };
            gif.Frames.AddFrame(f.Frames.RootFrame);
        }
    }
}
