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
using System.Numerics;
using VirusSimulator.SIR;
using Microsoft.Win32;
using VirusSimulator.Core.QuadTree;
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

        public int POICount { get; set; } = 1000;

        public int InfectedInit { get; set; } = 10;
        public float InfectionRate { get; set; } = 0.4f;
        public float InfectionRadius { get; set; } = 3f;
        public long FrameIndex { get; private set; }
        public float PersonActivity { get; set; } = 0.1f;
        public int? MaxInfectionRate { get; set; } = 80;

        private readonly int defaultFrameSkip = 5;

        public bool? EnableRealtimeOutput { get; set; } = true;
        public int? GroundPoolSize { get; set; } = 200;

        public List<DataPoint> HisData { get; } = new List<DataPoint>();

        public List<ColumnItem> SIRRunningTotal { get; } = new List<ColumnItem>();

        public IEnumerable<DataPoint> RecentHisData
        {
            get
            {
                return HisData.TakeLast(maxHisSteps);
            }
        }

        Runner<TestContext> runner;

        private List<Vector2> startupPoints;

        ImageProcessor<TestContext, Bgra32> imageProcessor;
        public System.Windows.Media.Imaging.WriteableBitmap ImageSource { get; private set; }

        private int maxHisSteps = 100;
        public string PointsInitSource { get; set; }

        public string RunStatus { get; private set; }

        private Stopwatch sw = new Stopwatch();

        private long fps;

        private Queue<TestContext> runHistory = new Queue<TestContext>();

        public bool EnableAutoStop { get; set; } = true;
        private POIProcessor<TestContext> poiProcessor;

        public MainViewModel()
        {
            //ImagePositionLoader il = new ImagePositionLoader("d:\\temp\\test1.bmp", MapSize);
            //il.TestOutput("d:\\temp\\test2.bmp", MapSize, 7000);
            initStartup();
        }

        private void initStartup()
        {
            startupPoints = new List<Vector2>(PersonCount);
            if (!string.IsNullOrWhiteSpace(PointsInitSource) && File.Exists(PointsInitSource))
            {
                ImagePositionLoader ipl = new ImagePositionLoader(PointsInitSource, MapSize);
                foreach (var item in ipl.GetRandomPoints(PersonCount))
                {
                    startupPoints.Add(item);
                }
            }
            //init random
            for (int i = 0; i < PersonCount; i++)
            {
                startupPoints.Add(new Vector2(Helper.RandomFloat(MapSize), Helper.RandomFloat(MapSize)));
            }
        }

        public void SaveCSV()
        {
            if (runHistory.Count==0)
            {
                return;
            }
            SaveFileDialog sfd = new SaveFileDialog()
            {
                AddExtension = true,
                Filter = "CSVFiles|*.csv",
                FilterIndex = 0
            };
            if(sfd.ShowDialog()==true)
            {
                updateStatus("Saving CSV...");
                using (StreamWriter writer=new StreamWriter(sfd.FileName,false))
                {
                    int frameId = 0;
                    
                    writer.WriteLine("FrameID,Susceptible,Infective,Grounded,Recovered,MovingPerson");
                    foreach (var item in runHistory)
                    {
                        int infective = 0;
                        int susceptible = 0;
                        int grounded = 0;
                        int recovered = 0;
                        int movingPerson = 0;
                        item.SIRInfo.ForAllParallelWtihReference(item.Persons, (int index,ref SIRData sir, ref PositionItem pos) =>
                         {
                             switch (sir.Status)
                             {
                                 case SIRData.Infective:
                                     Interlocked.Increment(ref infective);
                                     break;
                                 case SIRData.Susceptible:
                                     Interlocked.Increment(ref susceptible);
                                     break;
                                 case SIRData.Grounded:
                                     Interlocked.Increment(ref grounded);
                                     break;
                                 case SIRData.Recovered:
                                     Interlocked.Increment(ref recovered);
                                     break;
                                 default:
                                     break;
                             }
                             if(item.MoveStatus.Items.Span[index].IsMovingToTarget==MovingStatusEnum.Moving)
                             {
                                 Interlocked.Increment(ref movingPerson);
                             }

                         });
                        writer.WriteLine($"{frameId++},{susceptible},{infective},{grounded},{recovered},{movingPerson}");
                    }
                    
                }
                updateStatus($"CSV Saved to {sfd.FileName}");


            }
        }

        private void updateStatus(string value)
        {
            RunStatus = value;
            raisePropertyChanged(nameof(RunStatus));
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
            runHistory.Clear();
            runner = new Runner<TestContext>(PersonCount, 0, new System.Drawing.SizeF(MapSize, MapSize));
            //runner.Processors.Add(new TestPersonMoveProcessor<TestContext>());
            //runner.Processors.Add(new RandomMoveProcessor<TestContext>() { Speed = 4 });
            runner.Processors.Add(new PersonMoveProcessor<TestContext>());

            //ImagePositionLoader p = new ImagePositionLoader("d:\\temp\\map1_poi.jpg", MapSize);
            //poiProcessor = POIProcessor<TestContext>.CreateFromPoints(p.GetRandomPoints(POICount));
            poiProcessor = initPOI();
            poiProcessor.POIScanRadiusLarge = MapSize / 6;
            poiProcessor.POIScanRadiusSmall = MapSize/6/10;
            poiProcessor.Activity = PersonActivity;
            runner.Processors.Add(poiProcessor);
            //runner.Processors.Add(new TestVirusProcessor<TestContext>(InfectedInit) { InfectionRadius = InfectionRadias, InfectionRate = InfectionRate });
            runner.Processors.Add(new SIRProcessor<TestContext>(InfectedInit) { InfectionRadius = InfectionRadius, InfectionRate = InfectionRate,GroundPoolSize=GroundPoolSize.Value });
            //var r = new SimpleProcessor<TestContext>(renderResult).AsOutput(2);
            //runner.Processors.Add(r);



            if (EnableRealtimeOutput == true || EnableGIFOutput == true)
            {
                imageProcessor = new ImageProcessor<TestContext, Bgra32>(renderImageResult);
                runner.Processors.Add(imageProcessor.AsOutput(FrameSkip.GetValueOrDefault(defaultFrameSkip)));
                var s = new ImageSourceHandler<Bgra32>(MapSize, MapSize, System.Windows.Media.PixelFormats.Bgra32);

                if (EnableRealtimeOutput == true)
                {
                    ImageSource = s.ImageSource;
                    imageProcessor.Plugins.Add(s);
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImageSource)));
                }
                if (EnableGIFOutput == true)
                {
                    imageProcessor.Plugins.Add(new GifOutputPlugin<Bgra32>(MapSize/3,MapSize/3 ,  Path.Combine(GifOutputPath, DateTime.Now.ToString("yyyyMMdd_HHMMss") + ".gif")));
                }


            }
            runner.Processors.Add(new SimpleProcessor<TestContext>(updateUI)
                    .AsOutput(FrameSkip.GetValueOrDefault(defaultFrameSkip))
                    );

            runner.OnStep += Runner_OnStep;

            initStartup();
            runner.Context.Persons.ForAllParallel((int index, ref PositionItem item) =>
            {
                item.Move(startupPoints[index]);
            });
            sw.Reset();
            sw.Start();
            
            runner.Start(StepGap);

        }

        private POIProcessor<TestContext> initPOI()
        {
            if (string.IsNullOrEmpty(PointsInitSource))
            {
                return POIProcessor<TestContext>.CreateRandomPOI(POICount, PersonActivity);
            }
            else
            {
                string ex = Path.GetExtension(PointsInitSource);
                string path=Path.ChangeExtension(PointsInitSource, $"poi.{ex}");
                if (File.Exists(path))
                {
                    ImagePositionLoader ipl = new ImagePositionLoader(path, MapSize);
                    return POIProcessor<TestContext>.CreateFromPoints(ipl.GetRandomPoints(POICount));
                }
                else
                {
                    return POIProcessor<TestContext>.CreateRandomPOI(POICount, PersonActivity);
                }
            }
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
            long duration = (long)sw.Elapsed.TotalSeconds;
            if (duration == 0)
            {
                return;
            }

            SIRRunningTotal.Clear();
            SIRRunningTotal.Add(new ColumnItem(Statistics.Infective));
            SIRRunningTotal.Add(new ColumnItem(Statistics.Grounded));

            fps = FrameIndex / (long)sw.Elapsed.TotalSeconds;
            updateStatus($"Running {sw.Elapsed} fps={fps}");
            HisData.Add(new DataPoint(FrameIndex, Statistics.Infective));
            raisePropertyChanged(nameof(FrameIndex), nameof(HisData), nameof(RecentHisData), nameof(Statistics), nameof(WorldClock),nameof(SIRRunningTotal));
        }

        private void Runner_OnStep(object sender, StepInfo e)
        {
            refreshStatistics();


            
            int infectedCount = Statistics.Infective;
            if (EnableAutoStop && (e.FrameIndex >= MaxSteps
                || MaxInfectionRate.HasValue && infectedCount * 100 / PersonCount >= MaxInfectionRate
                || infectedCount == 0))
            {
                e.IsCancel = true;
                runner = null;
                sw.Stop();
                updateStatus($"Stopped Duration={sw.Elapsed} fps={fps}");
                return;
            }
            FrameIndex = e.FrameIndex;
            runHistory.Enqueue((sender as Runner<TestContext>).Context.Clone());
        }

        public void DoTestStop()
        {
            runner?.Stop();
            runner = null;
            sw.Stop();
            updateStatus($"Stopped Duration={sw.Elapsed} fps={fps}");
        }
        private void renderImageResult(IImageProcessingContext img, TestContext context)
        {
            img.Fill(Color.Black);
            //byte mask = SIRData.CanInfect | SIRData.CanInfectOthers;
            context.Persons.ForAllParallelWtihReference(context.SIRInfo, (ref PositionItem p, ref SIRData infection) =>
            {
                if (infection.Status == SIRData.Susceptible || infection.Status == SIRData.Infective)//can infect others or can be infected
                {
                    Color c;
                    if (infection.Status == SIRData.Susceptible)
                    {
                        c = Color.Green;
                    }
                    else
                    {
                        if (infection.GroundCountdown == TimeSpan.Zero)
                        {
                            c = Color.Red;
                        }
                        else
                        {
                            c = Color.Yellow;
                        }
                    }
                    img.Draw(c, 5, new SixLabors.Shapes.RectangularPolygon(p.Position, new SizeF(1, 1)));
                }


            });
            //    context.Persons.ForAllParallelWtihReference(context.POIData, (ref PositionItem p, ref POIInfo poi) =>
            //{
            //    Color c;
            //    c = poi.POIStatus switch
            //    {
            //        POIStatusEnum.AtHome => Color.White,
            //        POIStatusEnum.FromHomeToPOI => Color.Green,
            //        POIStatusEnum.FromPOIToPOI => Color.Yellow,
            //        POIStatusEnum.GoHome => Color.Red
            //    };
            //    img.Draw(c, 5, new SixLabors.Shapes.RectangularPolygon(p.Position, new SizeF(10, 10)));


            //});

            //render POIInfo
            //context.POIData.ForAllParallel((ref POIInfo poi) =>
            //{
            //    img.Draw(Color.Yellow, 5, new SixLabors.Shapes.RectangularPolygon(poi.HomePosition, new SizeF(1, 1)));
            //});
            //foreach (var item in poiProcessor.POIList)
            //{
            //    img.Draw(Color.Red, 5, new SixLabors.Shapes.RectangularPolygon(item, new SizeF(1, 1)));
            //}
            //render POI
            //context.Persons.ForAllParallel((int index, ref PositionItem pos) =>
            //{
            //    foreach (var item in poiProcessor.POIIndex.GetItemsInDistance(pos.Position, poiProcessor.POIScanRadiusLarge))
            //    {
            //        img.Draw(Color.Yellow, 5, new SixLabors.Shapes.RectangularPolygon(item, new SizeF(1, 1)));
            //    }
            //});
            //drawQuadTree(img,poiProcessor.POIIndex);
        }

        private void drawQuadTree(IImageProcessingContext img,QuadTreeNode<Vector2> q)
        {
            if (q.IsLeaf)
            {
                img.Draw(Color.White,1, RectangleF.FromLTRB(q.Range.Left,q.Range.Top,q.Range.Right,q.Range.Bottom));
            }
            else
            {
                foreach (var item in q.Children)
                {
                    drawQuadTree(img, item);
                }
            }
        }


        private void refreshStatistics()
        {
            if (runner==null)
            {
                return;
            }
            Statistics = (runner.Context as ISIRContext).GetCount();
        }


        public SIRDataStatistics Statistics { get; private set; } = SIRDataStatistics.ZERO;
        public string[] ColumnCategories { get; } = new string[2] { nameof(SIRData.Infective), nameof(SIRData.Grounded) };

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
