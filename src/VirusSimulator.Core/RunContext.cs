using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace VirusSimulator.Core
{
    public abstract class RunContext
    {
        
        public DataBuffer<PositionItem> Persons { get; internal set; }
        public DateTime WorldClock { get; set; } 
        public SizeF Size { get;  set; }

        
        protected virtual void Init()
        {

        }

        public static T CreateInstance<T>(int personCount, DateTime clock, SizeF size, int bins = 10) where T:RunContext,new()
        {
            T result = new T();
            result.Persons = new DataBuffer<PositionItem>(personCount, bins,index=>new PositionItem() { Transform = Matrix3x2.Identity ,ID=index});
            result.WorldClock = clock;
            result.Size = size;
            result.Init();
            return result;
        }

        

    }
}
