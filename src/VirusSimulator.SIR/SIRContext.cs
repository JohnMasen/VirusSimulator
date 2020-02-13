using System;
using System.Collections.Generic;
using System.Text;
using VirusSimulator.Core;

namespace VirusSimulator.SIR
{
    public class SIRContext : RunContext, ISIRContext
    {
        public DataBuffer<SIRData> SIRInfo { get; private set; }
        public int InitInfected { get; set; } = 10;
        protected override void Init()
        {
            base.Init();
            SIRInfo = new DataBuffer<SIRData>(Persons.Items.Length,0,_=>
            {
                return new SIRData() { Status = SIRData.Person_Susceptible };
            });
        }
    }
}
