using UnityEngine;
using UnityEngine.Playables;

namespace ATCG.Battle.CapacitySystem.Core.Cutscenes.QTEs.Timeline
{
    /// <summary>
    /// The authorable QTE clip. Its LENGTH on the track is the QTE duration. Holds
    /// only thin presentation data; the behaviour bridges to the cutscene at runtime.
    /// </summary>
    public class QtePlayableAsset : PlayableAsset
    {
        public QteClipData data;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<QtePlayableBehaviour>.Create(graph);
            playable.GetBehaviour().data = data;
            return playable;
        }
    }
}