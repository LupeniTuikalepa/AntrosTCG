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
    /// when a track with its exact name+type already exists. Matching is by name
    /// (intentionally): multiple tracks can share a type while only some are auto-bound.
    /// Debug binding reads the DebugCutsceneRig's channel table (no runtime resolveDebug).
    /// </summary>
    public static class CapacityTimelineTrackBinder
    {
        public static bool HasTrack(TimelineAsset timeline, AutoBindChannel channel)
        {
            return timeline != null && timeline.GetOutputTracks()
                .Any(t => t.name == channel.trackName && t.GetType() == channel.trackType);
        }

        public static void AddTrack(TimelineAsset timeline, AutoBindChannel channel, PlayableDirector director, DebugCutsceneRig rig)
        {
            if (HasTrack(timeline, channel))
                return;

            TrackAsset track = timeline.CreateTrack(channel.trackType, null, channel.trackName);
            BindTrack(director, track, channel, rig);

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

        // Binds one track to the rig's reference for its channel, if present.
        public static void BindTrack(PlayableDirector director, TrackAsset track, AutoBindChannel channel, DebugCutsceneRig rig)
        {
            if (director == null || track == null)
                return;

            if (rig != null && rig.TryGet(channel.trackName, out Object reference))
                director.SetGenericBinding(track, reference);
            else
                Debug.LogWarning(
                    $"[CapacityTimelineEditor] No rig reference for channel '{channel.trackName}'. " +
                    $"Fill it on the DebugCutsceneRig (Populate from CutsceneChannels).");
        }
    }
}