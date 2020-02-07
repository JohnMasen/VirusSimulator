using System;
using System.Collections.Generic;
using System.Text;
using VirusSimulator.Core;

namespace VirusSimulator.Processor.Test
{
    public interface IVirusContext
    {
        public DataBuffer<InfectionData> VirusData { get; internal set; }

        public int GetInfectedCount()
        {
            if (VirusData==null)
            {
                return 0;
            }
            int result = 0;
            foreach (var item in VirusData.Items.Span)
            {
                if (item.IsInfected==InfectionData.Infected)
                {
                    result++;
                }
            }
            return result;
        }
    }
}
