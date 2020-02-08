using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using System.Diagnostics.CodeAnalysis;
using VirusSimulator.Core;
using VirusSimulator.Core.QuadTree;
using System.Diagnostics;

namespace VirusSimulator.Processor.Test
{
    public struct InfectionData
    {
        public int ID;
        public byte IsInfected;
        public byte IsInfectedNext;
        public const byte Infected = 0x1;
        public const byte NotInfected = 0x0;
    }
    public class TestVirusProcessor<T> : ProcessorBase<T> where T: notnull,RunContext, IVirusContext
    {
        public float InfectionRadius { get; set; } = 5f;
        QuadTreeNode<PositionItem> index;
        int infected;
        public TestVirusProcessor(int infectedCount)
        {
            infected = infectedCount;
        }
        public override void Process(T context, TimeSpan span)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));
            index = PositionItem.CreatePersonQuadTree(RectangleF.FromLTRB(0, 0, context.Size.Width, context.Size.Width));
            for (int i = 0; i < context.VirusData.Items.Span.Length; i++)
            {
                var item = context.VirusData.Items.Span[i];
                if (item.IsInfected==InfectionData.Infected)
                {
                    index.AddItem(context.Persons.Items.Span[i]);
                }
            }
            //try to infection
            context.VirusData.ForAll((ref InfectionData data)=>
            {
                if (data.IsInfected==InfectionData.NotInfected)
                {
                    var result = index.GetItemInDistance(context.Persons.Items.Span[data.ID].Position, InfectionRadius)?.Any();
                    data.IsInfectedNext = (result == true)?InfectionData.Infected:InfectionData.NotInfected;
                }
            });
            //commit infection
            context.VirusData.ForAll((ref InfectionData data) =>
            {
                if (data.IsInfectedNext == InfectionData.Infected)
                {
                    data.IsInfected = InfectionData.Infected;
                }
                data.IsInfected = data.IsInfectedNext;
            });

            //context.VirusData.ForAll()
        }
        public override void Init(T context)
        {
            context.VirusData.ForAllParallel((ref InfectionData inf) =>
            {
                inf.IsInfected = inf.ID < infected ? InfectionData.Infected : InfectionData.NotInfected;
                inf.IsInfectedNext = inf.IsInfected;
            });
        }


    }
}
