using ATCG.Battle.CapacitySystem.Core.Cutscenes.Tracks;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEditor.UIElements;
using UnityEngine.Timeline;
using UnityEngine.UIElements;

namespace ATCG.Editor.Cutscenes
{
    /// <summary>
    /// Clip inspector for the impact shake (UI Toolkit). Exposes Attack / Sustain / Decay as
    /// numbers mapped straight onto the clip's Ease In / Ease Out (the single source of truth):
    /// editing a field moves the fade handle, and dragging the handle updates the field via a
    /// scheduled poll. Sustain is the un-eased middle, shown read-only.
    /// </summary>
    [CustomEditor(typeof(ScreenShakeImpactClip))]
    public sealed class ScreenShakeImpactClipEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            root.Add(new Label("Impact envelope (the clip fades)"));

            var attack = new DoubleField("Attack (fade in)");
            var decay = new DoubleField("Decay (fade out)");
            var sustain = new DoubleField("Sustain (middle)");
            sustain.SetEnabled(false);

            attack.RegisterValueChangedCallback(evt => ApplyEase(evt.newValue, null));
            decay.RegisterValueChangedCallback(evt => ApplyEase(null, evt.newValue));

            root.Add(attack);
            root.Add(decay);
            root.Add(sustain);

            // Pull clip values into the fields (covers Ease In/Out handle drags), but never
            // clobber a field the user is currently typing in.
            root.schedule.Execute(() =>
            {
                TimelineClip clip = FindClip();
                if (clip == null)
                    return;

                if (attack.focusController?.focusedElement != attack)
                    attack.SetValueWithoutNotify(clip.easeInDuration);
                if (decay.focusController?.focusedElement != decay)
                    decay.SetValueWithoutNotify(clip.easeOutDuration);

                sustain.SetValueWithoutNotify(clip.duration - clip.easeInDuration - clip.easeOutDuration);
            }).Every(100);

            var defaults = new VisualElement();
            InspectorElement.FillDefaultInspector(defaults, serializedObject, this);
            root.Add(defaults);

            return root;
        }

        // Push a field edit onto the clip's ease durations (the single source of truth).
        private void ApplyEase(double? attack, double? decay)
        {
            TimelineClip clip = FindClip();
            if (clip == null)
                return;

            TrackAsset track = clip.GetParentTrack();
            Undo.RegisterCompleteObjectUndo(track, "Edit impact envelope");

            if (attack.HasValue)
                clip.easeInDuration = attack.Value < 0.0 ? 0.0 : attack.Value;
            if (decay.HasValue)
                clip.easeOutDuration = decay.Value < 0.0 ? 0.0 : decay.Value;

            EditorUtility.SetDirty(track);
            TimelineEditor.Refresh(RefreshReason.ContentsModified);
        }

        // Ease durations live on the TimelineClip, not the PlayableAsset, so reach them through
        // the currently inspected timeline.
        private TimelineClip FindClip()
        {
            TimelineAsset timeline = TimelineEditor.inspectedAsset;
            if (timeline == null)
                return null;

            foreach (TrackAsset track in timeline.GetOutputTracks())
                foreach (TimelineClip clip in track.GetClips())
                    if (clip.asset == target)
                        return clip;

            return null;
        }
    }
}
