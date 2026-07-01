using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.Core.Players;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Commands.Players;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components.Status.Signals;
using ATCG.Battle.Entities.Runtime.Grid;
using ATCG.Battle.Entities.Runtime.Status;
using ATCG.Battle.Grids.Runtime;
using ATCG.HexGrids.Utility;
using PrimeTween;
using UnityEngine;

namespace ATCG.Battle.Entities.Runtime
{
	public abstract partial class RuntimeEntity<T> :
		IEntityCommandListener<DeathCommand>,
		IEntityCommandListener<DamageCommand>,
		IEntityCommandListener<MoveCommand>,
		IEntityCommandListener<FallCommand>,
		IEntityCommandListener<BasicAttackCommand>,
		IEntityCommandListener<StatusSignal>

	{
	public Entity Entity => Address.entity;
	public RuntimeBattleGrid RuntimeBattleGrid => Manager.RuntimeBattleGrid;


	async Awaitable ICommandListener<DeathCommand>.Play(CommandListenerState state, CommandContext context,
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

	void ICommandListener<DeathCommand>.OnEnd(in CommandListenerState state, CommandContext context,
		DeathCommand command)
	{
		Destroy(gameObject);
	}

	async Awaitable ICommandListener<DamageCommand>.Play(CommandListenerState state, CommandContext context,
		DamageCommand command)
	{
		state.CompleteWindUp(this);
		await OnTakeDamage(state, context, command);

		Tween.CompleteAll(transform);

		await Tween.PunchScale(transform, -Vector3.one * .3f, .3f);

		state.CompleteFollowThrough(this);
	}

	async Awaitable ICommandListener<BasicAttackCommand>.Play(CommandListenerState state, CommandContext context,
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


	protected virtual async Awaitable OnDeath(CommandListenerState state, CommandContext context,
		DeathCommand command)
		=> await Awaitable.MainThreadAsync();

	protected virtual async Awaitable OnTakeDamage(CommandListenerState state, CommandContext context,
		DamageCommand command)
		=> await Awaitable.MainThreadAsync();

	public async Awaitable Play(CommandListenerState state, CommandContext context, MoveCommand command)
	{
		state.CompleteWindUp(this);

		var infos = command.GetInfos();

		var destination = infos.to;

		if (RuntimeBattleGrid.TryGetBattleCellAt(destination, out RuntimeBattleCell cell))
		{
			HexOperations.ComputeQuaternion(
				transform.position,
				cell.transform.position,
				out var targetRotation);

			await Tween.Rotation(transform, targetRotation, 0.15f, Ease.InOutQuint);
			await Tween.Position(transform, cell.transform.position, .15f, Ease.OutCirc);
		}

		state.CompleteFollowThrough(this);
	}

	public async Awaitable Play(CommandListenerState state, CommandContext context, FallCommand command)
	{
		float targetY = transform.position.y - 10f;

		await Tween.PositionY(transform, targetY, duration: 0.8f, ease: Ease.InQuad);

		await Tween.Scale(transform, Vector3.zero, duration: 0.8f, ease: Ease.InQuad);
		state.CompleteAll(this);

	}

	async Awaitable ICommandListener<StatusSignal>.Play(CommandListenerState state, CommandContext context, StatusSignal command)
	{
		await Awaitable.MainThreadAsync();
		state.CompleteAll(this);
            
		var infos = command.GetInfos();
		var runtimeContext = new RuntimeStatusContext(infos.data, Entity, this);
            
		switch (infos.action)
		{
			case StatusAction.Apply:
				ApplyRuntimeStatus(runtimeContext);
				return;
                
			case StatusAction.Remove:
				RemoveRuntimeStatus(runtimeContext);
				return;
                
			case StatusAction.Tick:
				TickRuntimeStatus(runtimeContext);
				return;
		}
	}
	private void TickRuntimeStatus(RuntimeStatusContext runtimeContext)
	{
		var statusData = runtimeContext.statusData;
		if (statusDatas.TryGetValue(statusData, out RuntimeStatus tickStatus))
			tickStatus.Tick(runtimeContext);
	}

	private void RemoveRuntimeStatus(RuntimeStatusContext runtimeContext)
	{
		var statusData = runtimeContext.statusData;
		if (!statusDatas.TryGetValue(statusData, out RuntimeStatus removeStatus)) 
			return;
            
		removeStatus.Remove();
		Destroy(removeStatus.gameObject);
		statusDatas.Remove(statusData);
	}

	private void ApplyRuntimeStatus(RuntimeStatusContext runtimeContext)
	{
		var statusData = runtimeContext.statusData;
		if (statusDatas.ContainsKey(statusData))
			return;

		if (!statusData.StatusVFX.TryGetComponent(out RuntimeStatus prefabStatus))
		{
			Debug.LogWarning($"[RuntimeStatusController] No RuntimeStatus found");
			return;
		}
            
		RuntimeStatus runtimeStatus = Instantiate(prefabStatus, statusRoot);
		statusDatas.Add(statusData, runtimeStatus);
		runtimeStatus.Apply(statusData);
	}
	}
}