using System.Collections.Generic;
using CutsceneEngine;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.UIElements;
using static CutsceneEngineEditor.HumanoidIKDigitPoseUtility;

namespace CutsceneEngineEditor
{
    [Overlay(typeof(SceneView), "CutsceneEngine.HumanoidIKControls", "Humanoid IK", false)]
    public class HumanoidIKSceneOverlay : Overlay, ITransientOverlay
    {
        const float PoseMin = -1f;
        const float PoseMax = 1f;
        const float HandCanvasWidth = 400f;
        const float HandCanvasHeight = 160f;
        const float HandImageWidth = 320f;
        const float FootCanvasWidth = 400f;
        const float FootCanvasHeight = 160f;
        const float FootImageWidth = 320f;
        const float OverallSliderWidth = 60f;
        const float JointSliderLength = 54f;
        const float SparseToeSliderX = 208f;
        const float SparseToeSliderLength = 108f;
        const float ToeBaseSliderX = 198f;
        const float ToeBaseSliderY = 75f;
        const float ToeBaseSliderWidth = 54f;
        const float ToeBaseSliderHeight = 14f;
        const float ToeJointRegionX = 256f;
        const float ToeJointRegionRightBig = 316f;
        const float ToeJointRegionRightLittle = 306f;
        const float ToeJointSliderGap = 3f;
        const float TopSliderLabelWidth = 150f;
        const float TopSliderFieldWidth = 50f;

        static readonly string[] HandDigitLabels =
        {
            "Thumb",
            "Index",
            "Middle",
            "Ring",
            "Little"
        };

        static readonly string[] ToeDigitLabels =
        {
            "Big Toe",
            "Second Toe",
            "Third Toe",
            "Fourth Toe",
            "Fifth Toe"
        };

        static readonly float[] ToeDiagramCenterY = { 31f, 57f, 82f, 107f, 132f };

        static bool timelineRefreshPending;
        static bool stretchDragActive;
        static HumanoidIKClip stretchDragClip;
        static float stretchDragValue;
        public bool visible => TryGetSelectedContext(out _, out _, out _);

        public HumanoidIKSceneOverlay()
        {
            minSize = new Vector2(356f, 295f);
            maxSize = new Vector2(500f, 440f);
            defaultSize = minSize;
        }

        public override void OnWillBeDestroyed()
        {
            EndStretchDrag();
            FlushPendingTimelineRefresh();
            base.OnWillBeDestroyed();
        }

        public override VisualElement CreatePanelContent()
        {
            var root = new VisualElement();
            root.style.minWidth = 356f;
            root.style.flexGrow = 1f;
            root.style.paddingLeft = 10f;
            root.style.paddingRight = 10f;

            var imgui = new IMGUIContainer(DrawPanel);
            imgui.style.flexGrow = 1f;
            root.Add(imgui);
            return root;
        }

        void DrawPanel()
        {
            // EditorGUI.Slider consumes mouse events, so capture the raw event before drawing it.
            var panelRawEventType = Event.current?.rawType ?? EventType.Ignore;
            EditorGUILayout.Space(4f);

            if (!TryGetSelectedContext(out var timelineClip, out var clip, out var track))
            {
                EndStretchDrag();
                FlushPendingTimelineRefresh();
                return;
            }

            EditorGUILayout.LabelField($"{GetTargetLabel(track.target)} IK", EditorStyles.boldLabel);
            DrawClipHeader(timelineClip, clip, track, panelRawEventType);

            DrawWeightSlider(clip, panelRawEventType);

            if (HumanoidIKUtility.IsHand(track.target))
            {
                DrawStretchSlider(clip, true, null, false, panelRawEventType);
                DrawFingerFanSlider(clip, panelRawEventType);
                DrawResetBendsButton(clip, true);
                DrawHandDiagram(clip, track.target == HumanoidIKTarget.RightHand);
                FinishMouseInteractionForEvent(panelRawEventType);
                return;
            }

            var animator = TimelineEditor.inspectedDirector
                ? TimelineEditor.inspectedDirector.GetGenericBinding(track) as Animator
                : null;
            var toeRigKind = HumanoidIKUtility.GetToeRigKind(animator, track.target);
            var toeChains = HumanoidIKDigitChainCache.GetChains(animator, track.target);
            var hasArticulatedToeBase = toeRigKind == HumanoidIKToeRigKind.ArticulatedToes;
            DrawStretchSlider(
                clip,
                false,
                toeChains,
                hasArticulatedToeBase,
                panelRawEventType);
            DrawToeFanSlider(clip, panelRawEventType);
            DrawResetBendsButton(clip, false);

            DrawFootDiagram(
                clip,
                track.target == HumanoidIKTarget.RightFoot,
                toeRigKind,
                toeChains);

            FinishMouseInteractionForEvent(panelRawEventType);
        }

        static void DrawResetBendsButton(HumanoidIKClip clip, bool isHand)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (!GUILayout.Button("Reset Bends", EditorStyles.miniButton, GUILayout.Width(88f))) return;

                Undo.RecordObject(clip, isHand
                    ? "Reset Humanoid IK Finger Bends"
                    : "Reset Humanoid IK Toe Bends");
                clip.digitBends = default;
                if (!isHand)
                {
                    clip.toeBaseBend = 0f;
                    clip.toeFan = 0f;
                }
                MarkClipChanged(clip);
            }
        }

        static void DrawClipHeader(
            TimelineClip timelineClip,
            HumanoidIKClip clip,
            HumanoidIKTrack track,
            EventType rawEventType)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                var clipNameContent = new GUIContent(timelineClip.displayName);
                var clipNameWidth = EditorStyles.miniLabel.CalcSize(clipNameContent).x;
                GUILayout.Label(
                    clipNameContent,
                    EditorStyles.miniLabel,
                    GUILayout.Width(clipNameWidth));

                EditorGUI.BeginChangeCheck();
                var nextColor = EditorGUILayout.ColorField(
                    new GUIContent(string.Empty, "Gizmo Color and Opacity"),
                    clip.GetGizmoColor(track.target),
                    true,
                    true,
                    false,
                    GUILayout.Width(48f));
                if (!EditorGUI.EndChangeCheck()) return;

                Undo.RecordObject(clip, "Change Humanoid IK Gizmo Color");
                clip.SetGizmoColor(nextColor);
                MarkClipChanged(clip, rawEventType);
            }
        }

        static bool TryGetSelectedContext(out TimelineClip timelineClip, out HumanoidIKClip clip, out HumanoidIKTrack track)
        {
            timelineClip = TimelineEditor.selectedClip;
            clip = null;
            track = null;

            if (timelineClip == null) return false;

            clip = timelineClip.asset as HumanoidIKClip;
            if (!clip) return false;

            track = timelineClip.GetParentTrack() as HumanoidIKTrack;
            if (!track && TimelineEditor.inspectedDirector)
            {
                track = TimelineEditor.inspectedDirector.GetTrackOf<HumanoidIKTrack>(clip);
            }

            return track;
        }

        static void DrawWeightSlider(HumanoidIKClip clip, EventType rawEventType)
        {
            var mixedValue = !Mathf.Approximately(clip.positionWeight, clip.rotationWeight) ||
                             !Mathf.Approximately(clip.positionWeight, clip.bendWeight) ||
                             !Mathf.Approximately(clip.positionWeight, clip.digitWeight);
            var value = (clip.positionWeight + clip.rotationWeight + clip.bendWeight + clip.digitWeight) * 0.25f;
            DrawSlider(
                clip,
                "Weight",
                value,
                0f,
                1f,
                "Adjust Humanoid IK Weight",
                nextValue =>
                {
                    nextValue = Mathf.Clamp01(nextValue);
                    clip.positionWeight = nextValue;
                    clip.rotationWeight = nextValue;
                    clip.bendWeight = nextValue;
                    clip.digitWeight = nextValue;
                },
                rawEventType,
                mixedValue);
        }

        static void DrawStretchSlider(
            HumanoidIKClip clip,
            bool isHand,
            IReadOnlyList<Transform[]> toeChains,
            bool includeToeBase,
            EventType rawEventType)
        {
            if (stretchDragActive && stretchDragClip != clip)
            {
                EndStretchDrag();
                FlushPendingTimelineRefresh();
            }

            var value = GetStretchSliderValue(clip, isHand, toeChains, includeToeBase);

            EditorGUI.BeginChangeCheck();
            var nextValue = DrawStableSlider("Stretch", value, PoseMin, PoseMax);
            if (!EditorGUI.EndChangeCheck()) return;

            if (!stretchDragActive && IsMouseInteractionInProgress(rawEventType))
            {
                stretchDragActive = true;
                stretchDragClip = clip;
            }

            if (stretchDragActive && stretchDragClip == clip)
            {
                stretchDragValue = nextValue;
            }

            Undo.RecordObject(clip, isHand
                ? "Adjust Humanoid IK Stretch"
                : "Adjust Humanoid IK Toe Stretch");
            if (isHand)
            {
                SetHandStretchPose(clip, nextValue);
            }
            else
            {
                SetToeStretchPose(clip, toeChains, includeToeBase, nextValue);
            }
            MarkClipChanged(clip, rawEventType);
        }

        internal static void SetHandStretchPose(HumanoidIKClip clip, float value)
        {
            var pose = clip.digitBends;
            for (var i = 0; i < HandDigitLabels.Length; i++)
            {
                var bend = GetDigitBend(in pose, i);
                SetAllJointPose(clip, ref bend, i, value);
                SetDigitBend(ref pose, i, bend);
            }

            clip.digitBends = pose;
        }

        internal static void SetToeStretchPose(
            HumanoidIKClip clip,
            IReadOnlyList<Transform[]> chains,
            bool includeToeBase,
            float value)
        {
            if (chains == null) return;

            value = Mathf.Clamp(value, PoseMin, PoseMax);
            if (includeToeBase)
            {
                clip.toeBaseBend = value;
            }

            var pose = clip.digitBends;
            var rowCount = Mathf.Min(ToeDigitLabels.Length, chains.Count);
            for (var digitIndex = 0; digitIndex < rowCount; digitIndex++)
            {
                var jointCount = GetExistingJointCount(chains[digitIndex]);
                if (jointCount <= 0) continue;

                var bend = GetDigitBend(in pose, digitIndex);
                SetToeAllJointPose(clip, ref bend, jointCount, value);
                SetDigitBend(ref pose, digitIndex, bend);
            }

            clip.digitBends = pose;
        }

        static void DrawFingerFanSlider(HumanoidIKClip clip, EventType rawEventType)
        {
            DrawSlider(
                clip,
                "Finger Fan",
                GetFingerFanPose(clip, in clip.digitBends),
                PoseMin,
                PoseMax,
                "Adjust Humanoid IK Finger Fan",
                value => SetFingerFanPose(clip, value),
                rawEventType);
        }

        static void DrawToeFanSlider(HumanoidIKClip clip, EventType rawEventType)
        {
            DrawSlider(
                clip,
                "Toe Fan",
                clip.toeFan,
                PoseMin,
                PoseMax,
                "Adjust Humanoid IK Toe Fan",
                value => clip.toeFan = Mathf.Clamp(value, PoseMin, PoseMax),
                rawEventType);
        }

        static void SetFingerFanPose(HumanoidIKClip clip, float value)
        {
            var pose = clip.digitBends;
            for (var digitIndex = 1; digitIndex < HandDigitLabels.Length; digitIndex++)
            {
                var bend = GetDigitBend(in pose, digitIndex);
                bend.proximal.y = GetFingerSpreadAngleFromPose(clip, value, digitIndex);
                SetDigitBend(ref pose, digitIndex, bend);
            }

            clip.digitBends = pose;
        }

        static bool IsMouseInteractionInProgress(EventType rawEventType)
        {
            return rawEventType == EventType.MouseDown ||
                   rawEventType == EventType.MouseDrag;
        }

        static void FinishMouseInteractionForEvent(EventType rawType)
        {
            // UI Toolkit can transiently clear IMGUI hotControl and emit Ignore while
            // the pointer is still held. Only the actual MouseUp event commits this
            // drag transaction and releases the latched authoring value.
            if (rawType != EventType.MouseUp) return;

            if (stretchDragActive) EndStretchDrag();
            FlushPendingTimelineRefresh();
        }

        static void EndStretchDrag()
        {
            stretchDragActive = false;
            stretchDragClip = null;
            stretchDragValue = 0f;
        }

        void DrawHandDiagram(HumanoidIKClip clip, bool mirror)
        {
            var canvasRect = GUILayoutUtility.GetRect(
                HandCanvasWidth,
                HandCanvasHeight,
                GUILayout.ExpandWidth(true));

            var scale = Mathf.Min(
                canvasRect.width / HandCanvasWidth,
                canvasRect.height / HandCanvasHeight);
            var origin = new Vector2(
                canvasRect.x + (canvasRect.width - HandCanvasWidth * scale) * 0.5f,
                canvasRect.y + (canvasRect.height - HandCanvasHeight * scale) * 0.5f);

            DrawHandTexture(origin, scale, mirror);

            DrawThumbDiagramRow(clip, origin, scale, mirror);
            DrawDigitDiagramRow(clip, 1, origin, scale, mirror, 54f);
            DrawDigitDiagramRow(clip, 2, origin, scale, mirror, 82f);
            DrawDigitDiagramRow(clip, 3, origin, scale, mirror, 111f);
            DrawDigitDiagramRow(clip, 4, origin, scale, mirror, 139f);
            DrawThumbSpreadSlider(clip, origin, scale, mirror);
        }

        static void DrawFootDiagram(
            HumanoidIKClip clip,
            bool mirror,
            HumanoidIKToeRigKind rigKind,
            IReadOnlyList<Transform[]> chains)
        {
            var canvasRect = GUILayoutUtility.GetRect(
                FootCanvasWidth,
                FootCanvasHeight,
                GUILayout.ExpandWidth(true));

            var scale = Mathf.Min(
                canvasRect.width / FootCanvasWidth,
                canvasRect.height / FootCanvasHeight);
            var origin = new Vector2(
                canvasRect.x + (canvasRect.width - FootCanvasWidth * scale) * 0.5f,
                canvasRect.y + (canvasRect.height - FootCanvasHeight * scale) * 0.5f);

            DrawFootTexture(origin, scale, mirror);

            if (rigKind == HumanoidIKToeRigKind.None || chains == null || chains.Count == 0) return;

            if (rigKind == HumanoidIKToeRigKind.ArticulatedToes)
            {
                DrawToeBaseDiagramSlider(clip, origin, scale, mirror);
            }

            var useWideSliders = GetVisibleToeSliderCount(rigKind, chains) <= 2;
            if (rigKind == HumanoidIKToeRigKind.ToeFoot)
            {
                DrawToeDiagramRow(
                    clip,
                    0,
                    chains[0],
                    "Toe-Foot",
                    origin,
                    scale,
                    mirror,
                    82f,
                    true,
                    useWideSliders);
                return;
            }

            var rowCount = Mathf.Min(ToeDigitLabels.Length, chains.Count);
            for (var digitIndex = 0; digitIndex < rowCount; digitIndex++)
            {
                DrawToeDiagramRow(
                    clip,
                    digitIndex,
                    chains[digitIndex],
                    ToeDigitLabels[digitIndex],
                    origin,
                    scale,
                    mirror,
                    ToeDiagramCenterY[digitIndex],
                    false,
                    useWideSliders);
            }
        }

        static int GetVisibleToeSliderCount(
            HumanoidIKToeRigKind rigKind,
            IReadOnlyList<Transform[]> chains)
        {
            if (chains == null) return 0;

            var rowCount = rigKind == HumanoidIKToeRigKind.ToeFoot
                ? Mathf.Min(1, chains.Count)
                : Mathf.Min(ToeDigitLabels.Length, chains.Count);
            var sliderCount = 0;
            for (var digitIndex = 0; digitIndex < rowCount; digitIndex++)
            {
                var jointCount = GetExistingJointCount(chains[digitIndex]);
                sliderCount += jointCount;
                if (jointCount > 1)
                {
                    sliderCount++;
                }
            }

            return sliderCount;
        }

        static void DrawToeDiagramRow(
            HumanoidIKClip clip,
            int digitIndex,
            Transform[] chain,
            string label,
            Vector2 origin,
            float scale,
            bool mirror,
            float centerY,
            bool simpleToeFoot,
            bool useWideSliders)
        {
            var jointCount = GetExistingJointCount(chain);
            if (jointCount <= 0) return;

            var bend = GetDigitBend(in clip.digitBends, digitIndex);
            if (jointCount > 1)
            {
                DrawCompactSlider(
                    clip,
                    GetFootCanvasRect(origin, scale, mirror, 330f, centerY - 7f, OverallSliderWidth, 14f),
                    GetToeDigitPose(bend, jointCount),
                    PoseMin,
                    PoseMax,
                    $"{label} Curl",
                    "Adjust Humanoid IK Toe Curl",
                    value =>
                    {
                        var pose = clip.digitBends;
                        var nextBend = GetDigitBend(in pose, digitIndex);
                        SetToeAllJointPose(ref nextBend, jointCount, value);
                        SetDigitBend(ref pose, digitIndex, nextBend);
                        clip.digitBends = pose;
                    });
            }

            for (var jointIndex = 0; jointIndex < jointCount; jointIndex++)
            {
                var capturedJointIndex = jointIndex;
                float x;
                float sliderLength;
                if (useWideSliders)
                {
                    x = SparseToeSliderX;
                    sliderLength = SparseToeSliderLength;
                }
                else if (simpleToeFoot)
                {
                    x = 240f + jointIndex * 58f;
                    sliderLength = JointSliderLength;
                }
                else
                {
                    GetArticulatedToeSliderLayout(
                        digitIndex,
                        jointIndex,
                        jointCount,
                        out x,
                        out sliderLength);
                }
                DrawCompactSlider(
                    clip,
                    GetFootCanvasRect(origin, scale, mirror, x, centerY - 7f, sliderLength, 14f),
                    GetToeJointPose(clip, bend, jointIndex),
                    PoseMin,
                    PoseMax,
                    $"{label} Joint {jointIndex + 1}",
                    "Adjust Humanoid IK Toe Joint",
                    value =>
                    {
                        var pose = clip.digitBends;
                        var nextBend = GetDigitBend(in pose, digitIndex);
                        SetToeJointPose(clip, ref nextBend, capturedJointIndex, value);
                        SetDigitBend(ref pose, digitIndex, nextBend);
                        clip.digitBends = pose;
                    });
            }
        }

        static void DrawToeBaseDiagramSlider(
            HumanoidIKClip clip,
            Vector2 origin,
            float scale,
            bool mirror)
        {
            var rect = GetFootCanvasRect(
                origin,
                scale,
                mirror,
                ToeBaseSliderX,
                ToeBaseSliderY,
                ToeBaseSliderWidth,
                ToeBaseSliderHeight);

            EditorGUI.BeginChangeCheck();
            var nextValue = GUI.HorizontalSlider(rect, clip.toeBaseBend, PoseMin, PoseMax);
            GUI.Label(
                rect,
                new GUIContent(string.Empty, $"Toe Bone Bend: {nextValue:0.00}"),
                GUIStyle.none);
            if (!EditorGUI.EndChangeCheck()) return;

            Undo.RecordObject(clip, "Adjust Humanoid IK Toe Bone Bend");
            clip.toeBaseBend = Mathf.Clamp(nextValue, PoseMin, PoseMax);
            MarkClipChanged(clip);
        }

        internal static Rect GetToeBaseSliderCanvasRect(bool mirror)
        {
            return GetFootCanvasRect(
                Vector2.zero,
                1f,
                mirror,
                ToeBaseSliderX,
                ToeBaseSliderY,
                ToeBaseSliderWidth,
                ToeBaseSliderHeight);
        }

        static void GetArticulatedToeSliderLayout(
            int digitIndex,
            int jointIndex,
            int jointCount,
            out float x,
            out float width)
        {
            var normalizedDigit = Mathf.Clamp(digitIndex, 0, 4) / 4f;
            var regionRight = Mathf.Lerp(
                ToeJointRegionRightBig,
                ToeJointRegionRightLittle,
                normalizedDigit);
            var clampedJointCount = Mathf.Clamp(jointCount, 1, 3);
            var totalGap = ToeJointSliderGap * (clampedJointCount - 1);
            width = (regionRight - ToeJointRegionX - totalGap) / clampedJointCount;
            x = ToeJointRegionX + Mathf.Clamp(jointIndex, 0, clampedJointCount - 1) *
                (width + ToeJointSliderGap);
        }

        static void DrawHandTexture(Vector2 origin, float scale, bool mirror)
        {
            if (Event.current.type != EventType.Repaint) return;

            var texture = HumanoidIKDiagramTextures.GetHand(mirror);
            if (!texture) return;

            var imageRect = new Rect(
                origin.x + (mirror ? HandCanvasWidth - HandImageWidth : 0f) * scale,
                origin.y,
                HandImageWidth * scale,
                HandCanvasHeight * scale);
            GUI.DrawTexture(imageRect, texture, ScaleMode.StretchToFill, true);
        }

        static void DrawFootTexture(Vector2 origin, float scale, bool mirror)
        {
            if (Event.current.type != EventType.Repaint) return;

            var texture = HumanoidIKDiagramTextures.GetFoot(mirror);
            if (!texture) return;

            var imageRect = new Rect(
                origin.x + (mirror ? FootCanvasWidth - FootImageWidth : 0f) * scale,
                origin.y,
                FootImageWidth * scale,
                FootCanvasHeight * scale);
            GUI.DrawTexture(imageRect, texture, ScaleMode.StretchToFill, true);
        }

        static void DrawThumbDiagramRow(
            HumanoidIKClip clip,
            Vector2 origin,
            float scale,
            bool mirror)
        {
            var bend = GetDigitBend(in clip.digitBends, 0);
            DrawCompactSlider(
                clip,
                GetCanvasRect(origin, scale, mirror, 330f, 18f, OverallSliderWidth, 14f),
                GetDigitPose(clip, bend, 0),
                PoseMin,
                PoseMax,
                "Thumb Curl",
                "Adjust Humanoid IK Finger Curl",
                value =>
                {
                    var pose = clip.digitBends;
                    var nextBend = GetDigitBend(in pose, 0);
                    SetAllJointPose(clip, ref nextBend, 0, value);
                    SetDigitBend(ref pose, 0, nextBend);
                    clip.digitBends = pose;
                });

            DrawJointDiagramSlider(clip, 0, 0, origin, scale, mirror, 90f, 25f, JointSliderLength);
            DrawJointDiagramSlider(clip, 0, 1, origin, scale, mirror, 154f, 25f, JointSliderLength);
            DrawJointDiagramSlider(clip, 0, 2, origin, scale, mirror, 220f, 25f, JointSliderLength);
        }

        static void DrawDigitDiagramRow(
            HumanoidIKClip clip,
            int digitIndex,
            Vector2 origin,
            float scale,
            bool mirror,
            float centerY)
        {
            var bend = GetDigitBend(in clip.digitBends, digitIndex);
            DrawCompactSlider(
                clip,
                GetCanvasRect(origin, scale, mirror, 330f, centerY - 7f, OverallSliderWidth, 14f),
                GetDigitPose(clip, bend, digitIndex),
                PoseMin,
                PoseMax,
                $"{HandDigitLabels[digitIndex]} Curl",
                "Adjust Humanoid IK Finger Curl",
                value =>
                {
                    var pose = clip.digitBends;
                    var nextBend = GetDigitBend(in pose, digitIndex);
                    SetAllJointPose(clip, ref nextBend, digitIndex, value);
                    SetDigitBend(ref pose, digitIndex, nextBend);
                    clip.digitBends = pose;
                });

            DrawJointDiagramSlider(clip, digitIndex, 0, origin, scale, mirror, 112f, centerY, JointSliderLength);
            DrawJointDiagramSlider(clip, digitIndex, 1, origin, scale, mirror, 176f, centerY, JointSliderLength);
            DrawJointDiagramSlider(clip, digitIndex, 2, origin, scale, mirror, 240f, centerY, JointSliderLength);
        }

        static void DrawJointDiagramSlider(
            HumanoidIKClip clip,
            int digitIndex,
            int jointIndex,
            Vector2 origin,
            float scale,
            bool mirror,
            float x,
            float centerY,
            float sliderWidth)
        {
            var bend = GetDigitBend(in clip.digitBends, digitIndex);
            DrawCompactSlider(
                clip,
                GetCanvasRect(origin, scale, mirror, x, centerY - 7f, sliderWidth, 14f),
                GetJointPose(clip, bend, digitIndex, jointIndex),
                PoseMin,
                PoseMax,
                $"{HandDigitLabels[digitIndex]} Joint {jointIndex + 1}",
                "Adjust Humanoid IK Finger Joint",
                value =>
                {
                    var pose = clip.digitBends;
                    var nextBend = GetDigitBend(in pose, digitIndex);
                    SetJointPose(clip, ref nextBend, digitIndex, jointIndex, value);
                    SetDigitBend(ref pose, digitIndex, nextBend);
                    clip.digitBends = pose;
                });
        }

        static void DrawThumbSpreadSlider(
            HumanoidIKClip clip,
            Vector2 origin,
            float scale,
            bool mirror)
        {
            var rect = GetCanvasRect(origin, scale, mirror, 52f, 43f, 14f, 52f);

            EditorGUI.BeginChangeCheck();
            var sliderRange = GetThumbSpreadVerticalSliderRange(clip);
            var nextValue = GUI.VerticalSlider(
                rect,
                clip.digitBends.thumbOrBigToe.proximal.y,
                sliderRange.x,
                sliderRange.y);
            GUI.Label(
                rect,
                new GUIContent(string.Empty, $"Thumb Spread: {nextValue:0.0}°"),
                GUIStyle.none);
            if (!EditorGUI.EndChangeCheck()) return;

            Undo.RecordObject(clip, "Adjust Humanoid IK Thumb Spread");
            var pose = clip.digitBends;
            var thumb = pose.thumbOrBigToe;
            thumb.proximal.y = nextValue;
            pose.thumbOrBigToe = thumb;
            clip.digitBends = pose;
            MarkClipChanged(clip);
        }

        internal static Vector2 GetThumbSpreadVerticalSliderRange(HumanoidIKClip clip)
        {
            var spreadRange = GetThumbSpreadRange(clip);
            return new Vector2(spreadRange.y, spreadRange.x);
        }

        static void DrawCompactSlider(
            HumanoidIKClip clip,
            Rect rect,
            float value,
            float min,
            float max,
            string tooltipLabel,
            string undoName,
            System.Action<float> apply)
        {
            EditorGUI.BeginChangeCheck();
            var nextValue = GUI.HorizontalSlider(rect, value, min, max);
            GUI.Label(
                rect,
                new GUIContent(string.Empty, $"{tooltipLabel}: {nextValue:0.00}"),
                GUIStyle.none);
            if (!EditorGUI.EndChangeCheck()) return;

            Undo.RecordObject(clip, undoName);
            apply(nextValue);
            MarkClipChanged(clip);
        }

        static Rect GetCanvasRect(
            Vector2 origin,
            float scale,
            bool mirror,
            float x,
            float y,
            float width,
            float height)
        {
            return GetDiagramRect(HandCanvasWidth, origin, scale, mirror, x, y, width, height);
        }

        static Rect GetFootCanvasRect(
            Vector2 origin,
            float scale,
            bool mirror,
            float x,
            float y,
            float width,
            float height)
        {
            return GetDiagramRect(FootCanvasWidth, origin, scale, mirror, x, y, width, height);
        }

        static Rect GetDiagramRect(
            float canvasWidth,
            Vector2 origin,
            float scale,
            bool mirror,
            float x,
            float y,
            float width,
            float height)
        {
            if (mirror)
            {
                x = canvasWidth - x - width;
            }

            return new Rect(
                origin.x + x * scale,
                origin.y + y * scale,
                width * scale,
                height * scale);
        }

        static void DrawSlider(
            HumanoidIKClip clip,
            string label,
            float value,
            float min,
            float max,
            string undoName,
            System.Action<float> apply,
            EventType rawEventType,
            bool mixedValue = false)
        {
            var previousMixedValue = EditorGUI.showMixedValue;
            float nextValue;
            bool changed;
            try
            {
                EditorGUI.showMixedValue = mixedValue;
                EditorGUI.BeginChangeCheck();
                nextValue = DrawStableSlider(label, value, min, max);
                changed = EditorGUI.EndChangeCheck();
            }
            finally
            {
                EditorGUI.showMixedValue = previousMixedValue;
            }

            if (!changed) return;

            Undo.RecordObject(clip, undoName);
            apply(nextValue);
            MarkClipChanged(clip, rawEventType);
        }

        static float DrawStableSlider(string label, float value, float min, float max)
        {
            // EditorGUILayout.Slider derives its track from shared EditorGUIUtility layout state.
            // Timeline and Inspector repaints can change that state between IMGUIContainer drag
            // events. Reserve an explicit row and pin every width used by the compound slider.
            var position = GUILayoutUtility.GetRect(
                1f,
                EditorGUIUtility.singleLineHeight,
                GUILayout.ExpandWidth(true));
            var previousWideMode = EditorGUIUtility.wideMode;
            var previousLabelWidth = EditorGUIUtility.labelWidth;
            var previousFieldWidth = EditorGUIUtility.fieldWidth;

            try
            {
                EditorGUIUtility.wideMode = true;
                EditorGUIUtility.labelWidth = TopSliderLabelWidth;
                EditorGUIUtility.fieldWidth = TopSliderFieldWidth;
                return EditorGUI.Slider(position, label, value, min, max);
            }
            finally
            {
                EditorGUIUtility.wideMode = previousWideMode;
                EditorGUIUtility.labelWidth = previousLabelWidth;
                EditorGUIUtility.fieldWidth = previousFieldWidth;
            }
        }

        static float GetStretchSliderValue(
            HumanoidIKClip clip,
            bool isHand,
            IReadOnlyList<Transform[]> toeChains,
            bool includeToeBase)
        {
            return stretchDragActive && stretchDragClip == clip
                ? stretchDragValue
                : isHand
                    ? GetHandStretch(clip, in clip.digitBends)
                    : GetToeStretch(
                        clip,
                        in clip.digitBends,
                        toeChains,
                        includeToeBase,
                        clip.toeBaseBend);
        }

        static string GetTargetLabel(HumanoidIKTarget target)
        {
            return target switch
            {
                HumanoidIKTarget.LeftHand => "Left Hand",
                HumanoidIKTarget.RightHand => "Right Hand",
                HumanoidIKTarget.LeftFoot => "Left Foot",
                HumanoidIKTarget.RightFoot => "Right Foot",
                _ => target.ToString()
            };
        }

        static void MarkClipChanged(HumanoidIKClip clip)
        {
            var rawEventType = Event.current?.rawType ?? EventType.Ignore;
            MarkClipChanged(clip, rawEventType);
        }

        static void MarkClipChanged(HumanoidIKClip clip, EventType rawEventType)
        {
            EditorUtility.SetDirty(clip);
            // A Timeline refresh rebuilds the preview driver. Keep the clip-driven gizmo live,
            // but defer that rebuild until the active mouse interaction explicitly ends.
            if (stretchDragActive || IsMouseInteractionInProgress(rawEventType))
            {
                timelineRefreshPending = true;
            }
            else
            {
                timelineRefreshPending = false;
                RefreshTimeline();
            }

            SceneView.RepaintAll();
        }

        static void FlushPendingTimelineRefresh()
        {
            if (!timelineRefreshPending) return;

            timelineRefreshPending = false;
            RefreshTimeline();
        }

        static void RefreshTimeline()
        {
            TimelineEditor.Refresh(RefreshReason.ContentsModified);
        }
    }
}
