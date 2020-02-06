using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;

namespace VirusSimulator.Core
{
    public class Runner<T> where T:RunContext,new()
    {
        public List<IProcessor<T>> Processors { get; private set; } = new List<IProcessor<T>>();
        public T Context { get; private set; }
        private bool isFirstRun = true;
        public Runner(int personCount,int bins,SizeF areaSize)
        {
            Context = RunContext.CreateInstance<T>(personCount, DateTime.Now, areaSize, bins);
        }

        

        public void Step(TimeSpan span)
        {
            if (isFirstRun)
            {
                foreach (var item in Processors)
                {
                    item.Init(Context);
                }
                isFirstRun = false;
            }
            Context.WorldClock += span;
            foreach (var item in Processors)
            {
                item.Process(Context,span);
            }
        }

    }
}
