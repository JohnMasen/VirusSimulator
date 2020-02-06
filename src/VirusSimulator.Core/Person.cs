using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;
using VirusSimulator.Core.QuadTree;

namespace VirusSimulator.Core
{
    public struct Person
    {
        public int ID;
        public Vector2 Position;
        public Matrix3x2 Transform;

        public static QuadTreeNode<Person> CreatePersonQuadTree(RectangleF range,int capacity=10)
        {
            return new QuadTreeNode<Person>(range, (ref Person p) =>p.Position, capacity);
        }
    }

    public static class PersonHelper
    {
        public static void MoveTo(ref this Person person, Vector2 position)
        {
            person.Transform = Matrix3x2.CreateTranslation(position) * person.Transform;
            person.Position = person.Transform.Translation;
        }

        public static void MoveTo(ref this Person person, float x, float y)
        {
            person.MoveTo(new Vector2(x, y));
        }

        public static void Rotate(ref this Person person, float radians)
        {
            person.Transform = Matrix3x2.CreateRotation(radians) * person.Transform;
            person.Position = person.Transform.Translation;
        }

        public static DataBuffer<Person> CreateBuffer(int count, int bins)
        {
            DataBuffer<Person> result = new DataBuffer<Person>(count, bins);
            int id = 0;
            result.ForAll(buffer =>
            {
                var s = buffer.Span;
                for (int i = 0; i < buffer.Length; i++)
                {
                    s[i].ID = id++;
                    s[i].Transform = Matrix3x2.Identity;
                }
            }
            );
            return result;
        }

        
    }
}
