using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using VirusSimulator.Core;
using VirusSimulator.Image;
using VirusSimulator.Processor;
using VirusSimulator.Processor.Test;
using VirusSimulator.SIR;

namespace VirusSimulator.WPF
{
    public class TestContext : RunContext,ISIRContext, IPersonMoveContext,IPOIContext
    {
        public DataBuffer<MoveStatus> MoveStatus { get; set; }
        public DataBuffer<POIInfo> POIData { get; set; }
        public DataBuffer<SIRData> SIRInfo { get; private set; }

        protected override void Init()
        {
            base.Init();
            //(this as IVirusContext).VirusData = new DataBuffer<InfectionData>(Persons.Items.Length, Persons.Bins);
            SIRInfo = new DataBuffer<SIRData>(Persons.Items.Length, 0, _ =>
            {
                return new SIRData() { Status = SIRData.Susceptible,InfectedBy=-1 };
            });
            (this as IPersonMoveContext).MoveStatus = new DataBuffer<MoveStatus>(Persons.Items.Length, Persons.Bins, (index) =>
            {
                return new MoveStatus() { ID = index, CurrentTarget = Vector2.Zero, IsMovingToTarget = MovingStatusEnum.Idle };
            });
            POIData = new DataBuffer<POIInfo>(Persons.Items.Length, Persons.Bins, _ =>
                {
                    return new POIInfo() { POIStatus = POIStatusEnum.AtHome };
                });
        }

        public void InitRandomPosition()
        {
            Persons.ForAllParallel((int index, ref PositionItem p) =>
            {
                //Debug.WriteLine($"init {index}");
                p.Move(Helper.RandomFloat(Size.Width), Helper.RandomFloat(Size.Height));
            });
        }

        public void InitCirclePosition(Vector2 center, float radius)
        {
            Persons.ForAllParallel((int index, ref PositionItem p) =>
            {
                p.MoveTo(center);
                p.Rotate(Helper.RandomFloat(Helper.TwoPI));
                p.Move(0, Helper.RandomFloat(radius));
            });
        }

        public int DataSize { get
            {
                return Persons.DataBufferSize+(this as IPersonMoveContext).MoveStatus.DataBufferSize+SIRInfo.DataBufferSize;
            } 
        }

        

        public TestContext Clone()
        {
            return new TestContext()
            {
                SIRInfo = SIRInfo.Clone(),
                Persons = Persons.Clone(),
                MoveStatus = MoveStatus.Clone(),
                POIData = POIData.Clone(),
                WorldClock = WorldClock,
                Size = Size
            };
        }
    }
}
