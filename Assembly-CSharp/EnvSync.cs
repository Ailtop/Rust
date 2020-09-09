using Facepunch;
using ProtoBuf;
using System;
using UnityEngine;

public class EnvSync : PointEntity
{
	private const float syncInterval = 5f;

	private const float syncIntervalInv = 0.2f;

	public override void ServerInit()
	{
		base.ServerInit();
		InvokeRepeating(UpdateNetwork, 5f, 5f);
	}

	private void UpdateNetwork()
	{
		SendNetworkUpdate();
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.environment = Pool.Get<ProtoBuf.Environment>();
		if ((bool)TOD_Sky.Instance)
		{
			info.msg.environment.dateTime = TOD_Sky.Instance.Cycle.DateTime.ToBinary();
		}
		if ((bool)SingletonComponent<Climate>.Instance)
		{
			info.msg.environment.clouds = SingletonComponent<Climate>.Instance.Overrides.Clouds;
			info.msg.environment.fog = SingletonComponent<Climate>.Instance.Overrides.Fog;
			info.msg.environment.wind = SingletonComponent<Climate>.Instance.Overrides.Wind;
			info.msg.environment.rain = SingletonComponent<Climate>.Instance.Overrides.Rain;
		}
		info.msg.environment.engineTime = Time.realtimeSinceStartup;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.environment != null && (bool)TOD_Sky.Instance)
		{
			TOD_Sky.Instance.Cycle.DateTime = DateTime.FromBinary(info.msg.environment.dateTime);
		}
	}
}
