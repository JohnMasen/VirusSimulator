using System;
using System.Collections.Generic;
using System.Text;

namespace VirusSimulator.Core
{
    public class Runtime
    {
        public IArea Area { get; private set; }
        public Dictionary<int,IPerson> Persons { get; private set; }

        public DateTime WorldClock { get; set; }
        public Runtime(IArea area,DateTime clock)
        {
            area.Runtime = this;
            Area = area;
            Persons = new Dictionary<int, IPerson>();
            WorldClock = clock;
        }
    }
}
