using System;
using System.Collections.Generic;
using System.Text;

namespace VirusSimulator.Core.Test
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
                if (item.IsInfected)
                {
                    result++;
                }
            }
            return result;
        }
    }
}
