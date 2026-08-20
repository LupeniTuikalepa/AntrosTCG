using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CutsceneEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.Rendering;

namespace CutsceneEngineEditor
{
    [CustomEditor(typeof(LookAtClip))]
    [CanEditMultipleObjects]
    public sealed class LookAtClipInspector : Editor
    {
        internal enum BlendShapeKeyRole
        {
            Blink,
            UpperEyelidFollow,
            LowerEyelidFollow,
            HorizontalEyelidFollow
        }
        const float TargetMarkerSize = 0.08f;
        const float TargetLineDashSize = 5f;
        const float AngleFieldWidth = 52f;
        const float AutoButtonWidth = 42f;
        const float ArrayHeaderHeight = 20f;
        const float ArrayAddButtonWidth = 20f;
        const float ArrayItemCountWidth = 46f;
        const float ArrayRemoveButtonWidth = 18f;
        const float ArrayElementPadding = 2f;
        const float ArrayBorderThickness = 1f;

        readonly Dictionary<string, ReorderableList> _blendShapeKeyLists = new();

        void OnEnable()
        {
            foreach (var selectedTarget in targets)
            {
                if (selectedTarget is not LookAtClip clip) continue;

                var changed = SanitizeAngleLimits(ref clip.eyesAngleLimits);
                changed |= SanitizeAngleLimits(ref clip.headAngleLimits);
                changed |= SanitizeAngleLimits(ref clip.neckAngleLimits);
                changed |= SanitizeAngleLimits(ref clip.bodyAngleLimits);
                if (changed) EditorUtility.SetDirty(clip);
            }
        }

        public override void OnInspectorGUI()
        {
            var eyelidKeyConfigurationHash =
                GetEyelidKeyConfigurationHash();
            serializedObject.Update();

            DrawContextHelp();
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty(nameof(LookAtClip.target)));
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty(nameof(LookAtClip.position)));
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty(nameof(LookAtClip.gizmoColor)),
                new GUIContent(
                    "Gizmo Color",
                    "Color and opacity shared by the Scene view target gizmo and the Timeline clip accent."));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Head Pose", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty(nameof(LookAtClip.chinOffset)));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Rotation Weights", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty(nameof(LookAtClip.eyesWeight)));
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty(nameof(LookAtClip.headWeight)));
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty(nameof(LookAtClip.neckWeight)));
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty(nameof(LookAtClip.bodyWeight)));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Rotation Limits", EditorStyles.boldLabel);
            DrawAngleLimits("Eyes", nameof(LookAtClip.eyesAngleLimits));
            DrawAngleLimits("Head", nameof(LookAtClip.headAngleLimits));
            DrawAngleLimits("Neck", nameof(LookAtClip.neckAngleLimits));
            DrawAngleLimits("Body", nameof(LookAtClip.bodyAngleLimits));

            EditorGUILayout.Space();
            DrawBlinkSettings();

            if (serializedObject.ApplyModifiedProperties())
            {
                var director = TimelineEditor.inspectedDirector;
                if (director)
                {
                    foreach (var selectedTarget in targets)
                    {
                        if (selectedTarget is LookAtClip selectedClip)
                        {
                            LookAtTimelinePreviewUpdater.NotifyClipChanged(
                                director,
                                selectedClip);
                        }
                    }
                }

                var refreshReason =
                    eyelidKeyConfigurationHash !=
                    GetEyelidKeyConfigurationHash()
                        ? RefreshReason.ContentsModified
                        : RefreshReason.WindowNeedsRedraw;
                TimelineEditor.Refresh(refreshReason);
                SceneView.RepaintAll();
            }
        }

        int GetEyelidKeyConfigurationHash()
        {
            unchecked
            {
                var hash = 17;
                foreach (var selectedTarget in targets)
                {
                    if (selectedTarget is not LookAtClip clip) continue;

                    hash = hash * 31 + clip.GetHashCode();
                    hash = AppendStringArrayHash(
                        hash,
                        clip.upperEyelidFollowBlendShapeKeys);
                    hash = AppendStringArrayHash(
                        hash,
                        clip.lowerEyelidFollowBlendShapeKeys);
                    hash = AppendStringArrayHash(
                        hash,
                        clip.horizontalEyelidFollowBlendShapeKeys);
                }

                return hash;
            }
        }

        static int AppendStringArrayHash(
            int hash,
            string[] values)
        {
            unchecked
            {
                if (values == null) return hash * 31;

                hash = hash * 31 + values.Length;
                for (var i = 0; i < values.Length; i++)
                {
                    hash = hash * 31 +
                           StringComparer.Ordinal.GetHashCode(
                               values[i] ?? string.Empty);
                }

                return hash;
            }
        }


        void DrawAngleLimits(string label, string propertyName)
        {
            var limits = serializedObject.FindProperty(propertyName);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(label, EditorStyles.miniBoldLabel);
                DrawAngleRange(
                    limits.FindPropertyRelative(nameof(LookAtAngleLimits.horizontal)),
                    "Horizontal (Yaw)");
                DrawAngleRange(
                    limits.FindPropertyRelative(nameof(LookAtAngleLimits.vertical)),
                    "Vertical (Pitch)");
            }
        }

        static void DrawAngleRange(SerializedProperty range, string label)
        {
            var value = range.vector2Value;
            var min = value.x;
            var max = value.y;
            var previousMixedValue = EditorGUI.showMixedValue;
            EditorGUI.showMixedValue = range.hasMultipleDifferentValues;

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PrefixLabel(label);
                EditorGUI.BeginChangeCheck();
                min = EditorGUILayout.FloatField(min, GUILayout.Width(AngleFieldWidth));
                EditorGUILayout.MinMaxSlider(
                    ref min,
                    ref max,
                    LookAtAngleLimits.MinimumAngle,
                    LookAtAngleLimits.MaximumAngle);
                max = EditorGUILayout.FloatField(max, GUILayout.Width(AngleFieldWidth));
                if (EditorGUI.EndChangeCheck())
                {
                    min = Mathf.Clamp(
                        min,
                        LookAtAngleLimits.MinimumAngle,
                        LookAtAngleLimits.MaximumAngle);
                    max = Mathf.Clamp(
                        max,
                        LookAtAngleLimits.MinimumAngle,
                        LookAtAngleLimits.MaximumAngle);
                    range.vector2Value = new Vector2(
                        Mathf.Min(min, max),
                        Mathf.Max(min, max));
                }
            }

            EditorGUI.showMixedValue = previousMixedValue;
        }

        static bool SanitizeAngleLimits(ref LookAtAngleLimits angleLimits)
        {
            var sanitized = angleLimits.Sanitized();
            if (angleLimits.Equals(sanitized)) return false;

            angleLimits = sanitized;
            return true;
        }

        void DrawContextHelp()
        {
            var director = TimelineEditor.inspectedDirector;
            if (!director)
            {
                EditorGUILayout.HelpBox(
                    "Select this clip through a Timeline window to resolve its target and humanoid binding.",
                    MessageType.Info);
                return;
            }

            var clip = target as LookAtClip;
            var track = clip ? director.GetTrackOf<LookAtTrack>(clip) : null;
            if (!track) return;

            if (!clip.ResolveTarget(director))
            {
                EditorGUILayout.HelpBox(
                    "Position is used as a PlayableDirector-local look target while Target is unassigned or unresolved.",
                    MessageType.Info);
            }

            var animator = director.GetGenericBinding(track) as Animator;
            if (!animator)
            {
                EditorGUILayout.HelpBox(
                    "Bind an Animator to the Look At track.",
                    MessageType.Warning);
                return;
            }

            var hasEyes = false;
            if (HumanoidIKUtility.IsUsableHumanoid(animator))
            {
                hasEyes =
                    animator.GetBoneTransform(HumanBodyBones.LeftEye) ||
                    animator.GetBoneTransform(HumanBodyBones.RightEye);
            }
            else if (LookAtGenericRigUtility.TryResolve(
                         animator,
                         track,
                         out var genericRig))
            {
                hasEyes = genericRig.LeftEye || genericRig.RightEye;
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "A Generic Animator needs a Head. Enable automatic detection or assign it in the Look At Track Inspector.",
                    MessageType.Warning);
                return;
            }

            if (clip.eyesWeight > 0f && !hasEyes)
            {
                EditorGUILayout.HelpBox(
                    "This rig has no mapped eye bones. Eyes Weight is ignored; head, neck, and body channels still work.",
                    MessageType.Info);
            }
        }

        internal static void DrawTargetGizmo(
            TimelineClip timelineClip,
            LookAtClip clip,
            PlayableDirector director,
            LookAtTrack track,
            float opacityMultiplier,
            bool drawPositionHandle)
        {
            if (!director || !clip || !track) return;

            var animator = director.GetGenericBinding(track) as Animator;
            var lookTarget = clip.ResolveTarget(director);
            var targetPosition = LookAtUtility.ResolveTargetPosition(
                lookTarget,
                director.transform,
                clip.position);

            var previousColor = Handles.color;
            var previousMatrix = Handles.matrix;
            var previousZTest = Handles.zTest;
            try
            {
                Handles.color = LookAtTimelineGizmoRegistry.ResolveGizmoColor(
                    clip,
                    opacityMultiplier);
                Handles.matrix = Matrix4x4.identity;
                Handles.zTest = CompareFunction.Always;

                if (Event.current.type == EventType.Repaint)
                {
                    if (LookAtGizmoUtility.TryGetLineOrigins(
                            animator,
                            track,
                            out var primaryOrigin,
                            out var secondaryOrigin))
                    {
                        Handles.DrawDottedLine(
                            primaryOrigin.position,
                            targetPosition,
                            TargetLineDashSize);
                        if (secondaryOrigin)
                        {
                            Handles.DrawDottedLine(
                                secondaryOrigin.position,
                                targetPosition,
                                TargetLineDashSize);
                        }
                    }

                    Handles.SphereHandleCap(
                        0,
                        targetPosition,
                        Quaternion.identity,
                        HandleUtility.GetHandleSize(targetPosition) * TargetMarkerSize,
                        EventType.Repaint);
                }

                var showPositionHandle =
                    drawPositionHandle &&
                    (Tools.current == Tool.Move || Tools.current == Tool.Transform);
                if ( !showPositionHandle) return;

                var handleRotation = Tools.pivotRotation == PivotRotation.Local
                    ? director.transform.rotation
                    : Quaternion.identity;
                EditorGUI.BeginChangeCheck();
                var nextTargetPosition = Handles.PositionHandle(
                    targetPosition,
                    handleRotation);

                if (!EditorGUI.EndChangeCheck()) return;

                var undoID = Undo.GetCurrentGroup();
                Undo.RecordObject(clip, "Move Look At Target");

                if (lookTarget)
                {
                    Undo.RecordObject(lookTarget.transform, "Move Look At Target");
                    lookTarget.transform.position = nextTargetPosition;
                    clip.position = director.transform.InverseTransformPoint(lookTarget.position);
                    
                    EditorUtility.SetDirty(lookTarget);
                }
                else
                {
                    clip.position = director.transform.InverseTransformPoint(nextTargetPosition);    
                }
                
                EditorUtility.SetDirty(clip);
                Undo.CollapseUndoOperations(undoID);
                LookAtTimelinePreviewUpdater.NotifyClipChanged(
                    director,
                    timelineClip);
                TimelineEditor.Refresh(RefreshReason.WindowNeedsRedraw); 
            }
            finally
            {
                Handles.color = previousColor;
                Handles.matrix = previousMatrix;
                Handles.zTest = previousZTest;
            }
        }


        void DrawBlinkSettings()
        {
            EditorGUILayout.LabelField("Eyelids", EditorStyles.boldLabel);
            DrawBlendShapeKeyArray(
                serializedObject.FindProperty(
                    nameof(LookAtClip.blinkBlendShapeKeys)),
                new GUIContent(
                    "Blink BlendShape Keys",
                    "BlendShapes driven only by blinking."),
                BlendShapeKeyRole.Blink);

            EditorGUILayout.LabelField("Blink", EditorStyles.miniBoldLabel);
            var mode = serializedObject.FindProperty(
                nameof(LookAtClip.blinkMode));
            EditorGUILayout.PropertyField(mode);

            if (!mode.hasMultipleDifferentValues)
            {
                if ((LookAtBlinkMode)mode.enumValueIndex ==
                    LookAtBlinkMode.AnimationCurve)
                {
                    EditorGUILayout.CurveField(
                        serializedObject.FindProperty(
                            nameof(LookAtClip.blinkCurve)),
                        Color.green,
                        new Rect(0f, 0f, 1f, 1f),
                        new GUIContent("Blink Curve"));
                }
                else
                {
                    EditorGUILayout.PropertyField(
                        serializedObject.FindProperty(
                            nameof(LookAtClip.blinkFrequency)));
                    EditorGUILayout.PropertyField(
                        serializedObject.FindProperty(
                            nameof(LookAtClip.blinkDuration)));
                    EditorGUILayout.CurveField(
                        serializedObject.FindProperty(
                            nameof(LookAtClip.automaticBlinkCurve)),
                        Color.cyan,
                        new Rect(0f, 0f, 1f, 1f),
                        new GUIContent(
                            "Blink Shape",
                            "Normalized automatic-blink time and eyelid openness: 0 is closed and 1 is open."));
                    EditorGUILayout.PropertyField(
                        serializedObject.FindProperty(
                            nameof(LookAtClip.blinkNoiseOffset)));
                }
            }

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField(
                "Eyelid Muscle Fine-Tuning",
                EditorStyles.miniBoldLabel);

            DrawBlendShapeKeyArray(
                serializedObject.FindProperty(
                    nameof(LookAtClip.upperEyelidFollowBlendShapeKeys)),
                new GUIContent(
                    "Upper Eyelid Muscle Keys",
                    "BlendShapes used for subtle upper-eyelid muscle response while the eyes look down."),
                BlendShapeKeyRole.UpperEyelidFollow);
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty(
                    nameof(LookAtClip.upperEyelidFollowWeight)),
                new GUIContent(
                    "Upper Eyelid Muscle Strength",
                    "Maximum upper-eyelid muscle response while looking down."));
            EditorGUILayout.CurveField(
                serializedObject.FindProperty(
                    nameof(LookAtClip.upperEyelidFollowCurve)),
                new Color(1f, 0.7f, 0.2f),
                new Rect(0f, 0f, 1f, 1f),
                new GUIContent(
                    "Upper Eyelid Muscle Curve",
                    "X is vertical eye direction (0 down, 0.5 forward, 1 up). Y is upper-eyelid muscle response."));

            DrawBlendShapeKeyArray(
                serializedObject.FindProperty(
                    nameof(LookAtClip.lowerEyelidFollowBlendShapeKeys)),
                new GUIContent(
                    "Lower Eyelid Muscle Keys",
                    "BlendShapes used for subtle lower-eyelid muscle response while the eyes look up."),
                BlendShapeKeyRole.LowerEyelidFollow);
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty(
                    nameof(LookAtClip.lowerEyelidFollowWeight)),
                new GUIContent(
                    "Lower Eyelid Muscle Strength",
                    "Maximum lower-eyelid muscle response while looking up."));
            EditorGUILayout.CurveField(
                serializedObject.FindProperty(
                    nameof(LookAtClip.lowerEyelidFollowCurve)),
                new Color(0.4f, 0.8f, 1f),
                new Rect(0f, 0f, 1f, 1f),
                new GUIContent(
                    "Lower Eyelid Muscle Curve",
                    "X is vertical eye direction (0 down, 0.5 forward, 1 up). Y is lower-eyelid muscle response."));

            DrawBlendShapeKeyArray(
                serializedObject.FindProperty(
                    nameof(LookAtClip.horizontalEyelidFollowBlendShapeKeys)),
                new GUIContent(
                    "Side-to-Side Eyelid Muscle Keys",
                    "BlendShapes used for subtle eyelid-muscle response while the eyes look left or right."),
                BlendShapeKeyRole.HorizontalEyelidFollow);
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty(
                    nameof(LookAtClip.horizontalEyelidFollowWeight)),
                new GUIContent(
                    "Side-to-Side Eyelid Muscle Strength",
                    "Maximum eyelid-muscle response while looking left or right."));
            EditorGUILayout.CurveField(
                serializedObject.FindProperty(
                    nameof(LookAtClip.horizontalEyelidFollowCurve)),
                new Color(0.6f, 0.85f, 0.45f),
                new Rect(0f, 0f, 1f, 1f),
                new GUIContent(
                    "Side-to-Side Eyelid Muscle Curve",
                    "X is horizontal eye direction (0 left, 0.5 forward, 1 right). Y is eyelid-muscle response."));

        }

        void DrawBlendShapeKeyArray(
            SerializedProperty property,
            GUIContent label,
            BlendShapeKeyRole role)
        {
            var headerRect = EditorGUILayout.GetControlRect(
                hasLabel: false,
                height: ArrayHeaderHeight);
            GUI.Box(headerRect, GUIContent.none, EditorStyles.toolbar);

            var addRect = new Rect(
                headerRect.xMax - ArrayAddButtonWidth,
                headerRect.y,
                ArrayAddButtonWidth,
                headerRect.height);
            var autoRect = new Rect(
                addRect.x - AutoButtonWidth,
                headerRect.y,
                AutoButtonWidth,
                headerRect.height);
            var countRect = new Rect(
                autoRect.x - ArrayItemCountWidth,
                headerRect.y,
                ArrayItemCountWidth,
                headerRect.height);
            var foldoutRect = new Rect(
                headerRect.x + 2f,
                headerRect.y,
                Mathf.Max(0f, countRect.x - headerRect.x - 4f),
                headerRect.height);

            property.isExpanded = EditorGUI.Foldout(
                foldoutRect,
                property.isExpanded,
                label,
                toggleOnLabelClick: true);
            GUI.Label(
                countRect,
                property.hasMultipleDifferentValues
                    ? "Mixed"
                    : $"{property.arraySize} Items",
                EditorStyles.centeredGreyMiniLabel);

            using (new EditorGUI.DisabledScope(
                       !HasBoundAnimatorForAutoDetection()))
            {
                var tooltip = role switch
                {
                    BlendShapeKeyRole.Blink =>
                        "Find likely blink BlendShapes on the bound Animator.",
                    BlendShapeKeyRole.UpperEyelidFollow =>
                        "Find likely upper-eyelid muscle BlendShapes on the bound Animator.",
                    BlendShapeKeyRole.LowerEyelidFollow =>
                        "Find likely lower-eyelid muscle BlendShapes on the bound Animator.",
                    _ =>
                        "Find likely side-to-side eyelid-muscle BlendShapes on the bound Animator."
                };
                if (GUI.Button(
                        autoRect,
                        new GUIContent("Auto", tooltip),
                        EditorStyles.toolbarButton))
                {
                    AutoDetectBlendShapeKeys(targets.Cast<LookAtClip>(), role);
                    serializedObject.Update();
                    TimelineEditor.Refresh(
                        RefreshReason.ContentsModified);
                    SceneView.RepaintAll();
                }
            }

            using (new EditorGUI.DisabledScope(
                       property.hasMultipleDifferentValues))
            {
                if (GUI.Button(
                        addRect,
                        new GUIContent("+", "Add a BlendShape key."),
                        EditorStyles.toolbarButton))
                {
                    var newIndex = property.arraySize;
                    property.InsertArrayElementAtIndex(newIndex);
                    property.GetArrayElementAtIndex(newIndex).stringValue =
                        string.Empty;
                }
            }

            if (!property.isExpanded)
            {
                DrawArrayTopLeftBorder(headerRect, headerRect.yMax);
                return;
            }

            var list = GetBlendShapeKeyList(property);
            var removeIndex = -1;
            list.drawElementCallback =
                (rect, index, isActive, isFocused) =>
                {
                    if (index < 0 || index >= property.arraySize) return;

                    rect.y += ArrayElementPadding;
                    rect.height = EditorGUIUtility.singleLineHeight;
                    var removeRect = new Rect(
                        rect.xMax - ArrayRemoveButtonWidth,
                        rect.y,
                        ArrayRemoveButtonWidth,
                        rect.height);
                    var fieldRect = new Rect(
                        rect.x,
                        rect.y,
                        Mathf.Max(
                            0f,
                            rect.width - ArrayRemoveButtonWidth - 2f),
                        rect.height);

                    EditorGUI.PropertyField(
                        fieldRect,
                        property.GetArrayElementAtIndex(index),
                        GUIContent.none);
                    if (GUI.Button(
                            removeRect,
                            new GUIContent("x", "Remove this key."),
                            EditorStyles.centeredGreyMiniLabel))
                    {
                        removeIndex = index;
                    }
                };

            using (new EditorGUI.DisabledScope(
                       property.hasMultipleDifferentValues))
            {
                list.DoLayoutList();
            }
            var listRect = GUILayoutUtility.GetLastRect();

            if (removeIndex >= 0)
            {
                property.DeleteArrayElementAtIndex(removeIndex);
            }

            DrawArrayTopLeftBorder(
                headerRect,
                Mathf.Max(headerRect.yMax, listRect.yMax));
        }

        static void DrawArrayTopLeftBorder(
            Rect headerRect,
            float bottom)
        {
            if (Event.current.type != EventType.Repaint) return;

            var borderColor = EditorGUIUtility.isProSkin
                ? new Color(0.11f, 0.11f, 0.11f, 1f)
                : new Color(0.55f, 0.55f, 0.55f, 1f);
            EditorGUI.DrawRect(
                new Rect(
                    headerRect.x,
                    headerRect.y,
                    headerRect.width,
                    ArrayBorderThickness),
                borderColor);
            EditorGUI.DrawRect(
                new Rect(
                    headerRect.x,
                    headerRect.y,
                    ArrayBorderThickness,
                    Mathf.Max(
                        ArrayBorderThickness,
                        bottom - headerRect.y)),
                borderColor);
        }


        ReorderableList GetBlendShapeKeyList(
            SerializedProperty property)
        {
            if (_blendShapeKeyLists.TryGetValue(
                    property.propertyPath,
                    out var list))
            {
                return list;
            }

            list = new ReorderableList(
                serializedObject,
                property,
                draggable: true,
                displayHeader: false,
                displayAddButton: false,
                displayRemoveButton: false)
            {
                elementHeight = EditorGUIUtility.singleLineHeight +
                                ArrayElementPadding * 2f,
                footerHeight = 0f,
                headerHeight = 0f,
                showDefaultBackground = true
            };
            _blendShapeKeyLists.Add(property.propertyPath, list);
            return list;
        }


        bool HasBoundAnimatorForAutoDetection()
        {
            var director = TimelineEditor.inspectedDirector;
            if (!director) return false;

            foreach (var selectedTarget in targets)
            {
                if (selectedTarget is LookAtClip clip &&
                    ResolveBoundAnimator(director, clip))
                {
                    return true;
                }
            }

            return false;
        }

        internal static void AutoDetectBlendShapeKeys(IEnumerable<LookAtClip> targets, BlendShapeKeyRole role)
        {
            var director = TimelineEditor.inspectedDirector;
            if (!director) return;
            
            
            AutoDetectBlendShapeKeys(combine_targets(), role); 
            return;
            
            IEnumerable<(LookAtClip, Animator)> combine_targets()
            {
                foreach (var clip in targets)
                {
                    var animator = ResolveBoundAnimator(director, clip);
                    yield return (clip, animator);
                } 
            }
        }

        internal static void AutoDetectBlendShapeKeys(IEnumerable<(LookAtClip clip, Animator animator)> targets, BlendShapeKeyRole role)
        {
            var director = TimelineEditor.inspectedDirector;
            if (!director) return;

            var roleLabel = role switch
            {
                BlendShapeKeyRole.Blink => "blink",
                BlendShapeKeyRole.UpperEyelidFollow => "upper-lid",
                BlendShapeKeyRole.LowerEyelidFollow => "lower-lid",
                _ => "side-to-side-lid"
            };
            var updatedClipCount = 0;
            var detectedKeyCount = 0;
            foreach (var (clip, animator) in targets)
            {
                var detectedKeys = FindLikelyBlendShapeKeys(
                    animator,
                    role);
                if (detectedKeys.Length == 0) continue;

                Undo.RecordObject(
                    clip,
                    $"Auto Detect {roleLabel} BlendShapes");
                switch (role)
                {
                    case BlendShapeKeyRole.Blink:
                        clip.blinkBlendShapeKeys = detectedKeys;
                        break;
                    case BlendShapeKeyRole.UpperEyelidFollow:
                        clip.upperEyelidFollowBlendShapeKeys =
                            detectedKeys;
                        break;
                    case BlendShapeKeyRole.LowerEyelidFollow:
                        clip.lowerEyelidFollowBlendShapeKeys =
                            detectedKeys;
                        break;
                    case BlendShapeKeyRole.HorizontalEyelidFollow:
                        clip.horizontalEyelidFollowBlendShapeKeys =
                            detectedKeys;
                        break;
                }

                EditorUtility.SetDirty(clip);
                LookAtTimelinePreviewUpdater.NotifyClipChanged(
                    director,
                    clip);
                updatedClipCount++;
                detectedKeyCount += detectedKeys.Length;
            }

            if (updatedClipCount > 0)
            {
                Debug.Log(
                    $"[Look At] Detected {detectedKeyCount} likely " +
                    $"{roleLabel} BlendShape key(s) for " +
                    $"{updatedClipCount} clip(s).",
                    director);
            }
            else
            {
                Debug.LogWarning(
                    $"[Look At] No likely {roleLabel} BlendShapes were " +
                    "found on the bound Animator.",
                    director);
            }
        }

        static Animator ResolveBoundAnimator(
            PlayableDirector director,
            LookAtClip clip)
        {
            if (!director || !clip) return null;

            var track = director.GetTrackOf<LookAtTrack>(clip);
            return track
                ? director.GetGenericBinding(track) as Animator
                : null;
        }

        internal static string[] FindLikelyBlendShapeKeys(
            Animator animator,
            BlendShapeKeyRole role)
        {
            if (!animator) return Array.Empty<string>();

            var bestScore = 0;
            var results = new List<string>();
            var seenKeys = new HashSet<string>(
                StringComparer.Ordinal);
            var renderers = animator.GetComponentsInChildren<
                SkinnedMeshRenderer>(true);

            for (var rendererIndex = 0;
                 rendererIndex < renderers.Length;
                 rendererIndex++)
            {
                var mesh = renderers[rendererIndex].sharedMesh;
                if (!mesh) continue;

                for (var blendShapeIndex = 0;
                     blendShapeIndex < mesh.blendShapeCount;
                     blendShapeIndex++)
                {
                    var key = mesh.GetBlendShapeName(
                        blendShapeIndex);
                    if (string.IsNullOrEmpty(key) ||
                        !seenKeys.Add(key))
                    {
                        continue;
                    }

                    var score = ScoreBlendShapeKey(
                        key,
                        role);
                    if (score <= 0 || score < bestScore) continue;

                    if (score > bestScore)
                    {
                        bestScore = score;
                        results.Clear();
                    }

                    results.Add(key);
                }
            }

            results.Sort(StringComparer.Ordinal);
            return results.ToArray();
        }

        static int ScoreBlendShapeKey(
            string key,
            BlendShapeKeyRole role)
        {
            var normalized = NormalizeBlendShapeKey(key);
            if (normalized.Length == 0) return 0;

            return role switch
            {
                BlendShapeKeyRole.Blink =>
                    ScoreBlinkKey(key, normalized),
                BlendShapeKeyRole.UpperEyelidFollow =>
                    ScoreEyelidKey(
                        key,
                        normalized,
                        upper: true),
                BlendShapeKeyRole.LowerEyelidFollow =>
                    ScoreEyelidKey(
                        key,
                        normalized,
                        upper: false),
                _ => ScoreHorizontalEyelidKey(
                    key,
                    normalized)
            };
        }

        static int ScoreEyelidKey(
            string key,
            string normalized,
            bool upper)
        {
            var isRequestedLid = upper
                ? normalized.Contains("upperlid") ||
                  normalized.Contains("uppereyelid") ||
                  normalized.Contains("lidupper")
                : normalized.Contains("lowerlid") ||
                  normalized.Contains("lowereyelid") ||
                  normalized.Contains("lidlower");
            var isDirectionalEyeLook =
                normalized.Contains("eye") &&
                normalized.Contains(upper ? "lookdown" : "lookup");
            if (!isRequestedLid && !isDirectionalEyeLook) return 0;

            var hasMovementTerm = upper
                ? normalized.Contains("down") ||
                  normalized.Contains("close") ||
                  normalized.Contains("follow") ||
                  normalized.Contains("lower")
                : normalized.Contains("up") ||
                  normalized.Contains("close") ||
                  normalized.Contains("follow") ||
                  normalized.Contains("raise");
            var score = isRequestedLid
                ? hasMovementTerm
                    ? 150
                    : 130
                : 140;
            if (HasSideMarker(key, normalized))
            {
                score += 5;
            }

            return score;
        }

        static int ScoreHorizontalEyelidKey(
            string key,
            string normalized)
        {
            var isDirectionalEyeLook =
                normalized.Contains("eye") &&
                (normalized.Contains("lookleft") ||
                 normalized.Contains("lookright") ||
                 normalized.Contains("lookl") ||
                 normalized.Contains("lookr") ||
                 normalized.Contains("eyeleft") ||
                 normalized.Contains("eyeright"));
            if (!isDirectionalEyeLook) return 0;

            return HasSideMarker(key, normalized)
                ? 145
                : 140;
        }


        static int ScoreBlinkKey(
            string key,
            string normalized)
        {
            var score = 0;
            if (normalized.Contains("eyeblink"))
            {
                score = 120;
            }
            else if (normalized.Contains("blink"))
            {
                score = 110;
            }
            else if (normalized.Contains("eyeclose") ||
                     normalized.Contains("closeeye") ||
                     normalized.Contains("eyesclosed") ||
                     normalized.Contains("closedeyes") ||
                     normalized.Contains("eyeshut") ||
                     normalized.Contains("shuteye"))
            {
                score = 100;
            }
            else if (normalized.Contains("lidclose") ||
                     normalized.Contains("closelid"))
            {
                score = 95;
            }
            else if (normalized.Contains("au45"))
            {
                score = 90;
            }

            if (score > 0 &&
                HasSideMarker(key, normalized))
            {
                score += 5;
            }

            return score;
        }

        static string NormalizeBlendShapeKey(string key)
        {
            var builder = new StringBuilder(key.Length);
            for (var i = 0; i < key.Length; i++)
            {
                var character = key[i];
                if (char.IsLetterOrDigit(character))
                {
                    builder.Append(
                        char.ToLowerInvariant(character));
                }
            }

            return builder.ToString();
        }

        static bool HasSideMarker(
            string key,
            string normalized)
        {
            if (normalized.Contains("left") ||
                normalized.Contains("right"))
            {
                return true;
            }

            var lower = key.ToLowerInvariant();
            if (lower.EndsWith("_l") ||
                lower.EndsWith("_r") ||
                lower.EndsWith(".l") ||
                lower.EndsWith(".r") ||
                lower.EndsWith("-l") ||
                lower.EndsWith("-r"))
            {
                return true;
            }

            if (key.Length == 0) return false;

            var lastCharacter = key[key.Length - 1];
            return lastCharacter == 'L' ||
                   lastCharacter == 'R';
        }

    }

    internal static class LookAtGizmoUtility
    {
        internal static bool TryGetLineOrigins(
            Animator animator,
            LookAtTrack track,
            out Transform primaryOrigin,
            out Transform secondaryOrigin)
        {
            primaryOrigin = null;
            secondaryOrigin = null;
            if (!animator) return false;

            Transform head;
            Transform leftEye;
            Transform rightEye;
            if (HumanoidIKUtility.IsUsableHumanoid(animator))
            {
                head = animator.GetBoneTransform(HumanBodyBones.Head);
                leftEye = animator.GetBoneTransform(HumanBodyBones.LeftEye);
                rightEye = animator.GetBoneTransform(HumanBodyBones.RightEye);
            }
            else if (LookAtGenericRigUtility.TryResolve(
                         animator,
                         track,
                         out var genericRig))
            {
                head = genericRig.Head;
                leftEye = genericRig.LeftEye;
                rightEye = genericRig.RightEye;
            }
            else
            {
                return false;
            }

            if (leftEye)
            {
                primaryOrigin = leftEye;
                if (rightEye && rightEye != leftEye)
                {
                    secondaryOrigin = rightEye;
                }

                return true;
            }

            if (rightEye)
            {
                primaryOrigin = rightEye;
                return true;
            }

            primaryOrigin = FindHeadEnd(head) ?? head;
            return primaryOrigin;
        }

        internal static Transform FindHeadEnd(Transform head)
        {
            if (!head) return null;

            Transform best = null;
            var bestScore = int.MinValue;
            for (var i = 0; i < head.childCount; i++)
            {
                var child = head.GetChild(i);
                if (!child) continue;

                var normalized = NormalizeName(child.name);
                var isTerminalName = normalized is
                    "end" or "tip" or "nub" or "top";
                var isHeadTerminal =
                    normalized.Contains("head", StringComparison.Ordinal) &&
                    (normalized.Contains("end", StringComparison.Ordinal) ||
                     normalized.Contains("tip", StringComparison.Ordinal) ||
                     normalized.Contains("nub", StringComparison.Ordinal) ||
                     normalized.Contains("top", StringComparison.Ordinal));
                if (!isTerminalName && !isHeadTerminal) continue;

                var score = isHeadTerminal ? 100 : 0;
                if (child.childCount == 0) score += 10;
                if (normalized.Contains("end", StringComparison.Ordinal))
                {
                    score += 4;
                }

                if (score <= bestScore) continue;
                bestScore = score;
                best = child;
            }

            return best;
        }

        static string NormalizeName(string name)
        {
            if (string.IsNullOrEmpty(name)) return string.Empty;

            var builder = new StringBuilder(name.Length);
            for (var i = 0; i < name.Length; i++)
            {
                var character = name[i];
                if (char.IsLetterOrDigit(character))
                {
                    builder.Append(char.ToLowerInvariant(character));
                }
            }

            return builder.ToString();
        }
    }
}
