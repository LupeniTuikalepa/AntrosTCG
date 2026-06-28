using System.Threading;
using UnityEngine;
using UnityEngine.Playables;

namespace ATCG.Battle.CapacitySystem.Core.Cutscenes
{
    /// <summary>
    /// Default cutscene component placed on a capacity prefab. Binds the
    /// PlayableDirector and (later) the custom QTE track. This is the ONLY place
    /// that touches Unity Timeline; the director above it stays engine-agnostic.
    ///
    /// The Timeline hookup (play/pause via graph speed, the custom QTE track that
    /// opens the window and signals "marker reached") is left as the next step —
    /// the methods mark exactly where it plugs in. Stubs complete immediately /
    /// return neutral so the whole cast chain runs end-to-end first.
    /// </summary>
    [AddComponentMenu("ATCG/Gameplay/Capacities/Capacity Cutscene")]
    public class CapacityCutscene : MonoBehaviour, ICapacityCutscene
    {
        [SerializeField]
        private PlayableDirector playableDirector;

        public async Awaitable Begin(CancellationToken token)
        {
            // TODO: playableDirector.Play(); hold at speed 0 until first advance.
            await Awaitable.MainThreadAsync();
        }

        public async Awaitable<float> PlayNextQteWindow(CancellationToken token)
        {
            // TODO: resume timeline to the next QTE window, open the custom track,
            // read input, return [0,1]. Stub: neutral result so chain runs.
            await Awaitable.MainThreadAsync();
            return 1f;
        }

        public async Awaitable AdvanceToNextConsumption(CancellationToken token)
        {
            // TODO: resume timeline to the next consumption marker, pause there.
            await Awaitable.MainThreadAsync();
        }

        public async Awaitable End(CancellationToken token)
        {
            // TODO: stop / release the timeline.
            await Awaitable.MainThreadAsync();
        }
    }
}
