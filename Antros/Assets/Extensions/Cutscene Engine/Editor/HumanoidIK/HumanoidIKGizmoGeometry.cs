using System.Collections.Generic;
using CutsceneEngine;
using UnityEngine;

namespace CutsceneEngineEditor
{
    internal readonly struct HumanoidIKCanonicalFootSlab
    {
        public readonly Vector3 Center;
        public readonly Vector3 Size;
        public readonly bool BendsAtToeBase;

        public HumanoidIKCanonicalFootSlab(Vector3 center, Vector3 size, bool bendsAtToeBase)
        {
            Center = center;
            Size = size;
            BendsAtToeBase = bendsAtToeBase;
        }
    }

    internal readonly struct HumanoidIKCanonicalToe
    {
        public readonly Vector3 BasePosition;
        public readonly Vector3 Forward;
        public readonly float Length;
        public readonly float Radius;

        public HumanoidIKCanonicalToe(
            Vector3 basePosition,
            Vector3 forward,
            float length,
            float radius)
        {
            BasePosition = basePosition;
            Forward = forward.sqrMagnitude > 0.000001f
                ? forward.normalized
                : Vector3.forward;
            Length = length;
            Radius = radius;
        }
    }

    internal sealed class HumanoidIKCanonicalFootFit
    {
        readonly Vector3[] _toeBaseOverrides;
        readonly float[] _toeLengthOverrides;
        readonly bool[] _hasToeOverrides;
        readonly Vector3[] _toeForwardOverrides;
        readonly Vector3? _toeBasePivotOverride;

        public Vector3 Scale { get; }
        public float SoleHeight { get; }

        internal HumanoidIKCanonicalFootFit(
            Vector3 scale,
            float soleHeight = 0f,
            Vector3[] toeBaseOverrides = null,
            float[] toeLengthOverrides = null,
            bool[] hasToeOverrides = null,
            Vector3[] toeForwardOverrides = null,
            Vector3? toeBasePivotOverride = null)
        {
            Scale = new Vector3(
                Mathf.Max(Mathf.Abs(scale.x), 0.0001f),
                Mathf.Max(Mathf.Abs(scale.y), 0.0001f),
                Mathf.Max(Mathf.Abs(scale.z), 0.0001f));
            SoleHeight = soleHeight;
            _toeBaseOverrides = toeBaseOverrides;
            _toeLengthOverrides = toeLengthOverrides;
            _hasToeOverrides = hasToeOverrides;
            _toeForwardOverrides = toeForwardOverrides;
            _toeBasePivotOverride = toeBasePivotOverride;
        }

        internal HumanoidIKCanonicalFootSlab GetSlab(bool isLeftFoot, int slabIndex)
        {
            var canonical = HumanoidIKGizmoGeometry.GetCanonicalFootSlab(isLeftFoot, slabIndex);
            var size = Vector3.Scale(canonical.Size, Scale);
            var center = Vector3.Scale(canonical.Center, Scale);
            var canonicalBottom = center.y - size.y * 0.5f;
            center.y += SoleHeight - canonicalBottom;
            return new HumanoidIKCanonicalFootSlab(
                center,
                size,
                canonical.BendsAtToeBase);
        }

        internal HumanoidIKCanonicalToe GetToe(bool isLeftFoot, int toeIndex)
        {
            var canonical = HumanoidIKGizmoGeometry.GetCanonicalToe(isLeftFoot, toeIndex);
            var basePosition = Vector3.Scale(canonical.BasePosition, Scale);
            var defaultLength = canonical.Length * Scale.z;
            var length = defaultLength;
            var forward = canonical.Forward;
            var hasOverride = _hasToeOverrides != null &&
                              toeIndex >= 0 &&
                              toeIndex < _hasToeOverrides.Length &&
                              _hasToeOverrides[toeIndex];
            if (hasOverride)
            {
                if (_toeBaseOverrides != null && toeIndex < _toeBaseOverrides.Length)
                {
                    basePosition = _toeBaseOverrides[toeIndex];
                }

                if (_toeLengthOverrides != null &&
                    toeIndex < _toeLengthOverrides.Length &&
                    _toeLengthOverrides[toeIndex] > 0.0001f)
                {
                    length = _toeLengthOverrides[toeIndex];
                }

                if (_toeForwardOverrides != null &&
                    toeIndex < _toeForwardOverrides.Length &&
                    _toeForwardOverrides[toeIndex].sqrMagnitude > 0.000001f)
                {
                    forward = _toeForwardOverrides[toeIndex].normalized;
                }
            }

            // Bound toe length follows the reference chain, but thickness stays
            // tied to the fitted canonical digit so shortening cannot collapse
            // the primitive. The small bound-only scale avoids the former
            // over-weighted appearance without changing the unbound silhouette.
            var radiusScale = hasOverride
                ? HumanoidIKGizmoGeometry.FittedToeRadiusScale
                : 1f;
            var radius = HumanoidIKGizmoGeometry.GetDigitPrimitiveRadius(
                defaultLength,
                HumanoidIKGizmoGeometry.GetCanonicalToeDiameterToLength(toeIndex) *
                radiusScale,
                defaultLength);
            // An articulated override is the immutable reference-pose bone
            // pivot, so the first rendered sphere must stay centered there.
            // Canonical fallbacks still rest one radius above the fitted sole.
            if (!hasOverride)
            {
                basePosition.y = Mathf.Max(basePosition.y, SoleHeight + radius);
            }

            return new HumanoidIKCanonicalToe(basePosition, forward, length, radius);
        }
        internal Vector3 GetToeBasePivot(bool isLeftFoot)
        {
            if (_toeBasePivotOverride.HasValue)
            {
                return _toeBasePivotOverride.Value;
            }

            var pivot = Vector3.Scale(
                HumanoidIKGizmoGeometry.GetCanonicalToeBasePivot(isLeftFoot),
                Scale);
            pivot.y += SoleHeight -
                       HumanoidIKGizmoGeometry.CanonicalFootSoleBottom * Scale.y;
            return pivot;
        }

        internal HumanoidIKCanonicalFootSlab GetToeBridgeSlab(bool isLeftFoot)
        {
            var forefoot = GetSlab(
                isLeftFoot,
                HumanoidIKGizmoGeometry.CanonicalFootSlabCount - 1);
            var toeBasePivot = GetToeBasePivot(isLeftFoot);
            var useOnlyOverrides = false;
            if (_hasToeOverrides != null)
            {
                for (var toeIndex = 0;
                     toeIndex < Mathf.Min(
                         _hasToeOverrides.Length,
                         HumanoidIKGizmoGeometry.CanonicalToeCount);
                     toeIndex++)
                {
                    if (!_hasToeOverrides[toeIndex]) continue;
                    useOnlyOverrides = true;
                    break;
                }
            }

            var rootZSum = 0f;
            var rootCount = 0;
            for (var toeIndex = 0;
                 toeIndex < HumanoidIKGizmoGeometry.CanonicalToeCount;
                 toeIndex++)
            {
                var hasOverride = _hasToeOverrides != null &&
                                  toeIndex < _hasToeOverrides.Length &&
                                  _hasToeOverrides[toeIndex];
                if (useOnlyOverrides && !hasOverride) continue;

                var rootZ = GetToe(isLeftFoot, toeIndex).BasePosition.z;
                if (rootZ <= toeBasePivot.z + 0.0001f) continue;
                rootZSum += rootZ;
                rootCount++;
            }

            var frontZ = rootCount > 0
                ? rootZSum / rootCount
                : toeBasePivot.z +
                  HumanoidIKGizmoGeometry.GetCanonicalToeBridgeSlab(isLeftFoot).Size.z *
                  Scale.z;
            return HumanoidIKGizmoGeometry.CreateToeBridgeSlab(
                forefoot,
                toeBasePivot,
                frontZ);
        }
    }

    internal static class HumanoidIKGizmoGeometry
    {
        internal const int CanonicalFootSlabCount = 5;
        internal const int CanonicalToeCount = 5;
        // A neutral, unbound gizmo represents a 249 x 100 mm adult-foot envelope.
        // Bound previews still replace this reference length with the rig's
        // sole-projected Foot-to-Toes distance.
        internal const float CanonicalFootLength = 0.125f;
        internal const float CanonicalFootBaseWidth = 0.09f;
        internal const float CanonicalFootWidthMultiplier = 1f;
        internal const float CanonicalFootSlabLengthMultiplier = 1.2f;
        internal const float CanonicalFootWidth =
            CanonicalFootBaseWidth * CanonicalFootWidthMultiplier;
        internal const float CanonicalFootThickness = 0.055f;
        internal const float CanonicalFootSoleBottom = -CanonicalFootThickness * 0.35f;
        internal const float CanonicalFootMedialOffset = 0.006f;
        internal const float CanonicalFootLateralExtension = 0.01f;
        internal const float CanonicalToeLengthMultiplier = 1.5f;
        internal const float CanonicalToeRadiusMultiplier = 1.2f;
        internal const float CanonicalLesserToeRadiusScale = 0.6f;
        const float CanonicalToeBaseDiameterToLength = 0.3f;
        internal const float CanonicalToeDiameterToLength =
            CanonicalToeBaseDiameterToLength *
            CanonicalToeRadiusMultiplier /
            CanonicalToeLengthMultiplier;
        internal const float CanonicalToeRootSpan = 0.072f;
        // The Humanoid Toes transform is the shared Toe Base bend pivot, not a
        // visual digit root. Canonical/synthetic digit roots start in front of it.
        internal const float CanonicalToeRootForwardOffsetFromBase = 0.065f;
        internal const float CanonicalToeFirstSegmentRatio = 0.4f;
        internal const float CanonicalToeSecondSegmentRatio = 0.32f;
        internal const float CanonicalToeDisplayLengthRatio =
            CanonicalToeFirstSegmentRatio + CanonicalToeSecondSegmentRatio;
        internal const float FittedToeLengthMultiplier = 1.15f;
        internal const float FittedToeMinimumLengthScale = 0.75f;
        internal const float FittedToeMaximumLengthScale = 1f;
        internal const float FittedToeRadiusScale = 0.9f;
        // Reference toe pivots sit inside the foot. Keep the bound sole below
        // them to leave a stable skin/shoe allowance in the display-local frame.
        internal const float BoundFootSoleDropBelowToe = 0.01f;

        // Ratios are defined in the intrinsic foot frame: +Z toes, +Y dorsum,
        // and +X toward the left foot's medial side. The right foot mirrors X once.
        static readonly float[] FootSlabBoundaries = { -0.24f, 0f, 0.28f, 0.55f, 0.76f, 1f };
        static readonly float[] FootSlabWidthRatios = { 0.72f, 0.78f, 0.76f, 0.88f, 1f };
        static readonly float[] FootSlabHeightRatios = { 1.05f, 1.28f, 1.05f, 0.75f, 0.5f };
        static readonly float[] FootSlabLateralRatios = { 0f, 0f, -0.06f, -0.03f, 0f };
        static readonly float[] ToeRootOffsets = { 0.036f, 0.014f, -0.003f, -0.019f, -0.036f };
        // The neutral visual digit-root line recedes toward the outside from the
        // separate shared Toe Base pivot. Articulated rigs replace these drawing
        // positions with their exact reference-pose toe bone roots.
        static readonly float[] ToeRootForwardOffsets = { 0f, -0.001f, -0.003f, -0.005f, -0.008f };
        static readonly float[] ToeLengths = { 0.034f, 0.032f, 0.03f, 0.028f, 0.026f };
        static readonly float[] ToeSegmentRatios =
            { CanonicalToeFirstSegmentRatio, CanonicalToeSecondSegmentRatio };

        internal static Vector3 SolvePreviewMidpoint(
            Vector3 rootPosition,
            Vector3 currentMidPosition,
            Vector3 currentEndPosition,
            Vector3 targetPosition,
            Vector3 bendTargetPosition)
        {
            var upperLength = Vector3.Distance(rootPosition, currentMidPosition);
            var lowerLength = Vector3.Distance(currentMidPosition, currentEndPosition);
            var targetOffset = targetPosition - rootPosition;
            var targetDistance = targetOffset.magnitude;

            if (upperLength <= Mathf.Epsilon || lowerLength <= Mathf.Epsilon || targetDistance <= Mathf.Epsilon)
            {
                return currentMidPosition;
            }

            var minDistance = Mathf.Abs(upperLength - lowerLength) + 0.0001f;
            var maxDistance = upperLength + lowerLength - 0.0001f;
            targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);

            var targetDirection = targetOffset.normalized;
            var bendDirection = Vector3.ProjectOnPlane(bendTargetPosition - rootPosition, targetDirection);
            if (bendDirection.sqrMagnitude <= 0.000001f)
            {
                bendDirection = Vector3.ProjectOnPlane(currentMidPosition - rootPosition, targetDirection);
            }
            if (bendDirection.sqrMagnitude <= 0.000001f)
            {
                bendDirection = Vector3.Cross(targetDirection, Vector3.up);
            }

            bendDirection.Normalize();
            var adjacent = (upperLength * upperLength + targetDistance * targetDistance - lowerLength * lowerLength) /
                           (2f * targetDistance);
            var height = Mathf.Sqrt(Mathf.Max(0f, upperLength * upperLength - adjacent * adjacent));
            return rootPosition + targetDirection * adjacent + bendDirection * height;
        }

        internal static Quaternion GetBoneBoxRotation(Vector3 direction, Vector3 preferredUp)
        {
            if (direction.sqrMagnitude <= 0.000001f) return Quaternion.identity;

            direction.Normalize();
            var up = Vector3.ProjectOnPlane(preferredUp, direction);
            if (up.sqrMagnitude <= 0.000001f)
            {
                up = Vector3.ProjectOnPlane(Vector3.up, direction);
            }
            if (up.sqrMagnitude <= 0.000001f)
            {
                up = Vector3.Cross(direction, Vector3.right);
            }

            return Quaternion.LookRotation(direction, up.normalized);
        }

        internal static HumanoidIKCanonicalFootSlab GetCanonicalFootSlab(bool isLeftFoot, int slabIndex)
        {
            slabIndex = Mathf.Clamp(slabIndex, 0, CanonicalFootSlabCount - 1);
            var mirrorSign = isLeftFoot ? 1f : -1f;
            var start = GetExpandedFootSlabZ(FootSlabBoundaries[slabIndex]);
            var end = GetExpandedFootSlabZ(FootSlabBoundaries[slabIndex + 1]);
            // Preserve the medial edge while adding mesh coverage only toward
            // the little-toe (lateral) side. Bound fits scale this canonical
            // 10 mm allowance with the rest of the foot width.
            var width = FootSlabWidthRatios[slabIndex] * CanonicalFootWidth +
                        CanonicalFootLateralExtension;
            var height = FootSlabHeightRatios[slabIndex] * CanonicalFootThickness;
            var commonBottom = CanonicalFootSoleBottom;
            var lateralCenter = mirrorSign *
                                (CanonicalFootMedialOffset +
                                 FootSlabLateralRatios[slabIndex] * CanonicalFootWidth -
                                 CanonicalFootLateralExtension * 0.5f);

            return new HumanoidIKCanonicalFootSlab(
                new Vector3(lateralCenter, commonBottom + height * 0.5f, (start + end) * 0.5f),
                new Vector3(width, height, end - start),
                slabIndex == CanonicalFootSlabCount - 1);
        }

        static float GetExpandedFootSlabZ(float boundaryRatio)
        {
            var boneRelativeZ = boundaryRatio * CanonicalFootLength;

            // The skinned sole extends beyond the Foot-to-Toes bone span. Expand
            // every connected slab around the fixed Toe Base so bound toe roots
            // remain aligned while the extra coverage grows toward the heel.
            return CanonicalFootLength +
                   (boneRelativeZ - CanonicalFootLength) *
                   CanonicalFootSlabLengthMultiplier;
        }

        internal static HumanoidIKCanonicalToe GetCanonicalToe(bool isLeftFoot, int toeIndex)
        {
            toeIndex = Mathf.Clamp(toeIndex, 0, CanonicalToeCount - 1);
            var mirrorSign = isLeftFoot ? 1f : -1f;
            var length = ToeLengths[toeIndex] * CanonicalToeLengthMultiplier;
            var radius = GetDigitPrimitiveRadius(
                length,
                GetCanonicalToeDiameterToLength(toeIndex),
                length);
            return new HumanoidIKCanonicalToe(
                new Vector3(
                    ToeRootOffsets[toeIndex] * mirrorSign,
                    CanonicalFootSoleBottom + radius,
                    CanonicalFootLength +
                    CanonicalToeRootForwardOffsetFromBase +
                    ToeRootForwardOffsets[toeIndex]),
                Vector3.forward,
                length,
                radius);
        }

        internal static HumanoidIKCanonicalFootSlab GetCanonicalToeBridgeSlab(bool isLeftFoot)
        {
            var forefoot = GetCanonicalFootSlab(
                isLeftFoot,
                CanonicalFootSlabCount - 1);
            var toeBasePivot = GetCanonicalToeBasePivot(isLeftFoot);
            var rootZSum = 0f;
            for (var toeIndex = 0; toeIndex < CanonicalToeCount; toeIndex++)
            {
                rootZSum += GetCanonicalToe(isLeftFoot, toeIndex).BasePosition.z;
            }

            return CreateToeBridgeSlab(
                forefoot,
                toeBasePivot,
                rootZSum / CanonicalToeCount);
        }

        internal static HumanoidIKCanonicalFootSlab CreateToeBridgeSlab(
            in HumanoidIKCanonicalFootSlab forefoot,
            Vector3 toeBasePivot,
            float frontZ)
        {
            frontZ = Mathf.Max(frontZ, toeBasePivot.z + 0.0001f);
            return new HumanoidIKCanonicalFootSlab(
                new Vector3(
                    forefoot.Center.x,
                    forefoot.Center.y,
                    (toeBasePivot.z + frontZ) * 0.5f),
                new Vector3(
                    forefoot.Size.x,
                    forefoot.Size.y,
                    frontZ - toeBasePivot.z),
                true);
        }

        internal static Quaternion GetCanonicalToeBaseRotation(
            HumanoidIKToeRigKind rigKind,
            float toeBaseBend)
        {
            return rigKind == HumanoidIKToeRigKind.ArticulatedToes
                ? Quaternion.Euler(-HumanoidIKUtility.GetToeBaseBendAngle(toeBaseBend), 0f, 0f)
                : Quaternion.identity;
        }

        internal static Quaternion GetCanonicalToeGroupRotation(
            bool isLeftFoot,
            HumanoidIKToeRigKind rigKind,
            in HumanoidIKDigitBendPose digitBends,
            float toeBaseBend)
        {
            if (rigKind == HumanoidIKToeRigKind.ArticulatedToes)
            {
                return GetCanonicalToeBaseRotation(rigKind, toeBaseBend);
            }

            if (rigKind != HumanoidIKToeRigKind.ToeFoot)
            {
                return Quaternion.identity;
            }

            var bend = HumanoidIKUtility.ClampToeFootBend(
                HumanoidIKUtility.GetDigitBend(in digitBends, 0));
            return HumanoidIKUtility.TryGetToeAnatomicalRotation(
                bend.proximal,
                Vector3.forward,
                Vector3.up,
                isLeftFoot,
                out var rotation)
                ? rotation
                : Quaternion.identity;
        }

        internal static void GetCanonicalFootSlabPose(
            bool isLeftFoot,
            int slabIndex,
            HumanoidIKToeRigKind rigKind,
            float toeBaseBend,
            out HumanoidIKCanonicalFootSlab slab,
            out Vector3 localCenter,
            out Quaternion localRotation)
        {
            slab = GetCanonicalFootSlab(isLeftFoot, slabIndex);
            localCenter = slab.Center;
            localRotation = Quaternion.identity;
            if (!slab.BendsAtToeBase) return;

            localRotation = GetCanonicalToeBaseRotation(rigKind, toeBaseBend);
            localCenter = RotatePointAroundPivot(
                localCenter,
                GetCanonicalToeBasePivot(isLeftFoot),
                localRotation);
        }

        internal static void GetFittedFootSlabPose(
            bool isLeftFoot,
            int slabIndex,
            HumanoidIKCanonicalFootFit fit,
            HumanoidIKToeRigKind rigKind,
            float toeBaseBend,
            out HumanoidIKCanonicalFootSlab slab,
            out Vector3 localCenter,
            out Quaternion localRotation)
        {
            if (fit == null)
            {
                GetCanonicalFootSlabPose(
                    isLeftFoot,
                    slabIndex,
                    rigKind,
                    toeBaseBend,
                    out slab,
                    out localCenter,
                    out localRotation);
                return;
            }

            slab = fit.GetSlab(isLeftFoot, slabIndex);
            localCenter = slab.Center;
            localRotation = Quaternion.identity;
            if (!slab.BendsAtToeBase) return;

            localRotation = GetCanonicalToeBaseRotation(rigKind, toeBaseBend);
            localCenter = RotatePointAroundPivot(
                localCenter,
                fit.GetToeBasePivot(isLeftFoot),
                localRotation);
        }

        internal static void GetFittedToeBridgeSlabPose(
            bool isLeftFoot,
            HumanoidIKCanonicalFootFit fit,
            HumanoidIKToeRigKind rigKind,
            in HumanoidIKDigitBendPose digitBends,
            float toeBaseBend,
            out HumanoidIKCanonicalFootSlab slab,
            out Vector3 localCenter,
            out Quaternion localRotation)
        {
            slab = fit?.GetToeBridgeSlab(isLeftFoot) ??
                   GetCanonicalToeBridgeSlab(isLeftFoot);
            var toeBasePivot = fit?.GetToeBasePivot(isLeftFoot) ??
                               GetCanonicalToeBasePivot(isLeftFoot);
            localRotation = GetCanonicalToeGroupRotation(
                isLeftFoot,
                rigKind,
                in digitBends,
                toeBaseBend);
            localCenter = RotatePointAroundPivot(
                slab.Center,
                toeBasePivot,
                localRotation);
        }

        internal static HumanoidIKJointBend GetCanonicalToeSegmentBend(
            HumanoidIKToeRigKind rigKind,
            in HumanoidIKDigitBendPose digitBends,
            int toeIndex,
            float toeFan)
        {
            switch (rigKind)
            {
                case HumanoidIKToeRigKind.ArticulatedToes:
                    return HumanoidIKUtility.ClampToeBend(
                        HumanoidIKUtility.GetDigitBend(in digitBends, toeIndex) +
                        HumanoidIKUtility.GetArticulatedToeFanOffset(toeIndex, toeFan));
                default:
                    return default;
            }
        }

        internal static void BuildCanonicalToePoints(
            bool isLeftFoot,
            int toeIndex,
            HumanoidIKToeRigKind rigKind,
            in HumanoidIKDigitBendPose digitBends,
            float toeBaseBend,
            float toeFan,
            List<Vector3> points)
        {
            BuildCanonicalToePoints(
                isLeftFoot,
                toeIndex,
                GetCanonicalToe(isLeftFoot, toeIndex),
                GetCanonicalToeBasePivot(isLeftFoot),
                rigKind,
                in digitBends,
                toeBaseBend,
                toeFan,
                points);
        }

        internal static void BuildCanonicalToePoints(
            bool isLeftFoot,
            int toeIndex,
            in HumanoidIKCanonicalToe toe,
            Vector3 toeBasePivot,
            HumanoidIKToeRigKind rigKind,
            in HumanoidIKDigitBendPose digitBends,
            float toeBaseBend,
            float toeFan,
            List<Vector3> points)
        {
            points.Clear();
            var toeBaseRotation = GetCanonicalToeGroupRotation(
                isLeftFoot,
                rigKind,
                in digitBends,
                toeBaseBend);
            var currentPoint = RotatePointAroundPivot(
                toe.BasePosition,
                toeBasePivot,
                toeBaseRotation);
            var currentForward = toeBaseRotation * toe.Forward;
            var currentUp = toeBaseRotation * Vector3.up;
            var bend = GetCanonicalToeSegmentBend(
                rigKind,
                in digitBends,
                toeIndex,
                toeFan);

            points.Add(currentPoint);
            for (var jointIndex = 0; jointIndex < ToeSegmentRatios.Length; jointIndex++)
            {
                var bendEuler = GetJointBendEuler(in bend, jointIndex);
                if (HumanoidIKUtility.TryGetToeAnatomicalRotation(
                        bendEuler,
                        currentForward,
                        currentUp,
                        isLeftFoot,
                        out var anatomicalRotation))
                {
                    currentForward = anatomicalRotation * currentForward;
                    currentUp = anatomicalRotation * currentUp;
                }

                currentPoint += currentForward *
                                (toe.Length * ToeSegmentRatios[jointIndex]);
                points.Add(currentPoint);
            }
        }

        internal static Vector3 GetCanonicalFootFitScale(
            float footToToeDistance,
            float toeRootSpan,
            float footToSoleDrop = 0f)
        {
            var lengthScale = footToToeDistance > 0.0001f
                ? footToToeDistance / CanonicalFootLength
                : 1f;
            var widthScale = toeRootSpan > 0.0001f
                ? toeRootSpan / CanonicalToeRootSpan
                : lengthScale;

            // Toe roots describe the useful forefoot width, but malformed rigs can
            // place helpers arbitrarily far from the foot. Keep the fitted silhouette
            // recognizable while still allowing normal anatomical aspect differences.
            widthScale = Mathf.Clamp(widthScale, lengthScale * 0.65f, lengthScale * 1.75f);
            var heightScale = Mathf.Sqrt(lengthScale * widthScale);
            if (footToSoleDrop > 0.0001f)
            {
                var tallestSlabHeight = CanonicalFootThickness * 1.28f;
                heightScale = Mathf.Max(heightScale, footToSoleDrop / tallestSlabHeight);
            }
            return new Vector3(widthScale, heightScale, lengthScale);
        }

        internal static float GetBoundFootSoleHeight(float toeHeight)
        {
            return toeHeight - BoundFootSoleDropBelowToe;
        }

        internal static Vector3 GetFittedToeGroupBase(
            bool isLeftFoot,
            int toeIndex,
            Vector3 scale,
            Vector3 mappedToePivot)
        {
            var canonicalBase = Vector3.Scale(
                GetCanonicalToe(isLeftFoot, toeIndex).BasePosition,
                scale);
            var canonicalPivot = Vector3.Scale(
                GetCanonicalToeBasePivot(isLeftFoot),
                scale);
            return new Vector3(
                mappedToePivot.x + canonicalBase.x - canonicalPivot.x,
                mappedToePivot.y,
                mappedToePivot.z + canonicalBase.z - canonicalPivot.z);
        }

        internal static Vector3 GetCanonicalToeBasePivot(bool isLeftFoot)
        {
            var mirrorSign = isLeftFoot ? 1f : -1f;
            // This is the collective bend pivot represented by Humanoid LeftToes
            // or RightToes. It deliberately remains behind every visual digit root.
            return new Vector3(
                CanonicalFootMedialOffset * mirrorSign,
                0f,
                CanonicalFootLength);
        }

        internal static Vector3 RotatePointAroundPivot(
            Vector3 point,
            Vector3 pivot,
            Quaternion rotationDelta)
        {
            return pivot + rotationDelta * (point - pivot);
        }

        internal const int HandDigitCount = 5;
        internal const int HandPalmFirstDigitIndex = 1;
        internal const int HandPalmLastDigitIndex = HandDigitCount - 1;
        internal const float DefaultHandDigitDiameterToFirstSegment = 0.30f;
        internal const float HandDigitRadiusToTotalLength = 0.14f;
        internal const float HandDigitMaximumRadiusToProximalSpacing = 0.40f;
        internal const float HandDigitTipLengthToPreviousSegment = 0.55f;
        internal const float HandDigitJointRadiusScale = 1.02f;

        internal static bool TryGetHandPalmMeasurements(
            Vector3 wristPosition,
            Quaternion displayRotation,
            IReadOnlyList<Vector3> digitBasePositions,
            IReadOnlyList<bool> hasDigitBasePosition,
            IReadOnlyList<float> digitBaseRadii,
            out Vector3 palmCenter,
            out float width,
            out float length,
            out float thickness)
        {
            palmCenter = Vector3.zero;
            width = 0f;
            length = 0f;
            thickness = 0f;
            if (digitBasePositions == null ||
                hasDigitBasePosition == null ||
                digitBaseRadii == null)
            {
                return false;
            }

            var availableDigitCount = Mathf.Min(
                HandDigitCount,
                Mathf.Min(
                    digitBasePositions.Count,
                    Mathf.Min(hasDigitBasePosition.Count, digitBaseRadii.Count)));
            if (availableDigitCount <= HandPalmLastDigitIndex ||
                !hasDigitBasePosition[0] ||
                !hasDigitBasePosition[HandPalmFirstDigitIndex] ||
                !hasDigitBasePosition[HandPalmLastDigitIndex])
            {
                return false;
            }

            var worldToDisplay = Quaternion.Inverse(displayRotation);
            var allDigitMinimumRight = float.PositiveInfinity;
            var allDigitMaximumRight = float.NegativeInfinity;
            for (var digitIndex = 0;
                 digitIndex < availableDigitCount;
                 digitIndex++)
            {
                if (!hasDigitBasePosition[digitIndex]) continue;

                var displayLocal = worldToDisplay *
                                   (digitBasePositions[digitIndex] - wristPosition);
                allDigitMinimumRight = Mathf.Min(allDigitMinimumRight, displayLocal.x);
                allDigitMaximumRight = Mathf.Max(allDigitMaximumRight, displayLocal.x);

                var proximalRadius = GetHandDigitProximalRadius(
                    digitBaseRadii[digitIndex],
                    digitIndex == 0);
                thickness = Mathf.Max(thickness, proximalRadius * 2f);
            }

            if (float.IsPositiveInfinity(allDigitMinimumRight) ||
                float.IsNegativeInfinity(allDigitMaximumRight))
            {
                return false;
            }

            var palmCenterRight =
                (allDigitMinimumRight + allDigitMaximumRight) * 0.5f;
            var indexBase = worldToDisplay *
                            (digitBasePositions[HandPalmFirstDigitIndex] - wristPosition);
            var littleBase = worldToDisplay *
                             (digitBasePositions[HandPalmLastDigitIndex] - wristPosition);
            var indexRadius = GetHandDigitProximalRadius(
                digitBaseRadii[HandPalmFirstDigitIndex],
                false);
            var littleRadius = GetHandDigitProximalRadius(
                digitBaseRadii[HandPalmLastDigitIndex],
                false);
            var requiredHalfWidth = Mathf.Max(
                Mathf.Abs(indexBase.x - palmCenterRight) + indexRadius,
                Mathf.Abs(littleBase.x - palmCenterRight) + littleRadius);
            width = requiredHalfWidth * 2f;

            var thumbBase = worldToDisplay * (digitBasePositions[0] - wristPosition);
            var thumbRadius = GetHandDigitProximalRadius(digitBaseRadii[0], true);
            var nearestFingerBase = Vector3.zero;
            var nearestFingerDistance = float.PositiveInfinity;
            for (var digitIndex = HandPalmFirstDigitIndex;
                 digitIndex < availableDigitCount;
                 digitIndex++)
            {
                if (!hasDigitBasePosition[digitIndex]) continue;

                var candidateBase = worldToDisplay *
                                    (digitBasePositions[digitIndex] - wristPosition);
                var distance = Mathf.Abs(candidateBase.z - thumbBase.z);
                if (distance >= nearestFingerDistance) continue;

                nearestFingerDistance = distance;
                nearestFingerBase = candidateBase;
            }

            if (float.IsPositiveInfinity(nearestFingerDistance) ||
                nearestFingerDistance <= 0.0001f ||
                width <= 0.0001f)
            {
                return false;
            }

            var thumbToFingerSign = Mathf.Sign(nearestFingerBase.z - thumbBase.z);
            var palmStartForward = thumbBase.z - thumbToFingerSign * thumbRadius;
            var palmCenterInDisplay = new Vector3(
                palmCenterRight,
                (thumbBase.y + nearestFingerBase.y) * 0.5f,
                (palmStartForward + nearestFingerBase.z) * 0.5f);
            palmCenter = wristPosition + displayRotation * palmCenterInDisplay;
            length = nearestFingerDistance + thumbRadius;
            return true;
        }

        internal static Vector3 GetHandPalmBoxSize(
            float measuredWidth,
            float measuredLength,
            float measuredThickness)
        {
            return new Vector3(
                Mathf.Max(measuredWidth, 0f),
                Mathf.Max(measuredThickness, 0.003f),
                Mathf.Max(measuredLength, 0f));
        }

        internal static float GetHandWristSphereRadius(float measuredPalmWidth)
        {
            return Mathf.Max(measuredPalmWidth, 0f) * 0.5f;
        }

        internal static float GetNearestHandDigitProximalSpacing(
            int digitIndex,
            IReadOnlyList<Vector3> digitBasePositions,
            IReadOnlyList<bool> hasDigitBasePosition,
            Quaternion displayRotation)
        {
            if (digitBasePositions == null || hasDigitBasePosition == null) return 0f;

            var availableDigitCount = Mathf.Min(
                HandDigitCount,
                Mathf.Min(digitBasePositions.Count, hasDigitBasePosition.Count));
            if (digitIndex < 0 ||
                digitIndex >= availableDigitCount ||
                !hasDigitBasePosition[digitIndex])
            {
                return 0f;
            }

            var nearestSpacing = float.PositiveInfinity;
            for (var otherDigitIndex = 0;
                 otherDigitIndex < availableDigitCount;
                 otherDigitIndex++)
            {
                if (otherDigitIndex == digitIndex || !hasDigitBasePosition[otherDigitIndex]) continue;

                nearestSpacing = Mathf.Min(
                    nearestSpacing,
                    GetDistanceInDisplayPlane(
                        digitBasePositions[digitIndex],
                        digitBasePositions[otherDigitIndex],
                        displayRotation));
            }

            return float.IsPositiveInfinity(nearestSpacing) ? 0f : nearestSpacing;
        }

        internal static float GetHandDigitBaseRadius(
            float totalLength,
            float nearestProximalSpacing,
            bool isThumb)
        {
            var preferredRadius =
                Mathf.Max(totalLength, 0f) * HandDigitRadiusToTotalLength;
            if (nearestProximalSpacing <= 0.0001f) return preferredRadius;

            var maximumRadiusScale = GetHandDigitMaximumRadiusScale(isThumb);
            if (maximumRadiusScale <= 0f) return 0f;

            return Mathf.Min(
                preferredRadius,
                nearestProximalSpacing * HandDigitMaximumRadiusToProximalSpacing /
                maximumRadiusScale);
        }

        internal static float GetHandDigitProximalRadius(float baseRadius, bool isThumb)
        {
            return Mathf.Max(baseRadius, 0f) * GetHandDigitRadiusScale(0, isThumb);
        }

        internal static float GetPolylineLength(IReadOnlyList<Vector3> points)
        {
            if (points == null) return 0f;

            var length = 0f;
            for (var pointIndex = 0; pointIndex < points.Count - 1; pointIndex++)
            {
                length += Vector3.Distance(points[pointIndex], points[pointIndex + 1]);
            }

            return length;
        }

        internal static float GetDistanceInDisplayPlane(
            Vector3 firstPoint,
            Vector3 secondPoint,
            Quaternion displayRotation)
        {
            var worldToDisplay = Quaternion.Inverse(displayRotation);
            var displayOffset = worldToDisplay * (secondPoint - firstPoint);
            return new Vector2(displayOffset.x, displayOffset.z).magnitude;
        }

        internal static float GetHandDigitMaximumRadiusScale(bool isThumb)
        {
            return GetHandDigitRadiusScale(0, isThumb) * HandDigitJointRadiusScale;
        }

        internal static Vector3 GetSyntheticHandDigitTipPoint(
            Vector3 previousPoint,
            Vector3 distalPoint,
            Quaternion referenceDistalFrameRotation,
            Quaternion posedDistalFrameRotation)
        {
            // HumanBodyBones.Distal is the final driven joint. Imported children
            // can be arbitrary end markers or rig helpers, so the render-only tip
            // always continues from the mapped intermediate-to-distal segment.
            var previousSegment = distalPoint - previousPoint;
            if (previousSegment.sqrMagnitude <= 0.000001f) return distalPoint;

            var distalLocalContinuation =
                Quaternion.Inverse(referenceDistalFrameRotation) *
                (previousSegment * HandDigitTipLengthToPreviousSegment);
            return distalPoint + posedDistalFrameRotation * distalLocalContinuation;
        }

        internal static float GetHandDigitRadiusScale(int segmentIndex, bool isThumb)
        {
            return Mathf.Clamp(segmentIndex, 0, 2) switch
            {
                0 => isThumb ? 1.12f : 1f,
                1 => 0.96f,
                _ => 0.92f
            };
        }

        internal static float GetDigitPrimitiveRadius(
            float segmentLength,
            float diameterToLength,
            float fixedDiameterReferenceLength = -1f,
            float minimumRadius = 0.0015f)
        {
            var diameterReferenceLength = fixedDiameterReferenceLength >= 0f
                ? fixedDiameterReferenceLength
                : segmentLength;
            return Mathf.Max(
                diameterReferenceLength * diameterToLength * 0.5f,
                Mathf.Max(minimumRadius, 0f));
        }

        internal static float GetCanonicalToeDiameterToLength(int toeIndex)
        {
            toeIndex = Mathf.Clamp(toeIndex, 0, CanonicalToeCount - 1);
            return CanonicalToeDiameterToLength *
                   (toeIndex == 0 ? 1f : CanonicalLesserToeRadiusScale);
        }

        internal static float GetFittedToeLength(float measuredLength, float defaultLength)
        {
            if (defaultLength <= 0f) return 0f;
            if (measuredLength <= 0.0001f) return defaultLength;

            return Mathf.Clamp(
                measuredLength * FittedToeLengthMultiplier,
                defaultLength * FittedToeMinimumLengthScale,
                defaultLength * FittedToeMaximumLengthScale);
        }

        static Vector3 GetJointBendEuler(in HumanoidIKJointBend bend, int jointIndex)
        {
            return jointIndex switch
            {
                0 => bend.proximal,
                // The toe gizmo deliberately has one fewer display joint than
                // a finger. Merge the remaining authored bends so the distal
                // channel still affects the compact two-segment preview.
                1 => bend.intermediate + bend.distal,
                _ => Vector3.zero
            };
        }
    }
}
