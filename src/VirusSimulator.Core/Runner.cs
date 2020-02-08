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
        public List<OutputProcessorBase<T>> OutputProcessors { get; private set; } = new List<OutputProcessorBase<T>>();
        public Runner(int personCount,int bins,SizeF areaSize)
        {
            Context = RunContext.CreateInstance<T>(personCount, DateTime.Now, areaSize, bins);
        }
        
        private void forAllProcessors(Action<IProcessor<T>> action)
        {
            foreach (var item in Processors)
            {
                action(item);
            }
            foreach (var item in OutputProcessors)
            {
                action(item);
            }
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
                    foreach (var item in OutputProcessors)
                    {
                        item.Process(Context, interval);
                    }

                }
                forAllProcessors(x => x.Close(Context));
            }, cts.Token, TaskCreationOptions.None, TaskScheduler.Default);
        }

        public void Stop()
        {
            cts?.Cancel();
            cts = null;
        }

        public void Step(TimeSpan span)
        {
            if (isFirstRun)
            {
                forAllProcessors(x => x.Init(Context));
                isFirstRun = false;
            }
            Context.WorldClock += span;
            foreach (var item in Processors)
            {
                if (item.EnableProcess)
                {
                    item.Process(Context, span);
                }
            }
        }

        
    }
}
