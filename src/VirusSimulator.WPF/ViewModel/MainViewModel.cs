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
using System.Windows;
using VirusSimulator.Image;
using VirusSimulator.Image.WPF;
using VirusSimulator.Image.Plugins;
//using System.Windows.Media;

namespace VirusSimulator.WPF.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        public int? FrameSkip { get; set; } = 5;
        public bool? EnableGIFOutput { get; set; } = false;
        public string GifOutputPath { get; set; } = Environment.CurrentDirectory;
        public int? MaxSteps { get; set; } = int.MaxValue;
        public TimeSpan StepGap { get; set; } = TimeSpan.FromHours(1);
        public int PersonCount { get; set; } = 7000;
        public int MapSize { get; set; } = 2000;

        public int POICount { get; set; } = 10;

        public int InfectedInit { get; set; } = 3;
        public float InfectionRate { get; set; } = 0.2f;
        public float InfectionRadias { get; set; } = 2f;
        public long FrameIndex { get; private set; }

        public int? MaxInfectionRate { get; set; } = 80;

        private readonly int defaultFrameSkip = 5;

        public List<DataPoint> HisData { get; } = new List<DataPoint>();
        Runner<TestContext> runner;


        ImageProcessor<TestContext, Bgra32> imageProcessor;
        public System.Windows.Media.Imaging.WriteableBitmap ImageSource { get; private set; }
        public MainViewModel()
        {

        }


        public void DoTest()
        {
            //doStep();
        }

        public void DoTestStart()
        {
            if (runner != null)
            {
                return;
            }
            HisData.Clear();
            runner = new Runner<TestContext>(PersonCount, 0, new System.Drawing.SizeF(MapSize, MapSize));
            //runner.Processors.Add(new TestPersonMoveProcessor<TestContext>());
            //runner.Processors.Add(new RandomMoveProcessor<TestContext>() { Speed = 4 });
            runner.Processors.Add(new PersonMoveProcessor<TestContext>());
            runner.Processors.Add(POIProcessor<TestContext>.CreateRandomPOI(POICount, MapSize / 6));
            runner.Processors.Add(new TestVirusProcessor<TestContext>(InfectedInit) { InfectionRadius = InfectionRadias, InfectionRate = InfectionRate });

            //var r = new SimpleProcessor<TestContext>(renderResult).AsOutput(2);
            //runner.Processors.Add(r);

            imageProcessor = new ImageProcessor<TestContext, Bgra32>(renderImageResult);
            runner.Processors.Add(imageProcessor.AsOutput(FrameSkip.GetValueOrDefault(defaultFrameSkip)));

            var s = new ImageSourceHandler<Bgra32>(MapSize, MapSize, System.Windows.Media.PixelFormats.Bgra32);
            ImageSource = s.ImageSource;
            imageProcessor.Plugins.Add(s);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImageSource)));

            if (EnableGIFOutput == true)
            {
                imageProcessor.Plugins.Add(new GifOutputPlugin<Bgra32>(300, 300, Path.Combine(GifOutputPath, DateTime.Now.ToString("yyyyMMdd_HHMMss") + ".gif")));
            }
            runner.Processors.Add(new SimpleProcessor<TestContext>(updateUI)
                .AsOutput(FrameSkip.GetValueOrDefault(defaultFrameSkip))
                );


            runner.OnStep += Runner_OnStep;

            runner.Context.InitRandomPosition();
            //runner.Context.InitCirclePosition(new System.Numerics.Vector2(runner.Context.Size.Width/2, runner.Context.Size.Height/2), 400);
            runner.Start(StepGap);

        }

        private void raisePropertyChanged(params string[] names)
        {
            if (Application.Current.Dispatcher.Thread.ManagedThreadId == Thread.CurrentThread.ManagedThreadId)
            {
                foreach (var item in names)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(item));
                }
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() => { raisePropertyChanged(names); });
            }
        }
        private void updateUI(TestContext _, TimeSpan __)
        {

            HisData.Add(new DataPoint(FrameIndex, Infected));
            raisePropertyChanged(nameof(FrameIndex), nameof(HisData), nameof(Infected), nameof(WorldClock));
        }

        private void Runner_OnStep(object sender, StepInfo e)
        {
            if (e.FrameIndex >= MaxSteps || (MaxInfectionRate.HasValue && Infected*100 / PersonCount >= MaxInfectionRate))
            {
                e.IsCancel = true;
                runner = null;
            }
            FrameIndex = e.FrameIndex;

        }

        public void DoTestStop()
        {
            runner?.Stop();
            runner = null;

        }
        private void renderImageResult(IImageProcessingContext img, TestContext context)
        {
            img.Fill(Color.White);
            context.Persons.ForAllParallelWtihReference((context as IVirusContext).VirusData, (ref PositionItem p, ref InfectionData infection) =>
            {
                img.Draw(infection.IsInfected == InfectionData.Infected ? Color.Red : Color.Green, 5, new SixLabors.Shapes.RectangularPolygon(p.Position, new SizeF(1, 1)));
            });
        }





        public int Infected
        {
            get
            {
                if (runner == null)
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
                if (runner == null)
                {
                    return DateTime.Today;
                }
                return runner.Context.WorldClock;
            }
        }

    }
}
