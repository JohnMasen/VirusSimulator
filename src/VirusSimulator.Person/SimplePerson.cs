using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using VirusSimulator.Core;

namespace VirusSimulator.Person
{
    public class SimplePerson : IPerson
    {
        public Runtime Runtime { get; set; }
        private Matrix3x2 transform = Matrix3x2.Identity;
        public float Speed { get; set; } = 3f;

        public bool IsInfected { get; set; } = false;
        public int ID { get; private set; }
        public Vector2 Position
        {
            get
            {
                return transform.Translation;
            }
        }

        public Dictionary<string, IVirus> Viruses { get; } = new Dictionary<string, IVirus>();

        public void Init(int id,  Vector2 position)
        {
            ID = id;
            Move(position);
        }

        public void Update(TimeSpan duration)
        {
            //var x = Matrix3x2.CreateTranslation(0, Speed);
            //x *= Matrix3x2.CreateRotation(Helper.NextRandom(Helper.TwoPI));
            //x *= Matrix3x2.CreateTranslation(Position);
            //Position = transform.Translation;
            //Rotate(Helper.NextRandom(Helper.TwoPI));
            //Move(0, Helper.NextRandom(Speed));
        }

        protected void Move(Vector2 vector)
        {
            
            Move(vector.X, vector.Y);
            if (Position.X<0 || Position.Y<0 || Position.X>Runtime.Area.Size.X || Position.Y>Runtime.Area.Size.Y)
            {
                Move(-vector.X, - vector.Y);
            }
        }
        protected void Move(float x, float y)
        {
            transform =  Matrix3x2.CreateTranslation(x, y) * transform;
        }
        protected void Rotate(float radius)
        {
            transform =  Matrix3x2.CreateRotation(radius)*transform;
        }

        
    }
}
