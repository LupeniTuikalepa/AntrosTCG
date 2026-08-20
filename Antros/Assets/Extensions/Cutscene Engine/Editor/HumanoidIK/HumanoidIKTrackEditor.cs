using System;
using System.Text.RegularExpressions;
using CutsceneEngine;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Timeline;

namespace CutsceneEngineEditor
{
    [CustomTimelineEditor(typeof(HumanoidIKTrack))]
    public class HumanoidIKTrackEditor : TrackEditor
    {
        public override TrackDrawOptions GetTrackOptions(TrackAsset track, UnityEngine.Object binding)
        {
            var options = base.GetTrackOptions(track, binding);
            if (track is HumanoidIKTrack humanoidTrack)
            {
                options.trackColor = new UnityEngine.Color(0.2f, 0.75f, 1f);
                var icon = HumanoidIKTrackIconProvider.GetIcon(humanoidTrack.target);
                if (icon)
                {
                    options.icon = icon;
                }

                if (binding is not Animator animator)
                {
                    options.errorText = "Bind a humanoid Animator.";
                }
                else if (!HumanoidIKUtility.IsUsableHumanoid(animator))
                {
                    options.errorText = "The bound Animator must use a humanoid avatar.";
                }
                else if (HumanoidIKTrackValidation.TryFindDuplicateTarget(
                             humanoidTrack,
                             animator,
                             TimelineEditor.inspectedDirector,
                             out _))
                {
                    options.errorText = $"Another track bound to this Animator already controls {humanoidTrack.target}.";
                }
            }

            return options;
        }
    }

    internal static class HumanoidIKTrackIconProvider
    {
        const string HandIconGuid = "ffd1d4cb4fc256943a9fbe377a55f470";
        const string FootIconGuid = "e8fc7fd83cf609442ad52f4705873e2f";
        const string CombinedIconGuid = "6111f926d81193b4280de04ec4b156c9";

        static Texture2D _handIcon;
        static Texture2D _flippedHandIcon;
        static Texture2D _footIcon;
        static Texture2D _flippedFootIcon;
        static Texture2D _combinedIcon;

        internal static Texture2D GetIcon(HumanoidIKTarget target)
        {
            switch (target)
            {
                case HumanoidIKTarget.LeftHand:
                    return LoadIcon(ref _handIcon, HandIconGuid);
                case HumanoidIKTarget.RightHand:
                    return GetFlippedIcon(ref _flippedHandIcon, ref _handIcon, HandIconGuid);
                case HumanoidIKTarget.LeftFoot:
                    return GetFlippedIcon(ref _flippedFootIcon, ref _footIcon, FootIconGuid);
                case HumanoidIKTarget.RightFoot:
                    return LoadIcon(ref _footIcon, FootIconGuid);
                default:
                    return LoadIcon(ref _combinedIcon, CombinedIconGuid);
            }
        }

        static Texture2D GetFlippedIcon(
            ref Texture2D cachedFlippedIcon,
            ref Texture2D cachedSourceIcon,
            string guid)
        {
            if (cachedFlippedIcon) return cachedFlippedIcon;

            var source = LoadIcon(ref cachedSourceIcon, guid);
            if (!source) return null;

            cachedFlippedIcon = CreateFlippedTexture(source);
            return cachedFlippedIcon ? cachedFlippedIcon : source;
        }

        static Texture2D CreateFlippedTexture(Texture2D source)
        {
            if (!source) return null;

            var rt = RenderTexture.GetTemporary(
                source.width,
                source.height,
                0,
                RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.sRGB);
            var previous = RenderTexture.active;
            Graphics.Blit(source, rt, new Vector2(-1f, 1f), new Vector2(1f, 0f));

            RenderTexture.active = rt;
            var flipped = new Texture2D(
                source.width,
                source.height,
                TextureFormat.RGBA32,
                false)
            {
                name = source.name + " (Flipped)",
                hideFlags = HideFlags.DontSave
            };
            flipped.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
            flipped.Apply();

            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(rt);

            return flipped;
        }

        static Texture2D LoadIcon(ref Texture2D cachedIcon, string guid)
        {
            if (cachedIcon) return cachedIcon;

            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path)) return null;

            cachedIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            return cachedIcon;
        }
    }

    [CustomEditor(typeof(HumanoidIKTrack))]
    public class HumanoidIKTrackInspector : Editor
    {
        static readonly Regex SideTokenRegex = new Regex(
            @"(?:Left|Right)(?=$|[^a-z]|[A-Z])",
            RegexOptions.CultureInvariant);

        static readonly Regex EndTokenRegex = new Regex(
            @"(?:Hand|Foot)(?=$|[^a-z]|[A-Z])",
            RegexOptions.CultureInvariant);

        SerializedProperty targetProperty;
        SerializedProperty autoRenameClipsProperty;

        void OnEnable()
        {
            targetProperty = serializedObject.FindProperty(nameof(HumanoidIKTrack.target));
            autoRenameClipsProperty = serializedObject.FindProperty(nameof(HumanoidIKTrack.autoRenameClips));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var track = (HumanoidIKTrack)target;
            var previousTarget = track.target;

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(targetProperty);
            var targetChanged = EditorGUI.EndChangeCheck();
            EditorGUILayout.PropertyField(autoRenameClipsProperty);

            var nextTarget = (HumanoidIKTarget)targetProperty.enumValueIndex;
            var shouldRename = autoRenameClipsProperty.boolValue;
            if (targetChanged && shouldRename)
            {
                Undo.RecordObject(track, "Change Humanoid IK Target And Rename Clips");
            }

            serializedObject.ApplyModifiedProperties();
            DrawDuplicateTargetWarning(track);
            if (!targetChanged || previousTarget == nextTarget) return;

            if (shouldRename)
            {
                RenameClipTokens(track, nextTarget);
            }

            EditorUtility.SetDirty(track);
            TimelineEditor.Refresh(RefreshReason.ContentsModified);
            SceneView.RepaintAll();
        }

        static void RenameClipTokens(HumanoidIKTrack track, HumanoidIKTarget target)
        {
            var side = target is HumanoidIKTarget.LeftHand or HumanoidIKTarget.LeftFoot
                ? "Left"
                : "Right";
            var end = target is HumanoidIKTarget.LeftHand or HumanoidIKTarget.RightHand
                ? "Hand"
                : "Foot";

            foreach (var clip in track.GetClips())
            {
                if (string.IsNullOrEmpty(clip.displayName)) continue;

                var displayName = SideTokenRegex.Replace(clip.displayName, side);
                displayName = EndTokenRegex.Replace(displayName, end);
                if (string.Equals(displayName, clip.displayName, StringComparison.Ordinal)) continue;

                clip.displayName = displayName;
            }
        }

        static void DrawDuplicateTargetWarning(HumanoidIKTrack track)
        {
            var director = TimelineEditor.inspectedDirector;
            if (!director) return;

            var animator = director.GetGenericBinding(track) as Animator;
            if (!HumanoidIKTrackValidation.TryFindDuplicateTarget(
                    track,
                    animator,
                    director,
                    out var duplicate))
            {
                return;
            }

            EditorGUILayout.HelpBox(
                $"'{duplicate.name}' is also bound to this Animator and controls {track.target}. " +
                "Only one Humanoid IK track per Animator and target is supported; evaluation order otherwise decides which track wins.",
                MessageType.Warning);
        }
    }
}
