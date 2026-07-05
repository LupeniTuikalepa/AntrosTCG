// Temporary diagnostic — drop under an Editor folder, select the CutsceneDirector
// (or any PlayableDirector) in a scene, then run ATCG/Debug/Dump Timeline Tracks.
// Prints each track's exact name + full type so we can see why auto-bind rejects it.

using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace ATCG.Editor.Debugging
{
    public static class DumpTimelineTracks
    {
        [MenuItem("ATCG/Debug/Dump Timeline Tracks")]
        private static void Dump()
        {
            PlayableDirector director = Selection.activeGameObject != null
                ? Selection.activeGameObject.GetComponentInChildren<PlayableDirector>()
                : null;

            if (director == null)
            {
                Debug.LogWarning("Select a GameObject with a PlayableDirector first.");
                return;
            }

            if (director.playableAsset is not TimelineAsset timeline)
            {
                Debug.LogWarning($"Director '{director.name}' has no TimelineAsset (playableAsset = {director.playableAsset}).");
                return;
            }

            Debug.Log($"=== Timeline '{timeline.name}' — {timeline.outputTrackCount} output track(s) ===");
            foreach (TrackAsset track in timeline.GetOutputTracks())
            {
                object binding = director.GetGenericBinding(track);
                Debug.Log(
                    $"name='{track.name}' | type={track.GetType().FullName} | " +
                    $"muted={track.muted} | binding={(binding != null ? binding.GetType().Name : "null")}");
            }
        }
    }
}