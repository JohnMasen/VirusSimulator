using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace VirusSimulator.Core
{
    public class Simulator<TArea, TPerson> where TArea : IArea,new() where TPerson:IPerson,new()
    {
        public event EventHandler<TPerson> OnPersonCreated;
        CancellationTokenSource cts = new CancellationTokenSource();
        public TArea Area { get; private set; } = new TArea();
        private Dictionary<int,TPerson> persons = new Dictionary<int, TPerson>();
        public IEnumerable<KeyValuePair<int,TPerson>> Persons => persons;

        public DateTime WorldClock { get; private set; }


        public void Init(float width, float height, int count)
        {
            Init(width, height, count, DateTime.Now);
        }
        public void Init(float width,float height, int count,DateTime clock)
        {
            WorldClock = clock;
            Area.Init(width, height,count);
            foreach (var item in Area.Points)
            {
                TPerson p = new TPerson();
                p.Init(item.Key, WorldClock, item.Value);
                OnPersonCreated?.Invoke(this,p);
                persons.Add(p.ID,p);
            }
            
        }

        public void Step(TimeSpan duration)
        {
            foreach (var item in persons)
            {
                item.Value.Update(duration);
                Area.Points[item.Key] = item.Value.Position;
                foreach (var virus in item.Value.Viruses)
                {
                    foreach (var candidate in virus.Value.ScanForCandidates(item.Value, Area))//扫描潜在感染者
                    {
                        virus.Value.Infect(item.Value, persons[candidate]);//逐个感染
                    } 
                }
            }
            
            WorldClock += duration;
        }
    }
}
