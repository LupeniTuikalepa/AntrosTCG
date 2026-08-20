using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace CutsceneEngine
{
    [Serializable]
    public class HumanoidIKClip : PlayableAsset, ITimelineClipAsset
    {
        const int CurrentFingerSpreadRangesVersion = 2;
        internal const int CurrentFootRotationFrameVersion = 1;
        internal const string FootRotationFrameVersionFieldName = nameof(footRotationFrameVersion);

        [Tooltip("Optional target anchor. When assigned, the IK position follows this transform's world position exactly. When unassigned, Position is relative to the owning PlayableDirector.")]
        public ExposedReference<Transform> anchorTransform;

        [SerializeField, HideInInspector]
        bool useDirectorTransformAsDefaultAnchor;

        [SerializeField]
        [Tooltip("Color and opacity used by this clip's Scene view IK gizmo.")]
        Color gizmoColor = new Color(1f, 0.05f, 0.04f, 0.8f);

        [SerializeField, HideInInspector]
        bool gizmoColorInitialized;

        [Tooltip("PlayableDirector-local position used only when Anchor Transform is unassigned. An explicit Anchor Transform supplies its own world position.")]
        public Vector3 position;

        [Tooltip("Anchor-local anatomical target Euler rotation. Hand tracks store the palm effector frame; foot tracks store the canonical sole frame (+Z toes, +Y dorsum). Without an explicit anchor, this is stored relative to the owning PlayableDirector.")]
        public Vector3 rotation;

        [SerializeField, HideInInspector]
        HumanoidIKRotationSpace rotationSpace;

        [SerializeField, HideInInspector]
        int footRotationFrameVersion;

        [Tooltip("Anchor-local Humanoid pole vector. Its raw magnitude is preserved for Inspector and Scene handle placement; runtime IK normalizes only when resolving the bend direction. Without an explicit anchor, this is stored relative to the owning PlayableDirector. Legacy clips may still store a bend target point until converted.")]
        public Vector3 bendTarget = new Vector3(0f, 0.5f, 1.2f);

        [SerializeField, HideInInspector]
        HumanoidIKBendSpace bendSpace;

        [Range(0f, 1f)]
        [Tooltip("How strongly the limb end follows Position.")]
        public float positionWeight = 1f;

        [Range(0f, 1f)]
        [Tooltip("How strongly the limb end follows Rotation.")]
        public float rotationWeight = 1f;

        [Range(0f, 1f)]
        [Tooltip("How strongly Bend Target controls the elbow or knee plane.")]
        public float bendWeight = 1f;

        [Range(0f, 1f)]
        [Tooltip("How strongly Finger/Toe Bends are applied.")]
        public float digitWeight = 1f;

        [Tooltip("Hand X values are absolute Humanoid Stretched muscle angles in degrees; proximal Y values are absolute Spread muscle angles. Toe X/Y/Z values are anatomical bend/fan/roll angles resolved from each reference toe direction and the sole normal, not imported bone-local Euler axes.")]
        public HumanoidIKDigitBendPose digitBends;

        [Range(-1f, 1f)]
        [Tooltip("Bend applied to the mapped Humanoid Toes root before the five articulated toe branches. Simple Foot-Toe rigs use their existing first toe joint control instead.")]
        public float toeBaseBend;

        [Range(-1f, 1f)]
        [Tooltip("Collective toe fan for articulated multi-toe rigs. Stored but intentionally ignored by simple Foot-Toe rigs.")]
        public float toeFan;

        [HideInInspector]
        public Vector2[] digitBendRanges;

        [HideInInspector]
        public Vector2 thumbSpreadRange;

        [HideInInspector]
        public Vector2[] fingerSpreadRanges;

        [HideInInspector]
        public Vector2[] toeBendRanges;

        [HideInInspector]
        public Vector2 toeBaseBendRange;

        [SerializeField, HideInInspector]
        bool digitBendRangesInitialized;

        [SerializeField, HideInInspector]
        bool thumbSpreadRangeInitialized;

        [SerializeField, HideInInspector]
        bool fingerSpreadRangesInitialized;

        [SerializeField, HideInInspector]
        bool toeBendRangesInitialized;

        [SerializeField, HideInInspector]
        int fingerSpreadRangesVersion;

        public ClipCaps clipCaps => ClipCaps.Blending | ClipCaps.Extrapolation;
        public HumanoidIKRotationSpace RotationSpace => rotationSpace;
        public HumanoidIKBendSpace BendSpace => bendSpace;
        public bool UsesHumanoidEffectorRotation => rotationSpace == HumanoidIKRotationSpace.HumanoidEffector;
        public bool UsesHumanoidPoleDirection => bendSpace == HumanoidIKBendSpace.HumanoidPoleDirection;
        internal bool UsesDirectorTransformAsDefaultAnchor => useDirectorTransformAsDefaultAnchor;
        internal int FootRotationFrameVersion => footRotationFrameVersion;
        internal bool UsesProjectedSoleRotation =>
            footRotationFrameVersion >= CurrentFootRotationFrameVersion;

        public Color GetGizmoColor(HumanoidIKTarget target)
        {
            return gizmoColorInitialized ? gizmoColor : GetDefaultGizmoColor(target);
        }

        public void SetGizmoColor(Color color)
        {
            gizmoColor = color;
            gizmoColorInitialized = true;
        }

        void OnValidate()
        {
            positionWeight = Mathf.Clamp01(positionWeight);
            rotationWeight = Mathf.Clamp01(rotationWeight);
            bendWeight = Mathf.Clamp01(bendWeight);
            digitWeight = Mathf.Clamp01(digitWeight);
            toeBaseBend = Mathf.Clamp(toeBaseBend, -1f, 1f);
            toeFan = Mathf.Clamp(toeFan, -1f, 1f);
            EnsureDigitBendRangesInitialized();
        }

        internal void InitializeHumanoidSpaces()
        {
            rotationSpace = HumanoidIKRotationSpace.HumanoidEffector;
            footRotationFrameVersion = CurrentFootRotationFrameVersion;
            bendSpace = HumanoidIKBendSpace.HumanoidPoleDirection;
            useDirectorTransformAsDefaultAnchor = true;
        }

        internal Transform ResolveAnchor(IExposedPropertyTable resolver, Transform directorTransform)
        {
            var anchor = ResolveExplicitAnchor(resolver);
            return anchor ? anchor : useDirectorTransformAsDefaultAnchor ? directorTransform : null;
        }
        internal Transform ResolveExplicitAnchor(IExposedPropertyTable resolver)
        {
            return anchorTransform.Resolve(resolver);
        }

        internal void ResolveEffectiveSpace(
            IExposedPropertyTable resolver,
            Transform directorTransform,
            out Transform anchor,
            out bool positionFollowsAnchor,
            out Vector3 resolvedPosition,
            out Vector3 resolvedRotation,
            out Vector3 resolvedBendTarget)
        {
            positionFollowsAnchor = false;
            resolvedPosition = position;
            resolvedRotation = rotation;
            resolvedBendTarget = bendTarget;
            anchor = ResolveExplicitAnchor(resolver);
            if (anchor)
            {
                positionFollowsAnchor = true;
                resolvedPosition = Vector3.zero;
                return;
            }
            if (!directorTransform) return;

            anchor = directorTransform;
            if (useDirectorTransformAsDefaultAnchor || HasExplicitAnchorReference()) return;

            ConvertWorldValuesToLocal(
                directorTransform,
                ref resolvedPosition,
                ref resolvedRotation,
                ref resolvedBendTarget);
        }

        internal bool EnsureDirectorLocalDefaultAnchor(Transform directorTransform)
        {
            if (useDirectorTransformAsDefaultAnchor || !directorTransform) return false;

            // A declared explicit anchor already owns the stored local values. Only
            // update its missing-reference fallback; converting those values as if
            // they were world-space would change the authored pose.
            if (!HasExplicitAnchorReference())
            {
                ConvertWorldValuesToLocal(
                    directorTransform,
                    ref position,
                    ref rotation,
                    ref bendTarget);
            }

            useDirectorTransformAsDefaultAnchor = true;
            return true;
        }

        void ConvertWorldValuesToLocal(
            Transform localSpace,
            ref Vector3 resolvedPosition,
            ref Vector3 resolvedRotation,
            ref Vector3 resolvedBendTarget)
        {
            resolvedPosition = localSpace.InverseTransformPoint(resolvedPosition);
            resolvedRotation = (
                Quaternion.Inverse(localSpace.rotation) *
                Quaternion.Euler(resolvedRotation)).eulerAngles;
            resolvedBendTarget = localSpace.InverseTransformPoint(resolvedBendTarget);
        }

        bool HasExplicitAnchorReference()
        {
            return anchorTransform.defaultValue ||
                   !PropertyName.IsNullOrEmpty(anchorTransform.exposedName);
        }

        internal void InitializeGizmoColor(HumanoidIKTarget target)
        {
            SetGizmoColor(GetDefaultGizmoColor(target));
        }

        static Color GetDefaultGizmoColor(HumanoidIKTarget target)
        {
            return target switch
            {
                HumanoidIKTarget.LeftHand => new Color(1f, 0.05f, 0.04f, 0.8f),
                HumanoidIKTarget.RightHand => new Color(0f, 0.95f, 1f, 0.8f),
                HumanoidIKTarget.LeftFoot => new Color(1f, 0.05f, 0.04f, 0.8f),
                HumanoidIKTarget.RightFoot => new Color(0f, 0.95f, 1f, 0.8f),
                _ => new Color(0.2f, 0.75f, 1f, 0.8f)
            };
        }

        internal void SetTargetWorldRotation(Transform anchor, Quaternion worldRotation)
        {
            var localRotation = anchor ? Quaternion.Inverse(anchor.rotation) * worldRotation : worldRotation;
            rotation = localRotation.eulerAngles;
            rotationSpace = HumanoidIKRotationSpace.HumanoidEffector;
            footRotationFrameVersion = CurrentFootRotationFrameVersion;
        }

        internal void SetHumanoidPoleWorldVector(Transform anchor, Vector3 worldPosition)
        {
            bendTarget = anchor
                ? anchor.InverseTransformPoint(worldPosition)
                : worldPosition;
            bendSpace = HumanoidIKBendSpace.HumanoidPoleDirection;
        }

        public bool EnsureDigitBendRangesInitialized()
        {
            var changed = false;
            if (!digitBendRangesInitialized || digitBendRanges == null || digitBendRanges.Length != 15)
            {
                digitBendRanges = CreateDefaultHandDigitBendRanges();
                digitBendRangesInitialized = true;
                changed = true;
            }

            if (!thumbSpreadRangeInitialized)
            {
                thumbSpreadRange = new Vector2(-60f, 30f);
                thumbSpreadRangeInitialized = true;
                changed = true;
            }

            if (!fingerSpreadRangesInitialized || fingerSpreadRanges == null || fingerSpreadRanges.Length != 4)
            {
                fingerSpreadRanges = CreateDefaultFingerSpreadRanges();
                fingerSpreadRangesInitialized = true;
                fingerSpreadRangesVersion = CurrentFingerSpreadRangesVersion;
                changed = true;
            }
            else
            {
                if (fingerSpreadRangesVersion < 1)
                {
                    if (HasLegacyDefaultFingerSpreadRanges())
                    {
                        fingerSpreadRanges[1] = new Vector2(-1f, 1f);
                    }

                    fingerSpreadRangesVersion = 1;
                    changed = true;
                }

                if (fingerSpreadRangesVersion < 2)
                {
                    MigrateLegacyFingerFanPose();
                    fingerSpreadRangesVersion = CurrentFingerSpreadRangesVersion;
                    changed = true;
                }
            }

            if (!toeBendRangesInitialized || toeBendRanges == null || toeBendRanges.Length != 3)
            {
                toeBendRanges = CreateDefaultToeBendRanges();
                toeBaseBendRange = new Vector2(-25f, 20f);
                toeBendRangesInitialized = true;
                changed = true;
            }

            return changed;
        }

        public Vector2 GetDigitBendRange(int digitIndex, int jointIndex)
        {
            var ranges = digitBendRangesInitialized && digitBendRanges != null && digitBendRanges.Length == 15
                ? digitBendRanges
                : CreateDefaultHandDigitBendRanges();
            digitIndex = Mathf.Clamp(digitIndex, 0, 4);
            jointIndex = Mathf.Clamp(jointIndex, 0, 2);
            return ranges[digitIndex * 3 + jointIndex];
        }

        public Vector2 GetThumbSpreadRange()
        {
            return thumbSpreadRangeInitialized ? thumbSpreadRange : new Vector2(-60f, 30f);
        }

        public Vector2 GetFingerSpreadRange(int digitIndex)
        {
            var ranges = fingerSpreadRangesInitialized && fingerSpreadRanges != null && fingerSpreadRanges.Length == 4
                ? fingerSpreadRanges
                : CreateDefaultFingerSpreadRanges();
            digitIndex = Mathf.Clamp(digitIndex, 1, 4);
            return ranges[digitIndex - 1];
        }

        public Vector2 GetToeBendRange(int jointIndex)
        {
            var ranges = toeBendRangesInitialized && toeBendRanges != null && toeBendRanges.Length == 3
                ? toeBendRanges
                : CreateDefaultToeBendRanges();
            jointIndex = Mathf.Clamp(jointIndex, 0, 2);
            return ranges[jointIndex];
        }

        public Vector2 GetToeBaseBendRange()
        {
            return toeBendRangesInitialized ? toeBaseBendRange : new Vector2(-25f, 20f);
        }

        static Vector2[] CreateDefaultToeBendRanges()
        {
            return new[]
            {
                new Vector2(-25f, 20f),
                new Vector2(-18f, 8f),
                new Vector2(-12f, 5f)
            };
        }

        static Vector2[] CreateDefaultHandDigitBendRanges()
        {
            return new[]
            {
                new Vector2(-60f, 0f),
                new Vector2(-60f, 30f),
                new Vector2(-60f, 30f),
                new Vector2(-60f, 50f),
                new Vector2(-60f, 50f),
                new Vector2(-60f, 50f),
                new Vector2(-60f, 50f),
                new Vector2(-60f, 50f),
                new Vector2(-60f, 50f),
                new Vector2(-60f, 50f),
                new Vector2(-60f, 50f),
                new Vector2(-60f, 50f),
                new Vector2(-60f, 50f),
                new Vector2(-60f, 50f),
                new Vector2(-60f, 50f)
            };
        }

        static Vector2[] CreateDefaultFingerSpreadRanges()
        {
            return new[]
            {
                new Vector2(-20f, 20f),
                new Vector2(-1f, 1f),
                new Vector2(-7.5f, 7.5f),
                new Vector2(-20f, 20f)
            };
        }

        bool HasLegacyDefaultFingerSpreadRanges()
        {
            return fingerSpreadRanges != null &&
                   fingerSpreadRanges.Length == 4 &&
                   fingerSpreadRanges[0] == new Vector2(-20f, 20f) &&
                   fingerSpreadRanges[1] == new Vector2(-7.5f, 7.5f) &&
                   fingerSpreadRanges[2] == new Vector2(-7.5f, 7.5f) &&
                   fingerSpreadRanges[3] == new Vector2(-20f, 20f);
        }

        void MigrateLegacyFingerFanPose()
        {
            var indexPose = GetPoseFromRange(digitBends.indexOrSecondToe.proximal.y, fingerSpreadRanges[0]);
            var ringPose = -GetPoseFromRange(digitBends.ringOrFourthToe.proximal.y, fingerSpreadRanges[2]);
            var littlePose = -GetPoseFromRange(digitBends.littleOrFifthToe.proximal.y, fingerSpreadRanges[3]);
            var fanPose = Mathf.Clamp((indexPose + ringPose + littlePose) / 3f, -1f, 1f);

            var pose = digitBends;
            var index = pose.indexOrSecondToe;
            var middle = pose.middleOrThirdToe;
            var ring = pose.ringOrFourthToe;
            var little = pose.littleOrFifthToe;

            index.proximal.y = GetAngleFromRange(fanPose, fingerSpreadRanges[0]);
            middle.proximal.y = GetAngleFromRange(fanPose, fingerSpreadRanges[1]);
            ring.proximal.y = GetAngleFromRange(fanPose, fingerSpreadRanges[2]);
            little.proximal.y = GetAngleFromRange(fanPose, fingerSpreadRanges[3]);

            pose.indexOrSecondToe = index;
            pose.middleOrThirdToe = middle;
            pose.ringOrFourthToe = ring;
            pose.littleOrFifthToe = little;
            digitBends = pose;
        }

        static float GetPoseFromRange(float angle, Vector2 range)
        {
            var min = Mathf.Min(range.x, range.y);
            var max = Mathf.Max(range.x, range.y);
            if (Mathf.Approximately(min, max)) return 0f;
            return Mathf.Lerp(-1f, 1f, Mathf.InverseLerp(min, max, angle));
        }

        static float GetAngleFromRange(float pose, Vector2 range)
        {
            var min = Mathf.Min(range.x, range.y);
            var max = Mathf.Max(range.x, range.y);
            return Mathf.Lerp(min, max, Mathf.InverseLerp(-1f, 1f, pose));
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<HumanoidIKBehaviour>.Create(graph);
            var behaviour = playable.GetBehaviour();

            ResolveEffectiveSpace(
                graph.GetResolver(),
                owner ? owner.transform : null,
                out behaviour.anchorTransform,
                out _,
                out behaviour.position,
                out behaviour.rotation,
                out behaviour.bendTarget);
            behaviour.rotationSpace = rotationSpace;
            behaviour.footRotationFrameVersion = footRotationFrameVersion;
            behaviour.bendSpace = bendSpace;
            behaviour.positionWeight = positionWeight;
            behaviour.rotationWeight = rotationWeight;
            behaviour.bendWeight = bendWeight;
            behaviour.digitWeight = digitWeight;
            behaviour.digitBends = digitBends;
            behaviour.toeBaseBend = toeBaseBend;
            behaviour.toeFan = toeFan;
            behaviour.toeBendRanges = toeBendRangesInitialized && toeBendRanges != null && toeBendRanges.Length == 3
                ? toeBendRanges
                : CreateDefaultToeBendRanges();
            behaviour.toeBaseBendRange = toeBendRangesInitialized ? toeBaseBendRange : new Vector2(-25f, 20f);

            return playable;
        }
    }
}
