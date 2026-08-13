using ATCG.Cutscenes;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace ATCG.Editor.Tools.CutsceneEditor
{
    /// <summary>
    /// Reconnects a director's auto-bindable tracks to the DebugCutsceneRig's channel table.
    /// Serialized bindings on the director prefab point at objects that don't exist inside the
    /// isolated stage, so on stage open every auto-bindable track is rebound by channel name against
    /// the rig actually present in the stage. Logs a precise reason for any track it can't bind.
    /// </summary>
    public static class CutsceneAutoBinder
    {
        public static void RebindAll(PlayableDirector director, DebugCutsceneRig rig)
        {
            if (director == null)
            {
                Debug.LogWarning("[CutsceneEditor] Rebind skipped: no PlayableDirector in the stage.");
                return;
            }

            if (director.playableAsset is not TimelineAsset timeline)
            {
                Debug.LogWarning("[CutsceneEditor] Rebind skipped: director has no TimelineAsset " +
                                 $"(playableAsset = {director.playableAsset}).");
                return;
            }

            if (rig == null)
            {
                Debug.LogWarning("[CutsceneEditor] Rebind skipped: no DebugCutsceneRig in the stage.");
                return;
            }

            foreach (TrackAsset track in timeline.GetOutputTracks())
            {
                foreach (AutoBindChannel channel in CutsceneChannels.All)
                {
                    if (track.name != channel.trackName || track.GetType() != channel.trackType)
                        continue;

                    if (rig.TryGet(channel.trackName, out Object reference))
                    {
                        director.SetGenericBinding(track, reference);
                    }
                    else
                    {
                        Debug.LogWarning(
                            $"[CutsceneEditor] Track '{channel.trackName}' has no usable rig reference. " +
                            $"On the DebugCutsceneRig, run 'Populate from CutsceneChannels' and assign it.");
                    }
                    break;
                }
            }

            // Persist the rebound bindings so they survive save/reopen.
            EditorUtility.SetDirty(director);
        }
    }
}
