using System.Collections;
using Oxide.Core;
using UnityEngine;

namespace Rust.Ai.HTN.Scientist
{
	[CreateAssetMenu(menuName = "Rust/AI/Scientist Definition")]
	public class ScientistDefinition : BaseNpcDefinition
	{
		[Header("Aim")]
		public AnimationCurve MissFunction = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

		[Header("Equipment")]
		public PlayerInventoryProperties[] loadouts;

		public LootContainer.LootSpawnSlot[] Loot;

		[Header("Audio")]
		public Vector2 RadioEffectRepeatRange = new Vector2(10f, 15f);

		public GameObjectRef RadioEffect;

		public GameObjectRef DeathEffect;

		private bool _isRadioEffectRunning;

		public override void StartVoices(HTNPlayer target)
		{
			if (!_isRadioEffectRunning)
			{
				_isRadioEffectRunning = true;
				target.StartCoroutine(RadioChatter(target));
			}
		}

		public override void StopVoices(HTNPlayer target)
		{
			if (_isRadioEffectRunning)
			{
				_isRadioEffectRunning = false;
			}
		}

		private IEnumerator RadioChatter(HTNPlayer target)
		{
			while (_isRadioEffectRunning && target != null && target.transform != null && !target.IsDestroyed && !target.IsDead())
			{
				if (RadioEffect.isValid)
				{
					Effect.server.Run(RadioEffect.resourcePath, target, StringPool.Get("head"), Vector3.zero, Vector3.zero);
				}
				float seconds = UnityEngine.Random.Range(RadioEffectRepeatRange.x, RadioEffectRepeatRange.y + 1f);
				yield return CoroutineEx.waitForSeconds(seconds);
			}
		}

		public override void Loadout(HTNPlayer target)
		{
			if (target == null || target.IsDestroyed || target.IsDead() || target.IsWounded() || target.inventory == null || target.inventory.containerBelt == null || target.inventory.containerMain == null || target.inventory.containerWear == null)
			{
				return;
			}
			if (loadouts != null && loadouts.Length != 0)
			{
				PlayerInventoryProperties playerInventoryProperties = loadouts[UnityEngine.Random.Range(0, loadouts.Length)];
				if (playerInventoryProperties != null)
				{
					playerInventoryProperties.GiveToPlayer(target);
					target.StartCoroutine(EquipWeapon(target));
				}
			}
			else
			{
				Debug.LogWarning("Loadout for NPC " + base.name + " was empty.");
			}
		}

		public IEnumerator EquipWeapon(HTNPlayer target)
		{
			yield return CoroutineEx.waitForSeconds(0.25f);
			if (target == null || target.IsDestroyed || target.IsDead() || target.IsWounded() || target.inventory == null || target.inventory.containerBelt == null)
			{
				yield break;
			}
			Item slot = target.inventory.containerBelt.GetSlot(0);
			if (slot == null)
			{
				yield break;
			}
			target.UpdateActiveItem(slot.uid);
			yield return CoroutineEx.waitForSeconds(0.25f);
			ScientistDomain scientistDomain = target.AiDomain as ScientistDomain;
			if (!scientistDomain)
			{
				yield break;
			}
			if (slot.info.category == ItemCategory.Weapon)
			{
				BaseEntity heldEntity = slot.GetHeldEntity();
				if (heldEntity is BaseProjectile)
				{
					scientistDomain.ScientistContext.SetFact(Facts.HeldItemType, ItemType.ProjectileWeapon);
					scientistDomain.ReloadFirearm();
				}
				else if (heldEntity is BaseMelee)
				{
					scientistDomain.ScientistContext.SetFact(Facts.HeldItemType, ItemType.MeleeWeapon);
				}
				else if (heldEntity is ThrownWeapon)
				{
					scientistDomain.ScientistContext.SetFact(Facts.HeldItemType, ItemType.ThrowableWeapon);
				}
			}
			else if (slot.info.category == ItemCategory.Medical)
			{
				scientistDomain.ScientistContext.SetFact(Facts.HeldItemType, ItemType.HealingItem);
			}
			else if (slot.info.category == ItemCategory.Tool)
			{
				scientistDomain.ScientistContext.SetFact(Facts.HeldItemType, ItemType.LightSourceItem);
			}
		}

		public override BaseCorpse OnCreateCorpse(HTNPlayer target)
		{
			if (DeathEffect.isValid)
			{
				Effect.server.Run(DeathEffect.resourcePath, target, 0u, Vector3.zero, Vector3.zero);
			}
			using (TimeWarning.New("Create corpse"))
			{
				NPCPlayerCorpse nPCPlayerCorpse = target.DropCorpse("assets/prefabs/npc/scientist/scientist_corpse.prefab") as NPCPlayerCorpse;
				if ((bool)nPCPlayerCorpse)
				{
					if (target.AiDomain != null && target.AiDomain.NavAgent != null && target.AiDomain.NavAgent.isOnNavMesh)
					{
						nPCPlayerCorpse.transform.position = nPCPlayerCorpse.transform.position + Vector3.down * target.AiDomain.NavAgent.baseOffset;
					}
					nPCPlayerCorpse.SetLootableIn(2f);
					nPCPlayerCorpse.SetFlag(BaseEntity.Flags.Reserved5, target.HasPlayerFlag(BasePlayer.PlayerFlags.DisplaySash));
					nPCPlayerCorpse.SetFlag(BaseEntity.Flags.Reserved2, true);
					nPCPlayerCorpse.TakeFrom(target.inventory.containerMain, target.inventory.containerWear, target.inventory.containerBelt);
					nPCPlayerCorpse.playerName = target.displayName;
					nPCPlayerCorpse.playerSteamID = target.userID;
					nPCPlayerCorpse.Spawn();
					nPCPlayerCorpse.TakeChildren(target);
					ItemContainer[] containers = nPCPlayerCorpse.containers;
					for (int i = 0; i < containers.Length; i++)
					{
						containers[i].Clear();
					}
					if (Loot.Length != 0)
					{
						object obj = Interface.CallHook("OnCorpsePopulate", target, nPCPlayerCorpse);
						if (obj is BaseCorpse)
						{
							return (BaseCorpse)obj;
						}
						LootContainer.LootSpawnSlot[] loot = Loot;
						for (int i = 0; i < loot.Length; i++)
						{
							LootContainer.LootSpawnSlot lootSpawnSlot = loot[i];
							for (int j = 0; j < lootSpawnSlot.numberToSpawn; j++)
							{
								if (UnityEngine.Random.Range(0f, 1f) <= lootSpawnSlot.probability)
								{
									lootSpawnSlot.definition.SpawnIntoContainer(nPCPlayerCorpse.containers[0]);
								}
							}
						}
					}
				}
				return nPCPlayerCorpse;
			}
		}
	}
}
