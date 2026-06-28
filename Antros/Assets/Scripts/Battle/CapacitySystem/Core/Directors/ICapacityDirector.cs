using System.Threading;
using UnityEngine;

namespace ATCG.Battle.CapacitySystem.Core.Directors
{
    /// <summary>
    /// Drives the presentation of a capacity cast for ONE screen and decides
    /// this screen's role (emit QTE results vs. wait for them). Owns a
    /// CapacityCutscene (spawned from the capacity data) and paces it. Never
    /// mutates the ECS and never decides step order — the phase owns those.
    /// </summary>
    public interface ICapacityDirector
    {
        Awaitable Begin(CastCapacityPhase phase, CancellationToken token);

        /// <summary>
        /// Advance presentation up to the next step boundary. Owner screen plays
        /// the QTE window and emits a QteCommand; other screens advance their
        /// cutscene to the consumption marker (waiting there if needed).
        /// </summary>
        Awaitable AdvanceToNextStep(CancellationToken token);

        Awaitable End(CancellationToken token);
    }
}
