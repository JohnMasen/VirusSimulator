using System;
using System.Collections.Generic;
using System.Text;

namespace VirusSimulator.Core
{
    public enum VirusInfectionEnum
    {
        Clean,
        Infecting,
        Incubation,
        Visible
    }
    public interface IVirus
    {
        public VirusInfectionEnum InfectionProgress { get; }
        public string Name { get; }
        public void Infect(IPerson owner, IPerson victum);

        public IEnumerable<int> ScanForCandidates(IPerson owner, IArea area);
    }
}
