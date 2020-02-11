using System;
using System.Collections.Generic;
using System.Text;

namespace VirusSimulator.Core.Processors
{
    public class SimpleProcessor<TContext> : ProcessorBase<TContext> where TContext : RunContext
    {
        private Action<TContext, TimeSpan> action;
        public SimpleProcessor(Action<TContext, TimeSpan> process)
        {
            action = process;
        }
        public override void Process(TContext context, TimeSpan span)
        {
            action(context, span);
        }
    }
}
