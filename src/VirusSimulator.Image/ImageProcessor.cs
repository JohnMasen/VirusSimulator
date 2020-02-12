using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using VirusSimulator.Core;
using VirusSimulator.Core.Processors;

namespace VirusSimulator.Image
{
    public class ImageProcessor<TContext,TPixel>: ProcessorBase<TContext> where TPixel : struct, IPixel<TPixel> where TContext :RunContext
    {
        public List<IImageProcessorPlugIn<TPixel>> Plugins { get; } = new List<IImageProcessorPlugIn<TPixel>>();
        public Image<TPixel> Image { get; private set; }
        private Action<IImageProcessingContext, TContext> draw;
        public ImageProcessor(Action<IImageProcessingContext, TContext> renderCallback)
        {
            draw = renderCallback??throw new ArgumentNullException(nameof(renderCallback));
        }

        public override void Init(TContext context)
        {
            Image = new Image<TPixel>((int)context.Size.Width, (int)context.Size.Height);
            Plugins.ForEach(x => x.Init(Image));
            base.Init(context);
        }

        public override void Process(TContext context, TimeSpan span)
        {
            Image.Mutate(op =>
            {
                draw(op, context);
                Plugins.ForEach(x => x.OnDraw(Image));
            });
        }
        public override void Close(TContext context)
        {
            Plugins.ForEach(x => x.Close(Image));
            base.Close(context);
        }

    }
}
