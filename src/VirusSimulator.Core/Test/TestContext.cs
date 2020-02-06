using System;
using System.Collections.Generic;
using System.Data;
using System.Numerics;
using System.Text;

namespace VirusSimulator.Core.Test
{
    public class TestContext : RunContext, IVirusContext
    {
        DataBuffer<InfectionData> IVirusContext.VirusData { get; set; }
    }
}
