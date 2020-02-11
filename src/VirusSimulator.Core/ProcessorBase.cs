using System;
using System.Collections.Generic;
using System.Text;

namespace VirusSimulator.Core
{
    public abstract class ProcessorBase<T> : IProcessor<T> where T : RunContext
    {
        public bool EnableProcess { get; set; } = true;

        public virtual void Close(T context)
        {

        }
        

        public virtual void Init(T context)
        {

        }


        public abstract void Process(T context, TimeSpan span);
        
    }
}
