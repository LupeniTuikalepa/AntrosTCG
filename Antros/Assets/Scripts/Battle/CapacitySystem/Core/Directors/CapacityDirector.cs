using System.Threading;
using ATCG.Battle.CapacitySystem.Core;
using ATCG.Battle.CapacitySystem.Core.Cutscenes;
using ATCG.Battle.CapacitySystem.Core.Directors;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.GameCommands.Capacities;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Players.Local;
using ATCG.Battle.Players.Local.Runtime;
using UnityEngine;

namespace ATCG.Battle.CapacitySystem.Directors
{
    /// <summary>
    /// One per screen, created by the phase. Routing rule: this screen OWNS the
    /// cast iff its logical player == the casting player (casterPlayerId). The
    /// owner plays the QTE and emits a QteCommand from real input; the others
    /// advance their cutscene and receive the QteCommand through the pipeline.
    ///
    /// Holds and drives an ICapacityCutscene (presentation). The cutscene is
    /// spawned from the capacity data prefab; this director is engine-agnostic.
    /// Works with OR without a caster entity (spell cards): routing is on the
    /// player id, not on an EntityAddress.
    /// </summary>
    public class CapacityDirector : ICapacityDirector
    {
        private readonly RuntimeLocalBattlePlayer runtimePlayer;
        private readonly BattleID casterPlayerId;
        private readonly ICapacityCutscene cutscene;

        private bool IsOwner =>
            runtimePlayer.BattlePlayer != null
            && runtimePlayer.BattlePlayer.ID == casterPlayerId;

        public CapacityDirector(
            RuntimeLocalBattlePlayer runtimePlayer,
            BattleID casterPlayerId,
            ICapacityCutscene cutscene)
        {
            this.runtimePlayer = runtimePlayer;
            this.casterPlayerId = casterPlayerId;
            this.cutscene = cutscene;
        }

        public async Awaitable Begin(CastCapacityPhase phase, CancellationToken token)
        {
            if (cutscene != null)
                await cutscene.Begin(token);
            else
                await Awaitable.MainThreadAsync();
        }

        public async Awaitable AdvanceToNextStep(CancellationToken token)
        {
            if (IsOwner)
            {
                // Play the QTE window locally, get the [0,1], and broadcast it as
                // a command so BOTH screens stack the same value.
                float result = cutscene != null ? await cutscene.PlayNextQteWindow(token) : 1f;

                LocalBattlePlayer caster = runtimePlayer.BattlePlayer;
                new QteCommand(caster, result).Run(runtimePlayer.BattlePlayer.BattlePhase);
            }
            else
            {
                // Observe: advance to the consumption marker, pausing there until
                // the QteCommand has been received (the phase stacks it on OnBegin).
                if (cutscene != null)
                    await cutscene.AdvanceToNextConsumption(token);
            }
        }

        public async Awaitable End(CancellationToken token)
        {
            if (cutscene != null)
                await cutscene.End(token);
            else
                await Awaitable.MainThreadAsync();
        }
    }
}
