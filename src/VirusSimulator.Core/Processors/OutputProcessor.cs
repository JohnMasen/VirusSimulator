using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Text;

namespace VirusSimulator.Core.Processors
{
    public class OutputProcessor<TContext>:IProcessor<TContext>  where TContext:RunContext
    {
        public TimeSpan OutputTimeSpan { get; set; } = TimeSpan.FromSeconds(1);
        public int FrameSkip { get; set; } = 0;
        private int skipCount = 0;
        private long frameCount=0;
        private Stopwatch sw = new Stopwatch();
        Action<TContext, long> output;
        public OutputProcessor(Action<TContext, long> callback)
        {
            output = callback;
        }

        public void Process(TContext context, TimeSpan span)
        {
            frameCount++;

            if (skipCount > FrameSkip)
            {
                skipCount = 0;
            }
            if (sw.Elapsed > OutputTimeSpan || skipCount == 0)
            {

                skipCount = 1;
                output(context,frameCount);
                sw.Restart();
            }
            else
            {
                skipCount++;
            }
        }

        public void Init(TContext context)
        {
            
        }
    }
}
