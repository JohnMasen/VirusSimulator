using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace VirusSimulator.Core
{
    public static class DataBufferHelper
    {
        public delegate void EditWithReferenceDelegate<T, T1>(ref T item, ref T1 item1);
        public delegate void EditWithIndexReferenceDelegate<T, T1>(int index, ref T item, ref T1 item1);
        public static void ForAllParallelWtihReference<T, T1>(this DataBuffer<T> buffer, DataBuffer<T1> referenceBuffer, EditWithReferenceDelegate<T,T1> action) where T : struct where T1 : struct
        {
            buffer.CheckCompatible(referenceBuffer);
            buffer.ForAllParallel((int index, ref T item) =>
            {
                action(ref item, ref referenceBuffer.buffer.Span[index]);
            });
        }

        public static void ForAllParallelWtihReference<T, T1>(this DataBuffer<T> buffer, DataBuffer<T1> referenceBuffer, EditWithIndexReferenceDelegate<T, T1> action) where T : struct where T1 : struct
        {
            buffer.CheckCompatible(referenceBuffer);
            buffer.ForAllParallel((int index, ref T item) =>
            {
                action(index,ref item, ref referenceBuffer.buffer.Span[index]);
            });
        }

        public static void ForAllWtihReference<T, T1>(this DataBuffer<T> buffer, DataBuffer<T1> referenceBuffer, EditWithReferenceDelegate<T, T1> action) where T : struct where T1 : struct
        {
            buffer.CheckCompatible(referenceBuffer);
            buffer.ForAll((int index, ref T item) =>
            {
                action(ref item, ref referenceBuffer.buffer.Span[index]);
            });
        }
    }
}
