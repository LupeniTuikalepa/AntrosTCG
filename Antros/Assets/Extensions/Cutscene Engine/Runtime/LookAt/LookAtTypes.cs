using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Timeline;
using Random = UnityEngine.Random;

namespace CutsceneEngine
{
    [Serializable]
    public struct LookAtAngleLimits
    {
        public const float MinimumAngle = -180f;
        public const float MaximumAngle = 180f;

        [Tooltip("Minimum and maximum horizontal (yaw) rotation in degrees.")]
        public Vector2 horizontal;

        [Tooltip("Minimum and maximum vertical (pitch) rotation in degrees.")]
        public Vector2 vertical;

        [SerializeField, HideInInspector]
        bool initialized;

        public static LookAtAngleLimits Unrestricted => new LookAtAngleLimits(
            new Vector2(MinimumAngle, MaximumAngle),
            new Vector2(MinimumAngle, MaximumAngle));

        public LookAtAngleLimits(Vector2 horizontal, Vector2 vertical)
        {
            this.horizontal = horizontal;
            this.vertical = vertical;
            initialized = true;
        }

        internal LookAtAngleLimits Sanitized()
        {
            if (!initialized) return Unrestricted;

            return new LookAtAngleLimits(
                SanitizeRange(horizontal),
                SanitizeRange(vertical));
        }

        static Vector2 SanitizeRange(Vector2 range)
        {
            var first = Mathf.Clamp(range.x, MinimumAngle, MaximumAngle);
            var second = Mathf.Clamp(range.y, MinimumAngle, MaximumAngle);
            return new Vector2(Mathf.Min(first, second), Mathf.Max(first, second));
        }
    }
    internal enum LookAtEyelidDirection
    {
        Down,
        Up,
        Left,
        Right,
        Horizontal
    }

    internal enum LookAtEyelidSide
    {
        Both,
        Left,
        Right
    }

    internal readonly struct LookAtEyelidBlendShapeKey
    {
        public readonly string Key;
        public readonly LookAtEyelidDirection Direction;
        public readonly LookAtEyelidSide Side;

        public LookAtEyelidBlendShapeKey(
            string key,
            LookAtEyelidDirection direction,
            LookAtEyelidSide side)
        {
            Key = key;
            Direction = direction;
            Side = side;
        }
    }

    internal readonly struct LookAtEyeDirectionState
    {
        public readonly bool HasLeft;
        public readonly float LeftPitch;
        public readonly float LeftYaw;
        public readonly bool HasRight;
        public readonly float RightPitch;
        public readonly float RightYaw;

        public LookAtEyeDirectionState(
            bool hasLeft,
            float leftPitch,
            float leftYaw,
            bool hasRight,
            float rightPitch,
            float rightYaw)
        {
            HasLeft = hasLeft;
            LeftPitch = leftPitch;
            LeftYaw = leftYaw;
            HasRight = hasRight;
            RightPitch = rightPitch;
            RightYaw = rightYaw;
        }

        internal bool HasAny => HasLeft || HasRight;

        internal bool TryResolvePitch(
            LookAtEyelidSide side,
            out float pitch)
        {
            return TryResolve(
                side,
                HasLeft,
                LeftPitch,
                HasRight,
                RightPitch,
                out pitch);
        }

        internal bool TryResolveYaw(
            LookAtEyelidSide side,
            out float yaw)
        {
            return TryResolve(
                side,
                HasLeft,
                LeftYaw,
                HasRight,
                RightYaw,
                out yaw);
        }

        static bool TryResolve(
            LookAtEyelidSide side,
            bool hasLeft,
            float left,
            bool hasRight,
            float right,
            out float value)
        {
            if (side == LookAtEyelidSide.Left && hasLeft)
            {
                value = left;
                return true;
            }

            if (side == LookAtEyelidSide.Right && hasRight)
            {
                value = right;
                return true;
            }

            if (hasLeft && hasRight)
            {
                value = (left + right) * 0.5f;
                return true;
            }

            if (hasLeft)
            {
                value = left;
                return true;
            }

            if (hasRight)
            {
                value = right;
                return true;
            }

            value = 0f;
            return false;
        }
    }

    internal struct LookAtSample
    {
        public LookAtClip SourceClip;
        public IExposedPropertyTable Resolver;
        public Transform Target;
        public Transform DirectorTransform;
        public Vector3 Position;
        public float TimelineWeight;
        public float EyesWeight;
        public float HeadWeight;
        public float NeckWeight;
        public float BodyWeight;
        public float ChinOffset;
        public LookAtAngleLimits EyesAngleLimits;
        public LookAtAngleLimits HeadAngleLimits;
        public LookAtAngleLimits NeckAngleLimits;
        public LookAtAngleLimits BodyAngleLimits;
        public double LocalTime;
        public double LocalDuration;
        public string[] BlinkBlendShapeKeys;
        public LookAtBlinkMode BlinkMode;
        public AnimationCurve BlinkCurve;
        public float BlinkFrequency;
        public float BlinkDuration;
        public AnimationCurve AutomaticBlinkCurve;
        public float BlinkNoiseOffset;
        public string[] UpperEyelidFollowBlendShapeKeys;
        public LookAtEyelidBlendShapeKey[] UpperEyelidFollowKeyCache;
        public float UpperEyelidFollowWeight;
        public AnimationCurve UpperEyelidFollowCurve;
        public string[] LowerEyelidFollowBlendShapeKeys;
        public LookAtEyelidBlendShapeKey[] LowerEyelidFollowKeyCache;
        public float LowerEyelidFollowWeight;
        public AnimationCurve LowerEyelidFollowCurve;
        public string[] HorizontalEyelidFollowBlendShapeKeys;
        public LookAtEyelidBlendShapeKey[] HorizontalEyelidFollowKeyCache;
        public float HorizontalEyelidFollowWeight;
        public AnimationCurve HorizontalEyelidFollowCurve;

        internal Vector3 ResolveTargetPosition()
        {
            var source = SourceClip;
            var resolvedTarget = source ? source.ResolveTarget(Resolver) : Target;
            var resolvedPosition = source ? source.position : Position;
            return LookAtUtility.ResolveTargetPosition(
                resolvedTarget,
                DirectorTransform,
                resolvedPosition);
        }

        internal float ResolveEyesWeight()
        {
            return Mathf.Clamp01(SourceClip ? SourceClip.eyesWeight : EyesWeight);
        }

        internal float ResolveHeadWeight()
        {
            return Mathf.Clamp01(SourceClip ? SourceClip.headWeight : HeadWeight);
        }

        internal float ResolveNeckWeight()
        {
            return Mathf.Clamp01(SourceClip ? SourceClip.neckWeight : NeckWeight);
        }

        internal float ResolveBodyWeight()
        {
            return Mathf.Clamp01(SourceClip ? SourceClip.bodyWeight : BodyWeight);
        }

        internal float ResolveChinPitchOffsetDegrees()
        {
            var chinOffset = SourceClip ? SourceClip.chinOffset : ChinOffset;
            return LookAtUtility.SanitizeChinOffset(chinOffset) *
                   LookAtClip.MaximumChinPitchOffsetDegrees;
        }

        internal LookAtAngleLimits ResolveEyesAngleLimits()
        {
            return (SourceClip ? SourceClip.eyesAngleLimits : EyesAngleLimits).Sanitized();
        }

        internal LookAtAngleLimits ResolveHeadAngleLimits()
        {
            return (SourceClip ? SourceClip.headAngleLimits : HeadAngleLimits).Sanitized();
        }

        internal LookAtAngleLimits ResolveNeckAngleLimits()
        {
            return (SourceClip ? SourceClip.neckAngleLimits : NeckAngleLimits).Sanitized();
        }

        internal LookAtAngleLimits ResolveBodyAngleLimits()
        {
            return (SourceClip ? SourceClip.bodyAngleLimits : BodyAngleLimits).Sanitized();
        }

        internal string[] ResolveBlinkBlendShapeKeys()
        {
            return SourceClip
                ? SourceClip.blinkBlendShapeKeys
                : BlinkBlendShapeKeys;
        }

        internal LookAtBlinkMode ResolveBlinkMode()
        {
            return SourceClip ? SourceClip.blinkMode : BlinkMode;
        }

        internal AnimationCurve ResolveBlinkCurve()
        {
            return SourceClip ? SourceClip.blinkCurve : BlinkCurve;
        }

        internal float ResolveBlinkFrequency()
        {
            return Mathf.Clamp01(SourceClip ? SourceClip.blinkFrequency : BlinkFrequency);
        }

        internal float ResolveBlinkDuration()
        {
            return Mathf.Clamp(
                SourceClip ? SourceClip.blinkDuration : BlinkDuration,
                LookAtClip.MinimumAutomaticBlinkDuration,
                LookAtClip.MaximumAutomaticBlinkDuration);
        }

        internal AnimationCurve ResolveAutomaticBlinkCurve()
        {
            return SourceClip
                ? SourceClip.automaticBlinkCurve
                : AutomaticBlinkCurve;
        }

        internal float ResolveBlinkNoiseOffset()
        {
            return LookAtUtility.SanitizeBlinkNoiseOffset(
                SourceClip ? SourceClip.blinkNoiseOffset : BlinkNoiseOffset);
        }

        internal string[] ResolveUpperEyelidFollowBlendShapeKeys()
        {
            return SourceClip
                ? SourceClip.upperEyelidFollowBlendShapeKeys
                : UpperEyelidFollowBlendShapeKeys;
        }
        internal LookAtEyelidBlendShapeKey[] ResolveUpperEyelidFollowKeyCache()
        {
            return UpperEyelidFollowKeyCache ??
                   LookAtUtility.CacheEyelidBlendShapeKeys(
                       ResolveUpperEyelidFollowBlendShapeKeys(),
                       LookAtEyelidDirection.Down);
        }

        internal float ResolveUpperEyelidFollowWeight()
        {
            return Mathf.Clamp01(
                SourceClip
                    ? SourceClip.upperEyelidFollowWeight
                    : UpperEyelidFollowWeight);
        }

        internal AnimationCurve ResolveUpperEyelidFollowCurve()
        {
            return SourceClip
                ? SourceClip.upperEyelidFollowCurve
                : UpperEyelidFollowCurve;
        }

        internal string[] ResolveLowerEyelidFollowBlendShapeKeys()
        {
            return SourceClip
                ? SourceClip.lowerEyelidFollowBlendShapeKeys
                : LowerEyelidFollowBlendShapeKeys;
        }
        internal LookAtEyelidBlendShapeKey[] ResolveLowerEyelidFollowKeyCache()
        {
            return LowerEyelidFollowKeyCache ??
                   LookAtUtility.CacheEyelidBlendShapeKeys(
                       ResolveLowerEyelidFollowBlendShapeKeys(),
                       LookAtEyelidDirection.Up);
        }

        internal float ResolveLowerEyelidFollowWeight()
        {
            return Mathf.Clamp01(
                SourceClip
                    ? SourceClip.lowerEyelidFollowWeight
                    : LowerEyelidFollowWeight);
        }

        internal AnimationCurve ResolveLowerEyelidFollowCurve()
        {
            return SourceClip
                ? SourceClip.lowerEyelidFollowCurve
                : LowerEyelidFollowCurve;
        }

        internal string[] ResolveHorizontalEyelidFollowBlendShapeKeys()
        {
            return SourceClip
                ? SourceClip.horizontalEyelidFollowBlendShapeKeys
                : HorizontalEyelidFollowBlendShapeKeys;
        }

        internal LookAtEyelidBlendShapeKey[] ResolveHorizontalEyelidFollowKeyCache()
        {
            return HorizontalEyelidFollowKeyCache ??
                   LookAtUtility.CacheEyelidBlendShapeKeys(
                       ResolveHorizontalEyelidFollowBlendShapeKeys(),
                       LookAtEyelidDirection.Horizontal);
        }

        internal float ResolveHorizontalEyelidFollowWeight()
        {
            return Mathf.Clamp01(
                SourceClip
                    ? SourceClip.horizontalEyelidFollowWeight
                    : HorizontalEyelidFollowWeight);
        }

        internal AnimationCurve ResolveHorizontalEyelidFollowCurve()
        {
            return SourceClip
                ? SourceClip.horizontalEyelidFollowCurve
                : HorizontalEyelidFollowCurve;
        }

        internal bool HasAnyChannelWeight()
        {
            return ResolveEyesWeight() > 0f ||
                   ResolveHeadWeight() > 0f ||
                   ResolveNeckWeight() > 0f ||
                   ResolveBodyWeight() > 0f;
        }

        internal bool HasBlinkConfiguration()
        {
            if (!LookAtUtility.HasAnyKey(ResolveBlinkBlendShapeKeys()))
            {
                return false;
            }

            return ResolveBlinkMode() == LookAtBlinkMode.AnimationCurve
                ? ResolveBlinkCurve() != null && ResolveBlinkCurve().length > 0
                : ResolveBlinkFrequency() > 0f;
        }

        internal float ResolveBlinkAmount()
        {
            if (!HasBlinkConfiguration()) return 0f;

            return LookAtUtility.EvaluateBlink(
                ResolveBlinkMode(),
                ResolveBlinkCurve(),
                ResolveBlinkFrequency(),
                ResolveBlinkDuration(),
                LocalTime,
                LocalDuration,
                ResolveBlinkNoiseOffset(),
                ResolveAutomaticBlinkCurve());
        }

        internal bool HasUpperEyelidFollowConfiguration()
        {
            return HasEyelidFollowChannel(
                ResolveUpperEyelidFollowKeyCache(),
                ResolveUpperEyelidFollowWeight(),
                ResolveUpperEyelidFollowCurve());
        }

        internal bool HasLowerEyelidFollowConfiguration()
        {
            return HasEyelidFollowChannel(
                ResolveLowerEyelidFollowKeyCache(),
                ResolveLowerEyelidFollowWeight(),
                ResolveLowerEyelidFollowCurve());
        }

        internal bool HasHorizontalEyelidFollowConfiguration()
        {
            return HasEyelidFollowChannel(
                ResolveHorizontalEyelidFollowKeyCache(),
                ResolveHorizontalEyelidFollowWeight(),
                ResolveHorizontalEyelidFollowCurve());
        }

        internal bool HasEyelidFollowConfiguration()
        {
            return HasUpperEyelidFollowConfiguration() ||
                   HasLowerEyelidFollowConfiguration() ||
                   HasHorizontalEyelidFollowConfiguration();
        }

        internal float ResolveUpperEyelidFollowAmount(
            float eyePitchDegrees,
            LookAtEyelidDirection direction)
        {
            if (!HasUpperEyelidFollowConfiguration()) return 0f;

            return LookAtUtility.EvaluateDirectionalEyelidFollow(
                eyePitchDegrees,
                ResolveEyesAngleLimits(),
                ResolveUpperEyelidFollowWeight(),
                ResolveUpperEyelidFollowCurve(),
                direction,
                LookAtEyelidDirection.Down);
        }

        internal float ResolveLowerEyelidFollowAmount(
            float eyePitchDegrees,
            LookAtEyelidDirection direction)
        {
            if (!HasLowerEyelidFollowConfiguration()) return 0f;

            return LookAtUtility.EvaluateDirectionalEyelidFollow(
                eyePitchDegrees,
                ResolveEyesAngleLimits(),
                ResolveLowerEyelidFollowWeight(),
                ResolveLowerEyelidFollowCurve(),
                direction,
                LookAtEyelidDirection.Up);
        }

        internal float ResolveHorizontalEyelidFollowAmount(
            float eyeYawDegrees,
            LookAtEyelidDirection direction)
        {
            if (!HasHorizontalEyelidFollowConfiguration()) return 0f;

            return LookAtUtility.EvaluateHorizontalEyelidFollow(
                eyeYawDegrees,
                ResolveEyesAngleLimits(),
                ResolveHorizontalEyelidFollowWeight(),
                ResolveHorizontalEyelidFollowCurve(),
                direction);
        }

        static bool HasEyelidFollowChannel(
            LookAtEyelidBlendShapeKey[] keys,
            float weight,
            AnimationCurve curve)
        {
            return LookAtUtility.HasAnyEyelidKey(keys) &&
                   weight > 0f &&
                   curve != null &&
                   curve.length > 0;
        }

        internal bool HasAnyEffect()
        {
            return HasAnyChannelWeight() ||
                   HasBlinkConfiguration() ||
                   HasEyelidFollowConfiguration();
        }

    }

    internal struct LookAtState
    {
        public bool Active;
        public LookAtSample[] Samples;
        public int SampleCount;
        public LookAtTrack SourceTrack;
    }

    internal readonly struct LookAtChannelState
    {
        public readonly Vector3 TargetPosition;
        public readonly float Weight;
        public readonly LookAtAngleLimits AngleLimits;
        public readonly float PitchOffsetDegrees;

        public LookAtChannelState(
            Vector3 targetPosition,
            float weight,
            LookAtAngleLimits angleLimits,
            float pitchOffsetDegrees = 0f)
        {
            TargetPosition = targetPosition;
            Weight = weight;
            AngleLimits = angleLimits;
            PitchOffsetDegrees = pitchOffsetDegrees;
        }
    }

    internal readonly struct LookAtEvaluatedState
    {
        public readonly LookAtChannelState Eyes;
        public readonly LookAtChannelState Head;
        public readonly LookAtChannelState Neck;
        public readonly LookAtChannelState Body;

        public LookAtEvaluatedState(
            LookAtChannelState eyes,
            LookAtChannelState head,
            LookAtChannelState neck,
            LookAtChannelState body)
        {
            Eyes = eyes;
            Head = head;
            Neck = neck;
            Body = body;
        }
    }

    internal readonly struct LookAtBoneFrame
    {
        public readonly Transform Bone;
        public readonly Vector3 ForwardInBone;

        public LookAtBoneFrame(Transform bone, Vector3 forwardInBone)
        {
            Bone = bone;
            ForwardInBone = forwardInBone;
        }
    }

    internal readonly struct LookAtGenericRigDefinition
    {
        internal readonly Transform Head;
        internal readonly Transform[] Body;
        internal readonly Transform Neck;
        internal readonly Transform LeftEye;
        internal readonly Transform RightEye;

        internal LookAtGenericRigDefinition(
            Transform head,
            Transform[] body,
            Transform neck,
            Transform leftEye,
            Transform rightEye)
        {
            Head = head;
            Body = body;
            Neck = neck;
            LeftEye = leftEye;
            RightEye = rightEye;
        }
    }

    internal static class LookAtGenericRigUtility
    {
        readonly struct EyeCandidate
        {
            internal readonly Transform Bone;
            internal readonly int Score;
            internal readonly int Side;
            internal readonly float LocalX;

            internal EyeCandidate(
                Transform bone,
                int score,
                int side,
                float localX)
            {
                Bone = bone;
                Score = score;
                Side = side;
                LocalX = localX;
            }
        }

        internal static bool TryResolve(
            Animator animator,
            LookAtTrack track,
            out LookAtGenericRigDefinition definition)
        {
            definition = default;
            if (!TryResolveAutomatic(animator, track, out var automatic))
            {
                return false;
            }

            var mapping = GetMapping(animator);
            if (!mapping || !mapping.initialized)
            {
                definition = automatic;
                return true;
            }

            var root = animator.transform;
            var body = new List<Transform>();
            var bodyBones = mapping.bodyBones;
            if (bodyBones != null)
            {
                for (var i = 0; i < bodyBones.Length; i++)
                {
                    var bone = ResolveMappedAncestor(
                        root,
                        automatic.Head,
                        bodyBones[i]);
                    if (bone && !body.Contains(bone))
                    {
                        body.Add(bone);
                    }
                }
            }

            Transform neck = null;
            if (body.Count > 0)
            {
                neck = body[^1];
                body.RemoveAt(body.Count - 1);
            }

            definition = new LookAtGenericRigDefinition(
                automatic.Head,
                body.ToArray(),
                neck,
                ResolveMappedEye(
                    root,
                    automatic.Head,
                    mapping.leftEye),
                ResolveMappedEye(
                    root,
                    automatic.Head,
                    mapping.rightEye));
            return true;
        }

        internal static bool TryResolveAutomatic(
            Animator animator,
            LookAtTrack track,
            out LookAtGenericRigDefinition definition)
        {
            definition = default;
            if (!animator || animator.isHuman || !track) return false;

            var mapping = GetMapping(animator);
            if (mapping && mapping.initialized)
            {
                return TryBuildAutomatic(
                    animator,
                    mapping.pelvis,
                    mapping.head,
                    out definition);
            }

            var head = DetectHead(animator);
            var pelvis = DetectPelvis(animator, head);
            return TryBuildAutomatic(
                animator,
                pelvis,
                head,
                out definition);
        }

        internal static bool TryBuildAutomatic(
            Animator animator,
            Transform pelvis,
            Transform head,
            out LookAtGenericRigDefinition definition)
        {
            definition = default;
            if (!animator || animator.isHuman || !head) return false;

            var root = animator.transform;
            if (!IsDescendantOf(root, head)) return false;
            if (pelvis &&
                (!IsDescendantOf(root, pelvis) ||
                 pelvis == head ||
                 !head.IsChildOf(pelvis)))
            {
                pelvis = null;
            }

            var body = new List<Transform>();
            Transform neck = null;

            var current = head.parent;
            if (current && current != root && current != pelvis)
            {
                neck = current;
                if (pelvis && pelvis != head && head.IsChildOf(pelvis))
                {
                    current = neck.parent;
                    while (current && current != pelvis)
                    {
                        body.Add(current);
                        current = current.parent;
                    }

                    if (current == pelvis)
                    {
                        body.Reverse();
                    }
                    else
                    {
                        body.Clear();
                    }
                }
            }

            FindEyes(root, head, out var leftEye, out var rightEye);
            definition = new LookAtGenericRigDefinition(
                head,
                body.ToArray(),
                neck,
                leftEye,
                rightEye);
            return true;
        }

        static Transform ResolveMappedAncestor(
            Transform root,
            Transform head,
            Transform bone)
        {
            return IsDescendantOf(root, bone) &&
                   bone != head &&
                   head.IsChildOf(bone)
                ? bone
                : null;
        }

        static Transform ResolveMappedEye(
            Transform root,
            Transform head,
            Transform bone)
        {
            return IsDescendantOf(root, bone) &&
                   bone != head &&
                   bone.IsChildOf(head)
                ? bone
                : null;
        }

        internal static LookAtGenericRigMapping GetMapping(
            Animator animator)
        {
            return animator
                ? animator.GetComponent<LookAtGenericRigMapping>()
                : null;
        }

        internal static int GetMappingHash(Animator animator)
        {
            var mapping = GetMapping(animator);
            return mapping ? mapping.GetMappingHash() : 0;
        }

        internal static Transform ResolveHead(
            Animator animator,
            LookAtTrack track)
        {
            if (!animator || animator.isHuman || !track) return null;

            var mapping = GetMapping(animator);
            if (mapping && mapping.initialized)
            {
                return IsDescendantOf(animator.transform, mapping.head)
                    ? mapping.head
                    : null;
            }

            return DetectHead(animator);
        }

        internal static Transform ResolvePelvis(
            Animator animator,
            LookAtTrack track,
            Transform head)
        {
            if (!animator || animator.isHuman || !track || !head) return null;

            var mapping = GetMapping(animator);
            if (mapping && mapping.initialized)
            {
                var pelvis = mapping.pelvis;
                return IsDescendantOf(animator.transform, pelvis) &&
                       pelvis != head &&
                       head.IsChildOf(pelvis)
                    ? pelvis
                    : null;
            }

            return DetectPelvis(animator, head);
        }

        internal static Transform DetectHead(Animator animator)
        {
            return animator && !animator.isHuman
                ? FindBestHead(animator.transform)
                : null;
        }

        internal static Transform DetectPelvis(
            Animator animator,
            Transform head)
        {
            if (!animator || animator.isHuman || !head) return null;

            var root = animator.transform;
            if (!IsDescendantOf(root, head)) return null;

            Transform best = null;
            Transform fallback = null;
            var bestScore = int.MinValue;
            var current = head.parent;
            while (current && current != root)
            {
                var normalized = NormalizeName(current.name);
                var score = normalized == "pelvis"
                    ? 1000
                    : normalized == "hips" || normalized == "hip"
                        ? 900
                        : normalized.Contains("pelvis", StringComparison.Ordinal)
                            ? 800
                            : normalized.Contains("hips", StringComparison.Ordinal)
                                ? 700
                                : 0;
                score -= GetDepth(root, current);
                if (score > bestScore)
                {
                    bestScore = score;
                    best = current;
                }

                if (IsBodyBoundaryName(normalized))
                {
                    fallback = current;
                }

                current = current.parent;
            }

            return bestScore > 0 ? best : fallback;
        }

        internal static bool IsDescendantOf(
            Transform root,
            Transform candidate)
        {
            return root && candidate &&
                   candidate != root &&
                   candidate.IsChildOf(root);
        }

        internal static string GetRelativePath(
            Transform root,
            Transform target)
        {
            if (!root || !target || target == root || !target.IsChildOf(root))
            {
                return string.Empty;
            }

            var names = new List<string>();
            var current = target;
            while (current && current != root)
            {
                names.Add(current.name);
                current = current.parent;
            }

            if (current != root) return string.Empty;
            names.Reverse();
            return string.Join("/", names);
        }

        internal static Transform ResolvePath(
            Transform root,
            string path)
        {
            return root && !string.IsNullOrEmpty(path)
                ? root.Find(path)
                : null;
        }

        static Transform FindBestHead(Transform root)
        {
            var transforms = root.GetComponentsInChildren<Transform>(true);
            Transform best = null;
            var bestScore = int.MinValue;
            for (var i = 0; i < transforms.Length; i++)
            {
                var candidate = transforms[i];
                if (!candidate || candidate == root) continue;

                var normalized = NormalizeName(candidate.name);
                var score = normalized == "head"
                    ? 1000
                    : normalized.Contains("head", StringComparison.Ordinal)
                        ? 700
                        : 0;
                if (score <= 0) continue;
                if (normalized.Contains("end", StringComparison.Ordinal) ||
                    normalized.Contains("tip", StringComparison.Ordinal) ||
                    normalized.Contains("nub", StringComparison.Ordinal) ||
                    normalized.Contains("top", StringComparison.Ordinal))
                {
                    score -= 500;
                }

                score -= GetDepth(root, candidate);
                if (score <= bestScore) continue;
                bestScore = score;
                best = candidate;
            }

            return best;
        }

        static bool IsBodyBoundaryName(string normalized)
        {
            return normalized.Contains("spine", StringComparison.Ordinal) ||
                   normalized.Contains("chest", StringComparison.Ordinal) ||
                   normalized.Contains("torso", StringComparison.Ordinal) ||
                   normalized.Contains("upperbody", StringComparison.Ordinal);
        }

        static void FindEyes(
            Transform root,
            Transform head,
            out Transform leftEye,
            out Transform rightEye)
        {
            var transforms = head.GetComponentsInChildren<Transform>(true);
            var eyes = new List<EyeCandidate>();
            for (var i = 0; i < transforms.Length; i++)
            {
                var bone = transforms[i];
                if (!bone || bone == head) continue;

                var score = GetEyeScore(bone.name);
                if (score <= 0) continue;

                var side = GetNamedSide(bone.name);
                var localX = root.InverseTransformPoint(bone.position).x;
                eyes.Add(new EyeCandidate(
                    bone,
                    score,
                    side,
                    localX));
            }

            EyeCandidate? left = null;
            EyeCandidate? right = null;
            for (var i = 0; i < eyes.Count; i++)
            {
                var eye = eyes[i];
                var side = eye.Side != 0
                    ? eye.Side
                    : eye.LocalX < -0.0001f
                        ? -1
                        : eye.LocalX > 0.0001f
                            ? 1
                            : 0;
                if (side < 0 && (!left.HasValue || eye.Score > left.Value.Score))
                {
                    left = eye;
                }
                else if (side > 0 && (!right.HasValue || eye.Score > right.Value.Score))
                {
                    right = eye;
                }
            }

            if ((!left.HasValue || !right.HasValue) && eyes.Count >= 2)
            {
                eyes.Sort((a, b) => a.LocalX.CompareTo(b.LocalX));
                left ??= eyes[0];
                right ??= eyes[^1];
                if (left.Value.Bone == right.Value.Bone)
                {
                    right = null;
                }
            }

            leftEye = left?.Bone;
            rightEye = right?.Bone;
        }

        static int GetEyeScore(string name)
        {
            var normalized = NormalizeName(name);
            if (!normalized.Contains("eye", StringComparison.Ordinal)) return 0;
            if (normalized.Contains("eyelid", StringComparison.Ordinal) ||
                normalized.Contains("lash", StringComparison.Ordinal) ||
                normalized.Contains("brow", StringComparison.Ordinal) ||
                normalized.Contains("target", StringComparison.Ordinal) ||
                normalized.Contains("aim", StringComparison.Ordinal))
            {
                return 0;
            }

            var score = GetNamedSide(name) != 0 ? 500 : 300;
            if (normalized.Contains("end", StringComparison.Ordinal) ||
                normalized.Contains("tip", StringComparison.Ordinal))
            {
                score -= 200;
            }

            return score;
        }

        static int GetNamedSide(string name)
        {
            if (string.IsNullOrEmpty(name)) return 0;

            var normalized = NormalizeName(name);
            if (normalized.Contains("left", StringComparison.Ordinal) ||
                HasStandaloneSideToken(name, 'l'))
            {
                return -1;
            }

            if (normalized.Contains("right", StringComparison.Ordinal) ||
                HasStandaloneSideToken(name, 'r'))
            {
                return 1;
            }

            return 0;
        }

        static bool HasStandaloneSideToken(string name, char side)
        {
            for (var i = 0; i < name.Length; i++)
            {
                if (char.ToLowerInvariant(name[i]) != side) continue;

                var startsToken =
                    i == 0 || !char.IsLetterOrDigit(name[i - 1]);
                var endsToken =
                    i == name.Length - 1 ||
                    !char.IsLetterOrDigit(name[i + 1]);
                if (startsToken && endsToken) return true;
            }

            return false;
        }

        static int GetDepth(Transform root, Transform target)
        {
            var depth = 0;
            var current = target;
            while (current && current != root)
            {
                depth++;
                current = current.parent;
            }

            return current == root ? depth : int.MaxValue;
        }

        static string NormalizeName(string name)
        {
            if (string.IsNullOrEmpty(name)) return string.Empty;

            var normalized = new StringBuilder(name.Length);
            for (var i = 0; i < name.Length; i++)
            {
                var character = name[i];
                if (char.IsLetterOrDigit(character))
                {
                    normalized.Append(char.ToLowerInvariant(character));
                }
            }

            return normalized.ToString();
        }
    }

    internal sealed class LookAtGenericReferencePose
    {
        readonly Animator _animator;
        readonly Avatar _avatar;
        readonly Dictionary<Transform, Quaternion> _rootToBoneRotations =
            new Dictionary<Transform, Quaternion>();
        readonly List<Matrix4x4> _bindPoses = new List<Matrix4x4>();

        internal LookAtGenericReferencePose(Animator animator)
        {
            _animator = animator;
            _avatar = animator ? animator.avatar : null;
            CacheBindPoseRotations();
        }

        internal bool IsValidFor(Animator animator)
        {
            return animator &&
                   animator == _animator &&
                   animator.avatar == _avatar;
        }

        internal bool TryGetRootToBoneRotation(
            Transform bone,
            out Quaternion rotation)
        {
            rotation = Quaternion.identity;
            if (!_animator || !bone ||
                (bone != _animator.transform &&
                 !bone.IsChildOf(_animator.transform)))
            {
                return false;
            }

            if (_rootToBoneRotations.TryGetValue(bone, out rotation))
            {
                return true;
            }

            var rootToBone =
                _animator.transform.worldToLocalMatrix * bone.localToWorldMatrix;
            if (!TryGetRotation(rootToBone, out rotation)) return false;

            _rootToBoneRotations[bone] = rotation;
            return true;
        }

        void CacheBindPoseRotations()
        {
            if (!_animator) return;

            var renderers =
                _animator.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            for (var rendererIndex = 0;
                 rendererIndex < renderers.Length;
                 rendererIndex++)
            {
                var renderer = renderers[rendererIndex];
                var mesh = renderer ? renderer.sharedMesh : null;
                if (!mesh) continue;

                _bindPoses.Clear();
                mesh.GetBindposes(_bindPoses);
                var bones = renderer.bones;
                var count = Mathf.Min(bones.Length, _bindPoses.Count);
                if (!TryGetRootToBindSpace(
                        renderer,
                        bones,
                        count,
                        out var rootToBindSpace))
                {
                    continue;
                }

                for (var boneIndex = 0; boneIndex < count; boneIndex++)
                {
                    var bone = bones[boneIndex];
                    if (!bone || _rootToBoneRotations.ContainsKey(bone))
                    {
                        continue;
                    }

                    var rootToBone =
                        rootToBindSpace *
                        _bindPoses[boneIndex].inverse;
                    if (TryGetRotation(rootToBone, out var rotation))
                    {
                        _rootToBoneRotations.Add(bone, rotation);
                    }
                }
            }
        }

        bool TryGetRootToBindSpace(
            SkinnedMeshRenderer renderer,
            Transform[] bones,
            int count,
            out Matrix4x4 rootToBindSpace)
        {
            rootToBindSpace = default;
            var anchor = renderer.rootBone;
            if (anchor)
            {
                for (var boneIndex = 0; boneIndex < count; boneIndex++)
                {
                    if (bones[boneIndex] != anchor) continue;

                    var rootToAnchor =
                        _animator.transform.worldToLocalMatrix *
                        anchor.localToWorldMatrix;
                    var bindSpaceToAnchor =
                        _bindPoses[boneIndex].inverse;
                    if (rootToAnchor.ValidTRS() &&
                        bindSpaceToAnchor.ValidTRS())
                    {
                        rootToBindSpace =
                            rootToAnchor *
                            bindSpaceToAnchor.inverse;
                        return rootToBindSpace.ValidTRS();
                    }

                    break;
                }
            }

            rootToBindSpace =
                _animator.transform.worldToLocalMatrix *
                renderer.transform.localToWorldMatrix;
            return rootToBindSpace.ValidTRS();
        }

        static bool TryGetRotation(
            Matrix4x4 matrix,
            out Quaternion rotation)
        {
            rotation = Quaternion.identity;
            if (!matrix.ValidTRS()) return false;

            rotation = matrix.rotation;
            return !float.IsNaN(rotation.x) &&
                   !float.IsNaN(rotation.y) &&
                   !float.IsNaN(rotation.z) &&
                   !float.IsNaN(rotation.w) &&
                   !float.IsInfinity(rotation.x) &&
                   !float.IsInfinity(rotation.y) &&
                   !float.IsInfinity(rotation.z) &&
                   !float.IsInfinity(rotation.w);
        }
    }

    internal static class LookAtUtility
    {
        static readonly HumanBodyBones[] LookAtBones =
        {
            HumanBodyBones.Spine,
            HumanBodyBones.Chest,
            HumanBodyBones.UpperChest,
            HumanBodyBones.Neck,
            HumanBodyBones.Head,
            HumanBodyBones.LeftEye,
            HumanBodyBones.RightEye
        };

        internal static void GatherLookAtBoneRotations(
            Animator animator,
            IPropertyCollector driver)
        {
            GatherLookAtBoneRotations(animator, null, driver);
        }

        internal static void GatherLookAtBoneRotations(
            Animator animator,
            LookAtTrack track,
            IPropertyCollector driver)
        {
            if (!animator || driver == null) return;

            if (!HumanoidIKUtility.IsUsableHumanoid(animator))
            {
                if (!LookAtGenericRigUtility.TryResolve(
                        animator,
                        track,
                        out var genericRig))
                {
                    return;
                }

                var gathered = new HashSet<Transform>();
                for (var i = 0; i < genericRig.Body.Length; i++)
                {
                    AddTransformRotation(
                        driver,
                        genericRig.Body[i],
                        gathered);
                }

                AddTransformRotation(driver, genericRig.Neck, gathered);
                AddTransformRotation(driver, genericRig.Head, gathered);
                AddTransformRotation(driver, genericRig.LeftEye, gathered);
                AddTransformRotation(driver, genericRig.RightEye, gathered);
                return;
            }

            for (var i = 0; i < LookAtBones.Length; i++)
            {
                AddTransformRotation(driver, animator.GetBoneTransform(LookAtBones[i]));
            }
        }

        internal static void GatherEyelidBlendShapes(
            Animator animator,
            IEnumerable<TimelineClip> clips,
            IPropertyCollector driver)
        {
            if (!animator || clips == null) return;

            var keys = new HashSet<string>(StringComparer.Ordinal);
            foreach (var timelineClip in clips)
            {
                if (timelineClip?.asset is not LookAtClip clip) continue;

                AddBlendShapeKeys(keys, clip.blinkBlendShapeKeys);
                AddBlendShapeKeys(
                    keys,
                    clip.upperEyelidFollowBlendShapeKeys);
                AddBlendShapeKeys(
                    keys,
                    clip.lowerEyelidFollowBlendShapeKeys);
                AddBlendShapeKeys(
                    keys,
                    clip.horizontalEyelidFollowBlendShapeKeys);
            }

            if (keys.Count == 0) return;

            var renderers = animator.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            for (var rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
            {
                var renderer = renderers[rendererIndex];
                var mesh = renderer ? renderer.sharedMesh : null;
                if (!mesh) continue;

                foreach (var key in keys)
                {
                    var blendShapeIndex = mesh.GetBlendShapeIndex(key);
                    if (blendShapeIndex < 0) continue;

                    driver.AddFromName<SkinnedMeshRenderer>(
                        renderer.gameObject,
                        $"m_BlendShapeWeights.Array.data[{blendShapeIndex}]");
                }
            }
        }

        internal const float AutomaticBlinkSampleInterval = 1f / 60f;
        internal const float DefaultEyelidFollowPitchRange = 25f;
        internal const float DefaultEyelidFollowYawRange = 40f;

        static readonly AnimationCurve DefaultAutomaticBlinkCurve =
            new AnimationCurve(
                new Keyframe(0f, 1f, 0f, 0f),
                new Keyframe(0.38f, 0.1f, -3.2f, 1.7f),
                new Keyframe(1f, 1f, 0f, 0f));

        internal static AnimationCurve CreateDefaultAutomaticBlinkCurve()
        {
            return new AnimationCurve(
                new Keyframe(0f, 1f, 0f, 0f),
                new Keyframe(0.38f, 0f, -3.2f, 1.7f),
                new Keyframe(1f, 1f, 0f, 0f));
        }

        internal static bool UsesAutomaticBlinkOpennessConvention(
            AnimationCurve curve)
        {
            if (curve == null || curve.length == 0) return true;

            return curve.Evaluate(0f) + curve.Evaluate(1f) >= 1f;
        }

        internal static AnimationCurve InvertCurveVertically01(
            AnimationCurve curve)
        {
            if (curve == null) return null;

            var keys = curve.keys;
            for (var i = 0; i < keys.Length; i++)
            {
                var key = keys[i];
                key.value = 1f - key.value;
                key.inTangent = -key.inTangent;
                key.outTangent = -key.outTangent;
                keys[i] = key;
            }

            return new AnimationCurve(keys)
            {
                preWrapMode = curve.preWrapMode,
                postWrapMode = curve.postWrapMode
            };
        }

        internal static AnimationCurve CreateDefaultUpperEyelidFollowCurve()
        {
            return new AnimationCurve(
                new Keyframe(0f, 1f, 0f, -2.4f),
                new Keyframe(0.45f, 0f, 0f, 0f),
                new Keyframe(1f, 0f, 0f, 0f));
        }

        internal static AnimationCurve CreateDefaultLowerEyelidFollowCurve()
        {
            return new AnimationCurve(
                new Keyframe(0f, 0f, 0f, 0f),
                new Keyframe(0.55f, 0f, 0f, 0f),
                new Keyframe(1f, 1f, 2.4f, 0f));
        }

        internal static AnimationCurve CreateDefaultHorizontalEyelidFollowCurve()
        {
            return new AnimationCurve(
                new Keyframe(0f, 1f, 0f, -2.4f),
                new Keyframe(0.45f, 0f, 0f, 0f),
                new Keyframe(0.55f, 0f, 0f, 0f),
                new Keyframe(1f, 1f, 2.4f, 0f));
        }



        internal static float SanitizeBlinkNoiseOffset(float noiseOffset)
        {
            return float.IsNaN(noiseOffset) || float.IsInfinity(noiseOffset)
                ? 0f
                : noiseOffset;
        }

        internal static void CollectAutomaticBlinkTriggerTimes(
            double startTime,
            double endTime,
            float frequency,
            float noiseOffset,
            List<double> destination)
        {
            destination.Clear();
            frequency = Mathf.Clamp01(frequency);
            if (frequency <= 0f || endTime <= startTime) return;

            noiseOffset = SanitizeBlinkNoiseOffset(noiseOffset);
            var firstSample = Mathf.Max(
                1,
                Mathf.CeilToInt(
                    (float)(startTime /
                            AutomaticBlinkSampleInterval)));
            var lastSampleExclusive = Mathf.CeilToInt(
                (float)(endTime /
                        AutomaticBlinkSampleInterval));

            for (var sampleIndex = firstSample;
                 sampleIndex < lastSampleExclusive;
                 sampleIndex++)
            {
                if (IsAutomaticBlinkTrigger(
                        sampleIndex,
                        frequency,
                        noiseOffset))
                {
                    destination.Add(
                        sampleIndex *
                        (double)AutomaticBlinkSampleInterval);
                }
            }
        }

        internal static void ClampCurve01(AnimationCurve curve)
        {
            if (curve == null) return;

            var keys = curve.keys;
            for (var i = 0; i < keys.Length; i++)
            {
                var key = keys[i];
                key.time = Mathf.Clamp01(key.time);
                key.value = Mathf.Clamp01(key.value);
                keys[i] = key;
            }

            curve.keys = keys;
        }

        internal static float EvaluateBlink(
            LookAtBlinkMode mode,
            AnimationCurve curve,
            float frequency,
            float blinkDuration,
            double localTime,
            double localDuration,
            float noiseOffset = 0f,
            AnimationCurve automaticBlinkCurve = null)
        {
            if (mode == LookAtBlinkMode.AnimationCurve)
            {
                if (curve == null) return 0f;

                var normalizedTime = NormalizeClipTime(
                    localTime,
                    localDuration);
                var openness = Mathf.Clamp01(
                    curve.Evaluate(normalizedTime));
                return 1f - openness;
            }

            return EvaluateAutomaticBlink(
                localTime,
                frequency,
                blinkDuration,
                noiseOffset,
                automaticBlinkCurve);
        }

        internal static float NormalizeClipTime(
            double localTime,
            double localDuration)
        {
            if (double.IsNaN(localTime) ||
                double.IsInfinity(localTime))
            {
                return 0f;
            }

            if (double.IsNaN(localDuration) ||
                double.IsInfinity(localDuration) ||
                localDuration <= double.Epsilon)
            {
                return Mathf.Clamp01((float)localTime);
            }

            return Mathf.Clamp01((float)(localTime / localDuration));
        }

        internal static float EvaluateAutomaticBlink(
            double localTime,
            float frequency,
            float blinkDuration,
            float noiseOffset = 0f,
            AnimationCurve automaticBlinkCurve = null)
        {
            frequency = Mathf.Clamp01(frequency);
            if (frequency <= 0f ||
                double.IsNaN(localTime) ||
                double.IsInfinity(localTime) ||
                localTime < 0f)
            {
                return 0f;
            }

            blinkDuration = Mathf.Clamp(
                blinkDuration,
                LookAtClip.MinimumAutomaticBlinkDuration,
                LookAtClip.MaximumAutomaticBlinkDuration);
            noiseOffset = SanitizeBlinkNoiseOffset(noiseOffset);

            var latestSample = Mathf.FloorToInt(
                (float)(localTime / AutomaticBlinkSampleInterval));
            var earliestSample = Mathf.Max(
                1,
                Mathf.CeilToInt(
                    (float)((localTime - blinkDuration) /
                            AutomaticBlinkSampleInterval)));

            for (var sampleIndex = latestSample;
                 sampleIndex >= earliestSample;
                 sampleIndex--)
            {
                if (!IsAutomaticBlinkTrigger(
                        sampleIndex,
                        frequency,
                        noiseOffset))
                {
                    continue;
                }

                var triggerTime =
                    sampleIndex * AutomaticBlinkSampleInterval;
                var elapsed = (float)localTime - triggerTime;
                return EvaluateAutomaticBlinkPulse(
                    elapsed,
                    blinkDuration,
                    automaticBlinkCurve);
            }

            return 0f;
        }

        internal static float EvaluateAutomaticBlinkPulse(
            float elapsed,
            float blinkDuration,
            AnimationCurve curve = null)
        {
            blinkDuration = Mathf.Clamp(
                blinkDuration,
                LookAtClip.MinimumAutomaticBlinkDuration,
                LookAtClip.MaximumAutomaticBlinkDuration);
            if (elapsed < 0f || elapsed >= blinkDuration) return 0f;

            var blinkCurve = curve != null && curve.length > 0
                ? curve
                : DefaultAutomaticBlinkCurve;
            var normalizedTime = Mathf.Clamp01(elapsed / blinkDuration);
            var openness = Mathf.Clamp01(
                blinkCurve.Evaluate(normalizedTime));
            return 1f - openness;
        }

        internal static bool IsAutomaticBlinkTrigger(
            int sampleIndex,
            float frequency,
            float noiseOffset = 0f)
        {
            frequency = Mathf.Clamp01(frequency);
            if (sampleIndex <= 0 || frequency <= 0f) return false;

            noiseOffset = SanitizeBlinkNoiseOffset(noiseOffset);
            var time = sampleIndex * AutomaticBlinkSampleInterval;
            if (time < 0.1f) return false;

            var previousNoise = EvaluateAutomaticBlinkNoise(
                time - AutomaticBlinkSampleInterval,
                frequency,
                noiseOffset);
            var currentNoise = EvaluateAutomaticBlinkNoise(
                time,
                frequency,
                noiseOffset);
            var threshold = Mathf.Lerp(
                0.75f,
                0.45f,
                frequency);
            return previousNoise < threshold &&
                   currentNoise >= threshold;
        }

        internal static float EvaluateAutomaticBlinkNoise(
            float time,
            float frequency,
            float noiseOffset = 0f)
        {
            frequency = Mathf.Clamp01(frequency);
            var patternTime =
                time + SanitizeBlinkNoiseOffset(noiseOffset);
            var speed = Mathf.Lerp(
                0.12f,
                5f,
                Mathf.Pow(frequency, 1.5f));
            var primary = Mathf.PerlinNoise1D(
                patternTime * speed);
            var secondary = Mathf.PerlinNoise1D(
                patternTime * speed * 0.37f);
            return primary * 0.75f + secondary * 0.25f;
        }

        internal static float BlendBlendShapeWeight(
            float baseWeight,
            float weightedTargetSum,
            float totalTimelineWeight)
        {
            if (totalTimelineWeight <= 0f) return baseWeight;

            var targetWeight = Mathf.Clamp(
                weightedTargetSum / totalTimelineWeight,
                0f,
                100f);
            return Mathf.Lerp(
                baseWeight,
                targetWeight,
                Mathf.Clamp01(totalTimelineWeight));
        }
        internal static LookAtEyelidBlendShapeKey[] CacheEyelidBlendShapeKeys(
            string[] keys,
            LookAtEyelidDirection fallbackDirection)
        {
            if (keys == null || keys.Length == 0)
            {
                return Array.Empty<LookAtEyelidBlendShapeKey>();
            }

            var uniqueKeys = new HashSet<string>(StringComparer.Ordinal);
            var cachedKeys = new List<LookAtEyelidBlendShapeKey>(keys.Length);
            for (var i = 0; i < keys.Length; i++)
            {
                var key = keys[i];
                if (string.IsNullOrEmpty(key) || !uniqueKeys.Add(key))
                {
                    continue;
                }

                var normalized = NormalizeBlendShapeKey(key);
                var direction = ResolveEyelidDirection(
                    normalized,
                    fallbackDirection);
                var side = ResolveEyelidSide(key, normalized);
                cachedKeys.Add(new LookAtEyelidBlendShapeKey(
                    key,
                    direction,
                    side));
            }

            return cachedKeys.ToArray();
        }

        internal static float EvaluateDirectionalEyelidFollow(
            float eyePitchDegrees,
            LookAtAngleLimits eyeAngleLimits,
            float followWeight,
            AnimationCurve followCurve,
            LookAtEyelidDirection direction,
            LookAtEyelidDirection curveDirection)
        {
            followWeight = Mathf.Clamp01(followWeight);
            if (followWeight <= 0f ||
                followCurve == null ||
                followCurve.length == 0)
            {
                return 0f;
            }

            var normalizedPitch = NormalizeEyelidFollowPitch(
                eyePitchDegrees,
                eyeAngleLimits);
            if (direction == LookAtEyelidDirection.Down)
            {
                if (normalizedPitch >= 0.5f) return 0f;
            }
            else if (normalizedPitch <= 0.5f)
            {
                return 0f;
            }

            var curvePosition = direction == curveDirection
                ? normalizedPitch
                : 1f - normalizedPitch;
            return Mathf.Clamp01(followCurve.Evaluate(curvePosition)) *
                   followWeight;
        }

        static LookAtEyelidDirection ResolveEyelidDirection(
            string normalizedKey,
            LookAtEyelidDirection fallbackDirection)
        {
            if (normalizedKey.Contains("lookdown") ||
                normalizedKey.Contains("eyedown") ||
                normalizedKey.Contains("downlook"))
            {
                return LookAtEyelidDirection.Down;
            }

            if (normalizedKey.Contains("lookup") ||
                normalizedKey.Contains("eyeup") ||
                normalizedKey.Contains("uplook"))
            {
                return LookAtEyelidDirection.Up;
            }

            if (normalizedKey.Contains("lookleft") ||
                normalizedKey.Contains("eyeleft") ||
                normalizedKey.Contains("leftlook"))
            {
                return LookAtEyelidDirection.Left;
            }

            if (normalizedKey.Contains("lookright") ||
                normalizedKey.Contains("eyeright") ||
                normalizedKey.Contains("rightlook"))
            {
                return LookAtEyelidDirection.Right;
            }

            if (normalizedKey.Contains("lookl"))
            {
                return LookAtEyelidDirection.Left;
            }

            if (normalizedKey.Contains("lookr"))
            {
                return LookAtEyelidDirection.Right;
            }

            return fallbackDirection;
        }

        static LookAtEyelidSide ResolveEyelidSide(
            string key,
            string normalizedKey)
        {
            var sideKey = StripDirectionalWords(normalizedKey);
            var sideTokenKey = StripDirectionalTokens(key);
            var hasLeft = sideKey.Contains("left") ||
                          HasSideToken(sideTokenKey, 'l') ||
                          HasDirectionalSideSuffix(normalizedKey, sideKey, 'l');
            var hasRight = sideKey.Contains("right") ||
                           HasSideToken(sideTokenKey, 'r') ||
                           HasDirectionalSideSuffix(normalizedKey, sideKey, 'r');
            if (hasLeft == hasRight) return LookAtEyelidSide.Both;
            return hasLeft
                ? LookAtEyelidSide.Left
                : LookAtEyelidSide.Right;
        }

        static string StripDirectionalWords(string normalizedKey)
        {
            return normalizedKey
                .Replace("lookdown", string.Empty)
                .Replace("eyedown", string.Empty)
                .Replace("downlook", string.Empty)
                .Replace("lookup", string.Empty)
                .Replace("eyeup", string.Empty)
                .Replace("uplook", string.Empty)
                .Replace("lookleft", string.Empty)
                .Replace("eyeleft", string.Empty)
                .Replace("leftlook", string.Empty)
                .Replace("lookright", string.Empty)
                .Replace("eyeright", string.Empty)
                .Replace("rightlook", string.Empty)
                .Replace("lookl", string.Empty)
                .Replace("lookr", string.Empty);
        }

        static string StripDirectionalTokens(string key)
        {
            var tokens = new List<string>();
            var tokenStart = 0;
            for (var i = 0; i <= key.Length; i++)
            {
                if (i < key.Length && char.IsLetterOrDigit(key[i]))
                {
                    continue;
                }

                var tokenLength = i - tokenStart;
                if (tokenLength > 0)
                {
                    tokens.Add(key.Substring(tokenStart, tokenLength));
                }

                tokenStart = i + 1;
            }

            var sideTokens = new StringBuilder(key.Length);
            for (var i = 0; i < tokens.Count; i++)
            {
                var adjacentToLook =
                    (i > 0 &&
                     string.Equals(
                         tokens[i - 1],
                         "look",
                         StringComparison.OrdinalIgnoreCase)) ||
                    (i + 1 < tokens.Count &&
                     string.Equals(
                         tokens[i + 1],
                         "look",
                         StringComparison.OrdinalIgnoreCase));
                if (adjacentToLook &&
                    IsDirectionToken(tokens[i]))
                {
                    continue;
                }

                if (sideTokens.Length > 0)
                {
                    sideTokens.Append('_');
                }

                sideTokens.Append(tokens[i]);
            }

            return sideTokens.ToString();
        }

        static bool IsDirectionToken(string token)
        {
            return string.Equals(
                       token,
                       "l",
                       StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(
                       token,
                       "r",
                       StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(
                       token,
                       "left",
                       StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(
                       token,
                       "right",
                       StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(
                       token,
                       "up",
                       StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(
                       token,
                       "down",
                       StringComparison.OrdinalIgnoreCase);
        }

        static bool HasSideToken(string key, char side)
        {
            var tokenStart = 0;
            for (var i = 0; i <= key.Length; i++)
            {
                if (i < key.Length && char.IsLetterOrDigit(key[i]))
                {
                    continue;
                }

                var tokenLength = i - tokenStart;
                if (tokenLength == 1 &&
                    char.ToLowerInvariant(key[tokenStart]) == side)
                {
                    return true;
                }

                tokenStart = i + 1;
            }

            return false;
        }

        static bool HasDirectionalSideSuffix(
            string normalizedKey,
            string sideKey,
            char side)
        {
            if (sideKey.Length == 0 ||
                sideKey[sideKey.Length - 1] != side)
            {
                return false;
            }

            return normalizedKey.Contains("lookup") ||
                   normalizedKey.Contains("lookdown") ||
                   normalizedKey.Contains("lookleft") ||
                   normalizedKey.Contains("lookright") ||
                   normalizedKey.Contains("lookl") ||
                   normalizedKey.Contains("lookr") ||
                   normalizedKey.Contains("eyeup") ||
                   normalizedKey.Contains("eyedown") ||
                   normalizedKey.Contains("eyeleft") ||
                   normalizedKey.Contains("eyeright");
        }

        static string NormalizeBlendShapeKey(string key)
        {
            var normalized = new StringBuilder(key.Length);
            for (var i = 0; i < key.Length; i++)
            {
                var character = key[i];
                if (char.IsLetterOrDigit(character))
                {
                    normalized.Append(char.ToLowerInvariant(character));
                }
            }

            return normalized.ToString();
        }

        internal static float EvaluateEyelidFollow(
            float eyePitchDegrees,
            LookAtAngleLimits eyeAngleLimits,
            float followWeight,
            AnimationCurve followCurve)
        {
            followWeight = Mathf.Clamp01(followWeight);
            if (followWeight <= 0f ||
                followCurve == null ||
                followCurve.length == 0)
            {
                return 0f;
            }

            var normalizedPitch = NormalizeEyelidFollowPitch(
                eyePitchDegrees,
                eyeAngleLimits);
            return Mathf.Clamp01(followCurve.Evaluate(normalizedPitch)) *
                   followWeight;
        }

        internal static float NormalizeEyelidFollowPitch(
            float eyePitchDegrees,
            LookAtAngleLimits eyeAngleLimits)
        {
            if (float.IsNaN(eyePitchDegrees) ||
                float.IsInfinity(eyePitchDegrees))
            {
                return 0.5f;
            }

            var verticalLimits = eyeAngleLimits.Sanitized().vertical;
            var downRange = verticalLimits.x < -0.0001f
                ? Mathf.Min(
                    -verticalLimits.x,
                    DefaultEyelidFollowPitchRange)
                : DefaultEyelidFollowPitchRange;
            var upRange = verticalLimits.y > 0.0001f
                ? Mathf.Min(
                    verticalLimits.y,
                    DefaultEyelidFollowPitchRange)
                : DefaultEyelidFollowPitchRange;

            if (eyePitchDegrees < 0f)
            {
                return 0.5f - 0.5f * Mathf.Clamp01(
                    -eyePitchDegrees / downRange);
            }

            if (eyePitchDegrees > 0f)
            {
                return 0.5f + 0.5f * Mathf.Clamp01(
                    eyePitchDegrees / upRange);
            }

            return 0.5f;
        }

        internal static float EvaluateHorizontalEyelidFollow(
            float eyeYawDegrees,
            LookAtAngleLimits eyeAngleLimits,
            float followWeight,
            AnimationCurve followCurve,
            LookAtEyelidDirection direction)
        {
            followWeight = Mathf.Clamp01(followWeight);
            if (followWeight <= 0f ||
                followCurve == null ||
                followCurve.length == 0)
            {
                return 0f;
            }

            var normalizedYaw = NormalizeEyelidFollowYaw(
                eyeYawDegrees,
                eyeAngleLimits);
            if (direction == LookAtEyelidDirection.Left &&
                normalizedYaw >= 0.5f)
            {
                return 0f;
            }

            if (direction == LookAtEyelidDirection.Right &&
                normalizedYaw <= 0.5f)
            {
                return 0f;
            }

            return Mathf.Clamp01(followCurve.Evaluate(normalizedYaw)) *
                   followWeight;
        }

        internal static float NormalizeEyelidFollowYaw(
            float eyeYawDegrees,
            LookAtAngleLimits eyeAngleLimits)
        {
            if (float.IsNaN(eyeYawDegrees) ||
                float.IsInfinity(eyeYawDegrees))
            {
                return 0.5f;
            }

            var horizontalLimits = eyeAngleLimits.Sanitized().horizontal;
            var leftRange = horizontalLimits.x < -0.0001f
                ? Mathf.Min(
                    -horizontalLimits.x,
                    DefaultEyelidFollowYawRange)
                : DefaultEyelidFollowYawRange;
            var rightRange = horizontalLimits.y > 0.0001f
                ? Mathf.Min(
                    horizontalLimits.y,
                    DefaultEyelidFollowYawRange)
                : DefaultEyelidFollowYawRange;

            if (eyeYawDegrees < 0f)
            {
                return 0.5f - 0.5f * Mathf.Clamp01(
                    -eyeYawDegrees / leftRange);
            }

            if (eyeYawDegrees > 0f)
            {
                return 0.5f + 0.5f * Mathf.Clamp01(
                    eyeYawDegrees / rightRange);
            }

            return 0.5f;
        }



        internal static float NormalizeEyePitch(
            float eyePitchDegrees,
            LookAtAngleLimits eyeAngleLimits)
        {
            if (float.IsNaN(eyePitchDegrees) ||
                float.IsInfinity(eyePitchDegrees))
            {
                return 0.5f;
            }

            var verticalLimits = eyeAngleLimits.Sanitized().vertical;
            if (eyePitchDegrees < 0f)
            {
                if (verticalLimits.x >= -0.0001f) return 0.5f;

                return 0.5f * (1f - Mathf.Clamp01(
                    eyePitchDegrees / verticalLimits.x));
            }

            if (eyePitchDegrees > 0f)
            {
                if (verticalLimits.y <= 0.0001f) return 0.5f;

                return 0.5f + 0.5f * Mathf.Clamp01(
                    eyePitchDegrees / verticalLimits.y);
            }

            return 0.5f;
        }

        internal static float CombineEyelidClosures(
            float blinkClosure,
            float followClosure)
        {
            blinkClosure = Mathf.Clamp01(blinkClosure);
            followClosure = Mathf.Clamp01(followClosure);
            return 1f -
                   (1f - blinkClosure) *
                   (1f - followClosure);
        }



        internal static bool TryGetEyeCenter(
            Animator animator,
            out Vector3 eyeCenter)
        {
            return TryGetEyeCenter(animator, null, out eyeCenter);
        }

        internal static bool TryGetEyeCenter(
            Animator animator,
            LookAtTrack track,
            out Vector3 eyeCenter)
        {
            eyeCenter = default;

            if (!HumanoidIKUtility.IsUsableHumanoid(animator))
            {
                if (!LookAtGenericRigUtility.TryResolve(
                        animator,
                        track,
                        out var genericRig))
                {
                    return false;
                }

                if (genericRig.LeftEye && genericRig.RightEye)
                {
                    eyeCenter =
                        (genericRig.LeftEye.position +
                         genericRig.RightEye.position) * 0.5f;
                    return true;
                }

                if (genericRig.LeftEye || genericRig.RightEye)
                {
                    eyeCenter = genericRig.LeftEye
                        ? genericRig.LeftEye.position
                        : genericRig.RightEye.position;
                    return true;
                }

                eyeCenter = genericRig.Head.position;
                return true;
            }

            var leftEye = animator.GetBoneTransform(HumanBodyBones.LeftEye);
            var rightEye = animator.GetBoneTransform(HumanBodyBones.RightEye);
            if (leftEye && rightEye)
            {
                eyeCenter = (leftEye.position + rightEye.position) * 0.5f;
                return true;
            }

            if (leftEye || rightEye)
            {
                eyeCenter = leftEye ? leftEye.position : rightEye.position;
                return true;
            }

            var head = animator.GetBoneTransform(HumanBodyBones.Head);
            if (!head) return false;

            eyeCenter = head.position;
            return true;
        }

        internal static bool TryEvaluateState(
            in LookAtState state,
            out LookAtEvaluatedState evaluatedState)
        {
            evaluatedState = default;
            if (!state.Active || state.Samples == null || state.SampleCount <= 0) return false;

            var eyes = new ChannelAccumulator();
            var head = new ChannelAccumulator();
            var neck = new ChannelAccumulator();
            var body = new ChannelAccumulator();

            for (var i = 0; i < state.SampleCount && i < state.Samples.Length; i++)
            {
                var sample = state.Samples[i];
                if (sample.TimelineWeight <= 0f) continue;

                var targetPosition = sample.ResolveTargetPosition();
                eyes.Add(
                    targetPosition,
                    sample.TimelineWeight * sample.ResolveEyesWeight(),
                    sample.ResolveEyesAngleLimits());
                head.Add(
                    targetPosition,
                    sample.TimelineWeight * sample.ResolveHeadWeight(),
                    sample.ResolveHeadAngleLimits(),
                    sample.ResolveChinPitchOffsetDegrees());
                neck.Add(
                    targetPosition,
                    sample.TimelineWeight * sample.ResolveNeckWeight(),
                    sample.ResolveNeckAngleLimits());
                body.Add(
                    targetPosition,
                    sample.TimelineWeight * sample.ResolveBodyWeight(),
                    sample.ResolveBodyAngleLimits());
            }

            if (!eyes.HasWeight && !head.HasWeight && !neck.HasWeight && !body.HasWeight)
            {
                return false;
            }

            evaluatedState = new LookAtEvaluatedState(
                eyes.ToState(),
                head.ToState(),
                neck.ToState(),
                body.ToState());
            return true;
        }

        internal static float GetPerBoneWeight(float combinedWeight, int boneCount)
        {
            combinedWeight = Mathf.Clamp01(combinedWeight);
            if (boneCount <= 0 || combinedWeight <= 0f) return 0f;
            if (boneCount == 1 || combinedWeight >= 1f) return combinedWeight;

            return 1f - Mathf.Pow(1f - combinedWeight, 1f / boneCount);
        }

        internal static float GetGradualBoneWeight(float combinedWeight, int boneIndex, int boneCount)
        {
            combinedWeight = Mathf.Clamp01(combinedWeight);
            if (boneCount <= 0 || combinedWeight <= 0f) return 0f;
            if (boneCount == 1) return combinedWeight;

            var weightSum = boneCount * (boneCount + 1) * 0.5f;
            var ratio = (boneIndex + 1) / weightSum;
            return combinedWeight * ratio;
        }

        internal static Vector3 GetForwardInBone(Quaternion rootToBoneRotation)
        {
            return Quaternion.Inverse(rootToBoneRotation) * Vector3.forward;
        }

        internal static float GetRelativeEyePitch(
            Quaternion referenceRotation,
            Vector3 headForward,
            Vector3 eyeForward)
        {
            if (headForward.sqrMagnitude <= 0.000001f ||
                eyeForward.sqrMagnitude <= 0.000001f)
            {
                return 0f;
            }

            var inverseReference = Quaternion.Inverse(referenceRotation);
            GetYawPitch(
                inverseReference * headForward.normalized,
                out _,
                out var headPitch);
            GetYawPitch(
                inverseReference * eyeForward.normalized,
                out _,
                out var eyePitch);
            return Mathf.DeltaAngle(headPitch, eyePitch);
        }

        internal static float GetRelativeEyeYaw(
            Quaternion referenceRotation,
            Vector3 headForward,
            Vector3 eyeForward)
        {
            if (headForward.sqrMagnitude <= 0.000001f ||
                eyeForward.sqrMagnitude <= 0.000001f)
            {
                return 0f;
            }

            var inverseReference = Quaternion.Inverse(referenceRotation);
            GetYawPitch(
                inverseReference * headForward.normalized,
                out var headYaw,
                out _);
            GetYawPitch(
                inverseReference * eyeForward.normalized,
                out var eyeYaw,
                out _);
            return Mathf.DeltaAngle(headYaw, eyeYaw);
        }



        internal static Vector3 GetEyeForwardLocalPosition(
            Transform directorTransform,
            Vector3 eyeCenter)
        {
            return directorTransform
                ? directorTransform.InverseTransformPoint(eyeCenter) + Vector3.forward
                : LookAtClip.DefaultLocalPosition;
        }

        internal static Vector3 ResolveTargetPosition(
            Transform target,
            Transform directorTransform,
            Vector3 directorLocalPosition)
        {
            if (target) return target.position;

            return directorTransform
                ? directorTransform.TransformPoint(directorLocalPosition)
                : directorLocalPosition;
        }

        internal static Vector3 ClampTargetDirection(
            Vector3 currentForward,
            Vector3 targetDirection,
            Quaternion referenceRotation,
            LookAtAngleLimits angleLimits,
            float pitchOffsetDegrees = 0f)
        {
            if (currentForward.sqrMagnitude <= 0.000001f ||
                targetDirection.sqrMagnitude <= 0.000001f)
            {
                return currentForward;
            }

            var inverseReference = Quaternion.Inverse(referenceRotation);
            var currentLocal = inverseReference * currentForward.normalized;
            var targetLocal = inverseReference * targetDirection.normalized;
            GetYawPitch(currentLocal, out var currentYaw, out var currentPitch);
            GetYawPitch(targetLocal, out var targetYaw, out var targetPitch);
            if (float.IsNaN(pitchOffsetDegrees) ||
                float.IsInfinity(pitchOffsetDegrees))
            {
                pitchOffsetDegrees = 0f;
            }
            targetPitch = Mathf.Clamp(
                targetPitch + pitchOffsetDegrees,
                -90f,
                90f);

            var limits = angleLimits.Sanitized();
            var yawDelta = Mathf.Clamp(
                Mathf.DeltaAngle(currentYaw, targetYaw),
                limits.horizontal.x,
                limits.horizontal.y);
            var pitchDelta = Mathf.Clamp(
                targetPitch - currentPitch,
                limits.vertical.x,
                limits.vertical.y);

            var yaw = currentYaw + yawDelta;
            var pitch = Mathf.Clamp(currentPitch + pitchDelta, -90f, 90f);
            var yawRadians = yaw * Mathf.Deg2Rad;
            var pitchRadians = pitch * Mathf.Deg2Rad;
            var horizontalScale = Mathf.Cos(pitchRadians);
            var clampedLocal = new Vector3(
                Mathf.Sin(yawRadians) * horizontalScale,
                Mathf.Sin(pitchRadians),
                Mathf.Cos(yawRadians) * horizontalScale);
            return referenceRotation * clampedLocal;
        }

        internal static float SanitizeChinOffset(float chinOffset)
        {
            return float.IsNaN(chinOffset) || float.IsInfinity(chinOffset)
                ? LookAtClip.DefaultChinOffset
                : Mathf.Clamp(chinOffset, -1f, 1f);
        }

        internal static void GetYawPitch(Vector3 direction, out float yaw, out float pitch)
        {
            direction.Normalize();
            yaw = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            pitch = Mathf.Atan2(
                direction.y,
                Mathf.Sqrt(direction.x * direction.x + direction.z * direction.z)) * Mathf.Rad2Deg;
        }

        static void AddTransformRotation(IPropertyCollector driver, Transform transform)
        {
            if (!transform) return;

            driver.AddFromName<Transform>(transform.gameObject, "m_LocalRotation.x");
            driver.AddFromName<Transform>(transform.gameObject, "m_LocalRotation.y");
            driver.AddFromName<Transform>(transform.gameObject, "m_LocalRotation.z");
            driver.AddFromName<Transform>(transform.gameObject, "m_LocalRotation.w");
        }

        static void AddTransformRotation(
            IPropertyCollector driver,
            Transform transform,
            HashSet<Transform> gathered)
        {
            if (!transform || gathered == null || !gathered.Add(transform))
            {
                return;
            }

            AddTransformRotation(driver, transform);
        }

        struct ChannelAccumulator
        {
            Vector3 _weightedPosition;
            Vector2 _weightedHorizontalLimits;
            Vector2 _weightedVerticalLimits;
            float _weightedPitchOffsetDegrees;
            float _weight;

            internal bool HasWeight => _weight > Mathf.Epsilon;

            internal void Add(
                Vector3 targetPosition,
                float weight,
                LookAtAngleLimits angleLimits,
                float pitchOffsetDegrees = 0f)
            {
                if (weight <= 0f) return;

                var limits = angleLimits.Sanitized();
                _weightedPosition += targetPosition * weight;
                _weightedHorizontalLimits += limits.horizontal * weight;
                _weightedVerticalLimits += limits.vertical * weight;
                _weightedPitchOffsetDegrees += pitchOffsetDegrees * weight;
                _weight += weight;
            }

            internal LookAtChannelState ToState()
            {
                return HasWeight
                    ? new LookAtChannelState(
                        _weightedPosition / _weight,
                        Mathf.Clamp01(_weight),
                        new LookAtAngleLimits(
                            _weightedHorizontalLimits / _weight,
                            _weightedVerticalLimits / _weight),
                        _weightedPitchOffsetDegrees / _weight)
                    : default;
            }
        }
        internal static bool HasAnyEyelidKey(
            LookAtEyelidBlendShapeKey[] keys)
        {
            if (keys == null) return false;

            for (var i = 0; i < keys.Length; i++)
            {
                if (!string.IsNullOrEmpty(keys[i].Key)) return true;
            }

            return false;
        }

        internal static bool HasAnyKey(string[] keys)
        {
            if (keys == null) return false;

            for (var i = 0; i < keys.Length; i++)
            {
                if (!string.IsNullOrEmpty(keys[i])) return true;
            }

            return false;
        }

        static void AddBlendShapeKeys(HashSet<string> destination, string[] keys)
        {
            if (keys == null) return;

            for (var i = 0; i < keys.Length; i++)
            {
                var key = keys[i];
                if (!string.IsNullOrEmpty(key))
                {
                    destination.Add(key);
                }
            }
        }
    }
}
