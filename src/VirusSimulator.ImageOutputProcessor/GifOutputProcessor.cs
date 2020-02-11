using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using System.Text;
using VirusSimulator.Core;
using VirusSimulator.Core.Processors;

namespace VirusSimulator.ImageOutputProcessor
{
    public class GIFOutput<T>:OutputProcessorBase<T> where T:RunContext
    {
        private Image<Rgba32> Image { get; }

        private Action<IImageProcessingContext,T> p;
        private string path;
        public GIFOutput(Size outputSize, Action<IImageProcessingContext,T> renderAction,string outputPath)
        {
            Image = new Image<Rgba32>(outputSize.Width, outputSize.Height);
            p = renderAction;
            path = outputPath;
        }

        
        protected override void Output(T context, long frame)
        {
            Image<Rgba32> buffer = new Image<Rgba32>((int)context.Size.Width, (int)context.Size.Height);
            buffer.Mutate(b =>
            {
                p(b, context);
            });
            if (buffer.Width != Image.Width || buffer.Height != Image.Height)
            {
                buffer.Mutate((img) =>
                {
                    img.Resize(Image.Width, Image.Height);
                });
            }
            Image.Frames.AddFrame(buffer.Frames[0]);

            
            
        }
        public override void Close(T context)
        {
            Image.Frames.RemoveFrame(0);//remove first empty frame;
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                Image.SaveAsGif(fs);
            }
        }


    }

    
}
