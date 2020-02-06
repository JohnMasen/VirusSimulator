using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Security.Cryptography;
using VirusSimulator.Core;
using System.Linq;

namespace VirusSimulator.Area
{
    public class SimpleArea : IArea
    {

        public Runtime Runtime { get; set; }
        Random r = new Random();

        public Vector2 Size { get; private set; }

        public Dictionary<int, Vector2> Points { get; } = new Dictionary<int, Vector2>();

        public IEnumerable<Vector2> CreateInitialPoints(int count)
        {
            for (int i = 0; i < count; i++)
            {
                yield return new Vector2(getRandom(Size.X),getRandom(Size.Y));
            }
        }

        private float getRandom(float max)
        {
            float f = (float)r.NextDouble();
            return f * max;
        }

        public void Init(float width, float height,int pointsCount)
        {
            Size = new Vector2(width, height);
            for (int i = 0; i < pointsCount; i++)
            {
                Points.Add(i, new Vector2(getRandom(Size.X), getRandom(Size.Y)));
            }
        }

        public IEnumerable<KeyValuePair<int, Vector2>> GetPointsInRange(Vector2 center,float r)
        {
            float distinctSquared = r * r;
            return from item in Points
                   where Vector2.DistanceSquared(center, item.Value) <= distinctSquared
                   select item;
        }
    }
}
