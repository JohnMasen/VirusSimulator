using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace VirusSimulator.Core.Decorators
{
    public class OutputDecorator<TContext> : ProcessorDecoratorBase<TContext> where TContext : RunContext
    {
        public TimeSpan OutputTimeSpan { get; set; } = TimeSpan.MaxValue;
        public int FrameSkip { get; set; } = 0;
        private int skipCount = 0;
        private long frameCount = 0;
        private Stopwatch sw = new Stopwatch();
        public OutputDecorator(IProcessor<TContext> value) : base(value)
        {
            sw.Start();
        }
        public override void Process(TContext context, TimeSpan span)
        {
            frameCount++;
            if (sw.Elapsed > OutputTimeSpan || skipCount == 0)
            {


                base.Process(context, span);
                sw.Restart();
            }
            if (skipCount++ >= FrameSkip)
            {
                skipCount = 0;
            }
        }
    }
}
