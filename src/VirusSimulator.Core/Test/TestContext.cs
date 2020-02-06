using System;
using System.Collections.Generic;
using System.Data;
using System.Numerics;
using System.Text;

namespace VirusSimulator.Core.Test
{
    public class TestContext:RunContext
    {
        public DataBuffer<InfectionData> VirusData { get; private set; }

        
        protected override void Init()
        {
            Persons.ForAll(randomizeItemPosition);
            VirusData = new DataBuffer<InfectionData>(Persons.Items.Length, Persons.Bins);
            VirusData.ForAll(buffer =>
            {
                for (int i = 0; i < buffer.Length; i++)
                {
                    buffer.Span[i].ID = i;
                }
            });
        }
        private void randomizeItemPosition(ref Person person)
        {
            //person.Position = Vector2.One;
            person.MoveTo(Helper.RandomFloat(Size.Width), Helper.RandomFloat(Size.Height));
        }
    }
}
