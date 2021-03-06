#define UNITY_ASSERTIONS
using System;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Audio;

public class MicrophoneStand : BaseMountable
{
	public enum SpeechMode
	{
		Normal,
		HighPitch,
		LowPitch
	}

	public VoiceProcessor VoiceProcessor;

	public AudioSource VoiceSource;

	private SpeechMode currentSpeechMode;

	public AudioMixerGroup NormalMix;

	public AudioMixerGroup HighPitchMix;

	public AudioMixerGroup LowPitchMix;

	public Translate.Phrase NormalPhrase = new Translate.Phrase("microphone_normal", "Normal");

	public Translate.Phrase NormalDescPhrase = new Translate.Phrase("microphone_normal_desc", "No voice effect");

	public Translate.Phrase HighPitchPhrase = new Translate.Phrase("microphone_high", "High Pitch");

	public Translate.Phrase HighPitchDescPhrase = new Translate.Phrase("microphone_high_desc", "High pitch voice");

	public Translate.Phrase LowPitchPhrase = new Translate.Phrase("microphone_low", "Low");

	public Translate.Phrase LowPitchDescPhrase = new Translate.Phrase("microphone_low_desc", "Low pitch voice");

	public GameObjectRef IOSubEntity;

	public Transform IOSubEntitySpawnPos;

	public bool IsStatic;

	public EntityRef<IOEntity> ioEntity;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("MicrophoneStand.OnRpcMessage"))
		{
			if (rpc == 1420522459 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - SetMode "));
				}
				using (TimeWarning.New("SetMode"))
				{
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage mode = rPCMessage;
							SetMode(mode);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in SetMode");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	[RPC_Server]
	public void SetMode(RPCMessage msg)
	{
		if (!(msg.player != _mounted))
		{
			SpeechMode speechMode = (SpeechMode)msg.read.Int32();
			if (speechMode != currentSpeechMode)
			{
				currentSpeechMode = speechMode;
				SendNetworkUpdate();
			}
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (info.msg.microphoneStand == null)
		{
			info.msg.microphoneStand = Facepunch.Pool.Get<ProtoBuf.MicrophoneStand>();
		}
		info.msg.microphoneStand.microphoneMode = (int)currentSpeechMode;
		info.msg.microphoneStand.IORef = ioEntity.uid;
	}

	public void SpawnChildEntity()
	{
		MicrophoneStandIOEntity microphoneStandIOEntity = GameManager.server.CreateEntity(IOSubEntity.resourcePath, IOSubEntitySpawnPos.localPosition, IOSubEntitySpawnPos.localRotation) as MicrophoneStandIOEntity;
		microphoneStandIOEntity.enableSaving = enableSaving;
		microphoneStandIOEntity.SetParent(this);
		microphoneStandIOEntity.Spawn();
		ioEntity.Set(microphoneStandIOEntity);
		SendNetworkUpdate();
	}

	public override void OnDeployed(BaseEntity parent, BasePlayer deployedBy)
	{
		base.OnDeployed(parent, deployedBy);
		SpawnChildEntity();
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.microphoneStand != null)
		{
			currentSpeechMode = (SpeechMode)info.msg.microphoneStand.microphoneMode;
			ioEntity.uid = info.msg.microphoneStand.IORef;
		}
	}
}
