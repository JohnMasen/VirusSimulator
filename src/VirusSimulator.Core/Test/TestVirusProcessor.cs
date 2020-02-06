using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
namespace VirusSimulator.Core.Test
{
    public struct InfectionData
    {
        public int ID;
        public bool IsInfected;
        public bool IsInfectedNext;
    }
    public class TestVirusProcessor : IProcessor<TestContext>
    {
        public float InfectionRadius { get; set; } = 5f;
        QuadTree.QuadTreeNode<Person> index;
        TestContext c;
        int infected;
        public TestVirusProcessor(int infectedCount)
        {
            infected = infectedCount;
        }
        public void Process(TestContext context, TimeSpan span)
        {
            c = context;
            index = Person.CreatePersonQuadTree(RectangleF.FromLTRB(0, 0, context.Size.Width, context.Size.Width));
            for (int i = 0; i < context.VirusData.Items.Span.Length; i++)
            {
                var item = context.VirusData.Items.Span[i];
                if (item.IsInfected)
                {
                    index.AddItem(context.Persons.Items.Span[i]);
                }
            }
            context.VirusData.ForAll(tryInfection);
            context.VirusData.ForAllParallel(updateInfection);
        }

        public void Init(TestContext context)
        {
            context.VirusData.ForAllParallel((ref InfectionData d) =>
            {
                if (d.ID<infected)
                {
                    d.IsInfected = true;
                }
            });
        }

        private void tryInfection(ref InfectionData data)
        {
            if (!data.IsInfected)
            {
                var result = index.GetItemInDistance(c.Persons.Items.Span[data.ID].Position, InfectionRadius)?.Any();
                data.IsInfectedNext = (result == true);
            }
        }

        private void updateInfection(ref InfectionData data)
        {
            if (data.IsInfectedNext == true)
            {
                data.IsInfected = true;
            }
        }
    }
}
