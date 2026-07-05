using System.Linq;
using ATCG.Battle.CapacitySystem.Core.Cutscenes.Tracks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace ATCG.Editor.Tools.CapacityEditor
{
    /// <summary>
    /// Adds/removes the auto-bindable channels (CutsceneChannels.All) on a capacity's
    /// timeline. The checklist always lists the POSSIBLE channels; a channel is ticked
    /// when a track with its exact name+type already exists on the timeline. Matching
    /// is by name (intentionally): multiple tracks can share a type while only some are
    /// auto-bound. Binding to the DebugCutsceneRig only happens when the editing scene
    /// (with the rig) is open; otherwise the track is created and a warning is logged.
    /// </summary>
    public static class CapacityTimelineTrackBinder
    {
        public static bool HasTrack(TimelineAsset timeline, AutoBindChannel channel)
        {
            return timeline != null && timeline.GetOutputTracks()
                .Any(t => t.name == channel.trackName && t.GetType() == channel.trackType);
        }

        // director may be null (no stage in scene); binding is then skipped with a warning.
        public static void AddTrack(TimelineAsset timeline, AutoBindChannel channel, PlayableDirector director, DebugCutsceneRig rig)
        {
            if (HasTrack(timeline, channel))
                return;

            TrackAsset track = timeline.CreateTrack(channel.trackType, null, channel.trackName);

            Object binding = rig != null ? channel.resolveDebug?.Invoke(rig) : null;
            if (binding != null && director != null)
                director.SetGenericBinding(track, binding);
            else
                Debug.LogWarning(
                    $"[CapacityTimelineEditor] Track '{channel.trackName}' created but not bound " +
                    $"(open the editing scene with a DebugCutsceneRig to bind it).");

            EditorUtility.SetDirty(timeline);
            AssetDatabase.SaveAssetIfDirty(timeline);
        }

        public static void RemoveTrack(TimelineAsset timeline, AutoBindChannel channel)
        {
            TrackAsset track = timeline.GetOutputTracks()
                .FirstOrDefault(t => t.name == channel.trackName && t.GetType() == channel.trackType);

            if (track == null)
                return;

            if (track.GetClips().Any() &&
                !EditorUtility.DisplayDialog(
                    "Remove Track",
                    $"'{channel.trackName}' has clips on it. Remove the track and its clips?",
                    "Remove", "Cancel"))
                return;

            timeline.DeleteTrack(track);
            EditorUtility.SetDirty(timeline);
            AssetDatabase.SaveAssetIfDirty(timeline);
        }
    }
}