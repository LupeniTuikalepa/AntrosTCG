using UnityEngine;
using UnityEngine.Playables;

namespace ATCG.Core.Cutscenes
{
    public class OrbitalSpeedRotateClip : PlayableAsset
    {
        [Tooltip("Degrés par seconde.")]
        public float degreesPerSecond = 90f;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<OrbitalSpeedRotateBehaviour>.Create(graph);
            playable.GetBehaviour().degreesPerSecond = degreesPerSecond;
            return playable;
        }
    }
}