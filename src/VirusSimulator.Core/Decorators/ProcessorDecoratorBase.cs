using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace VirusSimulator.Core.Decorators
{
    public abstract class ProcessorDecoratorBase<TContext> : IProcessor<TContext> where TContext : RunContext
    {
        protected readonly IProcessor<TContext> source;
        protected readonly IProcessor<TContext> root;
        public ProcessorDecoratorBase(IProcessor<TContext> value)
        {
            source = value;
            root = (value as ProcessorDecoratorBase<TContext>)?.root??value;
        }

        public bool EnableProcess
        {
            get { return source.EnableProcess; }
            set { source.EnableProcess = value; }
        }

        public virtual void Close(TContext context)
        {
            source.Close(context);
        }

        public virtual void Init(TContext context)
        {
            source.Init(context);
        }

        public virtual void Process(TContext context, TimeSpan span)
        {
            source.Process(context, span);
        }
    }
}
