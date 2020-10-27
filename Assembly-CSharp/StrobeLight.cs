#define UNITY_ASSERTIONS
using ConVar;
using Network;
using Rust;
using System;
using UnityEngine;
using UnityEngine.Assertions;

public class StrobeLight : BaseCombatEntity
{
	public float frequency;

	public MeshRenderer lightMesh;

	public Light strobeLight;

	private float speedSlow = 10f;

	private float speedMed = 20f;

	private float speedFast = 40f;

	public float burnRate = 10f;

	public float lifeTimeSeconds = 21600f;

	public const Flags Flag_Slow = Flags.Reserved6;

	public const Flags Flag_Med = Flags.Reserved7;

	public const Flags Flag_Fast = Flags.Reserved8;

	private int currentSpeed = 1;

	public float GetFrequency()
	{
		if (HasFlag(Flags.Reserved6))
		{
			return speedSlow;
		}
		if (HasFlag(Flags.Reserved7))
		{
			return speedMed;
		}
		if (HasFlag(Flags.Reserved8))
		{
			return speedFast;
		}
		return speedSlow;
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void SetStrobe(RPCMessage msg)
	{
		bool flag = msg.read.Bit();
		ServerEnableStrobing(flag);
		if (flag)
		{
			UpdateSpeedFlags();
		}
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void SetStrobeSpeed(RPCMessage msg)
	{
		int num = currentSpeed = msg.read.Int32();
		UpdateSpeedFlags();
	}

	public void UpdateSpeedFlags()
	{
		SetFlag(Flags.Reserved6, currentSpeed == 1);
		SetFlag(Flags.Reserved7, currentSpeed == 2);
		SetFlag(Flags.Reserved8, currentSpeed == 3);
	}

	public void ServerEnableStrobing(bool wantsOn)
	{
		SetFlag(Flags.Reserved6, false);
		SetFlag(Flags.Reserved7, false);
		SetFlag(Flags.Reserved8, false);
		SetFlag(Flags.On, wantsOn);
		SendNetworkUpdateImmediate();
		UpdateSpeedFlags();
		if (wantsOn)
		{
			InvokeRandomized(SelfDamage, 0f, 10f, 0.1f);
		}
		else
		{
			CancelInvoke(SelfDamage);
		}
	}

	public void SelfDamage()
	{
		float num = burnRate / lifeTimeSeconds;
		Hurt(num * MaxHealth(), DamageType.Decay, this, false);
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("StrobeLight.OnRpcMessage"))
		{
			RPCMessage rPCMessage;
			if (rpc == 1433326740 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player + " - SetStrobe ");
				}
				using (TimeWarning.New("SetStrobe"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(1433326740u, "SetStrobe", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage strobe = rPCMessage;
							SetStrobe(strobe);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in SetStrobe");
					}
				}
				return true;
			}
			if (rpc == 1814332702 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player + " - SetStrobeSpeed ");
				}
				using (TimeWarning.New("SetStrobeSpeed"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(1814332702u, "SetStrobeSpeed", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage strobeSpeed = rPCMessage;
							SetStrobeSpeed(strobeSpeed);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in SetStrobeSpeed");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}
}
