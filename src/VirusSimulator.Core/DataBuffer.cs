using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

namespace VirusSimulator.Core
{
    public class DataBuffer<T> where T:struct
    {
        private Memory<T> buffer;
        public ReadOnlyMemory<T> Items { get; private set; }
        public delegate void ForAllDelegate(ref T item);

        readonly List<Memory<T>> blocks = new List<Memory<T>>();

        public int Bins => blocks.Count;

        public DataBuffer(int size, int bins,Func<int,T> creationCallback)
        {
            T[] data = new T[size];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = creationCallback(i);
            }
            initFromArray(data, bins);
        }
        public DataBuffer(int size, int bins) : this(new T[size], bins)
        { 

        }

        public DataBuffer(T[] data,  int bins)
        {
            initFromArray(data?? throw new ArgumentNullException(nameof(data)), bins);
        }

        private void initFromArray(T[] data, int bins)
        {
            buffer = data.AsMemory();
            Items = buffer;
            int size = data.Length;
            int binSize = size / bins;
            if (size % bins > 0)
            {
                binSize++;
            }
            int pos = 0;
            while (size > 0)
            {
                if (binSize > size)
                {
                    binSize = size;
                }
                blocks.Add(buffer.Slice(pos, binSize));
                pos += binSize;
                size -= binSize;
            }
        }

        public void ForAllBlocks(Action<Memory<T>> a)
        {
            blocks.AsParallel().ForAll(block =>
            {
                a(block);
            });
        }
        public void ForAllParallel(ForAllDelegate a)
        {
            blocks.AsParallel().ForAll(block =>
            {
                var s = block.Span;
                for (int i = 0; i < s.Length; i++)
                {
                    a(ref s[i]);
                }
            });
        }


        public void ForAll(Action<Memory<T>> action)
        {
            (action ?? throw new ArgumentNullException(nameof(action))).Invoke(buffer);
        }

        public void ForAll(ForAllDelegate action)
        {
            var s = buffer.Span;
            for (int i = 0; i < buffer.Length; i++)
            {
                action(ref s[i]);
            }
        }

        
    }
}
