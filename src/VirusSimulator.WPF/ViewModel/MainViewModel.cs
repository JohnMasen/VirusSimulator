﻿using OxyPlot;
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
using VirusSimulator.Core.Test;
using VirusSimulator.Processor.Test;
using VirusSimulator.Processor;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using VirusSimulator.ImageOutputProcessor;
using SixLabors.ImageSharp.Processing;
using System.IO;
using SixLabors.Primitives;

namespace VirusSimulator.WPF.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        public string GIFOutputPath { get; set; } = "d:\\temp\\o.gif";
        public int MaxSteps { get; set; } = 200;
        public TimeSpan StepGap { get; set; } = TimeSpan.FromHours(1);
        public int PersonCount { get; set; } = 3000;
        CancellationTokenSource cts;
        List<ScatterPoint> points = new List<ScatterPoint>();

        Runner<TestContext> runner;
        public MainViewModel()
        {
            PlotModel.Series.Add(new ScatterSeries() { ItemsSource = points, MarkerSize = 1 });
            PlotModel.Axes.Clear();
            PlotModel.Axes.Add(new LinearAxis() { Minimum = 0, Maximum = 1000 });
            PlotModel.Axes.Add(new LinearAxis() { Minimum = 0, Maximum = 1000, Position = AxisPosition.Bottom });
            var colorAxe = new LinearColorAxis() { Minimum = 0, Maximum = 1, Position = AxisPosition.Top };
            colorAxe.Palette.Colors.Clear();
            colorAxe.Palette.Colors.Add(OxyColor.FromRgb(0, 255, 0));
            colorAxe.Palette.Colors.Add(OxyColor.FromRgb(255, 0, 0));
            PlotModel.Axes.Add(colorAxe);

            
        }

        private void drawGifFrame(IImageProcessingContext image, TestContext context)
        {
            image.Fill(Color.White);
            foreach (var item in (context as IVirusContext).VirusData.Items.Span)
            {
                var pos = context.Persons.Items.Span[item.ID].Position;
                image.Draw(item.IsInfected==InfectionData.Infected?Color.Red:Color.Green, 1, new SixLabors.Shapes.RectangularPolygon(pos,new SizeF(3,3)));
            }
        }

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
            runner = new Runner<TestContext>(PersonCount, 10, new System.Drawing.SizeF(1000, 1000));
            //runner.Processors.Add(new TestPersonMoveProcessor<TestContext>());
            //runner.Processors.Add(new RandomMoveProcessor<TestContext>() { Speed = 4 });
            runner.Processors.Add(new PersonMoveProcessor<TestContext>());
            runner.Processors.Add(POIProcessor<TestContext>.CreateRandomPOI(10, 100));
            runner.Processors.Add(new TestVirusProcessor<TestContext>(3) { InfectionRadius = 2f });

            var r = new SimpleOutputProcessor<TestContext>(renderResult) { FrameSkip = 10, OutputTimeSpan = TimeSpan.FromMilliseconds(10000) };
            runner.OutputProcessors.Add(r);

            var tmp = new GIFOutput<TestContext>(new SixLabors.Primitives.Size(300, 300), drawGifFrame,GIFOutputPath);
            tmp.FrameSkip = r.FrameSkip;
            tmp.OutputTimeSpan = r.OutputTimeSpan;
            runner.OnStep += Runner_OnStep;

            runner.Context.InitRandomPosition();
            runner.Start(StepGap);
            //if (cts != null)
            //{
            //    return;
            //}
            //cts = new CancellationTokenSource();
            //Task.Factory.StartNew(() =>
            //{
            //    DoLoop(cts.Token);
            //}, cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
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
        }
        //private void doStep()
        //{
            
        //}

        private void renderResult(TestContext c,long frameCount)
        {
            points.Clear();

            var vData = (c as IVirusContext).VirusData.Items.Span;
            foreach (var item in c.Persons.Items.Span)
            {
                points.Add(new ScatterPoint(item.Position.X, item.Position.Y, double.NaN, vData[item.ID].IsInfected==InfectionData.Infected ? 1 : 0));
            }
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                PlotModel.InvalidatePlot(false);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WorldClock)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Infected)));
            });

        }

        //private void DoLoop(CancellationToken token)
        //{
        //    int currentStep = 0;
        //    while (!token.IsCancellationRequested && currentStep++<MaxSteps)
        //    {
        //        runner.Step(TimeSpan.FromHours(1));
        //    }
        //    using (FileStream fs=new FileStream("d:\\temp\\o.gif", FileMode.Create))
        //    {
        //        Debug.WriteLine("begin output gif");
        //        gifImage.Frames.RemoveFrame(0);
        //        gifImage.SaveAsGif(fs);
        //        Debug.WriteLine("Gif output complete");
        //    }
            
        //}


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
