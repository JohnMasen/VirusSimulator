using System;
using System.Collections.Generic;
using System.Text;

namespace VirusSimulator.Core
{
    public interface IProcessor<T> where T:RunContextBase
    {
        void Process(T context);
    }
}
