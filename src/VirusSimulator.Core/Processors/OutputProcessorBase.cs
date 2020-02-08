using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Text;

namespace VirusSimulator.Core.Processors
{
    public abstract class OutputProcessorBase<TContext>:IProcessor<TContext>  where TContext:RunContext
    {
        public TimeSpan OutputTimeSpan { get; set; } = TimeSpan.MaxValue;
        public int FrameSkip { get; set; } = 0;
        private int skipCount = 0;
        private long frameCount=0;
        private Stopwatch sw = new Stopwatch();

        public OutputProcessorBase()
        {

            sw.Start();
        }
        protected abstract void Output(TContext context, long frame);
        
        public void Process(TContext context, TimeSpan span)
        {
            frameCount++;

            if (sw.Elapsed > OutputTimeSpan || skipCount >= FrameSkip)
            {

                skipCount = 0;
                Output(context, frameCount);
                sw.Restart();
            }
            else
            {
                skipCount++;
            }
        }

        public virtual void Init(TContext context)
        {
            
        }

        public virtual void Close(TContext context)
        {

        }
    }
}
