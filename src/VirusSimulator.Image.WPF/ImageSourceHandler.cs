using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using VirusSimulator.Core;
using VirusSimulator.Core.Decorators;

namespace VirusSimulator.Image.WPF
{
    public class ImageSourceHandler<TPixel> :IImageProcessorPlugIn<TPixel> where TPixel:struct,IPixel<TPixel>
    {
        public WriteableBitmap ImageSource { get; private set; }
        private Int32Rect imageArea;
        byte[] buffer;
        public ImageSourceHandler(int width,int height,PixelFormat format) 
        {
            ImageSource = new WriteableBitmap(width, height, 96, 96, format, null);
            imageArea = new Int32Rect(0, 0, width, height);
            buffer = new byte[width * height * format.BitsPerPixel/8];
        }

        public void OnDraw(Image<TPixel> image)
        {
             MemoryMarshal.AsBytes(image.GetPixelSpan()).CopyTo(buffer);
            Application.Current.Dispatcher.Invoke(() =>
            {
                ImageSource.Lock();
                Marshal.Copy(buffer, 0, ImageSource.BackBuffer, buffer.Length);
                ImageSource.AddDirtyRect(imageArea);
                ImageSource.Unlock();
            });

        }

        public void Init(Image<TPixel> image)
        {
        }

        public void Close(Image<TPixel> image)
        {
        }
    }
}
