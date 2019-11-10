using MathNet.Numerics;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Random;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;
using Material = UnityEngine.Material;

public class GameManagerScript : MonoBehaviour
{
	private static EntityManager eManager;

	[SerializeField] private Material particleMat = null;
	[SerializeField] private Mesh quadMesh = null;
	[SerializeField, Range(0, 100)] private int particle_quantity = 10;

	public const float Height = 10;
	public const float Width = 10;
	private List<Material> adjustedMaterials;
	private readonly System.Random random_Gen = new MersenneTwister(RandomSeed.Robust());
	public ForceStats stats;

	private static GameManagerScript instance;

	public static GameManagerScript GetInstance()
		=> instance;

	private void Start()
	{
		instance = this;
		eManager = World.Active.EntityManager;

		var particleArchetype = eManager.CreateArchetype(
			typeof(Translation),
			typeof(RenderMesh),
			typeof(Rotation),
			typeof(PhysicsCollider),
			typeof(LocalToWorld),
			typeof(MoveComponent),
			typeof(Particle),
			typeof(Friction)
		);

		GenerateMaterials();
		SetupStats();

		SpawnParticles(particleArchetype);
	}

	private void Update()
	{
		Vector3 tl = new Vector3(-Width, Height);
		Vector3 tr = new Vector3(Width, Height);
		Vector3 bl = new Vector3(-Width, -Height);
		Vector3 br = new Vector3(Width, -Height);

		Debug.DrawLine(tl, tr);
		Debug.DrawLine(tl, bl);
		Debug.DrawLine(tr, br);
		Debug.DrawLine(br, bl);
	}

	private void SpawnParticles(EntityArchetype arch)
	{
		NativeArray<Entity> particles = new NativeArray<Entity>(particle_quantity, Allocator.Temp);

		eManager.CreateEntity(arch, particles);

		for (int i = 0; i < particle_quantity; i++)
		{
			float x = Random.Range(-Width, Width);
			float y = Random.Range(-Height, Height);
			float vx = (float)Normal.Sample(random_Gen, 0, 1) * 0.2f;
			float vy = (float)Normal.Sample(random_Gen, 0, 1) * 0.2f;
			int type = Random.Range(0, particle_quantity - 1);
			SetComponents(particles[i], new float2(x, y), type, new float2(vx, vy), 0.1f);
		}

		particles.Dispose();
	}

	private void SetComponents(Entity entity, float2 pos, int type, float2 velocity, float fric)
	{
		eManager.SetComponentData(entity, new Translation { Value = new float3(pos, 0) });
		eManager.SetComponentData(entity, new Particle { type = type });
		eManager.SetComponentData(entity, new MoveComponent { Value = velocity });
		eManager.SetComponentData(entity, new Friction { Value = fric });
		eManager.SetSharedComponentData(entity, new RenderMesh { material = adjustedMaterials[type], mesh = quadMesh });
	}

	private void SetupStats()
	{
		stats = new ForceStats(particle_quantity);
		for (int i = 0; i < particle_quantity; i++)
		{
			for (int j = 0; j < particle_quantity; j++)
			{
				float attract, minr, maxr;
				if (i == j)
				{
					attract = -math.abs((float)Normal.Sample(random_Gen, 0.08, 0.1));
					minr = 1.0f;
				}
				else
				{
					attract = (float)Normal.Sample(random_Gen, 0.08, 0.1);
					minr = math.max(Random.Range(0f, 2f), 1f);
				}
				maxr = math.max(Random.Range(2, 3), minr);

				stats[i, j, ForceEnum.Attract] = attract;
				stats[i, j, ForceEnum.MaxR] = maxr;
				stats[i, j, ForceEnum.MinR] = minr;
				stats[j, i, ForceEnum.MaxR] = maxr;
				stats[j, i, ForceEnum.MinR] = minr;
			}
		}
	}

	private void GenerateMaterials()
	{
		adjustedMaterials = new List<Material>();
		for (int i = 0; i < particle_quantity; i++)
		{
			Color particle_color = FromHSV((float)i / particle_quantity, 1.0f, i % 2 * 0.5f + 0.5f);
			adjustedMaterials.Add(new Material(particleMat) { color = particle_color });
		}
	}

	private Color FromHSV(float h, float s, float v)
	{
		int i = (int)(h * 6);
		float f = h * 6 - i;
		float p = v * (1 - s);
		float q = v * (1 - f * s);
		float t = v * (1 - (1 - f) * s);

		float r = 0;
		float g = 0;
		float b = 0;
		switch (i % 6)
		{
			case 0: r = v; g = t; b = p; break;
			case 1: r = q; g = v; b = p; break;
			case 2: r = p; g = v; b = t; break;
			case 3: r = p; g = q; b = v; break;
			case 4: r = t; g = p; b = v; break;
			case 5: r = v; g = p; b = q; break;
		}

		return new Color(r * 255, g * 255, b * 255);
	}
}
