using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

namespace VirusSimulator.Core.Test
{
    public struct InfectionData
    {
        public int ID;
        public bool IsInfected;
        public bool IsInfectedNext;
    }
    public class TestVirusProcessor<T> : IProcessor<T> where T: notnull,RunContext, IVirusContext
    {
        public float InfectionRadius { get; set; } = 5f;
        QuadTree.QuadTreeNode<PositionItem> index;
        int infected;
        public TestVirusProcessor(int infectedCount)
        {
            infected = infectedCount;
        }
        public void Process(T context, TimeSpan span)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));
            index = PositionItem.CreatePersonQuadTree(RectangleF.FromLTRB(0, 0, context.Size.Width, context.Size.Width));
            for (int i = 0; i < context.VirusData.Items.Span.Length; i++)
            {
                var item = context.VirusData.Items.Span[i];
                if (item.IsInfected)
                {
                    index.AddItem(context.Persons.Items.Span[i]);
                }
            }
            //try to infection
            context.VirusData.ForAll((ref InfectionData data)=>
            {
                if (!data.IsInfected)
                {
                    var result = index.GetItemInDistance(context.Persons.Items.Span[data.ID].Position, InfectionRadius)?.Any();
                    data.IsInfectedNext = (result == true);
                }
            });
            //commit infection
            context.VirusData.ForAllParallel(updateInfection);
        }
        public void Init(T context)
        {
            context.VirusData = new DataBuffer<InfectionData>(context.Persons.Items.Length, context.Persons.Bins,index=>
            {
                return new InfectionData() { ID = index, IsInfected = index < infected };
            });
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
