using System;
using System.Collections.Generic;
using System.Text;

namespace VirusSimulator.Core
{
    public static class DecoratorHelper
    {
        public static IProcessor<T> AsOutput<T>(this IProcessor<T> processor,int frameSkip,TimeSpan maxFrameGap) where T:RunContext 
        {
            return new Decorators.OutputDecorator<T>(processor) { FrameSkip = frameSkip, OutputTimeSpan = maxFrameGap};
        }

        public static IProcessor<T> AsOutput<T>(this IProcessor<T> processor,int frameSkip) where T : RunContext
        {
            return processor.AsOutput(frameSkip, TimeSpan.MaxValue);
        }
    }
}
