using System;
using System.Collections;
using System.Collections.Generic;
using Apex.AI;
using Apex.AI.Components;
using Apex.Ai.HTN;
using Apex.Serialization;
using ConVar;
using Rust.Ai.HTN.Reasoning;
using Rust.Ai.HTN.ScientistJunkpile.Reasoners;
using Rust.Ai.HTN.ScientistJunkpile.Sensors;
using Rust.Ai.HTN.Sensors;
using UnityEngine;
using UnityEngine.AI;

namespace Rust.Ai.HTN.ScientistJunkpile
{
	public class ScientistJunkpileDomain : HTNDomain
	{
		public class JunkpileWorldStateEffect : EffectBase<ScientistJunkpileContext>
		{
			[ApexSerialization]
			public Facts Fact;

			[ApexSerialization]
			public byte Value;

			public override void Apply(ScientistJunkpileContext context, bool fromPlanner, bool temporary)
			{
				if (fromPlanner)
				{
					context.PushFactChangeDuringPlanning(Fact, Value, temporary);
				}
				else
				{
					context.SetFact(Fact, Value);
				}
			}

			public override void Reverse(ScientistJunkpileContext context, bool fromPlanner)
			{
				if (fromPlanner)
				{
					context.PopFactChangeDuringPlanning(Fact);
				}
				else
				{
					context.WorldState[(uint)Fact] = context.PreviousWorldState[(uint)Fact];
				}
			}
		}

		public class JunkpileWorldStateBoolEffect : EffectBase<ScientistJunkpileContext>
		{
			[ApexSerialization]
			public Facts Fact;

			[ApexSerialization]
			public bool Value;

			public override void Apply(ScientistJunkpileContext context, bool fromPlanner, bool temporary)
			{
				if (fromPlanner)
				{
					context.PushFactChangeDuringPlanning(Fact, Value, temporary);
				}
				else
				{
					context.SetFact(Fact, Value);
				}
			}

			public override void Reverse(ScientistJunkpileContext context, bool fromPlanner)
			{
				if (fromPlanner)
				{
					context.PopFactChangeDuringPlanning(Fact);
				}
				else
				{
					context.WorldState[(uint)Fact] = context.PreviousWorldState[(uint)Fact];
				}
			}
		}

		public class JunkpileWorldStateIncrementEffect : EffectBase<ScientistJunkpileContext>
		{
			[ApexSerialization]
			public Facts Fact;

			[ApexSerialization]
			public byte Value;

			public override void Apply(ScientistJunkpileContext context, bool fromPlanner, bool temporary)
			{
				if (fromPlanner)
				{
					byte b = context.PeekFactChangeDuringPlanning(Fact);
					context.PushFactChangeDuringPlanning(Fact, b + Value, temporary);
				}
				else
				{
					context.SetFact(Fact, context.GetFact(Fact) + Value);
				}
			}

			public override void Reverse(ScientistJunkpileContext context, bool fromPlanner)
			{
				if (fromPlanner)
				{
					context.PopFactChangeDuringPlanning(Fact);
				}
				else
				{
					context.WorldState[(uint)Fact] = context.PreviousWorldState[(uint)Fact];
				}
			}
		}

		public class JunkpileHealEffect : EffectBase<ScientistJunkpileContext>
		{
			[ApexSerialization]
			public HealthState Health;

			public override void Apply(ScientistJunkpileContext context, bool fromPlanner, bool temporary)
			{
				if (fromPlanner)
				{
					context.PushFactChangeDuringPlanning(Facts.HealthState, Health, temporary);
				}
				else
				{
					context.SetFact(Facts.HealthState, Health);
				}
			}

			public override void Reverse(ScientistJunkpileContext context, bool fromPlanner)
			{
				if (fromPlanner)
				{
					context.PopFactChangeDuringPlanning(Facts.HealthState);
				}
				else
				{
					context.SetFact(Facts.HealthState, context.GetPreviousFact(Facts.HealthState));
				}
			}
		}

		public class JunkpileIsNavigatingEffect : EffectBase<ScientistJunkpileContext>
		{
			public override void Apply(ScientistJunkpileContext context, bool fromPlanner, bool temporary)
			{
				if (fromPlanner)
				{
					context.PushFactChangeDuringPlanning(Facts.IsNavigating, 1, temporary);
					return;
				}
				context.PreviousWorldState[5] = context.WorldState[5];
				context.WorldState[5] = 1;
			}

			public override void Reverse(ScientistJunkpileContext context, bool fromPlanner)
			{
				if (fromPlanner)
				{
					context.PopFactChangeDuringPlanning(Facts.IsNavigating);
				}
				else
				{
					context.WorldState[5] = context.PreviousWorldState[5];
				}
			}
		}

		public class JunkpileIsNotNavigatingEffect : EffectBase<ScientistJunkpileContext>
		{
			public override void Apply(ScientistJunkpileContext context, bool fromPlanner, bool temporary)
			{
				ApplyStatic(context, fromPlanner, temporary);
			}

			public override void Reverse(ScientistJunkpileContext context, bool fromPlanner)
			{
				if (fromPlanner)
				{
					context.PopFactChangeDuringPlanning(Facts.IsNavigating);
				}
				else
				{
					context.WorldState[5] = context.PreviousWorldState[5];
				}
			}

			public static void ApplyStatic(ScientistJunkpileContext context, bool fromPlanner, bool temporary)
			{
				if (fromPlanner)
				{
					context.PushFactChangeDuringPlanning(Facts.IsNavigating, (byte)0, temporary);
					return;
				}
				context.PreviousWorldState[5] = context.WorldState[5];
				context.WorldState[5] = 0;
			}
		}

		public class JunkpileHoldItemOfTypeEffect : EffectBase<ScientistJunkpileContext>
		{
			[ApexSerialization]
			public ItemType Value;

			public override void Apply(ScientistJunkpileContext context, bool fromPlanner, bool temporary)
			{
				if (fromPlanner)
				{
					context.PushFactChangeDuringPlanning(Facts.HeldItemType, Value, temporary);
				}
				else
				{
					context.SetFact(Facts.HeldItemType, Value);
				}
			}

			public override void Reverse(ScientistJunkpileContext context, bool fromPlanner)
			{
				if (fromPlanner)
				{
					context.PopFactChangeDuringPlanning(Facts.HeldItemType);
				}
				else
				{
					context.SetFact(Facts.HeldItemType, context.GetPreviousFact(Facts.HeldItemType));
				}
			}
		}

		public class JunkpileTimeBlockNavigationEffect : EffectBase<ScientistJunkpileContext>
		{
			[ApexSerialization]
			[FriendlyName("Time (Seconds)")]
			public float Time;

			public override void Apply(ScientistJunkpileContext context, bool fromPlanner, bool temporary)
			{
			}

			public override void Reverse(ScientistJunkpileContext context, bool fromPlanner)
			{
			}
		}

		public class JunkpileBlockNavigationEffect : EffectBase<ScientistJunkpileContext>
		{
			public override void Apply(ScientistJunkpileContext context, bool fromPlanner, bool temporary)
			{
			}

			public override void Reverse(ScientistJunkpileContext context, bool fromPlanner)
			{
			}
		}

		public class JunkpileUnblockNavigationEffect : EffectBase<ScientistJunkpileContext>
		{
			public override void Apply(ScientistJunkpileContext context, bool fromPlanner, bool temporary)
			{
			}

			public override void Reverse(ScientistJunkpileContext context, bool fromPlanner)
			{
			}
		}

		public class JunkpileBlockReloadingEffect : EffectBase<ScientistJunkpileContext>
		{
			public override void Apply(ScientistJunkpileContext context, bool fromPlanner, bool temporary)
			{
			}

			public override void Reverse(ScientistJunkpileContext context, bool fromPlanner)
			{
			}
		}

		public class JunkpileUnblockReloadingEffect : EffectBase<ScientistJunkpileContext>
		{
			public override void Apply(ScientistJunkpileContext context, bool fromPlanner, bool temporary)
			{
			}

			public override void Reverse(ScientistJunkpileContext context, bool fromPlanner)
			{
			}
		}

		public class JunkpileBlockShootingEffect : EffectBase<ScientistJunkpileContext>
		{
			public override void Apply(ScientistJunkpileContext context, bool fromPlanner, bool temporary)
			{
			}

			public override void Reverse(ScientistJunkpileContext context, bool fromPlanner)
			{
			}
		}

		public class JunkpileUnblockShootingEffect : EffectBase<ScientistJunkpileContext>
		{
			public override void Apply(ScientistJunkpileContext context, bool fromPlanner, bool temporary)
			{
			}

			public override void Reverse(ScientistJunkpileContext context, bool fromPlanner)
			{
			}
		}

		public class JunkpileChangeFirearmOrder : EffectBase<ScientistJunkpileContext>
		{
			[ApexSerialization]
			public FirearmOrders Order;

			public override void Apply(ScientistJunkpileContext context, bool fromPlanner, bool temporary)
			{
				if (fromPlanner)
				{
					context.PushFactChangeDuringPlanning(Facts.FirearmOrder, Order, temporary);
				}
				else
				{
					context.SetFact(Facts.FirearmOrder, Order);
				}
			}

			public override void Reverse(ScientistJunkpileContext context, bool fromPlanner)
			{
				if (fromPlanner)
				{
					context.PopFactChangeDuringPlanning(Facts.FirearmOrder);
				}
				else
				{
					context.SetFact(Facts.FirearmOrder, context.GetPreviousFact(Facts.FirearmOrder));
				}
			}
		}

		public class JunkpileFutureCoverState : EffectBase<ScientistJunkpileContext>
		{
			[ApexSerialization]
			public CoverTactic Tactic;

			public override void Apply(ScientistJunkpileContext context, bool fromPlanner, bool temporary)
			{
				CoverPoint cover = JunkpileNavigateToCover.GetCover(Tactic, context);
				if (fromPlanner)
				{
					context.PushFactChangeDuringPlanning(Facts.CoverState, (cover != null) ? ((cover.NormalCoverType == CoverPoint.CoverType.Partial) ? CoverState.Partial : CoverState.Full) : CoverState.None, temporary);
				}
				else
				{
					context.SetFact(Facts.CoverState, (cover != null) ? ((cover.NormalCoverType == CoverPoint.CoverType.Partial) ? CoverState.Partial : CoverState.Full) : CoverState.None);
				}
			}

			public override void Reverse(ScientistJunkpileContext context, bool fromPlanner)
			{
				if (fromPlanner)
				{
					context.PopFactChangeDuringPlanning(Facts.CoverState);
				}
				else
				{
					context.SetFact(Facts.CoverState, context.GetPreviousFact(Facts.CoverState));
				}
			}
		}

		public abstract class BaseNavigateTo : OperatorBase<ScientistJunkpileContext>
		{
			[ApexSerialization]
			public bool RunUntilArrival = true;

			protected abstract Vector3 _GetDestination(ScientistJunkpileContext context);

			protected virtual void OnPreStart(ScientistJunkpileContext context)
			{
			}

			protected virtual void OnStart(ScientistJunkpileContext context)
			{
			}

			protected virtual void OnPathFailed(ScientistJunkpileContext context)
			{
			}

			protected virtual void OnPathComplete(ScientistJunkpileContext context)
			{
			}

			public override void Execute(ScientistJunkpileContext context)
			{
				OnPreStart(context);
				context.ReserveCoverPoint(null);
				context.Domain.SetDestination(_GetDestination(context));
				if (!RunUntilArrival)
				{
					context.OnWorldStateChangedEvent = (ScientistJunkpileContext.WorldStateChangedEvent)Delegate.Combine(context.OnWorldStateChangedEvent, new ScientistJunkpileContext.WorldStateChangedEvent(TrackWorldState));
				}
				OnStart(context);
			}

			private void TrackWorldState(ScientistJunkpileContext context, Facts fact, byte oldValue, byte newValue)
			{
				if (fact == Facts.PathStatus)
				{
					switch (newValue)
					{
					case 2:
						context.OnWorldStateChangedEvent = (ScientistJunkpileContext.WorldStateChangedEvent)Delegate.Remove(context.OnWorldStateChangedEvent, new ScientistJunkpileContext.WorldStateChangedEvent(TrackWorldState));
						JunkpileIsNotNavigatingEffect.ApplyStatic(context, false, false);
						ApplyExpectedEffects(context, context.CurrentTask);
						context.Domain.StopNavigating();
						OnPathComplete(context);
						break;
					case 3:
						context.OnWorldStateChangedEvent = (ScientistJunkpileContext.WorldStateChangedEvent)Delegate.Remove(context.OnWorldStateChangedEvent, new ScientistJunkpileContext.WorldStateChangedEvent(TrackWorldState));
						JunkpileIsNotNavigatingEffect.ApplyStatic(context, false, false);
						context.Domain.StopNavigating();
						OnPathFailed(context);
						break;
					}
				}
			}

			public override OperatorStateType Tick(ScientistJunkpileContext context, PrimitiveTaskSelector task)
			{
				switch (context.GetFact(Facts.PathStatus))
				{
				default:
					context.Domain.StopNavigating();
					OnPathFailed(context);
					return OperatorStateType.Aborted;
				case 0:
				case 2:
					JunkpileIsNotNavigatingEffect.ApplyStatic(context, false, false);
					ApplyExpectedEffects(context, task);
					context.Domain.StopNavigating();
					OnPathComplete(context);
					return OperatorStateType.Complete;
				case 1:
					if (RunUntilArrival)
					{
						return OperatorStateType.Running;
					}
					return OperatorStateType.Complete;
				}
			}

			public override void Abort(ScientistJunkpileContext context, PrimitiveTaskSelector task)
			{
				JunkpileIsNotNavigatingEffect.ApplyStatic(context, false, false);
				context.Domain.StopNavigating();
			}
		}

		public class JunkpileNavigateToCover : BaseNavigateTo
		{
			[ApexSerialization]
			private CoverTactic _preferredTactic;

			public static CoverPoint GetCover(CoverTactic tactic, ScientistJunkpileContext context)
			{
				switch (tactic)
				{
				case CoverTactic.Retreat:
					if (context.BestRetreatCover != null && context.BestRetreatCover.IsValidFor(context.Body))
					{
						return context.BestRetreatCover;
					}
					if (context.BestFlankCover != null && context.BestFlankCover.IsValidFor(context.Body))
					{
						return context.BestFlankCover;
					}
					if (context.BestAdvanceCover != null && context.BestAdvanceCover.IsValidFor(context.Body))
					{
						return context.BestAdvanceCover;
					}
					break;
				case CoverTactic.Flank:
					if (context.BestFlankCover != null && context.BestFlankCover.IsValidFor(context.Body))
					{
						return context.BestFlankCover;
					}
					if (context.BestRetreatCover != null && context.BestRetreatCover.IsValidFor(context.Body))
					{
						return context.BestRetreatCover;
					}
					if (context.BestAdvanceCover != null && context.BestAdvanceCover.IsValidFor(context.Body))
					{
						return context.BestAdvanceCover;
					}
					break;
				case CoverTactic.Advance:
					if (context.BestAdvanceCover != null && context.BestAdvanceCover.IsValidFor(context.Body))
					{
						return context.BestAdvanceCover;
					}
					if (context.BestFlankCover != null && context.BestFlankCover.IsValidFor(context.Body))
					{
						return context.BestFlankCover;
					}
					if (context.BestRetreatCover != null && context.BestRetreatCover.IsValidFor(context.Body))
					{
						return context.BestRetreatCover;
					}
					break;
				case CoverTactic.Closest:
					if (context.ClosestCover != null && context.ClosestCover.IsValidFor(context.Body))
					{
						return context.ClosestCover;
					}
					break;
				}
				return null;
			}

			private static Vector3 _GetCoverPosition(CoverTactic tactic, ScientistJunkpileContext context)
			{
				switch (tactic)
				{
				case CoverTactic.Retreat:
					if (context.BestRetreatCover != null && context.BestRetreatCover.IsValidFor(context.Body))
					{
						context.SetFact(Facts.CoverTactic, CoverTactic.Retreat);
						context.ReserveCoverPoint(context.BestRetreatCover);
						return context.BestRetreatCover.Position;
					}
					if (context.BestFlankCover != null && context.BestFlankCover.IsValidFor(context.Body))
					{
						context.SetFact(Facts.CoverTactic, CoverTactic.Flank);
						context.ReserveCoverPoint(context.BestFlankCover);
						return context.BestFlankCover.Position;
					}
					if (context.BestAdvanceCover != null && context.BestAdvanceCover.IsValidFor(context.Body))
					{
						context.SetFact(Facts.CoverTactic, CoverTactic.Advance);
						context.ReserveCoverPoint(context.BestAdvanceCover);
						return context.BestAdvanceCover.Position;
					}
					break;
				case CoverTactic.Flank:
					if (context.BestFlankCover != null && context.BestFlankCover.IsValidFor(context.Body))
					{
						context.SetFact(Facts.CoverTactic, CoverTactic.Flank);
						context.ReserveCoverPoint(context.BestFlankCover);
						return context.BestFlankCover.Position;
					}
					if (context.BestRetreatCover != null && context.BestRetreatCover.IsValidFor(context.Body))
					{
						context.SetFact(Facts.CoverTactic, CoverTactic.Retreat);
						context.ReserveCoverPoint(context.BestRetreatCover);
						return context.BestRetreatCover.Position;
					}
					if (context.BestAdvanceCover != null && context.BestAdvanceCover.IsValidFor(context.Body))
					{
						context.SetFact(Facts.CoverTactic, CoverTactic.Advance);
						context.ReserveCoverPoint(context.BestAdvanceCover);
						return context.BestAdvanceCover.Position;
					}
					break;
				case CoverTactic.Advance:
					if (context.BestAdvanceCover != null && context.BestAdvanceCover.IsValidFor(context.Body))
					{
						context.SetFact(Facts.CoverTactic, CoverTactic.Advance);
						context.ReserveCoverPoint(context.BestAdvanceCover);
						return context.BestAdvanceCover.Position;
					}
					if (context.BestFlankCover != null && context.BestFlankCover.IsValidFor(context.Body))
					{
						context.SetFact(Facts.CoverTactic, CoverTactic.Flank);
						context.ReserveCoverPoint(context.BestFlankCover);
						return context.BestFlankCover.Position;
					}
					if (context.BestRetreatCover != null && context.BestRetreatCover.IsValidFor(context.Body))
					{
						context.SetFact(Facts.CoverTactic, CoverTactic.Retreat);
						context.ReserveCoverPoint(context.BestRetreatCover);
						return context.BestRetreatCover.Position;
					}
					break;
				case CoverTactic.Closest:
					if (context.ClosestCover != null && context.ClosestCover.IsValidFor(context.Body))
					{
						context.SetFact(Facts.CoverTactic, CoverTactic.Closest);
						context.ReserveCoverPoint(context.ClosestCover);
						return context.ClosestCover.Position;
					}
					break;
				}
				return context.Body.transform.position;
			}

			public static Vector3 GetCoverPosition(CoverTactic tactic, ScientistJunkpileContext context)
			{
				if (UnityEngine.Time.time - context.Memory.CachedCoverDestinationTime < 0.01f)
				{
					return context.Memory.CachedCoverDestination;
				}
				Vector3 vector = _GetCoverPosition(tactic, context);
				Vector3 vector2 = vector;
				if ((vector2 - context.Memory.CachedCoverDestination).sqrMagnitude < 2f)
				{
					return context.Memory.CachedCoverDestination;
				}
				for (int i = 0; i < 10; i++)
				{
					bool flag = false;
					NavMeshHit hit;
					if (NavMesh.FindClosestEdge(vector2, out hit, context.Domain.NavAgent.areaMask))
					{
						Vector3 position = hit.position;
						if (context.Memory.IsValid(position))
						{
							context.Memory.CachedCoverDestination = position;
							context.Memory.CachedCoverDestinationTime = UnityEngine.Time.time;
							return position;
						}
						flag = true;
					}
					if (NavMesh.SamplePosition(vector2, out hit, 2f * context.Domain.NavAgent.height, context.Domain.NavAgent.areaMask))
					{
						Vector3 vector3 = context.Domain.ToAllowedMovementDestination(hit.position);
						if (context.Memory.IsValid(vector3))
						{
							context.Memory.CachedCoverDestination = vector3;
							context.Memory.CachedCoverDestinationTime = UnityEngine.Time.time;
							return vector3;
						}
						flag = true;
					}
					if (!flag)
					{
						context.Memory.AddFailedDestination(vector2);
					}
					Vector2 vector4 = UnityEngine.Random.insideUnitCircle * 5f;
					vector2 = vector + new Vector3(vector4.x, 0f, vector4.y);
				}
				return context.BodyPosition;
			}

			protected override Vector3 _GetDestination(ScientistJunkpileContext context)
			{
				return GetCoverPosition(_preferredTactic, context);
			}

			protected override void OnPathFailed(ScientistJunkpileContext context)
			{
				context.SetFact(Facts.CoverTactic, CoverTactic.None);
				context.Domain.NavAgent.stoppingDistance = 1f;
			}

			protected override void OnPathComplete(ScientistJunkpileContext context)
			{
				context.SetFact(Facts.CoverTactic, CoverTactic.None);
				context.Domain.NavAgent.stoppingDistance = 1f;
			}

			protected override void OnPreStart(ScientistJunkpileContext context)
			{
				context.Domain.NavAgent.stoppingDistance = 1f;
			}
		}

		public class JunkpileNavigateToWaypoint : BaseNavigateTo
		{
			public static Vector3 GetNextWaypointPosition(ScientistJunkpileContext context)
			{
				return context.Body.transform.position + Vector3.forward * 10f;
			}

			protected override Vector3 _GetDestination(ScientistJunkpileContext context)
			{
				return GetNextWaypointPosition(context);
			}
		}

		public class JunkpileNavigateToPreferredFightingRange : BaseNavigateTo
		{
			public static Vector3 GetPreferredFightingPosition(ScientistJunkpileContext context, bool snapToAllowedRange = true)
			{
				if (UnityEngine.Time.time - context.Memory.CachedPreferredDistanceDestinationTime < 0.01f)
				{
					return context.Memory.CachedPreferredDistanceDestination;
				}
				NpcPlayerInfo primaryEnemyPlayerTarget = context.GetPrimaryEnemyPlayerTarget();
				if (primaryEnemyPlayerTarget.Player != null)
				{
					Vector3 bodyPosition = context.BodyPosition;
					if (context.GetFact(Facts.Frustration) <= ConVar.AI.npc_htn_player_frustration_threshold)
					{
						bodyPosition = JunkpileNavigateToCover.GetCoverPosition(CoverTactic.Closest, context);
					}
					else
					{
						AttackEntity firearm = context.Domain.GetFirearm();
						float num = context.Body.AiDefinition.Engagement.CenterOfMediumRangeFirearm(firearm);
						float num2 = num * num;
						Vector3 vector;
						float magnitude;
						if (primaryEnemyPlayerTarget.SqrDistance < num2)
						{
							vector = context.Body.transform.position - primaryEnemyPlayerTarget.Player.transform.position;
							magnitude = vector.magnitude;
							vector.Normalize();
						}
						else
						{
							vector = primaryEnemyPlayerTarget.Player.transform.position - context.Body.transform.position;
							magnitude = vector.magnitude;
							vector.Normalize();
						}
						float num3 = magnitude - num;
						bodyPosition = context.Body.transform.position + vector * num3;
					}
					Vector3 vector2 = bodyPosition;
					for (int i = 0; i < 10; i++)
					{
						NavMeshHit hit;
						if (NavMesh.SamplePosition(vector2 + Vector3.up * 0.1f, out hit, 2f * context.Domain.NavAgent.height, -1))
						{
							Vector3 position = hit.position;
							if (snapToAllowedRange)
							{
								context.Domain.ToAllowedMovementDestination(position);
							}
							if (context.Memory.IsValid(position))
							{
								context.Memory.CachedPreferredDistanceDestination = position;
								context.Memory.CachedPreferredDistanceDestinationTime = UnityEngine.Time.time;
								return position;
							}
						}
						else
						{
							context.Memory.AddFailedDestination(vector2);
						}
						Vector2 vector3 = UnityEngine.Random.insideUnitCircle * 5f;
						vector2 = bodyPosition + new Vector3(vector3.x, 0f, vector3.y);
					}
				}
				return context.Body.transform.position;
			}

			protected override Vector3 _GetDestination(ScientistJunkpileContext context)
			{
				return GetPreferredFightingPosition(context, false);
			}
		}

		public class JunkpileNavigateToLastKnownLocationOfPrimaryEnemyPlayer : BaseNavigateTo
		{
			[ApexSerialization]
			private bool DisableIsSearchingOnComplete = true;

			public static Vector3 GetDestination(ScientistJunkpileContext context)
			{
				BaseNpcMemory.EnemyPlayerInfo primaryKnownEnemyPlayer = context.Memory.PrimaryKnownEnemyPlayer;
				NavMeshHit hit;
				if (primaryKnownEnemyPlayer.PlayerInfo.Player != null && !context.HasVisitedLastKnownEnemyPlayerLocation && NavMesh.FindClosestEdge(primaryKnownEnemyPlayer.LastKnownPosition, out hit, context.Domain.NavAgent.areaMask))
				{
					return hit.position;
				}
				return context.Body.transform.position;
			}

			protected override Vector3 _GetDestination(ScientistJunkpileContext context)
			{
				return GetDestination(context);
			}

			protected override void OnPreStart(ScientistJunkpileContext context)
			{
				context.Domain.NavAgent.stoppingDistance = 0.5f;
			}

			protected override void OnStart(ScientistJunkpileContext context)
			{
				context.SetFact(Facts.IsSearching, true);
			}

			protected override void OnPathFailed(ScientistJunkpileContext context)
			{
				context.SetFact(Facts.IsSearching, false);
				context.Domain.NavAgent.stoppingDistance = 1f;
				context.HasVisitedLastKnownEnemyPlayerLocation = false;
			}

			protected override void OnPathComplete(ScientistJunkpileContext context)
			{
				if (DisableIsSearchingOnComplete)
				{
					context.SetFact(Facts.IsSearching, false);
				}
				context.Domain.NavAgent.stoppingDistance = 1f;
				context.HasVisitedLastKnownEnemyPlayerLocation = true;
			}
		}

		public class JunkpileNavigateInDirectionOfLastKnownHeadingOfPrimaryEnemyPlayer : BaseNavigateTo
		{
			[ApexSerialization]
			private bool DisableIsSearchingOnComplete = true;

			public static Vector3 GetDestination(ScientistJunkpileContext context)
			{
				BaseNpcMemory.EnemyPlayerInfo primaryKnownEnemyPlayer = context.Memory.PrimaryKnownEnemyPlayer;
				NavMeshHit hit;
				if (primaryKnownEnemyPlayer.PlayerInfo.Player != null && NavMesh.FindClosestEdge(primaryKnownEnemyPlayer.LastKnownPosition + primaryKnownEnemyPlayer.LastKnownHeading * 2f, out hit, context.Domain.NavAgent.areaMask))
				{
					Vector3 position = hit.position;
					context.Memory.LastClosestEdgeNormal = hit.normal;
					return position;
				}
				return context.Body.transform.position;
			}

			public static Vector3 GetContinuousDestinationFromBody(ScientistJunkpileContext context)
			{
				if (context.Memory.LastClosestEdgeNormal.sqrMagnitude < 0.01f)
				{
					return context.Body.transform.position;
				}
				BaseNpcMemory.EnemyPlayerInfo primaryKnownEnemyPlayer = context.Memory.PrimaryKnownEnemyPlayer;
				if (primaryKnownEnemyPlayer.PlayerInfo.Player != null)
				{
					Vector3 vector = context.Body.estimatedVelocity.normalized;
					if (vector.sqrMagnitude < 0.01f)
					{
						vector = context.Body.estimatedVelocity.normalized;
					}
					if (vector.sqrMagnitude < 0.01f)
					{
						vector = primaryKnownEnemyPlayer.LastKnownHeading;
					}
					NavMeshHit hit;
					if (NavMesh.FindClosestEdge(context.Body.transform.position + vector * 2f, out hit, context.Domain.NavAgent.areaMask))
					{
						if (Vector3.Dot(context.Memory.LastClosestEdgeNormal, hit.normal) > 0.9f)
						{
							return hit.position;
						}
						context.Memory.LastClosestEdgeNormal = hit.normal;
						return hit.position + hit.normal * 0.25f;
					}
				}
				return context.Body.transform.position;
			}

			public override OperatorStateType Tick(ScientistJunkpileContext context, PrimitiveTaskSelector task)
			{
				OperatorStateType operatorStateType = base.Tick(context, task);
				if (operatorStateType == OperatorStateType.Running)
				{
					if (context.Domain.NavAgent.remainingDistance < context.Domain.NavAgent.stoppingDistance + 0.5f)
					{
						OnContinuePath(context, task);
					}
					return operatorStateType;
				}
				return operatorStateType;
			}

			private void OnContinuePath(ScientistJunkpileContext context, PrimitiveTaskSelector task)
			{
				Vector3 continuousDestinationFromBody = GetContinuousDestinationFromBody(context);
				if (!((context.Body.transform.position - continuousDestinationFromBody).sqrMagnitude <= 0.2f))
				{
					OnPreStart(context);
					context.Domain.SetDestination(continuousDestinationFromBody);
					OnStart(context);
				}
			}

			protected override Vector3 _GetDestination(ScientistJunkpileContext context)
			{
				return GetDestination(context);
			}

			protected override void OnPreStart(ScientistJunkpileContext context)
			{
				context.Domain.NavAgent.stoppingDistance = 0.5f;
			}

			protected override void OnStart(ScientistJunkpileContext context)
			{
				context.SetFact(Facts.IsSearching, true);
			}

			protected override void OnPathFailed(ScientistJunkpileContext context)
			{
				context.SetFact(Facts.IsSearching, false);
				context.Domain.NavAgent.stoppingDistance = 1f;
			}

			protected override void OnPathComplete(ScientistJunkpileContext context)
			{
				if (DisableIsSearchingOnComplete)
				{
					context.SetFact(Facts.IsSearching, false);
				}
				context.Domain.NavAgent.stoppingDistance = 1f;
			}
		}

		public class JunkpileNavigateToPositionWhereWeLastSawPrimaryEnemyPlayer : BaseNavigateTo
		{
			[ApexSerialization]
			private bool DisableIsSearchingOnComplete = true;

			public static Vector3 GetDestination(ScientistJunkpileContext context)
			{
				BaseNpcMemory.EnemyPlayerInfo primaryKnownEnemyPlayer = context.Memory.PrimaryKnownEnemyPlayer;
				NavMeshHit hit;
				if (primaryKnownEnemyPlayer.PlayerInfo.Player != null && NavMesh.FindClosestEdge(primaryKnownEnemyPlayer.OurLastPositionWhenLastSeen, out hit, context.Domain.NavAgent.areaMask))
				{
					return context.Domain.ToAllowedMovementDestination(hit.position);
				}
				return context.Body.transform.position;
			}

			protected override Vector3 _GetDestination(ScientistJunkpileContext context)
			{
				return GetDestination(context);
			}

			protected override void OnPreStart(ScientistJunkpileContext context)
			{
				context.Domain.NavAgent.stoppingDistance = 0.5f;
			}

			protected override void OnStart(ScientistJunkpileContext context)
			{
				context.SetFact(Facts.IsSearching, true);
			}

			protected override void OnPathFailed(ScientistJunkpileContext context)
			{
				context.SetFact(Facts.IsSearching, false);
				context.Domain.NavAgent.stoppingDistance = 1f;
			}

			protected override void OnPathComplete(ScientistJunkpileContext context)
			{
				if (DisableIsSearchingOnComplete)
				{
					context.SetFact(Facts.IsSearching, false);
				}
				context.Domain.NavAgent.stoppingDistance = 1f;
			}
		}

		public class JunkpileNavigateAwayFromExplosive : BaseNavigateTo
		{
			[ApexSerialization]
			private bool DisableIsAvoidingExplosiveOnComplete = true;

			public static Vector3 GetDestination(ScientistJunkpileContext context)
			{
				BaseEntity baseEntity = null;
				Vector3 vector = Vector3.zero;
				float num = float.MaxValue;
				for (int i = 0; i < context.Memory.KnownTimedExplosives.Count; i++)
				{
					BaseNpcMemory.EntityOfInterestInfo entityOfInterestInfo = context.Memory.KnownTimedExplosives[i];
					if (entityOfInterestInfo.Entity != null)
					{
						Vector3 vector2 = context.BodyPosition - entityOfInterestInfo.Entity.transform.position;
						float sqrMagnitude = vector2.sqrMagnitude;
						if (sqrMagnitude < num)
						{
							vector = vector2;
							num = sqrMagnitude;
							baseEntity = entityOfInterestInfo.Entity;
						}
					}
				}
				if (baseEntity != null)
				{
					vector.Normalize();
					NavMeshHit hit;
					if (NavMesh.FindClosestEdge(context.BodyPosition + vector * 10f, out hit, context.Domain.NavAgent.areaMask))
					{
						context.Memory.LastClosestEdgeNormal = hit.normal;
						return hit.position;
					}
				}
				return context.Body.transform.position;
			}

			protected override Vector3 _GetDestination(ScientistJunkpileContext context)
			{
				return GetDestination(context);
			}

			protected override void OnPreStart(ScientistJunkpileContext context)
			{
				context.Domain.NavAgent.stoppingDistance = 0.5f;
			}

			protected override void OnStart(ScientistJunkpileContext context)
			{
				context.SetFact(Facts.IsAvoidingExplosive, true);
			}

			protected override void OnPathFailed(ScientistJunkpileContext context)
			{
				context.SetFact(Facts.IsAvoidingExplosive, false);
				context.Domain.NavAgent.stoppingDistance = 1f;
			}

			protected override void OnPathComplete(ScientistJunkpileContext context)
			{
				if (DisableIsAvoidingExplosiveOnComplete)
				{
					context.SetFact(Facts.IsAvoidingExplosive, false);
				}
				context.Domain.NavAgent.stoppingDistance = 1f;
			}
		}

		public class JunkpileNavigateAwayFromAnimal : BaseNavigateTo
		{
			[ApexSerialization]
			private bool DisableIsAvoidingAnimalOnComplete = true;

			public static Vector3 GetDestination(ScientistJunkpileContext context)
			{
				if (context.Memory.PrimaryKnownAnimal.Animal != null)
				{
					Vector3 normalized = (context.BodyPosition - context.Memory.PrimaryKnownAnimal.Animal.transform.position).normalized;
					NavMeshHit hit;
					if (NavMesh.FindClosestEdge(context.BodyPosition + normalized * 10f, out hit, context.Domain.NavAgent.areaMask))
					{
						context.Memory.LastClosestEdgeNormal = hit.normal;
						return hit.position;
					}
				}
				return context.Body.transform.position;
			}

			protected override Vector3 _GetDestination(ScientistJunkpileContext context)
			{
				return GetDestination(context);
			}

			protected override void OnPreStart(ScientistJunkpileContext context)
			{
				context.Domain.NavAgent.stoppingDistance = 0.5f;
			}

			protected override void OnStart(ScientistJunkpileContext context)
			{
				context.SetFact(Facts.IsAvoidingAnimal, true);
			}

			protected override void OnPathFailed(ScientistJunkpileContext context)
			{
				context.SetFact(Facts.IsAvoidingAnimal, false);
				context.Domain.NavAgent.stoppingDistance = 1f;
			}

			protected override void OnPathComplete(ScientistJunkpileContext context)
			{
				if (DisableIsAvoidingAnimalOnComplete)
				{
					context.SetFact(Facts.IsAvoidingAnimal, false);
				}
				context.Domain.NavAgent.stoppingDistance = 1f;
			}
		}

		public class JunkpileArrivedAtLocation : OperatorBase<ScientistJunkpileContext>
		{
			public override void Execute(ScientistJunkpileContext context)
			{
			}

			public override OperatorStateType Tick(ScientistJunkpileContext context, PrimitiveTaskSelector task)
			{
				ApplyExpectedEffects(context, task);
				return OperatorStateType.Complete;
			}

			public override void Abort(ScientistJunkpileContext context, PrimitiveTaskSelector task)
			{
			}
		}

		public class JunkpileStopMoving : OperatorBase<ScientistJunkpileContext>
		{
			public override void Execute(ScientistJunkpileContext context)
			{
				JunkpileIsNotNavigatingEffect.ApplyStatic(context, false, false);
				context.Domain.StopNavigating();
			}

			public override OperatorStateType Tick(ScientistJunkpileContext context, PrimitiveTaskSelector task)
			{
				ApplyExpectedEffects(context, task);
				return OperatorStateType.Complete;
			}

			public override void Abort(ScientistJunkpileContext context, PrimitiveTaskSelector task)
			{
			}
		}

		public class JunkpileDuck : OperatorBase<ScientistJunkpileContext>
		{
			public override void Execute(ScientistJunkpileContext context)
			{
				context.Body.modelState.ducked = true;
				JunkpileIsNotNavigatingEffect.ApplyStatic(context, false, false);
				context.Domain.StopNavigating();
			}

			public override OperatorStateType Tick(ScientistJunkpileContext context, PrimitiveTaskSelector task)
			{
				ApplyExpectedEffects(context, task);
				return OperatorStateType.Complete;
			}

			public override void Abort(ScientistJunkpileContext context, PrimitiveTaskSelector task)
			{
				context.Body.modelState.ducked = false;
			}
		}

		public class JunkpileDuckTimed : OperatorBase<ScientistJunkpileContext>
		{
			[ApexSerialization]
			private float _duckTimeMin = 1f;

			[ApexSerialization]
			private float _duckTimeMax = 1f;

			public override void Execute(ScientistJunkpileContext context)
			{
				context.Body.modelState.ducked = true;
				context.SetFact(Facts.IsDucking, true);
				JunkpileIsNotNavigatingEffect.ApplyStatic(context, false, false);
				context.Domain.StopNavigating();
				if (_duckTimeMin > _duckTimeMax)
				{
					float duckTimeMin = _duckTimeMin;
					_duckTimeMin = _duckTimeMax;
					_duckTimeMax = duckTimeMin;
				}
				float time = UnityEngine.Random.value * (_duckTimeMax - _duckTimeMin) + _duckTimeMin;
				context.Body.StartCoroutine(AsyncTimer(context, time));
			}

			public override OperatorStateType Tick(ScientistJunkpileContext context, PrimitiveTaskSelector task)
			{
				if (context.IsFact(Facts.IsDucking))
				{
					return OperatorStateType.Running;
				}
				ApplyExpectedEffects(context, task);
				return OperatorStateType.Complete;
			}

			public override void Abort(ScientistJunkpileContext context, PrimitiveTaskSelector task)
			{
				context.Body.StopCoroutine(AsyncTimer(context, 0f));
				Reset(context);
			}

			private IEnumerator AsyncTimer(ScientistJunkpileContext context, float time)
			{
				yield return CoroutineEx.waitForSeconds(time);
				Reset(context);
			}

			private void Reset(ScientistJunkpileContext context)
			{
				context.Body.modelState.ducked = false;
				context.SetFact(Facts.IsDucking, false);
			}
		}

		public class JunkpileStand : OperatorBase<ScientistJunkpileContext>
		{
			public override void Execute(ScientistJunkpileContext context)
			{
				context.SetFact(Facts.IsStandingUp, true);
				context.Body.StartCoroutine(AsyncTimer(context, 0.2f));
			}

			public override OperatorStateType Tick(ScientistJunkpileContext context, PrimitiveTaskSelector task)
			{
				if (context.IsFact(Facts.IsStandingUp))
				{
					return OperatorStateType.Running;
				}
				ApplyExpectedEffects(context, task);
				return OperatorStateType.Complete;
			}

			public override void Abort(ScientistJunkpileContext context, PrimitiveTaskSelector task)
			{
				context.Body.StopCoroutine(AsyncTimer(context, 0f));
				Reset(context);
			}

			private IEnumerator AsyncTimer(ScientistJunkpileContext context, float time)
			{
				yield return CoroutineEx.waitForSeconds(time);
				context.Body.modelState.ducked = false;
				context.SetFact(Facts.IsDucking, false);
				yield return CoroutineEx.waitForSeconds(time * 2f);
				context.SetFact(Facts.IsStandingUp, false);
			}

			private void Reset(ScientistJunkpileContext context)
			{
				context.Body.modelState.ducked = false;
				context.SetFact(Facts.IsDucking, false);
				context.SetFact(Facts.IsStandingUp, false);
			}
		}

		public class JunkpileIdle_JustStandAround : OperatorBase<ScientistJunkpileContext>
		{
			public override void Execute(ScientistJunkpileContext context)
			{
				ResetWorldState(context);
				context.SetFact(Facts.IsIdle, true);
				context.Domain.ReloadFirearm();
			}

			public override OperatorStateType Tick(ScientistJunkpileContext context, PrimitiveTaskSelector task)
			{
				return OperatorStateType.Running;
			}

			public override void Abort(ScientistJunkpileContext context, PrimitiveTaskSelector task)
			{
				context.SetFact(Facts.IsIdle, false);
			}

			private void ResetWorldState(ScientistJunkpileContext context)
			{
				context.SetFact(Facts.IsSearching, false);
				context.SetFact(Facts.IsNavigating, false);
				context.SetFact(Facts.IsLookingAround, false);
			}
		}

		public class JunkpileHoldLocation : OperatorBase<ScientistJunkpileContext>
		{
			public override void Execute(ScientistJunkpileContext context)
			{
				JunkpileIsNotNavigatingEffect.ApplyStatic(context, false, false);
				context.Domain.StopNavigating();
			}

			public override OperatorStateType Tick(ScientistJunkpileContext context, PrimitiveTaskSelector task)
			{
				return OperatorStateType.Running;
			}

			public override void Abort(ScientistJunkpileContext context, PrimitiveTaskSelector task)
			{
			}
		}

		public class JunkpileHoldLocationTimed : OperatorBase<ScientistJunkpileContext>
		{
			[ApexSerialization]
			private float _duckTimeMin = 1f;

			[ApexSerialization]
			private float _duckTimeMax = 1f;

			public override void Execute(ScientistJunkpileContext context)
			{
				JunkpileIsNotNavigatingEffect.ApplyStatic(context, false, false);
				context.Domain.StopNavigating();
				context.SetFact(Facts.IsWaiting, true);
				if (_duckTimeMin > _duckTimeMax)
				{
					float duckTimeMin = _duckTimeMin;
					_duckTimeMin = _duckTimeMax;
					_duckTimeMax = duckTimeMin;
				}
				float time = UnityEngine.Random.value * (_duckTimeMax - _duckTimeMin) + _duckTimeMin;
				context.Body.StartCoroutine(AsyncTimer(context, time));
			}

			public override OperatorStateType Tick(ScientistJunkpileContext context, PrimitiveTaskSelector task)
			{
				if (context.IsFact(Facts.IsWaiting))
				{
					return OperatorStateType.Running;
				}
				ApplyExpectedEffects(context, task);
				return OperatorStateType.Complete;
			}

			public override void Abort(ScientistJunkpileContext context, PrimitiveTaskSelector task)
			{
				context.SetFact(Facts.IsWaiting, false);
			}

			private IEnumerator AsyncTimer(ScientistJunkpileContext context, float time)
			{
				yield return CoroutineEx.waitForSeconds(time);
				context.SetFact(Facts.IsWaiting, false);
			}
		}

		public class JunkpileApplyFirearmOrder : OperatorBase<ScientistJunkpileContext>
		{
			public override void Execute(ScientistJunkpileContext context)
			{
			}

			public override OperatorStateType Tick(ScientistJunkpileContext context, PrimitiveTaskSelector task)
			{
				ApplyExpectedEffects(context, task);
				return OperatorStateType.Complete;
			}

			public override void Abort(ScientistJunkpileContext context, PrimitiveTaskSelector task)
			{
			}
		}

		public class JunkpileLookAround : OperatorBase<ScientistJunkpileContext>
		{
			[ApexSerialization]
			private float _lookAroundTime = 1f;

			public override void Execute(ScientistJunkpileContext context)
			{
				context.SetFact(Facts.IsLookingAround, true);
				context.Body.StartCoroutine(LookAroundAsync(context));
			}

			public override OperatorStateType Tick(ScientistJunkpileContext context, PrimitiveTaskSelector task)
			{
				if (context.IsFact(Facts.IsLookingAround))
				{
					return OperatorStateType.Running;
				}
				ApplyExpectedEffects(context, task);
				return OperatorStateType.Complete;
			}

			private IEnumerator LookAroundAsync(ScientistJunkpileContext context)
			{
				yield return CoroutineEx.waitForSeconds(_lookAroundTime);
				if (context.IsFact(Facts.CanSeeEnemy))
				{
					context.SetFact(Facts.IsSearching, false);
				}
				context.SetFact(Facts.IsLookingAround, false);
			}

			public override void Abort(ScientistJunkpileContext context, PrimitiveTaskSelector task)
			{
				context.SetFact(Facts.IsSearching, false);
				context.SetFact(Facts.IsLookingAround, false);
			}
		}

		public class JunkpileHoldItemOfType : OperatorBase<ScientistJunkpileContext>
		{
			[ApexSerialization]
			private ItemType _item;

			[ApexSerialization]
			private float _switchTime = 0.2f;

			public override void Execute(ScientistJunkpileContext context)
			{
				SwitchToItem(context, _item);
				context.Body.StartCoroutine(WaitAsync(context));
			}

			public override OperatorStateType Tick(ScientistJunkpileContext context, PrimitiveTaskSelector task)
			{
				if (context.IsFact(Facts.IsWaiting))
				{
					return OperatorStateType.Running;
				}
				ApplyExpectedEffects(context, task);
				return OperatorStateType.Complete;
			}

			private IEnumerator WaitAsync(ScientistJunkpileContext context)
			{
				context.SetFact(Facts.IsWaiting, true);
				yield return CoroutineEx.waitForSeconds(_switchTime);
				context.SetFact(Facts.IsWaiting, false);
			}

			public override void Abort(ScientistJunkpileContext context, PrimitiveTaskSelector task)
			{
				ItemType previousFact = (ItemType)context.GetPreviousFact(Facts.HeldItemType);
				SwitchToItem(context, previousFact);
				context.SetFact(Facts.IsWaiting, false);
			}

			public static void SwitchToItem(ScientistJunkpileContext context, ItemType _item)
			{
				context.Body.inventory.AllItemsNoAlloc(ref BaseNpcContext.InventoryLookupCache);
				foreach (Item item in BaseNpcContext.InventoryLookupCache)
				{
					if (_item == ItemType.HealingItem && item.info.category == ItemCategory.Medical && item.CanBeHeld())
					{
						context.Body.UpdateActiveItem(item.uid);
						context.SetFact(Facts.HeldItemType, _item);
						break;
					}
					if (_item == ItemType.MeleeWeapon && item.info.category == ItemCategory.Weapon && item.GetHeldEntity() is BaseMelee)
					{
						context.Body.UpdateActiveItem(item.uid);
						context.SetFact(Facts.HeldItemType, _item);
						break;
					}
					if (_item == ItemType.ProjectileWeapon && item.info.category == ItemCategory.Weapon && item.GetHeldEntity() is BaseProjectile)
					{
						context.Body.UpdateActiveItem(item.uid);
						context.SetFact(Facts.HeldItemType, _item);
						break;
					}
					if (_item == ItemType.ThrowableWeapon && item.info.category == ItemCategory.Weapon && item.GetHeldEntity() is ThrownWeapon)
					{
						context.Body.UpdateActiveItem(item.uid);
						context.SetFact(Facts.HeldItemType, _item);
						break;
					}
					HeldEntity heldEntity = item.GetHeldEntity() as HeldEntity;
					if (_item == ItemType.LightSourceItem && item.info.category == ItemCategory.Tool && item.CanBeHeld() && heldEntity != null && heldEntity.toolType == NPCPlayerApex.ToolTypeEnum.Lightsource)
					{
						context.Body.UpdateActiveItem(item.uid);
						context.SetFact(Facts.HeldItemType, _item);
						break;
					}
					if (_item == ItemType.ResearchItem && item.info.category == ItemCategory.Tool && item.CanBeHeld() && heldEntity != null && heldEntity.toolType == NPCPlayerApex.ToolTypeEnum.Research)
					{
						context.Body.UpdateActiveItem(item.uid);
						context.SetFact(Facts.HeldItemType, _item);
						break;
					}
				}
			}
		}

		public class JunkpileUseMedicalTool : OperatorBase<ScientistJunkpileContext>
		{
			[ApexSerialization]
			public HealthState Health;

			public override void Execute(ScientistJunkpileContext context)
			{
				context.Body.StartCoroutine(UseItem(context));
			}

			public override OperatorStateType Tick(ScientistJunkpileContext context, PrimitiveTaskSelector task)
			{
				if (context.IsFact(Facts.IsApplyingMedical))
				{
					return OperatorStateType.Running;
				}
				ApplyExpectedEffects(context, task);
				return OperatorStateType.Complete;
			}

			public override void Abort(ScientistJunkpileContext context, PrimitiveTaskSelector task)
			{
				context.SetFact(Facts.IsApplyingMedical, false);
				ItemType previousFact = (ItemType)context.GetPreviousFact(Facts.HeldItemType);
				JunkpileHoldItemOfType.SwitchToItem(context, previousFact);
			}

			private IEnumerator UseItem(ScientistJunkpileContext context)
			{
				Item activeItem = context.Body.GetActiveItem();
				if (activeItem != null)
				{
					MedicalTool medicalTool = activeItem.GetHeldEntity() as MedicalTool;
					if (medicalTool != null)
					{
						context.SetFact(Facts.IsApplyingMedical, true);
						medicalTool.ServerUse();
						if (Health == HealthState.FullHealth)
						{
							context.Body.Heal(context.Body.MaxHealth());
						}
						yield return CoroutineEx.waitForSeconds(medicalTool.repeatDelay * 4f);
					}
				}
				context.SetFact(Facts.IsApplyingMedical, false);
				ItemType previousFact = (ItemType)context.GetPreviousFact(Facts.HeldItemType);
				JunkpileHoldItemOfType.SwitchToItem(context, previousFact);
			}
		}

		public class JunkpileReloadFirearmOperator : OperatorBase<ScientistJunkpileContext>
		{
			public override void Execute(ScientistJunkpileContext context)
			{
				context.Domain.ReloadFirearm();
			}

			public override OperatorStateType Tick(ScientistJunkpileContext context, PrimitiveTaskSelector task)
			{
				if (context.IsFact(Facts.IsReloading))
				{
					return OperatorStateType.Running;
				}
				ApplyExpectedEffects(context, task);
				return OperatorStateType.Complete;
			}

			public override void Abort(ScientistJunkpileContext context, PrimitiveTaskSelector task)
			{
			}
		}

		public class JunkpileApplyFrustration : OperatorBase<ScientistJunkpileContext>
		{
			public override void Execute(ScientistJunkpileContext context)
			{
			}

			public override OperatorStateType Tick(ScientistJunkpileContext context, PrimitiveTaskSelector task)
			{
				ApplyExpectedEffects(context, task);
				return OperatorStateType.Complete;
			}

			public override void Abort(ScientistJunkpileContext context, PrimitiveTaskSelector task)
			{
			}
		}

		public class JunkpileUseThrowableWeapon : OperatorBase<ScientistJunkpileContext>
		{
			[ApexSerialization]
			private NpcOrientation _orientation = NpcOrientation.LastKnownPrimaryTargetLocation;

			public static float LastTimeThrown;

			public override void Execute(ScientistJunkpileContext context)
			{
				if (context.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.Player != null)
				{
					context.Body.StartCoroutine(UseItem(context));
				}
			}

			public override OperatorStateType Tick(ScientistJunkpileContext context, PrimitiveTaskSelector task)
			{
				if (context.IsFact(Facts.IsThrowingWeapon))
				{
					return OperatorStateType.Running;
				}
				ApplyExpectedEffects(context, task);
				return OperatorStateType.Complete;
			}

			public override void Abort(ScientistJunkpileContext context, PrimitiveTaskSelector task)
			{
				context.SetFact(Facts.IsThrowingWeapon, false);
				ItemType previousFact = (ItemType)context.GetPreviousFact(Facts.HeldItemType);
				JunkpileHoldItemOfType.SwitchToItem(context, previousFact);
			}

			private IEnumerator UseItem(ScientistJunkpileContext context)
			{
				Item activeItem = context.Body.GetActiveItem();
				if (activeItem != null)
				{
					ThrownWeapon thrownWeapon = activeItem.GetHeldEntity() as ThrownWeapon;
					if (thrownWeapon != null)
					{
						context.SetFact(Facts.IsThrowingWeapon, true);
						LastTimeThrown = UnityEngine.Time.time;
						context.OrientationType = _orientation;
						context.Body.ForceOrientationTick();
						yield return null;
						thrownWeapon.ServerThrow(context.Memory.PrimaryKnownEnemyPlayer.LastKnownPosition);
						yield return null;
					}
				}
				context.SetFact(Facts.IsThrowingWeapon, false);
				JunkpileHoldItemOfType.SwitchToItem(context, ItemType.ProjectileWeapon);
			}
		}

		public delegate void OnPlanAborted(ScientistJunkpileDomain domain);

		public delegate void OnPlanCompleted(ScientistJunkpileDomain domain);

		public class JunkpileHasWorldState : ContextualScorerBase<ScientistJunkpileContext>
		{
			[ApexSerialization]
			public Facts Fact;

			[ApexSerialization]
			public byte Value;

			public override float Score(ScientistJunkpileContext c)
			{
				if (c.GetWorldState(Fact) != Value)
				{
					return 0f;
				}
				return score;
			}
		}

		public class JunkpileHasWorldStateBool : ContextualScorerBase<ScientistJunkpileContext>
		{
			[ApexSerialization]
			public Facts Fact;

			[ApexSerialization]
			public bool Value;

			public override float Score(ScientistJunkpileContext c)
			{
				byte b = (byte)(Value ? 1u : 0u);
				if (c.GetWorldState(Fact) != b)
				{
					return 0f;
				}
				return score;
			}
		}

		public class JunkpileHasWorldStateGreaterThan : ContextualScorerBase<ScientistJunkpileContext>
		{
			[ApexSerialization]
			public Facts Fact;

			[ApexSerialization]
			public byte Value;

			public override float Score(ScientistJunkpileContext c)
			{
				if (c.GetWorldState(Fact) <= Value)
				{
					return 0f;
				}
				return score;
			}
		}

		public class JunkpileHasWorldStateLessThan : ContextualScorerBase<ScientistJunkpileContext>
		{
			[ApexSerialization]
			public Facts Fact;

			[ApexSerialization]
			public byte Value;

			public override float Score(ScientistJunkpileContext c)
			{
				if (c.GetWorldState(Fact) >= Value)
				{
					return 0f;
				}
				return score;
			}
		}

		public class JunkpileHasWorldStateEnemyRange : ContextualScorerBase<ScientistJunkpileContext>
		{
			[ApexSerialization]
			public EnemyRange Value;

			public override float Score(ScientistJunkpileContext c)
			{
				if ((uint)c.GetWorldState(Facts.EnemyRange) != (uint)Value)
				{
					return 0f;
				}
				return score;
			}
		}

		public class JunkpileHasWorldStateAmmo : ContextualScorerBase<ScientistJunkpileContext>
		{
			[ApexSerialization]
			public AmmoState Value;

			public override float Score(ScientistJunkpileContext c)
			{
				if ((uint)c.GetWorldState(Facts.AmmoState) != (uint)Value)
				{
					return 0f;
				}
				return score;
			}
		}

		public class JunkpileHasWorldStateHealth : ContextualScorerBase<ScientistJunkpileContext>
		{
			[ApexSerialization]
			public HealthState Value;

			public override float Score(ScientistJunkpileContext c)
			{
				if ((uint)c.GetWorldState(Facts.HealthState) != (uint)Value)
				{
					return 0f;
				}
				return score;
			}
		}

		public class JunkpileHasWorldStateCoverState : ContextualScorerBase<ScientistJunkpileContext>
		{
			[ApexSerialization]
			public CoverState Value;

			public override float Score(ScientistJunkpileContext c)
			{
				if ((uint)c.GetWorldState(Facts.CoverState) != (uint)Value)
				{
					return 0f;
				}
				return score;
			}
		}

		public class JunkpileHasItem : ContextualScorerBase<ScientistJunkpileContext>
		{
			[ApexSerialization]
			public ItemType Value;

			public override float Score(ScientistJunkpileContext c)
			{
				c.Body.inventory.AllItemsNoAlloc(ref BaseNpcContext.InventoryLookupCache);
				foreach (Item item in BaseNpcContext.InventoryLookupCache)
				{
					if (Value == ItemType.HealingItem && item.info.category == ItemCategory.Medical)
					{
						return score;
					}
					if (Value == ItemType.MeleeWeapon && item.info.category == ItemCategory.Weapon && item.GetHeldEntity() is BaseMelee)
					{
						return score;
					}
					if (Value == ItemType.ProjectileWeapon && item.info.category == ItemCategory.Weapon && item.GetHeldEntity() is BaseProjectile)
					{
						return score;
					}
					if (Value == ItemType.ThrowableWeapon && item.info.category == ItemCategory.Weapon && item.GetHeldEntity() is ThrownWeapon)
					{
						return score;
					}
					if (Value == ItemType.LightSourceItem && item.info.category == ItemCategory.Tool)
					{
						return score;
					}
					if (Value == ItemType.ResearchItem && item.info.category == ItemCategory.Tool)
					{
						return score;
					}
				}
				return 0f;
			}
		}

		public class JunkpileIsHoldingItem : ContextualScorerBase<ScientistJunkpileContext>
		{
			[ApexSerialization]
			public ItemType Value;

			public override float Score(ScientistJunkpileContext c)
			{
				if ((uint)c.GetWorldState(Facts.HeldItemType) == (uint)Value)
				{
					return score;
				}
				return 0f;
			}
		}

		public class JunkpileIsNavigationBlocked : ContextualScorerBase<ScientistJunkpileContext>
		{
			public override float Score(ScientistJunkpileContext c)
			{
				if (!CanNavigate(c))
				{
					return score;
				}
				return 0f;
			}

			public static bool CanNavigate(ScientistJunkpileContext c)
			{
				if (c.Domain.NavAgent != null && c.Domain.NavAgent.isOnNavMesh)
				{
					return true;
				}
				return false;
			}
		}

		public class JunkpileIsNavigationAllowed : ContextualScorerBase<ScientistJunkpileContext>
		{
			public override float Score(ScientistJunkpileContext c)
			{
				if (!JunkpileIsNavigationBlocked.CanNavigate(c))
				{
					return 0f;
				}
				return score;
			}
		}

		public class JunkpileIsReloadingBlocked : ContextualScorerBase<ScientistJunkpileContext>
		{
			public override float Score(ScientistJunkpileContext c)
			{
				return 0f;
			}
		}

		public class JunkpileIsReloadingAllowed : ContextualScorerBase<ScientistJunkpileContext>
		{
			public override float Score(ScientistJunkpileContext c)
			{
				return score;
			}
		}

		public class JunkpileIsShootingBlocked : ContextualScorerBase<ScientistJunkpileContext>
		{
			public override float Score(ScientistJunkpileContext c)
			{
				return 0f;
			}
		}

		public class JunkpileIsShootingAllowed : ContextualScorerBase<ScientistJunkpileContext>
		{
			public override float Score(ScientistJunkpileContext c)
			{
				return score;
			}
		}

		public class JunkpileHasFirearmOrder : ContextualScorerBase<ScientistJunkpileContext>
		{
			[ApexSerialization]
			public FirearmOrders Order;

			public override float Score(ScientistJunkpileContext c)
			{
				return score;
			}
		}

		public class JunkpileCanNavigateToWaypoint : ContextualScorerBase<ScientistJunkpileContext>
		{
			public override float Score(ScientistJunkpileContext c)
			{
				Vector3 nextWaypointPosition = JunkpileNavigateToWaypoint.GetNextWaypointPosition(c);
				if (!c.Memory.IsValid(nextWaypointPosition))
				{
					return 0f;
				}
				return score;
			}
		}

		public class JunkpileCanNavigateToPreferredFightingRange : ContextualScorerBase<ScientistJunkpileContext>
		{
			[ApexSerialization]
			private bool CanNot;

			public override float Score(ScientistJunkpileContext c)
			{
				Vector3 preferredFightingPosition = JunkpileNavigateToPreferredFightingRange.GetPreferredFightingPosition(c, false);
				if ((preferredFightingPosition - c.Body.transform.position).sqrMagnitude < 0.01f)
				{
					if (!CanNot)
					{
						return 0f;
					}
					return score;
				}
				bool flag = c.Memory.IsValid(preferredFightingPosition);
				if (flag)
				{
					flag = c.Domain.AllowedMovementDestination(preferredFightingPosition);
				}
				if (CanNot)
				{
					if (!flag)
					{
						return score;
					}
					return 0f;
				}
				if (!flag)
				{
					return 0f;
				}
				return score;
			}
		}

		public class JunkpileCanRememberPrimaryEnemyTarget : ContextualScorerBase<ScientistJunkpileContext>
		{
			public override float Score(ScientistJunkpileContext c)
			{
				if (!(c.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.Player != null))
				{
					return 0f;
				}
				return score;
			}
		}

		public class JunkpileCanNavigateToLastKnownPositionOfPrimaryEnemyTarget : ContextualScorerBase<ScientistJunkpileContext>
		{
			public override float Score(ScientistJunkpileContext c)
			{
				if (c.HasVisitedLastKnownEnemyPlayerLocation)
				{
					return score;
				}
				Vector3 destination = JunkpileNavigateToLastKnownLocationOfPrimaryEnemyPlayer.GetDestination(c);
				if (!c.Domain.AllowedMovementDestination(destination))
				{
					return 0f;
				}
				if ((destination - c.BodyPosition).sqrMagnitude < 0.1f)
				{
					return 0f;
				}
				if (!c.Memory.IsValid(destination))
				{
					return 0f;
				}
				return score;
			}
		}

		public class JunkpileCanNavigateToCoverLocation : ContextualScorerBase<ScientistJunkpileContext>
		{
			[ApexSerialization]
			private CoverTactic _preferredTactic;

			public override float Score(ScientistJunkpileContext c)
			{
				if (!Try(_preferredTactic, c))
				{
					return 0f;
				}
				return score;
			}

			public static bool Try(CoverTactic tactic, ScientistJunkpileContext c)
			{
				Vector3 coverPosition = JunkpileNavigateToCover.GetCoverPosition(tactic, c);
				if (!c.Domain.AllowedMovementDestination(coverPosition))
				{
					return false;
				}
				if ((coverPosition - c.BodyPosition).sqrMagnitude < 0.1f)
				{
					return false;
				}
				return c.Memory.IsValid(coverPosition);
			}
		}

		public class JunkpileCanNavigateAwayFromExplosive : ContextualScorerBase<ScientistJunkpileContext>
		{
			public override float Score(ScientistJunkpileContext c)
			{
				if (!Try(c))
				{
					return 0f;
				}
				return score;
			}

			public static bool Try(ScientistJunkpileContext c)
			{
				Vector3 destination = JunkpileNavigateAwayFromExplosive.GetDestination(c);
				if (!c.Domain.AllowedMovementDestination(destination))
				{
					return false;
				}
				if ((destination - c.BodyPosition).sqrMagnitude < 0.1f)
				{
					return false;
				}
				return c.Memory.IsValid(destination);
			}
		}

		public class JunkpileCanNavigateAwayFromAnimal : ContextualScorerBase<ScientistJunkpileContext>
		{
			public override float Score(ScientistJunkpileContext c)
			{
				if (!Try(c))
				{
					return 0f;
				}
				return score;
			}

			public static bool Try(ScientistJunkpileContext c)
			{
				Vector3 destination = JunkpileNavigateAwayFromAnimal.GetDestination(c);
				if (!c.Domain.AllowedMovementDestination(destination))
				{
					return false;
				}
				if ((destination - c.BodyPosition).sqrMagnitude < 0.1f)
				{
					return false;
				}
				return c.Memory.IsValid(destination);
			}
		}

		public class JunkpileCanUseWeaponAtCurrentRange : ContextualScorerBase<ScientistJunkpileContext>
		{
			public override float Score(ScientistJunkpileContext c)
			{
				if (!Try(c))
				{
					return 0f;
				}
				return score;
			}

			public static bool Try(ScientistJunkpileContext c)
			{
				AttackEntity firearm = c.Domain.GetFirearm();
				if (firearm == null)
				{
					return false;
				}
				switch (c.GetFact(Facts.EnemyRange))
				{
				case 2:
					return firearm.CanUseAtLongRange;
				case 1:
					return firearm.CanUseAtMediumRange;
				case 3:
					return firearm.CanUseAtLongRange;
				default:
					return true;
				}
			}
		}

		public class JunkpileCanThrowAtLastKnownLocation : ContextualScorerBase<ScientistJunkpileContext>
		{
			public override float Score(ScientistJunkpileContext c)
			{
				if (!Try(c))
				{
					return 0f;
				}
				return score;
			}

			public static bool Try(ScientistJunkpileContext c)
			{
				if (!ConVar.AI.npc_use_thrown_weapons)
				{
					return false;
				}
				if (c.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.Player == null)
				{
					return false;
				}
				if (UnityEngine.Time.time - JunkpileUseThrowableWeapon.LastTimeThrown < 8f)
				{
					return false;
				}
				Vector3 destination = JunkpileNavigateToLastKnownLocationOfPrimaryEnemyPlayer.GetDestination(c);
				if ((destination - c.BodyPosition).sqrMagnitude < 0.1f)
				{
					return false;
				}
				Vector3 vector = destination + PlayerEyes.EyeOffset;
				Vector3 vector2 = c.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.Player.transform.position + PlayerEyes.EyeOffset;
				if ((vector - vector2).sqrMagnitude > 8f)
				{
					return false;
				}
				Vector3 vector3 = c.BodyPosition + PlayerEyes.EyeOffset;
				if (Mathf.Abs(Vector3.Dot((vector3 - vector).normalized, (vector3 - vector2).normalized)) < 0.75f)
				{
					return false;
				}
				if (!c.Body.IsVisible(vector) && (!c.Memory.PrimaryKnownEnemyPlayer.HeadVisibleWhenLastNoticed || c.Memory.PrimaryKnownEnemyPlayer.BodyVisibleWhenLastNoticed))
				{
					return false;
				}
				return true;
			}
		}

		[SerializeField]
		[ReadOnly]
		private bool _isRegisteredWithAgency;

		[ReadOnly]
		[SerializeField]
		private static List<ScientistJunkpileDomain> _allJunkpileNPCs;

		private Vector3 missOffset;

		private float missToHeadingAlignmentTime;

		private float repeatMissTime;

		private bool recalculateMissOffset = true;

		private bool isMissing;

		private static Vector3[] pathCornerCache = new Vector3[128];

		private static NavMeshPath _pathCache = null;

		public OnPlanAborted OnPlanAbortedEvent;

		public OnPlanCompleted OnPlanCompletedEvent;

		[Header("Context")]
		[SerializeField]
		private ScientistJunkpileContext _context;

		[Header("Navigation")]
		[ReadOnly]
		[SerializeField]
		private NavMeshAgent _navAgent;

		[SerializeField]
		[ReadOnly]
		private Vector3 _spawnPosition;

		[Header("Sensors")]
		[ReadOnly]
		[SerializeField]
		private List<INpcSensor> _sensors = new List<INpcSensor>
		{
			new PlayersInRangeSensor
			{
				TickFrequency = 1f
			},
			new PlayersOutsideRangeSensor
			{
				TickFrequency = 0.1f
			},
			new PlayersDistanceSensor
			{
				TickFrequency = 0.1f
			},
			new PlayersViewAngleSensor
			{
				TickFrequency = 0.1f
			},
			new EnemyPlayersInRangeSensor
			{
				TickFrequency = 0.1f
			},
			new EnemyPlayersLineOfSightSensor
			{
				TickFrequency = 0.25f
			},
			new EnemyPlayersHearingSensor
			{
				TickFrequency = 0.1f
			},
			new CoverPointsInRangeSensor
			{
				TickFrequency = 1f
			},
			new AnimalsInRangeSensor
			{
				TickFrequency = 1f
			},
			new AnimalDistanceSensor
			{
				TickFrequency = 0.25f
			}
		};

		[Header("Reasoners")]
		[ReadOnly]
		[SerializeField]
		private List<INpcReasoner> _reasoners = new List<INpcReasoner>
		{
			new EnemyPlayerMarkTooCloseReasoner
			{
				TickFrequency = 0.1f
			},
			new EnemyPlayerLineOfSightReasoner
			{
				TickFrequency = 0.1f
			},
			new EnemyPlayerHearingReasoner
			{
				TickFrequency = 0.1f
			},
			new EnemyTargetReasoner
			{
				TickFrequency = 0.1f
			},
			new FireTacticReasoner
			{
				TickFrequency = 0.1f
			},
			new OrientationReasoner
			{
				TickFrequency = 0.01f
			},
			new PreferredFightingRangeReasoner
			{
				TickFrequency = 0.1f
			},
			new AtLastKnownEnemyPlayerLocationReasoner
			{
				TickFrequency = 0.1f
			},
			new FirearmPoseReasoner
			{
				TickFrequency = 0.1f
			},
			new HealthReasoner
			{
				TickFrequency = 0.1f
			},
			new AmmoReasoner
			{
				TickFrequency = 0.1f
			},
			new VulnerabilityReasoner
			{
				TickFrequency = 0.1f
			},
			new FrustrationReasoner
			{
				TickFrequency = 0.25f
			},
			new CoverPointsReasoner
			{
				TickFrequency = 0.5f
			},
			new AtCoverLocationReasoner
			{
				TickFrequency = 0.1f
			},
			new MaintainCoverReasoner
			{
				TickFrequency = 0.1f
			},
			new ReturnHomeReasoner
			{
				TickFrequency = 5f
			},
			new AtHomeLocationReasoner
			{
				TickFrequency = 5f
			},
			new ExplosivesReasoner
			{
				TickFrequency = 0.1f
			},
			new AnimalReasoner
			{
				TickFrequency = 0.25f
			},
			new AlertnessReasoner
			{
				TickFrequency = 0.1f
			},
			new EnemyRangeReasoner
			{
				TickFrequency = 0.1f
			},
			new FollowWaypointsReasoner
			{
				TickFrequency = 0.125f
			}
		};

		[SerializeField]
		[ReadOnly]
		[Header("Firearm Utility")]
		private float _lastFirearmUsageTime;

		[SerializeField]
		[ReadOnly]
		private bool _isFiring;

		[ReadOnly]
		[SerializeField]
		public bool ReducedLongRangeAccuracy;

		private HTNUtilityAiClient _aiClient;

		private ScientistJunkpileDefinition _scientistJunkpileDefinition;

		public static List<ScientistJunkpileDomain> AllJunkpileNPCs => _allJunkpileNPCs;

		public ScientistJunkpileDefinition ScientistDefinition
		{
			get
			{
				if (_scientistJunkpileDefinition == null)
				{
					_scientistJunkpileDefinition = _context.Body.AiDefinition as ScientistJunkpileDefinition;
				}
				return _scientistJunkpileDefinition;
			}
		}

		public Vector3 SpawnPosition => _spawnPosition;

		public ScientistJunkpileContext ScientistContext => _context;

		public override BaseNpcContext NpcContext => _context;

		public override IHTNContext PlannerContext => _context;

		public override IUtilityAI PlannerAi => _aiClient.ai;

		public override IUtilityAIClient PlannerAiClient => _aiClient;

		public override NavMeshAgent NavAgent => _navAgent;

		public override List<INpcSensor> Sensors => _sensors;

		public override List<INpcReasoner> Reasoners => _reasoners;

		private void InitializeAgency()
		{
			if (!(SingletonComponent<AiManager>.Instance == null) && SingletonComponent<AiManager>.Instance.enabled && ConVar.AI.npc_enable && !_isRegisteredWithAgency)
			{
				_isRegisteredWithAgency = true;
				SingletonComponent<AiManager>.Instance.HTNAgency.Add(_context.Body);
				if (_allJunkpileNPCs == null)
				{
					_allJunkpileNPCs = new List<ScientistJunkpileDomain>();
				}
				_allJunkpileNPCs.Add(this);
				if (_context.Junkpile != null)
				{
					_context.Junkpile.AddNpc(_context.Body);
				}
			}
		}

		private void RemoveAgency()
		{
			if (!(SingletonComponent<AiManager>.Instance == null) && _isRegisteredWithAgency)
			{
				_isRegisteredWithAgency = false;
				SingletonComponent<AiManager>.Instance.HTNAgency.Remove(_context.Body);
				_allJunkpileNPCs?.Remove(this);
			}
		}

		public override void Resume()
		{
			ResumeNavigation();
		}

		public override void Pause()
		{
			PauseNavigation();
		}

		private void TickFirearm(float time)
		{
			if (_isFiring || !_context.IsBodyAlive())
			{
				return;
			}
			if (_context.IsFact(Facts.IsUsingTool) && TOD_Sky.Instance != null && !_context.IsFact(Facts.HasEnemyTarget) && _context.Domain.ScientistContext.Memory.PrimaryKnownAnimal.Animal == null)
			{
				ItemType fact = (ItemType)_context.GetFact(Facts.HeldItemType);
				if (TOD_Sky.Instance.IsNight && fact != ItemType.LightSourceItem)
				{
					JunkpileHoldItemOfType.SwitchToItem(_context, ItemType.LightSourceItem);
				}
				else if (TOD_Sky.Instance.IsDay && fact != ItemType.ResearchItem)
				{
					JunkpileHoldItemOfType.SwitchToItem(_context, ItemType.ResearchItem);
				}
			}
			else if (_context.IsFact(Facts.HasEnemyTarget) || !(_context.Domain.ScientistContext.Memory.PrimaryKnownAnimal.Animal == null))
			{
				switch (_context.GetFact(Facts.FirearmOrder))
				{
				case 1:
					TickFirearm(time, 0f);
					break;
				case 2:
					TickFirearm(time, 0.2f);
					break;
				case 3:
					TickFirearm(time, 0.5f);
					break;
				}
			}
		}

		private void TickFirearm(float time, float interval)
		{
			AttackEntity attackEntity = ReloadFirearmIfEmpty();
			if (attackEntity == null || !(attackEntity is BaseProjectile))
			{
				JunkpileHoldItemOfType.SwitchToItem(_context, ItemType.ProjectileWeapon);
			}
			if (time - _lastFirearmUsageTime < interval || attackEntity == null)
			{
				return;
			}
			AnimalInfo primaryKnownAnimal = _context.Memory.PrimaryKnownAnimal;
			NpcPlayerInfo primaryEnemyPlayerTarget = _context.GetPrimaryEnemyPlayerTarget();
			if (primaryEnemyPlayerTarget.Player == null || (!primaryEnemyPlayerTarget.BodyVisible && !primaryEnemyPlayerTarget.HeadVisible))
			{
				if (primaryKnownAnimal.Animal == null || !CanUseFirearmAtRange(primaryKnownAnimal.SqrDistance))
				{
					return;
				}
			}
			else if (!CanUseFirearmAtRange(primaryEnemyPlayerTarget.SqrDistance))
			{
				return;
			}
			BaseProjectile baseProjectile = attackEntity as BaseProjectile;
			if (!baseProjectile || !(baseProjectile.NextAttackTime > time))
			{
				switch (_context.GetFact(Facts.FireTactic))
				{
				default:
					FireSingle(attackEntity, time);
					break;
				case 0:
					FireBurst(baseProjectile, time);
					break;
				case 2:
					FireFullAuto(baseProjectile, time);
					break;
				}
			}
		}

		private void FireFullAuto(BaseProjectile proj, float time)
		{
			if (!(proj == null))
			{
				StartCoroutine(HoldTriggerLogic(proj, time, 4f));
			}
		}

		private void FireBurst(BaseProjectile proj, float time)
		{
			if (!(proj == null))
			{
				StartCoroutine(HoldTriggerLogic(proj, time, UnityEngine.Random.Range(proj.attackLengthMin, proj.attackLengthMax)));
			}
		}

		private void FireSingle(AttackEntity attackEnt, float time)
		{
			if (_context.EnemyPlayersInLineOfSight.Count > 3)
			{
				attackEnt.ServerUse((1f + UnityEngine.Random.value * 0.5f) * ConVar.AI.npc_htn_player_base_damage_modifier);
			}
			else if (_context.PrimaryEnemyPlayerInLineOfSight.Player != null && _context.PrimaryEnemyPlayerInLineOfSight.Player.healthFraction < 0.2f)
			{
				attackEnt.ServerUse((0.1f + UnityEngine.Random.value * 0.5f) * ConVar.AI.npc_htn_player_base_damage_modifier);
			}
			else
			{
				attackEnt.ServerUse(ConVar.AI.npc_htn_player_base_damage_modifier);
			}
			_lastFirearmUsageTime = time + attackEnt.attackSpacing * (0.5f + UnityEngine.Random.value * 0.5f);
			_context.IncrementFact(Facts.Vulnerability, 1);
		}

		private IEnumerator HoldTriggerLogic(BaseProjectile proj, float startTime, float triggerDownInterval)
		{
			_isFiring = true;
			_lastFirearmUsageTime = startTime + triggerDownInterval + proj.attackSpacing;
			_context.IncrementFact(Facts.Vulnerability, 1);
			while (UnityEngine.Time.time - startTime < triggerDownInterval && _context.IsBodyAlive() && _context.IsFact(Facts.CanSeeEnemy) && !(_context.Domain.ScientistContext.Memory.PrimaryKnownAnimal.Animal == null))
			{
				if (_context.EnemyPlayersInLineOfSight.Count > 3)
				{
					proj.ServerUse((1f + UnityEngine.Random.value * 0.5f) * ConVar.AI.npc_htn_player_base_damage_modifier);
				}
				else if (_context.PrimaryEnemyPlayerInLineOfSight.Player != null && _context.PrimaryEnemyPlayerInLineOfSight.Player.healthFraction < 0.2f)
				{
					proj.ServerUse((0.1f + UnityEngine.Random.value * 0.5f) * ConVar.AI.npc_htn_player_base_damage_modifier);
				}
				else
				{
					proj.ServerUse(ConVar.AI.npc_htn_player_base_damage_modifier);
				}
				yield return CoroutineEx.waitForSeconds(proj.repeatDelay);
				if (proj.primaryMagazine.contents <= 0)
				{
					break;
				}
			}
			_isFiring = false;
		}

		public AttackEntity GetFirearm()
		{
			return _context.Body.GetHeldEntity() as AttackEntity;
		}

		public BaseProjectile GetFirearmProj()
		{
			AttackEntity firearm = GetFirearm();
			if ((bool)firearm)
			{
				return firearm as BaseProjectile;
			}
			return null;
		}

		public BaseProjectile ReloadFirearmProjIfEmpty()
		{
			BaseProjectile firearmProj = GetFirearmProj();
			ReloadFirearmIfEmpty(firearmProj);
			return firearmProj;
		}

		public AttackEntity ReloadFirearmIfEmpty()
		{
			AttackEntity firearm = GetFirearm();
			if ((bool)firearm)
			{
				BaseProjectile proj = firearm as BaseProjectile;
				ReloadFirearmIfEmpty(proj);
			}
			return firearm;
		}

		public void ReloadFirearmIfEmpty(BaseProjectile proj)
		{
			if ((bool)proj && proj.primaryMagazine.contents <= 0)
			{
				ReloadFirearm(proj);
			}
		}

		public BaseProjectile ReloadFirearm()
		{
			BaseProjectile firearmProj = GetFirearmProj();
			ReloadFirearm(firearmProj);
			return firearmProj;
		}

		public void ReloadFirearm(BaseProjectile proj)
		{
			if ((bool)proj && _context.IsBodyAlive() && proj.primaryMagazine.contents < proj.primaryMagazine.capacity)
			{
				StartCoroutine(ReloadHandler(proj));
			}
		}

		private IEnumerator ReloadHandler(BaseProjectile proj)
		{
			_context.SetFact(Facts.IsReloading, true);
			proj.ServerReload();
			yield return CoroutineEx.waitForSeconds(proj.reloadTime);
			_context.SetFact(Facts.IsReloading, false);
		}

		private bool CanUseFirearmAtRange(float sqrRange)
		{
			AttackEntity firearm = GetFirearm();
			if (firearm == null)
			{
				return false;
			}
			if (sqrRange <= _context.Body.AiDefinition.Engagement.SqrCloseRangeFirearm(firearm))
			{
				return true;
			}
			if (sqrRange <= _context.Body.AiDefinition.Engagement.SqrMediumRangeFirearm(firearm))
			{
				return firearm.CanUseAtMediumRange;
			}
			return firearm.CanUseAtLongRange;
		}

		public override void ForceProjectileOrientation()
		{
			if (_context.OrientationType == NpcOrientation.LookAtAnimal || _context.OrientationType == NpcOrientation.PrimaryTargetBody || _context.OrientationType == NpcOrientation.PrimaryTargetHead)
			{
				return;
			}
			if (_context.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.Player != null)
			{
				if (!_context.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.BodyVisible && _context.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.HeadVisible)
				{
					_context.OrientationType = NpcOrientation.PrimaryTargetHead;
				}
				else
				{
					_context.OrientationType = NpcOrientation.PrimaryTargetBody;
				}
			}
			else if (_context.Memory.PrimaryKnownAnimal.Animal != null)
			{
				_context.OrientationType = NpcOrientation.LookAtAnimal;
			}
		}

		public Vector3 ModifyFirearmAim(Vector3 heading, Vector3 target, Vector3 origin, float swayModifier = 1f)
		{
			if (!ConVar.AI.npc_use_new_aim_system)
			{
				AttackEntity firearm = GetFirearm();
				if ((bool)firearm)
				{
					return firearm.ModifyAIAim(heading, swayModifier);
				}
			}
			float sqrMagnitude = (target - origin).sqrMagnitude;
			float num = (int)_context.GetFact(Facts.Alertness);
			if (num > 10f)
			{
				num = 10f;
			}
			AttackEntity firearm2 = GetFirearm();
			if (sqrMagnitude <= _context.Body.AiDefinition.Engagement.SqrCloseRangeFirearm(firearm2) + 2f)
			{
				return heading;
			}
			if (ReducedLongRangeAccuracy && sqrMagnitude > _context.Body.AiDefinition.Engagement.SqrMediumRangeFirearm(firearm2))
			{
				num *= 0.5f;
			}
			if (_context.PrimaryEnemyPlayerInLineOfSight.Player != null && (_context.PrimaryEnemyPlayerInLineOfSight.Player.modelState.jumped || (!_context.PrimaryEnemyPlayerInLineOfSight.BodyVisible && _context.PrimaryEnemyPlayerInLineOfSight.HeadVisible)))
			{
				num *= 0.5f;
			}
			return GetMissVector(heading, target, origin, ConVar.AI.npc_deliberate_miss_to_hit_alignment_time * 1.1f, num * ConVar.AI.npc_alertness_to_aim_modifier);
		}

		private Vector3 GetMissVector(Vector3 heading, Vector3 target, Vector3 origin, float maxTime, float repeatTime)
		{
			float time = UnityEngine.Time.time;
			if (!isMissing && repeatMissTime < time)
			{
				if (!recalculateMissOffset)
				{
					return heading;
				}
				missOffset = Vector3.zero;
				missOffset.x = ((UnityEngine.Random.value > 0.5f) ? 1f : (-1f));
				missOffset *= ConVar.AI.npc_deliberate_miss_offset_multiplier;
				missToHeadingAlignmentTime = time + maxTime;
				repeatMissTime = missToHeadingAlignmentTime + repeatTime;
				recalculateMissOffset = false;
				isMissing = true;
			}
			Vector3 vector = target + missOffset - origin;
			float num = Mathf.Max(missToHeadingAlignmentTime - time, 0f);
			float num2 = ScientistDefinition.MissFunction.Evaluate(Mathf.Approximately(num, 0f) ? 1f : (1f - Mathf.Min(num / maxTime, 1f)));
			if (Mathf.Approximately(num2, 1f))
			{
				recalculateMissOffset = true;
				isMissing = false;
				float num3 = Mathf.Min(1f, ConVar.AI.npc_deliberate_hit_randomizer);
				return Vector3.Lerp(vector.normalized, heading, 1f - num3 + UnityEngine.Random.value * num3);
			}
			return Vector3.Lerp(vector.normalized, heading, num2);
		}

		public void PauseNavigation()
		{
			if (NavAgent != null && NavAgent.enabled)
			{
				NavAgent.enabled = false;
			}
		}

		public void ResumeNavigation()
		{
			if (!(NavAgent == null))
			{
				if (!NavAgent.isOnNavMesh)
				{
					StartCoroutine(TryForceToNavmesh());
					return;
				}
				NavAgent.enabled = true;
				NavAgent.stoppingDistance = 1f;
				UpdateNavmeshOffset();
			}
		}

		public override Vector3 GetNextPosition(float delta)
		{
			if (NavAgent == null || !NavAgent.isOnNavMesh || !NavAgent.hasPath)
			{
				return _context.BodyPosition;
			}
			return NavAgent.nextPosition;
		}

		private void UpdateNavmeshOffset()
		{
			float num = _spawnPosition.y - _context.BodyPosition.y;
			if (num < 0f)
			{
				num = Mathf.Max(num, -0.25f);
				NavAgent.baseOffset = num;
			}
		}

		private IEnumerator TryForceToNavmesh()
		{
			yield return null;
			int numTries = 0;
			float waitForRetryTime2 = 1f;
			float maxDistanceMultiplier = 2f;
			if (SingletonComponent<DynamicNavMesh>.Instance != null)
			{
				while (SingletonComponent<DynamicNavMesh>.Instance.IsBuilding)
				{
					yield return CoroutineEx.waitForSecondsRealtime(waitForRetryTime2);
					waitForRetryTime2 += 0.5f;
				}
			}
			waitForRetryTime2 = 1f;
			for (; numTries < 3; numTries++)
			{
				if (NavAgent != null && !NavAgent.isOnNavMesh)
				{
					NavMeshHit hit;
					if (NavMesh.SamplePosition(_context.Body.transform.position, out hit, NavAgent.height * maxDistanceMultiplier, NavAgent.areaMask))
					{
						_context.Body.transform.position = hit.position;
						NavAgent.Warp(_context.Body.transform.position);
						NavAgent.enabled = true;
						NavAgent.stoppingDistance = 1f;
						UpdateNavmeshOffset();
						SetDestination(hit.position + Vector3.forward);
						yield break;
					}
					yield return CoroutineEx.waitForSecondsRealtime(waitForRetryTime2);
					maxDistanceMultiplier *= 1.5f;
					continue;
				}
				NavAgent.enabled = true;
				NavAgent.stoppingDistance = 1f;
				yield break;
			}
			int areaFromName = NavMesh.GetAreaFromName("Walkable");
			if ((NavAgent.areaMask & (1 << areaFromName)) == 0)
			{
				NavMeshBuildSettings settingsByIndex = NavMesh.GetSettingsByIndex(1);
				NavAgent.agentTypeID = settingsByIndex.agentTypeID;
				NavAgent.areaMask = 1 << areaFromName;
				yield return TryForceToNavmesh();
			}
			else if (_context.Body.transform != null && !_context.Body.IsDestroyed)
			{
				Debug.LogWarningFormat("Failed to spawn {0} on a valid navmesh.", base.name);
				_context.Body.Kill();
			}
		}

		public bool SetDestination(Vector3 destination)
		{
			if (NavAgent == null || !NavAgent.isOnNavMesh)
			{
				_context.SetFact(Facts.PathStatus, (byte)3, true, false, true);
				return false;
			}
			destination = ToAllowedMovementDestination(destination);
			_context.Memory.HasTargetDestination = true;
			_context.Memory.TargetDestination = destination;
			_context.Domain.NavAgent.destination = destination;
			if (!IsPathValid())
			{
				_context.Memory.AddFailedDestination(_context.Memory.TargetDestination);
				_context.Domain.NavAgent.isStopped = true;
				_context.SetFact(Facts.PathStatus, (byte)3, true, false, true);
				return false;
			}
			_context.Domain.NavAgent.isStopped = false;
			_context.SetFact(Facts.PathStatus, (byte)1, true, false, true);
			return true;
		}

		public override void TickDestinationTracker()
		{
			if (NavAgent == null || !NavAgent.isOnNavMesh)
			{
				_context.SetFact(Facts.PathStatus, (byte)0, true, false, true);
				return;
			}
			if (!IsPathValid())
			{
				_context.Memory.AddFailedDestination(_context.Memory.TargetDestination);
				_context.Domain.NavAgent.isStopped = true;
				_context.Memory.HasTargetDestination = false;
				_context.SetFact(Facts.PathStatus, (byte)3, true, false, true);
			}
			if (_context.Memory.HasTargetDestination && _context.Domain.NavAgent.remainingDistance <= _context.Domain.NavAgent.stoppingDistance)
			{
				_context.Memory.HasTargetDestination = false;
				_context.SetFact(Facts.PathStatus, (byte)2, true, false, true);
			}
			if (_context.Memory.HasTargetDestination && NavAgent.hasPath)
			{
				_context.SetFact(Facts.PathStatus, (byte)1, true, false, true);
			}
			else
			{
				_context.SetFact(Facts.PathStatus, (byte)0, true, false, true);
			}
		}

		public bool IsPathValid()
		{
			if (!_context.IsBodyAlive())
			{
				return false;
			}
			if (_context.Memory.HasTargetDestination && !_context.Domain.NavAgent.pathPending && (_context.Domain.NavAgent.pathStatus != 0 || (_context.Domain.NavAgent.destination - _context.Memory.TargetDestination).sqrMagnitude > 0.01f))
			{
				return false;
			}
			if (!AllowedMovementDestination(_context.Memory.TargetDestination))
			{
				return false;
			}
			return true;
		}

		public override Vector3 GetHeadingDirection()
		{
			if (NavAgent != null && NavAgent.isOnNavMesh && _context.GetFact(Facts.IsNavigating) > 0)
			{
				return NavAgent.desiredVelocity.normalized;
			}
			return _context.Body.transform.forward;
		}

		public override Vector3 GetHomeDirection()
		{
			Vector3 v = SpawnPosition - _context.BodyPosition;
			if (v.SqrMagnitudeXZ() < 0.01f)
			{
				return _context.Body.transform.forward;
			}
			return v.normalized;
		}

		public void StopNavigating()
		{
			if (NavAgent != null && NavAgent.isOnNavMesh)
			{
				NavAgent.isStopped = true;
			}
			_context.Memory.HasTargetDestination = false;
			_context.SetFact(Facts.PathStatus, (byte)0, true, false, true);
		}

		public bool PathDistanceIsValid(Vector3 from, Vector3 to, bool allowCloseRange = false)
		{
			float sqrMagnitude = (from - to).sqrMagnitude;
			if (sqrMagnitude > ScientistContext.Body.AiDefinition.Engagement.SqrMediumRange || (!allowCloseRange && sqrMagnitude < ScientistContext.Body.AiDefinition.Engagement.SqrCloseRange))
			{
				return true;
			}
			float num = Mathf.Sqrt(sqrMagnitude);
			if (_pathCache == null)
			{
				_pathCache = new NavMeshPath();
			}
			if (NavMesh.CalculatePath(from, to, NavAgent.areaMask, _pathCache))
			{
				int cornersNonAlloc = _pathCache.GetCornersNonAlloc(pathCornerCache);
				if (_pathCache.status == NavMeshPathStatus.PathComplete && cornersNonAlloc > 1)
				{
					float num2 = PathDistance(cornersNonAlloc, ref pathCornerCache, num + ConVar.AI.npc_cover_path_vs_straight_dist_max_diff);
					if (Mathf.Abs(num - num2) > ConVar.AI.npc_cover_path_vs_straight_dist_max_diff)
					{
						return false;
					}
				}
			}
			return true;
		}

		private float PathDistance(int count, ref Vector3[] path, float maxDistance)
		{
			if (count < 2)
			{
				return 0f;
			}
			Vector3 a = path[0];
			float num = 0f;
			for (int i = 0; i < count; i++)
			{
				Vector3 vector = path[i];
				num += Vector3.Distance(a, vector);
				a = vector;
				if (num > maxDistance)
				{
					return num;
				}
			}
			return num;
		}

		public override float SqrDistanceToSpawn()
		{
			return (_context.BodyPosition - SpawnPosition).sqrMagnitude;
		}

		public override bool AllowedMovementDestination(Vector3 destination)
		{
			if (Movement == MovementRule.FreeMove)
			{
				return true;
			}
			if (Movement == MovementRule.NeverMove)
			{
				return false;
			}
			return (SpawnPosition - destination).sqrMagnitude <= base.SqrMovementRadius + 0.1f;
		}

		public Vector3 ToAllowedMovementDestination(Vector3 destination)
		{
			if (!AllowedMovementDestination(destination))
			{
				Vector3 normalized = (destination - _context.Domain.SpawnPosition).normalized;
				destination = _context.Domain.SpawnPosition + normalized * MovementRadius;
			}
			return destination;
		}

		public float GetAllowedCoverRangeSqr()
		{
			float result = 225f;
			if (Movement == MovementRule.RestrainedMove && MovementRadius < 15f)
			{
				result = base.SqrMovementRadius;
			}
			return result;
		}

		protected override void AbortPlan()
		{
			base.AbortPlan();
			OnPlanAbortedEvent?.Invoke(this);
			_context.SetFact(Facts.MaintainCover, 0, false);
			_context.Body.modelState.ducked = false;
			_context.SetFact(Facts.IsDucking, false, false);
			_context.SetFact(Facts.IsUsingTool, 0, false);
			_context.SetFact(Facts.IsNavigating, 0, false);
		}

		protected override void CompletePlan()
		{
			base.CompletePlan();
			OnPlanCompletedEvent?.Invoke(this);
			_context.SetFact(Facts.MaintainCover, 0, false);
			_context.Body.modelState.ducked = false;
			_context.SetFact(Facts.IsDucking, false, false);
			_context.SetFact(Facts.IsUsingTool, 0, false);
			_context.SetFact(Facts.IsNavigating, 0, false);
		}

		protected override bool CanTickReasoner(float deltaTime, INpcReasoner reasoner)
		{
			if (_context.Memory.MarkedEnemies.Count == 0 && _context.Domain.ScientistContext.Memory.PrimaryKnownAnimal.Animal == null)
			{
				return deltaTime >= reasoner.TickFrequency * 4f;
			}
			return deltaTime >= reasoner.TickFrequency;
		}

		protected override void TickReasoner(INpcReasoner reasoner, float deltaTime, float time)
		{
			reasoner.Tick(_context.Body, deltaTime, time);
		}

		public override void OnSensation(Sensation sensation)
		{
			switch (sensation.Type)
			{
			case SensationType.Gunshot:
				OnGunshotSensation(ref sensation);
				break;
			case SensationType.ThrownWeapon:
				OnThrownWeaponSensation(ref sensation);
				break;
			case SensationType.Explosion:
				OnExplosionSensation(ref sensation);
				break;
			}
		}

		private void OnGunshotSensation(ref Sensation info)
		{
			BasePlayer initiatorPlayer = info.InitiatorPlayer;
			if (!(initiatorPlayer != null) || !(initiatorPlayer != _context.Body))
			{
				return;
			}
			bool flag = false;
			foreach (NpcPlayerInfo item in _context.EnemyPlayersInRange)
			{
				if (RememberGunshot(ref info, item, initiatorPlayer))
				{
					if (_context.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.Player == null || _context.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.Player == initiatorPlayer)
					{
						_context.Memory.RememberPrimaryEnemyPlayer(initiatorPlayer);
					}
					_context.IncrementFact(Facts.Vulnerability, (!_context.IsFact(Facts.CanSeeEnemy)) ? 1 : 0);
					_context.IncrementFact(Facts.Alertness, 1);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				_context.IncrementFact(Facts.Vulnerability, 1);
				_context.IncrementFact(Facts.Alertness, 1);
				_context.PlayersOutsideDetectionRange.Add(new NpcPlayerInfo
				{
					Player = initiatorPlayer,
					Time = UnityEngine.Time.time
				});
			}
		}

		private void OnThrownWeaponSensation(ref Sensation info)
		{
			RememberEntityOfInterest(ref info);
			if (!_context.IsFact(Facts.CanSeeEnemy) || !_context.IsFact(Facts.CanHearEnemy))
			{
				return;
			}
			BasePlayer initiatorPlayer = info.InitiatorPlayer;
			if (!(initiatorPlayer != null) || !(initiatorPlayer != _context.Body))
			{
				return;
			}
			bool flag = false;
			foreach (NpcPlayerInfo item in _context.EnemyPlayersInRange)
			{
				if (RememberThrownItem(ref info, item, initiatorPlayer))
				{
					if (_context.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.Player == null)
					{
						_context.Memory.RememberPrimaryEnemyPlayer(initiatorPlayer);
					}
					_context.IncrementFact(Facts.Vulnerability, (!_context.IsFact(Facts.CanSeeEnemy)) ? 1 : 0);
					_context.IncrementFact(Facts.Alertness, 1);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				_context.IncrementFact(Facts.Vulnerability, 1);
				_context.PlayersOutsideDetectionRange.Add(new NpcPlayerInfo
				{
					Player = initiatorPlayer,
					Time = UnityEngine.Time.time
				});
			}
		}

		private void OnExplosionSensation(ref Sensation info)
		{
			BasePlayer initiatorPlayer = info.InitiatorPlayer;
			if (!(initiatorPlayer != null) || !(initiatorPlayer != _context.Body))
			{
				return;
			}
			bool flag = false;
			foreach (NpcPlayerInfo item in _context.EnemyPlayersInRange)
			{
				if (RememberExplosion(ref info, item, initiatorPlayer))
				{
					_context.IncrementFact(Facts.Vulnerability, _context.IsFact(Facts.CanSeeEnemy) ? 1 : 2);
					_context.IncrementFact(Facts.Alertness, 1);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				_context.IncrementFact(Facts.Vulnerability, 1);
				_context.IncrementFact(Facts.Alertness, 1);
			}
		}

		private void RememberEntityOfInterest(ref Sensation info)
		{
			if (info.UsedEntity != null)
			{
				_context.Memory.RememberEntityOfInterest(_context.Body, info.UsedEntity, UnityEngine.Time.time, info.UsedEntity.name);
			}
		}

		private bool RememberGunshot(ref Sensation info, NpcPlayerInfo player, BasePlayer initiator)
		{
			if (player.Player == initiator)
			{
				float uncertainty = info.Radius * 0.05f;
				_context.Memory.RememberEnemyPlayer(_context.Body, ref player, UnityEngine.Time.time, uncertainty, "GUNSHOT!");
				return true;
			}
			return false;
		}

		private bool RememberExplosion(ref Sensation info, NpcPlayerInfo player, BasePlayer initiator)
		{
			return false;
		}

		private bool RememberThrownItem(ref Sensation info, NpcPlayerInfo player, BasePlayer initiator)
		{
			if (player.Player == initiator)
			{
				float uncertainty = info.Radius * 0.05f;
				_context.Memory.RememberEnemyPlayer(_context.Body, ref player, UnityEngine.Time.time, uncertainty, "THROW!");
				return true;
			}
			return false;
		}

		protected override bool CanTickSensor(float deltaTime, INpcSensor sensor)
		{
			if (_context.Memory.MarkedEnemies.Count == 0 && _context.Domain.ScientistContext.Memory.PrimaryKnownAnimal.Animal == null)
			{
				return deltaTime >= sensor.TickFrequency * 4f;
			}
			return deltaTime >= sensor.TickFrequency;
		}

		protected override void TickSensor(INpcSensor sensor, float deltaTime, float time)
		{
			sensor.Tick(_context.Body, deltaTime, time);
		}

		public override IAIContext GetContext(Guid aiId)
		{
			return _context;
		}

		public override void Initialize(BaseEntity body)
		{
			if (_aiClient == null || _aiClient.ai == null || _aiClient.ai.id != AINameMap.HTNDomainScientistJunkpile)
			{
				_aiClient = new HTNUtilityAiClient(AINameMap.HTNDomainScientistJunkpile, this);
			}
			if (_context == null || _context.Body != body)
			{
				_context = new ScientistJunkpileContext(body as HTNPlayer, this);
			}
			if (_navAgent == null)
			{
				_navAgent = GetComponent<NavMeshAgent>();
			}
			if ((bool)_navAgent)
			{
				_navAgent.updateRotation = false;
				_navAgent.updatePosition = false;
				_navAgent.speed = _context.Body.AiDefinition.Movement.DuckSpeed;
			}
			_spawnPosition = body.transform.position;
			_aiClient.Initialize();
			_context.Body.Resume();
			InitializeAgency();
		}

		public override void Dispose()
		{
			_aiClient?.Kill();
			RemoveAgency();
		}

		public override void ResetState()
		{
			base.ResetState();
			_lastFirearmUsageTime = 0f;
			_isFiring = false;
		}

		public override void Tick(float time)
		{
			base.Tick(time);
			TickFirearm(time);
			_context.Memory.Forget(_context.Body.AiDefinition.Memory.ForgetTime);
			if (_context.IsFact(Facts.IsIdle) || _context.IsFact(Facts.IsDucking) || (!_context.IsFact(Facts.HasEnemyTarget) && !_context.IsFact(Facts.NearbyAnimal) && !_context.IsFact(Facts.NearbyExplosives)))
			{
				_navAgent.speed = _context.Body.AiDefinition.Movement.DuckSpeed;
				return;
			}
			float num = Vector3.Dot(_context.Body.transform.forward, _navAgent.desiredVelocity.normalized);
			if (num <= 0.5f)
			{
				_navAgent.speed = _context.Body.AiDefinition.Movement.WalkSpeed;
				return;
			}
			float t = (num - 0.5f) * 2f;
			_navAgent.speed = Mathf.Lerp(_context.Body.AiDefinition.Movement.WalkSpeed, _context.Body.AiDefinition.Movement.RunSpeed, t);
		}

		public override void OnHurt(HitInfo info)
		{
			BasePlayer initiatorPlayer = info.InitiatorPlayer;
			if (initiatorPlayer != null && initiatorPlayer != _context.Body)
			{
				_context.Memory.MarkEnemy(initiatorPlayer);
				bool flag = false;
				foreach (NpcPlayerInfo item in _context.EnemyPlayersInRange)
				{
					if (RememberPlayerThatHurtUs(item, initiatorPlayer))
					{
						if (_context.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.Player == null)
						{
							_context.Memory.RememberPrimaryEnemyPlayer(initiatorPlayer);
						}
						_context.IncrementFact(Facts.Vulnerability, _context.IsFact(Facts.CanSeeEnemy) ? 1 : 10);
						_context.IncrementFact(Facts.Alertness, 1);
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					_context.IncrementFact(Facts.Vulnerability, 10);
					_context.IncrementFact(Facts.Alertness, 1);
					_context.PlayersOutsideDetectionRange.Add(new NpcPlayerInfo
					{
						Player = initiatorPlayer,
						Time = UnityEngine.Time.time
					});
				}
			}
			if (_context.ReservedCoverPoint != null && _context.IsFact(Facts.AtLocationCover))
			{
				_context.SetFact(Facts.MaintainCover, true, false);
				_context.ReservedCoverPoint.CoverIsCompromised(ConVar.AI.npc_cover_compromised_cooldown);
				_context.ReserveCoverPoint(null);
			}
		}

		private bool RememberPlayerThatHurtUs(NpcPlayerInfo player, BasePlayer initiator)
		{
			if (player.Player == initiator)
			{
				float num = 0f;
				NpcPlayerInfo info = player;
				BaseProjectile baseProjectile = initiator.GetHeldEntity() as BaseProjectile;
				if (baseProjectile != null)
				{
					num = baseProjectile.NoiseRadius * 0.1f;
					if (baseProjectile.IsSilenced())
					{
						num *= 3f;
					}
				}
				_context.Memory.RememberEnemyPlayer(_context.Body, ref info, UnityEngine.Time.time, num, "HURT!");
				return true;
			}
			return false;
		}
	}
}
