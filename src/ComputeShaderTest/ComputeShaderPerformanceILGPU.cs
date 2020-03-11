using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.CsProj;
using BenchmarkDotNet.Toolchains.InProcess.Emit;
using BenchmarkDotNet.Validators;
using ILGPU;
using ILGPU.Algorithms;
using ILGPU.Runtime;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace ComputeShaderTest
{
    [Config(typeof(ILGPUConfig))]
    public class ComputeShaderPerformanceILGPU
    {

        private class ILGPUConfig:ManualConfig
        {
            public ILGPUConfig()
            {
                var baseJob = Job.Dry;
                Add(baseJob
                    .WithNuGet("ILGPU.Algorithms")
                    //.WithNuGet("ILGPU")
                    //.With(InProcessEmitToolchain.Instance)
                    );
            }
            
        }

        [Params(1000,10000)]
        public int Items { get; set; } = 1000;
        public bool CopyBack { get; set; } = false;
        [ParamsSource(nameof(GetAccelerators))]
        public Accelerator Acc { get; set; }
        Action<ILGPU.Index, ArrayView<Vector2>, ArrayView<Vector2>> run;
        private MemoryBuffer<Vector2> buffer1, buffer2;
        ILGPU.Index idx;
        Context c = new Context();
        [GlobalSetup]
        public void Init()
        {
            var init = Acc.CreateInitializer<Vector2>();
            buffer1 = Acc.Allocate<Vector2>(Items);
            init(Acc.DefaultStream, buffer1, Vector2.One);
            buffer2 = Acc.Allocate<Vector2>(Items);
            init(Acc.DefaultStream, buffer2, Vector2.One);
            run = Acc.LoadAutoGroupedStreamKernel<ILGPU.Index, ArrayView<Vector2>, ArrayView<Vector2>>(ilgpu_test);
            idx = new ILGPU.Index(Items);
        }

        [GlobalCleanup]
        public void Close()
        {
            run = null;
            buffer1.Dispose();
            buffer2.Dispose();
            Acc.Dispose();
            c.Dispose();
        }

        public IEnumerable<Accelerator> GetAccelerators
        {
            get
            {
                foreach (var item in Accelerator.Accelerators)
                {
                    yield return Accelerator.Create(c,item);
                }
            }
        }

        [Benchmark]
        public void Step()
        {
            run(idx, buffer1, buffer2);
            Acc.Synchronize();
            if (CopyBack)
            {
                var r=buffer1.GetAsArray();
            }
        }

        private static void ilgpu_test(ILGPU.Index idx, ArrayView<Vector2> value1, ArrayView<Vector2> value2)
        {
            value1[idx] = value1[idx] + value2[idx];
        }
    }
}
