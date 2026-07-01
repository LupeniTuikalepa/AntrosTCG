using ATCG.Battle.CapacitySystem.Core.Cutscenes;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace ATCG.Battle.CapacitySystem.Core.Cutscenes.Timeline
{
    /// <summary>
    /// Track holding QTE clips. Bound to the cutscene (IQteWindowHost) via the
    /// track binding, so every clip's behaviour receives the cutscene as playerData
    /// and can report its window. One QTE per clip; clip length = QTE duration.
    /// </summary>
    [TrackColor(0.9f, 0.6f, 0.1f)]
    [TrackClipType(typeof(QtePlayableAsset))]
    [TrackBindingType(typeof(CapacityCutscene))]
    public class QteTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            // No mixing needed; clips report individually. A simple passthrough.
            return base.CreateTrackMixer(graph, go, inputCount);
        }
    }
}
