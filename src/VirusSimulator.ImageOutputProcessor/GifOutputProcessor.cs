using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using VirusSimulator.Core;
using VirusSimulator.Core.Processors;

namespace VirusSimulator.ImageOutputProcessor
{
    public class GIFOutput<T> where T:RunContext
    {
        public Image<Rgba32> Image { get; }
        Action<IImageProcessingContext,T> p;
        bool isFirst = true;
        public GIFOutput(Size outputSize, Action<IImageProcessingContext,T> process)
        {
            Image = new Image<Rgba32>(outputSize.Width, outputSize.Height);
            p = process;
        }

        public void Process(T context, long frameId)
        {
            Image<Rgba32> buffer = new Image<Rgba32>((int)context.Size.Width, (int)context.Size.Height);
            buffer.Mutate(b=>
            {
                p(b, context);
            });
            if (buffer.Width!=Image.Width || buffer.Height!=Image.Height)
            {
                buffer.Mutate((img) =>
                {
                    img.Resize(Image.Width, Image.Height);
                });
            }
            Image.Frames.AddFrame(buffer.Frames[0]);
        }
    }

    public static class GIFOutputHelper
    {
        public static (OutputProcessor<T> processor, Image<Rgba32> Image) AddGIFOutput<T>(this Runner<T> r, Size size, Action<IImageProcessingContext,T> renderCallback) where T : RunContext,new()
        {
            var tmp = new GIFOutput<T>(size, renderCallback);
            OutputProcessor<T> result = new OutputProcessor<T>(tmp.Process);
            r.Processors.Add(result);
            return (result,tmp.Image);
        }
    }
}
