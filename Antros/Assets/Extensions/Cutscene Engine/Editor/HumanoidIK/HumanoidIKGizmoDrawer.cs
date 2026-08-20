using System;
using System.Collections.Generic;
using CutsceneEngine;
using UnityEditor;
using UnityEngine;
using static CutsceneEngineEditor.HumanoidIKGizmoGeometry;
using static CutsceneEngineEditor.HumanoidIKPrimitiveRenderer;

namespace CutsceneEngineEditor
{
    internal readonly struct HumanoidIKScenePreviewPose
    {
        public readonly Transform Anchor;
        public readonly HumanoidIKTarget Target;
        public readonly Animator Animator;
        public readonly HumanoidIKLimbBones Limb;
        public readonly bool HasBoundLimb;
        public readonly bool PositionFollowsAnchor;
        public readonly Vector3 TargetPosition;
        public readonly Quaternion TargetRotation;
        public readonly Quaternion TargetBoneRotation;
        public readonly Vector3 BendTarget;
        public readonly Color GizmoColor;

        public HumanoidIKScenePreviewPose(
            Transform anchor,
            HumanoidIKTarget target,
            Animator animator,
            HumanoidIKLimbBones limb,
            bool hasBoundLimb,
            bool positionFollowsAnchor,
            Vector3 targetPosition,
            Quaternion targetRotation,
            Quaternion targetBoneRotation,
            Vector3 bendTarget,
            Color gizmoColor)
        {
            Anchor = anchor;
            Target = target;
            Animator = animator;
            Limb = limb;
            HasBoundLimb = hasBoundLimb;
            PositionFollowsAnchor = positionFollowsAnchor;
            TargetPosition = targetPosition;
            TargetRotation = targetRotation;
            TargetBoneRotation = targetBoneRotation;
            BendTarget = bendTarget;
            GizmoColor = gizmoColor;
        }
    }

    internal sealed class HumanoidIKGizmoDrawer : IDisposable
    {
        sealed class PreviewContext : IDisposable
        {
            readonly Animator _animator;
            readonly Avatar _avatar;
            readonly float[][] _handDigitBaseRadii =
            {
                new float[HandDigitCount],
                new float[HandDigitCount]
            };
            readonly bool[] _hasHandDigitBaseRadii = new bool[2];

            public readonly HumanoidIKHumanPoseSolver PoseSolver;
            public readonly HumanoidIKReferencePose ReferencePose;
            public readonly HumanoidIKCanonicalFootFit LeftFootFit;
            public readonly HumanoidIKCanonicalFootFit RightFootFit;

            PreviewContext(
                Animator animator,
                HumanoidIKHumanPoseSolver poseSolver,
                HumanoidIKReferencePose referencePose)
            {
                _animator = animator;
                _avatar = animator.avatar;
                PoseSolver = poseSolver;
                ReferencePose = referencePose;
                LeftFootFit = BuildFootFit(animator, referencePose, HumanoidIKTarget.LeftFoot);
                RightFootFit = BuildFootFit(animator, referencePose, HumanoidIKTarget.RightFoot);
            }

            public static bool TryCreate(Animator animator, out PreviewContext context)
            {
                context = null;
                if (!HumanoidIKUtility.IsUsableHumanoid(animator)) return false;

                HumanoidIKHumanPoseSolver.TryCreate(animator, out var poseSolver);
                var referencePose = poseSolver?.ReferencePose;
                if (referencePose == null)
                {
                    HumanoidIKReferencePose.TryCreate(animator, out referencePose);
                }

                if (poseSolver == null && referencePose == null) return false;
                context = new PreviewContext(animator, poseSolver, referencePose);
                return true;
            }

            public bool IsValidFor(Animator animator)
            {
                return animator && animator == _animator && animator.avatar == _avatar &&
                       (PoseSolver == null || PoseSolver.IsValidFor(animator)) &&
                       (ReferencePose == null || ReferencePose.IsValidFor(animator));
            }

            public void Dispose()
            {
                PoseSolver?.Dispose();
            }

            public HumanoidIKCanonicalFootFit GetFootFit(HumanoidIKTarget target)
            {
                return target == HumanoidIKTarget.LeftFoot ? LeftFootFit : RightFootFit;
            }

            public bool TryGetHandDigitBaseRadii(HumanoidIKTarget target, float[] destination)
            {
                var handIndex = GetHandIndex(target);
                if (handIndex < 0 ||
                    !_hasHandDigitBaseRadii[handIndex] ||
                    destination == null ||
                    destination.Length < HandDigitCount)
                {
                    return false;
                }

                Array.Copy(
                    _handDigitBaseRadii[handIndex],
                    destination,
                    HandDigitCount);
                return true;
            }

            public void CacheHandDigitBaseRadii(HumanoidIKTarget target, float[] source)
            {
                var handIndex = GetHandIndex(target);
                if (handIndex < 0 || source == null || source.Length < HandDigitCount) return;

                Array.Copy(source, _handDigitBaseRadii[handIndex], HandDigitCount);
                _hasHandDigitBaseRadii[handIndex] = true;
            }

            static int GetHandIndex(HumanoidIKTarget target)
            {
                return target switch
                {
                    HumanoidIKTarget.LeftHand => 0,
                    HumanoidIKTarget.RightHand => 1,
                    _ => -1
                };
            }

            static HumanoidIKCanonicalFootFit BuildFootFit(
                Animator animator,
                HumanoidIKReferencePose referencePose,
                HumanoidIKTarget target)
            {
                if (!animator || referencePose == null || !HumanoidIKUtility.IsFoot(target)) return null;

                var isLeftFoot = target == HumanoidIKTarget.LeftFoot;
                var foot = animator.GetBoneTransform(
                    isLeftFoot ? HumanBodyBones.LeftFoot : HumanBodyBones.RightFoot);
                var toes = animator.GetBoneTransform(
                    isLeftFoot ? HumanBodyBones.LeftToes : HumanBodyBones.RightToes);
                var lowerLeg = animator.GetBoneTransform(
                    isLeftFoot ? HumanBodyBones.LeftLowerLeg : HumanBodyBones.RightLowerLeg);
                if (!foot || !lowerLeg ||
                    !referencePose.TryGetRelativeMatrix(animator.transform, foot, out var footMatrix) ||
                    !referencePose.TryGetRelativeMatrix(animator.transform, lowerLeg, out var lowerLegMatrix))
                {
                    return null;
                }

                var toeMatrix = Matrix4x4.identity;
                var hasToeMatrix = toes &&
                                   referencePose.TryGetRelativeMatrix(
                                       animator.transform,
                                       toes,
                                       out toeMatrix);
                if (!HumanoidIKUtility.TryBuildFootDisplayRotation(
                        footMatrix,
                        hasToeMatrix,
                        toeMatrix,
                        lowerLegMatrix,
                        out var displayRotation))
                {
                    return null;
                }

                var rootScale = GetUniformRootScale(animator.transform.lossyScale);
                var footPosition = footMatrix.MultiplyPoint3x4(Vector3.zero);
                var toeInSole = Vector3.zero;
                if (hasToeMatrix)
                {
                    toeInSole = Quaternion.Inverse(displayRotation) *
                                (toeMatrix.MultiplyPoint3x4(Vector3.zero) - footPosition) *
                                rootScale;
                }

                // The imported Foot bone points diagonally from ankle to toes. Its
                // vertical drop is a height measurement, never the sole forward axis.
                var footToToeDistance = hasToeMatrix
                    ? Mathf.Max(toeInSole.z, 0.0001f)
                    : Mathf.Max(animator.humanScale, 0.01f) * 0.14f * rootScale;
                var soleHeight = hasToeMatrix
                    ? GetBoundFootSoleHeight(toeInSole.y)
                    : 0f;
                var footToToeVerticalDrop = hasToeMatrix
                    ? Mathf.Max(0f, -toeInSole.y)
                    : 0f;

                var chains = new List<Transform[]>(CanonicalToeCount);
                HumanoidIKUtility.GetDigitChains(animator, target, chains);
                var toeRigKind = HumanoidIKUtility.GetToeRigKind(animator, target);
                var toeBases = new Vector3[CanonicalToeCount];
                var toeLengths = new float[CanonicalToeCount];
                var toeForwards = new Vector3[CanonicalToeCount];
                var hasOverrides = new bool[CanonicalToeCount];
                var rootPositions = new Vector3[CanonicalToeCount];
                var hasRoot = new bool[CanonicalToeCount];
                var minimumRootX = float.PositiveInfinity;
                var maximumRootX = float.NegativeInfinity;
                var fittedCount = Mathf.Min(chains.Count, CanonicalToeCount);

                if (toeRigKind == HumanoidIKToeRigKind.ArticulatedToes)
                {
                    for (var toeIndex = 0; toeIndex < fittedCount; toeIndex++)
                    {
                        var chain = chains[toeIndex];
                        if (chain == null || chain.Length == 0 || !chain[0] ||
                            !TryGetReferenceDisplayPosition(
                                referencePose,
                                animator.transform,
                                chain[0],
                                footPosition,
                                displayRotation,
                                rootScale,
                                out var rootPosition))
                        {
                            continue;
                        }

                        rootPositions[toeIndex] = rootPosition;
                        hasRoot[toeIndex] = true;
                        minimumRootX = Mathf.Min(minimumRootX, rootPosition.x);
                        maximumRootX = Mathf.Max(maximumRootX, rootPosition.x);
                    }
                }

                var toeRootSpan = maximumRootX > minimumRootX
                    ? maximumRootX - minimumRootX
                    : 0f;
                var fitScale = GetCanonicalFootFitScale(
                    footToToeDistance,
                    toeRootSpan,
                    footToToeVerticalDrop);
                if (toeRigKind == HumanoidIKToeRigKind.ToeFoot && hasToeMatrix)
                {
                    // A simple Humanoid Toe mapping is the shared Toe Base pivot,
                    // not a per-digit root. Keep the five synthetic roots at their
                    // canonical forward offsets from that mapped pivot.
                    for (var toeIndex = 0; toeIndex < CanonicalToeCount; toeIndex++)
                    {
                        var canonicalToe = GetCanonicalToe(isLeftFoot, toeIndex);
                        toeBases[toeIndex] = GetFittedToeGroupBase(
                            isLeftFoot,
                            toeIndex,
                            fitScale,
                            toeInSole);
                        toeLengths[toeIndex] = canonicalToe.Length * fitScale.z;
                        toeForwards[toeIndex] = canonicalToe.Forward;
                        hasOverrides[toeIndex] = true;
                    }
                }
                else
                {
                    for (var toeIndex = 0; toeIndex < fittedCount; toeIndex++)
                    {
                        if (!hasRoot[toeIndex]) continue;

                        toeBases[toeIndex] = rootPositions[toeIndex];
                        var canonicalToe = GetCanonicalToe(isLeftFoot, toeIndex);
                        var defaultLength = canonicalToe.Length * fitScale.z;
                        toeLengths[toeIndex] = TryEstimateToeMetrics(
                            referencePose,
                            animator.transform,
                            chains[toeIndex],
                            footPosition,
                            displayRotation,
                            rootScale,
                            out var measuredLength,
                            out var referenceForward)
                            ? GetFittedToeLength(measuredLength, defaultLength)
                            : defaultLength;
                        toeForwards[toeIndex] = referenceForward;
                        hasOverrides[toeIndex] = true;
                    }
                }

                return new HumanoidIKCanonicalFootFit(
                    fitScale,
                    soleHeight,
                    toeBases,
                    toeLengths,
                    hasOverrides,
                    toeForwards,
                    hasToeMatrix ? (Vector3?)toeInSole : null);
            }

            static bool TryGetReferenceDisplayPosition(
                HumanoidIKReferencePose referencePose,
                Transform animatorRoot,
                Transform bone,
                Vector3 footPosition,
                Quaternion displayRotation,
                float rootScale,
                out Vector3 position)
            {
                position = Vector3.zero;
                if (!bone ||
                    !referencePose.TryGetRelativeMatrix(animatorRoot, bone, out var matrix))
                {
                    return false;
                }

                position = Quaternion.Inverse(displayRotation) *
                           (matrix.MultiplyPoint3x4(Vector3.zero) - footPosition) * rootScale;
                return true;
            }

            static bool TryEstimateToeMetrics(
                HumanoidIKReferencePose referencePose,
                Transform animatorRoot,
                Transform[] chain,
                Vector3 footPosition,
                Quaternion displayRotation,
                float rootScale,
                out float length,
                out Vector3 forward)
            {
                length = 0f;
                forward = Vector3.forward;
                if (chain == null || chain.Length < 2) return false;

                var pointCount = 0;
                var previousPosition = Vector3.zero;
                var firstSegment = Vector3.zero;
                for (var i = 0; i < chain.Length; i++)
                {
                    var bone = chain[i];
                    if (!TryGetReferenceDisplayPosition(
                            referencePose,
                            animatorRoot,
                            bone,
                            footPosition,
                            displayRotation,
                            rootScale,
                            out var position))
                    {
                        break;
                    }

                    if (pointCount > 0)
                    {
                        var segment = position - previousPosition;
                        length += segment.magnitude;
                        if (pointCount == 1) firstSegment = segment;
                    }

                    previousPosition = position;
                    pointCount++;
                }

                if (pointCount < 2 ||
                    length <= 0.0001f ||
                    firstSegment.sqrMagnitude <= 0.000001f)
                {
                    return false;
                }

                // Toe direction is anatomical fan in the sole plane. Vertical
                // chain offsets affect root height, not the preview's forward.
                firstSegment.y = 0f;
                if (firstSegment.sqrMagnitude <= 0.000001f) return false;
                forward = firstSegment.normalized;

                // Do not extrapolate a removed distal display joint. The mapped
                // span remains the fitted source length and the renderer keeps
                // only the original 40% / 32% reach.
                return true;
            }
            static float GetUniformRootScale(Vector3 lossyScale)
            {
                return (Mathf.Abs(lossyScale.x) +
                        Mathf.Abs(lossyScale.y) +
                        Mathf.Abs(lossyScale.z)) / 3f;
            }
        }

        readonly HumanoidIKPrimitiveRenderer _renderer = new HumanoidIKPrimitiveRenderer();
        readonly Dictionary<Animator, PreviewContext> _previewContexts =
            new Dictionary<Animator, PreviewContext>();
        readonly List<Transform[]> _digitChains = new List<Transform[]>();
        readonly List<Vector3> _scratchPoints = new List<Vector3>();
        readonly Vector3[] _handDigitBasePositions = new Vector3[HandDigitCount];
        readonly bool[] _hasHandDigitBasePosition = new bool[HandDigitCount];
        readonly float[] _handDigitTotalLengths = new float[HandDigitCount];
        readonly float[] _handDigitProximalSpacings = new float[HandDigitCount];
        readonly float[] _handDigitBaseRadii = new float[HandDigitCount];
        readonly List<Vector3>[] _handDigitPreviewPoints =
        {
            new List<Vector3>(4),
            new List<Vector3>(4),
            new List<Vector3>(4),
            new List<Vector3>(4),
            new List<Vector3>(4)
        };
        readonly List<Transform> _scratchTransformPath = new List<Transform>();
        readonly Dictionary<Transform, Quaternion> _resolvedPreviewRotations =
            new Dictionary<Transform, Quaternion>();

        PreviewContext _activePreviewContext;

        internal int CachedAnimatorCount => _previewContexts.Count;
        internal int PendingBoxCommandCount => _renderer.PendingBoxCommandCount;

        internal void BeginFrame(EventType eventType)
        {
            _renderer.BeginFrame(eventType);
        }

        internal void FlushFrame()
        {
            _renderer.FlushFrame();
        }

        internal void CancelFrame()
        {
            _renderer.CancelFrame();
        }

        internal void ClearPreviewContexts()
        {
            foreach (var context in _previewContexts.Values)
            {
                context.Dispose();
            }

            _previewContexts.Clear();
            _activePreviewContext = null;
            _resolvedPreviewRotations.Clear();
        }

        public void Dispose()
        {
            CancelFrame();
            _renderer.Dispose();
            ClearPreviewContexts();
        }

        internal bool TryResolveClipPreview(
            HumanoidIKClip clip,
            UnityEngine.Playables.PlayableDirector director,
            HumanoidIKTrack track,
            float opacityMultiplier,
            out HumanoidIKScenePreviewPose pose)
        {
            pose = default;
            if (!clip || !director || !track) return false;

            clip.ResolveEffectiveSpace(
                director,
                director.transform,
                out var anchor,
                out var positionFollowsAnchor,
                out var resolvedPosition,
                out var resolvedRotation,
                out var resolvedBendTarget);
            var animator = director.GetGenericBinding(track) as Animator;
            var limb = default(HumanoidIKLimbBones);
            var hasBoundLimb = HumanoidIKUtility.IsUsableHumanoid(animator) &&
                               HumanoidIKUtility.TryGetLimbBones(animator, track.target, out limb);

            HumanoidIKUtility.ResolveWorldPose(
                anchor,
                resolvedPosition,
                resolvedRotation,
                resolvedBendTarget,
                out var targetPosition,
                out var storedRotation,
                out var legacyBendTarget);

            var boneToEffectorRotation = Quaternion.identity;
            if (hasBoundLimb)
            {
                TryGetPreviewBoneToEffectorRotation(
                    animator,
                    track.target,
                    out boneToEffectorRotation);
            }

            var targetRotation = HumanoidIKUtility.ToEffectorRotation(
                storedRotation,
                clip.RotationSpace,
                boneToEffectorRotation);
            if (HumanoidIKUtility.IsFoot(track.target) &&
                TryGetPreviewLegacyFootBoneToEffectorRotation(
                    animator,
                    track.target,
                    out var boneToLegacyFootLineRotation))
            {
                targetRotation = HumanoidIKUtility.ToProjectedSoleRotation(
                    storedRotation,
                    clip.RotationSpace,
                    clip.FootRotationFrameVersion,
                    boneToEffectorRotation,
                    boneToLegacyFootLineRotation);
            }

            var targetBoneRotation = HumanoidIKUtility.ToBoneRotation(
                targetRotation,
                boneToEffectorRotation);
            var bendTarget = legacyBendTarget;

            var gizmoColor = clip.GetGizmoColor(track.target);
            gizmoColor.a *= Mathf.Clamp01(opacityMultiplier);
            pose = new HumanoidIKScenePreviewPose(
                anchor,
                track.target,
                animator,
                limb,
                hasBoundLimb,
                positionFollowsAnchor,
                targetPosition,
                targetRotation,
                targetBoneRotation,
                bendTarget,
                gizmoColor);
            return true;
        }

        internal void DrawClipPreview(
            HumanoidIKClip clip,
            in HumanoidIKScenePreviewPose pose)
        {
            if (pose.HasBoundLimb)
            {
                var previewMidPosition = DrawLimbPreview(
                    clip,
                    pose.Animator,
                    pose.Target,
                    pose.Limb,
                    pose.TargetPosition,
                    pose.TargetRotation,
                    pose.TargetBoneRotation,
                    pose.BendTarget,
                    pose.GizmoColor);

                Handles.color = pose.GizmoColor;
                var bendSize = HandleUtility.GetHandleSize(pose.BendTarget) * 0.08f;
                Handles.SphereHandleCap(
                    0,
                    pose.BendTarget,
                    Quaternion.identity,
                    bendSize,
                    EventType.Repaint);
                Handles.DrawDottedLine(previewMidPosition, pose.BendTarget, 4f);
                return;
            }

            DrawDefaultEndShape(
                pose.Target,
                pose.TargetPosition,
                pose.TargetRotation,
                pose.GizmoColor);
        }

        static Vector3 GetBendHandlePoint(HumanoidIKLimbBones limb, Vector3 bendVector)
        {
            if (bendVector.sqrMagnitude > 0.000001f)
            {
                return limb.Upper.position + bendVector;
            }

            bendVector = limb.Lower.position - limb.Upper.position;
            if (bendVector.sqrMagnitude <= 0.000001f) return limb.Lower.position;

            return limb.Upper.position + bendVector;
        }

        internal bool TryGetPreviewBoneToEffectorRotation(
            Animator animator,
            HumanoidIKTarget target,
            out Quaternion boneToEffectorRotation)
        {
            var context = GetPreviewContext(animator);
            if (context?.PoseSolver != null &&
                context.PoseSolver.TryGetBoneToEffectorRotation(target, out boneToEffectorRotation))
            {
                return true;
            }

            boneToEffectorRotation = Quaternion.identity;
            return false;
        }

        internal bool TryGetPreviewLegacyFootBoneToEffectorRotation(
            Animator animator,
            HumanoidIKTarget target,
            out Quaternion boneToEffectorRotation)
        {
            var context = GetPreviewContext(animator);
            if (context?.PoseSolver != null &&
                context.PoseSolver.TryGetLegacyFootBoneToEffectorRotation(
                    target,
                    out boneToEffectorRotation))
            {
                return true;
            }

            boneToEffectorRotation = Quaternion.identity;
            return false;
        }

        internal Vector3 DrawLimbPreview(
            HumanoidIKClip clip,
            Animator animator,
            HumanoidIKTarget target,
            HumanoidIKLimbBones limb,
            Vector3 targetPosition,
            Quaternion targetEffectorRotation,
            Quaternion targetBoneRotation,
            Vector3 bendTargetPosition,
            Color previewColor)
        {
            var rootPosition = limb.Upper.position;
            var currentMidPosition = limb.Lower.position;
            var currentEndPosition = limb.End.position;
            var previewMidPosition = SolvePreviewMidpoint(
                rootPosition,
                currentMidPosition,
                currentEndPosition,
                targetPosition,
                bendTargetPosition);

            Handles.color = previewColor;
            Handles.DrawAAPolyLine(5f, rootPosition, previewMidPosition, targetPosition);
            Handles.SphereHandleCap(
                0,
                rootPosition,
                Quaternion.identity,
                HandleUtility.GetHandleSize(rootPosition) * 0.06f,
                EventType.Repaint);
            Handles.SphereHandleCap(
                0,
                previewMidPosition,
                Quaternion.identity,
                HandleUtility.GetHandleSize(previewMidPosition) * 0.05f,
                EventType.Repaint);

            DrawEndShape(
                animator,
                target,
                limb.End,
                targetPosition,
                targetEffectorRotation,
                targetBoneRotation,
                previewColor,
                in clip.digitBends,
                clip.toeBaseBend,
                clip.toeFan);

            return previewMidPosition;
        }

        internal void DrawDefaultEndShape(
            HumanoidIKTarget target,
            Vector3 targetPosition,
            Quaternion targetEffectorRotation,
            Color previewColor)
        {
            if (HumanoidIKUtility.IsHand(target))
            {
                var targetMatrix = Matrix4x4.TRS(targetPosition, targetEffectorRotation, Vector3.one);
                var palmCenter = targetPosition + targetEffectorRotation * Vector3.forward * 0.055f;
                _renderer.DrawBox(
                    palmCenter,
                    targetEffectorRotation,
                    new Vector3(0.085f, 0.028f, 0.11f),
                    previewColor);
                _renderer.DrawSphere(
                    targetPosition,
                    GetHandWristSphereRadius(0.085f),
                    previewColor);

                DrawDefaultDigitChain(targetMatrix, new Vector3(-0.03f, 0f, 0.105f), 0.07f, previewColor);
                DrawDefaultDigitChain(targetMatrix, new Vector3(-0.01f, 0f, 0.108f), 0.082f, previewColor);
                DrawDefaultDigitChain(targetMatrix, new Vector3(0.01f, 0f, 0.107f), 0.078f, previewColor);
                DrawDefaultDigitChain(targetMatrix, new Vector3(0.03f, 0f, 0.101f), 0.066f, previewColor);

                var thumbSide = target == HumanoidIKTarget.LeftHand ? 1f : -1f;
                DrawDefaultThumbChain(targetMatrix, thumbSide, previewColor);
            }
            else
            {
                DrawCanonicalFoot(
                    target,
                    targetPosition,
                    targetEffectorRotation,
                    HumanoidIKToeRigKind.None,
                    default,
                    0f,
                    0f,
                    null,
                    previewColor);
            }

            var handleSize = HandleUtility.GetHandleSize(targetPosition);
            _renderer.DrawSphere(
                targetPosition,
                handleSize * 0.07f,
                WithAlpha(previewColor, previewColor.a * 0.7f));
        }

        void DrawEndShape(
            Animator animator,
            HumanoidIKTarget target,
            Transform endBone,
            Vector3 targetPosition,
            Quaternion targetEffectorRotation,
            Quaternion targetBoneRotation,
            Color previewColor,
            in HumanoidIKDigitBendPose digitBends,
            float toeBaseBend,
            float toeFan)
        {
            if (HumanoidIKUtility.IsFoot(target))
            {
                var footContext = GetPreviewContext(animator);
                DrawCanonicalFoot(
                    target,
                    targetPosition,
                    targetEffectorRotation,
                    HumanoidIKUtility.GetToeRigKind(animator, target),
                    in digitBends,
                    toeBaseBend,
                    toeFan,
                    footContext?.GetFootFit(target),
                    previewColor);
                DrawTargetMarker(targetPosition, previewColor);
                return;
            }

            var context = GetPreviewContext(animator);
            if (!endBone || context?.ReferencePose == null)
            {
                DrawDefaultEndShape(target, targetPosition, targetEffectorRotation, previewColor);
                return;
            }

            var previousContext = _activePreviewContext;
            _activePreviewContext = context;
            try
            {
                var targetBoneMatrix = Matrix4x4.TRS(
                    targetPosition,
                    targetBoneRotation,
                    GetPreviewScale(animator, endBone));
                Dictionary<Transform, Quaternion> resolvedHandRotations = null;
                HumanoidIKUtility.GetDigitChains(animator, target, _digitChains);
                CacheHandDigitBaseMeasurements(
                    targetBoneMatrix,
                    endBone,
                    targetEffectorRotation);
                if (!context.TryGetHandDigitBaseRadii(target, _handDigitBaseRadii))
                {
                    CacheHandDigitBaseRadii();
                    context.CacheHandDigitBaseRadii(target, _handDigitBaseRadii);
                }
                DrawPalmBox(targetEffectorRotation, targetPosition, previewColor);
                if (TryResolvePreviewHandRotations(context, target, in digitBends))
                {
                    resolvedHandRotations = _resolvedPreviewRotations;
                }

                CacheHandDigitPreviewGeometry(
                    targetBoneMatrix,
                    endBone,
                    resolvedHandRotations,
                    in digitBends);
                for (var digitIndex = 0;
                     digitIndex < Mathf.Min(HandDigitCount, _digitChains.Count);
                     digitIndex++)
                {
                    DrawDigitChainPrimitives(digitIndex, previewColor);
                }

                DrawTargetMarker(targetPosition, previewColor);
            }
            finally
            {
                _activePreviewContext = previousContext;
            }
        }

        void DrawCanonicalFoot(
            HumanoidIKTarget target,
            Vector3 targetPosition,
            Quaternion targetFootRotation,
            HumanoidIKToeRigKind toeRigKind,
            in HumanoidIKDigitBendPose digitBends,
            float toeBaseBend,
            float toeFan,
            HumanoidIKCanonicalFootFit fit,
            Color color)
        {
            var isLeftFoot = target == HumanoidIKTarget.LeftFoot;
            var footToWorld = Matrix4x4.TRS(targetPosition, targetFootRotation, Vector3.one);
            for (var slabIndex = 0; slabIndex < CanonicalFootSlabCount; slabIndex++)
            {
                GetFittedFootSlabPose(
                    isLeftFoot,
                    slabIndex,
                    fit,
                    toeRigKind,
                    toeBaseBend,
                    out var slab,
                    out var localCenter,
                    out var localRotation);
                _renderer.DrawBox(
                    footToWorld.MultiplyPoint3x4(localCenter),
                    targetFootRotation * localRotation,
                    slab.Size,
                    color);
            }

            GetFittedToeBridgeSlabPose(
                isLeftFoot,
                fit,
                toeRigKind,
                in digitBends,
                toeBaseBend,
                out var toeBridge,
                out var toeBridgeCenter,
                out var toeBridgeRotation);
            _renderer.DrawBox(
                footToWorld.MultiplyPoint3x4(toeBridgeCenter),
                targetFootRotation * toeBridgeRotation,
                toeBridge.Size,
                color);

            for (var toeIndex = 0; toeIndex < CanonicalToeCount; toeIndex++)
            {
                var toe = fit?.GetToe(isLeftFoot, toeIndex) ??
                          GetCanonicalToe(isLeftFoot, toeIndex);
                var toeBasePivot = fit?.GetToeBasePivot(isLeftFoot) ??
                                   GetCanonicalToeBasePivot(isLeftFoot);
                BuildCanonicalToePoints(
                    isLeftFoot,
                    toeIndex,
                    in toe,
                    toeBasePivot,
                    toeRigKind,
                    in digitBends,
                    toeBaseBend,
                    toeFan,
                    _scratchPoints);
                for (var pointIndex = 0; pointIndex < _scratchPoints.Count; pointIndex++)
                {
                    _scratchPoints[pointIndex] = footToWorld.MultiplyPoint3x4(_scratchPoints[pointIndex]);
                }

                DrawScratchDigitChain(
                    color,
                    fixedRadius: toe.Radius);
            }
        }

        void DrawTargetMarker(Vector3 targetPosition, Color color)
        {
            var handleSize = HandleUtility.GetHandleSize(targetPosition);
            _renderer.DrawSphere(
                targetPosition,
                handleSize * 0.07f,
                WithAlpha(color, color.a * 0.7f));
        }

        void DrawDefaultDigitChain(
            Matrix4x4 targetMatrix,
            Vector3 basePoint,
            float length,
            Color color,
            float diameterToLength = DefaultHandDigitDiameterToFirstSegment,
            float fixedDiameterReferenceLength = -1f)
        {
            _scratchPoints.Clear();
            _scratchPoints.Add(targetMatrix.MultiplyPoint3x4(basePoint));
            _scratchPoints.Add(targetMatrix.MultiplyPoint3x4(basePoint + Vector3.forward * (length * 0.4f)));
            _scratchPoints.Add(targetMatrix.MultiplyPoint3x4(basePoint + Vector3.forward * (length * 0.72f)));
            _scratchPoints.Add(targetMatrix.MultiplyPoint3x4(basePoint + Vector3.forward * length));
            DrawScratchDigitChain(color, diameterToLength, fixedDiameterReferenceLength);
        }

        void DrawDefaultThumbChain(Matrix4x4 targetMatrix, float side, Color color)
        {
            _scratchPoints.Clear();
            _scratchPoints.Add(targetMatrix.MultiplyPoint3x4(new Vector3(0.04f * side, -0.002f, 0.035f)));
            _scratchPoints.Add(targetMatrix.MultiplyPoint3x4(new Vector3(0.056f * side, -0.002f, 0.052f)));
            _scratchPoints.Add(targetMatrix.MultiplyPoint3x4(new Vector3(0.067f * side, -0.001f, 0.071f)));
            _scratchPoints.Add(targetMatrix.MultiplyPoint3x4(new Vector3(0.073f * side, 0f, 0.089f)));
            DrawScratchDigitChain(color, isThumb: true);
        }

        void DrawPalmBox(
            Quaternion targetEffectorRotation,
            Vector3 targetPosition,
            Color color)
        {
            if (!TryGetHandPalmMeasurements(
                    targetPosition,
                    targetEffectorRotation,
                    _handDigitBasePositions,
                    _hasHandDigitBasePosition,
                    _handDigitBaseRadii,
                    out var palmCenter,
                    out var baseWidth,
                    out var palmLength,
                    out var palmThickness))
            {
                var center = targetPosition + targetEffectorRotation * Vector3.forward * 0.055f;
                _renderer.DrawBox(
                    center,
                    targetEffectorRotation,
                    new Vector3(0.085f, 0.028f, 0.11f),
                    color);
                _renderer.DrawSphere(
                    targetPosition,
                    GetHandWristSphereRadius(0.085f),
                    color);
                return;
            }

            // The Avatar supplies only immutable reference dimensions. The authored
            // effector frame remains the one display frame for the entire palm.
            _renderer.DrawBox(
                palmCenter,
                targetEffectorRotation,
                GetHandPalmBoxSize(baseWidth, palmLength, palmThickness),
                color);
            _renderer.DrawSphere(
                targetPosition,
                GetHandWristSphereRadius(baseWidth),
                color);
        }

        void CacheHandDigitBaseMeasurements(
            Matrix4x4 targetBoneMatrix,
            Transform endBone,
            Quaternion targetEffectorRotation)
        {
            Array.Clear(_handDigitBasePositions, 0, _handDigitBasePositions.Length);
            Array.Clear(_hasHandDigitBasePosition, 0, _hasHandDigitBasePosition.Length);
            Array.Clear(_handDigitTotalLengths, 0, _handDigitTotalLengths.Length);
            Array.Clear(_handDigitProximalSpacings, 0, _handDigitProximalSpacings.Length);

            var availableDigitCount = Mathf.Min(HandDigitCount, _digitChains.Count);
            for (var digitIndex = 0; digitIndex < availableDigitCount; digitIndex++)
            {
                var chain = _digitChains[digitIndex];
                if (chain == null || chain.Length == 0 || !chain[0]) continue;
                if (!TryGetReferenceRelativePosition(endBone, chain[0], out var localPoint)) continue;

                _handDigitBasePositions[digitIndex] = targetBoneMatrix.MultiplyPoint3x4(localPoint);
                _hasHandDigitBasePosition[digitIndex] = true;
            }

            for (var digitIndex = 0; digitIndex < availableDigitCount; digitIndex++)
            {
                var chain = _digitChains[digitIndex];
                if (chain != null && TryBuildStableDigitPreviewPoints(
                        targetBoneMatrix,
                        endBone,
                        chain,
                        null,
                        default))
                {
                    _handDigitTotalLengths[digitIndex] = GetPolylineLength(_scratchPoints);
                }

                _handDigitProximalSpacings[digitIndex] = GetNearestHandDigitProximalSpacing(
                    digitIndex,
                    _handDigitBasePositions,
                    _hasHandDigitBasePosition,
                    targetEffectorRotation);
            }
        }

        void CacheHandDigitBaseRadii()
        {
            Array.Clear(_handDigitBaseRadii, 0, _handDigitBaseRadii.Length);
            for (var digitIndex = 0; digitIndex < HandDigitCount; digitIndex++)
            {
                _handDigitBaseRadii[digitIndex] = GetHandDigitBaseRadius(
                    _handDigitTotalLengths[digitIndex],
                    _handDigitProximalSpacings[digitIndex],
                    digitIndex == 0);
            }
        }

        void CacheHandDigitPreviewGeometry(
            Matrix4x4 targetBoneMatrix,
            Transform endBone,
            Dictionary<Transform, Quaternion> resolvedHandRotations,
            in HumanoidIKDigitBendPose digitBends)
        {
            for (var digitIndex = 0; digitIndex < HandDigitCount; digitIndex++)
            {
                _handDigitPreviewPoints[digitIndex].Clear();
            }

            var availableDigitCount = Mathf.Min(HandDigitCount, _digitChains.Count);
            for (var digitIndex = 0; digitIndex < availableDigitCount; digitIndex++)
            {
                var chain = _digitChains[digitIndex];
                var bend = HumanoidIKUtility.GetDigitBend(in digitBends, digitIndex);
                if (chain == null || !TryBuildStableDigitPreviewPoints(
                        targetBoneMatrix,
                        endBone,
                        chain,
                        resolvedHandRotations,
                        bend))
                {
                    continue;
                }

                _handDigitPreviewPoints[digitIndex].AddRange(_scratchPoints);
            }
        }

        void DrawDigitChainPrimitives(int digitIndex, Color color)
        {
            if (digitIndex < 0 || digitIndex >= HandDigitCount) return;
            var points = _handDigitPreviewPoints[digitIndex];
            if (points.Count == 0) return;

            _scratchPoints.Clear();
            _scratchPoints.AddRange(points);
            DrawScratchDigitChain(
                color,
                1f,
                _handDigitBaseRadii[digitIndex] * 2f,
                isThumb: digitIndex == 0,
                minimumRadius: 0f);
        }

        bool TryBuildStableDigitPreviewPoints(
            Matrix4x4 targetBoneMatrix,
            Transform endBone,
            Transform[] chain,
            Dictionary<Transform, Quaternion> resolvedHandRotations,
            HumanoidIKJointBend bend)
        {
            if (!endBone || chain == null) return false;

            _scratchPoints.Clear();
            var localPosition = Vector3.zero;
            var localRotation = Quaternion.identity;
            var localScale = Vector3.one;
            var pathRoot = endBone;
            var hasPreviousChainPoint = false;
            var hasLastChainPoint = false;
            var previousChainPoint = Vector3.zero;
            var lastChainPoint = Vector3.zero;
            var lastReferenceDistalFrameRotation = Quaternion.identity;
            var lastPosedDistalFrameRotation = Quaternion.identity;

            for (var i = 0; i < chain.Length; i++)
            {
                if (!chain[i]) continue;
                if (!TryBuildTransformPath(pathRoot, chain[i], _scratchTransformPath)) return false;

                for (var j = 0; j < _scratchTransformPath.Count; j++)
                {
                    var step = _scratchTransformPath[j];
                    if (!TryGetPreviewBonePose(step, out var referencePose)) return false;

                    localPosition += localRotation * Vector3.Scale(localScale, referencePose.Position);
                    var resolvedHandRotation = Quaternion.identity;
                    var hasResolvedHandRotation = resolvedHandRotations != null &&
                                                  resolvedHandRotations.TryGetValue(
                                                      step,
                                                      out resolvedHandRotation);
                    var stepReferenceRotation = referencePose.Rotation;
                    var stepPosedRotation = hasResolvedHandRotation
                        ? resolvedHandRotation
                        : referencePose.Rotation;
                    var chainIndex = GetChainBoneIndex(chain, step);
                    if (!hasResolvedHandRotation && chainIndex >= 0)
                    {
                        stepPosedRotation *= Quaternion.Euler(GetJointBendEuler(bend, chainIndex));
                    }

                    if (step == chain[i])
                    {
                        if (hasLastChainPoint)
                        {
                            previousChainPoint = lastChainPoint;
                            hasPreviousChainPoint = true;
                        }

                        lastChainPoint = localPosition;
                        lastReferenceDistalFrameRotation = localRotation * stepReferenceRotation;
                        lastPosedDistalFrameRotation = localRotation * stepPosedRotation;
                        hasLastChainPoint = true;
                        _scratchPoints.Add(targetBoneMatrix.MultiplyPoint3x4(localPosition));
                    }

                    localRotation *= stepPosedRotation;
                    localScale = Vector3.Scale(localScale, referencePose.Scale);
                }

                pathRoot = chain[i];
            }

            AppendStableDigitTipPoint(
                targetBoneMatrix,
                hasPreviousChainPoint,
                previousChainPoint,
                lastChainPoint,
                lastReferenceDistalFrameRotation,
                lastPosedDistalFrameRotation);
            return _scratchPoints.Count > 0;
        }

        void AppendStableDigitTipPoint(
            Matrix4x4 targetBoneMatrix,
            bool hasPreviousPoint,
            Vector3 previousLocalPoint,
            Vector3 tipLocalPoint,
            Quaternion referenceDistalFrameRotation,
            Quaternion posedDistalFrameRotation)
        {
            if (!hasPreviousPoint || _scratchPoints.Count == 0) return;

            var syntheticTip = GetSyntheticHandDigitTipPoint(
                previousLocalPoint,
                tipLocalPoint,
                referenceDistalFrameRotation,
                posedDistalFrameRotation);
            if ((syntheticTip - tipLocalPoint).sqrMagnitude <= 0.000001f) return;

            _scratchPoints.Add(targetBoneMatrix.MultiplyPoint3x4(syntheticTip));
        }

        void DrawScratchDigitChain(
            Color color,
            float diameterToLength = DefaultHandDigitDiameterToFirstSegment,
            float fixedDiameterReferenceLength = -1f,
            float fixedRadius = 0f,
            bool isThumb = false,
            float minimumRadius = 0.0015f)
        {
            if (_scratchPoints.Count == 0) return;
            if (_scratchPoints.Count == 1)
            {
                _renderer.DrawSphere(_scratchPoints[0], 0.003f, color);
                return;
            }

            var radiusReferenceLength = fixedDiameterReferenceLength;
            if (fixedRadius <= 0f && radiusReferenceLength < 0f)
            {
                for (var i = 0; i < _scratchPoints.Count - 1; i++)
                {
                    var candidateLength = Vector3.Distance(_scratchPoints[i], _scratchPoints[i + 1]);
                    if (candidateLength <= 0.0001f) continue;

                    radiusReferenceLength = candidateLength;
                    break;
                }
            }

            var lastRadius = 0.003f;
            for (var i = 0; i < _scratchPoints.Count - 1; i++)
            {
                var start = _scratchPoints[i];
                var end = _scratchPoints[i + 1];
                var length = Vector3.Distance(start, end);
                if (length <= 0.0001f) continue;

                var radius = fixedRadius > 0f
                    ? fixedRadius
                    : GetDigitPrimitiveRadius(
                        length,
                        diameterToLength * GetHandDigitRadiusScale(i, isThumb),
                        radiusReferenceLength,
                        minimumRadius);
                lastRadius = radius;
                _renderer.DrawCylinder(start, end, radius, color);
                var jointRadiusScale = fixedRadius > 0f
                    ? 1.08f
                    : HandDigitJointRadiusScale;
                _renderer.DrawSphere(
                    start,
                    radius * jointRadiusScale,
                    WithAlpha(color, color.a * 0.85f));
            }

            _renderer.DrawSphere(
                _scratchPoints[_scratchPoints.Count - 1],
                lastRadius,
                WithAlpha(color, color.a * 0.9f));
        }

        PreviewContext GetPreviewContext(Animator animator)
        {
            if (!HumanoidIKUtility.IsUsableHumanoid(animator)) return null;
            if (_previewContexts.TryGetValue(animator, out var context))
            {
                if (context.IsValidFor(animator)) return context;

                context.Dispose();
                _previewContexts.Remove(animator);
            }

            if (!PreviewContext.TryCreate(animator, out context)) return null;
            _previewContexts.Add(animator, context);
            return context;
        }

        bool TryResolvePreviewHandRotations(
            PreviewContext context,
            HumanoidIKTarget target,
            in HumanoidIKDigitBendPose digitBends)
        {
            if (context?.PoseSolver == null) return false;

            var resolveLeft = target == HumanoidIKTarget.LeftHand;
            var resolveRight = target == HumanoidIKTarget.RightHand;
            var emptyPose = default(HumanoidIKDigitBendPose);
            var leftPose = resolveLeft ? digitBends : emptyPose;
            var rightPose = resolveRight ? digitBends : emptyPose;
            return context.PoseSolver.TryResolveHandLocalRotations(
                resolveLeft,
                in leftPose,
                resolveLeft ? 1f : 0f,
                resolveRight,
                in rightPose,
                resolveRight ? 1f : 0f,
                _resolvedPreviewRotations);
        }

        bool TryGetPreviewBonePose(Transform bone, out HumanoidIKReferenceBonePose pose)
        {
            pose = default;
            return _activePreviewContext?.ReferencePose != null &&
                   _activePreviewContext.ReferencePose.TryGetBonePose(bone, out pose);
        }

        bool TryGetReferenceRelativePosition(Transform ancestor, Transform descendant, out Vector3 position)
        {
            position = Vector3.zero;
            if (!TryGetReferenceRelativeMatrix(ancestor, descendant, out var matrix)) return false;

            position = matrix.MultiplyPoint3x4(Vector3.zero);
            return true;
        }

        bool TryGetReferenceRelativeMatrix(
            Transform ancestor,
            Transform descendant,
            out Matrix4x4 matrix)
        {
            matrix = Matrix4x4.identity;
            return _activePreviewContext?.ReferencePose != null &&
                   _activePreviewContext.ReferencePose.TryGetRelativeMatrix(
                       ancestor,
                       descendant,
                       out matrix);
        }

        Vector3 GetPreviewScale(Animator animator, Transform endBone)
        {
            if (!animator || !endBone) return Vector3.one;

            var scale = animator.transform.lossyScale;
            if (TryBuildTransformPath(animator.transform, endBone, _scratchTransformPath))
            {
                for (var i = 0; i < _scratchTransformPath.Count; i++)
                {
                    if (TryGetPreviewBonePose(_scratchTransformPath[i], out var pose))
                    {
                        scale = Vector3.Scale(scale, pose.Scale);
                    }
                }
            }

            scale.x = Mathf.Max(Mathf.Abs(scale.x), 0.0001f);
            scale.y = Mathf.Max(Mathf.Abs(scale.y), 0.0001f);
            scale.z = Mathf.Max(Mathf.Abs(scale.z), 0.0001f);
            return scale;
        }

        static bool TryBuildTransformPath(Transform ancestor, Transform descendant, List<Transform> path)
        {
            path.Clear();
            if (!ancestor || !descendant || ancestor == descendant) return false;

            var current = descendant;
            while (current && current != ancestor)
            {
                path.Add(current);
                current = current.parent;
            }

            if (current != ancestor)
            {
                path.Clear();
                return false;
            }

            path.Reverse();
            return path.Count > 0;
        }

        static int GetChainBoneIndex(Transform[] chain, Transform bone)
        {
            for (var i = 0; i < chain.Length; i++)
            {
                if (chain[i] == bone) return i;
            }

            return -1;
        }

        static Vector3 GetJointBendEuler(HumanoidIKJointBend bend, int index)
        {
            return index switch
            {
                0 => bend.proximal,
                1 => bend.intermediate,
                2 => bend.distal,
                _ => Vector3.zero
            };
        }
    }
}
