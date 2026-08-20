using System;
using System.Collections.Generic;
using UnityEngine;

namespace CutsceneEngine
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(9000)]
    public sealed class LookAtLateUpdateDriver : MonoBehaviour
    {
        struct RotationRecord
        {
            public Quaternion BaseLocalRotation;
            public Quaternion AppliedLocalRotation;
        }

        readonly struct BlendShapeBinding : IEquatable<BlendShapeBinding>
        {
            public readonly SkinnedMeshRenderer Renderer;
            public readonly Mesh Mesh;
            public readonly int Index;

#if UNITY_6000_4_OR_NEWER
            readonly EntityId _rendererInstanceId;
            readonly EntityId _meshInstanceId;
#else
            readonly int _rendererInstanceId;
            readonly int _meshInstanceId;
#endif

            public BlendShapeBinding(
                SkinnedMeshRenderer renderer,
                Mesh mesh,
                int index)
            {
                Renderer = renderer;
                Mesh = mesh;
                Index = index;
#if UNITY_6000_4_OR_NEWER
                _rendererInstanceId = renderer.GetEntityId();
                _meshInstanceId = mesh.GetEntityId();
#else
                _rendererInstanceId = renderer.GetInstanceID();
                _meshInstanceId = mesh.GetInstanceID();
#endif
            }

            public bool IsValid =>
                Renderer &&
                Mesh &&
                Renderer.sharedMesh == Mesh &&
                Index >= 0 &&
                Index < Mesh.blendShapeCount;

            public bool Equals(BlendShapeBinding other)
            {
                return _rendererInstanceId == other._rendererInstanceId &&
                       _meshInstanceId == other._meshInstanceId &&
                       Index == other.Index;
            }

            public override bool Equals(object obj)
            {
                return obj is BlendShapeBinding other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
#if UNITY_6000_4_OR_NEWER
                    var hash = _rendererInstanceId.GetHashCode();
                    hash = (hash * 397) ^ _meshInstanceId.GetHashCode();
                    return (hash * 397) ^ Index;
#else
                    var hash = _rendererInstanceId;
                    hash = (hash * 397) ^ _meshInstanceId;
                    return (hash * 397) ^ Index;
#endif
                }
            }
        }

        struct BlendShapeRecord
        {
            public float BaseWeight;
            public float AppliedWeight;
        }

        struct BlendShapeAccumulator
        {
            public float WeightedTargetSum;
            public float TimelineWeight;

            public void Add(float targetWeight, float timelineWeight)
            {
                WeightedTargetSum += targetWeight * timelineWeight;
                TimelineWeight += timelineWeight;
            }
        }

        readonly Dictionary<Transform, RotationRecord> _modifiedRotations =
            new Dictionary<Transform, RotationRecord>();
        readonly Dictionary<BlendShapeBinding, BlendShapeRecord> _modifiedBlendShapes =
            new Dictionary<BlendShapeBinding, BlendShapeRecord>();
        readonly Dictionary<BlendShapeBinding, BlendShapeAccumulator>
            _blendShapeAccumulators =
                new Dictionary<BlendShapeBinding, BlendShapeAccumulator>();
        readonly Dictionary<BlendShapeBinding, float>
            _sampleBlendShapeTargets =
                new Dictionary<BlendShapeBinding, float>();
        readonly HashSet<BlendShapeBinding> _sampleBlendShapeBindings =
            new HashSet<BlendShapeBinding>();
        readonly HashSet<string> _resolvedBlendShapeKeys = new HashSet<string>(StringComparer.Ordinal);


        Dictionary<string, List<BlendShapeBinding>> _blendShapeLookup;
        SkinnedMeshRenderer[] _blendShapeRenderers = System.Array.Empty<SkinnedMeshRenderer>();

        Animator _animator;
        LookAtState _state;
        LookAtRig _rig;
        bool _managedByTimeline;
        int _timelineOwnerCount;
        bool _reportedMissingReferenceFrame;
#if UNITY_EDITOR
        Vector3[] _editorResolvedTargetPositions = System.Array.Empty<Vector3>();
        int _editorResolvedTargetCount;
        bool _editorApplyPending;
        bool _editorApplyQueued;
#endif

        public static LookAtLateUpdateDriver GetOrCreate(Animator animator)
        {
            if (!animator) return null;

            var driver = animator.GetComponent<LookAtLateUpdateDriver>();
            if (!driver)
            {
                driver = animator.gameObject.AddComponent<LookAtLateUpdateDriver>();
                driver.hideFlags = HideFlags.HideInInspector;
                driver._managedByTimeline = true;
            }

            driver._animator = animator;
            driver._timelineOwnerCount++;
            driver.enabled = true;
            return driver;
        }

        internal int TimelineOwnerCount => _timelineOwnerCount;

        internal void SetState(LookAtState state)
        {
            _state = state;
            enabled = true;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                CaptureEditorTargetPositions();
                _editorApplyPending = true;
            }
#endif
        }

        internal void ClearState()
        {
            _state = default;
#if UNITY_EDITOR
            _editorApplyPending = false;
#endif
            enabled = false;
        }

        internal void InvalidateRig()
        {
            RestoreModifiedBones();
            _rig = null;
            _reportedMissingReferenceFrame = false;
#if UNITY_EDITOR
            RequestEditorApply();
#endif
        }

        internal void ReleaseTimelineOwner()
        {
            if (_timelineOwnerCount > 0) _timelineOwnerCount--;
            TryDisposeIfReleased();
        }

#if UNITY_EDITOR
        internal bool EditorApplyPending => _editorApplyPending;

        internal bool HasActiveEditorStateFor(Transform directorTransform)
        {
            if (Application.isPlaying || !_state.Active || _state.Samples == null) return false;

            var sampleCount = Mathf.Min(_state.SampleCount, _state.Samples.Length);
            for (var i = 0; i < sampleCount; i++)
            {
                var sample = _state.Samples[i];
                if (sample.DirectorTransform == directorTransform &&
                    sample.TimelineWeight > 0f &&
                    sample.HasAnyEffect())
                {
                    return true;
                }
            }

            return false;
        }

        internal void RequestEditorApply()
        {
            if (!Application.isPlaying) _editorApplyPending = true;
        }

        internal bool RefreshEditorInputs()
        {
            if (Application.isPlaying || !_state.Active) return false;
            if (!RefreshEditorTargetPositions()) return false;

            _editorApplyPending = true;
            return true;
        }

        internal bool ApplyPendingEditorState()
        {
            if (Application.isPlaying || !_editorApplyPending) return false;

            _editorApplyPending = false;
            ApplyCurrentStateInternal();
            return _modifiedRotations.Count > 0 ||
                   _modifiedBlendShapes.Count > 0;
        }

        internal bool RestoreEditorPose()
        {
            if (Application.isPlaying) return false;

            _editorApplyPending = false;
            var hadModifiedState = _modifiedRotations.Count > 0 ||
                                   _modifiedBlendShapes.Count > 0;
            RestoreModifiedBones();
            RestoreModifiedBlendShapes(force: true);
            return hadModifiedState;
        }
        internal void ScheduleEditorApplyCurrentStates()
        {
            if (Application.isPlaying || _editorApplyQueued) return;

            _editorApplyQueued = true;
            UnityEditor.EditorApplication.delayCall += ApplyCurrentStatesFromEditorDelay;
            UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
        }

        void ApplyCurrentStatesFromEditorDelay()
        {
            _editorApplyQueued = false;
            if (!this || !isActiveAndEnabled) return;

            ApplyCurrentStateInternal();
            UnityEditor.SceneView.RepaintAll();
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
            _editorApplyPending = false;
#endif
            RestoreModifiedBones();
            RestoreModifiedBlendShapes(force: true);
            if (_timelineOwnerCount <= 0)
            {
                ResetBlendShapeCache();
                _rig = null;
            }
        }

        void OnDestroy()
        {
#if UNITY_EDITOR
            _editorApplyPending = false;
#endif
            RestoreModifiedBones();
            RestoreModifiedBlendShapes(force: true);
            ResetBlendShapeCache();
            _rig = null;
        }

        void LateUpdate()
        {
            ApplyCurrentStateInternal();
        }

        void ApplyCurrentStateInternal()
        {
            RestoreModifiedBones();
            RestoreModifiedBlendShapes(force: false);

            if (!_animator) return;

            var hasRotationState = LookAtUtility.TryEvaluateState(
                in _state,
                out var evaluatedState);
            var needsEyeDirection =
                HasEyelidFollowConfiguration(in _state);
            var rigReady = false;
            if (hasRotationState || needsEyeDirection)
            {
                rigReady = EnsureRig();
            }

            if (rigReady && hasRotationState)
            {
                ApplyBody(in evaluatedState.Body);
                ApplyBone(in _rig.Neck, in evaluatedState.Neck);
                ApplyBone(in _rig.Head, in evaluatedState.Head);
                ApplyBone(in _rig.LeftEye, in evaluatedState.Eyes);
                ApplyBone(in _rig.RightEye, in evaluatedState.Eyes);
            }

            var eyeDirections = default(LookAtEyeDirectionState);
            if (rigReady && needsEyeDirection)
            {
                TryGetFinalEyeDirections(out eyeDirections);
            }

            ApplyEyelids(in _state, in eyeDirections);
        }

        bool EnsureRig()
        {
            if (_rig != null &&
                _rig.IsValidFor(_animator, _state.SourceTrack))
            {
                return true;
            }

            _rig = null;
            if (LookAtRig.TryCreate(
                    _animator,
                    _state.SourceTrack,
                    out _rig))
            {
                _reportedMissingReferenceFrame = false;
                return true;
            }

            if (!_reportedMissingReferenceFrame)
            {
                _reportedMissingReferenceFrame = true;
                Debug.LogWarning(
                    $"Look At cannot resolve a Head and stable reference frame for '{_animator.name}'. " +
                    "Assign the Generic Head on the track or verify the Humanoid Avatar.",
                    _animator);
            }

            return false;
        }

        void ApplyBody(in LookAtChannelState channel)
        {
            var body = _rig.Body;
            if (body == null || body.Length == 0 || channel.Weight <= 0f) return;

            var firstBone = body[0].Bone;
            if (!firstBone) return;

            var targetDirection = channel.TargetPosition - firstBone.position;
            if (targetDirection.sqrMagnitude <= 0.000001f) return;

            var currentForward = firstBone.TransformDirection(body[0].ForwardInBone);
            if (currentForward.sqrMagnitude <= 0.000001f) return;

            var clampedDirection = LookAtUtility.ClampTargetDirection(
                currentForward,
                targetDirection,
                _animator.transform.rotation,
                channel.AngleLimits);
            for (var i = 0; i < body.Length; i++)
            {
                var boneWeight = LookAtUtility.GetGradualBoneWeight(channel.Weight, i, body.Length);
                ApplyBoneTowardDirection(body[i], clampedDirection, boneWeight);
            }
        }

        void ApplyBone(in LookAtBoneFrame frame, in LookAtChannelState channel)
        {
            ApplyBone(
                frame,
                channel.TargetPosition,
                channel.Weight,
                channel.AngleLimits,
                channel.PitchOffsetDegrees);
        }

        void ApplyBone(
            in LookAtBoneFrame frame,
            Vector3 targetPosition,
            float weight,
            LookAtAngleLimits angleLimits,
            float pitchOffsetDegrees)
        {
            var bone = frame.Bone;
            if (!bone || weight <= 0f) return;

            var targetDirection = targetPosition - bone.position;
            if (targetDirection.sqrMagnitude <= 0.000001f) return;

            var currentForward = bone.TransformDirection(frame.ForwardInBone);
            if (currentForward.sqrMagnitude <= 0.000001f) return;

            var clampedDirection = LookAtUtility.ClampTargetDirection(
                currentForward,
                targetDirection,
                _animator.transform.rotation,
                angleLimits,
                pitchOffsetDegrees);
            ApplyBoneTowardDirection(frame, clampedDirection, weight);
        }

        void ApplyBoneTowardDirection(
            in LookAtBoneFrame frame,
            Vector3 targetDirection,
            float weight)
        {
            var bone = frame.Bone;
            if (!bone || weight <= 0f || targetDirection.sqrMagnitude <= 0.000001f) return;

            var currentForward = bone.TransformDirection(frame.ForwardInBone);
            if (currentForward.sqrMagnitude <= 0.000001f) return;

            var fullDelta = Quaternion.FromToRotation(currentForward, targetDirection);
            var weightedDelta = Quaternion.Slerp(
                Quaternion.identity,
                fullDelta,
                Mathf.Clamp01(weight));
            SetWorldRotation(bone, weightedDelta * bone.rotation);
        }

        void SetWorldRotation(Transform bone, Quaternion worldRotation)
        {
            var parent = bone.parent;
            var localRotation = parent
                ? Quaternion.Inverse(parent.rotation) * worldRotation
                : worldRotation;
            SetLocalRotation(bone, localRotation);
        }

        void SetLocalRotation(Transform bone, Quaternion localRotation)
        {
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

        void RestoreModifiedBones()
        {
            if (_modifiedRotations.Count == 0) return;

            foreach (var pair in _modifiedRotations)
            {
                var bone = pair.Key;
                if (!bone) continue;

                var record = pair.Value;
                bone.localRotation = record.BaseLocalRotation;
            }

            _modifiedRotations.Clear();
        }

        void TryDisposeIfReleased()
        {
            if (!_managedByTimeline || _timelineOwnerCount > 0 || _state.Active) return;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                DestroyImmediate(this);
                return;
            }
#endif
            Destroy(this);
        }

#if UNITY_EDITOR
        void CaptureEditorTargetPositions()
        {
            var sampleCount = GetEditorSampleCount();
            EnsureEditorTargetPositionCapacity(sampleCount);
            for (var i = 0; i < sampleCount; i++)
            {
                _editorResolvedTargetPositions[i] = _state.Samples[i].ResolveTargetPosition();
            }

            _editorResolvedTargetCount = sampleCount;
        }

        bool RefreshEditorTargetPositions()
        {
            var sampleCount = GetEditorSampleCount();
            EnsureEditorTargetPositionCapacity(sampleCount);
            var changed = sampleCount != _editorResolvedTargetCount;
            for (var i = 0; i < sampleCount; i++)
            {
                var currentPosition = _state.Samples[i].ResolveTargetPosition();
                if ((currentPosition - _editorResolvedTargetPositions[i]).sqrMagnitude > 0.0000000001f)
                {
                    changed = true;
                }

                _editorResolvedTargetPositions[i] = currentPosition;
            }

            _editorResolvedTargetCount = sampleCount;
            return changed;
        }

        int GetEditorSampleCount()
        {
            return _state.Samples == null
                ? 0
                : Mathf.Min(_state.SampleCount, _state.Samples.Length);
        }

        void EnsureEditorTargetPositionCapacity(int requiredCapacity)
        {
            if (_editorResolvedTargetPositions.Length >= requiredCapacity) return;

            var nextCapacity = Mathf.NextPowerOfTwo(Mathf.Max(requiredCapacity, 1));
            _editorResolvedTargetPositions = new Vector3[nextCapacity];
        }
#endif

        sealed class LookAtRig
        {
            static readonly HumanBodyBones[] BodyBoneIds =
            {
                HumanBodyBones.Spine,
                HumanBodyBones.Chest,
                HumanBodyBones.UpperChest
            };

            readonly Animator _animator;
            readonly Avatar _avatar;
            readonly LookAtTrack _sourceTrack;
            readonly int _genericMappingHash;
            readonly HumanoidIKReferencePose _humanoidReferencePose;
            readonly LookAtGenericReferencePose _genericReferencePose;

            internal readonly LookAtBoneFrame[] Body;
            internal readonly LookAtBoneFrame Neck;
            internal readonly LookAtBoneFrame Head;
            internal readonly LookAtBoneFrame LeftEye;
            internal readonly LookAtBoneFrame RightEye;

            LookAtRig(
                Animator animator,
                LookAtTrack sourceTrack,
                HumanoidIKReferencePose humanoidReferencePose,
                LookAtGenericReferencePose genericReferencePose,
                LookAtBoneFrame[] body,
                LookAtBoneFrame neck,
                LookAtBoneFrame head,
                LookAtBoneFrame leftEye,
                LookAtBoneFrame rightEye)
            {
                _animator = animator;
                _avatar = animator.avatar;
                _sourceTrack = sourceTrack;
                _genericMappingHash =
                    LookAtGenericRigUtility.GetMappingHash(animator);
                _humanoidReferencePose = humanoidReferencePose;
                _genericReferencePose = genericReferencePose;
                Body = body;
                Neck = neck;
                Head = head;
                LeftEye = leftEye;
                RightEye = rightEye;
            }

            internal bool IsValidFor(
                Animator animator,
                LookAtTrack sourceTrack)
            {
                if (!animator ||
                    animator != _animator ||
                    animator.avatar != _avatar)
                {
                    return false;
                }

                if (_humanoidReferencePose != null)
                {
                    return HumanoidIKUtility.IsUsableHumanoid(animator) &&
                           _humanoidReferencePose.IsValidFor(animator);
                }

                return !animator.isHuman &&
                       sourceTrack == _sourceTrack &&
                       sourceTrack &&
                       LookAtGenericRigUtility.GetMappingHash(animator) ==
                       _genericMappingHash &&
                       _genericReferencePose != null &&
                       _genericReferencePose.IsValidFor(animator) &&
                       Head.Bone &&
                       Head.Bone.IsChildOf(animator.transform);
            }

            internal static bool TryCreate(
                Animator animator,
                LookAtTrack sourceTrack,
                out LookAtRig rig)
            {
                rig = null;
                if (!animator) return false;

                if (HumanoidIKUtility.IsUsableHumanoid(animator))
                {
                    return TryCreateHumanoid(animator, out rig);
                }

                return TryCreateGeneric(animator, sourceTrack, out rig);
            }

            static bool TryCreateHumanoid(
                Animator animator,
                out LookAtRig rig)
            {
                rig = null;
                if (!HumanoidIKReferencePose.TryCreate(
                        animator,
                        out var referencePose))
                {
                    return false;
                }

                var bodyFrames = new List<LookAtBoneFrame>(BodyBoneIds.Length);
                for (var i = 0; i < BodyBoneIds.Length; i++)
                {
                    if (TryCreateFrame(animator, referencePose, BodyBoneIds[i], out var frame))
                    {
                        bodyFrames.Add(frame);
                    }
                }

                TryCreateFrame(animator, referencePose, HumanBodyBones.Neck, out var neck);
                if (!TryCreateFrame(animator, referencePose, HumanBodyBones.Head, out var head))
                {
                    return false;
                }
                TryCreateFrame(animator, referencePose, HumanBodyBones.LeftEye, out var leftEye);
                TryCreateFrame(animator, referencePose, HumanBodyBones.RightEye, out var rightEye);

                rig = new LookAtRig(
                    animator,
                    null,
                    referencePose,
                    null,
                    bodyFrames.ToArray(),
                    neck,
                    head,
                    leftEye,
                    rightEye);
                return true;
            }

            static bool TryCreateGeneric(
                Animator animator,
                LookAtTrack sourceTrack,
                out LookAtRig rig)
            {
                rig = null;
                if (!LookAtGenericRigUtility.TryResolve(
                        animator,
                        sourceTrack,
                        out var definition))
                {
                    return false;
                }

                var referencePose =
                    new LookAtGenericReferencePose(animator);
                var bodyFrames =
                    new List<LookAtBoneFrame>(definition.Body.Length);
                for (var i = 0; i < definition.Body.Length; i++)
                {
                    if (TryCreateFrame(
                            referencePose,
                            definition.Body[i],
                            out var frame))
                    {
                        bodyFrames.Add(frame);
                    }
                }

                TryCreateFrame(referencePose, definition.Neck, out var neck);
                if (!TryCreateFrame(
                        referencePose,
                        definition.Head,
                        out var head))
                {
                    return false;
                }
                TryCreateFrame(
                    referencePose,
                    definition.LeftEye,
                    out var leftEye);
                TryCreateFrame(
                    referencePose,
                    definition.RightEye,
                    out var rightEye);

                rig = new LookAtRig(
                    animator,
                    sourceTrack,
                    null,
                    referencePose,
                    bodyFrames.ToArray(),
                    neck,
                    head,
                    leftEye,
                    rightEye);
                return true;
            }

            static bool TryCreateFrame(
                Animator animator,
                HumanoidIKReferencePose referencePose,
                HumanBodyBones boneId,
                out LookAtBoneFrame frame)
            {
                frame = default;
                var bone = animator.GetBoneTransform(boneId);
                if (!bone ||
                    !referencePose.TryGetRelativeMatrix(
                        animator.transform,
                        bone,
                        out var rootToBone))
                {
                    return false;
                }

                var forwardInBone = LookAtUtility.GetForwardInBone(rootToBone.rotation);
                if (forwardInBone.sqrMagnitude <= 0.000001f) return false;

                frame = new LookAtBoneFrame(bone, forwardInBone.normalized);
                return true;
            }

            static bool TryCreateFrame(
                LookAtGenericReferencePose referencePose,
                Transform bone,
                out LookAtBoneFrame frame)
            {
                frame = default;
                if (!bone || referencePose == null ||
                    !referencePose.TryGetRootToBoneRotation(
                        bone,
                        out var rootToBoneRotation))
                {
                    return false;
                }

                var forwardInBone =
                    LookAtUtility.GetForwardInBone(rootToBoneRotation);
                if (forwardInBone.sqrMagnitude <= 0.000001f) return false;

                frame = new LookAtBoneFrame(
                    bone,
                    forwardInBone.normalized);
                return true;
            }
        }


        void ApplyEyelids(
            in LookAtState state,
            in LookAtEyeDirectionState eyeDirections)
        {
            if (!state.Active ||
                state.Samples == null ||
                state.SampleCount <= 0 ||
                !HasAnyEyelidConfiguration(in state))
            {
                return;
            }

            EnsureConfiguredBlendShapeLookup(in state, in eyeDirections);
            if (_blendShapeLookup == null ||
                _blendShapeLookup.Count == 0)
            {
                return;
            }

            _blendShapeAccumulators.Clear();
            var sampleCount = Mathf.Min(
                state.SampleCount,
                state.Samples.Length);
            for (var i = 0; i < sampleCount; i++)
            {
                var sample = state.Samples[i];
                if (sample.TimelineWeight <= 0f) continue;

                var hasBlink = sample.HasBlinkConfiguration();
                var hasUpperFollow =
                    eyeDirections.HasAny &&
                    sample.HasUpperEyelidFollowConfiguration();
                var hasLowerFollow =
                    eyeDirections.HasAny &&
                    sample.HasLowerEyelidFollowConfiguration();
                var hasHorizontalFollow =
                    eyeDirections.HasAny &&
                    sample.HasHorizontalEyelidFollowConfiguration();
                if (!hasBlink &&
                    !hasUpperFollow &&
                    !hasLowerFollow &&
                    !hasHorizontalFollow)
                {
                    continue;
                }

                _sampleBlendShapeTargets.Clear();
                if (hasBlink)
                {
                    AccumulateSampleBlendShapeKeys(
                        sample.ResolveBlinkBlendShapeKeys(),
                        sample.ResolveBlinkAmount());
                }

                if (hasUpperFollow)
                {
                    AccumulateSampleEyelidKeys(
                        sample.ResolveUpperEyelidFollowKeyCache(),
                        in sample,
                        upper: true,
                        in eyeDirections);
                }

                if (hasLowerFollow)
                {
                    AccumulateSampleEyelidKeys(
                        sample.ResolveLowerEyelidFollowKeyCache(),
                        in sample,
                        upper: false,
                        in eyeDirections);
                }

                if (hasHorizontalFollow)
                {
                    AccumulateSampleHorizontalEyelidKeys(
                        sample.ResolveHorizontalEyelidFollowKeyCache(),
                        in sample,
                        in eyeDirections);
                }

                foreach (var pair in _sampleBlendShapeTargets)
                {
                    _blendShapeAccumulators.TryGetValue(
                        pair.Key,
                        out var accumulator);
                    accumulator.Add(
                        pair.Value * 100f,
                        sample.TimelineWeight);
                    _blendShapeAccumulators[pair.Key] = accumulator;
                }
            }

            foreach (var pair in _blendShapeAccumulators)
            {
                ApplyBlendShape(pair.Key, pair.Value);
            }
        }

        static bool HasEyelidFollowConfiguration(in LookAtState state)
        {
            if (!state.Active ||
                state.Samples == null ||
                state.SampleCount <= 0)
            {
                return false;
            }

            var sampleCount = Mathf.Min(
                state.SampleCount,
                state.Samples.Length);
            for (var i = 0; i < sampleCount; i++)
            {
                var sample = state.Samples[i];
                if (sample.TimelineWeight > 0f &&
                    sample.HasEyelidFollowConfiguration())
                {
                    return true;
                }
            }

            return false;
        }


        static bool HasAnyEyelidConfiguration(in LookAtState state)
        {
            if (!state.Active ||
                state.Samples == null ||
                state.SampleCount <= 0)
            {
                return false;
            }

            var sampleCount = Mathf.Min(
                state.SampleCount,
                state.Samples.Length);
            for (var i = 0; i < sampleCount; i++)
            {
                var sample = state.Samples[i];
                if (sample.TimelineWeight > 0f &&
                    (sample.HasBlinkConfiguration() ||
                     sample.HasEyelidFollowConfiguration()))
                {
                    return true;
                }
            }

            return false;
        }
        bool TryGetFinalEyeDirections(
            out LookAtEyeDirectionState eyeDirections)
        {
            eyeDirections = default;
            if (_rig == null || !_rig.Head.Bone) return false;

            var headForward = _rig.Head.Bone.TransformDirection(
                _rig.Head.ForwardInBone);
            if (headForward.sqrMagnitude <= 0.000001f)
            {
                return false;
            }

            var referenceRotation = _animator.transform.rotation;
            var hasLeft = false;
            var leftPitch = 0f;
            var leftYaw = 0f;
            if (_rig.LeftEye.Bone)
            {
                var leftForward =
                    _rig.LeftEye.Bone.TransformDirection(
                        _rig.LeftEye.ForwardInBone);
                if (leftForward.sqrMagnitude > 0.000001f)
                {
                    leftPitch = LookAtUtility.GetRelativeEyePitch(
                        referenceRotation,
                        headForward,
                        leftForward);
                    leftYaw = LookAtUtility.GetRelativeEyeYaw(
                        referenceRotation,
                        headForward,
                        leftForward);
                    hasLeft = true;
                }
            }

            var hasRight = false;
            var rightPitch = 0f;
            var rightYaw = 0f;
            if (_rig.RightEye.Bone)
            {
                var rightForward =
                    _rig.RightEye.Bone.TransformDirection(
                        _rig.RightEye.ForwardInBone);
                if (rightForward.sqrMagnitude > 0.000001f)
                {
                    rightPitch = LookAtUtility.GetRelativeEyePitch(
                        referenceRotation,
                        headForward,
                        rightForward);
                    rightYaw = LookAtUtility.GetRelativeEyeYaw(
                        referenceRotation,
                        headForward,
                        rightForward);
                    hasRight = true;
                }
            }

            eyeDirections = new LookAtEyeDirectionState(
                hasLeft,
                leftPitch,
                leftYaw,
                hasRight,
                rightPitch,
                rightYaw);
            return eyeDirections.HasAny;
        }


        void EnsureConfiguredBlendShapeLookup(
            in LookAtState state,
            in LookAtEyeDirectionState eyeDirections)
        {
            EnsureBlendShapeStorage();

            var sampleCount = Mathf.Min(
                state.SampleCount,
                state.Samples.Length);
            for (var i = 0; i < sampleCount; i++)
            {
                var sample = state.Samples[i];
                if (sample.TimelineWeight <= 0f) continue;

                if (sample.HasBlinkConfiguration())
                {
                    ResolveBlendShapeKeys(
                        sample.ResolveBlinkBlendShapeKeys());
                }

                if (!eyeDirections.HasAny) continue;

                if (sample.HasUpperEyelidFollowConfiguration())
                {
                    ResolveBlendShapeKeys(
                        sample.ResolveUpperEyelidFollowKeyCache());
                }

                if (sample.HasLowerEyelidFollowConfiguration())
                {
                    ResolveBlendShapeKeys(
                        sample.ResolveLowerEyelidFollowKeyCache());
                }

                if (sample.HasHorizontalEyelidFollowConfiguration())
                {
                    ResolveBlendShapeKeys(
                        sample.ResolveHorizontalEyelidFollowKeyCache());
                }
            }
        }

        void EnsureBlendShapeStorage()
        {
            if (_blendShapeLookup != null) return;

            _blendShapeLookup =
                new Dictionary<string, List<BlendShapeBinding>>(StringComparer.Ordinal);
            _blendShapeRenderers = _animator
                ? _animator.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                : System.Array.Empty<SkinnedMeshRenderer>();
        }

        void ResolveBlendShapeKeys(string[] keys)
        {
            if (keys == null) return;

            for (var i = 0; i < keys.Length; i++)
            {
                ResolveBlendShapeKey(keys[i]);
            }
        }

        void ResolveBlendShapeKeys(LookAtEyelidBlendShapeKey[] keys)
        {
            if (keys == null) return;

            for (var i = 0; i < keys.Length; i++)
            {
                ResolveBlendShapeKey(keys[i].Key);
            }
        }

        void ResolveBlendShapeKey(string key)
        {
            if (string.IsNullOrEmpty(key) ||
                !_resolvedBlendShapeKeys.Add(key))
            {
                return;
            }

            for (var rendererIndex = 0;
                 rendererIndex < _blendShapeRenderers.Length;
                 rendererIndex++)
            {
                var renderer = _blendShapeRenderers[rendererIndex];
                var mesh = renderer ? renderer.sharedMesh : null;
                if (!mesh) continue;

                var blendShapeIndex = mesh.GetBlendShapeIndex(key);
                if (blendShapeIndex < 0) continue;

                if (!_blendShapeLookup.TryGetValue(key, out var bindings))
                {
                    bindings = new List<BlendShapeBinding>();
                    _blendShapeLookup.Add(key, bindings);
                }

                bindings.Add(new BlendShapeBinding(
                    renderer,
                    mesh,
                    blendShapeIndex));
            }
        }

        internal void CacheBlendShapeBindingsForKeys(string[] keys)
        {
            EnsureBlendShapeStorage();
            ResolveBlendShapeKeys(keys);
        }

        internal int CachedBlendShapeKeyCount =>
            _resolvedBlendShapeKeys.Count;

        internal int CachedBlendShapeBindingCount
        {
            get
            {
                var count = 0;
                if (_blendShapeLookup == null) return count;
                foreach (var pair in _blendShapeLookup) count += pair.Value.Count;
                return count;
            }
        }
        void EnsureBlendShapeLookup()
        {
            if (_blendShapeLookup != null) return;

            _blendShapeLookup =
                new Dictionary<string, List<BlendShapeBinding>>(StringComparer.Ordinal);
            if (!_animator) return;

            var renderers = _animator.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            for (var rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
            {
                var renderer = renderers[rendererIndex];
                var mesh = renderer ? renderer.sharedMesh : null;
                if (!mesh) continue;

                for (var blendShapeIndex = 0;
                     blendShapeIndex < mesh.blendShapeCount;
                     blendShapeIndex++)
                {
                    var key = mesh.GetBlendShapeName(blendShapeIndex);
                    if (string.IsNullOrEmpty(key)) continue;

                    if (!_blendShapeLookup.TryGetValue(key, out var bindings))
                    {
                        bindings = new List<BlendShapeBinding>();
                        _blendShapeLookup.Add(key, bindings);
                    }

                    bindings.Add(new BlendShapeBinding(
                        renderer,
                        mesh,
                        blendShapeIndex));
                }
            }
        }

        void AccumulateSampleBlendShapeKeys(
            string[] keys,
            float closure)
        {
            if (keys == null) return;

            closure = Mathf.Clamp01(closure);
            _sampleBlendShapeBindings.Clear();
            for (var keyIndex = 0; keyIndex < keys.Length; keyIndex++)
            {
                var key = keys[keyIndex];
                if (string.IsNullOrEmpty(key) ||
                    !_blendShapeLookup.TryGetValue(key, out var bindings))
                {
                    continue;
                }

                for (var bindingIndex = 0;
                     bindingIndex < bindings.Count;
                     bindingIndex++)
                {
                    var binding = bindings[bindingIndex];
                    if (!_sampleBlendShapeBindings.Add(binding)) continue;

                    _sampleBlendShapeTargets.TryGetValue(
                        binding,
                        out var existingClosure);
                    _sampleBlendShapeTargets[binding] =
                        LookAtUtility.CombineEyelidClosures(
                            existingClosure,
                            closure);
                }
            }
        }

        void AccumulateSampleEyelidKeys(
            LookAtEyelidBlendShapeKey[] keys,
            in LookAtSample sample,
            bool upper,
            in LookAtEyeDirectionState eyeDirections)
        {
            if (keys == null) return;

            _sampleBlendShapeBindings.Clear();
            for (var keyIndex = 0; keyIndex < keys.Length; keyIndex++)
            {
                var key = keys[keyIndex];
                if (string.IsNullOrEmpty(key.Key) ||
                    !eyeDirections.TryResolvePitch(
                        key.Side,
                        out var pitch) ||
                    !_blendShapeLookup.TryGetValue(
                        key.Key,
                        out var bindings))
                {
                    continue;
                }

                var closure = upper
                    ? sample.ResolveUpperEyelidFollowAmount(
                        pitch,
                        key.Direction)
                    : sample.ResolveLowerEyelidFollowAmount(
                        pitch,
                        key.Direction);
                AccumulateSampleEyelidBindings(bindings, closure);
            }
        }

        void AccumulateSampleHorizontalEyelidKeys(
            LookAtEyelidBlendShapeKey[] keys,
            in LookAtSample sample,
            in LookAtEyeDirectionState eyeDirections)
        {
            if (keys == null) return;

            _sampleBlendShapeBindings.Clear();
            for (var keyIndex = 0; keyIndex < keys.Length; keyIndex++)
            {
                var key = keys[keyIndex];
                if (string.IsNullOrEmpty(key.Key) ||
                    !eyeDirections.TryResolveYaw(
                        key.Side,
                        out var yaw) ||
                    !_blendShapeLookup.TryGetValue(
                        key.Key,
                        out var bindings))
                {
                    continue;
                }

                var closure =
                    sample.ResolveHorizontalEyelidFollowAmount(
                        yaw,
                        key.Direction);
                AccumulateSampleEyelidBindings(bindings, closure);
            }
        }

        void AccumulateSampleEyelidBindings(
            List<BlendShapeBinding> bindings,
            float closure)
        {
            closure = Mathf.Clamp01(closure);
            for (var bindingIndex = 0;
                 bindingIndex < bindings.Count;
                 bindingIndex++)
            {
                var binding = bindings[bindingIndex];
                if (!_sampleBlendShapeBindings.Add(binding)) continue;

                _sampleBlendShapeTargets.TryGetValue(
                    binding,
                    out var existingClosure);
                _sampleBlendShapeTargets[binding] =
                    LookAtUtility.CombineEyelidClosures(
                        existingClosure,
                        closure);
            }
        }



        void ApplyBlendShape(
            BlendShapeBinding binding,
            BlendShapeAccumulator accumulator)
        {
            if (!binding.IsValid) return;

            var baseWeight = binding.Renderer.GetBlendShapeWeight(binding.Index);
            var appliedWeight = LookAtUtility.BlendBlendShapeWeight(
                baseWeight,
                accumulator.WeightedTargetSum,
                accumulator.TimelineWeight);
            binding.Renderer.SetBlendShapeWeight(binding.Index, appliedWeight);
            _modifiedBlendShapes[binding] = new BlendShapeRecord
            {
                BaseWeight = baseWeight,
                AppliedWeight = appliedWeight
            };
        }

        void RestoreModifiedBlendShapes(bool force)
        {
            if (_modifiedBlendShapes.Count == 0) return;

            foreach (var pair in _modifiedBlendShapes)
            {
                var binding = pair.Key;
                if (!binding.IsValid) continue;

                var record = pair.Value;
                if (force ||
                    Mathf.Approximately(
                        binding.Renderer.GetBlendShapeWeight(binding.Index),
                        record.AppliedWeight))
                {
                    binding.Renderer.SetBlendShapeWeight(
                        binding.Index,
                        record.BaseWeight);
                }
            }

            _modifiedBlendShapes.Clear();
        }

        void ResetBlendShapeCache()
        {
            _blendShapeLookup = null;
            _blendShapeRenderers = System.Array.Empty<SkinnedMeshRenderer>();
            _resolvedBlendShapeKeys.Clear();
            _blendShapeAccumulators.Clear();
            _sampleBlendShapeTargets.Clear();
            _sampleBlendShapeBindings.Clear();
        }
    }
}
