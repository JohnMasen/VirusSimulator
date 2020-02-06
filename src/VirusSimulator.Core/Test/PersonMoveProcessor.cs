using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace VirusSimulator.Core.Test
{
    public class PersonMoveProcessor : IProcessor<TestContext>
    {
        public float Speed { get; set; } = 10f;
        //private TestContext context;
        
        public void Process(TestContext context,TimeSpan span)
        {
            //this.context = context;
            context.Persons.ForAllParallel((ref Person person)=>
            {
                (Matrix3x2 transform, Vector2 position) previous = (person.Transform, person.Position);
                float r = Helper.RandomFloat(Helper.TwoPI);
                person.Rotate(r);
                float d = Helper.RandomFloat(Speed);
                person.MoveTo(0, d);
                if (person.Position.X < 0 || person.Position.X > context.Size.Width || person.Position.Y < 0 || person.Position.Y > context.Size.Height)
                {
                    //out of area, restore previous position
                    person.Position = previous.position;
                    person.Transform = previous.transform;
                }
            }
            );
        }


        public void Init(TestContext context)
        {
            context.Persons.ForAllParallel((ref Person p)=> 
            {
                p.MoveTo(Helper.RandomFloat(context.Size.Width), Helper.RandomFloat(context.Size.Height));
            });
        }
        
    }
}
