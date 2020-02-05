using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

namespace VirusSimulator.Core
{
    public interface IPerson
    {
        public DateTime Now { get;  }
        public int ID { get;  }
        public void Init(int id, DateTime now, Vector2 position);
        
        public void Update(TimeSpan duration);

        public Vector2 Position { get;  }

        public Dictionary<string,IVirus>  Viruses { get; }

    }
}
