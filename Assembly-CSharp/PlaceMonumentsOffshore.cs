using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlaceMonumentsOffshore : ProceduralComponent
{
	private struct SpawnInfo
	{
		public Prefab prefab;

		public Vector3 position;

		public Quaternion rotation;

		public Vector3 scale;
	}

	public string ResourceFolder = string.Empty;

	public int TargetCount;

	public int MinDistanceFromTerrain = 100;

	public int MaxDistanceFromTerrain = 500;

	public int DistanceBetweenMonuments = 500;

	[FormerlySerializedAs("MinSize")]
	public int MinWorldSize;

	private const int Candidates = 10;

	private const int Attempts = 10000;

	public override void Process(uint seed)
	{
		if (World.Networked)
		{
			World.Spawn("Monument", "assets/bundled/prefabs/autospawn/" + ResourceFolder + "/");
		}
		else
		{
			if (World.Size < MinWorldSize)
			{
				return;
			}
			TerrainHeightMap heightMap = TerrainMeta.HeightMap;
			Prefab<MonumentInfo>[] array = Prefab.Load<MonumentInfo>("assets/bundled/prefabs/autospawn/" + ResourceFolder);
			if (array == null || array.Length == 0)
			{
				return;
			}
			ArrayEx.Shuffle(array, seed);
			ArrayEx.BubbleSort(array);
			Vector3 position = TerrainMeta.Position;
			Vector3 size = TerrainMeta.Size;
			float min = position.x - (float)MaxDistanceFromTerrain;
			float max = position.x - (float)MinDistanceFromTerrain;
			float min2 = position.x + size.x + (float)MinDistanceFromTerrain;
			float max2 = position.x + size.x + (float)MaxDistanceFromTerrain;
			float num = position.z - (float)MaxDistanceFromTerrain;
			int minDistanceFromTerrain = MinDistanceFromTerrain;
			float min3 = position.z + size.z + (float)MinDistanceFromTerrain;
			float max3 = position.z + size.z + (float)MaxDistanceFromTerrain;
			int num2 = 0;
			List<SpawnInfo> a = new List<SpawnInfo>();
			int num3 = 0;
			List<SpawnInfo> b = new List<SpawnInfo>();
			for (int i = 0; i < 10; i++)
			{
				num2 = 0;
				a.Clear();
				Prefab<MonumentInfo>[] array2 = array;
				foreach (Prefab<MonumentInfo> prefab in array2)
				{
					int num4 = (int)((!prefab.Parameters) ? PrefabPriority.Low : (prefab.Parameters.Priority + 1));
					int num5 = num4 * num4 * num4 * num4;
					for (int k = 0; k < 10000; k++)
					{
						float x = 0f;
						float z = 0f;
						switch (seed % 4u)
						{
						case 0u:
							x = SeedRandom.Range(ref seed, min, max);
							z = SeedRandom.Range(ref seed, num, max3);
							break;
						case 1u:
							x = SeedRandom.Range(ref seed, min2, max2);
							z = SeedRandom.Range(ref seed, num, max3);
							break;
						case 2u:
							x = SeedRandom.Range(ref seed, min, max2);
							z = SeedRandom.Range(ref seed, num, num);
							break;
						case 3u:
							x = SeedRandom.Range(ref seed, min, max2);
							z = SeedRandom.Range(ref seed, min3, max3);
							break;
						}
						float normX = TerrainMeta.NormalizeX(x);
						float normZ = TerrainMeta.NormalizeZ(z);
						float height = heightMap.GetHeight(normX, normZ);
						Vector3 pos = new Vector3(x, height, z);
						Quaternion rot = prefab.Object.transform.localRotation;
						Vector3 scale = prefab.Object.transform.localScale;
						if (!CheckRadius(a, pos, DistanceBetweenMonuments))
						{
							prefab.ApplyDecorComponents(ref pos, ref rot, ref scale);
							if ((!prefab.Component || prefab.Component.CheckPlacement(pos, rot, scale)) && !prefab.CheckEnvironmentVolumes(pos, rot, scale, EnvironmentType.Underground))
							{
								SpawnInfo item = default(SpawnInfo);
								item.prefab = prefab;
								item.position = pos;
								item.rotation = rot;
								item.scale = scale;
								a.Add(item);
								num2 += num5;
								break;
							}
						}
					}
					if (TargetCount > 0 && a.Count >= TargetCount)
					{
						break;
					}
				}
				if (num2 > num3)
				{
					num3 = num2;
					GenericsUtil.Swap(ref a, ref b);
				}
			}
			foreach (SpawnInfo item2 in b)
			{
				World.AddPrefab("Monument", item2.prefab, item2.position, item2.rotation, item2.scale);
			}
		}
	}

	public bool CheckRadius(List<SpawnInfo> spawns, Vector3 pos, float radius)
	{
		float num = radius * radius;
		foreach (SpawnInfo spawn in spawns)
		{
			if ((spawn.position - pos).sqrMagnitude < num)
			{
				return true;
			}
		}
		return false;
	}
}
