using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Numerics;
using System.Text;
using VirusSimulator.Processor;
using VirusSimulator.Processor.Test;

namespace VirusSimulator.Core.Test
{
    public class TestContext : RunContext, IVirusContext,IPersonMoveContext
    {
        DataBuffer<InfectionData> IVirusContext.VirusData { get; set; }
        DataBuffer<MoveStatus> IPersonMoveContext.MoveStatus { get; set; }
        protected override void Init()
        {
            base.Init();
            (this as IVirusContext).VirusData = new DataBuffer<InfectionData>(Persons.Items.Length, Persons.Bins);
            (this as IPersonMoveContext).MoveStatus = new DataBuffer<MoveStatus>(Persons.Items.Length, Persons.Bins, (index) =>
            {
                return new MoveStatus() { ID = index, CurrentTarget=Vector2.Zero, IsMovingToTarget = MovingStatusEnum.Idle };
            });
        }

        public void InitRandomPosition()
        {
            Persons.ForAllParallel((int index,ref PositionItem p) =>
            {
                //Debug.WriteLine($"init {index}");
                p.Move(Helper.RandomFloat(Size.Width), Helper.RandomFloat(Size.Height));
            });
        }

        public void InitCirclePosition(Vector2 center,float radias)
        {
            Persons.ForAllParallel((int index,ref PositionItem p) =>
            {
                p.MoveTo(center);
                p.Rotate(Helper.RandomFloat(Helper.TwoPI));
                p.Move(0, Helper.RandomFloat(radias));
            });
        }
    }
}
