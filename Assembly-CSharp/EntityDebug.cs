using System.Diagnostics;

public class EntityDebug : EntityComponent<BaseEntity>
{
	internal Stopwatch stopwatch = Stopwatch.StartNew();

	private void Update()
	{
		if (!BaseEntityEx.IsValid(base.baseEntity) || !base.baseEntity.IsDebugging())
		{
			base.enabled = false;
		}
		else if (!(stopwatch.Elapsed.TotalSeconds < 0.5))
		{
			bool isClient = base.baseEntity.isClient;
			if (base.baseEntity.isServer)
			{
				base.baseEntity.DebugServer(1, (float)stopwatch.Elapsed.TotalSeconds);
			}
			stopwatch.Reset();
			stopwatch.Start();
		}
	}
}
