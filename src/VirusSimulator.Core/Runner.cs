using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;

namespace VirusSimulator.Core
{
    public class Runner<T> where T:RunContextBase,new()
    {
        public List<IProcessor<T>> Processors { get; private set; } = new List<IProcessor<T>>();
        public T Context { get; private set; }
        Random r = new Random();
        public Runner(int personCount,int bins,SizeF areaSize)
        {
            Context = new T();
            Context.Persons = PersonHelper.CreateBuffer(personCount, bins);
            Context.Size = areaSize;
            Context.Persons.ForAll(randomizeItemPosition);
        }

        private void randomizeItemPosition(ref Person person)
        {
            person.MoveTo(r.NextFloat(Context.Size.Width), r.NextFloat(Context.Size.Height));
        }

        public void Step()
        {
            foreach (var item in Processors)
            {
                item.Process(Context);
            }
        }

    }
}
