using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;

public class MovementSystem : JobComponentSystem
{
	[BurstCompile]
	private struct MovementSystemJob : IJobForEach<Translation, MoveComponent>
	{
		public float delta;
		public float width;
		public float height;
		public bool wrap;
		public float radius;

		public void Execute(ref Translation translation, ref MoveComponent move)
		{
			float x = translation.Value.x;
			float y = translation.Value.y;

			x += move.Value.x * delta;
			y += move.Value.y * delta;

			if (wrap)
			{
				if (x < -width / 2)
					x += width;
				else if (x >= width / 2)
					x -= width;
				if (y < -height / 2)
					y += height;
				else if (y >= height / 2)
					y -= height;
			}
			else
			{
				if (x < -(width / 2) + radius)
				{
					move.Value.x = -move.Value.x;
					x = -(width / 2) + radius;
				}
				else if (x >= (width / 2) - radius)
				{
					move.Value.x = -move.Value.x;
					x = (width / 2) - radius;
				}
				if (y < -(height / 2) + radius)
				{
					move.Value.y = -move.Value.y;
					y = -(height / 2) + radius;
				}
				else if (y >= (height / 2) - radius)
				{
					move.Value.y = -move.Value.y;
					y = (height / 2) - radius;
				}
			}

			translation.Value.x = x;
			translation.Value.y = y;
		}
	}

	protected override JobHandle OnUpdate(JobHandle inputDeps)
	{
		var job = new MovementSystemJob()
		{
			delta = Time.deltaTime,
			width = GameManagerScript.Width * 2,
			height = GameManagerScript.Height * 2,
			wrap = false,
			radius = 0.5f,
		};
		var handle = job.Schedule(this, inputDeps);

		return handle;
	}
}
