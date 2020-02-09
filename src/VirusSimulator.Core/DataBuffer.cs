using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics.CodeAnalysis;
using System.Buffers;
using System.Diagnostics;
using System.Threading;

namespace VirusSimulator.Core
{
    public class DataBuffer<T> where T : struct
    {
        private struct BlockIndex
        {
            public int pos;
            public int length;
        }
        internal Memory<T> buffer;
        public ReadOnlyMemory<T> Items { get; private set; }
        public delegate void EditItemDelegate(ref T item);
        public delegate void EditItemWithIndexDelegate(int index, ref T item);
        readonly List<BlockIndex> blocks = new List<BlockIndex>();
        //private List<MemoryHandle> mh = new List<MemoryHandle>();
        public int Bins => blocks.Count;
        
        public DataBuffer(int size, int bins, Func<int, T> creationCallback)
        {
            
            T[] data = new T[size];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = creationCallback(i);
            }
            initFromArray(data, bins);
            //mh.Add(buffer.Pin());
        }
        public DataBuffer(int size, int bins=0) : this(new T[size], bins)
        {

        }

        public DataBuffer(T[] data, int bins=0)
        {
            initFromArray(data ?? throw new ArgumentNullException(nameof(data)), bins);
        }

        private void initFromArray(T[] data, int bins)
        {
            initBlocks(data.Length,bins);
            buffer = data.AsMemory();
            Items = buffer;
            //int size = data.Length;
            //int binSize = size / bins;
            //if (size % bins > 0)
            //{
            //    binSize++;
            //}
            //int pos = 0;
            //while (size > 0)
            //{
            //    if (binSize > size)
            //    {
            //        binSize = size;
            //    }
            //    blocks.Add(buffer.Slice(pos, binSize));
            //    pos += binSize;
            //    size -= binSize;
            //}
            //foreach (var item in blocks)
            //{
            //    mh.Add(item.Pin());
            //}
        }

        private void initBlocks(int size, int bins = 0)
        {
            if (bins <= 0)
            {
                bins = size < 100 ? 1 : Environment.ProcessorCount * 2;
            }
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
                blocks.Add(new BlockIndex() { pos = pos, length = binSize });
                pos += binSize;
                size -= binSize;
            }
        }

        public void ForAllBlocks(Action<Memory<T>> a)
        {
            blocks.AsParallel().ForAll(block =>
            {
                a(buffer.Slice(block.pos,block.length));
            });
        }
        public void ForAllParallel(EditItemWithIndexDelegate a)
        {
            blocks.AsParallel().ForAll(block =>
            {
                for (int i = block.pos; i < block.length+block.pos; i++)
                {
                    a(i, ref buffer.Span[i]);
                }
            });
        }

        public void ForAllParallel(EditItemDelegate a)
        {

            blocks.AsParallel().ForAll(block =>
            {
                for (int i = block.pos; i < block.length + block.pos; i++)
                {
                    a(ref buffer.Span[i]);
                }
            });
        }

        public void Update(int start, int length, EditItemWithIndexDelegate a)
        {
            var s = buffer.Span.Slice(start, length);
            for (int i = 0; i < length; i++)
            {
                a(i+start, ref buffer.Span[i]);
            }
            
        }

        public void ForAll(Action<Memory<T>> action)
        {
            (action ?? throw new ArgumentNullException(nameof(action))).Invoke(buffer);
        }

        public void ForAll(EditItemWithIndexDelegate action)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                action(i,ref buffer.Span[i]);
            }
        }
        public void ForAll(EditItemDelegate action)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                action(ref buffer.Span[i]);
            }
        }




        public void CheckCompatible<TTarget>(DataBuffer<TTarget> target)where TTarget:struct
        {
            if (target.Items.Length!=Items.Length)
            {
                throw new ArgumentException("target must have same items as current databuffer");
            }
            //if (!target.blocks.SequenceEqual(blocks))
            //{
            //    throw new ArgumentException("target much have same blocks as current databuffer");
            //}
        }


    }
}
