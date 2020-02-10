using OxyPlot;
using OxyPlot.Axes;
//using OxyPlot.Wpf;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using VirusSimulator.Core;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Documents;
using System.Diagnostics;
using VirusSimulator.Core.Processors;
using VirusSimulator.Processor.Test;
using VirusSimulator.Processor;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.IO;
using SixLabors.Primitives;
using VirusSimulator.ImageSharpOutput;
using VirusSimulator.ImageSharpOutput.WPF;
using System.Windows;
//using System.Windows.Media;

namespace VirusSimulator.WPF.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        public string GIFOutputPath { get; set; } = "d:\\temp\\o.gif";
        public int MaxSteps { get; set; } = int.MaxValue;
        public TimeSpan StepGap { get; set; } = TimeSpan.FromHours(1);
        public int PersonCount { get; set; } = 7000;
        List<ScatterPoint> points = new List<ScatterPoint>();
        public int MapSize { get; set; } = 2000;

        Runner<TestContext> runner;


        ImageSharpProcessor<TestContext, Bgra32> imageProcessor;
        public System.Windows.Media.Imaging.WriteableBitmap ImageSource { get; private set; }
        public MainViewModel()
        {
            //PlotModel.Background = OxyColors.Black;
            PlotModel.Series.Add(new ScatterSeries() { ItemsSource = points, MarkerSize = 1 });
            PlotModel.Axes.Clear();
            PlotModel.Axes.Add(new LinearAxis() { Minimum = 0, Maximum = MapSize });
            PlotModel.Axes.Add(new LinearAxis() { Minimum = 0, Maximum = MapSize, Position = AxisPosition.Bottom });
            var colorAxe = new LinearColorAxis() { Minimum = 0, Maximum = 1, Position = AxisPosition.Top };
            colorAxe.Palette.Colors.Clear();
            colorAxe.Palette.Colors.Add(OxyColor.FromRgb(0,255,0));
            colorAxe.Palette.Colors.Add(OxyColors.Red);
            PlotModel.Axes.Add(colorAxe);
            //ImageSource = new System.Windows.Media.Imaging.WriteableBitmap(100, 100, 96, 96, System.Windows.Media.PixelFormats.Bgra32, null);
        }

        //private void drawGifFrame(IImageProcessingContext image, TestContext context)
        //{
        //    image.Fill(Color.White);

        //    (context as IVirusContext).VirusData.ForAllWtihReference(context.Persons, (ref InfectionData infection, ref PositionItem p) =>
        //     {
        //         image.Draw(infection.IsInfected == InfectionData.Infected ? Color.Red : Color.Green, 1, new SixLabors.Shapes.RectangularPolygon(p.Position, new SizeF(3, 3)));
        //     });
        //    //foreach (var item in (context as IVirusContext).VirusData.Items.Span)
        //    //{
        //    //    var pos = context.Persons.Items.Span[item.Index].Position;
        //    //    image.Draw(item.IsInfected==InfectionData.Infected?Color.Red:Color.Green, 1, new SixLabors.Shapes.RectangularPolygon(pos,new SizeF(3,3)));
        //    //}
        //}

        public void DoTest()
        {
            //doStep();
        }

        public void DoTestStart()
        {
            if (runner!=null)
            {
                return;
            }
            runner = new Runner<TestContext>(PersonCount, 0, new System.Drawing.SizeF(MapSize, MapSize));
            //runner.Processors.Add(new TestPersonMoveProcessor<TestContext>());
            //runner.Processors.Add(new RandomMoveProcessor<TestContext>() { Speed = 4 });
            runner.Processors.Add(new PersonMoveProcessor<TestContext>());
            runner.Processors.Add(POIProcessor<TestContext>.CreateRandomPOI(10, 400));
            runner.Processors.Add(new TestVirusProcessor<TestContext>(3) { InfectionRadius = 2f });

            var r = new SimpleProcessor<TestContext>(renderResult).AsOutput(2);
            runner.Processors.Add(r);

            imageProcessor = new ImageSharpProcessor<TestContext,Bgra32>(renderImageResult);
            var s = new ImageSourceHandler<Bgra32>(MapSize, MapSize, System.Windows.Media.PixelFormats.Bgra32);
            imageProcessor.Plugins.Add(s);
            runner.Processors.Add(imageProcessor.AsOutput(20));
            ImageSource = s.ImageSource;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImageSource)));
            


            //var tmp = new GIFOutput<TestContext>(new SixLabors.Primitives.Size(300, 300), drawGifFrame,GIFOutputPath);
            //tmp.FrameSkip = r.FrameSkip;
            //tmp.OutputTimeSpan = r.OutputTimeSpan;
            //runner.OutputProcessors.Add(tmp);

            runner.OnStep += Runner_OnStep;

            runner.Context.InitRandomPosition();
            //runner.Context.InitCirclePosition(new System.Numerics.Vector2(runner.Context.Size.Width/2, runner.Context.Size.Height/2), 400);
            runner.Start(StepGap);
            
        }

        private void Runner_OnStep(object sender, StepInfo e)
        {
            if (e.FrameIndex>=MaxSteps)
            {
                e.IsCancel = true;
            }
        }

        public void DoTestStop()
        {
            runner?.Stop();
            runner = null;
            //using (var fs=new FileStream("d:\\temp\\test1.bmp",FileMode.Create))
            //{
            //    imageProcessor.Image.SaveAsBmp(fs);
            //}
            //byte[] b = new byte[MapSize * MapSize * 4];
            //ImageSource.CopyPixels(b, MapSize * 4, 0);
            //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImageSource)));

        }
        private void renderImageResult(IImageProcessingContext img,TestContext context)
        {
            img.Fill(Color.Black);
            context.Persons.ForAllParallelWtihReference((context as IVirusContext).VirusData, (ref PositionItem p, ref InfectionData infection) =>
            {
                img.Draw(infection.IsInfected == InfectionData.Infected ? Color.Red : Color.Green,5, new SixLabors.Shapes.RectangularPolygon(p.Position, new SizeF(1, 1)));
                //points.Add(new ScatterPoint(p.Position.X, p.Position.Y, double.NaN, infection.IsInfected == InfectionData.Infected ? 1 : 0));
            });
        }
        

        private void renderResult(TestContext c,TimeSpan span)
        {
            points.Clear();

            //var vData = (c as IVirusContext).VirusData.Items.Span;
            //foreach (var item in c.Persons.Items.Span)
            //{
            //    points.Add(new ScatterPoint(item.Position.X, item.Position.Y, double.NaN, vData[item.Index].IsInfected==InfectionData.Infected ? 1 : 0));
            //}

            c.Persons.ForAllWtihReference((c as IVirusContext).VirusData, (ref PositionItem p, ref InfectionData infection) =>
             {
                 points.Add(new ScatterPoint(p.Position.X, p.Position.Y, double.NaN, infection.IsInfected == InfectionData.Infected ? 1 : 0));
             });
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                PlotModel.InvalidatePlot(false);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WorldClock)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Infected)));
            });

        }

       
        public PlotModel PlotModel { get; } = new PlotModel();

        public int Infected
        {
            get
            {
                if (runner==null)
                {
                    return 0;
                }
                return (runner.Context as IVirusContext).GetInfectedCount();
            }
        }

        public DateTime WorldClock
        {
            get
            {
                if (runner==null)
                {
                    return DateTime.Today;
                }
                return runner.Context.WorldClock;
            }
        }

    }
}
