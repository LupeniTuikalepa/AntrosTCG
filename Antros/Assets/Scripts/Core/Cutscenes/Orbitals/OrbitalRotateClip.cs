// Assets/Scripts/Core/Cutscenes/OrbitalRotateClip.cs
using UnityEngine;
using UnityEngine.Playables;

namespace ATCG.Core.Cutscenes
{
    public class OrbitalRotateClip : PlayableAsset
    {
        public float from = 90f;
        public float to = 90f;
        public AnimationCurve ease= AnimationCurve.Linear(0f, 0f, 1f, 1f);

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<OrbitalRotateBehaviour>.Create(graph);
            OrbitalRotateBehaviour orbitalRotateBehaviour = playable.GetBehaviour();
            orbitalRotateBehaviour.from = from;
            orbitalRotateBehaviour.to = to;
            orbitalRotateBehaviour.ease = ease;

            return playable;
        }
    }
}