using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace VirusSimulator.WPF
{
    class RenderControl:FrameworkElement
    {
        DrawingGroup dw = new DrawingGroup();
        public RenderControl()
        {
            Random r = new Random();
            var context = dw.Open();
            SolidColorBrush b = new SolidColorBrush(Colors.Red);
            Pen p = new Pen(b, 1);
            for (int i = 0; i < 100000; i++)
            {
                context.DrawRectangle(b, p, new Rect(100, 100, 50, 50));
            }
            context.Close();
            DrawingImage di = new DrawingImage();
            di.Drawing = dw;
            
        }
        protected override void OnRender(DrawingContext drawingContext)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            drawingContext.DrawDrawing(dw);
            base.OnRender(drawingContext);
            sw.Stop();
            Debug.WriteLine(sw.Elapsed);
            //Task.Run(async () =>
            //{
            //    await Task.Delay(33);
            //    await Dispatcher.BeginInvoke((Action)this.InvalidateVisual);
            //});
        }
    }
}
