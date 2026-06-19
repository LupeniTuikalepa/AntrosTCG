using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.Core.Players;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Commands.Players;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Runtime.Grid;
using ATCG.Battle.Grids.Runtime;
using PrimeTween;
using UnityEngine;

namespace ATCG.Battle.Entities.Runtime
{
	public abstract partial class RuntimeEntity<T> :
		IEntityCommandListener<DeathCommand>,
		IEntityCommandListener<DamageCommand>,
		IEntityCommandListener<MoveCommand>, IEntityCommandListener<FallCommand>


	{
		public Entity Entity => Address.entity;
		public RuntimeBattleGrid RuntimeBattleGrid => Manager.RuntimeBattleGrid;


		async Awaitable ICommandListener<DeathCommand>.Play(CommandListenerState state, CommandContext context,
			DeathCommand command)
		{
			Manager.UnregisterRuntimeEntity(this);

			state.CompleteWindUp(this);

			Tween.CompleteAll(transform);
			await OnDeath(state, context, command);

			await Tween.Scale(transform, 0, .3f, Ease.InQuad);
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

		private bool Filter(BattleCellAspect aspect)
		{
			if (aspect.CanBeMovedOn())
				return true;

			foreach (var memberRef in aspect.GetMembers())
			{
				if (memberRef.Entity == Entity)
					return true;
			}

			return false;
		}
	}
}