using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using VirusSimulator.Core;

namespace VirusSimulator.Processor.Test
{

    public class RandomMoveProcessor<T> : IProcessor<T> where T:RunContext
    {
        public float Speed { get; set; } = 10f;

        public void Init(T context)
        {
        }

        //private TestContext context;

        public void Process(T context,TimeSpan span)
        {
            (context??throw new ArgumentNullException(nameof(context))).Persons.ForAllParallel((ref PositionItem person)=>
            {
                (Matrix3x2 transform, Vector2 position) previous = (person.Transform, person.Position);
                float r = Helper.RandomFloat(Helper.TwoPI);
                person.Rotate(r);
                float d = Helper.RandomFloat(Speed);
                person.Move(0, d);
                if (person.Position.X < 0 || person.Position.X > context.Size.Width || person.Position.Y < 0 || person.Position.Y > context.Size.Height)
                {
                    //out of area, restore previous position
                    person.Position = previous.position;
                    person.Transform = previous.transform;
                }
            }
            );
        }


                
    }
}
