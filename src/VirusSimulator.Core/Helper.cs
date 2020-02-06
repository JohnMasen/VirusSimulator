using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace VirusSimulator.Core
{
    public static class Helper
    {
        static System.Threading.ThreadLocal<Random> r = new System.Threading.ThreadLocal<Random>(()=> 
        {
            Debug.WriteLine($"Random created at {Thread.CurrentThread.ManagedThreadId}");
            return new Random(); 
        });
        static Random random = new Random();
        public static readonly float TwoPI = (float)Math.PI * 2;
        //public static float NextFloat(this Random r, float min, float max)
        //{
        //    return min + (float)r.NextDouble() * (max - min);
        //}

        //public static float NextFloat(this Random r,float max)
        //{
        //    return r.NextFloat(0, max);
        //}

        public static float RandomFloat(float min, float max)
        {
            return min + (float)r.Value.NextDouble() * (max - min);
        }

        public static float RandomFloat(float max)
        {
            return RandomFloat(0, max);
        }


    }
}
