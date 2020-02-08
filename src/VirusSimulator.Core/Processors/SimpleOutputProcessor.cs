using System;
using System.Collections.Generic;
using System.Text;

namespace VirusSimulator.Core.Processors
{
    public class SimpleOutputProcessor<T> : OutputProcessorBase<T> where T : RunContext
    {
        Action<T, long> action;
        public SimpleOutputProcessor(Action<T,long> outputAction)
        {
            action = outputAction;
        }
        protected override void Output(T context, long frame)
        {
            action(context, frame);
        }
    }
}
