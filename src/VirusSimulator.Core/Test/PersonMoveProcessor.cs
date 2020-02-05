using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace VirusSimulator.Core.Test
{
    public class PersonMoveProcessor : IProcessor<TestContext>
    {
        public float Speed { get; set; } = 10f;
        private TestContext context;
        public void Process(TestContext context)
        {
            this.context = context;
            context.Persons.ForAllBlocks(block =>
            {
                Random r = new Random();
                var s = block.Span;
                for (int i = 0; i < s.Length; i++)
                {
                    ref Person person = ref s[i];
                    (Matrix3x2 transform, Vector2 position) previous = (person.Transform, person.Position);
                    float rr=r.NextFloat(Helper.TwoPI);
                    person.Rotate(rr);
                    float d = r.NextFloat(-Speed);
                    person.MoveTo(0, d);
                    if (person.Position.X < 0 || person.Position.X > context.Size.Width || person.Position.Y < 0 || person.Position.Y > context.Size.Height)
                    {
                        //restore previous position
                        person.Position = previous.position;
                        person.Transform = previous.transform;
                    }
                }
            });
        }

        //private void doMove(ref Person person)
        //{
        //    (Matrix3x2 transform, Vector2 position) previous = (person.Transform, person.Position);
        //    float r = Helper.NextRandom(Helper.TwoPI);
        //    person.Rotate(r);
        //    float d = Helper.NextRandom(-Speed);
        //    person.MoveTo(0, d);
        //    if (person.Position.X<0 || person.Position.X>context.Size.Width || person.Position.Y<0 || person.Position.Y>context.Size.Height)
        //    {
        //        //restore previous position
        //        person.Position = previous.position;
        //        person.Transform = previous.transform;
        //    }
        //}
    }
}
