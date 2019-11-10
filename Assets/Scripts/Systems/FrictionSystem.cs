using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Properties;
using Unity.Collections;
using Unity.Burst;

public class FrictionSystem : JobComponentSystem
{
	[BurstCompile]
	private struct FrictionJob : IJobForEach<Friction, MoveComponent>
	{
		public void Execute([ReadOnly] ref Friction friction, ref MoveComponent move)
		{
			move.Value.x *= 1.0f - friction.Value;
			move.Value.y *= 1.0f - friction.Value;
		}
	}

	protected override JobHandle OnUpdate(JobHandle inputDeps)
	{
		var job = new FrictionJob();
		return job.Schedule(this, inputDeps);
	}
}
