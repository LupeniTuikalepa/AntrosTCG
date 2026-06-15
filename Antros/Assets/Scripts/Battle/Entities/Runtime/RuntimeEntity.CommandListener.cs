using System.Threading.Tasks;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.Core.Players;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Commands.Players;
using ATCG.Battle.Players.Local.Phases;
using ATCG.Metrics;
using Helteix.Tools;
using Helteix.Tools.Phases;
using PrimeTween;
using UnityEngine;

namespace ATCG.Battle.Entities.Runtime
{
    public abstract partial class RuntimeEntity<T> :
        IEntityCommandPlayer<DeathCommand>,
        IEntityCommandPlayer<DamageCommand>
    {
        public Entity Entity => Address.entity;



        async Awaitable ICommandPlayer<DeathCommand>.Play(CommandPlayerState state, CommandContext context, DeathCommand command)
        {
            Manager.UnregisterRuntimeEntity(this);

            state.CompleteWindUp(this);

            Tween.CompleteAll(transform);
            await OnDeath(state, context, command);

            await Tween.Scale(transform, 0, .3f, Ease.InQuad);
            state.CompleteFollowThrough(this);
            gameObject.SetActive(false);

        }

        void ICommandPlayer<DeathCommand>.OnEnd(in CommandPlayerState state, CommandContext context, DeathCommand command)
        {
            Destroy(gameObject);
        }

        async Awaitable ICommandPlayer<DamageCommand>.Play(CommandPlayerState state, CommandContext context, DamageCommand command)
        {
            state.CompleteWindUp(this);
            await OnTakeDamage(state, context, command);

            Tween.CompleteAll(transform);
            await Tween.PunchScale(transform, -Vector3.one * .3f, .3f);

            state.CompleteFollowThrough(this);
        }


        protected virtual async Awaitable OnDeath(CommandPlayerState state, CommandContext context, DeathCommand command)
            => await Awaitable.MainThreadAsync();
        protected virtual async Awaitable OnTakeDamage(CommandPlayerState state, CommandContext context, DamageCommand command)
            => await Awaitable.MainThreadAsync();
    }
}