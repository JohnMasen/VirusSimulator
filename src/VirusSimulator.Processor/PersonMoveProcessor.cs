using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using VirusSimulator.Core;
using VirusSimulator.Processor.Test;

namespace VirusSimulator.Processor
{
	public class PersonMoveProcessor<T> : ProcessorBase<T> where T : RunContext, IPersonMoveContext
	{
		public float Speed { get; set; } = 5f;

		
		public override void Process(T context, TimeSpan span)
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
						Vector2 newPos = p.Position + direction * Speed;
						p.MoveTo(newPos);
					}
				}
			});
		}
	}
}
