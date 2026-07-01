// Assets/Scripts/Core/Cutscenes/GoToClip.cs
using UnityEngine;
using UnityEngine.Playables;

namespace ATCG.Core.Cutscenes
{
    public class GoToClip : PlayableAsset
    {
        [Tooltip("Transforms cibles (résolus à l'exécution). Laisser 'from' vide pour partir de la position actuelle de la cible bindée.")]
        public ExposedReference<Transform> from;
        public ExposedReference<Transform> to;

        [Tooltip("Espace local au parent (true) ou monde (false).")]
        public bool useLocalSpace = false;

        [Tooltip("Easing. Linéaire (0,0)->(1,1) par défaut.")]
        public AnimationCurve ease = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<GoToBehaviour>.Create(graph);
            var b = playable.GetBehaviour();

            // Résolution des ExposedReference via le resolver du PlayableDirector.
            b.from = from.Resolve(graph.GetResolver());
            b.to = to.Resolve(graph.GetResolver());
            b.useLocalSpace = useLocalSpace;
            b.ease = ease;
            return playable;
        }
    }
}