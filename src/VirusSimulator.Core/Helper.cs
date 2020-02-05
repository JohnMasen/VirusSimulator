using System;
using System.Collections.Generic;
using System.Text;

namespace VirusSimulator.Core
{
    public static class Helper
    {
        static Random random = new Random();
        public static readonly float TwoPI = (float)Math.PI * 2;
        public static float NextFloat(this Random r, float min, float max)
        {
            return min + (float)r.NextDouble() * (max - min);
        }

        public static float NextFloat(this Random r,float max)
        {
            return r.NextFloat(0, max);
        }


    }
}
