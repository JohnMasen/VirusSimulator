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
using VirusSimulator.Core.Test;
using System.Windows.Documents;
using System.Diagnostics;

namespace VirusSimulator.WPF.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        private const int PersonCount = 10000;
        CancellationTokenSource cts;
        List<ScatterPoint> points = new List<ScatterPoint>();

        //private int frameCount = 0;
        //private const int FRAME_SKIP = 100;
        Runner<TestContext> runner;

        private RenderHelper renderHelper;
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
            runner.Processors.Add(new PersonMoveProcessor());
            runner.Processors.Add(new TestVirusProcessor(3) { InfectionRadius = 2f });

            renderHelper = new RenderHelper(renderResult) { FrameSkip = 5 } ;
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
            renderHelper.TryRender(true);
        }

        private void renderResult()
        {
            points.Clear();

            var vData = runner.Context.VirusData.Items.Span;
            foreach (var item in runner.Context.Persons.Items.Span)
            {
                points.Add(new ScatterPoint(item.Position.X, item.Position.Y, double.NaN, vData[item.ID].IsInfected ? 1 : 0));
            }
            //System.Windows.Application.Current.Dispatcher.Invoke(() =>
            //{
            
            //});
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

                renderHelper.TryRender();

            }
        }

        //public List<ScatterPoint> SeriesData { get; } = new List<ScatterPoint>();

        public PlotModel PlotModel { get; } = new PlotModel();

        public int Infected
        {
            get
            {
                int result = 0;
                foreach (var item in runner.Context.VirusData.Items.Span)
                {
                    if (item.IsInfected)
                    {
                        result++;
                    }
                }
                return result;
            }
        }

        public DateTime WorldClock
        {
            get
            {
                return runner.Context.WorldClock;
            }
        }

        private class RenderHelper
        {
            private Stopwatch sw = new Stopwatch();
            public TimeSpan FrameGap { get; set; } = TimeSpan.FromMilliseconds(100);
            public int FrameSkip { get; set; } = int.MaxValue;
            private int skipCount = 0;
            private int frameCount;
            private Action render;
            public RenderHelper(Action renderCallback)
            {
                render = renderCallback;
                sw.Start();
            }
            public bool TryRender(bool forceRender = false)
            {
                frameCount++;

                if (skipCount > FrameSkip)
                {
                    skipCount = 0;
                }
                if (sw.Elapsed > FrameGap || skipCount == 0 || forceRender)
                {

                    skipCount = 1;
                    Debug.WriteLine($"Render frame {frameCount} at Thread {Thread.CurrentThread.ManagedThreadId}");
                    render();
                    sw.Restart();
                    return true;
                }
                else
                {
                    skipCount++;
                    return false;
                }
            }
        }

    }
}
