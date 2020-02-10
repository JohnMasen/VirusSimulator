using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Advanced;
using SixLabors.Memory;

namespace VirusSimulator.WPF
{
    class ImageSharpImageSource 
    {
        public WriteableBitmap Source { get; private set; }
        public Image<Bgra32> Image { get; set; }
        private int bufferSize;
        Int32Rect rect;
        public ImageSharpImageSource(int width,int height)
        {
            Source = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
            Image = new Image<Bgra32>(width, height);
            bufferSize = width * height * Marshal.SizeOf<Bgra32>();
            rect = new Int32Rect(0, 0, width, height);
        }   
        //public WriteableBitmap Bitmap { get; private set; } 

        public void Draw(Action<IImageProcessingContext> action )
        {
            Image.Mutate(action);
            Source.Lock();
            //var x=Image.GetPixelSpan().GetPinnableReference();
            //var g=GCHandle.Alloc(x);
            //Memory<Bgra32> mmm = new Memory<Bgra32>();
            //MemoryMarshal
            Marshal.Copy(MemoryMarshal.AsBytes(Image.GetPixelSpan()).ToArray(), 0, Source.BackBuffer, bufferSize);
            Source.AddDirtyRect(rect);
            Source.Unlock();
        }
        
    }
}
