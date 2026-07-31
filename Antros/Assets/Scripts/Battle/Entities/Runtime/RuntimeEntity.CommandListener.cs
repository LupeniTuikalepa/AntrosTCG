using System.Linq;
using ATCG.Battle.CapacitySystem.Core.Status.Commands;
using ATCG.Battle.CapacitySystem.Status.Status;
using ATCG.Battle.Commands;
using ATCG.Battle.Commands.Directors;
using ATCG.Battle.Commands.Entities;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Grids.Runtime;
using ATCG.Battle.PassiveSystem.Core;
using ATCG.Battle.PassiveSystem.Runtimes;
using ATCG.Capacities.Data.Status;
using ATCG.HexGrids.Utility;
using ATCG.HexGrids;
using PrimeTween;
using UnityEngine;

namespace ATCG.Battle.Entities.Runtime
{
	public abstract partial class RuntimeEntity<T> :
		IEntityCommandDirector<DeathCommand>,
		IEntityCommandDirector<DamageCommand>,
		IEntityCommandDirector<MoveCommand>,
		IEntityCommandDirector<FallCommand>,
		IEntityCommandDirector<PushbackCommand>,
		IEntityCommandDirector<BasicAttackCommand>,
		IEntityCommandDirector<ApplyStatusCommand>,
		IEntityCommandDirector<TickStatusCommand>,
		IEntityCommandDirector<RemoveStatusCommand>,
		IEntityCommandDirector<ApplyPassiveCommand>,
		IEntityCommandDirector<TickPassiveCommand>,
		IEntityCommandDirector<RemovePassiveCommand>

	{
		public Entity Entity => Address.entity;
		public RuntimeBattleGrid RuntimeBattleGrid => Manager.RuntimeBattleGrid;


		async Awaitable ICommandDirector<DeathCommand>.Play(CommandDirectorState state, CommandContext context,
			DeathCommand command)
		{
			state.CompleteWindUp(this);

			Tween.CompleteAll(transform);
			await OnDeath(state, context, command);

			await Tween.Scale(transform, 0, .3f, Ease.InQuad);

			await Despawn();

			state.CompleteFollowThrough(this);
			gameObject.SetActive(false);

		}

		void ICommandDirector<DeathCommand>.OnEnd(in CommandDirectorState state, CommandContext context,
			DeathCommand command)
		{
			Destroy(gameObject);
		}

		async Awaitable ICommandDirector<DamageCommand>.Play(CommandDirectorState state, CommandContext context,
			DamageCommand command)
		{
			state.CompleteWindUp(this);
			await OnTakeDamage(state, context, command);

			Tween.CompleteAll(transform);

			await Tween.PunchScale(transform, -Vector3.one * .3f, .3f);

			state.CompleteFollowThrough(this);
		}

		async Awaitable ICommandDirector<BasicAttackCommand>.Play(CommandDirectorState state, CommandContext context,
			BasicAttackCommand command)
		{
			state.CompleteWindUp(this);

			var infos = command.GetInfos();

			if (Manager.TryGetRuntimeEntity(infos.victim, out var victimRuntimeEntity))
			{
				var victimTransform = victimRuntimeEntity.transform;

				HexOperations.ComputeQuaternion(
					victimTransform.position,
					transform.position,
					out var victimTargetRotation);

				HexOperations.ComputeQuaternion(
					transform.position,
					victimTransform.position,
					out var sourceTargetRotation);

				await Tween.Rotation(transform, sourceTargetRotation, .15f, Ease.InOutQuint);
				await Tween.Rotation(victimTransform, victimTargetRotation, .15f, Ease.InOutQuint);
			}

			state.CompleteFollowThrough(this);
		}


		protected virtual async Awaitable OnDeath(CommandDirectorState state, CommandContext context,
			DeathCommand command)
			=> await Awaitable.MainThreadAsync();

		protected virtual async Awaitable OnTakeDamage(CommandDirectorState state, CommandContext context,
			DamageCommand command)
			=> await Awaitable.MainThreadAsync();

		async Awaitable ICommandDirector<MoveCommand>.Play(CommandDirectorState state, CommandContext context,
			MoveCommand command)
		{
			state.CompleteWindUp(this);

			var infos = command.GetInfos();

			var destination = infos.to;

			var position = RuntimeBattleGrid.GetPositionAt(destination);

			HexOperations.ComputeQuaternion(
				transform.position,
				position,
				out var targetRotation);

			await Tween.Rotation(transform, targetRotation, 0.15f, Ease.InOutQuint);
			await Tween.Position(transform, position, .15f, Ease.OutCirc);


			state.CompleteFollowThrough(this);
		}
		async Awaitable ICommandDirector<PushbackCommand>.Play(CommandDirectorState state, CommandContext context, PushbackCommand command)
		{
			state.CompleteWindUp(this);

			var infos = command.GetInfos();
			var destination = RuntimeBattleGrid.GetPositionAt(infos.to);

			await Tween.Position(transform, destination, .15f, Ease.OutCirc);

			state.CompleteFollowThrough(this);
		}

		async Awaitable ICommandDirector<FallCommand>.Play(CommandDirectorState state, CommandContext context,
			FallCommand command)
		{
			float targetY = transform.position.y - 10f;

			await Tween.PositionY(transform, targetY, duration: 0.8f, ease: Ease.InQuad);

			await Tween.Scale(transform, Vector3.zero, duration: 0.8f, ease: Ease.InQuad);
			state.CompleteAll(this);

		}

		async Awaitable ICommandDirector<ApplyStatusCommand>.Play(CommandDirectorState state, CommandContext context, ApplyStatusCommand command)
		{
			await Awaitable.MainThreadAsync();
			state.CompleteAll(this);

			var infos = command.GetInfos();
			var runtimeContext = new RuntimeStatusContext(infos.data, Entity, this);
			var statusData = runtimeContext.statusData;
			if (statusDatas.ContainsKey(statusData))
				return;

			if (!statusData.RuntimeStatus.TryGetComponent(out RuntimeStatus prefabStatus))
			{
				Debug.LogWarning($"[RuntimeEntity] No RuntimeStatus found");
				return;
			}

			var runtimeStatus = Instantiate(prefabStatus, statusRoot);
			runtimeStatus.transform.localScale = Vector3.one;
			runtimeStatus.transform.localPosition = Vector3.zero;
			runtimeStatus.transform.localRotation = Quaternion.identity;

			statusDatas.Add(statusData, runtimeStatus);
			runtimeStatus.Apply(runtimeContext);
		}

		async Awaitable ICommandDirector<TickStatusCommand>.Play(CommandDirectorState state, CommandContext context, TickStatusCommand command)
		{
			await Awaitable.MainThreadAsync();
			state.CompleteAll(this);

			var infos = command.GetInfos();
			RuntimeStatusContext runtimeContext = new RuntimeStatusContext(infos.data, Entity, this);
			StatusData statusData = runtimeContext.statusData;
			if (statusDatas.TryGetValue(statusData, out RuntimeStatus tickStatus))
				tickStatus.Tick(runtimeContext);
		}

		async Awaitable ICommandDirector<RemoveStatusCommand>.Play(CommandDirectorState state, CommandContext context, RemoveStatusCommand command)
		{
			// Temporary diagnostics — please leave these in for the next test so we get
			// a real read on what's happening (the previous pair got removed before the
			// last test ran, so that test told us nothing new).
			// Debug.Log($"[RuntimeEntity] StatusRemoveCommand.Play fired on entity {Entity.id}.");

			await Awaitable.MainThreadAsync();
			state.CompleteAll(this);

			var infos = command.GetInfos();
			var runtimeContext = new RuntimeStatusContext(infos.data, Entity, this);
			var statusData = runtimeContext.statusData;
			if (!statusDatas.TryGetValue(statusData, out RuntimeStatus removeStatus))
			{
				string tracked = string.Join(", ", statusDatas.Keys.Select(k => k != null ? k.name : "null"));
				Debug.LogWarning($"[RuntimeEntity] StatusRemoveCommand.Play: no tracked RuntimeStatus for " +
				                 $"'{(statusData != null ? statusData.name : "null")}' on entity {Entity.id} — VFX won't be cleaned up. " +
				                 $"Currently tracked: [{tracked}]");
				return;
			}

			// Debug.Log($"[RuntimeEntity] Destroying RuntimeStatus '{removeStatus.name}' for '{statusData.name}' on entity {Entity.id}.");
			removeStatus.Remove(runtimeContext);
			Destroy(removeStatus.gameObject);
			statusDatas.Remove(statusData);
		}

		async Awaitable ICommandDirector<ApplyPassiveCommand>.Play(CommandDirectorState state, CommandContext context, ApplyPassiveCommand command)
		{
			await Awaitable.MainThreadAsync(); 
			state.CompleteAll(this);

			var infos = command.GetInfos();
			var passiveData = infos.data;
			
			var passiveContext = new RuntimePassiveContext(passiveData);
			if (passives.ContainsKey(passiveData))
				return;

			if (!passiveData.RuntimePassive.TryGetComponent(out RuntimePassive passive))
			{
				Debug.LogWarning($"[RuntimeEntity] No RuntimeStatus found");
				return;
			}

			var runtimePassive = Instantiate(passive, PassiveRoot);
			runtimePassive.transform.localScale = Vector3.one;
			runtimePassive.transform.localPosition = Vector3.zero;
			runtimePassive.transform.localRotation = Quaternion.identity;

			passives.Add(passiveData, runtimePassive);
			runtimePassive.Apply(passiveContext);
		}

		async Awaitable ICommandDirector<TickPassiveCommand>.Play(CommandDirectorState state, CommandContext context, TickPassiveCommand command)
		{
			await Awaitable.MainThreadAsync(); 
			state.CompleteAll(this);
			
			var infos = command.GetInfos();
			var passiveData = infos.data;
			
			var passiveContext = new RuntimePassiveContext(infos.data);
			if(passives.TryGetValue(passiveData, out var passive))
				passive.Tick(passiveContext);
			
		}

		async Awaitable ICommandDirector<RemovePassiveCommand>.Play(CommandDirectorState state, CommandContext context, RemovePassiveCommand command)
		{
			await Awaitable.MainThreadAsync(); 
			state.CompleteAll(this);
			
			var infos = command.GetInfos();
			var passiveData = infos.data;


			var passiveContext = new RuntimePassiveContext(infos.data);
			if (passives.TryGetValue(passiveData, out var passive))
			{
				passive.Remove(passiveContext);
				passives.Remove(passiveData);
			}
		}

		public async Awaitable LookAtCoord(HexCoordinates coordinates, float duration = 0.3f)
		{
			Vector3 target = RuntimeBattleGrid.RuntimeHexGrid.GetPositionAt(coordinates);
			Vector3 from = transform.position;
			Vector3 to = (target - from).normalized;
			to.y = 0;

			Quaternion rotation = Quaternion.LookRotation(to, Vector3.up);

			Tween.StopAll(transform);
			await Tween.Rotation(transform, rotation, duration, Ease.OutQuad);
		}
	}
}
