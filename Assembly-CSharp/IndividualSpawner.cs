using UnityEngine;

public class IndividualSpawner : BaseMonoBehaviour, IServerComponent, ISpawnPointUser, ISpawnGroup
{
	public GameObjectRef entityPrefab;

	public float respawnDelayMin = 10f;

	public float respawnDelayMax = 20f;

	public bool useCustomBoundsCheckMask;

	public LayerMask customBoundsCheckMask;

	private SpawnPointInstance spawnInstance;

	private float nextSpawnTime = -1f;

	public int currentPopulation
	{
		get
		{
			if (!(spawnInstance == null))
			{
				return 1;
			}
			return 0;
		}
	}

	private bool IsSpawned => spawnInstance != null;

	protected void Awake()
	{
		if ((bool)SingletonComponent<SpawnHandler>.Instance)
		{
			SingletonComponent<SpawnHandler>.Instance.SpawnGroups.Add(this);
		}
		else
		{
			Debug.LogWarning(GetType().Name + ": SpawnHandler instance not found.");
		}
	}

	protected void OnDrawGizmosSelected()
	{
		Bounds result;
		if (TryGetEntityBounds(out result))
		{
			Gizmos.color = Color.yellow;
			Gizmos.matrix = base.transform.localToWorldMatrix;
			Gizmos.DrawCube(result.center, result.size);
		}
	}

	public void ObjectSpawned(SpawnPointInstance instance)
	{
		spawnInstance = instance;
	}

	public void ObjectRetired(SpawnPointInstance instance)
	{
		spawnInstance = null;
		nextSpawnTime = Time.time + Random.Range(respawnDelayMin, respawnDelayMax);
	}

	public void Fill()
	{
		TrySpawnEntity();
	}

	public void SpawnInitial()
	{
		TrySpawnEntity();
	}

	public void Clear()
	{
		if (IsSpawned)
		{
			BaseEntity baseEntity = GameObjectEx.ToBaseEntity(spawnInstance.gameObject);
			if (baseEntity != null)
			{
				baseEntity.Kill();
			}
		}
	}

	public void SpawnRepeating()
	{
		if (!IsSpawned && Time.time >= nextSpawnTime)
		{
			TrySpawnEntity();
		}
	}

	public bool HasSpaceToSpawn()
	{
		if (useCustomBoundsCheckMask)
		{
			return SpawnHandler.CheckBounds(entityPrefab.Get(), base.transform.position, base.transform.rotation, Vector3.one, customBoundsCheckMask);
		}
		return SingletonComponent<SpawnHandler>.Instance.CheckBounds(entityPrefab.Get(), base.transform.position, base.transform.rotation, Vector3.one);
	}

	private void TrySpawnEntity()
	{
		if (IsSpawned)
		{
			return;
		}
		if (!HasSpaceToSpawn())
		{
			nextSpawnTime = Time.time + Random.Range(respawnDelayMin, respawnDelayMax);
			return;
		}
		BaseEntity baseEntity = GameManager.server.CreateEntity(entityPrefab.resourcePath, base.transform.position, base.transform.rotation, false);
		if (baseEntity != null)
		{
			baseEntity.enableSaving = false;
			PoolableEx.AwakeFromInstantiate(baseEntity.gameObject);
			baseEntity.Spawn();
			SpawnPointInstance spawnPointInstance = baseEntity.gameObject.AddComponent<SpawnPointInstance>();
			spawnPointInstance.parentSpawnPointUser = this;
			spawnPointInstance.Notify();
		}
		else
		{
			Debug.LogError("IndividualSpawner failed to spawn entity.", base.gameObject);
		}
	}

	private bool TryGetEntityBounds(out Bounds result)
	{
		if (entityPrefab != null)
		{
			GameObject gameObject = entityPrefab.Get();
			if (gameObject != null)
			{
				BaseEntity component = gameObject.GetComponent<BaseEntity>();
				if (component != null)
				{
					result = component.bounds;
					return true;
				}
			}
		}
		result = default(Bounds);
		return false;
	}
}
