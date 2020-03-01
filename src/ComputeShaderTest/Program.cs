using ComputeSharp;
using ComputeSharp.Graphics;
using ILGPU;
using ILGPU.Algorithms;
using ILGPU.Runtime;
using SharpGen.Runtime;
using System;
using System.Diagnostics;
using System.Numerics;
using VirusSimulator.Core;

namespace ComputeShaderTest
{
    class Program
    {
        private int items = 1000;
        private TimeSpan loopDuration = TimeSpan.FromSeconds(1);
        bool copyBack = true;
        static void Main(string[] args)
        {
            new Program().Run(args);
        }

        public void Run(string[] args)
        {
            if (args.Length >= 1)
            {
                int.TryParse(args[0], out items);
            }
            if (args.Length >= 2 && int.TryParse(args[1], out int loopDuationInSeconds))
            {
                loopDuration = TimeSpan.FromSeconds(loopDuationInSeconds);
            }

            if (args.Length >= 3)
            {
                bool.TryParse(args[2], out copyBack);
            }
            try
            {
                Console.WriteLine($"Test with {items} items, duration ={loopDuration},copy back ={copyBack}");
                Console.Write("Testing with CPU ");
                Console.WriteLine($"{runCpuTest(items, loopDuration)}");
                foreach (var item in Gpu.EnumerateDevices())
                {
                    Console.Write($"Testing with ComputeSharp [{item.Name}] ");
                    Console.WriteLine($"{runComputeSharpTest(items, loopDuration, item, copyBack)}");
                }
                using (Context c = new Context())
                {
                    c.EnableAlgorithms();
                    foreach (var item in Accelerator.Accelerators)
                    {
                        using (var acc = Accelerator.Create(c, item))
                        {
                            Console.Write($"Testing with ILGPU [{acc.Name}-{item.AcceleratorType}] ");
                            Console.WriteLine($"{runILGPUTest(items, loopDuration, acc, copyBack)}");
                        }
                    }
                }

            }
            catch (Exception ex)
            {

                Console.WriteLine(ex);
            }


        }

        private int runCpuTest(int items, TimeSpan duration)
        {
            Stopwatch sw = new Stopwatch();
            Random r = new Random();
            var b = createBuffer(items);

            DataBuffer<Vector2> buffer1 = new DataBuffer<Vector2>(b.buffer1);
            DataBuffer<Vector2> buffer2 = new DataBuffer<Vector2>(b.buffer2);
            sw.Start();
            int count = 0;
            while (sw.Elapsed < duration)
            {
                buffer1.ForAllParallelWtihReference(buffer2, (ref Vector2 item, ref Vector2 value) =>
                {
                    item += value;
                });
                count++;
            }
            sw.Stop();
            return count;
        }

        private int runComputeSharpTest(int items, TimeSpan duration, GraphicsDevice device, bool copyBack)
        {
            Stopwatch sw = new Stopwatch();
            var b = createBuffer(items);
            var buffer1 = device.AllocateReadWriteBuffer(b.buffer1);
            var buffer2 = device.AllocateReadOnlyBuffer(b.buffer2);
            int count = 0;
            Vector2[] tmp = new Vector2[items];
            //warm up
            Action<ThreadIds> a = idx =>
            {
                buffer1[idx.X] = buffer1[idx.X] + buffer2[idx.X];
            };
            device.For(items, a);


            sw.Start();
            while (sw.Elapsed < duration)
            {
                device.For(items, a);
                if (copyBack)
                {
                    buffer1.GetData(tmp);
                }
                count++;
            }
            sw.Stop();
            return count;
        }


        private int runILGPUTest(int items, TimeSpan duration, Accelerator acc, bool copyBack)
        {
            int count = 0;
            var init = acc.CreateInitializer<Vector2>();
            using (var buffer1 = acc.Allocate<Vector2>(items))
            {
                init(acc.DefaultStream, buffer1, Vector2.One);
                using (var buffer2 = acc.Allocate<Vector2>(items))
                {
                    init(acc.DefaultStream, buffer2, Vector2.One);
                    var run = acc.LoadAutoGroupedStreamKernel<ILGPU.Index, ArrayView<Vector2>, ArrayView<Vector2>>(ilgpu_test);
                    ILGPU.Index idx = new ILGPU.Index(items);

                    Stopwatch sb = new Stopwatch();
                    sb.Start();
                    while (sb.Elapsed < duration)
                    {
                        run(idx, buffer1, buffer2);
                        count++;
                        if (copyBack)
                        {
                            buffer1.GetAsArray();
                        }
                        count++;
                    }
                }
            }
            return count;
        }

        private static void ilgpu_test(ILGPU.Index idx, ArrayView<Vector2> value1, ArrayView<Vector2> value2)
        {
            value1[idx] = value1[idx] + value2[idx];
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
