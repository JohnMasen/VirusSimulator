using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using VirusSimulator.Core;
using VirusSimulator.Processor.Test;

namespace VirusSimulator.Processor
{
	public class PersonMoveProcessor<T> : IProcessor<T> where T : RunContext, IPersonMoveContext
	{
		public float Speed { get; set; } = 5f;

		public void Init(T context)
		{
			//context.MoveStatus.ForAllParallel((ref MoveStatus m) =>
			//{
			//	m.Status = MoveStatusEnum.MovingToTarget;
			//	m.CurrentTarget = new Vector2(100, 100);
			//});
		}
		public void Close(T context)
		{

		}
		public void Process(T context, TimeSpan span)
		{
			float d = Speed*Speed;
			context.Persons.ForAllParallel((ref PositionItem p) =>
			{
				MoveStatus m = context.MoveStatus.Items.Span[p.ID];
				if (m.IsMovingToTarget==MovingStatusEnum.Moving)
				{
					Vector2 currentTarget = m.CurrentTarget;
					if (Vector2.DistanceSquared(p.Position, currentTarget) <=d)
					{
						//target reached
						p.MoveTo(currentTarget);
						context.MoveStatus.Update(p.ID, (ref MoveStatus ms) =>
						 {
							 ms.IsMovingToTarget = MovingStatusEnum.Idle;
						 });
					}
					else
					{
						Vector2 direction = Vector2.Normalize(currentTarget - p.Position);
						p.Move(direction * Speed);
					}
				}
			});
		}
	}
}
