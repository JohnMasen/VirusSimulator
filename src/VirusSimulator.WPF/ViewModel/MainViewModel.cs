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
using VirusSimulator.Area;
using VirusSimulator.Core;
using VirusSimulator.Person;
using System.Linq;
using VirusSimulator.Virus;
using System.Runtime.CompilerServices;
using VirusSimulator.Core.Test;
using System.Windows.Documents;

namespace VirusSimulator.WPF.ViewModel
{
    public class MainViewModel:INotifyPropertyChanged
    {
        Simulator<SimpleArea, SimplePerson> simulator = new Simulator<SimpleArea, SimplePerson>();

        public event PropertyChangedEventHandler PropertyChanged;
        CancellationTokenSource cts;
        List<ScatterPoint> points = new List<ScatterPoint>();

        private int frameCount = 0;
        private const int FRAME_SKIP = 1000;
        Runner<TestContext> runner;
        public MainViewModel()
        {
            simulator.Init(1000, 1000, 3000);
            PlotModel.Series.Add(new ScatterSeries() { ItemsSource = points,MarkerSize=2 });
            PlotModel.Axes.Clear();
            PlotModel.Axes.Add(new LinearAxis() { Minimum = 0, Maximum = 1000 });
            PlotModel.Axes.Add(new LinearAxis() { Minimum = 0, Maximum = 1000, Position = AxisPosition.Bottom });
            var colorAxe = new LinearColorAxis() { Minimum = 0, Maximum = 1, Position = AxisPosition.Top };
            colorAxe.Palette.Colors.Clear();
            colorAxe.Palette.Colors.Add(OxyColor.FromRgb(0, 255, 0));
            colorAxe.Palette.Colors.Add(OxyColor.FromRgb(255, 0, 0));
            
            //PlotModel.Axes.Add(colorAxe);
            InitInfection();
            runner = new Runner<TestContext>(5000, 10, new System.Drawing.SizeF(1000, 1000));
            runner.Processors.Add(new PersonMoveProcessor());

        }

        private void InitInfection()
        {
            SimpleVirus virus = new SimpleVirus();
            foreach (var item in simulator.Runtime.Persons.Take(10))
            {
                virus.Infect(null, item.Value);
            }
        }
        public void DoTest()
        {
            doStep();
        }

        public void DoTestStart()
        {
            if (cts!=null)
            {
                return;
            }
            cts = new CancellationTokenSource();
            Task.Factory.StartNew(() =>
            {
                DoLoop(cts.Token);
            },cts.Token);
        }

        public void DoTestStop()
        {
            cts?.Cancel();
            cts = null;
        }
        private void doStep()
        {
            //simulator.Step(TimeSpan.FromDays(1));
            runner.Step();
            renderResult();
        }

        private void renderResult()
        {
            points.Clear();

            //foreach (var item in simulator.Runtime.Persons)
            //{
            //    points.Add(new ScatterPoint(item.Value.Position.X, item.Value.Position.Y, 3, item.Value.Viruses.Count == 0 ? 0 : 1));
            //}
            //System.Windows.Application.Current.Dispatcher.Invoke(() =>
            //{
            //    PlotModel.InvalidatePlot(false);
            //});
            //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Infected)));
            //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WorldClock)));

            foreach (var item in runner.Context.Persons.Items.Span)
            {
                points.Add(new ScatterPoint(item.Position.X, item.Position.Y, 3));
            }
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                PlotModel.InvalidatePlot(false);
            });
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WorldClock)));
        }

        private async void DoLoop(CancellationToken token)
        {
            //while (!token.IsCancellationRequested)
            //{
            //    simulator.Step(TimeSpan.FromDays(1));
            //    if (frameCount==0)
            //    {
            //        renderResult();
            //    }
            //    frameCount++;
            //    if (frameCount>FRAME_SKIP)
            //    {
            //        frameCount = 0;
            //    }
            //    await Task.Delay(30);
            //}
            while (!token.IsCancellationRequested)
            {
                runner.Step();
                if (frameCount == 0)
                {
                    renderResult();
                    await Task.Delay(33);
                }
                frameCount++;
                if (frameCount > FRAME_SKIP)
                {
                    frameCount = 0;
                }
                
            }
        }

        //public List<ScatterPoint> SeriesData { get; } = new List<ScatterPoint>();

        public PlotModel PlotModel { get; } = new PlotModel();

        public int Infected
        {
            get
            {
                return simulator.Runtime.Persons.Count(x => x.Value.Viruses.Count > 0);
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
