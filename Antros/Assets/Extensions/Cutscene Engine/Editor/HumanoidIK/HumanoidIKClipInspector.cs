using CutsceneEngine;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Rendering;

namespace CutsceneEngineEditor
{
    [CustomEditor(typeof(HumanoidIKClip))]
    [CanEditMultipleObjects]
    public class HumanoidIKClipInspector : Editor
    {
        const float FingerAngleLimitMin = -180f;
        const float FingerAngleLimitMax = 180f;
        const float FingerAngleFieldWidth = 58f;
        readonly HumanoidIKGizmoDrawer _gizmoDrawer = new HumanoidIKGizmoDrawer();
        bool _showFingerAngleRanges = true;
        bool _showToeAngleRanges = true;
        bool _rotationHandleDragActive;
        PivotRotation _rotationHandleDragPivot;
        Quaternion _rotationHandleDragStartTarget = Quaternion.identity;
        Quaternion _rotationHandleDragStartHandle = Quaternion.identity;
        Quaternion _rotationHandleDragCurrent = Quaternion.identity;
        UnityEngine.Playables.PlayableDirector _observedPreviewDirector;
        HumanoidIKTrack _observedPreviewTrack;
        UnityEngine.Object _observedPreviewBinding;

        void OnEnable()
        {
            EnsureFingerAngleRanges();
            RefreshObservedPreviewBinding(repaintSceneView: false);
            EditorApplication.update += DetectPreviewBindingChange;
            SceneView.duringSceneGui += DuringSceneGUI;
        }

        void OnDisable()
        {
            EditorApplication.update -= DetectPreviewBindingChange;
            SceneView.duringSceneGui -= DuringSceneGUI;
            _gizmoDrawer.Dispose();
            ResetRotationHandleDrag();
        }

        void DetectPreviewBindingChange()
        {
            RefreshObservedPreviewBinding(repaintSceneView: true);
        }

        void RefreshObservedPreviewBinding(bool repaintSceneView)
        {
            var director = TimelineEditor.inspectedDirector;
            HumanoidIKTrack track = null;
            UnityEngine.Object binding = null;
            if (director && target is HumanoidIKClip clip && clip)
            {
                track = director.GetTrackOf<HumanoidIKTrack>(clip);
                if (track) binding = director.GetGenericBinding(track);
            }

            if (director == _observedPreviewDirector &&
                track == _observedPreviewTrack &&
                binding == _observedPreviewBinding)
            {
                return;
            }

            _observedPreviewDirector = director;
            _observedPreviewTrack = track;
            _observedPreviewBinding = binding;
            _gizmoDrawer.ClearPreviewContexts();
            if (repaintSceneView) SceneView.RepaintAll();
        }

        public override void OnInspectorGUI()
        {
            EnsureSelectedClipsUseDirectorLocalSpace();
            EnsureFingerAngleRanges();
            serializedObject.Update();

            DrawTimelineContextHelp();
            DrawCaptureButton();
            DrawSpaceMigrationControls();

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(HumanoidIKClip.anchorTransform)));
            var gizmoColorChanged = DrawGizmoColorField();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("IK Target", EditorStyles.boldLabel);
            var primaryClip = (HumanoidIKClip)target;
            var explicitPositionAnchor = primaryClip.ResolveExplicitAnchor(TimelineEditor.inspectedDirector);
            if (explicitPositionAnchor)
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.Vector3Field(
                        new GUIContent(
                            "Position",
                            "Driven by Anchor Transform's world position."),
                        explicitPositionAnchor.position);
                }
            }
            else
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(HumanoidIKClip.position)));
            }
            var trackTarget = GetTrackTarget();
            var usesLegacyFootLineRotation =
                HumanoidIKUtility.IsFoot(trackTarget) &&
                primaryClip.UsesHumanoidEffectorRotation &&
                !primaryClip.UsesProjectedSoleRotation;
            var rotationLabel = HumanoidIKUtility.IsFoot(trackTarget)
                ? (usesLegacyFootLineRotation
                    ? "Legacy Ankle-Toe Rotation"
                    : primaryClip.UsesHumanoidEffectorRotation
                        ? "Foot Rotation"
                        : "Legacy Foot Rotation")
                : (primaryClip.UsesHumanoidEffectorRotation ? "Effector Rotation" : "Legacy Bone Rotation");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty(nameof(HumanoidIKClip.rotation)),
                new GUIContent(rotationLabel));
            if (EditorGUI.EndChangeCheck() && HumanoidIKUtility.IsFoot(trackTarget))
            {
                serializedObject.FindProperty(HumanoidIKClip.FootRotationFrameVersionFieldName).intValue =
                    HumanoidIKClip.CurrentFootRotationFrameVersion;
            }
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty(nameof(HumanoidIKClip.bendTarget)),
                new GUIContent(primaryClip.UsesHumanoidPoleDirection
                    ? "Pole Direction"
                    : "Legacy Bend Target"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Weights", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(HumanoidIKClip.positionWeight)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(HumanoidIKClip.rotationWeight)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(HumanoidIKClip.bendWeight)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(HumanoidIKClip.digitWeight)));

            EditorGUILayout.Space();
            if (HumanoidIKUtility.IsFoot(trackTarget))
            {
                EditorGUILayout.LabelField("Toe Group Controls", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(HumanoidIKClip.toeFan)));
                DrawToeRigHelp(trackTarget);
                EditorGUILayout.Space();
            }

            EditorGUILayout.LabelField(GetDigitLabel(), EditorStyles.boldLabel);
            if (HumanoidIKUtility.IsFoot(trackTarget))
            {
                var toeBaseProp = serializedObject.FindProperty(nameof(HumanoidIKClip.toeBaseBend));
                EditorGUI.BeginChangeCheck();
                var nextToeBase = EditorGUILayout.FloatField("Toe Root", toeBaseProp.floatValue);
                if (EditorGUI.EndChangeCheck())
                {
                    toeBaseProp.floatValue = Mathf.Clamp(nextToeBase, -1f, 1f);
                }
            }
            DrawDigitBendProperty(nameof(HumanoidIKDigitBendPose.thumbOrBigToe), GetFirstDigitLabel());
            DrawDigitBendProperty(nameof(HumanoidIKDigitBendPose.indexOrSecondToe), HumanoidIKUtility.IsHand(trackTarget) ? "Index" : "Second Toe");
            DrawDigitBendProperty(nameof(HumanoidIKDigitBendPose.middleOrThirdToe), HumanoidIKUtility.IsHand(trackTarget) ? "Middle" : "Third Toe");
            DrawDigitBendProperty(nameof(HumanoidIKDigitBendPose.ringOrFourthToe), HumanoidIKUtility.IsHand(trackTarget) ? "Ring" : "Fourth Toe");
            DrawDigitBendProperty(nameof(HumanoidIKDigitBendPose.littleOrFifthToe), HumanoidIKUtility.IsHand(trackTarget) ? "Little" : "Fifth Toe");

            if (HumanoidIKUtility.IsHand(trackTarget))
            {
                EditorGUILayout.Space();
                DrawFingerAngleRanges();
            }
            else if (HumanoidIKUtility.IsFoot(trackTarget))
            {
                EditorGUILayout.Space();
                DrawToeAngleRanges();
            }

            if (serializedObject.ApplyModifiedProperties() || gizmoColorChanged)
            {
                TimelineEditor.Refresh(RefreshReason.ContentsModified);
                SceneView.RepaintAll();
            }
        }

        void EnsureSelectedClipsUseDirectorLocalSpace()
        {
            var director = TimelineEditor.inspectedDirector;
            if (!director) return;

            var changed = false;
            foreach (var selectedTarget in targets)
            {
                if (selectedTarget is not HumanoidIKClip clip ||
                    !director.GetTrackOf<HumanoidIKTrack>(clip))
                {
                    continue;
                }

                changed |= HumanoidIKClipSpaceMigration.EnsureDirectorLocal(clip, director);
            }

            if (changed)
            {
                TimelineEditor.Refresh(RefreshReason.ContentsModified);
                SceneView.RepaintAll();
            }
        }

        void DrawTimelineContextHelp()
        {
            var director = TimelineEditor.inspectedDirector;
            if (!director)
            {
                EditorGUILayout.HelpBox("Select this clip through a Timeline window to enable binding-aware handles.", MessageType.Info);
                return;
            }

            var track = director.GetTrackOf<HumanoidIKTrack>((HumanoidIKClip)target);
            if (!track)
            {
                EditorGUILayout.HelpBox("Could not locate the HumanoidIKTrack for this clip.", MessageType.Warning);
                return;
            }

            var animator = director.GetGenericBinding(track) as Animator;
            if (!HumanoidIKUtility.IsUsableHumanoid(animator))
            {
                EditorGUILayout.HelpBox("Bind a humanoid Animator to this track.", MessageType.Warning);
            }
        }

        void DrawCaptureButton()
        {
            using (new EditorGUI.DisabledScope(!TryGetContext((HumanoidIKClip)target, out _, out _, out _, out _)))
            {
                if (GUILayout.Button("Capture Current Limb Pose"))
                {
                    foreach (var selectedTarget in targets)
                    {
                        var clip = (HumanoidIKClip)selectedTarget;
                        if (TryGetContext(clip, out var director, out var track, out var animator, out var anchor))
                        {
                            CaptureCurrentPose(
                                clip,
                                track.target,
                                animator,
                                anchor,
                                clip.ResolveExplicitAnchor(director));
                        }
                    }
                }
            }
        }

        void DrawSpaceMigrationControls()
        {
            var hasLegacySpace = false;
            foreach (var selectedTarget in targets)
            {
                if (selectedTarget is not HumanoidIKClip clip) continue;
                var usesLegacyFootLineRotation =
                    TryGetTimelineContext(clip, out _, out var track, out _) &&
                    HumanoidIKUtility.IsFoot(track.target) &&
                    clip.UsesHumanoidEffectorRotation &&
                    !clip.UsesProjectedSoleRotation;
                if (!clip.UsesHumanoidEffectorRotation ||
                    !clip.UsesHumanoidPoleDirection ||
                    usesLegacyFootLineRotation)
                {
                    hasLegacySpace = true;
                    break;
                }
            }

            if (!hasLegacySpace) return;

            EditorGUILayout.HelpBox(
                "This clip uses legacy rotation or bend storage. Legacy Foot effector values describe the sloped ankle-to-toe line; conversion preserves the visible Foot bone pose while storing the projected sole frame.",
                MessageType.Warning);

            using (new EditorGUI.DisabledScope(!CanConvertLegacySpaces((HumanoidIKClip)target)))
            {
                if (!GUILayout.Button("Convert to Current Humanoid Spaces")) return;

                foreach (var selectedTarget in targets)
                {
                    var clip = (HumanoidIKClip)selectedTarget;
                    if (TryGetTimelineContext(clip, out var director, out var track, out var anchor))
                    {
                        var animator = director.GetGenericBinding(track) as Animator;
                        ConvertLegacySpaces(clip, track.target, animator, anchor);
                    }
                }

                serializedObject.Update();
                TimelineEditor.Refresh(RefreshReason.ContentsModified);
                SceneView.RepaintAll();
            }
        }

        void ConvertLegacySpaces(
            HumanoidIKClip clip,
            HumanoidIKTarget target,
            Animator animator,
            Transform anchor)
        {
            var hasLimb = HumanoidIKUtility.TryGetLimbBones(animator, target, out var limb);

            Undo.RecordObject(clip, "Convert Humanoid IK Spaces");
            HumanoidIKUtility.ResolveWorldPose(
                anchor,
                clip.position,
                clip.rotation,
                clip.bendTarget,
                out _,
                out var storedWorldRotation,
                out _);
            if (!clip.UsesHumanoidEffectorRotation)
            {
                if (hasLimb &&
                    _gizmoDrawer.TryGetPreviewBoneToEffectorRotation(
                        animator,
                        target,
                        out var boneToEffectorRotation))
                {
                    clip.SetTargetWorldRotation(
                        anchor,
                        HumanoidIKUtility.ToEffectorRotation(
                            storedWorldRotation,
                            clip.RotationSpace,
                            boneToEffectorRotation));
                }
            }
            else if (HumanoidIKUtility.IsFoot(target) &&
                     !clip.UsesProjectedSoleRotation &&
                     _gizmoDrawer.TryGetPreviewBoneToEffectorRotation(
                         animator,
                         target,
                         out var boneToSoleRotation) &&
                     _gizmoDrawer.TryGetPreviewLegacyFootBoneToEffectorRotation(
                         animator,
                         target,
                         out var boneToLegacyFootLineRotation))
            {
                clip.SetTargetWorldRotation(
                    anchor,
                    HumanoidIKUtility.ToProjectedSoleRotation(
                        storedWorldRotation,
                        clip.RotationSpace,
                        clip.FootRotationFrameVersion,
                        boneToSoleRotation,
                        boneToLegacyFootLineRotation));
            }

            if (!clip.UsesHumanoidPoleDirection)
            {
                var worldBendTarget = anchor
                    ? anchor.TransformPoint(clip.bendTarget)
                    : clip.bendTarget;
                clip.SetHumanoidPoleWorldVector(anchor, worldBendTarget);
            }

            EditorUtility.SetDirty(clip);
        }

        bool CanConvertLegacySpaces(HumanoidIKClip clip)
        {
            if (!TryGetTimelineContext(clip, out var director, out var track, out _)) return false;
            var animator = director.GetGenericBinding(track) as Animator;
            return HumanoidIKUtility.IsUsableHumanoid(animator) &&
                   HumanoidIKUtility.TryGetLimbBones(animator, track.target, out _);
        }

        void DrawDigitBendProperty(string propertyName, string label)
        {
            var digitBends = serializedObject.FindProperty(nameof(HumanoidIKClip.digitBends));
            var property = digitBends.FindPropertyRelative(propertyName);
            EditorGUILayout.PropertyField(property, new GUIContent(label), includeChildren: true);
        }

        void DrawToeRigHelp(HumanoidIKTarget target)
        {
            if (!TryGetContext((HumanoidIKClip)this.target, out _, out _, out var animator, out _))
            {
                EditorGUILayout.HelpBox(
                    "Bind a humanoid Animator to preview the detected toe rig. The Scene overlay Stretch edits available toe bends using narrow ranges; Toe Fan applies only to articulated multi-toe rigs.",
                    MessageType.Info);
                return;
            }

            var rigKind = HumanoidIKUtility.GetToeRigKind(animator, target);
            if (rigKind == HumanoidIKToeRigKind.None)
            {
                EditorGUILayout.HelpBox(
                    "The bound Humanoid has no mapped toe transform. Foot position and rotation remain active, and the canonical five-toe gizmo stays neutral; only runtime toe posing is skipped.",
                    MessageType.Info);
            }
            else if (rigKind == HumanoidIKToeRigKind.ToeFoot)
            {
                EditorGUILayout.HelpBox(
                    "Simple Foot-Toe rig detected. Scene overlay Stretch and the first joint slider edit the same mapped toe bend. Toe Fan is preserved but does not rotate this rig.",
                    MessageType.Info);
            }
            else if (rigKind == HumanoidIKToeRigKind.ArticulatedToes)
            {
                EditorGUILayout.HelpBox(
                    "Articulated toe rig detected. Scene overlay Stretch rewrites all available toe bends; Toe Fan adds a narrow proximal spread.",
                    MessageType.None);
            }
        }
        void DrawFingerAngleRanges()
        {
            _showFingerAngleRanges = EditorGUILayout.Foldout(
                _showFingerAngleRanges,
                "Finger Stretch Angle Ranges",
                toggleOnLabelClick: true);
            if (!_showFingerAngleRanges) return;

            EditorGUILayout.HelpBox(
                "Adjust the clip's humanoid muscle-angle ranges used by the Scene view Stretch and Finger Fan controls. X Min/Max are the closed/open Stretch endpoints. Joint 1 Y calibrates thumb spread or each finger's fan travel.",
                MessageType.None);

            var ranges = serializedObject.FindProperty(nameof(HumanoidIKClip.digitBendRanges));
            var thumbSpreadRange = serializedObject.FindProperty(nameof(HumanoidIKClip.thumbSpreadRange));
            var fingerSpreadRanges = serializedObject.FindProperty(nameof(HumanoidIKClip.fingerSpreadRanges));
            if (ranges == null || !ranges.isArray || ranges.arraySize != 15 ||
                fingerSpreadRanges == null || !fingerSpreadRanges.isArray || fingerSpreadRanges.arraySize != 4)
            {
                EditorGUILayout.HelpBox("Finger angle ranges are being initialized. Re-select the clip if this message remains visible.", MessageType.Info);
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                DrawDigitAngleRanges(ranges, 0, "Thumb", thumbSpreadRange);
                DrawDigitAngleRanges(ranges, 1, "Index", fingerSpreadRanges.GetArrayElementAtIndex(0));
                DrawDigitAngleRanges(ranges, 2, "Middle", fingerSpreadRanges.GetArrayElementAtIndex(1));
                DrawDigitAngleRanges(ranges, 3, "Ring", fingerSpreadRanges.GetArrayElementAtIndex(2));
                DrawDigitAngleRanges(ranges, 4, "Little", fingerSpreadRanges.GetArrayElementAtIndex(3));
            }
        }

        void DrawToeAngleRanges()
        {
            _showToeAngleRanges = EditorGUILayout.Foldout(
                _showToeAngleRanges,
                "Toe Stretch Angle Ranges",
                toggleOnLabelClick: true);
            if (!_showToeAngleRanges) return;

            EditorGUILayout.HelpBox(
                "Adjust the clip's toe bend angle ranges used by the Scene view Stretch and Toe Joint controls. X Min/Max are the closed/open bend endpoints for each toe joint row.",
                MessageType.None);

            var ranges = serializedObject.FindProperty(nameof(HumanoidIKClip.toeBendRanges));
            var toeBaseRange = serializedObject.FindProperty(nameof(HumanoidIKClip.toeBaseBendRange));
            if (ranges == null || !ranges.isArray || ranges.arraySize != 3 || toeBaseRange == null)
            {
                EditorGUILayout.HelpBox("Toe angle ranges are being initialized. Re-select the clip if this message remains visible.", MessageType.Info);
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                DrawAngleRange(toeBaseRange, "Toe Root (Base)");
                DrawAngleRange(ranges.GetArrayElementAtIndex(0), "Joint 1 (Proximal)");
                DrawAngleRange(ranges.GetArrayElementAtIndex(1), "Joint 2 (Intermediate)");
                DrawAngleRange(ranges.GetArrayElementAtIndex(2), "Joint 3 (Distal)");
            }
        }

        static void DrawDigitAngleRanges(
            SerializedProperty ranges,
            int digitIndex,
            string label,
            SerializedProperty additionalProximalAxis = null)
        {
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                DrawAngleRange(ranges.GetArrayElementAtIndex(digitIndex * 3), "Joint 1 X");
                if (additionalProximalAxis != null)
                {
                    DrawAngleRange(additionalProximalAxis, "Joint 1 Y");
                }
                DrawAngleRange(ranges.GetArrayElementAtIndex(digitIndex * 3 + 1), "Joint 2 X");
                DrawAngleRange(ranges.GetArrayElementAtIndex(digitIndex * 3 + 2), "Joint 3 X");
            }
        }

        static void DrawAngleRange(SerializedProperty range, string label)
        {
            var minProperty = range.FindPropertyRelative(nameof(Vector2.x));
            var maxProperty = range.FindPropertyRelative(nameof(Vector2.y));
            var min = minProperty.floatValue;
            var max = maxProperty.floatValue;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(label);
            EditorGUI.BeginChangeCheck();
            min = EditorGUILayout.FloatField(min, GUILayout.Width(FingerAngleFieldWidth));
            EditorGUILayout.MinMaxSlider(ref min, ref max, FingerAngleLimitMin, FingerAngleLimitMax);
            max = EditorGUILayout.FloatField(max, GUILayout.Width(FingerAngleFieldWidth));
            if (EditorGUI.EndChangeCheck())
            {
                min = Mathf.Clamp(min, FingerAngleLimitMin, FingerAngleLimitMax);
                max = Mathf.Clamp(max, FingerAngleLimitMin, FingerAngleLimitMax);
                minProperty.floatValue = Mathf.Min(min, max);
                maxProperty.floatValue = Mathf.Max(min, max);
            }
            EditorGUILayout.EndHorizontal();
        }

        void EnsureFingerAngleRanges()
        {
            foreach (var selectedTarget in targets)
            {
                if (selectedTarget is HumanoidIKClip clip && clip.EnsureDigitBendRangesInitialized())
                {
                    EditorUtility.SetDirty(clip);
                }
            }
        }

        void DuringSceneGUI(SceneView sceneView)
        {
            foreach (var selectedTarget in targets)
            {
                if (selectedTarget is not HumanoidIKClip clip ||
                    !HumanoidIKTimelineGizmoRegistry.IsSelectedAndVisible(clip))
                {
                    continue;
                }

                DrawSceneGUI(clip);
            }
        }

        void DrawSceneGUI(HumanoidIKClip clip)
        {
            if (!clip ||
                !TryGetTimelineContext(clip, out var director, out var track, out _) ||
                !_gizmoDrawer.TryResolveClipPreview(
                    clip,
                    director,
                    track,
                    1f,
                    out var pose))
            {
                return;
            }

            var drawPositionHandle = Tools.current == Tool.Move || Tools.current == Tool.Transform;
            var drawRotationHandle = Tools.current == Tool.Rotate || Tools.current == Tool.Transform;
            var rawEventType = Event.current?.rawType ?? EventType.Ignore;
            if (!drawRotationHandle ||
                (_rotationHandleDragActive && Tools.pivotRotation != _rotationHandleDragPivot) ||
                (_rotationHandleDragActive && GUIUtility.hotControl == 0 &&
                 rawEventType != EventType.MouseDrag && rawEventType != EventType.MouseUp))
            {
                ResetRotationHandleDrag();
            }

            if (!drawPositionHandle && !drawRotationHandle)
            {
                return;
            }

            var previousZTest = Handles.zTest;
            var previousColor = Handles.color;
            var previousMatrix = Handles.matrix;
            try
            {
                Handles.color = pose.GizmoColor;
                Handles.zTest = CompareFunction.Always;
                if (pose.HasBoundLimb && drawPositionHandle)
                {
                    EditorGUI.BeginChangeCheck();
                    var newBendTarget = Handles.PositionHandle(
                        pose.BendTarget,
                        GetToolHandleRotation(pose.TargetRotation));
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(clip, "Move Humanoid IK Bend Target");
                        SetClipWorldBendTarget(
                            clip,
                            pose.Anchor,
                            pose.Limb,
                            newBendTarget);
                        EditorUtility.SetDirty(clip);
                    }
                }

                // Target and explicit-anchor handles are the selected clip's interaction overlay.
                var editsAnchorTransform = pose.PositionFollowsAnchor && pose.Anchor;
                var handleRotation = editsAnchorTransform
                    ? pose.Anchor.rotation
                    : pose.TargetRotation;
                if (drawRotationHandle)
                {
                    EditorGUI.BeginChangeCheck();
                    var newRotation = DrawToolRotationHandle(
                        handleRotation,
                        pose.TargetPosition);
                    if (EditorGUI.EndChangeCheck())
                    {
                        var changedObject = editsAnchorTransform
                            ? (UnityEngine.Object)pose.Anchor
                            : clip;
                        Undo.RecordObject(
                            changedObject,
                            editsAnchorTransform
                                ? "Rotate Humanoid IK Anchor"
                                : "Rotate Humanoid IK Target");
                        SetHandleWorldRotation(
                            clip,
                            pose.Anchor,
                            pose.PositionFollowsAnchor,
                            newRotation);
                        MarkHandleTargetChanged(changedObject);
                        handleRotation = newRotation;
                    }
                }

                if (drawPositionHandle)
                {
                    EditorGUI.BeginChangeCheck();
                    var newPosition = Handles.PositionHandle(
                        pose.TargetPosition,
                        GetToolHandleRotation(handleRotation));
                    if (EditorGUI.EndChangeCheck())
                    {
                        var changedObject = editsAnchorTransform
                            ? (UnityEngine.Object)pose.Anchor
                            : clip;
                        Undo.RecordObject(
                            changedObject,
                            editsAnchorTransform
                                ? "Move Humanoid IK Anchor"
                                : "Move Humanoid IK Target");
                        SetHandleWorldPosition(
                            clip,
                            pose.Anchor,
                            pose.PositionFollowsAnchor,
                            newPosition);
                        MarkHandleTargetChanged(changedObject);
                    }
                }

                if (rawEventType == EventType.MouseUp)
                {
                    ResetRotationHandleDrag();
                }
            }
            finally
            {
                Handles.matrix = previousMatrix;
                Handles.zTest = previousZTest;
                Handles.color = previousColor;
            }
        }

        static Quaternion GetToolHandleRotation(Quaternion targetRotation)
        {
            return Tools.pivotRotation == PivotRotation.Local
                ? targetRotation
                : Quaternion.identity;
        }

        Quaternion DrawToolRotationHandle(Quaternion targetRotation, Vector3 targetPosition)
        {
            var pivot = Tools.pivotRotation;
            // Keep the handle and authored target fixed to the drag-start frame. Feeding
            // the clip rotation written on the previous GUI event back into an active
            // RotationHandle makes the same mouse delta compound on every repaint.
            var handleRotation = _rotationHandleDragActive
                ? _rotationHandleDragCurrent
                : GetToolHandleRotation(targetRotation);
            var nextHandleRotation = Handles.RotationHandle(handleRotation, targetPosition);
            if (Quaternion.Angle(handleRotation, nextHandleRotation) <= 0.0001f)
            {
                return _rotationHandleDragActive
                    ? ApplyRotationHandleDelta(
                        _rotationHandleDragStartTarget,
                        _rotationHandleDragStartHandle,
                        _rotationHandleDragCurrent)
                    : targetRotation;
            }

            if (!_rotationHandleDragActive)
            {
                _rotationHandleDragActive = true;
                _rotationHandleDragPivot = pivot;
                _rotationHandleDragStartTarget = targetRotation;
                _rotationHandleDragStartHandle = handleRotation;
            }

            _rotationHandleDragCurrent = nextHandleRotation;
            return ApplyRotationHandleDelta(
                _rotationHandleDragStartTarget,
                _rotationHandleDragStartHandle,
                _rotationHandleDragCurrent);
        }

        static Quaternion ApplyRotationHandleDelta(
            Quaternion targetRotationAtDragStart,
            Quaternion handleRotationAtDragStart,
            Quaternion currentHandleRotation)
        {
            var handleDelta = currentHandleRotation * Quaternion.Inverse(handleRotationAtDragStart);
            return handleDelta * targetRotationAtDragStart;
        }

        void ResetRotationHandleDrag()
        {
            _rotationHandleDragActive = false;
            _rotationHandleDragPivot = PivotRotation.Global;
            _rotationHandleDragStartTarget = Quaternion.identity;
            _rotationHandleDragStartHandle = Quaternion.identity;
            _rotationHandleDragCurrent = Quaternion.identity;
        }

        bool DrawGizmoColorField()
        {
            var primaryClip = (HumanoidIKClip)target;
            var color = primaryClip.GetGizmoColor(GetTrackTarget(primaryClip));
            var mixedValue = false;
            foreach (var selectedTarget in targets)
            {
                if (selectedTarget is not HumanoidIKClip selectedClip) continue;

                var selectedColor = selectedClip.GetGizmoColor(GetTrackTarget(selectedClip));
                if (selectedColor != color)
                {
                    mixedValue = true;
                    break;
                }
            }

            var previousMixedValue = EditorGUI.showMixedValue;
            Color nextColor;
            bool changed;
            try
            {
                EditorGUI.showMixedValue = mixedValue;
                EditorGUI.BeginChangeCheck();
                nextColor = EditorGUILayout.ColorField(
                    new GUIContent("Gizmo Color", "Color and opacity used by this clip's Scene view IK gizmo."),
                    color,
                    true,
                    true,
                    false);
                changed = EditorGUI.EndChangeCheck();
            }
            finally
            {
                EditorGUI.showMixedValue = previousMixedValue;
            }

            if (!changed) return false;

            Undo.RecordObjects(targets, "Change Humanoid IK Gizmo Color");
            foreach (var selectedTarget in targets)
            {
                if (selectedTarget is not HumanoidIKClip selectedClip) continue;

                selectedClip.SetGizmoColor(nextColor);
                EditorUtility.SetDirty(selectedClip);
            }

            return true;
        }

        void CaptureCurrentPose(
            HumanoidIKClip clip,
            HumanoidIKTarget target,
            Animator animator,
            Transform anchor,
            bool positionFollowsAnchor)
        {
            if (!HumanoidIKUtility.TryGetLimbBones(animator, target, out var limb)) return;
            if (!_gizmoDrawer.TryGetPreviewBoneToEffectorRotation(
                    animator,
                    target,
                    out var boneToEffectorRotation)) return;
            var targetWorldRotation = limb.End.rotation * boneToEffectorRotation;

            Undo.RecordObject(clip, "Capture Humanoid IK Pose");
            if (!positionFollowsAnchor)
            {
                SetClipWorldPosition(clip, anchor, limb.End.position);
            }
            SetClipWorldRotation(clip, anchor, targetWorldRotation);
            var forwardDir = anchor ? anchor.forward : (animator ? animator.transform.forward : Vector3.forward);
            SetClipWorldBendTarget(clip, anchor, limb, limb.Lower.position + forwardDir * 1.0f);
            EditorUtility.SetDirty(clip);
        }

        void SetClipWorldPosition(HumanoidIKClip clip, Transform anchor, Vector3 worldPosition)
        {
            clip.position = anchor ? anchor.InverseTransformPoint(worldPosition) : worldPosition;
        }

        void SetClipWorldRotation(HumanoidIKClip clip, Transform anchor, Quaternion worldRotation)
        {
            clip.SetTargetWorldRotation(anchor, worldRotation);
        }

        internal static void SetHandleWorldPosition(
            HumanoidIKClip clip,
            Transform anchor,
            bool positionFollowsAnchor,
            Vector3 worldPosition)
        {
            if (positionFollowsAnchor && anchor)
            {
                anchor.position = worldPosition;
                return;
            }

            clip.position = anchor ? anchor.InverseTransformPoint(worldPosition) : worldPosition;
        }

        internal static void SetHandleWorldRotation(
            HumanoidIKClip clip,
            Transform anchor,
            bool positionFollowsAnchor,
            Quaternion worldRotation)
        {
            if (positionFollowsAnchor && anchor)
            {
                anchor.rotation = worldRotation;
                return;
            }

            clip.SetTargetWorldRotation(anchor, worldRotation);
        }

        static void MarkHandleTargetChanged(UnityEngine.Object changedObject)
        {
            EditorUtility.SetDirty(changedObject);
            if (changedObject is Transform anchor &&
                PrefabUtility.IsPartOfPrefabInstance(anchor))
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(anchor);
            }
        }

        void SetClipWorldBendTarget(
            HumanoidIKClip clip,
            Transform anchor,
            HumanoidIKLimbBones limb,
            Vector3 worldPosition)
        {
            clip.SetHumanoidPoleWorldVector(anchor, worldPosition);
        }

        bool TryGetContext(
            HumanoidIKClip clip,
            out UnityEngine.Playables.PlayableDirector director,
            out HumanoidIKTrack track,
            out Animator animator,
            out Transform anchor)
        {
            animator = null;
            if (!TryGetTimelineContext(clip, out director, out track, out anchor)) return false;

            animator = director.GetGenericBinding(track) as Animator;
            return HumanoidIKUtility.IsUsableHumanoid(animator);
        }

        bool TryGetTimelineContext(
            HumanoidIKClip clip,
            out UnityEngine.Playables.PlayableDirector director,
            out HumanoidIKTrack track,
            out Transform anchor)
        {
            director = TimelineEditor.inspectedDirector;
            track = null;
            anchor = null;

            if (!clip || !director) return false;

            track = director.GetTrackOf<HumanoidIKTrack>(clip);
            if (!track) return false;

            HumanoidIKClipSpaceMigration.EnsureDirectorLocal(clip, director);
            anchor = clip.ResolveAnchor(director, director.transform);
            return true;
        }

        HumanoidIKTarget GetTrackTarget()
        {
            return target is HumanoidIKClip clip
                ? GetTrackTarget(clip)
                : HumanoidIKTarget.LeftHand;
        }

        static HumanoidIKTarget GetTrackTarget(HumanoidIKClip clip)
        {
            var director = TimelineEditor.inspectedDirector;
            if (!director || !clip) return HumanoidIKTarget.LeftHand;

            var track = director.GetTrackOf<HumanoidIKTrack>(clip);
            return track ? track.target : HumanoidIKTarget.LeftHand;
        }

        string GetDigitLabel()
        {
            return HumanoidIKUtility.IsHand(GetTrackTarget()) ? "Finger Bends" : "Toe Bends";
        }

        string GetFirstDigitLabel()
        {
            return HumanoidIKUtility.IsHand(GetTrackTarget()) ? "Thumb" : "Big Toe";
        }
    }
}
