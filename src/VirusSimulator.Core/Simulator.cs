using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace VirusSimulator.Core
{
    public class Simulator<TArea, TPerson>:IRuntimeObject where TArea : IArea,new() where TPerson:IPerson,new() 
    {
        public Runtime Runtime { get; set; }
        public event EventHandler<TPerson> OnPersonCreated;
        CancellationTokenSource cts = new CancellationTokenSource();
        //public TArea Area { get; private set; } = new TArea();
        //private Dictionary<int,TPerson> persons = new Dictionary<int, TPerson>();
        //public IEnumerable<KeyValuePair<int,TPerson>> Persons => persons;

        //public DateTime WorldClock { get; private set; }



        public void Init(float width, float height, int count)
        {
            Init(width, height, count, DateTime.Now);
        }
        public void Init(float width,float height, int count,DateTime clock)
        {
            Runtime = new Runtime(new TArea(), clock);
            Runtime.Area.Init(width, height, count);
            foreach (var item in Runtime.Area.Points)
            {
                TPerson p = new TPerson() { Runtime = Runtime };
                p.Init(item.Key,  item.Value);
                OnPersonCreated?.Invoke(this,p);
                Runtime.Persons.Add(p.ID,p);
            }
            
        }

        public void Step(TimeSpan duration)
        {
            //update position
            //Runtime.Persons.AsParallel().ForAll(item =>
            //{
            //    item.Value.Update(duration);
            //    Runtime.Area.Points[item.Key] = item.Value.Position;
            //});
            Person p = new Person();

            foreach (var item in Runtime.Persons)
            {
                item.Value.Update(duration);
                Runtime.Area.Points[item.Key] = item.Value.Position;

            }
            //update virus status
            foreach (var item in Runtime.Persons)
            {
                foreach (var virus in item.Value.Viruses)
                {
                    foreach (var candidate in virus.Value.ScanForCandidates(item.Value, Runtime.Area))//扫描潜在感染者
                    {
                        virus.Value.Infect(item.Value, Runtime.Persons[candidate]);//逐个感染
                    }
                }
            }

            Runtime.WorldClock += duration;
        }
    }
}
