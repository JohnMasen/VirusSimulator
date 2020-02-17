using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using VirusSimulator.Core;

namespace VirusSimulator.Processor
{
    public enum POIStatusEnum:int
    {
        AtHome,
        FromHomeToPOI,
        FromPOIToPOI,
        GoHome
    }
    public struct POIInfo
    {
        public Vector2 HomePosition { get; set; }
        public POIStatusEnum POIStatus { get; set; }
    }
    public interface IPOIContext
    {
        public DataBuffer<POIInfo> POIData { get; set; }
    }
}
