using System;
using System.Collections.Generic;
using UnityEngine;

namespace CutsceneEngine
{
    public enum HumanoidIKTarget
    {
        LeftHand,
        RightHand,
        LeftFoot,
        RightFoot
    }

    public enum HumanoidIKRotationSpace
    {
        LegacyBoneRotation,
        HumanoidEffector
    }

    public enum HumanoidIKBendSpace
    {
        LegacyWorldTarget,
        HumanoidPoleDirection
    }

    public enum HumanoidIKToeRigKind
    {
        None,
        ToeFoot,
        ArticulatedToes
    }

    [Serializable]
    public struct HumanoidIKJointBend
    {
        public Vector3 proximal;
        public Vector3 intermediate;
        public Vector3 distal;

        public static HumanoidIKJointBend operator +(HumanoidIKJointBend a, HumanoidIKJointBend b)
        {
            return new HumanoidIKJointBend
            {
                proximal = a.proximal + b.proximal,
                intermediate = a.intermediate + b.intermediate,
                distal = a.distal + b.distal
            };
        }

        public static HumanoidIKJointBend operator *(HumanoidIKJointBend bend, float weight)
        {
            return new HumanoidIKJointBend
            {
                proximal = bend.proximal * weight,
                intermediate = bend.intermediate * weight,
                distal = bend.distal * weight
            };
        }
    }

    [Serializable]
    public struct HumanoidIKDigitBendPose
    {
        public HumanoidIKJointBend thumbOrBigToe;
        public HumanoidIKJointBend indexOrSecondToe;
        public HumanoidIKJointBend middleOrThirdToe;
        public HumanoidIKJointBend ringOrFourthToe;
        public HumanoidIKJointBend littleOrFifthToe;

        public static HumanoidIKDigitBendPose operator +(HumanoidIKDigitBendPose a, HumanoidIKDigitBendPose b)
        {
            return new HumanoidIKDigitBendPose
            {
                thumbOrBigToe = a.thumbOrBigToe + b.thumbOrBigToe,
                indexOrSecondToe = a.indexOrSecondToe + b.indexOrSecondToe,
                middleOrThirdToe = a.middleOrThirdToe + b.middleOrThirdToe,
                ringOrFourthToe = a.ringOrFourthToe + b.ringOrFourthToe,
                littleOrFifthToe = a.littleOrFifthToe + b.littleOrFifthToe
            };
        }

        public static HumanoidIKDigitBendPose operator *(HumanoidIKDigitBendPose pose, float weight)
        {
            return new HumanoidIKDigitBendPose
            {
                thumbOrBigToe = pose.thumbOrBigToe * weight,
                indexOrSecondToe = pose.indexOrSecondToe * weight,
                middleOrThirdToe = pose.middleOrThirdToe * weight,
                ringOrFourthToe = pose.ringOrFourthToe * weight,
                littleOrFifthToe = pose.littleOrFifthToe * weight
            };
        }
    }

    public struct HumanoidIKLimbBones
    {
        public Transform Upper;
        public Transform Lower;
        public Transform End;

        public bool IsValid => Upper && Lower && End;
    }

    internal struct HumanoidIKGoalState
    {
        public bool Active;
        public HumanoidIKSample[] Samples;
        public int SampleCount;
    }

    internal struct HumanoidIKSample
    {
        public Transform Anchor;
        public Vector3 Position;
        public Vector3 Rotation;
        public HumanoidIKRotationSpace RotationSpace;
        public int FootRotationFrameVersion;
        public Vector3 BendTarget;
        public HumanoidIKBendSpace BendSpace;
        public float TimelineWeight;
        public float PositionWeight;
        public float RotationWeight;
        public float BendWeight;
        public float DigitWeight;
        public HumanoidIKDigitBendPose DigitBends;
        public float ToeBaseBend;
        public float ToeFan;
        public Vector2[] ToeBendRanges;
        public Vector2 ToeBaseBendRange;
    }

    internal struct HumanoidIKEvaluatedState
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public Quaternion BoneToEffectorRotation;
        public bool RotationIsBoneSpace;
        public Vector3 BendDirection;
        public float PositionWeight;
        public float RotationWeight;
        public float BendWeight;
        public float DigitWeight;
        public HumanoidIKDigitBendPose DigitBends;
        public float ToeBaseBend;
        public float ToeFan;
        public Vector2[] ToeBendRanges;
        public Vector2 ToeBaseBendRange;
    }

    public static class HumanoidIKUtility
    {
        static readonly HumanBodyBones[] LeftThumb =
        {
            HumanBodyBones.LeftThumbProximal,
            HumanBodyBones.LeftThumbIntermediate,
            HumanBodyBones.LeftThumbDistal
        };

        static readonly HumanBodyBones[] LeftIndex =
        {
            HumanBodyBones.LeftIndexProximal,
            HumanBodyBones.LeftIndexIntermediate,
            HumanBodyBones.LeftIndexDistal
        };

        static readonly HumanBodyBones[] LeftMiddle =
        {
            HumanBodyBones.LeftMiddleProximal,
            HumanBodyBones.LeftMiddleIntermediate,
            HumanBodyBones.LeftMiddleDistal
        };

        static readonly HumanBodyBones[] LeftRing =
        {
            HumanBodyBones.LeftRingProximal,
            HumanBodyBones.LeftRingIntermediate,
            HumanBodyBones.LeftRingDistal
        };

        static readonly HumanBodyBones[] LeftLittle =
        {
            HumanBodyBones.LeftLittleProximal,
            HumanBodyBones.LeftLittleIntermediate,
            HumanBodyBones.LeftLittleDistal
        };

        static readonly HumanBodyBones[] RightThumb =
        {
            HumanBodyBones.RightThumbProximal,
            HumanBodyBones.RightThumbIntermediate,
            HumanBodyBones.RightThumbDistal
        };

        static readonly HumanBodyBones[] RightIndex =
        {
            HumanBodyBones.RightIndexProximal,
            HumanBodyBones.RightIndexIntermediate,
            HumanBodyBones.RightIndexDistal
        };

        static readonly HumanBodyBones[] RightMiddle =
        {
            HumanBodyBones.RightMiddleProximal,
            HumanBodyBones.RightMiddleIntermediate,
            HumanBodyBones.RightMiddleDistal
        };

        static readonly HumanBodyBones[] RightRing =
        {
            HumanBodyBones.RightRingProximal,
            HumanBodyBones.RightRingIntermediate,
            HumanBodyBones.RightRingDistal
        };

        static readonly HumanBodyBones[] RightLittle =
        {
            HumanBodyBones.RightLittleProximal,
            HumanBodyBones.RightLittleIntermediate,
            HumanBodyBones.RightLittleDistal
        };

        public static bool IsHand(HumanoidIKTarget target)
        {
            return target == HumanoidIKTarget.LeftHand || target == HumanoidIKTarget.RightHand;
        }

        public static bool IsFoot(HumanoidIKTarget target)
        {
            return target == HumanoidIKTarget.LeftFoot || target == HumanoidIKTarget.RightFoot;
        }

        public static bool IsUsableHumanoid(Animator animator)
        {
            return animator && animator.isHuman && animator.avatar;
        }

        public static bool TryGetLimbBones(Animator animator, HumanoidIKTarget target, out HumanoidIKLimbBones bones)
        {
            bones = default;
            if (!IsUsableHumanoid(animator)) return false;

            switch (target)
            {
                case HumanoidIKTarget.LeftHand:
                    bones.Upper = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
                    bones.Lower = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
                    bones.End = animator.GetBoneTransform(HumanBodyBones.LeftHand);
                    break;
                case HumanoidIKTarget.RightHand:
                    bones.Upper = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
                    bones.Lower = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
                    bones.End = animator.GetBoneTransform(HumanBodyBones.RightHand);
                    break;
                case HumanoidIKTarget.LeftFoot:
                    bones.Upper = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
                    bones.Lower = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
                    bones.End = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
                    break;
                case HumanoidIKTarget.RightFoot:
                    bones.Upper = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
                    bones.Lower = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
                    bones.End = animator.GetBoneTransform(HumanBodyBones.RightFoot);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(target), target, null);
            }

            return bones.IsValid;
        }

        public static void ResolveWorldPose(
            Transform anchor,
            Vector3 localOrWorldPosition,
            Vector3 localOrWorldEuler,
            Vector3 localOrWorldBendTarget,
            out Vector3 worldPosition,
            out Quaternion worldRotation,
            out Vector3 worldBendTarget)
        {
            if (anchor)
            {
                worldPosition = anchor.TransformPoint(localOrWorldPosition);
                worldRotation = anchor.rotation * Quaternion.Euler(localOrWorldEuler);
                worldBendTarget = anchor.TransformPoint(localOrWorldBendTarget);
                return;
            }

            worldPosition = localOrWorldPosition;
            worldRotation = Quaternion.Euler(localOrWorldEuler);
            worldBendTarget = localOrWorldBendTarget;
        }

        public static Vector3 ResolveWorldVector(Transform anchor, Vector3 localOrWorldVector)
        {
            return anchor
                ? anchor.TransformDirection(localOrWorldVector)
                : localOrWorldVector;
        }

        public static Vector3 ResolveWorldDirection(Transform anchor, Vector3 localOrWorldDirection)
        {
            var worldDirection = ResolveWorldVector(anchor, localOrWorldDirection);
            return worldDirection.sqrMagnitude > 0.000001f
                ? worldDirection.normalized
                : Vector3.zero;
        }

        public static Vector3 ResolveBendVector(
            Transform anchor,
            Vector3 localOrWorldBend,
            HumanoidIKBendSpace bendSpace,
            Vector3 limbRootPosition)
        {
            var worldTarget = anchor
                ? anchor.TransformPoint(localOrWorldBend)
                : localOrWorldBend;
            return worldTarget - limbRootPosition;
        }

        public static Vector3 ResolveBendDirection(
            Transform anchor,
            Vector3 localOrWorldBend,
            HumanoidIKBendSpace bendSpace,
            Vector3 limbRootPosition)
        {
            var bendVector = ResolveBendVector(anchor, localOrWorldBend, bendSpace, limbRootPosition);
            return bendVector.sqrMagnitude > 0.000001f ? bendVector.normalized : Vector3.zero;
        }

        public static Quaternion ToEffectorRotation(
            Quaternion storedWorldRotation,
            HumanoidIKRotationSpace rotationSpace,
            Quaternion boneToEffectorRotation)
        {
            return rotationSpace == HumanoidIKRotationSpace.HumanoidEffector
                ? storedWorldRotation
                : storedWorldRotation * boneToEffectorRotation;
        }

        public static Quaternion ToBoneRotation(
            Quaternion effectorWorldRotation,
            Quaternion boneToEffectorRotation)
        {
            return effectorWorldRotation * Quaternion.Inverse(boneToEffectorRotation);
        }

        internal static Quaternion ToProjectedSoleRotation(
            Quaternion storedWorldRotation,
            HumanoidIKRotationSpace rotationSpace,
            int footRotationFrameVersion,
            Quaternion boneToSoleRotation,
            Quaternion boneToLegacyFootLineRotation)
        {
            if (rotationSpace != HumanoidIKRotationSpace.HumanoidEffector)
            {
                return ToEffectorRotation(
                    storedWorldRotation,
                    rotationSpace,
                    boneToSoleRotation);
            }

            if (footRotationFrameVersion >= HumanoidIKClip.CurrentFootRotationFrameVersion)
            {
                return storedWorldRotation;
            }

            // Old Foot effector clips stored the sloped ankle-to-toe frame. Convert
            // through the imported Foot bone so the bone pose stays unchanged while
            // the authored/display frame becomes the projected sole frame.
            return storedWorldRotation *
                   Quaternion.Inverse(boneToLegacyFootLineRotation) *
                   boneToSoleRotation;
        }

        internal static bool TryBuildFootDisplayRotation(
            Matrix4x4 referenceFootMatrix,
            bool hasReferenceToeMatrix,
            Matrix4x4 referenceToeMatrix,
            Matrix4x4 referenceLowerLegMatrix,
            out Quaternion displayRotation)
        {
            return TryBuildFootDisplayRotation(
                referenceFootMatrix,
                hasReferenceToeMatrix,
                referenceToeMatrix,
                referenceLowerLegMatrix,
                projectAnkleToToeOntoSole: true,
                out displayRotation);
        }

        internal static bool TryBuildLegacyFootLineRotation(
            Matrix4x4 referenceFootMatrix,
            bool hasReferenceToeMatrix,
            Matrix4x4 referenceToeMatrix,
            Matrix4x4 referenceLowerLegMatrix,
            out Quaternion displayRotation)
        {
            return TryBuildFootDisplayRotation(
                referenceFootMatrix,
                hasReferenceToeMatrix,
                referenceToeMatrix,
                referenceLowerLegMatrix,
                projectAnkleToToeOntoSole: false,
                out displayRotation);
        }

        static bool TryBuildFootDisplayRotation(
            Matrix4x4 referenceFootMatrix,
            bool hasReferenceToeMatrix,
            Matrix4x4 referenceToeMatrix,
            Matrix4x4 referenceLowerLegMatrix,
            bool projectAnkleToToeOntoSole,
            out Quaternion displayRotation)
        {
            displayRotation = Quaternion.identity;
            var footPosition = referenceFootMatrix.MultiplyPoint3x4(Vector3.zero);
            var ankleToToe = hasReferenceToeMatrix
                ? referenceToeMatrix.MultiplyPoint3x4(Vector3.zero) - footPosition
                : Vector3.forward;
            // A Humanoid Foot transform normally points from the elevated ankle to
            // the lower Toes transform. That slope describes anatomy, not the sole.
            // Sole +Z is strictly the Avatar-up planar projection; the vertical
            // component is consumed separately by editor geometry as sole height.
            var forward = projectAnkleToToeOntoSole
                ? Vector3.ProjectOnPlane(ankleToToe, Vector3.up)
                : ankleToToe;
            if (forward.sqrMagnitude <= 0.000001f) return false;
            forward.Normalize();

            var preferredUp = Vector3.ProjectOnPlane(Vector3.up, forward);
            var legUp = Vector3.ProjectOnPlane(
                referenceLowerLegMatrix.MultiplyPoint3x4(Vector3.zero) - footPosition,
                forward);
            if (preferredUp.sqrMagnitude <= 0.000001f)
            {
                preferredUp = legUp;
            }
            else if (legUp.sqrMagnitude > 0.000001f && Vector3.Dot(preferredUp, legUp) < 0f)
            {
                preferredUp = -preferredUp;
            }

            if (preferredUp.sqrMagnitude <= 0.000001f)
            {
                preferredUp = Vector3.ProjectOnPlane(Vector3.forward, forward);
            }
            if (preferredUp.sqrMagnitude <= 0.000001f)
            {
                preferredUp = Vector3.ProjectOnPlane(Vector3.right, forward);
            }
            if (preferredUp.sqrMagnitude <= 0.000001f) return false;

            displayRotation = Quaternion.LookRotation(forward, preferredUp.normalized);
            return true;
        }

        public static void GetDigitChains(Animator animator, HumanoidIKTarget target, List<Transform[]> chains)
        {
            chains.Clear();
            var cachedChains = HumanoidIKDigitChainCache.GetChains(animator, target);
            for (var i = 0; i < cachedChains.Length; i++) chains.Add(cachedChains[i]);
        }

        public static HumanoidIKToeRigKind GetToeRigKind(Animator animator, HumanoidIKTarget target)
        {
            if (!IsUsableHumanoid(animator) || !IsFoot(target)) return HumanoidIKToeRigKind.None;

            var toes = GetToeRoot(animator, target);
            if (!toes) return HumanoidIKToeRigKind.None;

            return toes.childCount >= 2
                ? HumanoidIKToeRigKind.ArticulatedToes
                : HumanoidIKToeRigKind.ToeFoot;
        }

        public static Transform GetToeRoot(Animator animator, HumanoidIKTarget target)
        {
            if (!animator || !IsFoot(target)) return null;

            return animator.GetBoneTransform(target == HumanoidIKTarget.LeftFoot
                ? HumanBodyBones.LeftToes
                : HumanBodyBones.RightToes);
        }

        public static HumanoidIKJointBend GetArticulatedToeFanOffset(int digitIndex, float fan)
        {
            fan = Mathf.Clamp(fan, -1f, 1f);
            var fanCoefficient = Mathf.Lerp(1f, -1f, Mathf.Clamp(digitIndex, 0, 4) / 4f);

            return new HumanoidIKJointBend
            {
                proximal = new Vector3(0f, fan * fanCoefficient * 8f, 0f)
            };
        }

        internal static bool TryGetToeAnatomicalRotation(
            Vector3 authoredEuler,
            Vector3 toeForward,
            Vector3 preferredUp,
            bool isLeftFoot,
            out Quaternion rotation)
        {
            rotation = Quaternion.identity;
            if (toeForward.sqrMagnitude <= 0.000001f) return false;

            var forward = toeForward.normalized;
            var up = Vector3.ProjectOnPlane(preferredUp, forward);
            if (up.sqrMagnitude <= 0.000001f) return false;

            var anatomicalFrame = Quaternion.LookRotation(forward, up.normalized);
            var mirrorSign = isLeftFoot ? 1f : -1f;
            var anatomicalOffset =
                Quaternion.AngleAxis(authoredEuler.y * mirrorSign, Vector3.up) *
                Quaternion.AngleAxis(-authoredEuler.x, Vector3.right) *
                Quaternion.AngleAxis(authoredEuler.z * mirrorSign, Vector3.forward);
            rotation = anatomicalFrame * anatomicalOffset * Quaternion.Inverse(anatomicalFrame);
            return true;
        }

        public static Vector2 GetDefaultToeBendRange(int jointIndex)
        {
            return jointIndex switch
            {
                0 => new Vector2(-25f, 20f),
                1 => new Vector2(-18f, 8f),
                2 => new Vector2(-12f, 5f),
                _ => Vector2.zero
            };
        }

        public static Vector2 GetDefaultToeBaseBendRange()
        {
            return new Vector2(-25f, 20f);
        }

        public static float GetToeBaseBendAngle(float pose, Vector2 range = default)
        {
            if (range == default) range = GetDefaultToeBaseBendRange();
            pose = Mathf.Clamp(pose, -1f, 1f);
            return pose < 0f ? pose * Mathf.Abs(range.x) : pose * range.y;
        }

        public static float GetToeBaseBendAngle(float pose)
        {
            return GetToeBaseBendAngle(pose, GetDefaultToeBaseBendRange());
        }

        public static HumanoidIKJointBend ClampToeBend(HumanoidIKJointBend bend, Vector2[] toeBendRanges = null)
        {
            var range0 = toeBendRanges != null && toeBendRanges.Length > 0 ? toeBendRanges[0] : GetDefaultToeBendRange(0);
            var range1 = toeBendRanges != null && toeBendRanges.Length > 1 ? toeBendRanges[1] : GetDefaultToeBendRange(1);
            var range2 = toeBendRanges != null && toeBendRanges.Length > 2 ? toeBendRanges[2] : GetDefaultToeBendRange(2);
            bend.proximal = ClampToeJointEuler(bend.proximal, 0, range0);
            bend.intermediate = ClampToeJointEuler(bend.intermediate, 1, range1);
            bend.distal = ClampToeJointEuler(bend.distal, 2, range2);
            return bend;
        }

        public static HumanoidIKJointBend ClampToeFootBend(HumanoidIKJointBend bend, Vector2[] toeBendRanges = null)
        {
            bend = ClampToeBend(bend, toeBendRanges);
            bend.proximal = new Vector3(bend.proximal.x, 0f, 0f);
            bend.intermediate = Vector3.zero;
            bend.distal = Vector3.zero;
            return bend;
        }

        public static HumanoidIKJointBend GetDigitBend(in HumanoidIKDigitBendPose pose, int index)
        {
            return index switch
            {
                0 => pose.thumbOrBigToe,
                1 => pose.indexOrSecondToe,
                2 => pose.middleOrThirdToe,
                3 => pose.ringOrFourthToe,
                4 => pose.littleOrFifthToe,
                _ => default
            };
        }

        internal static bool TryGetHandDigitBoneIds(
            HumanoidIKTarget target,
            int digitIndex,
            out HumanBodyBones[] boneIds)
        {
            boneIds = null;
            if (!IsHand(target)) return false;

            var leftHand = target == HumanoidIKTarget.LeftHand;
            boneIds = digitIndex switch
            {
                0 => leftHand ? LeftThumb : RightThumb,
                1 => leftHand ? LeftIndex : RightIndex,
                2 => leftHand ? LeftMiddle : RightMiddle,
                3 => leftHand ? LeftRing : RightRing,
                4 => leftHand ? LeftLittle : RightLittle,
                _ => null
            };
            return boneIds != null;
        }

        public static Quaternion Normalize(Quaternion rotation)
        {
            var length = Mathf.Sqrt(
                rotation.x * rotation.x +
                rotation.y * rotation.y +
                rotation.z * rotation.z +
                rotation.w * rotation.w);

            if (length <= Mathf.Epsilon) return Quaternion.identity;

            var inverse = 1f / length;
            return new Quaternion(
                rotation.x * inverse,
                rotation.y * inverse,
                rotation.z * inverse,
                rotation.w * inverse);
        }

        static Vector3 ClampToeJointEuler(Vector3 euler, int jointIndex, Vector2 bendRange)
        {
            euler.x = Mathf.Clamp(euler.x, bendRange.x, bendRange.y);
            euler.y = Mathf.Clamp(euler.y, -10f, 10f);
            euler.z = Mathf.Clamp(euler.z, -10f, 10f);
            return euler;
        }

    }
    internal readonly struct HumanoidIKMuscleBinding
    {
        public readonly int Index;
        public readonly float NegativeRange;
        public readonly float PositiveRange;
        public readonly bool IsValid;

        HumanoidIKMuscleBinding(
            int index,
            float negativeRange,
            float positiveRange)
        {
            Index = index;
            NegativeRange = negativeRange;
            PositiveRange = positiveRange;
            IsValid = index >= 0;
        }

        internal static HumanoidIKMuscleBinding Create(
            HumanBodyBones bone,
            int degreeOfFreedom)
        {
            var index = HumanTrait.MuscleFromBone((int)bone, degreeOfFreedom);
            return index < 0
                ? default
                : new HumanoidIKMuscleBinding(
                    index,
                    Mathf.Abs(HumanTrait.GetMuscleDefaultMin(index)),
                    Mathf.Abs(HumanTrait.GetMuscleDefaultMax(index)));
        }

        internal float GetValue(float angle)
        {
            if (!IsValid || Mathf.Approximately(angle, 0f)) return 0f;

            var range = angle < 0f ? NegativeRange : PositiveRange;
            return range > Mathf.Epsilon ? angle / range : 0f;
        }
    }


    internal sealed class HumanoidIKHumanPoseSolver : IDisposable
    {
        const int StretchDegreeOfFreedom = 2;
        const int SpreadDegreeOfFreedom = 1;

        readonly Animator _animator;
        readonly Avatar _avatar;
        readonly HumanPoseHandler _poseHandler;
        readonly List<Transform[]> _digitChains = new List<Transform[]>();
        readonly Transform[] _restoreTransforms;
        readonly Vector3[] _restoreLocalPositions;
        readonly Quaternion[] _restoreLocalRotations;
        readonly HumanoidIKReferencePose _referencePose;
        readonly Quaternion[] _boneToEffectorRotations = new Quaternion[4];
        readonly HumanoidIKMuscleBinding[] _handMuscleBindings = new HumanoidIKMuscleBinding[40];
        readonly bool[] _hasBoneToEffectorRotation = new bool[4];
        readonly Quaternion[] _legacyFootBoneToEffectorRotations = new Quaternion[4];
        readonly bool[] _hasLegacyFootBoneToEffectorRotation = new bool[4];
        readonly Quaternion[] _referenceFootDisplayRotations = new Quaternion[4];
        readonly bool[] _hasReferenceFootDisplayRotation = new bool[4];

        HumanPose _humanPose;
        bool _disposed;

        internal HumanoidIKReferencePose ReferencePose => _referencePose;

        public bool IsValidFor(Animator animator)
        {
            return !_disposed && animator == _animator && animator && animator.avatar == _avatar;
        }

        public static bool TryCreate(Animator animator, out HumanoidIKHumanPoseSolver solver)
        {
            solver = null;
            if (!HumanoidIKUtility.IsUsableHumanoid(animator)) return false;

            try
            {
                solver = new HumanoidIKHumanPoseSolver(animator);
                return true;
            }
            catch (ArgumentException)
            {
                solver?.Dispose();
                solver = null;
                return false;
            }
            catch (InvalidOperationException)
            {
                solver?.Dispose();
                solver = null;
                return false;
            }
        }

        HumanoidIKHumanPoseSolver(Animator animator)
        {
            _animator = animator;
            _avatar = animator.avatar;
            _poseHandler = new HumanPoseHandler(_avatar, animator.transform);
            HumanoidIKReferencePose.TryCreate(animator, out var referencePose);
            _referencePose = referencePose;

            var transforms = new List<Transform>();
            AddUniqueTransform(transforms, animator.transform);
            AddUniqueTransform(transforms, animator.avatarRoot);
            for (var i = 0; i < (int)HumanBodyBones.LastBone; i++)
            {
                AddUniqueTransform(transforms, animator.GetBoneTransform((HumanBodyBones)i));
            }
            CacheHandMuscleBindings();

            _restoreTransforms = transforms.ToArray();
            _restoreLocalPositions = new Vector3[_restoreTransforms.Length];
            _restoreLocalRotations = new Quaternion[_restoreTransforms.Length];
            CacheBoneToEffectorRotations();
        }

        public bool TryGetBoneToEffectorRotation(
            HumanoidIKTarget target,
            out Quaternion boneToEffectorRotation)
        {
            var index = (int)target;
            if (index >= 0 && index < _boneToEffectorRotations.Length && _hasBoneToEffectorRotation[index])
            {
                boneToEffectorRotation = _boneToEffectorRotations[index];
                return true;
            }

            boneToEffectorRotation = Quaternion.identity;
            return false;
        }

        internal bool TryGetLegacyFootBoneToEffectorRotation(
            HumanoidIKTarget target,
            out Quaternion boneToEffectorRotation)
        {
            var index = (int)target;
            if (index >= 0 &&
                index < _legacyFootBoneToEffectorRotations.Length &&
                _hasLegacyFootBoneToEffectorRotation[index])
            {
                boneToEffectorRotation = _legacyFootBoneToEffectorRotations[index];
                return true;
            }

            boneToEffectorRotation = Quaternion.identity;
            return false;
        }

        internal bool TryGetReferenceFootDisplayRotation(
            HumanoidIKTarget target,
            out Quaternion displayRotation)
        {
            var index = (int)target;
            if (index >= 0 &&
                index < _referenceFootDisplayRotations.Length &&
                _hasReferenceFootDisplayRotation[index])
            {
                displayRotation = _referenceFootDisplayRotations[index];
                return true;
            }

            displayRotation = Quaternion.identity;
            return false;
        }

        public bool TryGetReferenceLocalRotation(Transform bone, out Quaternion localRotation)
        {
            localRotation = Quaternion.identity;
            if (_referencePose == null || !_referencePose.TryGetBonePose(bone, out var referenceBone))
            {
                return false;
            }

            localRotation = referenceBone.Rotation;
            return true;
        }

        public bool TryResolveHandLocalRotations(
            bool resolveLeft,
            in HumanoidIKDigitBendPose leftPose,
            float leftWeight,
            bool resolveRight,
            in HumanoidIKDigitBendPose rightPose,
            float rightWeight,
            Dictionary<Transform, Quaternion> resolvedRotations)
        {
            resolvedRotations.Clear();
            if (_disposed || !IsValidFor(_animator) || (!resolveLeft && !resolveRight)) return false;

            _poseHandler.GetHumanPose(ref _humanPose);
            if (_humanPose.muscles == null || _humanPose.muscles.Length != HumanTrait.MuscleCount) return false;

            if (resolveLeft)
            {
                ApplyHandMuscles(
                    HumanoidIKTarget.LeftHand,
                    in leftPose,
                    Mathf.Clamp01(leftWeight),
                    _humanPose.muscles);
            }

            if (resolveRight)
            {
                ApplyHandMuscles(
                    HumanoidIKTarget.RightHand,
                    in rightPose,
                    Mathf.Clamp01(rightWeight),
                    _humanPose.muscles);
            }

            CaptureCurrentTransforms();
            try
            {
                _poseHandler.SetHumanPose(ref _humanPose);
                if (resolveLeft)
                {
                    CaptureHandRotations(HumanoidIKTarget.LeftHand, resolvedRotations);
                }

                if (resolveRight)
                {
                    CaptureHandRotations(HumanoidIKTarget.RightHand, resolvedRotations);
                }
            }
            finally
            {
                RestoreCapturedTransforms();
            }

            return resolvedRotations.Count > 0;
        }

        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;
            _poseHandler.Dispose();
        }

        void CacheBoneToEffectorRotations()
        {
            // Foot clips author the canonical anatomical frame (+Z toes, +Y dorsum).
            // Convert that frame to each imported Foot bone axis using the same
            // immutable Avatar skeleton that supplies toe reference rotations.
            CacheReferenceFootBoneToEffectorRotation(HumanoidIKTarget.LeftFoot);
            CacheReferenceFootBoneToEffectorRotation(HumanoidIKTarget.RightFoot);

            CaptureCurrentTransforms();
            try
            {
                _poseHandler.GetHumanPose(ref _humanPose);
                if (_humanPose.muscles == null || _humanPose.muscles.Length != HumanTrait.MuscleCount) return;

                var sourceMuscles = new float[_humanPose.muscles.Length];
                Array.Copy(_humanPose.muscles, sourceMuscles, sourceMuscles.Length);
                Array.Clear(_humanPose.muscles, 0, _humanPose.muscles.Length);

                try
                {
                    _poseHandler.SetHumanPose(ref _humanPose);
                    for (var i = (int)HumanoidIKTarget.LeftHand;
                         i <= (int)HumanoidIKTarget.RightHand;
                         i++)
                    {
                        var target = (HumanoidIKTarget)i;
                        if (!TryBuildEffectorFrameRotation(target, out var effectorWorldRotation) ||
                            !HumanoidIKUtility.TryGetLimbBones(_animator, target, out var limb))
                        {
                            continue;
                        }

                        _boneToEffectorRotations[i] =
                            Quaternion.Inverse(limb.End.rotation) * effectorWorldRotation;
                        _hasBoneToEffectorRotation[i] = true;
                    }
                }
                finally
                {
                    Array.Copy(sourceMuscles, _humanPose.muscles, sourceMuscles.Length);
                }
            }
            catch (ArgumentException)
            {
                ClearHandBoneToEffectorRotations();
            }
            catch (InvalidOperationException)
            {
                ClearHandBoneToEffectorRotations();
            }
            finally
            {
                RestoreCapturedTransforms();
            }
        }

        void CacheReferenceFootBoneToEffectorRotation(HumanoidIKTarget target)
        {
            if (!HumanoidIKUtility.IsFoot(target) ||
                !HumanoidIKUtility.TryGetLimbBones(_animator, target, out var limb) ||
                _referencePose == null ||
                !_referencePose.TryGetRelativeMatrix(_animator.transform, limb.End, out var footMatrix) ||
                !_referencePose.TryGetRelativeMatrix(_animator.transform, limb.Lower, out var lowerMatrix))
            {
                return;
            }

            var toes = _animator.GetBoneTransform(target == HumanoidIKTarget.LeftFoot
                ? HumanBodyBones.LeftToes
                : HumanBodyBones.RightToes);
            var toeMatrix = Matrix4x4.identity;
            var hasToeMatrix = toes &&
                               _referencePose.TryGetRelativeMatrix(
                                   _animator.transform,
                                   toes,
                                   out toeMatrix);
            if (!HumanoidIKUtility.TryBuildFootDisplayRotation(
                    footMatrix,
                    hasToeMatrix,
                    toeMatrix,
                    lowerMatrix,
                    out var referenceDisplayRotation))
            {
                return;
            }

            var targetIndex = (int)target;
            _referenceFootDisplayRotations[targetIndex] = referenceDisplayRotation;
            _hasReferenceFootDisplayRotation[targetIndex] = true;
            _boneToEffectorRotations[targetIndex] =
                Quaternion.Inverse(footMatrix.rotation) * referenceDisplayRotation;
            _hasBoneToEffectorRotation[targetIndex] = true;

            if (HumanoidIKUtility.TryBuildLegacyFootLineRotation(
                    footMatrix,
                    hasToeMatrix,
                    toeMatrix,
                    lowerMatrix,
                    out var legacyFootLineRotation))
            {
                _legacyFootBoneToEffectorRotations[targetIndex] =
                    Quaternion.Inverse(footMatrix.rotation) * legacyFootLineRotation;
                _hasLegacyFootBoneToEffectorRotation[targetIndex] = true;
            }
        }

        void ClearHandBoneToEffectorRotations()
        {
            _hasBoneToEffectorRotation[(int)HumanoidIKTarget.LeftHand] = false;
            _hasBoneToEffectorRotation[(int)HumanoidIKTarget.RightHand] = false;
        }

        bool TryBuildEffectorFrameRotation(
            HumanoidIKTarget target,
            out Quaternion effectorWorldRotation)
        {
            effectorWorldRotation = Quaternion.identity;
            if (!HumanoidIKUtility.IsHand(target) ||
                !HumanoidIKUtility.TryGetLimbBones(_animator, target, out var limb))
            {
                return false;
            }

            var forward = Vector3.zero;
            var handWidth = Vector3.zero;
            var baseCenter = Vector3.zero;
            var baseCount = 0;
            Transform indexBase = null;
            Transform littleBase = null;
            for (var digitIndex = 1; digitIndex < 5; digitIndex++)
            {
                if (!HumanoidIKUtility.TryGetHandDigitBoneIds(target, digitIndex, out var boneIds)) continue;

                var proximal = _animator.GetBoneTransform(boneIds[0]);
                if (!proximal) continue;

                baseCenter += proximal.position;
                baseCount++;
                if (digitIndex == 1) indexBase = proximal;
                if (digitIndex == 4) littleBase = proximal;
            }

            if (baseCount > 0)
            {
                forward = baseCenter / baseCount - limb.End.position;
            }

            if (indexBase && littleBase)
            {
                handWidth = indexBase.position - littleBase.position;
            }

            if (forward.sqrMagnitude <= 0.000001f)
            {
                forward = limb.End.position - limb.Lower.position;
            }

            if (forward.sqrMagnitude <= 0.000001f) return false;
            forward.Normalize();

            var preferredUp = -_animator.transform.up;
            handWidth = Vector3.ProjectOnPlane(handWidth, forward);
            if (handWidth.sqrMagnitude > 0.000001f)
            {
                // The little-to-index span and wrist-to-finger direction define
                // the actual palm plane. Imported hand bone axes are irrelevant.
                var palmNormal = Vector3.Cross(handWidth.normalized, forward);
                if (Vector3.Dot(palmNormal, preferredUp) < 0f)
                {
                    palmNormal = -palmNormal;
                }

                preferredUp = palmNormal;
            }

            var up = Vector3.ProjectOnPlane(preferredUp, forward);
            if (up.sqrMagnitude <= 0.000001f)
            {
                up = Vector3.ProjectOnPlane(_animator.transform.forward, forward);
            }

            if (up.sqrMagnitude <= 0.000001f)
            {
                up = Vector3.ProjectOnPlane(_animator.transform.right, forward);
            }

            if (up.sqrMagnitude <= 0.000001f) return false;

            effectorWorldRotation = Quaternion.LookRotation(forward, up.normalized);
            return true;
        }

        static void AddUniqueTransform(List<Transform> transforms, Transform candidate)
        {
            if (candidate && !transforms.Contains(candidate)) transforms.Add(candidate);
        }

        void CacheHandMuscleBindings()
        {
            for (var handIndex = 0; handIndex < 2; handIndex++)
            {
                var target = handIndex == 0
                    ? HumanoidIKTarget.LeftHand
                    : HumanoidIKTarget.RightHand;
                for (var digitIndex = 0; digitIndex < 5; digitIndex++)
                {
                    if (!HumanoidIKUtility.TryGetHandDigitBoneIds(
                            target,
                            digitIndex,
                            out var boneIds))
                    {
                        continue;
                    }

                    var bindingOffset = handIndex * 20 + digitIndex * 4;
                    for (var jointIndex = 0;
                         jointIndex < boneIds.Length && jointIndex < 3;
                         jointIndex++)
                    {
                        _handMuscleBindings[bindingOffset + jointIndex] =
                            HumanoidIKMuscleBinding.Create(
                                boneIds[jointIndex],
                                StretchDegreeOfFreedom);
                    }

                    _handMuscleBindings[bindingOffset + 3] =
                        HumanoidIKMuscleBinding.Create(
                            boneIds[0],
                            SpreadDegreeOfFreedom);
                }
            }
        }

        void ApplyHandMuscles(
            HumanoidIKTarget target,
            in HumanoidIKDigitBendPose pose,
            float weight,
            float[] muscles)
        {
            var handOffset = target == HumanoidIKTarget.LeftHand ? 0 : 20;
            for (var digitIndex = 0; digitIndex < 5; digitIndex++)
            {
                var bindingOffset = handOffset + digitIndex * 4;

                var bend = HumanoidIKUtility.GetDigitBend(in pose, digitIndex);
                for (var jointIndex = 0; jointIndex < 3; jointIndex++)
                {
                    var angle = jointIndex switch
                    {
                        0 => bend.proximal.x,
                        1 => bend.intermediate.x,
                        2 => bend.distal.x,
                        _ => 0f
                    };
                    SetMuscleAngle(
                        in _handMuscleBindings[bindingOffset + jointIndex],
                        angle,
                        weight,
                        muscles);
                }

                SetMuscleAngle(
                    in _handMuscleBindings[bindingOffset + 3],
                    bend.proximal.y,
                    weight,
                    muscles);
            }
        }

        static void SetMuscleAngle(
            in HumanoidIKMuscleBinding binding,
            float angle,
            float weight,
            float[] muscles)
        {
            if (!binding.IsValid ||
                binding.Index < 0 ||
                binding.Index >= muscles.Length)
            {
                return;
            }

            var targetValue = binding.GetValue(angle);
            muscles[binding.Index] = Mathf.LerpUnclamped(
                muscles[binding.Index],
                targetValue,
                weight);
        }

        static float GetMuscleValueFromAngle(int muscleIndex, float angle)
        {
            if (Mathf.Approximately(angle, 0f)) return 0f;

            var limit = angle < 0f
                ? Mathf.Abs(HumanTrait.GetMuscleDefaultMin(muscleIndex))
                : Mathf.Abs(HumanTrait.GetMuscleDefaultMax(muscleIndex));
            return limit > Mathf.Epsilon ? angle / limit : 0f;
        }

        void CaptureHandRotations(
            HumanoidIKTarget target,
            Dictionary<Transform, Quaternion> resolvedRotations)
        {
            HumanoidIKUtility.GetDigitChains(_animator, target, _digitChains);
            for (var i = 0; i < _digitChains.Count; i++)
            {
                var chain = _digitChains[i];
                if (chain == null) continue;

                for (var j = 0; j < chain.Length; j++)
                {
                    var bone = chain[j];
                    if (bone) resolvedRotations[bone] = bone.localRotation;
                }
            }
        }

        void CaptureCurrentTransforms()
        {
            for (var i = 0; i < _restoreTransforms.Length; i++)
            {
                var bone = _restoreTransforms[i];
                if (!bone) continue;

                _restoreLocalPositions[i] = bone.localPosition;
                _restoreLocalRotations[i] = bone.localRotation;
            }
        }

        void RestoreCapturedTransforms()
        {
            for (var i = 0; i < _restoreTransforms.Length; i++)
            {
                var bone = _restoreTransforms[i];
                if (!bone) continue;

                bone.localPosition = _restoreLocalPositions[i];
                bone.localRotation = _restoreLocalRotations[i];
            }
        }
    }

    internal struct HumanoidIKQuaternionAccumulator
    {
        Vector4 _value;
        Quaternion _reference;
        bool _hasReference;

        public void Add(Quaternion rotation, float weight)
        {
            if (weight <= 0f) return;

            if (!_hasReference)
            {
                _reference = rotation;
                _hasReference = true;
            }
            else if (Quaternion.Dot(_reference, rotation) < 0f)
            {
                rotation = new Quaternion(-rotation.x, -rotation.y, -rotation.z, -rotation.w);
            }

            _value += new Vector4(rotation.x, rotation.y, rotation.z, rotation.w) * weight;
        }

        public Quaternion GetValue()
        {
            if (!_hasReference) return Quaternion.identity;
            return HumanoidIKUtility.Normalize(new Quaternion(_value.x, _value.y, _value.z, _value.w));
        }
    }
}
