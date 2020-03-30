using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using VirusSimulator.Core;
using VirusSimulator.Core.QuadTree;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Numerics;

namespace VirusSimulator.SIR
{
    public class SIRProcessor<T> : ProcessorBase<T> where T : RunContext, ISIRContext
    {
        public float InfectionRadius { get; set; } = 2f;

        public float InfectionRate { get; set; } = 0.1f;

        public TimeSpan GroundDelay { get; set; } = TimeSpan.FromDays(7);

        public TimeSpan CureDelay { get; set; } = TimeSpan.FromDays(7);

        public int GroundPoolSize { get; set; } = 200;

        private int initWithInfected;
        private int groundPoolRemain;
        public SIRProcessor(int initInfected)
        {
            initWithInfected = initInfected;
        }

        public TimeSpan InfectionCalculationRate { get; set; } = TimeSpan.FromHours(1);
        QuadTreeNode<PositionItem> infectedItems;
        public override void Process(T context, TimeSpan span)
        {
            infectedItems.Clear();
            //refresh infected person indexer
            context.SIRInfo.ForAllWtihReference(context.Persons, (ref SIRData sir, ref PositionItem pos) =>
             {
                 if (sir.Status == SIRData.Infective)
                 {
                     infectedItems.AddItem(pos);
                 }
             });

            //try to be infected
            ProcessInfection(context, span);
            //update ground info
            ProcessGround(context, span);
            ProcessCure(context, span);
        }

        protected virtual void ProcessInfection(T context, TimeSpan span)
        {
            float rate = (float)(span / InfectionCalculationRate)*InfectionRate;
            context.SIRInfo.ForAllParallelWtihReference(context.Persons, (ref SIRData sir, ref PositionItem pos) =>
            {
                if (sir.Status ==SIRData.Susceptible)
                {
                    var infectives = infectedItems.GetItemsInDistance(pos.Position, InfectionRadius).ToList();
                    var InfectivesCount = infectives.Count;

                    if (InfectivesCount > 0)
                    {
                        sir.InfectionProgress += InfectivesCount * rate;
                        if (sir.InfectionProgress >= 1f)
                        {
                            Vector2 currentPos = pos.Position;
                            var closestInfective = infectives.OrderByDescending(x => Vector2.DistanceSquared(x.Position, currentPos)).First();
                            sir.Status = SIRData.Infective;
                            sir.InfectionProgress = 0;
                            sir.GroundCountdown = GroundDelay;
                            sir.InfectedBy = closestInfective.ID;
                        }
                    }
                    else
                    {
                        sir.InfectionProgress = 0f;
                    }
                }
            });
        }

        protected virtual void ProcessGround(T context, TimeSpan span)
        {
            int newPoolSize = groundPoolRemain;
            context.SIRInfo.ForAllParallel((ref SIRData item) =>
            {
                if (item.Status==SIRData.Infective)
                {
                    if (item.GroundCountdown <= span) //time's up, this guy should be grounded
                    {
                        item.GroundCountdown = TimeSpan.Zero;//clear ground countdown
                        if (newPoolSize>0)
                        {
                            if (Interlocked.Decrement(ref newPoolSize) >= 0) //ground capcity is not full, do isolation
                            {
                                item.Status = SIRData.Grounded;
                                item.CureCountdown = CureDelay;
                            }
                        }
                    }
                    else
                    {
                        item.GroundCountdown -= span;
                    }
                }
            });
            groundPoolRemain = Math.Max(newPoolSize, 0);
        }

        protected virtual void ProcessCure(T context, TimeSpan span)
        {
            context.SIRInfo.ForAllParallel((ref SIRData item) =>
            {
                if (item.Status == SIRData.Grounded)
                {
                    if (item.CureCountdown<=span)
                    {
                        item.Status = SIRData.Recovered;
                        item.CureCountdown = TimeSpan.Zero;
                        Interlocked.Increment(ref groundPoolRemain);
                    }
                    else
                    {
                        item.CureCountdown -= span;
                    }
                }
            });
        }

        public override void Init(T context)
        {
            base.Init(context);
            groundPoolRemain = GroundPoolSize;
            infectedItems = new QuadTreeNode<PositionItem>(new System.Drawing.RectangleF(new PointF(0, 0), context.Size), (ref PositionItem x) =>
                {
                    return x.Position;
                }, 10, 32);
            context.SIRInfo.ForAllParallel((int idx, ref SIRData data) =>
            {
                if (idx < initWithInfected)
                {
                    data.Status = SIRData.Infective;
                    data.GroundCountdown = GroundDelay;
                }
            });
            
        }
    }
}
