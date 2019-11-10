using UnityEngine;
using System.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Jobs;

public class ForceSystem : JobComponentSystem
{
	[BurstCompile]
	private struct ForceJob : IJobForEach<MoveComponent, Translation, Particle>
	{
		[DeallocateOnJobCompletion, ReadOnly] public NativeArray<Translation> entity_positions;
		[DeallocateOnJobCompletion, ReadOnly] public NativeArray<int> types;

		[DeallocateOnJobCompletion, ReadOnly] public NativeArray<float> attract;
		[DeallocateOnJobCompletion, ReadOnly] public NativeArray<float> minR;
		[DeallocateOnJobCompletion, ReadOnly] public NativeArray<float> maxR;

		public int length;
		public float width;
		public float height;
		public bool wrap;

		private const float SMOOTH = 0.002f;

		private int C(int x, int y) => y * length + x;

		public void Execute(ref MoveComponent move,
							ref Translation translation,
							ref Particle particle)
		{
			for (int i = 0; i < types.Length; i++)
			{
				int type = types[i];
				Translation pos = entity_positions[i];
				float dx = pos.Value.x - translation.Value.x;
				float dy = pos.Value.y - translation.Value.y;

				if (wrap)
				{
					if (dx > width / 2f)
						dx -= width;
					else if (dx < -width / 2f)
						dx += width;
					if (dy > height / 2f)
						dy -= height;
					else if (dy < -height / 2f)
						dy += height;
				}

				float r2 = dx * dx + dy + dy;
				float minr = minR[C(type, particle.type)];
				float maxr = maxR[C(type, particle.type)];

				if (r2 > maxr * maxr || r2 < 0.01f)
					continue;

				float r = math.sqrt(r2);
				dx /= r;
				dy /= r;

				float f;
				if (r > minr)
				{
					float numer = 2.0f * math.abs(r - 0.5f * (maxr + minr));
					float denom = maxr - minr;
					f = attract[C(type, particle.type)] * (1.0f - numer / denom);
				}
				else
					f = SMOOTH * minr * (1.0f / (minr + SMOOTH) - 1.0f / (r + SMOOTH));

				move.Value.x += f * dx;
				move.Value.y += f * dy;
			}
		}
	}

	protected override JobHandle OnUpdate(JobHandle inputDeps)
	{
		var entities = EntityManager.GetAllEntities(Allocator.Temp);
		var translations = new NativeArray<Translation>(entities.Length, Allocator.TempJob);
		var types = new NativeArray<int>(entities.Length, Allocator.TempJob);

		var stats = GameManagerScript.GetInstance().stats;
		var attract = new NativeArray<float>(stats.attract, Allocator.TempJob);
		var minr = new NativeArray<float>(stats.minR, Allocator.TempJob);
		var maxr = new NativeArray<float>(stats.maxR, Allocator.TempJob);

		//? Might be a bottle neck
		for (int i = 0; i < entities.Length; i++)
		{
			translations[i] = EntityManager.GetComponentData<Translation>(entities[i]);
			types[i] = EntityManager.GetComponentData<Particle>(entities[i]).type;
		}

		entities.Dispose();

		var job = new ForceJob()
		{
			types = types,
			entity_positions = translations,
			width = GameManagerScript.Width * 2,
			height = GameManagerScript.Height * 2,
			wrap = false,
			attract = attract,
			minR = minr,
			maxR = maxr,
			length = stats.Count,
		};
		var handle = job.Schedule(this, inputDeps);

		return handle;
	}
}
