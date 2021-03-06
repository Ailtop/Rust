using Rust.Ai.HTN.Reasoning;

namespace Rust.Ai.HTN.Scientist.Reasoners
{
	public class AmmoReasoner : INpcReasoner
	{
		public float TickFrequency { get; set; }

		public float LastTickTime { get; set; }

		public void Tick(IHTNAgent npc, float deltaTime, float time)
		{
			ScientistContext scientistContext = npc.AiDomain.NpcContext as ScientistContext;
			if (scientistContext == null)
			{
				return;
			}
			HTNPlayer hTNPlayer = npc as HTNPlayer;
			if (hTNPlayer == null)
			{
				return;
			}
			AttackEntity attackEntity = hTNPlayer.GetHeldEntity() as AttackEntity;
			if ((bool)attackEntity)
			{
				BaseProjectile baseProjectile = attackEntity as BaseProjectile;
				if (baseProjectile != null)
				{
					float num = (float)baseProjectile.primaryMagazine.contents / (float)baseProjectile.primaryMagazine.capacity;
					if (num > 0.9f)
					{
						scientistContext.SetFact(Facts.AmmoState, AmmoState.FullClip);
					}
					else if (num > 0.6f)
					{
						scientistContext.SetFact(Facts.AmmoState, AmmoState.HighClip);
					}
					else if (num > 0.17f)
					{
						scientistContext.SetFact(Facts.AmmoState, AmmoState.MediumClip);
					}
					else if (num > 0f)
					{
						scientistContext.SetFact(Facts.AmmoState, AmmoState.LowAmmo);
					}
					else
					{
						scientistContext.SetFact(Facts.AmmoState, AmmoState.EmptyClip);
					}
					return;
				}
			}
			scientistContext.SetFact(Facts.AmmoState, AmmoState.DontRequireAmmo);
		}
	}
}
