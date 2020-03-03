using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using VirusSimulator.Core;

namespace ComputeShaderTest
{
    [SimpleJob(runtimeMoniker:RuntimeMoniker.HostProcess)]
    public  class ComputeShaderPerformanceCPU
    {
        [Params(1000,10000)]
        public int Items { get; set; } = 1000;
        public bool CopyBack { get; set; } = false;

        

        DataBuffer<Vector2> buffer1 ;
        DataBuffer<Vector2> buffer2 ;
        [GlobalSetup]
        public void Init()
        {
            Random r = new Random();
            var b = createBuffer(Items);
            buffer1 = new DataBuffer<Vector2>(b.buffer1);
            buffer2 = new DataBuffer<Vector2>(b.buffer2);
        }

        [Benchmark(Baseline =true)]
        public void CPUTestStep()
        {
            buffer1.ForAllParallelWtihReference(buffer2, (ref Vector2 item, ref Vector2 value) =>
            {
                item += value;
            });
        }


        private (Vector2[] buffer1, Vector2[] buffer2) createBuffer(int items)
        {
            Vector2[] result1 = new Vector2[items];
            Vector2[] result2 = new Vector2[items];
            for (int i = 0; i < items; i++)
            {
                result1[i] = Vector2.One;
                result2[i] = Vector2.One;
            }
            return (result1, result2);
        }
    }
}
