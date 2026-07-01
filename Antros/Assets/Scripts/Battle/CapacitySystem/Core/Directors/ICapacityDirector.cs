using System.Threading;
using ATCG.Battle.Players.Local.Runtime;
using UnityEngine;

namespace ATCG.Battle.CapacitySystem.Core.Directors
{
    /// <summary>
    /// One per screen. Owns a cutscene, plays it through, and relays its step
    /// markers to the phase (which barriers them across screens, then runs the
    /// step). Decides this screen's role for QTEs (owner emits, others observe).
    /// Never mutates the ECS, never runs steps.
    /// </summary>
    public interface ICapacityDirector
    {
        /// <summary>Play the cutscene from start to finish. Returns when the timeline ends.</summary>
        public Awaitable Play(CastCapacityPhase phase, RuntimeLocalBattlePlayer screenPlayer, CancellationToken token);

        /// <summary>Stop and release.</summary>
        Awaitable Stop(CancellationToken token);

        void Dispose();
    }
}