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

namespace VirusSimulator.SIR
{
    public class SIRProcessor<T> : ProcessorBase<T> where T : RunContext, ISIRContext
    {
        public float InfectionRadias { get; set; } = 2f;

        public float InfectionRate { get; set; } = 0.1f;

        public TimeSpan GroundDelay { get; set; } = TimeSpan.FromDays(7);

        /// <summary>
        /// 每小时可以隔离的人数
        /// </summary>
        public int GroundPerHour { get; set; } = 10;

        public TimeSpan InfectionCalculationRate { get; set; } = TimeSpan.FromHours(1);
        QuadTreeNode<PositionItem> infectedItems;
        TimeSpan groundDuration;
        public override void Process(T context, TimeSpan span)
        {
            infectedItems.Clear();
            //refresh infected person indexer
            context.SIRInfo.ForAllWtihReference(context.Persons, (ref SIRData sir, ref PositionItem pos) =>
             {
                 if ((sir.Status & SIRData.CanInfectOthers) > 0)
                 {
                     infectedItems.AddItem(pos);
                 }
             });
            
            //try to be infected
            ProcessInfection(context, span);

            //update infection status
            //CommitInfection(context, span);

            //update ground info
            UpdateGround(context, groundDuration);


        }

        protected virtual void ProcessInfection(T context, TimeSpan span)
        {
            float rate = (float)(span / InfectionCalculationRate);
            context.SIRInfo.ForAllParallelWtihReference(context.Persons, (ref SIRData sir, ref PositionItem pos) =>
            {
                if ((sir.Status & SIRData.CanInfect) > 0)
                {
                    var InfectivesCount = infectedItems.GetItemsCountInDistance(pos.Position, InfectionRadias);
                    if (InfectivesCount>0)
                    {
                        sir.InfectionRate += InfectivesCount * InfectionRate * rate;
                        if (sir.InfectionRate >= 1f)
                        {
                            sir.Status &= SIRData.Make_Infectives;
                            sir.InfectionRate = 0;
                            sir.GroundCountdown = GroundDelay;
                        }
                    }
                    
                }
            });
        }

        //protected virtual void CommitInfection(T context, TimeSpan _)
        //{
        //    context.SIRInfo.ForAllParallel((ref SIRData sir) =>
        //    {
        //        if (sir.InfectionRate >= 1f && (sir.Status & SIRData.CanInfect) > 0)
        //        {
        //            sir.Status &= SIRData.Make_Infectives;
        //            sir.InfectionRate = 0;
        //            sir.GroundCountdown = GroundDelay;
        //        }
        //    });
        //}


        protected virtual void UpdateGround(T context, TimeSpan span)
        {
            groundDuration += span;
            int hours = (int)(groundDuration / TimeSpan.FromHours(1));
            if (hours > 0)// at lear 1 hour has passed since last groud operation
            {
                groundDuration -= TimeSpan.FromHours(hours); //decrease the counter
                int personsToGround = hours * GroundPerHour;
                context.SIRInfo.ForAllParallel((ref SIRData item) =>
                {
                    if ((item.Status & SIRData.InfectedNotGrounded) >0) //this guy is infected and 还在外面乱走
                    {
                        if (item.GroundCountdown< span) //timeout's up, this guy should be grounded
                        {
                            if (Interlocked.Decrement(ref personsToGround) >= 0) //ground capcity is not full, do isolation
                            {
                                item.Status &= SIRData.Make_Grounded;
                            }
                        }
                        else
                        {
                            item.GroundCountdown -= span;
                        }
                        
                    }
                });
            }
        }

        public override void Init(T context)
        {
            base.Init(context);
            infectedItems = new QuadTreeNode<PositionItem>(new System.Drawing.RectangleF(new PointF(0, 0), context.Size), (ref PositionItem x) =>
                {
                    return x.Position;
                }, 10, 32);
            groundDuration = TimeSpan.Zero;
        }
    }
}
