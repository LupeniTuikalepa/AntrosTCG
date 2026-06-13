using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.Core.Players;
using ATCG.Battle.Commands.EntityCommands;
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

    }
}