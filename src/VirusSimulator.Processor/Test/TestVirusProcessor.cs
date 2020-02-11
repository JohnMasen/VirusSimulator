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
            indexer = PositionItem.CreatePersonQuadTree(RectangleF.FromLTRB(0, 0, context.Size.Width, context.Size.Width));
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
                     var items = indexer.GetItemInDistance(p.Position, InfectionRadius);
                     bool result;
                     if (items!=null)
                     {
                         result = Helper.RandomFloat(1)<= 1- (Math.Pow((1-InfectionRate), items.Count()));
                     }
                     else
                     {
                         result = false;
                     }
                     //bool result = indexer.GetItemInDistance(p.Position, InfectionRadius)?.Sum()>0;
                     data.IsInfectedNext = (result == true) ? InfectionData.Infected : InfectionData.NotInfected;
                 }
             });
            //commit infection
            context.VirusData.ForAllParallel((ref InfectionData data) =>
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
            context.VirusData.ForAllParallel((int index,ref InfectionData inf) =>
            {
                inf.IsInfected = index < infected ? InfectionData.Infected : InfectionData.NotInfected;
                inf.IsInfectedNext = inf.IsInfected;
            });
        }


    }
}
