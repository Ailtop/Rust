using Rust;
using UnityEngine;

public class SpawnPointInstance : MonoBehaviour
{
	public ISpawnPointUser parentSpawnPointUser;

	public BaseSpawnPoint parentSpawnPoint;

	public void Notify()
	{
		if (!ObjectEx.IsUnityNull(parentSpawnPointUser))
		{
			parentSpawnPointUser.ObjectSpawned(this);
		}
		if ((bool)parentSpawnPoint)
		{
			parentSpawnPoint.ObjectSpawned(this);
		}
	}

	protected void OnDestroy()
	{
		if (!Rust.Application.isQuitting)
		{
			if (!ObjectEx.IsUnityNull(parentSpawnPointUser))
			{
				parentSpawnPointUser.ObjectRetired(this);
			}
			if ((bool)parentSpawnPoint)
			{
				parentSpawnPoint.ObjectRetired(this);
			}
		}
	}
}
