using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using VirusSimulator.Core;

namespace VirusSimulator.Processor
{
	public enum MovingStatusEnum:byte
	{
		Idle=0x0,
		Moving=0x1,
		Freeze=0x2
	}

	public struct MoveStatus
	{
		public int ID;
		public MovingStatusEnum IsMovingToTarget;
		public Vector2 CurrentTarget;
	}

	public interface IPersonMoveContext
	{
		public DataBuffer<MoveStatus> MoveStatus { get; set; }

	}
}
