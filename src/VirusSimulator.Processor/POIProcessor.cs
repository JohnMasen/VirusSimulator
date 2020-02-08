﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;
using VirusSimulator.Core;
using System.Linq;

namespace VirusSimulator.Processor
{
    public class POIProcessor<T> : IProcessor<T> where T : RunContext, IPersonMoveContext
    {
        private List<Vector2> poi = new List<Vector2>();
        public float Radius { get; set; } = 300f;
        Func<T, IEnumerable<Vector2>> create;
        public POIProcessor(Func<T, IEnumerable<Vector2>> createPOI)
        {
            create = createPOI;
        }
        public void Close(T context)
        {

        }
        public void Init(T context)
        {
            poi.AddRange(create(context));
        }

        public void Process(T context, TimeSpan span)
        {
            context.MoveStatus.ForAllParallel((ref MoveStatus m) =>
            {
                if (m.IsMovingToTarget == MovingStatusEnum.Idle)
                {
                    m.IsMovingToTarget = MovingStatusEnum.Moving;
                    m.CurrentTarget = poi[Helper.RandomInt(poi.Count() - 1)] + new Vector2(Helper.RandomFloat(-Radius, Radius), Helper.RandomFloat(-Radius, Radius));
                }
            });
        }


        public static POIProcessor<T> CreateRandomPOI(int count,float radius=300f) 
        {
            return new POIProcessor<T>(context => createRandomPoints(count, context)) { Radius = radius };

        }
        private static IEnumerable<Vector2> createRandomPoints<TContext>(int count, TContext context) where TContext : RunContext, IPersonMoveContext
        {
            for (int i = 0; i < count; i++)
            {
                yield return new Vector2(Helper.RandomFloat(context.Size.Width), Helper.RandomFloat(context.Size.Height));
            }
        }
    }
}
