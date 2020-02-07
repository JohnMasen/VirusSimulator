using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;
using VirusSimulator.Core;

namespace VirusSimulator.Processor
{
    public class RandomInterestPointProcessor<T> : IProcessor<T> where T : RunContext, IPersonMoveContext
    {
        private List<Vector2> poi = new List<Vector2>();
        private int pCount;
        public float Radius { get; set; } = 300f;
        public RandomInterestPointProcessor(int pointsCount)
        {
            pCount = pointsCount;
            
        }

        public void Init(T context)
        {
            for (int i = 0; i < pCount; i++)
            {
                poi.Add(new Vector2(Helper.RandomFloat(context.Size.Width), Helper.RandomFloat(context.Size.Height)));
            }
        }

        public void Process(T context, TimeSpan span)
        {
            context.MoveStatus.ForAllParallel((ref MoveStatus m) =>
            {
                if (m.IsMovingToTarget==MovingStatusEnum.Idle)
                {
                    m.IsMovingToTarget = MovingStatusEnum.Moving;
                    m.CurrentTarget=poi[Helper.RandomInt(pCount - 1)];
                    //m.CurrentTarget = poi[Helper.RandomInt(pCount - 1)] + new Vector2(Helper.RandomFloat(-Radius, Radius));
                }
            });
        }
    }
}
