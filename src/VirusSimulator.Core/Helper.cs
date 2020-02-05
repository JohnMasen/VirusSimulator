using System;
using System.Collections.Generic;
using System.Text;

namespace VirusSimulator.Core
{
    public static class Helper
    {
        static Random random = new Random();
        public static readonly float TwoPI = (float)Math.PI * 2;
        public static float NextRandom(float min, float max)
        {
            return min + (float)random.NextDouble() * (max - min);
        }

        public static float NextRandom(float max)
        {
            return NextRandom(0, max);
        }


    }
}
