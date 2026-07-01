// Assets/Scripts/Core/Cutscenes/OrbitalRadiusClip.cs
using UnityEngine;
using UnityEngine.Playables;

namespace ATCG.Core.Cutscenes
{
    public class OrbitalRadiusClip : PlayableAsset
    {
        public float fromRadius = 10f;
        public float toRadius = 4f;

        [Tooltip("Easing optionnel. Linéaire par défaut.")]
        public AnimationCurve ease = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<OrbitalRadiusBehaviour>.Create(graph);
            var b = playable.GetBehaviour();
            b.fromRadius = fromRadius;
            b.toRadius = toRadius;
            b.ease = ease;
            return playable;
        }
    }
}