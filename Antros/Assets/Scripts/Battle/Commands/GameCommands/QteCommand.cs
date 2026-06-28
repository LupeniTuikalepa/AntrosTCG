using System;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.Infos;
using ATCG.Battle.Commands.Players;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.GameModes;
using ATCG.Battle.Players;
using UnityEngine;

namespace ATCG.Battle.Commands.GameCommands.Capacities
{
    /// <summary>
    /// A QTE result, expressed as a command so it travels through the SAME
    /// pipeline as every other command (serialized, ordered, routed to both
    /// screens). The caster's screen emits it from real input; the other screen
    /// receives and applies it. Process does NOT mutate the ECS — the only
    /// effect is to push the [0,1] result into the casting phase's QTE stack,
    /// which a later step flushes. Carries the player so a listener can tell
    /// whether it owns this QTE (its screen produced it) or merely observes it.
    /// </summary>
    [Serializable]
    public class QteCommand : PlayerCommand<NoInfos>
    {
        [field: SerializeField]
        public float Result { get; private set; }

        public QteCommand(IBattlePlayer caster, float result) : base(caster)
        {
            Result = result;
        }

        protected override void Process(in CommandContext context)
        {
            // No ECS mutation. The result is consumed by the casting phase,
            // which is registered as an ICommandListener<QteCommand> and pushes
            // Result onto its QTE stack on OnBegin. Keeping Process empty makes
            // it deterministic and replay-safe: applying the same QteCommand on
            // the other screen reproduces the same stacked value.
        }
    }
}
