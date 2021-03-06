using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GenerateDungeon : ProceduralComponent
{
	private class PathNode
	{
		public MonumentInfo monument;

		public PathFinder.Node node;
	}

	private class PathSegment
	{
		public PathFinder.Node start;

		public PathFinder.Node end;
	}

	private class PathLink
	{
		public PathLinkSide downwards;

		public PathLinkSide upwards;
	}

	private class PathLinkSide
	{
		public PathLinkSegment origin;

		public List<PathLinkSegment> segments;

		public PathLinkSegment prevSegment
		{
			get
			{
				if (segments.Count <= 0)
				{
					return origin;
				}
				return segments[segments.Count - 1];
			}
		}
	}

	private class PathLinkSegment
	{
		public Vector3 position;

		public Quaternion rotation;

		public Vector3 scale;

		public Prefab<DungeonLink> prefab;

		public DungeonLink link;

		public Transform downSocket => link.DownSocket;

		public Transform upSocket => link.UpSocket;

		public DungeonLinkType downType => link.DownType;

		public DungeonLinkType upType => link.UpType;
	}

	public string TunnelFolder = string.Empty;

	public string StationFolder = string.Empty;

	public string TransitionFolder = string.Empty;

	public string LinkFolder = string.Empty;

	public InfrastructureType ConnectionType = InfrastructureType.Tunnel;

	public int CellSize = 216;

	public float LinkHeight = 1.5f;

	public float LinkRadius = 3f;

	public float LinkTransition = 9f;

	private const int MaxDepth = 100000;

	public override bool RunOnCache => true;

	public override void Process(uint seed)
	{
		if (World.Cached)
		{
			TerrainMeta.Path.DungeonRoot = HierarchyUtil.GetRoot("Dungeon");
			return;
		}
		if (World.Networked)
		{
			World.Spawn("Dungeon");
			TerrainMeta.Path.DungeonRoot = HierarchyUtil.GetRoot("Dungeon");
			return;
		}
		Prefab<DungeonCell>[] array = Prefab.Load<DungeonCell>("assets/bundled/prefabs/autospawn/" + TunnelFolder);
		if (array == null || array.Length == 0)
		{
			return;
		}
		Prefab<DungeonCell>[] array2 = Prefab.Load<DungeonCell>("assets/bundled/prefabs/autospawn/" + StationFolder);
		if (array2 == null || array2.Length == 0)
		{
			return;
		}
		Prefab<DungeonCell>[] array3 = Prefab.Load<DungeonCell>("assets/bundled/prefabs/autospawn/" + TransitionFolder);
		if (array3 == null)
		{
			return;
		}
		Prefab<DungeonLink>[] array4 = Prefab.Load<DungeonLink>("assets/bundled/prefabs/autospawn/" + LinkFolder);
		if (array4 == null)
		{
			return;
		}
		array4 = array4.OrderByDescending((Prefab<DungeonLink> x) => x.Component.Priority).ToArray();
		List<DungeonInfo> list = (TerrainMeta.Path ? TerrainMeta.Path.DungeonEntrances : null);
		WorldSpaceGrid<Prefab<DungeonCell>> worldSpaceGrid = new WorldSpaceGrid<Prefab<DungeonCell>>(TerrainMeta.Size.x * 2f, CellSize);
		int[,] array5 = new int[worldSpaceGrid.CellCount, worldSpaceGrid.CellCount];
		_003C_003Ec__DisplayClass17_0 _003C_003Ec__DisplayClass17_ = default(_003C_003Ec__DisplayClass17_0);
		_003C_003Ec__DisplayClass17_.hashmap = new DungeonConnectionHash[worldSpaceGrid.CellCount, worldSpaceGrid.CellCount];
		_003C_003Ec__DisplayClass17_.pathFinder = new PathFinder(array5, false);
		int cellCount = worldSpaceGrid.CellCount;
		int num = 0;
		int num2 = worldSpaceGrid.CellCount - 1;
		for (int i = 0; i < cellCount; i++)
		{
			for (int j = 0; j < cellCount; j++)
			{
				array5[j, i] = 1;
			}
		}
		List<PathSegment> list2 = new List<PathSegment>();
		List<PathLink> list3 = new List<PathLink>();
		List<PathNode> list4 = new List<PathNode>();
		_003C_003Ec__DisplayClass17_.unconnectedNodeList = new List<PathNode>();
		_003C_003Ec__DisplayClass17_.secondaryNodeList = new List<PathNode>();
		List<PathFinder.Point> list5 = new List<PathFinder.Point>();
		List<PathFinder.Point> list6 = new List<PathFinder.Point>();
		List<PathFinder.Point> list7 = new List<PathFinder.Point>();
		_003C_003Ec__DisplayClass17_1 _003C_003Ec__DisplayClass17_2 = default(_003C_003Ec__DisplayClass17_1);
		_003C_003Ec__DisplayClass17_3 _003C_003Ec__DisplayClass17_3 = default(_003C_003Ec__DisplayClass17_3);
		foreach (DungeonInfo item in list)
		{
			_003C_003Ec__DisplayClass17_2.entrance = item;
			TerrainPathConnect[] componentsInChildren = _003C_003Ec__DisplayClass17_2.entrance.GetComponentsInChildren<TerrainPathConnect>(true);
			foreach (TerrainPathConnect terrainPathConnect in componentsInChildren)
			{
				_003C_003Ec__DisplayClass17_2 CS_0024_003C_003E8__locals0 = new _003C_003Ec__DisplayClass17_2();
				CS_0024_003C_003E8__locals0._003C_003E4__this = this;
				if (terrainPathConnect.Type != ConnectionType)
				{
					continue;
				}
				Vector2i cellPos = worldSpaceGrid.WorldToGridCoords(terrainPathConnect.transform.position);
				if (array5[cellPos.x, cellPos.y] == int.MaxValue)
				{
					continue;
				}
				CS_0024_003C_003E8__locals0.stationNode = _003C_003Ec__DisplayClass17_.pathFinder.FindClosestWalkable(new PathFinder.Point(cellPos.x, cellPos.y), 1);
				if (CS_0024_003C_003E8__locals0.stationNode == null)
				{
					continue;
				}
				Prefab<DungeonCell> prefab = ((cellPos.x > num) ? worldSpaceGrid[cellPos.x - 1, cellPos.y] : null);
				Prefab<DungeonCell> prefab2 = ((cellPos.x < num2) ? worldSpaceGrid[cellPos.x + 1, cellPos.y] : null);
				Prefab<DungeonCell> prefab3 = ((cellPos.y > num) ? worldSpaceGrid[cellPos.x, cellPos.y - 1] : null);
				Prefab<DungeonCell> prefab4 = ((cellPos.y < num2) ? worldSpaceGrid[cellPos.x, cellPos.y + 1] : null);
				Prefab<DungeonCell> prefab5 = null;
				float num3 = float.MaxValue;
				ArrayEx.Shuffle(array2, ref seed);
				Prefab<DungeonCell>[] array6 = array2;
				foreach (Prefab<DungeonCell> prefab6 in array6)
				{
					if ((prefab != null && prefab6.Component.West != prefab.Component.East) || (prefab2 != null && prefab6.Component.East != prefab2.Component.West) || (prefab3 != null && prefab6.Component.South != prefab3.Component.North) || (prefab4 != null && prefab6.Component.North != prefab4.Component.South))
					{
						continue;
					}
					DungeonLinkBlockVolume componentInChildren = prefab6.Object.GetComponentInChildren<DungeonLinkBlockVolume>();
					DungeonLinkBlockVolume componentInChildren2 = _003C_003Ec__DisplayClass17_2.entrance.GetComponentInChildren<DungeonLinkBlockVolume>();
					OBB bounds = componentInChildren.GetBounds(worldSpaceGrid.GridToWorldCoords(cellPos), Quaternion.identity);
					OBB bounds2 = componentInChildren2.GetBounds(_003C_003Ec__DisplayClass17_2.entrance.transform.position, Quaternion.identity);
					if (!bounds.Intersects2D(bounds2))
					{
						DungeonLink componentInChildren3 = prefab6.Object.GetComponentInChildren<DungeonLink>();
						Vector3 vector = worldSpaceGrid.GridToWorldCoords(new Vector2i(cellPos.x, cellPos.y)) + componentInChildren3.UpSocket.localPosition;
						float num4 = (terrainPathConnect.transform.position - vector).Magnitude2D();
						if (!(num3 < num4))
						{
							prefab5 = prefab6;
							num3 = num4;
						}
					}
				}
				if (prefab5 != null)
				{
					worldSpaceGrid[cellPos.x, cellPos.y] = prefab5;
					array5[cellPos.x, cellPos.y] = int.MaxValue;
					_003C_003Ec__DisplayClass17_3.isStartPoint = _003C_003Ec__DisplayClass17_.secondaryNodeList.Count == 0;
					_003C_003Ec__DisplayClass17_.secondaryNodeList.RemoveAll((PathNode x) => x.node.point == CS_0024_003C_003E8__locals0.stationNode.point);
					_003C_003Ec__DisplayClass17_.unconnectedNodeList.RemoveAll((PathNode x) => x.node.point == CS_0024_003C_003E8__locals0.stationNode.point);
					if (prefab5.Component.West != 0)
					{
						CS_0024_003C_003E8__locals0._003CProcess_003Eg__AddNode_007C1(cellPos.x - 1, cellPos.y, ref _003C_003Ec__DisplayClass17_, ref _003C_003Ec__DisplayClass17_2, ref _003C_003Ec__DisplayClass17_3);
					}
					if (prefab5.Component.East != 0)
					{
						CS_0024_003C_003E8__locals0._003CProcess_003Eg__AddNode_007C1(cellPos.x + 1, cellPos.y, ref _003C_003Ec__DisplayClass17_, ref _003C_003Ec__DisplayClass17_2, ref _003C_003Ec__DisplayClass17_3);
					}
					if (prefab5.Component.South != 0)
					{
						CS_0024_003C_003E8__locals0._003CProcess_003Eg__AddNode_007C1(cellPos.x, cellPos.y - 1, ref _003C_003Ec__DisplayClass17_, ref _003C_003Ec__DisplayClass17_2, ref _003C_003Ec__DisplayClass17_3);
					}
					if (prefab5.Component.North != 0)
					{
						CS_0024_003C_003E8__locals0._003CProcess_003Eg__AddNode_007C1(cellPos.x, cellPos.y + 1, ref _003C_003Ec__DisplayClass17_, ref _003C_003Ec__DisplayClass17_2, ref _003C_003Ec__DisplayClass17_3);
					}
					PathLink pathLink = new PathLink();
					DungeonLink componentInChildren4 = _003C_003Ec__DisplayClass17_2.entrance.gameObject.GetComponentInChildren<DungeonLink>();
					Vector3 position = _003C_003Ec__DisplayClass17_2.entrance.transform.position;
					Vector3 eulerAngles = _003C_003Ec__DisplayClass17_2.entrance.transform.rotation.eulerAngles;
					DungeonLink componentInChildren5 = prefab5.Object.GetComponentInChildren<DungeonLink>();
					Vector3 position2 = worldSpaceGrid.GridToWorldCoords(new Vector2i(cellPos.x, cellPos.y));
					Vector3 zero = Vector3.zero;
					pathLink.downwards = new PathLinkSide();
					pathLink.downwards.origin = new PathLinkSegment();
					pathLink.downwards.origin.position = position;
					pathLink.downwards.origin.rotation = Quaternion.Euler(eulerAngles);
					pathLink.downwards.origin.scale = Vector3.one;
					pathLink.downwards.origin.link = componentInChildren4;
					pathLink.downwards.segments = new List<PathLinkSegment>();
					pathLink.upwards = new PathLinkSide();
					pathLink.upwards.origin = new PathLinkSegment();
					pathLink.upwards.origin.position = position2;
					pathLink.upwards.origin.rotation = Quaternion.Euler(zero);
					pathLink.upwards.origin.scale = Vector3.one;
					pathLink.upwards.origin.link = componentInChildren5;
					pathLink.upwards.segments = new List<PathLinkSegment>();
					list3.Add(pathLink);
				}
			}
		}
		while (_003C_003Ec__DisplayClass17_.unconnectedNodeList.Count != 0 || _003C_003Ec__DisplayClass17_.secondaryNodeList.Count != 0)
		{
			if (_003C_003Ec__DisplayClass17_.unconnectedNodeList.Count == 0)
			{
				PathNode node3 = _003C_003Ec__DisplayClass17_.secondaryNodeList[0];
				_003C_003Ec__DisplayClass17_.unconnectedNodeList.AddRange(_003C_003Ec__DisplayClass17_.secondaryNodeList.Where((PathNode x) => x.monument == node3.monument));
				_003C_003Ec__DisplayClass17_.secondaryNodeList.RemoveAll((PathNode x) => x.monument == node3.monument);
				Vector2i vector2i = worldSpaceGrid.WorldToGridCoords(node3.monument.transform.position);
				_003C_003Ec__DisplayClass17_.pathFinder.PushPoint = new PathFinder.Point(vector2i.x, vector2i.y);
				_003C_003Ec__DisplayClass17_.pathFinder.PushRadius = 2;
				_003C_003Ec__DisplayClass17_.pathFinder.PushDistance = 2;
				_003C_003Ec__DisplayClass17_.pathFinder.PushMultiplier = 4;
			}
			list7.Clear();
			list7.AddRange(_003C_003Ec__DisplayClass17_.unconnectedNodeList.Select((PathNode x) => x.node.point));
			list6.Clear();
			list6.AddRange(list4.Select((PathNode x) => x.node.point));
			list6.AddRange(_003C_003Ec__DisplayClass17_.secondaryNodeList.Select((PathNode x) => x.node.point));
			list6.AddRange(list5);
			PathFinder.Node node4 = _003C_003Ec__DisplayClass17_.pathFinder.FindPathUndirected(list6, list7, 100000);
			if (node4 == null)
			{
				PathNode node2 = _003C_003Ec__DisplayClass17_.unconnectedNodeList[0];
				_003C_003Ec__DisplayClass17_.secondaryNodeList.AddRange(_003C_003Ec__DisplayClass17_.unconnectedNodeList.Where((PathNode x) => x.monument == node2.monument));
				_003C_003Ec__DisplayClass17_.unconnectedNodeList.RemoveAll((PathNode x) => x.monument == node2.monument);
				_003C_003Ec__DisplayClass17_.secondaryNodeList.Remove(node2);
				list4.Add(node2);
				continue;
			}
			PathSegment segment = new PathSegment();
			for (PathFinder.Node node5 = node4; node5 != null; node5 = node5.next)
			{
				if (node5 == node4)
				{
					segment.start = node5;
				}
				if (node5.next == null)
				{
					segment.end = node5;
				}
			}
			list2.Add(segment);
			PathNode node = _003C_003Ec__DisplayClass17_.unconnectedNodeList.Find((PathNode x) => x.node.point == segment.start.point || x.node.point == segment.end.point);
			_003C_003Ec__DisplayClass17_.secondaryNodeList.AddRange(_003C_003Ec__DisplayClass17_.unconnectedNodeList.Where((PathNode x) => x.monument == node.monument));
			_003C_003Ec__DisplayClass17_.unconnectedNodeList.RemoveAll((PathNode x) => x.monument == node.monument);
			_003C_003Ec__DisplayClass17_.secondaryNodeList.Remove(node);
			list4.Add(node);
			PathNode pathNode = _003C_003Ec__DisplayClass17_.secondaryNodeList.Find((PathNode x) => x.node.point == segment.start.point || x.node.point == segment.end.point);
			if (pathNode != null)
			{
				_003C_003Ec__DisplayClass17_.secondaryNodeList.Remove(pathNode);
				list4.Add(pathNode);
			}
			for (PathFinder.Node node6 = node4; node6 != null; node6 = node6.next)
			{
				if (node6 != node4 && node6.next != null)
				{
					list5.Add(node6.point);
				}
			}
		}
		foreach (PathSegment item2 in list2)
		{
			PathFinder.Node node7 = item2.start;
			while (node7 != null && node7.next != null)
			{
				DungeonConnectionHash dungeonConnectionHash = _003C_003Ec__DisplayClass17_.hashmap[node7.point.x, node7.point.y];
				DungeonConnectionHash dungeonConnectionHash2 = _003C_003Ec__DisplayClass17_.hashmap[node7.next.point.x, node7.next.point.y];
				if (node7.point.x > node7.next.point.x)
				{
					dungeonConnectionHash.West = true;
					dungeonConnectionHash2.East = true;
				}
				if (node7.point.x < node7.next.point.x)
				{
					dungeonConnectionHash.East = true;
					dungeonConnectionHash2.West = true;
				}
				if (node7.point.y > node7.next.point.y)
				{
					dungeonConnectionHash.South = true;
					dungeonConnectionHash2.North = true;
				}
				if (node7.point.y < node7.next.point.y)
				{
					dungeonConnectionHash.North = true;
					dungeonConnectionHash2.South = true;
				}
				_003C_003Ec__DisplayClass17_.hashmap[node7.point.x, node7.point.y] = dungeonConnectionHash;
				_003C_003Ec__DisplayClass17_.hashmap[node7.next.point.x, node7.next.point.y] = dungeonConnectionHash2;
				node7 = node7.next;
			}
		}
		for (int m = 0; m < worldSpaceGrid.CellCount; m++)
		{
			for (int n = 0; n < worldSpaceGrid.CellCount; n++)
			{
				if (array5[m, n] == int.MaxValue)
				{
					continue;
				}
				DungeonConnectionHash dungeonConnectionHash3 = _003C_003Ec__DisplayClass17_.hashmap[m, n];
				if (dungeonConnectionHash3.Value == 0)
				{
					continue;
				}
				ArrayEx.Shuffle(array, ref seed);
				Prefab<DungeonCell>[] array6 = array;
				foreach (Prefab<DungeonCell> prefab7 in array6)
				{
					Prefab<DungeonCell> prefab8 = ((m > num) ? worldSpaceGrid[m - 1, n] : null);
					if (((prefab8 != null) ? ((prefab7.Component.West == prefab8.Component.East) ? 1 : 0) : (dungeonConnectionHash3.West ? ((int)prefab7.Component.West) : ((prefab7.Component.West == DungeonConnectionType.None) ? 1 : 0))) == 0)
					{
						continue;
					}
					Prefab<DungeonCell> prefab9 = ((m < num2) ? worldSpaceGrid[m + 1, n] : null);
					if (((prefab9 != null) ? ((prefab7.Component.East == prefab9.Component.West) ? 1 : 0) : (dungeonConnectionHash3.East ? ((int)prefab7.Component.East) : ((prefab7.Component.East == DungeonConnectionType.None) ? 1 : 0))) == 0)
					{
						continue;
					}
					Prefab<DungeonCell> prefab10 = ((n > num) ? worldSpaceGrid[m, n - 1] : null);
					if (((prefab10 != null) ? ((prefab7.Component.South == prefab10.Component.North) ? 1 : 0) : (dungeonConnectionHash3.South ? ((int)prefab7.Component.South) : ((prefab7.Component.South == DungeonConnectionType.None) ? 1 : 0))) == 0)
					{
						continue;
					}
					Prefab<DungeonCell> prefab11 = ((n < num2) ? worldSpaceGrid[m, n + 1] : null);
					if (((prefab11 != null) ? ((prefab7.Component.North == prefab11.Component.South) ? 1 : 0) : (dungeonConnectionHash3.North ? ((int)prefab7.Component.North) : ((prefab7.Component.North == DungeonConnectionType.None) ? 1 : 0))) != 0 && (prefab7.Component.West == DungeonConnectionType.None || prefab8 == null || !prefab7.Component.ShouldAvoid(prefab8.ID)) && (prefab7.Component.East == DungeonConnectionType.None || prefab9 == null || !prefab7.Component.ShouldAvoid(prefab9.ID)) && (prefab7.Component.South == DungeonConnectionType.None || prefab10 == null || !prefab7.Component.ShouldAvoid(prefab10.ID)) && (prefab7.Component.North == DungeonConnectionType.None || prefab11 == null || !prefab7.Component.ShouldAvoid(prefab11.ID)))
					{
						worldSpaceGrid[m, n] = prefab7;
						bool num5 = prefab8 == null || prefab7.Component.WestVariant == prefab8.Component.EastVariant;
						bool flag = prefab10 == null || prefab7.Component.SouthVariant == prefab10.Component.NorthVariant;
						if (num5 && flag)
						{
							break;
						}
					}
				}
			}
		}
		Vector3 zero2 = Vector3.zero;
		Vector3 zero3 = Vector3.zero;
		Vector3 vector2 = Vector3.up * 10f;
		Vector3 vector3 = Vector3.up * (LinkTransition + 1f);
		do
		{
			zero3 = zero2;
			for (int num6 = 0; num6 < worldSpaceGrid.CellCount; num6++)
			{
				for (int num7 = 0; num7 < worldSpaceGrid.CellCount; num7++)
				{
					Prefab<DungeonCell> prefab12 = worldSpaceGrid[num6, num7];
					if (prefab12 != null)
					{
						Vector2i cellPos2 = new Vector2i(num6, num7);
						Vector3 vector4 = worldSpaceGrid.GridToWorldCoords(cellPos2);
						while (!prefab12.CheckEnvironmentVolumesInsideTerrain(zero2 + vector4 + vector2, Quaternion.identity, Vector3.one, EnvironmentType.Underground) || prefab12.CheckEnvironmentVolumes(zero2 + vector4 + vector3, Quaternion.identity, Vector3.one, EnvironmentType.Underground | EnvironmentType.Building) || prefab12.CheckEnvironmentVolumes(zero2 + vector4, Quaternion.identity, Vector3.one, EnvironmentType.Underground | EnvironmentType.Building))
						{
							zero2.y -= 9f;
						}
					}
				}
			}
		}
		while (zero2 != zero3);
		foreach (PathLink item3 in list3)
		{
			item3.upwards.origin.position += zero2;
		}
		for (int num8 = 0; num8 < worldSpaceGrid.CellCount; num8++)
		{
			for (int num9 = 0; num9 < worldSpaceGrid.CellCount; num9++)
			{
				Prefab<DungeonCell> prefab13 = worldSpaceGrid[num8, num9];
				if (prefab13 != null)
				{
					Vector2i cellPos3 = new Vector2i(num8, num9);
					Vector3 vector5 = worldSpaceGrid.GridToWorldCoords(cellPos3);
					World.AddPrefab("Dungeon", prefab13, zero2 + vector5, Quaternion.identity, Vector3.one);
				}
			}
		}
		for (int num10 = 0; num10 < worldSpaceGrid.CellCount - 1; num10++)
		{
			for (int num11 = 0; num11 < worldSpaceGrid.CellCount - 1; num11++)
			{
				Prefab<DungeonCell> prefab14 = worldSpaceGrid[num10, num11];
				Prefab<DungeonCell> prefab15 = worldSpaceGrid[num10 + 1, num11];
				Prefab<DungeonCell> prefab16 = worldSpaceGrid[num10, num11 + 1];
				Prefab<DungeonCell>[] array6;
				if (prefab14 != null && prefab15 != null && prefab14.Component.EastVariant != prefab15.Component.WestVariant)
				{
					ArrayEx.Shuffle(array3, ref seed);
					array6 = array3;
					foreach (Prefab<DungeonCell> prefab17 in array6)
					{
						if (prefab17.Component.West == prefab14.Component.East && prefab17.Component.East == prefab15.Component.West && prefab17.Component.WestVariant == prefab14.Component.EastVariant && prefab17.Component.EastVariant == prefab15.Component.WestVariant)
						{
							Vector2i cellPos4 = new Vector2i(num10, num11);
							Vector3 vector6 = worldSpaceGrid.GridToWorldCoords(cellPos4) + new Vector3(worldSpaceGrid.CellSizeHalf, 0f, 0f);
							World.AddPrefab("Dungeon", prefab17, zero2 + vector6, Quaternion.identity, Vector3.one);
							break;
						}
					}
				}
				if (prefab14 == null || prefab16 == null || prefab14.Component.NorthVariant == prefab16.Component.SouthVariant)
				{
					continue;
				}
				ArrayEx.Shuffle(array3, ref seed);
				array6 = array3;
				foreach (Prefab<DungeonCell> prefab18 in array6)
				{
					if (prefab18.Component.South == prefab14.Component.North && prefab18.Component.North == prefab16.Component.South && prefab18.Component.SouthVariant == prefab14.Component.NorthVariant && prefab18.Component.NorthVariant == prefab16.Component.SouthVariant)
					{
						Vector2i cellPos5 = new Vector2i(num10, num11);
						Vector3 vector7 = worldSpaceGrid.GridToWorldCoords(cellPos5) + new Vector3(0f, 0f, worldSpaceGrid.CellSizeHalf);
						World.AddPrefab("Dungeon", prefab18, zero2 + vector7, Quaternion.identity, Vector3.one);
						break;
					}
				}
			}
		}
		foreach (PathLink item4 in list3)
		{
			Vector3 vector8 = item4.upwards.origin.position + item4.upwards.origin.rotation * Vector3.Scale(item4.upwards.origin.upSocket.localPosition, item4.upwards.origin.scale);
			Vector3 vector9 = item4.downwards.origin.position + item4.downwards.origin.rotation * Vector3.Scale(item4.downwards.origin.downSocket.localPosition, item4.downwards.origin.scale) - vector8;
			Vector3[] array7 = new Vector3[2]
			{
				new Vector3(0f, 1f, 0f),
				new Vector3(1f, 1f, 1f)
			};
			for (int k = 0; k < array7.Length; k++)
			{
				Vector3 b = array7[k];
				int num12 = 0;
				int num13 = 0;
				while (vector9.magnitude > 1f && (num12 < 8 || num13 < 8))
				{
					bool flag2 = num12 > 2 && num13 > 2;
					bool flag3 = num12 > 4 && num13 > 4;
					Prefab<DungeonLink> prefab19 = null;
					Vector3 vector10 = Vector3.zero;
					int num14 = int.MinValue;
					Vector3 position3 = Vector3.zero;
					Quaternion rotation = Quaternion.identity;
					PathLinkSegment prevSegment = item4.downwards.prevSegment;
					Vector3 vector11 = prevSegment.position + prevSegment.rotation * Vector3.Scale(prevSegment.scale, prevSegment.downSocket.localPosition);
					Quaternion quaternion = prevSegment.rotation * prevSegment.downSocket.localRotation;
					Prefab<DungeonLink>[] array8 = array4;
					foreach (Prefab<DungeonLink> prefab20 in array8)
					{
						float num15 = SeedRandom.Value(ref seed);
						DungeonLink component = prefab20.Component;
						if (prevSegment.downType != component.UpType)
						{
							continue;
						}
						switch (component.DownType)
						{
						case DungeonLinkType.Elevator:
							if (flag2 || b.x != 0f || b.z != 0f)
							{
								continue;
							}
							break;
						case DungeonLinkType.Transition:
							if (b.x != 0f || b.z != 0f)
							{
								continue;
							}
							break;
						}
						int num16 = ((!flag2) ? component.Priority : 0);
						if (num14 > num16)
						{
							continue;
						}
						Quaternion quaternion2 = quaternion * Quaternion.Inverse(component.UpSocket.localRotation);
						Quaternion quaternion3 = quaternion2 * component.DownSocket.localRotation;
						PathLinkSegment prevSegment2 = item4.upwards.prevSegment;
						Quaternion quaternion4 = prevSegment2.rotation * prevSegment2.upSocket.localRotation;
						if (component.Rotation > 0)
						{
							if (Quaternion.Angle(quaternion4, quaternion3) > (float)component.Rotation)
							{
								continue;
							}
							Quaternion quaternion5 = quaternion4 * Quaternion.Inverse(quaternion3);
							quaternion2 *= quaternion5;
							quaternion3 *= quaternion5;
						}
						Vector3 vector12 = vector11 - quaternion2 * component.UpSocket.localPosition;
						Vector3 vector13 = quaternion2 * (component.DownSocket.localPosition - component.UpSocket.localPosition);
						Vector3 a = vector9 + vector10;
						Vector3 a2 = vector9 + vector13;
						float magnitude = a.magnitude;
						float magnitude2 = a2.magnitude;
						Vector3 vector14 = Vector3.Scale(a, b);
						Vector3 vector15 = Vector3.Scale(a2, b);
						float magnitude3 = vector14.magnitude;
						float magnitude4 = vector15.magnitude;
						if (vector10 != Vector3.zero)
						{
							if (magnitude3 < magnitude4 || (magnitude3 == magnitude4 && magnitude < magnitude2) || (magnitude3 == magnitude4 && magnitude == magnitude2 && num15 < 0.5f))
							{
								continue;
							}
						}
						else if (magnitude3 <= magnitude4)
						{
							continue;
						}
						if (Mathf.Abs(vector15.x) - Mathf.Abs(vector14.x) > 0.01f || (Mathf.Abs(vector15.x) > 0.01f && a.x * a2.x < 0f) || Mathf.Abs(vector15.y) - Mathf.Abs(vector14.y) > 0.01f || (Mathf.Abs(vector15.y) > 0.01f && a.y * a2.y < 0f) || Mathf.Abs(vector15.z) - Mathf.Abs(vector14.z) > 0.01f || (Mathf.Abs(vector15.z) > 0.01f && a.z * a2.z < 0f) || (flag2 && b.x == 0f && b.z == 0f && component.DownType == DungeonLinkType.Default && ((Mathf.Abs(a2.x) > 0.01f && Mathf.Abs(a2.x) < LinkRadius * 2f - 0.1f) || (Mathf.Abs(a2.z) > 0.01f && Mathf.Abs(a2.z) < LinkRadius * 2f - 0.1f))))
						{
							continue;
						}
						num14 = num16;
						if (b.x == 0f && b.z == 0f)
						{
							if (!flag2 && Mathf.Abs(a2.y) < LinkTransition - 0.1f)
							{
								continue;
							}
						}
						else if ((!flag2 && magnitude4 > 0.01f && (Mathf.Abs(a2.x) < LinkRadius * 2f - 0.1f || Mathf.Abs(a2.z) < LinkRadius * 2f - 0.1f)) || (!flag3 && magnitude4 > 0.01f && (Mathf.Abs(a2.x) < LinkRadius * 1f - 0.1f || Mathf.Abs(a2.z) < LinkRadius * 1f - 0.1f)))
						{
							continue;
						}
						if (!flag2 || !(magnitude4 < 0.01f) || !(magnitude2 < 0.01f) || !(Quaternion.Angle(quaternion4, quaternion3) > 10f))
						{
							prefab19 = prefab20;
							vector10 = vector13;
							num14 = num16;
							position3 = vector12;
							rotation = quaternion2;
						}
					}
					if (vector10 != Vector3.zero)
					{
						PathLinkSegment pathLinkSegment = new PathLinkSegment();
						pathLinkSegment.position = position3;
						pathLinkSegment.rotation = rotation;
						pathLinkSegment.scale = Vector3.one;
						pathLinkSegment.prefab = prefab19;
						pathLinkSegment.link = prefab19.Component;
						item4.downwards.segments.Add(pathLinkSegment);
						vector9 += vector10;
					}
					else
					{
						num13++;
					}
					if (b.x > 0f || b.z > 0f)
					{
						Prefab<DungeonLink> prefab21 = null;
						Vector3 vector16 = Vector3.zero;
						int num17 = int.MinValue;
						Vector3 position4 = Vector3.zero;
						Quaternion rotation2 = Quaternion.identity;
						PathLinkSegment prevSegment3 = item4.upwards.prevSegment;
						Vector3 vector17 = prevSegment3.position + prevSegment3.rotation * Vector3.Scale(prevSegment3.scale, prevSegment3.upSocket.localPosition);
						Quaternion quaternion6 = prevSegment3.rotation * prevSegment3.upSocket.localRotation;
						array8 = array4;
						foreach (Prefab<DungeonLink> prefab22 in array8)
						{
							float num18 = SeedRandom.Value(ref seed);
							DungeonLink component2 = prefab22.Component;
							if (prevSegment3.upType != component2.DownType)
							{
								continue;
							}
							switch (component2.DownType)
							{
							case DungeonLinkType.Elevator:
								if (flag2 || b.x != 0f || b.z != 0f)
								{
									continue;
								}
								break;
							case DungeonLinkType.Transition:
								if (b.x != 0f || b.z != 0f)
								{
									continue;
								}
								break;
							}
							int num19 = ((!flag2) ? component2.Priority : 0);
							if (num17 > num19)
							{
								continue;
							}
							Quaternion quaternion7 = quaternion6 * Quaternion.Inverse(component2.DownSocket.localRotation);
							Quaternion quaternion8 = quaternion7 * component2.UpSocket.localRotation;
							PathLinkSegment prevSegment4 = item4.downwards.prevSegment;
							Quaternion quaternion9 = prevSegment4.rotation * prevSegment4.downSocket.localRotation;
							if (component2.Rotation > 0)
							{
								if (Quaternion.Angle(quaternion9, quaternion8) > (float)component2.Rotation)
								{
									continue;
								}
								Quaternion quaternion10 = quaternion9 * Quaternion.Inverse(quaternion8);
								quaternion7 *= quaternion10;
								quaternion8 *= quaternion10;
							}
							Vector3 vector18 = vector17 - quaternion7 * component2.DownSocket.localPosition;
							Vector3 vector19 = quaternion7 * (component2.UpSocket.localPosition - component2.DownSocket.localPosition);
							Vector3 a3 = vector9 - vector16;
							Vector3 a4 = vector9 - vector19;
							float magnitude5 = a3.magnitude;
							float magnitude6 = a4.magnitude;
							Vector3 vector20 = Vector3.Scale(a3, b);
							Vector3 vector21 = Vector3.Scale(a4, b);
							float magnitude7 = vector20.magnitude;
							float magnitude8 = vector21.magnitude;
							if (vector16 != Vector3.zero)
							{
								if (magnitude7 < magnitude8 || (magnitude7 == magnitude8 && magnitude5 < magnitude6) || (magnitude7 == magnitude8 && magnitude5 == magnitude6 && num18 < 0.5f))
								{
									continue;
								}
							}
							else if (magnitude7 <= magnitude8)
							{
								continue;
							}
							if (Mathf.Abs(vector21.x) - Mathf.Abs(vector20.x) > 0.01f || (Mathf.Abs(vector21.x) > 0.01f && a3.x * a4.x < 0f) || Mathf.Abs(vector21.y) - Mathf.Abs(vector20.y) > 0.01f || (Mathf.Abs(vector21.y) > 0.01f && a3.y * a4.y < 0f) || Mathf.Abs(vector21.z) - Mathf.Abs(vector20.z) > 0.01f || (Mathf.Abs(vector21.z) > 0.01f && a3.z * a4.z < 0f) || (flag2 && b.x == 0f && b.z == 0f && component2.UpType == DungeonLinkType.Default && ((Mathf.Abs(a4.x) > 0.01f && Mathf.Abs(a4.x) < LinkRadius * 2f - 0.1f) || (Mathf.Abs(a4.z) > 0.01f && Mathf.Abs(a4.z) < LinkRadius * 2f - 0.1f))))
							{
								continue;
							}
							num17 = num19;
							if (b.x == 0f && b.z == 0f)
							{
								if (!flag2 && Mathf.Abs(a4.y) < LinkTransition - 0.1f)
								{
									continue;
								}
							}
							else if ((!flag2 && magnitude8 > 0.01f && (Mathf.Abs(a4.x) < LinkRadius * 2f - 0.1f || Mathf.Abs(a4.z) < LinkRadius * 2f - 0.1f)) || (!flag3 && magnitude8 > 0.01f && (Mathf.Abs(a4.x) < LinkRadius * 1f - 0.1f || Mathf.Abs(a4.z) < LinkRadius * 1f - 0.1f)))
							{
								continue;
							}
							if (!flag2 || !(magnitude8 < 0.01f) || !(magnitude6 < 0.01f) || !(Quaternion.Angle(quaternion9, quaternion8) > 10f))
							{
								prefab21 = prefab22;
								vector16 = vector19;
								num17 = num19;
								position4 = vector18;
								rotation2 = quaternion7;
							}
						}
						if (vector16 != Vector3.zero)
						{
							PathLinkSegment pathLinkSegment2 = new PathLinkSegment();
							pathLinkSegment2.position = position4;
							pathLinkSegment2.rotation = rotation2;
							pathLinkSegment2.scale = Vector3.one;
							pathLinkSegment2.prefab = prefab21;
							pathLinkSegment2.link = prefab21.Component;
							item4.upwards.segments.Add(pathLinkSegment2);
							vector9 -= vector16;
						}
						else
						{
							num12++;
						}
					}
					else
					{
						num12++;
					}
				}
			}
		}
		foreach (PathLink item5 in list3)
		{
			foreach (PathLinkSegment segment2 in item5.downwards.segments)
			{
				World.AddPrefab("Dungeon", segment2.prefab, segment2.position, segment2.rotation, segment2.scale);
			}
			foreach (PathLinkSegment segment3 in item5.upwards.segments)
			{
				World.AddPrefab("Dungeon", segment3.prefab, segment3.position, segment3.rotation, segment3.scale);
			}
		}
		if ((bool)TerrainMeta.Path)
		{
			TerrainMeta.Path.DungeonRoot = HierarchyUtil.GetRoot("Dungeon");
		}
	}
}
