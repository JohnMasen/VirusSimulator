using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace VirusSimulator.Core
{
    public class StepInfo
    {
        public bool IsCancel { get; set; }
        public long FrameIndex { get; private set; }
        
        public StepInfo()
        {
            IsCancel = false;
            FrameIndex = 0;
        }

        public void Step()
        {
            FrameIndex++;
        }
        
    }
}
