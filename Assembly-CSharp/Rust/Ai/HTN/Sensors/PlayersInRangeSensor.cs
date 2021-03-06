using System;
using System.Collections.Generic;
using ConVar;
using Oxide.Core;

namespace Rust.Ai.HTN.Sensors
{
	[Serializable]
	public class PlayersInRangeSensor : INpcSensor
	{
		public const int MaxPlayers = 128;

		public static BasePlayer[] PlayerQueryResults = new BasePlayer[128];

		public static int PlayerQueryResultCount = 0;

		public float TickFrequency { get; set; }

		public float LastTickTime { get; set; }

		public void Tick(IHTNAgent npc, float deltaTime, float time)
		{
			if (ConVar.AI.ignoreplayers || npc == null || npc.transform == null || npc.IsDestroyed || npc.AiDefinition == null)
			{
				return;
			}
			PlayerQueryResultCount = BaseEntity.Query.Server.GetPlayersInSphere(npc.transform.position, npc.AiDefinition.Sensory.VisionRange, PlayerQueryResults, delegate(BasePlayer player)
			{
				if (player == null || !player.isServer || player.IsDestroyed || player.transform == null || player.IsDead() || player.IsWounded())
				{
					return false;
				}
				return (!player.IsSleeping() || !(player.secondsSleeping < NPCAutoTurret.sleeperhostiledelay)) ? true : false;
			});
			List<NpcPlayerInfo> playersInRange = npc.AiDomain.NpcContext.PlayersInRange;
			if (PlayerQueryResultCount > 0)
			{
				for (int i = 0; i < PlayerQueryResultCount; i++)
				{
					BasePlayer basePlayer = PlayerQueryResults[i];
					HTNPlayer hTNPlayer = npc as HTNPlayer;
					if ((hTNPlayer != null && basePlayer == hTNPlayer) || (basePlayer.transform.position - npc.transform.position).sqrMagnitude > npc.AiDefinition.Sensory.SqrVisionRange)
					{
						continue;
					}
					bool flag = false;
					for (int j = 0; j < playersInRange.Count; j++)
					{
						NpcPlayerInfo value = playersInRange[j];
						if (value.Player == basePlayer)
						{
							value.Time = time;
							playersInRange[j] = value;
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						playersInRange.Add(new NpcPlayerInfo
						{
							Player = basePlayer,
							Time = time
						});
					}
				}
			}
			for (int k = 0; k < playersInRange.Count; k++)
			{
				NpcPlayerInfo player2 = playersInRange[k];
				if (Interface.CallHook("IOnNpcTarget", npc, player2.Player, k) != null)
				{
					break;
				}
				if (time - player2.Time > npc.AiDefinition.Memory.ForgetInRangeTime && npc.AiDomain.NpcContext.BaseMemory.ShouldRemoveOnPlayerForgetTimeout(time, player2))
				{
					playersInRange.RemoveAt(k);
					k--;
				}
			}
		}
	}
}
