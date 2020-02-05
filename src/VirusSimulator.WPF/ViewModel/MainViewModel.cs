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

namespace VirusSimulator.WPF.ViewModel
{
    public class MainViewModel:INotifyPropertyChanged
    {
        Simulator<SimpleArea, SimplePerson> simulator = new Simulator<SimpleArea, SimplePerson>();

        public event PropertyChangedEventHandler PropertyChanged;
        CancellationTokenSource cts;
        List<ScatterPoint> points = new List<ScatterPoint>();
        public MainViewModel()
        {
            simulator.Init(1000, 1000, 1000);
            PlotModel.Series.Add(new ScatterSeries() { ItemsSource = points,MarkerSize=2 });
            PlotModel.Axes.Clear();
            PlotModel.Axes.Add(new LinearAxis() { Minimum = 0, Maximum = 1000 });
            PlotModel.Axes.Add(new LinearAxis() { Minimum = 0, Maximum = 1000, Position = AxisPosition.Bottom });
            var colorAxe = new LinearColorAxis() { Minimum = 0, Maximum = 1, Position = AxisPosition.Top };
            colorAxe.Palette.Colors.Clear();
            colorAxe.Palette.Colors.Add(OxyColor.FromRgb(0, 255, 0));
            colorAxe.Palette.Colors.Add(OxyColor.FromRgb(255, 0, 0));
            
            PlotModel.Axes.Add(colorAxe);
            InitInfection();
        }

        private void InitInfection()
        {
            SimpleVirus virus = new SimpleVirus();
            foreach (var item in simulator.Persons.Take(10))
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
            simulator.Step(TimeSpan.FromDays(1));
            points.Clear();
            foreach (var item in simulator.Persons)
            {
                points.Add(new ScatterPoint(item.Value.Position.X, item.Value.Position.Y, 3, item.Value.Viruses.Count == 0 ? 0 : 1));
            }
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                PlotModel.InvalidatePlot(false);
            });
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Infected)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WorldClock)));
        }

        private async void DoLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                doStep();
                await Task.Delay(30);
            }
        }

        //public List<ScatterPoint> SeriesData { get; } = new List<ScatterPoint>();

        public PlotModel PlotModel { get; } = new PlotModel();

        public int Infected
        {
            get
            {
                return simulator.Persons.Count(x => x.Value.Viruses.Count > 0);
            }
        }

        public DateTime WorldClock
        {
            get
            {
                return simulator.WorldClock;
            }
        }

    }
}
