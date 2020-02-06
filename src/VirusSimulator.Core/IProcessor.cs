using System;
using System.Collections.Generic;
using System.Text;

namespace VirusSimulator.Core
{
    public interface IProcessor<T> where T:RunContext
    {
        void Process(T context,TimeSpan span);

        void Init(T context);
    }
}
