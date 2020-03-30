using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;
using VirusSimulator.Core.QuadTree;

namespace VirusSimulator.Core
{
    public struct PositionItem
    {
        public Vector2 Position;
        public Matrix3x2 Transform;
        public int ID { get; set; }
        public static QuadTreeNode<PositionItem> CreatePersonQuadTree(RectangleF range,int capacity=10)
        {
            return new QuadTreeNode<PositionItem>(range, (ref PositionItem p) =>p.Position, capacity);
        }
    }

    public static class PositionItemHelper
    {
        public static void Move(ref this PositionItem person, Vector2 v)
        {
            person.Transform = Matrix3x2.CreateTranslation(v) * person.Transform;
            person.Position = person.Transform.Translation;
        }

        public static void Move(ref this PositionItem person, float x, float y)
        {
            person.Move(new Vector2(x, y));
        }

        public static void Rotate(ref this PositionItem person, float radians)
        {
            person.Transform = Matrix3x2.CreateRotation(radians) * person.Transform;
            person.Position = person.Transform.Translation;
        }

        public static void MoveTo(ref this PositionItem person,Vector2 position)
        {
            person.Transform = Matrix3x2.CreateTranslation(position);
            person.Position = person.Transform.Translation;
        }
        

        
    }
}
