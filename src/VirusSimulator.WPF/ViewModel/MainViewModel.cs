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
using VirusSimulator.Core.Test;
using VirusSimulator.Processor.Test;
using VirusSimulator.Processor;

namespace VirusSimulator.WPF.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        public int PersonCount { get; set; } = 10000;
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

            runner = new Runner<TestContext>(PersonCount, 10, new System.Drawing.SizeF(1000, 1000));
            //runner.Processors.Add(new TestPersonMoveProcessor<TestContext>());
            //runner.Processors.Add(new RandomMoveProcessor<TestContext>() { Speed = 1 });
            runner.Processors.Add(new PersonMoveProcessor<TestContext>());
            runner.Processors.Add(new RandomInterestPointProcessor<TestContext>(10) { Radius = 100 });
            runner.Processors.Add(new TestVirusProcessor<TestContext>(3) { InfectionRadius = 2f });
            runner.Processors.Add(new OutputProcessor<TestContext>(renderResult) { FrameSkip = 3, OutputTimeSpan = TimeSpan.FromMilliseconds(33) });
            runner.Context.InitRandomPosition();
        }


        public void DoTest()
        {
            doStep();
        }

        public void DoTestStart()
        {
            if (cts != null)
            {
                return;
            }
            cts = new CancellationTokenSource();
            Task.Factory.StartNew(() =>
            {
                DoLoop(cts.Token);
            }, cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public void DoTestStop()
        {
            cts?.Cancel();
            cts = null;
        }
        private void doStep()
        {
            runner.Step(TimeSpan.FromHours(1));
            renderResult(runner.Context,0);
        }

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

        private void DoLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                runner.Step(TimeSpan.FromHours(1));
            }
        }


        public PlotModel PlotModel { get; } = new PlotModel();

        public int Infected
        {
            get
            {
                return (runner.Context as IVirusContext).GetInfectedCount();
            }
        }

        public DateTime WorldClock
        {
            get
            {
                return runner.Context.WorldClock;
            }
        }

    }
}
