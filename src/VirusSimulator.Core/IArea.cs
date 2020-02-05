using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;

namespace VirusSimulator.Core
{
    public interface IArea:IRuntimeObject
    {
        void Init(float width, float height,int pointsCount);

        public System.Numerics.Vector2 Size { get; }

        Dictionary<int, Vector2> Points { get; }

        IEnumerable<KeyValuePair<int, Vector2>> GetPointsInRange(Vector2 center,float r);
        
    }
}
