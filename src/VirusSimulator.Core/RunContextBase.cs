using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace VirusSimulator.Core
{
    public abstract class RunContextBase
    {
        public DataBuffer<Person> Persons { get; internal set; }
        public DateTime WorldClock { get; private set; } = DateTime.Now;
        public SizeF Size { get;  set; }
    }
}
