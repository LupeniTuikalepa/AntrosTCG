using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace ATCG.Battle.CapacitySystem.Core.Cutscenes.Loops
{
    /// <summary>
    /// Authorable loop clip. Points to its loop component via an ExposedReference,
    /// so several loops can live on one track with no track binding. Its span is the
    /// segment replayed once per turn; it carries no count.
    /// </summary>
    public class LoopClip : PlayableAsset, ITimelineClipAsset
    {
        public ExposedReference<LoopCutsceneComponentBase> component;

        public double clipStart;
        public double clipEnd;

        public ClipCaps clipCaps => ClipCaps.None;

        // Resolves the component from the director's exposed table and hands it to
        // the behaviour, so the track needs no binding.
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<LoopClipBehaviour>.Create(graph);
            LoopClipBehaviour behaviour = playable.GetBehaviour();
            behaviour.clipStart = clipStart;
            behaviour.clipEnd = clipEnd;
            behaviour.host = component.Resolve(graph.GetResolver());
            return playable;
        }
    }
}