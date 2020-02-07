using System;
using System.Collections.Generic;
using System.Data;
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
            (this as IVirusContext).VirusData = new DataBuffer<InfectionData>(Persons.Items.Length, Persons.Bins, index =>
            {
                return new InfectionData() { ID = index };
            });
            (this as IPersonMoveContext).MoveStatus = new DataBuffer<MoveStatus>(Persons.Items.Length, Persons.Bins, (index) =>
            {
                return new MoveStatus() { ID = index, CurrentTarget=Vector2.Zero, IsMovingToTarget = MovingStatusEnum.Idle };
            });

            
        }

        public void InitRandomPosition()
        {
            Persons.ForAllParallel((ref PositionItem p) =>
            {
                p.Move(Helper.RandomFloat(Size.Width), Helper.RandomFloat(Size.Height));
            });
        }
    }
}
