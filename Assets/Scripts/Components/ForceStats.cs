using System.Collections;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using System.Collections.Generic;

public struct ForceStats
{
	public float[] attract;
	public float[] minR;
	public float[] maxR;

	public ForceStats(int c)
	{
		attract = new float[c * c];
		minR = new float[c * c];
		maxR = new float[c * c];
		Count = c;
	}

	public int Count { get; private set; }
	
	public float this[int y, int x, ForceEnum e]
	{
		get
		{
			switch (e)
			{
				default:
				case ForceEnum.Attract:
					return attract[y * Count + x];
				case ForceEnum.MinR:
					return minR[y * Count + x];
				case ForceEnum.MaxR:
					return maxR[y * Count + x];
			}
		}
		set
		{
			switch (e)
			{
				default:
				case ForceEnum.Attract:
					attract[y * Count + x] = value;
					break;
				case ForceEnum.MinR:
					minR[y * Count + x] = value;
					break;
				case ForceEnum.MaxR:
					maxR[y * Count + x] = value;
					break;
			}
		}
	}
}

public enum ForceEnum
{
	Attract, MinR, MaxR
}

public static class ForceUtils
{
	public static float GetIndex(this NativeList<float> list, int x, int y)
		=> list[y * list.Length + x];

	public static float SetIndex(this NativeList<float> list, int x, int y, float input)
		=> list[y * list.Length + x] = input;
}
