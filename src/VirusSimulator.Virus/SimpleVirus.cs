using System;
using System.Collections.Generic;
using VirusSimulator.Core;
using System.Linq;
namespace VirusSimulator.Virus
{
    public class SimpleVirus : IVirus
    {
        public float Radius { get; set; } = 5f;
        public string Name { get; } = "SimpleVirus";

        public float InfectionPercent { get; set; }

        public VirusInfectionEnum InfectionProgress { get; private set; } = VirusInfectionEnum.Clean;

        public void Infect(IPerson owner, IPerson victum)
        {
            if (!victum.Viruses.ContainsKey(Name))
            {
                victum.Viruses.Add(Name, new SimpleVirus());
            }
        }

        public IEnumerable<int> ScanForCandidates(IPerson owner, IArea area)
        {
            return from item in area.GetPointsInRange(owner.Position, Radius)
                   select item.Key;
        }
    }
}
