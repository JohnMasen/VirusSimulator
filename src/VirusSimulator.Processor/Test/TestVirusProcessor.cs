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
        public byte IsInfected;
        public byte IsInfectedNext;
        public const byte Infected = 0x1;
        public const byte NotInfected = 0x0;
    }
    public class TestVirusProcessor<T> : ProcessorBase<T> where T: notnull,RunContext, IVirusContext
    {
        public float InfectionRate { get; set; } = 0.2f;
        public float InfectionRadius { get; set; } = 5f;
        QuadTreeNode<PositionItem> indexer;
        int infected;
        public TestVirusProcessor(int infectedCount)
        {
            infected = infectedCount;
        }
        public override void Process(T context, TimeSpan span)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));
            indexer.Clear();
            for (int i = 0; i < context.VirusData.Items.Span.Length; i++)
            {
                var item = context.VirusData.Items.Span[i];
                if (item.IsInfected==InfectionData.Infected)
                {
                    indexer.AddItem(context.Persons.Items.Span[i]);
                }
            }
            //try to infection

            context.VirusData.ForAllParallelWtihReference(context.Persons, (ref InfectionData data, ref PositionItem p) =>
             {
                 if (data.IsInfected == InfectionData.NotInfected)
                 {
                     int count = indexer.GetItemsCountInDistance(p.Position, InfectionRadius);
                     if (count>0 && Helper.RandomFloat(1) <= 1 - (Math.Pow((1 - InfectionRate), count)))
                     {
                         data.IsInfected = InfectionData.Infected;
                     }
                     else
                     {
                         data.IsInfected = InfectionData.NotInfected;
                     }
                 }
             });
        }
        public override void Init(T context)
        {
            context.VirusData.ForAllParallel((int index,ref InfectionData inf) =>
            {
                inf.IsInfected = index < infected ? InfectionData.Infected : InfectionData.NotInfected;
                inf.IsInfectedNext = inf.IsInfected;
            });
            indexer = PositionItem.CreatePersonQuadTree(RectangleF.FromLTRB(0, 0, context.Size.Width, context.Size.Width));
        }


    }
}
