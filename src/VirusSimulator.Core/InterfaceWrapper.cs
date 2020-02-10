using System;
using System.Collections.Generic;
using System.Text;

namespace VirusSimulator.Core
{
    public class InterfaceWrapper<TRoot> where TRoot:InterfaceWrapper<TRoot>
    {
        public InterfaceWrapper()
        {

        }
    }
}
