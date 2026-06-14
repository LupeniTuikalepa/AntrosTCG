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
    public abstract partial class RuntimeEntity<T> : IEntityCommandPlayer<DeathCommand>
    {
        public Entity Entity => Address.entity;

        async Awaitable ICommandPlayer<DeathCommand>.Play(CommandPlayerState state, CommandContext context, DeathCommand command)
        {
            Manager.UnregisterRuntimeEntity(this);
            
            state.CompleteWindUp(this);
            await Tween.Scale(transform, 0, .3f, Ease.InQuad);
            state.CompleteFollowThrough(this);
            
            gameObject.SetActive(false);
        }

        void ICommandPlayer<DeathCommand>.OnEnd(in CommandPlayerState state, CommandContext context, DeathCommand command)
        {
            Destroy(gameObject);
        }
    }
}