using System;
using System.Collections.Generic;
using UnityEngine;

namespace CutsceneEngine
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(10000)]
    public sealed class HumanoidIKLateUpdateDriver : MonoBehaviour
    {
        struct RotationRecord
        {
            public Quaternion BaseLocalRotation;
            public Quaternion AppliedLocalRotation;
        }

        struct ToeReferenceFrame
        {
            public Transform NextBone;
            public Transform PreviousBone;
            public Vector3 ForwardInBone;
            public Vector3 UpInBone;
        }

        readonly HumanoidIKGoalState[] _states = new HumanoidIKGoalState[4];
        readonly HumanoidIKEvaluatedState[] _evaluatedStates = new HumanoidIKEvaluatedState[4];
        readonly bool[] _hasEvaluatedState = new bool[4];
        readonly bool[] _reportedMissingEffectorCorrection = new bool[4];
        readonly Dictionary<Transform, RotationRecord> _modifiedRotations = new Dictionary<Transform, RotationRecord>();
        readonly Dictionary<Transform, Quaternion> _resolvedHandRotations = new Dictionary<Transform, Quaternion>();
        readonly List<Transform[]> _digitChains = new List<Transform[]>();

        readonly Dictionary<Transform, ToeReferenceFrame> _toeReferenceFrames = new Dictionary<Transform, ToeReferenceFrame>();

        Animator _animator;
        HumanoidIKHumanPoseSolver _humanPoseSolver;
        bool _managedByTimeline;
        Avatar _toeReferenceFrameAvatar;
        int _timelineOwnerCount;
#if UNITY_EDITOR
        bool _editorApplyQueued;
#endif

        public static HumanoidIKLateUpdateDriver GetOrCreate(Animator animator)
        {
            if (!animator) return null;

            var driver = animator.GetComponent<HumanoidIKLateUpdateDriver>();
            if (!driver)
            {
                driver = animator.gameObject.AddComponent<HumanoidIKLateUpdateDriver>();
                driver.hideFlags = HideFlags.HideInInspector;
                driver._managedByTimeline = true;
            }

            driver._animator = animator;
            driver._timelineOwnerCount++;
            driver.enabled = true;
            return driver;
        }

        internal int TimelineOwnerCount => _timelineOwnerCount;

        internal void SetState(HumanoidIKTarget target, HumanoidIKGoalState state)
        {
            _states[(int)target] = state;
            enabled = true;
        }

        internal void ClearState(HumanoidIKTarget target)
        {
            _states[(int)target] = default;
            if (!HasActiveState()) enabled = false;
        }

        internal void ReleaseTimelineOwner()
        {
            if (_timelineOwnerCount > 0) _timelineOwnerCount--;
            TryDisposeIfReleased();
        }

#if UNITY_EDITOR
        internal void ScheduleEditorApplyCurrentStates()
        {
            if (Application.isPlaying || _editorApplyQueued) return;

            _editorApplyQueued = true;
            UnityEditor.EditorApplication.delayCall += ApplyCurrentStatesFromEditorDelay;
            UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
        }
#endif

        void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        void OnEnable()
        {
            if (!_animator) _animator = GetComponent<Animator>();
        }

        void OnDisable()
        {
#if UNITY_EDITOR
            CancelEditorApply();
#endif
            RestoreModifiedBones();
            if (_timelineOwnerCount <= 0) DisposeHumanPoseSolver();
        }

        void OnDestroy()
        {
#if UNITY_EDITOR
            CancelEditorApply();
#endif
            RestoreModifiedBones();
            DisposeHumanPoseSolver();
        }

        void LateUpdate()
        {
            ApplyCurrentStatesInternal();
        }

        void ApplyCurrentStatesInternal()
        {
            RestoreModifiedBones();

            if (!HumanoidIKUtility.IsUsableHumanoid(_animator)) return;

            for (var i = 0; i < _states.Length; i++)
            {
                var state = _states[i];
                _hasEvaluatedState[i] = state.Active &&
                                        TryEvaluateState((HumanoidIKTarget)i, in state, out _evaluatedStates[i]);
            }

            var appliedHumanoidHandPose = ApplyHumanoidHandPose();
            for (var i = 0; i < _evaluatedStates.Length; i++)
            {
                if (!_hasEvaluatedState[i]) continue;
                ApplyState((HumanoidIKTarget)i, in _evaluatedStates[i], appliedHumanoidHandPose);
            }
        }

        void ApplyState(
            HumanoidIKTarget target,
            in HumanoidIKEvaluatedState evaluatedState,
            bool appliedHumanoidHandPose)
        {
            if (!HumanoidIKUtility.TryGetLimbBones(_animator, target, out var limb)) return;

            if (evaluatedState.PositionWeight > 0f)
            {
                ApplyTwoBoneIK(
                    limb,
                    evaluatedState.Position,
                    evaluatedState.BendDirection,
                    evaluatedState.PositionWeight,
                    evaluatedState.BendWeight);
            }

            if (evaluatedState.RotationWeight > 0f)
            {
                var targetBoneRotation = evaluatedState.RotationIsBoneSpace
                    ? evaluatedState.Rotation
                    : HumanoidIKUtility.ToBoneRotation(
                        evaluatedState.Rotation,
                        evaluatedState.BoneToEffectorRotation);
                var targetWorldRotation = Quaternion.Slerp(
                    limb.End.rotation,
                    targetBoneRotation,
                    evaluatedState.RotationWeight);
                SetWorldRotation(limb.End, targetWorldRotation);
            }

            if (evaluatedState.DigitWeight > 0f)
            {
                if (!HumanoidIKUtility.IsHand(target) || !appliedHumanoidHandPose)
                {
                    ApplyDigitBends(
                        target,
                        in evaluatedState.DigitBends,
                        evaluatedState.DigitWeight,
                        evaluatedState.ToeBaseBend,
                        evaluatedState.ToeFan,
                        evaluatedState.ToeBaseBendRange,
                        evaluatedState.ToeBendRanges);
                }
            }
        }

        bool ApplyHumanoidHandPose()
        {
            var leftIndex = (int)HumanoidIKTarget.LeftHand;
            var rightIndex = (int)HumanoidIKTarget.RightHand;
            var resolveLeft = _hasEvaluatedState[leftIndex] &&
                              _evaluatedStates[leftIndex].DigitWeight > Mathf.Epsilon;
            var resolveRight = _hasEvaluatedState[rightIndex] &&
                               _evaluatedStates[rightIndex].DigitWeight > Mathf.Epsilon;
            if (!resolveLeft && !resolveRight) return false;

            if (!EnsureHumanPoseSolver()) return false;

            if (!_humanPoseSolver.TryResolveHandLocalRotations(
                    resolveLeft,
                    in _evaluatedStates[leftIndex].DigitBends,
                    resolveLeft ? _evaluatedStates[leftIndex].DigitWeight : 0f,
                    resolveRight,
                    in _evaluatedStates[rightIndex].DigitBends,
                    resolveRight ? _evaluatedStates[rightIndex].DigitWeight : 0f,
                    _resolvedHandRotations))
            {
                return false;
            }

            foreach (var pair in _resolvedHandRotations)
            {
                SetLocalRotation(pair.Key, pair.Value);
            }

            return true;
        }

        bool EnsureHumanPoseSolver()
        {
            if (_humanPoseSolver != null && _humanPoseSolver.IsValidFor(_animator)) return true;

            DisposeHumanPoseSolver();
            return HumanoidIKHumanPoseSolver.TryCreate(_animator, out _humanPoseSolver);
        }

        bool TryGetBoneToEffectorRotation(
            HumanoidIKTarget target,
            out Quaternion boneToEffectorRotation)
        {
            if (EnsureHumanPoseSolver() &&
                _humanPoseSolver.TryGetBoneToEffectorRotation(target, out boneToEffectorRotation))
            {
                return true;
            }

            boneToEffectorRotation = Quaternion.identity;
            return false;
        }

        bool TryGetLegacyFootBoneToEffectorRotation(
            HumanoidIKTarget target,
            out Quaternion boneToEffectorRotation)
        {
            if (EnsureHumanPoseSolver() &&
                _humanPoseSolver.TryGetLegacyFootBoneToEffectorRotation(
                    target,
                    out boneToEffectorRotation))
            {
                return true;
            }

            boneToEffectorRotation = Quaternion.identity;
            return false;
        }

        void DisposeHumanPoseSolver()
        {
            _humanPoseSolver?.Dispose();
            _humanPoseSolver = null;
            _resolvedHandRotations.Clear();
        }

        bool TryEvaluateState(
            HumanoidIKTarget target,
            in HumanoidIKGoalState state,
            out HumanoidIKEvaluatedState evaluatedState)
        {
            evaluatedState = default;
            if (state.Samples == null || state.SampleCount <= 0) return false;
            if (!HumanoidIKUtility.TryGetLimbBones(_animator, target, out var limb)) return false;

            var position = Vector3.zero;
            var bendDirection = Vector3.zero;
            var rotation = new HumanoidIKQuaternionAccumulator();
            var digitBends = new HumanoidIKDigitBendPose();
            var toeFan = 0f;
            var toeBaseBend = 0f;

            var positionWeight = 0f;
            var rotationWeight = 0f;
            var bendWeight = 0f;
            var digitWeight = 0f;
            var correctionChecked = false;
            var hasBoneToEffectorRotation = false;
            var boneToEffectorRotation = Quaternion.identity;
            var hasLegacyFootBoneToEffectorRotation = false;
            var legacyFootBoneToEffectorRotation = Quaternion.identity;
            var rotationIsBoneSpace = false;

            for (var i = 0; i < state.SampleCount && i < state.Samples.Length; i++)
            {
                var sample = state.Samples[i];
                var timelineWeight = sample.TimelineWeight;
                if (timelineWeight <= 0f) continue;

                HumanoidIKUtility.ResolveWorldPose(
                    sample.Anchor,
                    sample.Position,
                    sample.Rotation,
                    sample.BendTarget,
                    out var worldPosition,
                    out var worldRotation,
                    out _);
                var worldBendDirection = HumanoidIKUtility.ResolveBendDirection(
                    sample.Anchor,
                    sample.BendTarget,
                    sample.BendSpace,
                    limb.Upper.position);

                var weightedPosition = timelineWeight * sample.PositionWeight;
                var weightedRotation = timelineWeight * sample.RotationWeight;
                var weightedBend = timelineWeight * sample.BendWeight;
                var weightedDigit = timelineWeight * sample.DigitWeight;

                position += worldPosition * weightedPosition;
                bendDirection += worldBendDirection * weightedBend;
                if (weightedRotation > 0f)
                {
                    if (!correctionChecked)
                    {
                        hasBoneToEffectorRotation = TryGetBoneToEffectorRotation(
                            target,
                            out boneToEffectorRotation);
                        correctionChecked = true;
                        if (hasBoneToEffectorRotation)
                        {
                            _reportedMissingEffectorCorrection[(int)target] = false;
                            if (HumanoidIKUtility.IsFoot(target))
                            {
                                hasLegacyFootBoneToEffectorRotation =
                                    TryGetLegacyFootBoneToEffectorRotation(
                                        target,
                                        out legacyFootBoneToEffectorRotation);
                            }
                        }
                    }

                    if (hasBoneToEffectorRotation)
                    {
                        var effectorRotation = HumanoidIKUtility.IsFoot(target) &&
                                               hasLegacyFootBoneToEffectorRotation
                            ? HumanoidIKUtility.ToProjectedSoleRotation(
                                worldRotation,
                                sample.RotationSpace,
                                sample.FootRotationFrameVersion,
                                boneToEffectorRotation,
                                legacyFootBoneToEffectorRotation)
                            : HumanoidIKUtility.ToEffectorRotation(
                                worldRotation,
                                sample.RotationSpace,
                                boneToEffectorRotation);
                        rotation.Add(
                            effectorRotation,
                            weightedRotation);
                    }
                    else if (sample.RotationSpace == HumanoidIKRotationSpace.LegacyBoneRotation)
                    {
                        rotation.Add(worldRotation, weightedRotation);
                        rotationIsBoneSpace = true;
                    }
                    else
                    {
                        ReportMissingEffectorCorrection(target);
                        weightedRotation = 0f;
                    }
                }
                digitBends += sample.DigitBends * weightedDigit;
                toeBaseBend += sample.ToeBaseBend * weightedDigit;
                toeFan += sample.ToeFan * weightedDigit;

                positionWeight += weightedPosition;
                rotationWeight += weightedRotation;
                bendWeight += weightedBend;
                digitWeight += weightedDigit;
            }

            if (positionWeight <= Mathf.Epsilon &&
                rotationWeight <= Mathf.Epsilon &&
                bendWeight <= Mathf.Epsilon &&
                digitWeight <= Mathf.Epsilon)
            {
                return false;
            }

            Vector2[] toeBendRanges = null;
            Vector2 toeBaseBendRange = default;
            for (var i = 0; i < state.SampleCount && i < state.Samples.Length; i++)
            {
                if (state.Samples[i].TimelineWeight > 0f)
                {
                    if (state.Samples[i].ToeBendRanges != null) toeBendRanges = state.Samples[i].ToeBendRanges;
                    if (state.Samples[i].ToeBaseBendRange != default) toeBaseBendRange = state.Samples[i].ToeBaseBendRange;
                }
            }

            evaluatedState = new HumanoidIKEvaluatedState
            {
                Position = positionWeight > Mathf.Epsilon ? position / positionWeight : Vector3.zero,
                Rotation = rotationWeight > Mathf.Epsilon ? rotation.GetValue() : Quaternion.identity,
                BoneToEffectorRotation = boneToEffectorRotation,
                RotationIsBoneSpace = rotationIsBoneSpace,
                BendDirection = bendWeight > Mathf.Epsilon && bendDirection.sqrMagnitude > 0.000001f
                    ? bendDirection.normalized
                    : Vector3.zero,
                PositionWeight = Mathf.Clamp01(positionWeight),
                RotationWeight = Mathf.Clamp01(rotationWeight),
                BendWeight = Mathf.Clamp01(bendWeight),
                DigitWeight = Mathf.Clamp01(digitWeight),
                DigitBends = digitWeight > Mathf.Epsilon ? digitBends * (1f / digitWeight) : default,
                ToeBaseBend = digitWeight > Mathf.Epsilon ? toeBaseBend / digitWeight : 0f,
                ToeFan = digitWeight > Mathf.Epsilon ? toeFan / digitWeight : 0f,
                ToeBendRanges = toeBendRanges,
                ToeBaseBendRange = toeBaseBendRange
            };

            return true;
        }

        void ReportMissingEffectorCorrection(HumanoidIKTarget target)
        {
            var index = (int)target;
            if (_reportedMissingEffectorCorrection[index]) return;

            _reportedMissingEffectorCorrection[index] = true;
            Debug.LogWarning(
                $"Humanoid IK cannot apply {target} effector rotation on '{_animator.name}' because the Avatar reference frame could not be resolved. " +
                "The position, pole, and digit channels remain active, but rotation is skipped.",
                _animator);
        }

        void ApplyTwoBoneIK(
            HumanoidIKLimbBones limb,
            Vector3 targetPosition,
            Vector3 bendDirection,
            float positionWeight,
            float bendWeight)
        {
            var upper = limb.Upper;
            var lower = limb.Lower;
            var end = limb.End;

            var upperLocal = upper.localRotation;
            var lowerLocal = lower.localRotation;

            var rootPosition = upper.position;
            var midPosition = lower.position;
            var endPosition = end.position;

            var upperLength = Vector3.Distance(rootPosition, midPosition);
            var lowerLength = Vector3.Distance(midPosition, endPosition);
            if (upperLength <= Mathf.Epsilon || lowerLength <= Mathf.Epsilon) return;

            var targetOffset = targetPosition - rootPosition;
            var targetDistance = targetOffset.magnitude;
            if (targetDistance <= Mathf.Epsilon) return;

            var minDistance = Mathf.Abs(upperLength - lowerLength) + 0.0001f;
            var maxDistance = upperLength + lowerLength - 0.0001f;
            targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);

            var targetDirection = targetOffset.normalized;
            var solvedBendDirection = GetBendDirection(
                rootPosition,
                midPosition,
                targetDirection,
                bendDirection,
                bendWeight);

            var adjacent = (upperLength * upperLength + targetDistance * targetDistance - lowerLength * lowerLength) /
                           (2f * targetDistance);
            var heightSquared = Mathf.Max(0f, upperLength * upperLength - adjacent * adjacent);
            var height = Mathf.Sqrt(heightSquared);
            var solvedMidPosition = rootPosition + targetDirection * adjacent + solvedBendDirection * height;
            var solvedTargetPosition = rootPosition + targetDirection * targetDistance;

            upper.rotation = Quaternion.FromToRotation(midPosition - rootPosition, solvedMidPosition - rootPosition) * upper.rotation;
            lower.rotation = Quaternion.FromToRotation(end.position - lower.position, solvedTargetPosition - lower.position) * lower.rotation;

            var solvedUpperLocal = upper.localRotation;
            var solvedLowerLocal = lower.localRotation;

            upper.localRotation = upperLocal;
            lower.localRotation = lowerLocal;

            SetLocalRotation(upper, Quaternion.Slerp(upperLocal, solvedUpperLocal, positionWeight));
            SetLocalRotation(lower, Quaternion.Slerp(lowerLocal, solvedLowerLocal, positionWeight));
        }

        Vector3 GetBendDirection(
            Vector3 rootPosition,
            Vector3 midPosition,
            Vector3 targetDirection,
            Vector3 bendDirection,
            float bendWeight)
        {
            var currentBendDirection = Vector3.ProjectOnPlane(midPosition - rootPosition, targetDirection);
            if (currentBendDirection.sqrMagnitude <= 0.000001f)
            {
                currentBendDirection = Vector3.Cross(targetDirection, transform.up);
            }

            if (currentBendDirection.sqrMagnitude <= 0.000001f)
            {
                currentBendDirection = Vector3.Cross(targetDirection, transform.right);
            }

            currentBendDirection.Normalize();

            if (bendWeight <= 0f) return currentBendDirection;

            var targetBendDirection = Vector3.ProjectOnPlane(bendDirection, targetDirection);
            if (targetBendDirection.sqrMagnitude <= 0.000001f) return currentBendDirection;

            targetBendDirection.Normalize();
            return Vector3.Slerp(currentBendDirection, targetBendDirection, Mathf.Clamp01(bendWeight)).normalized;
        }

        void ApplyDigitBends(
            HumanoidIKTarget target,
            in HumanoidIKDigitBendPose pose,
            float weight,
            float toeBaseBend,
            float toeFan,
            Vector2 toeBaseBendRange = default,
            Vector2[] toeBendRanges = null)
        {
            HumanoidIKUtility.GetDigitChains(_animator, target, _digitChains);
            var toeRigKind = HumanoidIKUtility.GetToeRigKind(_animator, target);
            var isFoot = HumanoidIKUtility.IsFoot(target);

            if (isFoot)
            {
                var toeRoot = HumanoidIKUtility.GetToeRoot(_animator, target);
                if (toeRoot)
                {
                    var baseAngle = toeBaseBendRange != default
                        ? HumanoidIKUtility.GetToeBaseBendAngle(toeBaseBend, toeBaseBendRange)
                        : HumanoidIKUtility.GetToeBaseBendAngle(toeBaseBend);
                    ApplyReferenceToeBaseBend(
                        target,
                        toeRoot,
                        baseAngle,
                        weight);
                }
            }

            for (var i = 0; i < _digitChains.Count && i < 5; i++)
            {
                var bend = HumanoidIKUtility.GetDigitBend(in pose, i);
                if (toeRigKind == HumanoidIKToeRigKind.ArticulatedToes)
                {
                    bend += HumanoidIKUtility.GetArticulatedToeFanOffset(i, toeFan);
                }

                if (isFoot)
                {
                    bend = toeRigKind == HumanoidIKToeRigKind.ToeFoot
                        ? HumanoidIKUtility.ClampToeFootBend(bend, toeBendRanges)
                        : HumanoidIKUtility.ClampToeBend(bend, toeBendRanges);
                }

                if (isFoot)
                {
                    ApplyReferenceToeJointBend(target, _digitChains[i], bend, weight);
                }
                else
                {
                    ApplyJointBend(_digitChains[i], bend, weight);
                }
            }
        }

        void ApplyReferenceToeBaseBend(
            HumanoidIKTarget target,
            Transform toeRoot,
            float bendAngle,
            float weight)
        {
            if (!toeRoot ||
                !TryGetReferenceToeFrame(
                    target,
                    null,
                    -1,
                    toeRoot,
                    out var forwardInBone,
                    out var upInBone))
            {
                return;
            }

            ApplyReferenceToeRotation(
                target,
                toeRoot,
                new Vector3(bendAngle, 0f, 0f),
                forwardInBone,
                upInBone,
                weight);
        }

        void ApplyReferenceToeJointBend(
            HumanoidIKTarget target,
            Transform[] chain,
            HumanoidIKJointBend bend,
            float weight)
        {
            if (chain == null) return;

            ApplyReferenceToeJointRotation(target, chain, 0, bend.proximal, weight);
            ApplyReferenceToeJointRotation(target, chain, 1, bend.intermediate, weight);
            ApplyReferenceToeJointRotation(target, chain, 2, bend.distal, weight);
        }

        void ApplyReferenceToeJointRotation(
            HumanoidIKTarget target,
            Transform[] chain,
            int jointIndex,
            Vector3 authoredEuler,
            float weight)
        {
            var bone = jointIndex >= 0 && jointIndex < chain.Length
                ? chain[jointIndex]
                : null;
            if (!bone ||
                !TryGetReferenceToeFrame(
                    target,
                    chain,
                    jointIndex,
                    bone,
                    out var forwardInBone,
                    out var upInBone))
            {
                return;
            }

            ApplyReferenceToeRotation(
                target,
                bone,
                authoredEuler,
                forwardInBone,
                upInBone,
                weight);
        }

        bool TryGetReferenceToeFrame(
            HumanoidIKTarget target,
            Transform[] chain,
            int jointIndex,
            Transform bone,
            out Vector3 forwardInBone,
            out Vector3 upInBone)
        {
            forwardInBone = Vector3.forward;
            upInBone = Vector3.up;

            Transform nextBone = null;
            Transform previousBone = null;
            if (chain != null && jointIndex >= 0)
            {
                for (var i = jointIndex + 1; i < chain.Length; i++)
                {
                    if (!chain[i]) continue;
                    nextBone = chain[i];
                    break;
                }

                for (var i = jointIndex - 1; i >= 0; i--)
                {
                    if (!chain[i]) continue;
                    previousBone = chain[i];
                    break;
                }
            }

            var avatar = _animator ? _animator.avatar : null;
            if (_toeReferenceFrameAvatar != avatar)
            {
                _toeReferenceFrameAvatar = avatar;
                _toeReferenceFrames.Clear();
            }

            if (bone &&
                _toeReferenceFrames.TryGetValue(bone, out var cachedFrame) &&
                cachedFrame.NextBone == nextBone &&
                cachedFrame.PreviousBone == previousBone)
            {
                forwardInBone = cachedFrame.ForwardInBone;
                upInBone = cachedFrame.UpInBone;
                return true;
            }

            if (!bone ||
                !EnsureHumanPoseSolver() ||
                !_humanPoseSolver.TryGetReferenceFootDisplayRotation(
                    target,
                    out var displayRotation) ||
                _humanPoseSolver.ReferencePose == null ||
                !_humanPoseSolver.ReferencePose.TryGetRelativeMatrix(
                    _animator.transform,
                    bone,
                    out var boneMatrix))
            {
                return false;
            }

            var bonePosition = boneMatrix.MultiplyPoint3x4(Vector3.zero);
            var forwardInRoot = Vector3.zero;
            if (nextBone &&
                _humanPoseSolver.ReferencePose.TryGetRelativeMatrix(
                    _animator.transform,
                    nextBone,
                    out var nextMatrix))
            {
                forwardInRoot =
                    nextMatrix.MultiplyPoint3x4(Vector3.zero) - bonePosition;
            }
            else if (previousBone &&
                     _humanPoseSolver.ReferencePose.TryGetRelativeMatrix(
                         _animator.transform,
                         previousBone,
                         out var previousMatrix))
            {
                forwardInRoot =
                    bonePosition - previousMatrix.MultiplyPoint3x4(Vector3.zero);
            }

            if (forwardInRoot.sqrMagnitude <= 0.000001f)
            {
                forwardInRoot = displayRotation * Vector3.forward;
            }

            var rootToBoneRotation = Quaternion.Inverse(boneMatrix.rotation);
            forwardInBone = rootToBoneRotation * forwardInRoot;
            upInBone = rootToBoneRotation * (displayRotation * Vector3.up);
            var isValid =
                forwardInBone.sqrMagnitude > 0.000001f &&
                Vector3.ProjectOnPlane(upInBone, forwardInBone).sqrMagnitude >
                0.000001f;
            if (isValid)
            {
                _toeReferenceFrames[bone] = new ToeReferenceFrame
                {
                    NextBone = nextBone,
                    PreviousBone = previousBone,
                    ForwardInBone = forwardInBone,
                    UpInBone = upInBone
                };
            }

            return isValid;
        }

        void ApplyReferenceToeRotation(
            HumanoidIKTarget target,
            Transform bone,
            Vector3 authoredEuler,
            Vector3 forwardInBone,
            Vector3 upInBone,
            float weight)
        {
            if (!bone ||
                weight <= 0f ||
                !EnsureHumanPoseSolver() ||
                !_humanPoseSolver.TryGetReferenceLocalRotation(
                    bone,
                    out var referenceLocalRotation) ||
                !HumanoidIKUtility.TryGetToeAnatomicalRotation(
                    authoredEuler,
                    forwardInBone,
                    upInBone,
                    target == HumanoidIKTarget.LeftFoot,
                    out var anatomicalOffset))
            {
                return;
            }

            var targetLocalRotation = referenceLocalRotation * anatomicalOffset;
            SetLocalRotation(
                bone,
                Quaternion.Slerp(bone.localRotation, targetLocalRotation, Mathf.Clamp01(weight)));
        }

        void ApplyJointBend(Transform[] chain, HumanoidIKJointBend bend, float weight)
        {
            if (chain == null) return;

            ApplyLocalOffset(chain.Length > 0 ? chain[0] : null, bend.proximal, weight);
            ApplyLocalOffset(chain.Length > 1 ? chain[1] : null, bend.intermediate, weight);
            ApplyLocalOffset(chain.Length > 2 ? chain[2] : null, bend.distal, weight);
        }

        void ApplyLocalOffset(Transform bone, Vector3 eulerOffset, float weight)
        {
            if (!bone || eulerOffset == Vector3.zero || weight <= 0f) return;
            SetLocalRotation(bone, bone.localRotation * Quaternion.Euler(eulerOffset * weight));
        }


        void SetWorldRotation(Transform bone, Quaternion worldRotation)
        {
            if (!bone) return;

            var parent = bone.parent;
            var localRotation = parent
                ? Quaternion.Inverse(parent.rotation) * worldRotation
                : worldRotation;

            SetLocalRotation(bone, localRotation);
        }

        void SetLocalRotation(Transform bone, Quaternion localRotation)
        {
            if (!bone) return;

            if (!_modifiedRotations.TryGetValue(bone, out var record))
            {
                record = new RotationRecord
                {
                    BaseLocalRotation = bone.localRotation
                };
            }

            bone.localRotation = localRotation;
            record.AppliedLocalRotation = localRotation;
            _modifiedRotations[bone] = record;
        }

#if UNITY_EDITOR
        void ApplyCurrentStatesFromEditorDelay()
        {
            _editorApplyQueued = false;
            if (!this || !isActiveAndEnabled) return;

            ApplyCurrentStatesInternal();
            UnityEditor.SceneView.RepaintAll();
        }

        void CancelEditorApply()
        {
            if (!_editorApplyQueued) return;

            UnityEditor.EditorApplication.delayCall -= ApplyCurrentStatesFromEditorDelay;
            _editorApplyQueued = false;
        }
#endif

        void RestoreModifiedBones()
        {
            if (_modifiedRotations.Count == 0) return;

            foreach (var kv in _modifiedRotations)
            {
                var bone = kv.Key;
                if (!bone) continue;

                var record = kv.Value;
                bone.localRotation = record.BaseLocalRotation;
            }

            _modifiedRotations.Clear();
        }

        bool HasActiveState()
        {

            for (var i = 0; i < _states.Length; i++)
            {
                if (_states[i].Active) return true;
            }

            return false;
        }

        void TryDisposeIfReleased()
        {
            if (!_managedByTimeline || _timelineOwnerCount > 0 || HasActiveState()) return;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                DestroyImmediate(this);
                return;
            }
#endif
            Destroy(this);
        }
    }
}
