using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

namespace VirusSimulator.Core
{
    public interface IPerson:IRuntimeObject
    {
        public int ID { get;  }
        public void Init(int id,  Vector2 position);
        
        public void Update(TimeSpan duration);

        public Vector2 Position { get;  }

        public Dictionary<string,IVirus>  Viruses { get; }

    }
}
