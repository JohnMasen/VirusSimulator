using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VirusSimulator.Core.Processors;
using System.Linq;

namespace VirusSimulator.Core
{
    public class Runner<T> where T:RunContext,new()
    {
        public List<IProcessor<T>> Processors { get; private set; } = new List<IProcessor<T>>();
        public T Context { get; private set; }
        private bool isFirstRun = true;
        public bool IsBusy => cts != null;
        private CancellationTokenSource cts;
        StepInfo stepInfo;
        
        public event EventHandler<StepInfo> OnStep;
        public Runner(int personCount,int bins,SizeF areaSize)
        {
            Context = RunContext.CreateInstance<T>(personCount, DateTime.Now, areaSize, bins);
        }


        public void Start(TimeSpan interval)
        {
            if (cts!=null)
            {
                throw new InvalidOperationException("Cannot start while running, please call Stop first");
            }
            stepInfo = new StepInfo();
            isFirstRun = true;
            runInternal(interval);
        }

        private void runInternal(TimeSpan interval)
        {
            cts = new CancellationTokenSource();
            Task.Factory.StartNew(() =>
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    Step(interval);
                    if (OnStep != null)
                    {
                        OnStep(this, stepInfo);
                        
                        stepInfo.Step();
                        if (stepInfo.IsCancel)
                        {
                            Stop();
                            break;
                        }
                    }

                }
                Processors.ForEach(x => x.Close(Context));
                cts = null;
            }, cts.Token, TaskCreationOptions.None, TaskScheduler.Default);
        }

        public void Stop()
        {
            cts?.Cancel();
        }

        public void Step(TimeSpan span)
        {
            if (isFirstRun)
            {
                Processors.ForEach(x => x.Init(Context));
                isFirstRun = false;
            }
            Context.WorldClock += span;
            Context.StepStart();
            foreach (var item in Processors)
            {
                if (item.EnableProcess)
                {
                    item.Process(Context, span);
                }
            }
            Context.StepEnd();
        }

        
    }
}
