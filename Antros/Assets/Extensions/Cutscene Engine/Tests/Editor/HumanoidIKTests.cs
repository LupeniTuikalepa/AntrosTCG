using System.Collections.Generic;
using CutsceneEngine;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace CutsceneEngineEditor.Tests
{
    public class HumanoidIKTests
    {
        readonly List<Object> _objectsToDestroy = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            for (var i = _objectsToDestroy.Count - 1; i >= 0; i--)
            {
                if (_objectsToDestroy[i]) Object.DestroyImmediate(_objectsToDestroy[i]);
            }
            _objectsToDestroy.Clear();
        }

        [Test]
        public void EffectorConversion_RoundTripsLegacyBoneRotation()
        {
            var boneRotation = Quaternion.Euler(18f, -37f, 9f);
            var correction = Quaternion.Euler(-4f, 82f, 16f);

            var effectorRotation = HumanoidIKUtility.ToEffectorRotation(
                boneRotation,
                HumanoidIKRotationSpace.LegacyBoneRotation,
                correction);
            var resolvedBoneRotation = HumanoidIKUtility.ToBoneRotation(effectorRotation, correction);

            Assert.That(Mathf.Abs(Quaternion.Dot(boneRotation, resolvedBoneRotation)), Is.GreaterThan(0.99999f));
        }

        [Test]
        public void DirectorLocalMigration_PreservesImplicitLegacyWorldPose()
        {
            var directorObject = Track(new GameObject("Director"));
            directorObject.transform.SetPositionAndRotation(
                new Vector3(3.5f, -1.25f, 8f),
                Quaternion.Euler(13f, 47f, -9f));
            directorObject.transform.localScale = new Vector3(1.25f, 0.8f, 1.6f);
            var clip = Track(ScriptableObject.CreateInstance<HumanoidIKClip>());
            clip.position = new Vector3(-2f, 4.5f, 7.25f);
            clip.rotation = new Vector3(31f, -72f, 18f);
            clip.bendTarget = new Vector3(1.5f, 2.25f, -0.75f);

            var legacyAnchor = clip.ResolveAnchor(null, directorObject.transform);
            HumanoidIKUtility.ResolveWorldPose(
                legacyAnchor,
                clip.position,
                clip.rotation,
                clip.bendTarget,
                out var expectedPosition,
                out var expectedRotation,
                out var expectedBendTarget);

            clip.ResolveEffectiveSpace(
                null,
                directorObject.transform,
                out var effectiveAnchor,
                out var effectivePositionFollowsAnchor,
                out var effectivePosition,
                out var effectiveRotation,
                out var effectiveBendTarget);
            HumanoidIKUtility.ResolveWorldPose(
                effectiveAnchor,
                effectivePosition,
                effectiveRotation,
                effectiveBendTarget,
                out var effectiveWorldPosition,
                out var effectiveWorldRotation,
                out var effectiveWorldBendTarget);

            Assert.That(legacyAnchor, Is.Null);
            Assert.That(effectiveAnchor, Is.SameAs(directorObject.transform));
            Assert.That(effectivePositionFollowsAnchor, Is.False);
            Assert.That(clip.UsesDirectorTransformAsDefaultAnchor, Is.False);
            Assert.That(Vector3.Distance(effectiveWorldPosition, expectedPosition), Is.LessThan(0.00001f));
            Assert.That(Vector3.Distance(effectiveWorldBendTarget, expectedBendTarget), Is.LessThan(0.00001f));
            Assert.That(
                Mathf.Abs(Quaternion.Dot(effectiveWorldRotation, expectedRotation)),
                Is.GreaterThan(0.99999f));

            var directorTranslation = new Vector3(2f, -3f, 1.5f);
            directorObject.transform.position += directorTranslation;
            HumanoidIKUtility.ResolveWorldPose(
                effectiveAnchor,
                effectivePosition,
                effectiveRotation,
                effectiveBendTarget,
                out var movedWorldPosition,
                out _,
                out _);
            Assert.That(
                Vector3.Distance(movedWorldPosition, expectedPosition + directorTranslation),
                Is.LessThan(0.00001f));
            directorObject.transform.position -= directorTranslation;

            Assert.That(
                clip.EnsureDirectorLocalDefaultAnchor(directorObject.transform),
                Is.True);
            Assert.That(clip.UsesDirectorTransformAsDefaultAnchor, Is.True);

            var migratedAnchor = clip.ResolveAnchor(null, directorObject.transform);
            HumanoidIKUtility.ResolveWorldPose(
                migratedAnchor,
                clip.position,
                clip.rotation,
                clip.bendTarget,
                out var actualPosition,
                out var actualRotation,
                out var actualBendTarget);

            Assert.That(migratedAnchor, Is.SameAs(directorObject.transform));
            Assert.That(Vector3.Distance(actualPosition, expectedPosition), Is.LessThan(0.00001f));
            Assert.That(Vector3.Distance(actualBendTarget, expectedBendTarget), Is.LessThan(0.00001f));
            Assert.That(
                Mathf.Abs(Quaternion.Dot(actualRotation, expectedRotation)),
                Is.GreaterThan(0.99999f));
            Assert.That(
                clip.EnsureDirectorLocalDefaultAnchor(directorObject.transform),
                Is.False);
        }

        [Test]
        public void ExplicitAnchorPosition_FollowsAnchorWorldPositionWithoutOffset()
        {
            var directorObject = Track(new GameObject("Director"));
            directorObject.transform.position = new Vector3(-8f, 2f, 5f);
            var anchorObject = Track(new GameObject("Explicit Anchor"));
            anchorObject.transform.SetPositionAndRotation(
                new Vector3(4.5f, -1.25f, 7f),
                Quaternion.Euler(12f, 38f, -7f));
            anchorObject.transform.localScale = new Vector3(2f, 0.75f, 1.5f);

            var clip = Track(ScriptableObject.CreateInstance<HumanoidIKClip>());
            clip.anchorTransform = new ExposedReference<Transform>
            {
                defaultValue = anchorObject.transform
            };
            clip.position = new Vector3(10f, 20f, 30f);
            clip.rotation = new Vector3(15f, -25f, 35f);
            clip.bendTarget = new Vector3(0.5f, 1.5f, -2f);

            clip.ResolveEffectiveSpace(
                null,
                directorObject.transform,
                out var effectiveAnchor,
                out var positionFollowsAnchor,
                out var effectivePosition,
                out var effectiveRotation,
                out var effectiveBendTarget);
            HumanoidIKUtility.ResolveWorldPose(
                effectiveAnchor,
                effectivePosition,
                effectiveRotation,
                effectiveBendTarget,
                out var worldPosition,
                out var worldRotation,
                out var worldBendTarget);

            Assert.That(effectiveAnchor, Is.SameAs(anchorObject.transform));
            Assert.That(positionFollowsAnchor, Is.True);
            Assert.That(effectivePosition, Is.EqualTo(Vector3.zero));
            Assert.That(Vector3.Distance(worldPosition, anchorObject.transform.position), Is.LessThan(0.00001f));
            Assert.That(
                Mathf.Abs(Quaternion.Dot(
                    worldRotation,
                    anchorObject.transform.rotation * Quaternion.Euler(clip.rotation))),
                Is.GreaterThan(0.99999f));
            Assert.That(
                Vector3.Distance(
                    worldBendTarget,
                    anchorObject.transform.TransformPoint(clip.bendTarget)),
                Is.LessThan(0.00001f));

            anchorObject.transform.position = new Vector3(-3f, 6f, 1.25f);
            HumanoidIKUtility.ResolveWorldPose(
                effectiveAnchor,
                effectivePosition,
                effectiveRotation,
                effectiveBendTarget,
                out var movedWorldPosition,
                out _,
                out _);
            Assert.That(
                Vector3.Distance(movedWorldPosition, anchorObject.transform.position),
                Is.LessThan(0.00001f));
        }

        [Test]
        public void ExplicitAnchorHandles_EditTransformWithoutRewritingClipPose()
        {
            var anchorObject = Track(new GameObject("Explicit Anchor"));
            anchorObject.transform.SetPositionAndRotation(
                new Vector3(1f, 2f, 3f),
                Quaternion.Euler(10f, 20f, 30f));
            var clip = Track(ScriptableObject.CreateInstance<HumanoidIKClip>());
            clip.anchorTransform = new ExposedReference<Transform>
            {
                defaultValue = anchorObject.transform
            };
            clip.position = new Vector3(4f, 5f, 6f);
            clip.rotation = new Vector3(15f, 25f, 35f);
            var expectedClipPosition = clip.position;
            var expectedClipRotation = clip.rotation;
            var nextAnchorPosition = new Vector3(-3f, 6f, 1.25f);
            var nextAnchorRotation = Quaternion.Euler(-12f, 48f, 7f);

            HumanoidIKClipInspector.SetHandleWorldPosition(
                clip,
                anchorObject.transform,
                positionFollowsAnchor: true,
                nextAnchorPosition);
            HumanoidIKClipInspector.SetHandleWorldRotation(
                clip,
                anchorObject.transform,
                positionFollowsAnchor: true,
                nextAnchorRotation);

            Assert.That(anchorObject.transform.position, Is.EqualTo(nextAnchorPosition));
            Assert.That(
                Quaternion.Angle(anchorObject.transform.rotation, nextAnchorRotation),
                Is.LessThan(0.0001f));
            Assert.That(clip.position, Is.EqualTo(expectedClipPosition));
            Assert.That(clip.rotation, Is.EqualTo(expectedClipRotation));
        }

        [Test]
        public void ImplicitDirectorPosition_RemainsDirectorRelative()
        {
            var directorObject = Track(new GameObject("Director"));
            directorObject.transform.SetPositionAndRotation(
                new Vector3(3f, -4f, 8f),
                Quaternion.Euler(0f, 55f, 0f));
            directorObject.transform.localScale = new Vector3(1.25f, 0.8f, 1.6f);
            var clip = Track(ScriptableObject.CreateInstance<HumanoidIKClip>());
            clip.InitializeHumanoidSpaces();
            clip.position = new Vector3(1f, 2f, -3f);

            clip.ResolveEffectiveSpace(
                null,
                directorObject.transform,
                out var effectiveAnchor,
                out var positionFollowsAnchor,
                out var effectivePosition,
                out _,
                out _);

            Assert.That(effectiveAnchor, Is.SameAs(directorObject.transform));
            Assert.That(positionFollowsAnchor, Is.False);
            Assert.That(effectivePosition, Is.EqualTo(clip.position));
            Assert.That(
                Vector3.Distance(
                    effectiveAnchor.TransformPoint(effectivePosition),
                    directorObject.transform.TransformPoint(clip.position)),
                Is.LessThan(0.00001f));
        }

        [Test]
        public void DirectorLocalMigration_PreservesLegacyPoleWorldPosition()
        {
            var directorObject = Track(new GameObject("Director"));
            directorObject.transform.SetPositionAndRotation(
                new Vector3(1f, 2f, 3f),
                Quaternion.Euler(-21f, 62f, 14f));
            var clip = Track(ScriptableObject.CreateInstance<HumanoidIKClip>());
            var expectedWorldPosition = new Vector3(-1.75f, 0.5f, 3.25f);
            clip.SetHumanoidPoleWorldVector(null, expectedWorldPosition);

            Assert.That(
                clip.EnsureDirectorLocalDefaultAnchor(directorObject.transform),
                Is.True);

            var migratedAnchor = clip.ResolveAnchor(null, directorObject.transform);
            HumanoidIKUtility.ResolveWorldPose(
                migratedAnchor,
                clip.position,
                clip.rotation,
                clip.bendTarget,
                out _,
                out _,
                out var actualWorldPosition);
            Assert.That(
                Vector3.Distance(actualWorldPosition, expectedWorldPosition),
                Is.LessThan(0.00001f));
        }

        [Test]
        public void BendGoal_PositionRemainsFixedRelativeToDirectorWhenLimbMoves()
        {
            var directorObject = Track(new GameObject("Director"));
            directorObject.transform.SetPositionAndRotation(
                new Vector3(5f, 0f, 10f),
                Quaternion.Euler(0f, 90f, 0f));
            var clip = Track(ScriptableObject.CreateInstance<HumanoidIKClip>());
            clip.InitializeHumanoidSpaces();
            var targetWorldBendGoal = new Vector3(8f, 2f, 12f);
            clip.SetHumanoidPoleWorldVector(directorObject.transform, targetWorldBendGoal);

            var anchor = clip.ResolveAnchor(null, directorObject.transform);
            HumanoidIKUtility.ResolveWorldPose(
                anchor,
                clip.position,
                clip.rotation,
                clip.bendTarget,
                out _,
                out _,
                out var worldBendGoal);

            Assert.That(Vector3.Distance(worldBendGoal, targetWorldBendGoal), Is.LessThan(0.00001f));

            // Simulating limb movement (upper arm at different positions)
            var upperArmPos1 = new Vector3(5f, 1.5f, 10f);
            var upperArmPos2 = new Vector3(5f, 3.0f, 15f);

            var bendVector1 = HumanoidIKUtility.ResolveBendVector(
                anchor,
                clip.bendTarget,
                clip.BendSpace,
                upperArmPos1);
            var bendVector2 = HumanoidIKUtility.ResolveBendVector(
                anchor,
                clip.bendTarget,
                clip.BendSpace,
                upperArmPos2);

            Assert.That(Vector3.Distance(upperArmPos1 + bendVector1, targetWorldBendGoal), Is.LessThan(0.00001f));
            Assert.That(Vector3.Distance(upperArmPos2 + bendVector2, targetWorldBendGoal), Is.LessThan(0.00001f));
        }

        [Test]
        public void DirectorLocalMigration_DoesNotReinterpretExplicitAnchorValues()
        {
            var directorObject = Track(new GameObject("Director"));
            var explicitAnchorObject = Track(new GameObject("Explicit Anchor"));
            var clip = Track(ScriptableObject.CreateInstance<HumanoidIKClip>());
            clip.anchorTransform = new ExposedReference<Transform>
            {
                defaultValue = explicitAnchorObject.transform
            };
            clip.position = new Vector3(1f, 2f, 3f);
            clip.rotation = new Vector3(15f, 25f, 35f);
            clip.bendTarget = new Vector3(-2f, 1f, 4f);
            var expectedPosition = clip.position;
            var expectedRotation = clip.rotation;
            var expectedBendTarget = clip.bendTarget;

            Assert.That(
                clip.EnsureDirectorLocalDefaultAnchor(directorObject.transform),
                Is.True);

            Assert.That(clip.position, Is.EqualTo(expectedPosition));
            Assert.That(clip.rotation, Is.EqualTo(expectedRotation));
            Assert.That(clip.bendTarget, Is.EqualTo(expectedBendTarget));
            Assert.That(
                clip.ResolveAnchor(null, directorObject.transform),
                Is.SameAs(explicitAnchorObject.transform));
        }

        [Test]
        public void ReferencePose_DistinguishesDuplicateBoneNamesByTransform()
        {
            var root = Track(new GameObject("Root"));
            var leftBranch = Track(new GameObject("LeftBranch"));
            var leftToe = Track(new GameObject("Toe"));
            var rightBranch = Track(new GameObject("RightBranch"));
            var rightToe = Track(new GameObject("Toe"));
            leftBranch.transform.SetParent(root.transform);
            leftToe.transform.SetParent(leftBranch.transform);
            rightBranch.transform.SetParent(root.transform);
            rightToe.transform.SetParent(rightBranch.transform);

            var skeleton = new[]
            {
                Bone("Root", Vector3.zero),
                Bone("LeftBranch", Vector3.left),
                Bone("Toe", new Vector3(1f, 2f, 3f)),
                Bone("RightBranch", Vector3.right),
                Bone("Toe", new Vector3(4f, 5f, 6f))
            };
            var map = new Dictionary<Transform, HumanoidIKReferenceBonePose>();

            HumanoidIKReferencePose.BuildBoneMap(root.transform, skeleton, map);

            Assert.That(map[leftToe.transform].Position, Is.EqualTo(new Vector3(1f, 2f, 3f)));
            Assert.That(map[rightToe.transform].Position, Is.EqualTo(new Vector3(4f, 5f, 6f)));
        }

        [Test]
        public void DigitPoseConversion_RoundTripsConfiguredRange()
        {
            var clip = Track(ScriptableObject.CreateInstance<HumanoidIKClip>());
            clip.EnsureDigitBendRangesInitialized();

            const float pose = 0.35f;
            var angle = HumanoidIKDigitPoseUtility.GetAngleFromPose(clip, pose, 1, 0);
            var resolvedPose = HumanoidIKDigitPoseUtility.GetPoseFromAngle(clip, angle, 1, 0);

            Assert.That(resolvedPose, Is.EqualTo(pose).Within(0.0001f));
        }

        [Test]
        public void ToeBendRanges_InitializeDefaultsAndCanBeCustomized()
        {
            var clip = Track(ScriptableObject.CreateInstance<HumanoidIKClip>());
            clip.EnsureDigitBendRangesInitialized();

            var joint0 = clip.GetToeBendRange(0);
            var joint1 = clip.GetToeBendRange(1);
            var joint2 = clip.GetToeBendRange(2);
            var baseRange = clip.GetToeBaseBendRange();

            Assert.That(joint0, Is.EqualTo(new Vector2(-25f, 20f)));
            Assert.That(joint1, Is.EqualTo(new Vector2(-18f, 8f)));
            Assert.That(joint2, Is.EqualTo(new Vector2(-12f, 5f)));
            Assert.That(baseRange, Is.EqualTo(new Vector2(-25f, 20f)));

            clip.toeBendRanges[0] = new Vector2(-40f, 35f);
            clip.toeBaseBendRange = new Vector2(-30f, 30f);

            Assert.That(clip.GetToeBendRange(0), Is.EqualTo(new Vector2(-40f, 35f)));
            Assert.That(clip.GetToeBaseBendRange(), Is.EqualTo(new Vector2(-30f, 30f)));

            var bend = clip.digitBends.thumbOrBigToe;
            HumanoidIKDigitPoseUtility.SetToeJointPose(clip, ref bend, 0, 1.0f);
            Assert.That(bend.proximal.x, Is.EqualTo(35f).Within(0.0001f));

            var baseAngle = HumanoidIKUtility.GetToeBaseBendAngle(1.0f, clip.GetToeBaseBendRange());
            Assert.That(baseAngle, Is.EqualTo(30f).Within(0.0001f));

            var pose = HumanoidIKDigitPoseUtility.GetToeJointPose(clip, bend, 0);
            Assert.That(pose, Is.EqualTo(1.0f).Within(0.0001f));
        }

        [Test]
        public void HandStretch_PreservesThumbSpread()
        {
            var clip = Track(ScriptableObject.CreateInstance<HumanoidIKClip>());
            clip.EnsureDigitBendRangesInitialized();
            var pose = clip.digitBends;
            var thumb = pose.thumbOrBigToe;
            const float expectedThumbSpread = 17.25f;
            thumb.proximal.y = expectedThumbSpread;
            pose.thumbOrBigToe = thumb;
            clip.digitBends = pose;

            HumanoidIKSceneOverlay.SetHandStretchPose(clip, 0.65f);

            Assert.That(
                clip.digitBends.thumbOrBigToe.proximal.y,
                Is.EqualTo(expectedThumbSpread));
            Assert.That(
                HumanoidIKDigitPoseUtility.GetHandStretch(clip, in clip.digitBends),
                Is.EqualTo(0.65f).Within(0.0001f));
        }

        [Test]
        public void ThumbSpreadVerticalSlider_PutsPositiveValueAtTop()
        {
            var clip = Track(ScriptableObject.CreateInstance<HumanoidIKClip>());
            clip.EnsureDigitBendRangesInitialized();
            clip.thumbSpreadRange = new Vector2(-42f, 27f);

            var sliderRange = HumanoidIKSceneOverlay.GetThumbSpreadVerticalSliderRange(clip);

            Assert.That(sliderRange.x, Is.EqualTo(27f));
            Assert.That(sliderRange.y, Is.EqualTo(-42f));
        }

        [Test]
        public void ToeStretch_IncludesSharedToeRootForArticulatedRig()
        {
            var chains = new List<Transform[]>();
            var pose = new HumanoidIKDigitBendPose();
            for (var digitIndex = 0; digitIndex < 5; digitIndex++)
            {
                var toe = Track(new GameObject($"Toe {digitIndex}"));
                chains.Add(new[] { toe.transform });

                var bend = new HumanoidIKJointBend();
                HumanoidIKDigitPoseUtility.SetToeJointPose(ref bend, 0, 0.4f);
                HumanoidIKDigitPoseUtility.SetDigitBend(ref pose, digitIndex, bend);
            }

            var articulatedStretch = HumanoidIKDigitPoseUtility.GetToeStretch(
                in pose,
                chains,
                true,
                -0.2f);
            var simpleStretch = HumanoidIKDigitPoseUtility.GetToeStretch(
                in pose,
                chains,
                false,
                -0.2f);

            Assert.That(articulatedStretch, Is.EqualTo(0.3f).Within(0.0001f));
            Assert.That(simpleStretch, Is.EqualTo(0.4f).Within(0.0001f));

            var clip = Track(ScriptableObject.CreateInstance<HumanoidIKClip>());
            clip.digitBends = pose;
            clip.toeBaseBend = -0.2f;
            HumanoidIKSceneOverlay.SetToeStretchPose(clip, chains, true, 0.65f);

            Assert.That(clip.toeBaseBend, Is.EqualTo(0.65f));
            Assert.That(
                HumanoidIKDigitPoseUtility.GetToeStretch(
                    in clip.digitBends,
                    chains,
                    true,
                    clip.toeBaseBend),
                Is.EqualTo(0.65f).Within(0.0001f));
        }

        [Test]
        public void ToeAnatomicalRotation_UsesToeDirectionInsteadOfImportedAxisNames()
        {
            var forwardAlongImportedY = Vector3.up;
            var soleNormalAlongImportedZ = Vector3.forward;
            var anatomicalRight = Vector3.Cross(
                soleNormalAlongImportedZ,
                forwardAlongImportedY).normalized;

            Assert.That(
                HumanoidIKUtility.TryGetToeAnatomicalRotation(
                    new Vector3(0f, 8f, 0f),
                    forwardAlongImportedY,
                    soleNormalAlongImportedZ,
                    true,
                    out var fanRotation),
                Is.True);

            var fannedForward = fanRotation * forwardAlongImportedY;
            Assert.That(Vector3.Dot(fannedForward, anatomicalRight), Is.GreaterThan(0f));
            fanRotation.ToAngleAxis(out _, out var fanAxis);
            Assert.That(
                Mathf.Abs(Vector3.Dot(fanAxis.normalized, soleNormalAlongImportedZ)),
                Is.GreaterThan(0.9999f));
        }

        [Test]
        public void ToeAnatomicalRotation_PositiveStretchOpensTowardDorsum()
        {
            Assert.That(
                HumanoidIKUtility.TryGetToeAnatomicalRotation(
                    new Vector3(20f, 0f, 0f),
                    Vector3.forward,
                    Vector3.up,
                    true,
                    out var openRotation),
                Is.True);
            Assert.That(
                HumanoidIKUtility.TryGetToeAnatomicalRotation(
                    new Vector3(-20f, 0f, 0f),
                    Vector3.forward,
                    Vector3.up,
                    true,
                    out var closedRotation),
                Is.True);

            Assert.That(Vector3.Dot(openRotation * Vector3.forward, Vector3.up), Is.GreaterThan(0f));
            Assert.That(Vector3.Dot(closedRotation * Vector3.forward, Vector3.up), Is.LessThan(0f));
        }

        [Test]
        public void ToeFan_PositivePoseSpreadsBigAndLittleToesApart()
        {
            var bigOffset = HumanoidIKUtility.GetArticulatedToeFanOffset(0, 1f);
            var middleOffset = HumanoidIKUtility.GetArticulatedToeFanOffset(2, 1f);
            var littleOffset = HumanoidIKUtility.GetArticulatedToeFanOffset(4, 1f);

            Assert.That(bigOffset.proximal.y, Is.GreaterThan(0f));
            Assert.That(middleOffset.proximal.y, Is.Zero.Within(0.000001f));
            Assert.That(littleOffset.proximal.y, Is.LessThan(0f));
        }

        [Test]
        public void ArticulatedToeOrder_UsesReferenceLateralPositionInsteadOfSiblingOrder()
        {
            var left = new List<(string Name, float ReferenceX, int SiblingIndex)>
            {
                ("Big", -0.055552f, 0),
                ("Second", -0.078819f, 1),
                ("Third", -0.095957f, 2),
                ("Fifth", -0.123392f, 3),
                ("Fourth", -0.110979f, 4)
            };
            left.Sort((a, b) => HumanoidIKDigitChainCache.CompareToeRootOrder(
                a.ReferenceX,
                a.SiblingIndex,
                b.ReferenceX,
                b.SiblingIndex,
                true));

            var right = new List<(string Name, float ReferenceX, int SiblingIndex)>
            {
                ("Big", 0.054286f, 0),
                ("Second", 0.078662f, 1),
                ("Third", 0.096209f, 2),
                ("Fifth", 0.123466f, 3),
                ("Fourth", 0.110948f, 4)
            };
            right.Sort((a, b) => HumanoidIKDigitChainCache.CompareToeRootOrder(
                a.ReferenceX,
                a.SiblingIndex,
                b.ReferenceX,
                b.SiblingIndex,
                false));

            CollectionAssert.AreEqual(
                new[] { "Big", "Second", "Third", "Fourth", "Fifth" },
                left.ConvertAll(entry => entry.Name));
            CollectionAssert.AreEqual(
                new[] { "Big", "Second", "Third", "Fourth", "Fifth" },
                right.ConvertAll(entry => entry.Name));
        }

        [Test]
        public void ToeBaseSliderLayout_MirrorsOnceBeforeToeJointRegion()
        {
            var leftRect = HumanoidIKSceneOverlay.GetToeBaseSliderCanvasRect(false);
            var rightRect = HumanoidIKSceneOverlay.GetToeBaseSliderCanvasRect(true);

            Assert.That(leftRect, Is.EqualTo(new Rect(198f, 75f, 54f, 14f)));
            Assert.That(rightRect, Is.EqualTo(new Rect(148f, 75f, 54f, 14f)));
            Assert.That(leftRect.center.x + rightRect.center.x, Is.EqualTo(400f));
        }

        [Test]
        public void GizmoGeometry_SolvePreviewMidpointPreservesLimbLengths()
        {
            var root = Vector3.zero;
            var currentMid = Vector3.up;
            var currentEnd = Vector3.up * 2f;
            var target = Vector3.right;
            var bendTarget = Vector3.up;

            var previewMid = HumanoidIKGizmoGeometry.SolvePreviewMidpoint(
                root,
                currentMid,
                currentEnd,
                target,
                bendTarget);

            Assert.That(Vector3.Distance(root, previewMid), Is.EqualTo(1f).Within(0.0001f));
            Assert.That(Vector3.Distance(previewMid, target), Is.EqualTo(1f).Within(0.0001f));
            Assert.That(previewMid.y, Is.GreaterThan(0f));
        }

        [Test]
        public void GizmoGeometry_BoneBoxRotationUsesNamedForwardAndUpAxes()
        {
            var direction = new Vector3(0.2f, 0.1f, 2f);
            var preferredUp = Vector3.up;

            var rotation = HumanoidIKGizmoGeometry.GetBoneBoxRotation(direction, preferredUp);
            var expectedForward = direction.normalized;
            var expectedUp = Vector3.ProjectOnPlane(preferredUp, expectedForward).normalized;

            Assert.That(Vector3.Dot(rotation * Vector3.forward, expectedForward), Is.GreaterThan(0.9999f));
            Assert.That(Vector3.Dot(rotation * Vector3.up, expectedUp), Is.GreaterThan(0.9999f));
        }

        [Test]
        public void TargetRotation_CanonicalFootFrameConvertsThroughImportedBoneAxes()
        {
            var referenceFootRotation = Quaternion.Euler(82f, -11f, 173f);
            var referenceFootMatrix = Matrix4x4.TRS(
                new Vector3(0.1f, 0.2f, -0.3f),
                referenceFootRotation,
                Vector3.one);
            var referenceToeMatrix = Matrix4x4.TRS(
                new Vector3(0.1f, 0.18f, -0.05f),
                Quaternion.identity,
                Vector3.one);
            var referenceLowerLegMatrix = Matrix4x4.TRS(
                new Vector3(0.1f, 0.65f, -0.28f),
                Quaternion.identity,
                Vector3.one);

            Assert.That(
                HumanoidIKUtility.TryBuildFootDisplayRotation(
                    referenceFootMatrix,
                    true,
                    referenceToeMatrix,
                    referenceLowerLegMatrix,
                    out var referenceDisplayRotation),
                Is.True);
            Assert.That(
                Vector3.Dot(referenceDisplayRotation * Vector3.forward, Vector3.forward),
                Is.GreaterThan(0.99999f));
            Assert.That(
                Vector3.Dot(referenceDisplayRotation * Vector3.up, Vector3.up),
                Is.GreaterThan(0.99999f));

            var boneToDisplayRotation =
                Quaternion.Inverse(referenceFootRotation) * referenceDisplayRotation;
            var targetDisplayRotation = Quaternion.Euler(14f, -32f, 7f);
            var targetBoneRotation = HumanoidIKUtility.ToBoneRotation(
                targetDisplayRotation,
                boneToDisplayRotation);
            var resolvedDisplayRotation = targetBoneRotation * boneToDisplayRotation;

            Assert.That(
                Mathf.Abs(Quaternion.Dot(targetDisplayRotation, resolvedDisplayRotation)),
                Is.GreaterThan(0.99999f));
            Assert.That(
                Quaternion.Angle(targetDisplayRotation, targetBoneRotation),
                Is.GreaterThan(30f));
        }

        [Test]
        public void FootDisplayRotation_ProjectsAnkleToToeSlopeOntoSolePlane()
        {
            var footMatrix = Matrix4x4.TRS(
                new Vector3(0.1f, 0.2f, -0.3f),
                Quaternion.Euler(18f, 4f, -7f),
                Vector3.one);
            var toeMatrix = Matrix4x4.TRS(
                new Vector3(0.15f, 0.08f, 0.12f),
                Quaternion.identity,
                Vector3.one);
            var lowerLegMatrix = Matrix4x4.TRS(
                new Vector3(0.1f, 0.7f, -0.25f),
                Quaternion.identity,
                Vector3.one);

            Assert.That(
                HumanoidIKUtility.TryBuildFootDisplayRotation(
                    footMatrix,
                    true,
                    toeMatrix,
                    lowerLegMatrix,
                    out var soleRotation),
                Is.True);
            Assert.That(
                HumanoidIKUtility.TryBuildLegacyFootLineRotation(
                    footMatrix,
                    true,
                    toeMatrix,
                    lowerLegMatrix,
                    out var legacyLineRotation),
                Is.True);

            var ankleToToe = toeMatrix.MultiplyPoint3x4(Vector3.zero) -
                             footMatrix.MultiplyPoint3x4(Vector3.zero);
            var toeInSole = Quaternion.Inverse(soleRotation) * ankleToToe;
            Assert.That(Mathf.Abs((soleRotation * Vector3.forward).y), Is.LessThan(0.000001f));
            Assert.That(Vector3.Dot(soleRotation * Vector3.up, Vector3.up), Is.GreaterThan(0.99999f));
            Assert.That(toeInSole.y, Is.EqualTo(-0.12f).Within(0.000001f));
            Assert.That(toeInSole.z, Is.EqualTo(
                Vector3.ProjectOnPlane(ankleToToe, Vector3.up).magnitude).Within(0.000001f));
            Assert.That(Quaternion.Angle(soleRotation, legacyLineRotation), Is.GreaterThan(10f));
        }

        [Test]
        public void FootRotationMigration_PreservesLegacyBonePoseInProjectedSoleFrame()
        {
            var referenceFootRotation = Quaternion.Euler(82f, -11f, 173f);
            var footMatrix = Matrix4x4.TRS(Vector3.zero, referenceFootRotation, Vector3.one);
            var toeMatrix = Matrix4x4.TRS(
                new Vector3(0.02f, -0.08f, 0.18f),
                Quaternion.identity,
                Vector3.one);
            var lowerLegMatrix = Matrix4x4.TRS(
                new Vector3(0f, 0.5f, 0.02f),
                Quaternion.identity,
                Vector3.one);
            Assert.That(HumanoidIKUtility.TryBuildFootDisplayRotation(
                footMatrix,
                true,
                toeMatrix,
                lowerLegMatrix,
                out var soleRotation), Is.True);
            Assert.That(HumanoidIKUtility.TryBuildLegacyFootLineRotation(
                footMatrix,
                true,
                toeMatrix,
                lowerLegMatrix,
                out var legacyLineRotation), Is.True);
            var boneToSole = Quaternion.Inverse(referenceFootRotation) * soleRotation;
            var boneToLegacyLine = Quaternion.Inverse(referenceFootRotation) * legacyLineRotation;
            var storedLegacyLineRotation = Quaternion.Euler(22f, -13f, 8f);

            var migratedSoleRotation = HumanoidIKUtility.ToProjectedSoleRotation(
                storedLegacyLineRotation,
                HumanoidIKRotationSpace.HumanoidEffector,
                0,
                boneToSole,
                boneToLegacyLine);
            var legacyBoneRotation =
                storedLegacyLineRotation * Quaternion.Inverse(boneToLegacyLine);
            var migratedBoneRotation =
                migratedSoleRotation * Quaternion.Inverse(boneToSole);

            Assert.That(Quaternion.Angle(legacyBoneRotation, migratedBoneRotation),
                Is.LessThan(0.0001f));
            Assert.That(
                HumanoidIKUtility.ToProjectedSoleRotation(
                    storedLegacyLineRotation,
                    HumanoidIKRotationSpace.HumanoidEffector,
                    HumanoidIKClip.CurrentFootRotationFrameVersion,
                    boneToSole,
                    boneToLegacyLine),
                Is.EqualTo(storedLegacyLineRotation));
        }

        [Test]
        public void TargetRotation_CanonicalFootFrameFallsBackWithoutMappedToes()
        {
            var footMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(90f, 0f, 0f), Vector3.one);
            var lowerLegMatrix = Matrix4x4.TRS(Vector3.up, Quaternion.identity, Vector3.one);

            Assert.That(
                HumanoidIKUtility.TryBuildFootDisplayRotation(
                    footMatrix,
                    false,
                    Matrix4x4.identity,
                    lowerLegMatrix,
                    out var displayRotation),
                Is.True);
            Assert.That(Vector3.Dot(displayRotation * Vector3.forward, Vector3.forward), Is.GreaterThan(0.99999f));
            Assert.That(Vector3.Dot(displayRotation * Vector3.up, Vector3.up), Is.GreaterThan(0.99999f));
        }

        [Test]
        public void GizmoGeometry_CanonicalFootSlabsAreConnectedAndMirrored()
        {
            var expectedBottom = float.NaN;
            var previousEnd = float.NaN;
            for (var slabIndex = 0;
                 slabIndex < HumanoidIKGizmoGeometry.CanonicalFootSlabCount;
                 slabIndex++)
            {
                var left = HumanoidIKGizmoGeometry.GetCanonicalFootSlab(true, slabIndex);
                var right = HumanoidIKGizmoGeometry.GetCanonicalFootSlab(false, slabIndex);
                var bottom = left.Center.y - left.Size.y * 0.5f;
                var start = left.Center.z - left.Size.z * 0.5f;
                var end = left.Center.z + left.Size.z * 0.5f;

                Assert.That(left.Center.x, Is.EqualTo(-right.Center.x).Within(0.000001f));
                Assert.That(left.Center.y, Is.EqualTo(right.Center.y).Within(0.000001f));
                Assert.That(left.Center.z, Is.EqualTo(right.Center.z).Within(0.000001f));
                Assert.That(left.Size, Is.EqualTo(right.Size));
                if (!float.IsNaN(expectedBottom))
                {
                    Assert.That(bottom, Is.EqualTo(expectedBottom).Within(0.000001f));
                    Assert.That(start, Is.EqualTo(previousEnd).Within(0.000001f));
                }

                expectedBottom = bottom;
                previousEnd = end;
            }
        }

        [Test]
        public void GizmoGeometry_CanonicalFootSlabsExtendOnlyTowardLateralSide()
        {
            var originalWidths = new[] { 0.0648f, 0.0702f, 0.0684f, 0.0792f, 0.09f };
            var originalCenters = new[] { 0.006f, 0.006f, 0.0006f, 0.0033f, 0.006f };
            for (var slabIndex = 0;
                 slabIndex < HumanoidIKGizmoGeometry.CanonicalFootSlabCount;
                 slabIndex++)
            {
                var slab = HumanoidIKGizmoGeometry.GetCanonicalFootSlab(true, slabIndex);
                var originalHalfWidth = originalWidths[slabIndex] * 0.5f;

                Assert.That(
                    slab.Size.x,
                    Is.EqualTo(
                        originalWidths[slabIndex] +
                        HumanoidIKGizmoGeometry.CanonicalFootLateralExtension)
                        .Within(0.000001f));
                Assert.That(
                    slab.Center.x + slab.Size.x * 0.5f,
                    Is.EqualTo(originalCenters[slabIndex] + originalHalfWidth)
                        .Within(0.000001f));
                Assert.That(
                    slab.Center.x - slab.Size.x * 0.5f,
                    Is.EqualTo(
                        originalCenters[slabIndex] - originalHalfWidth -
                        HumanoidIKGizmoGeometry.CanonicalFootLateralExtension)
                        .Within(0.000001f));
            }
        }

        [Test]
        public void GizmoGeometry_CanonicalFootSlabsUseRequestedWidthAndLengthMargins()
        {
            var rear = HumanoidIKGizmoGeometry.GetCanonicalFootSlab(true, 0);
            var middle = HumanoidIKGizmoGeometry.GetCanonicalFootSlab(true, 2);
            var forefoot = HumanoidIKGizmoGeometry.GetCanonicalFootSlab(
                true,
                HumanoidIKGizmoGeometry.CanonicalFootSlabCount - 1);
            var rearStart = rear.Center.z - rear.Size.z * 0.5f;
            var forefootEnd = forefoot.Center.z + forefoot.Size.z * 0.5f;

            Assert.That(
                forefoot.Size.x,
                Is.EqualTo(
                    HumanoidIKGizmoGeometry.CanonicalFootBaseWidth *
                    HumanoidIKGizmoGeometry.CanonicalFootWidthMultiplier +
                    HumanoidIKGizmoGeometry.CanonicalFootLateralExtension)
                    .Within(0.000001f));
            Assert.That(
                forefoot.Center.x + forefoot.Size.x * 0.5f,
                Is.EqualTo(
                    HumanoidIKGizmoGeometry.CanonicalFootMedialOffset +
                    HumanoidIKGizmoGeometry.CanonicalFootWidth * 0.5f)
                    .Within(0.000001f));
            Assert.That(
                forefoot.Center.x - forefoot.Size.x * 0.5f,
                Is.EqualTo(
                    HumanoidIKGizmoGeometry.CanonicalFootMedialOffset -
                    HumanoidIKGizmoGeometry.CanonicalFootWidth * 0.5f -
                    HumanoidIKGizmoGeometry.CanonicalFootLateralExtension)
                    .Within(0.000001f));
            Assert.That(
                forefootEnd,
                Is.EqualTo(HumanoidIKGizmoGeometry.CanonicalFootLength)
                    .Within(0.000001f));
            Assert.That(
                forefootEnd - rearStart,
                Is.EqualTo(
                    HumanoidIKGizmoGeometry.CanonicalFootLength *
                    1.24f *
                    HumanoidIKGizmoGeometry.CanonicalFootSlabLengthMultiplier)
                    .Within(0.000001f));

            var bigToe = HumanoidIKGizmoGeometry.GetCanonicalToe(true, 0);
            var overallEnvelopeLength =
                bigToe.BasePosition.z +
                bigToe.Length * HumanoidIKGizmoGeometry.CanonicalToeDisplayLengthRatio +
                bigToe.Radius - rearStart;
            Assert.That(overallEnvelopeLength, Is.InRange(0.285f, 0.3f));
            Assert.That(forefoot.Size.x, Is.EqualTo(0.1f).Within(0.000001f));
            Assert.That(middle.Size.x, Is.EqualTo(0.0784f).Within(0.000001f));
        }

        [Test]
        public void GizmoGeometry_BoundFootFitUsesReferenceLengthAndToeRootSpan()
        {
            const float footToToeDistance = 0.14291f;
            const float toeRootSpan = 0.07074f;
            const float footToSoleDrop = 0.04038f;
            var scale = HumanoidIKGizmoGeometry.GetCanonicalFootFitScale(
                footToToeDistance,
                toeRootSpan,
                footToSoleDrop);
            var fit = new HumanoidIKCanonicalFootFit(scale, -footToSoleDrop);
            var forefoot = fit.GetSlab(true, HumanoidIKGizmoGeometry.CanonicalFootSlabCount - 1);
            var toeBase = fit.GetToeBasePivot(true);

            Assert.That(scale.z, Is.EqualTo(
                footToToeDistance / HumanoidIKGizmoGeometry.CanonicalFootLength).Within(0.000001f));
            Assert.That(scale.x, Is.EqualTo(
                toeRootSpan / HumanoidIKGizmoGeometry.CanonicalToeRootSpan).Within(0.000001f));
            Assert.That(scale.y, Is.GreaterThanOrEqualTo(Mathf.Sqrt(scale.x * scale.z)));
            Assert.That(toeBase.z, Is.EqualTo(footToToeDistance).Within(0.000001f));
            Assert.That(
                toeBase.y,
                Is.EqualTo(
                    -footToSoleDrop -
                    HumanoidIKGizmoGeometry.CanonicalFootSoleBottom * scale.y)
                    .Within(0.000001f));
            Assert.That(toeBase.y, Is.GreaterThan(-footToSoleDrop));
            Assert.That(
                forefoot.Center.z + forefoot.Size.z * 0.5f,
                Is.EqualTo(footToToeDistance).Within(0.000001f));
            Assert.That(
                forefoot.Size.x,
                Is.EqualTo(
                    (HumanoidIKGizmoGeometry.CanonicalFootWidth +
                     HumanoidIKGizmoGeometry.CanonicalFootLateralExtension) *
                    scale.x)
                    .Within(0.000001f));
            var rear = fit.GetSlab(true, 0);
            Assert.That(
                forefoot.Center.z + forefoot.Size.z * 0.5f -
                (rear.Center.z - rear.Size.z * 0.5f),
                Is.EqualTo(
                    footToToeDistance *
                    1.24f *
                    HumanoidIKGizmoGeometry.CanonicalFootSlabLengthMultiplier)
                    .Within(0.000001f));

            for (var slabIndex = 0;
                 slabIndex < HumanoidIKGizmoGeometry.CanonicalFootSlabCount;
                 slabIndex++)
            {
                var slab = fit.GetSlab(true, slabIndex);
                Assert.That(
                    slab.Center.y - slab.Size.y * 0.5f,
                    Is.EqualTo(-footToSoleDrop).Within(0.000001f));
            }
        }

        [Test]
        public void GizmoGeometry_FootFitLengthTracksProjectedFootBoneLength()
        {
            const float shortFootToToeDistance = 0.1f;
            const float longFootToToeDistance = 0.2f;
            var shortFit = new HumanoidIKCanonicalFootFit(
                HumanoidIKGizmoGeometry.GetCanonicalFootFitScale(
                    shortFootToToeDistance,
                    0f));
            var longFit = new HumanoidIKCanonicalFootFit(
                HumanoidIKGizmoGeometry.GetCanonicalFootFitScale(
                    longFootToToeDistance,
                    0f));

            Assert.That(
                longFit.GetToeBasePivot(true).z,
                Is.EqualTo(shortFit.GetToeBasePivot(true).z * 2f).Within(0.000001f));
            for (var slabIndex = 0;
                 slabIndex < HumanoidIKGizmoGeometry.CanonicalFootSlabCount;
                 slabIndex++)
            {
                var shortSlab = shortFit.GetSlab(true, slabIndex);
                var longSlab = longFit.GetSlab(true, slabIndex);
                Assert.That(
                    longSlab.Center.z,
                    Is.EqualTo(shortSlab.Center.z * 2f).Within(0.000001f));
                Assert.That(
                    longSlab.Size.z,
                    Is.EqualTo(shortSlab.Size.z * 2f).Within(0.000001f));
            }
        }

        [Test]
        public void GizmoGeometry_ArticulatedFootFitUsesReferenceToeRootsAndLengths()
        {
            var scale = new Vector3(0.9f, 0.8f, 0.7f);
            var toeBases = new Vector3[HumanoidIKGizmoGeometry.CanonicalToeCount];
            var toeLengths = new float[HumanoidIKGizmoGeometry.CanonicalToeCount];
            var toeForwards = new Vector3[HumanoidIKGizmoGeometry.CanonicalToeCount];
            var hasOverrides = new bool[HumanoidIKGizmoGeometry.CanonicalToeCount];
            var toeBasePivot = new Vector3(0.006f, 0f, 0.142f);
            toeBases[0] = new Vector3(0.041f, -0.004f, 0.172f);
            toeLengths[0] = 0.027f;
            toeForwards[0] = new Vector3(0.2f, 0f, 1f).normalized;
            hasOverrides[0] = true;
            var fit = new HumanoidIKCanonicalFootFit(
                scale,
                -0.01f,
                toeBases,
                toeLengths,
                hasOverrides,
                toeForwards,
                toeBasePivot);

            var fittedBigToe = fit.GetToe(true, 0);
            var fittedSecondToe = fit.GetToe(true, 1);
            var canonicalBigToe = HumanoidIKGizmoGeometry.GetCanonicalToe(true, 0);
            var canonicalSecondToe = HumanoidIKGizmoGeometry.GetCanonicalToe(true, 1);

            var fittedForefoot = fit.GetSlab(
                true,
                HumanoidIKGizmoGeometry.CanonicalFootSlabCount - 1);
            var fittedSoleBottom = fittedForefoot.Center.y - fittedForefoot.Size.y * 0.5f;
            var fittedPivot = fit.GetToeBasePivot(true);

            Assert.That(
                Vector3.Distance(fittedBigToe.BasePosition, toeBases[0]),
                Is.LessThan(0.000001f));
            Assert.That(fittedBigToe.Length, Is.EqualTo(toeLengths[0]).Within(0.000001f));
            Assert.That(
                Vector3.Dot(fittedBigToe.Forward, toeForwards[0]),
                Is.GreaterThan(0.99999f));
            Assert.That(
                fittedBigToe.Radius,
                Is.EqualTo(
                    HumanoidIKGizmoGeometry.GetDigitPrimitiveRadius(
                        canonicalBigToe.Length * scale.z,
                        HumanoidIKGizmoGeometry.GetCanonicalToeDiameterToLength(0) *
                        HumanoidIKGizmoGeometry.FittedToeRadiusScale,
                        canonicalBigToe.Length * scale.z))
                    .Within(0.000001f));
            Assert.That(
                fittedSecondToe.BasePosition.x,
                Is.EqualTo(canonicalSecondToe.BasePosition.x * scale.x).Within(0.000001f));
            Assert.That(
                fittedSecondToe.BasePosition.z,
                Is.EqualTo(canonicalSecondToe.BasePosition.z * scale.z).Within(0.000001f));
            Assert.That(
                fittedSecondToe.BasePosition.y - fittedSecondToe.Radius,
                Is.EqualTo(fittedSoleBottom).Within(0.000001f));
            Assert.That(Vector3.Distance(fittedPivot, toeBasePivot), Is.LessThan(0.000001f));
            Assert.That(
                fittedSecondToe.Length,
                Is.EqualTo(canonicalSecondToe.Length * scale.z).Within(0.000001f));

            var pose = default(HumanoidIKDigitBendPose);
            var points = new List<Vector3>();
            HumanoidIKGizmoGeometry.BuildCanonicalToePoints(
                true,
                0,
                in fittedBigToe,
                fittedPivot,
                HumanoidIKToeRigKind.ArticulatedToes,
                in pose,
                0f,
                0f,
                points);
            Assert.That(Vector3.Distance(points[0], toeBases[0]), Is.LessThan(0.000001f));
        }

        [Test]
        public void GizmoGeometry_BoundSoleSitsTenMillimetersBelowMappedToePivot()
        {
            var toeBasePivot = new Vector3(0.004f, -0.012f, 0.145f);
            var soleHeight = HumanoidIKGizmoGeometry.GetBoundFootSoleHeight(
                toeBasePivot.y);
            var fit = new HumanoidIKCanonicalFootFit(
                Vector3.one,
                soleHeight,
                toeBasePivotOverride: toeBasePivot);

            Assert.That(
                toeBasePivot.y - soleHeight,
                Is.EqualTo(HumanoidIKGizmoGeometry.BoundFootSoleDropBelowToe)
                    .Within(0.000001f));
            Assert.That(
                Vector3.Distance(fit.GetToeBasePivot(true), toeBasePivot),
                Is.LessThan(0.000001f));
            for (var slabIndex = 0;
                 slabIndex < HumanoidIKGizmoGeometry.CanonicalFootSlabCount;
                 slabIndex++)
            {
                var slab = fit.GetSlab(true, slabIndex);
                Assert.That(
                    slab.Center.y - slab.Size.y * 0.5f,
                    Is.EqualTo(soleHeight).Within(0.000001f));
            }
        }

        [Test]
        public void GizmoGeometry_HandDigitThicknessUsesTotalLengthAndProximalSpacing()
        {
            const float totalLength = 0.08f;
            const float proximalSpacing = 0.10f;
            const float tightProximalSpacing = 0.02f;
            var baseRadius = HumanoidIKGizmoGeometry.GetHandDigitBaseRadius(
                totalLength,
                proximalSpacing,
                false);
            var shortDigitRadius = HumanoidIKGizmoGeometry.GetHandDigitBaseRadius(
                0.02f,
                proximalSpacing,
                false);
            var spacingCeilingRadius = HumanoidIKGizmoGeometry.GetHandDigitBaseRadius(
                0.20f,
                tightProximalSpacing,
                false);
            var thumbSpacingCeilingRadius = HumanoidIKGizmoGeometry.GetHandDigitBaseRadius(
                0.20f,
                tightProximalSpacing,
                true);
            var baseDiameter = baseRadius * 2f;
            var proximalScale = HumanoidIKGizmoGeometry.GetHandDigitRadiusScale(0, false);
            var intermediateScale = HumanoidIKGizmoGeometry.GetHandDigitRadiusScale(1, false);
            var distalScale = HumanoidIKGizmoGeometry.GetHandDigitRadiusScale(2, false);
            var proximal = HumanoidIKGizmoGeometry.GetDigitPrimitiveRadius(
                0.01f,
                proximalScale,
                baseDiameter);
            var intermediate = HumanoidIKGizmoGeometry.GetDigitPrimitiveRadius(
                0.04f,
                intermediateScale,
                baseDiameter);
            var distal = HumanoidIKGizmoGeometry.GetDigitPrimitiveRadius(
                0.02f,
                distalScale,
                baseDiameter);
            var thickestJoint = proximal * HumanoidIKGizmoGeometry.HandDigitJointRadiusScale;

            Assert.That(
                baseRadius / totalLength,
                Is.EqualTo(HumanoidIKGizmoGeometry.HandDigitRadiusToTotalLength)
                    .Within(0.0001f));
            Assert.That(
                shortDigitRadius / 0.02f,
                Is.EqualTo(HumanoidIKGizmoGeometry.HandDigitRadiusToTotalLength)
                    .Within(0.0001f));
            Assert.That(
                spacingCeilingRadius *
                HumanoidIKGizmoGeometry.GetHandDigitMaximumRadiusScale(false) /
                tightProximalSpacing,
                Is.EqualTo(HumanoidIKGizmoGeometry.HandDigitMaximumRadiusToProximalSpacing)
                    .Within(0.0001f));
            Assert.That(
                thumbSpacingCeilingRadius *
                HumanoidIKGizmoGeometry.GetHandDigitMaximumRadiusScale(true) /
                tightProximalSpacing,
                Is.EqualTo(HumanoidIKGizmoGeometry.HandDigitMaximumRadiusToProximalSpacing)
                    .Within(0.0001f));
            Assert.That(proximal, Is.GreaterThan(intermediate));
            Assert.That(intermediate, Is.GreaterThan(distal));
            Assert.That((thickestJoint - distal) / thickestJoint, Is.LessThan(0.1f));
            Assert.That(
                HumanoidIKGizmoGeometry.GetHandDigitRadiusScale(0, true),
                Is.GreaterThan(proximalScale * 1.1f));
            Assert.That(
                HumanoidIKGizmoGeometry.GetHandDigitRadiusScale(1, true),
                Is.EqualTo(intermediateScale));
            Assert.That(
                HumanoidIKGizmoGeometry.GetHandDigitRadiusScale(2, true),
                Is.EqualTo(distalScale));
        }

        [Test]
        public void GizmoGeometry_HandDigitProximalSpacingUsesPalmDisplayPlane()
        {
            var digitBases = new[]
            {
                new Vector3(0f, 0f, 0f),
                new Vector3(0.02f, 0.5f, 0f)
            };
            var hasDigitBase = new[] { true, true };

            var spacing = HumanoidIKGizmoGeometry.GetNearestHandDigitProximalSpacing(
                0,
                digitBases,
                hasDigitBase,
                Quaternion.identity);

            Assert.That(spacing, Is.EqualTo(0.02f).Within(0.0001f));
        }

        [Test]
        public void GizmoGeometry_HandDigitRadiusStaysBelowHalfProximalSpacing()
        {
            const float minimumSpacing = 0.02f;
            const float longDigitLength = 1f;
            var fingerBaseRadius = HumanoidIKGizmoGeometry.GetHandDigitBaseRadius(
                longDigitLength,
                minimumSpacing,
                false);
            var thumbBaseRadius = HumanoidIKGizmoGeometry.GetHandDigitBaseRadius(
                longDigitLength,
                minimumSpacing,
                true);
            var fingerMaximumRadius = fingerBaseRadius *
                                      HumanoidIKGizmoGeometry.GetHandDigitMaximumRadiusScale(false);
            var thumbMaximumRadius = thumbBaseRadius *
                                     HumanoidIKGizmoGeometry.GetHandDigitMaximumRadiusScale(true);

            Assert.That(fingerMaximumRadius, Is.LessThan(minimumSpacing * 0.5f));
            Assert.That(thumbMaximumRadius, Is.LessThan(minimumSpacing * 0.5f));
            Assert.That(
                fingerMaximumRadius / minimumSpacing,
                Is.EqualTo(0.40f).Within(0.0001f));
            Assert.That(
                thumbMaximumRadius / minimumSpacing,
                Is.EqualTo(0.40f).Within(0.0001f));
            Assert.That(
                fingerMaximumRadius + thumbMaximumRadius,
                Is.EqualTo(
                        minimumSpacing *
                        HumanoidIKGizmoGeometry.HandDigitMaximumRadiusToProximalSpacing *
                        2f)
                    .Within(0.0001f));
        }

        [Test]
        public void GizmoGeometry_HandPalmUsesDisplayAxisBoundsAndNearestLengthCoordinate()
        {
            var wrist = new Vector3(1f, 2f, 3f);
            var displayRotation = Quaternion.Euler(0f, 90f, 0f);
            var displayLocalDigitBases = new[]
            {
                new Vector3(0.04f, 0f, 0.02f),
                new Vector3(-0.03f, 0f, 0.09f),
                new Vector3(0.035f, 0f, 0.10f),
                new Vector3(0.01f, 0f, 0.11f),
                new Vector3(0.035f, 0f, 0.12f)
            };
            var digitBases = new Vector3[displayLocalDigitBases.Length];
            for (var digitIndex = 0; digitIndex < displayLocalDigitBases.Length; digitIndex++)
            {
                digitBases[digitIndex] = wrist +
                                         displayRotation * displayLocalDigitBases[digitIndex];
            }
            var hasDigitBase = new[] { true, true, true, true, true };
            var digitBaseRadii = new[] { 0.01f, 0.004f, 0.005f, 0.006f, 0.007f };

            var measured = HumanoidIKGizmoGeometry.TryGetHandPalmMeasurements(
                wrist,
                displayRotation,
                digitBases,
                hasDigitBase,
                digitBaseRadii,
                out var palmCenter,
                out var width,
                out var length,
                out var thickness);
            var boxSize = HumanoidIKGizmoGeometry.GetHandPalmBoxSize(
                width,
                length,
                thickness);
            var wristRadius = HumanoidIKGizmoGeometry.GetHandWristSphereRadius(width);
            var palmCenterInDisplay = Quaternion.Inverse(displayRotation) *
                                      (palmCenter - wrist);
            var thumbProximalRadius = HumanoidIKGizmoGeometry.GetHandDigitProximalRadius(
                digitBaseRadii[0],
                true);

            Assert.That(measured, Is.True);
            Assert.That(
                Vector3.Distance(displayLocalDigitBases[0], displayLocalDigitBases[1]),
                Is.GreaterThan(
                    Vector3.Distance(displayLocalDigitBases[0], displayLocalDigitBases[2])));
            Assert.That(palmCenterInDisplay.x, Is.EqualTo(0.005f).Within(0.0001f));
            Assert.That(palmCenterInDisplay.y, Is.Zero.Within(0.0001f));
            Assert.That(
                palmCenterInDisplay.z,
                Is.EqualTo((0.02f - thumbProximalRadius + 0.09f) * 0.5f)
                    .Within(0.0001f));
            Assert.That(width, Is.EqualTo(0.078f).Within(0.0001f));
            Assert.That(
                length,
                Is.EqualTo(0.07f + thumbProximalRadius).Within(0.0001f));
            Assert.That(
                thickness,
                Is.EqualTo(thumbProximalRadius * 2f).Within(0.0001f));
            Assert.That(boxSize.x, Is.EqualTo(width).Within(0.0001f));
            Assert.That(boxSize.y, Is.EqualTo(thickness).Within(0.0001f));
            Assert.That(boxSize.z, Is.EqualTo(length).Within(0.0001f));
            Assert.That(wristRadius * 2f, Is.EqualTo(width).Within(0.0001f));
            Assert.That(
                HumanoidIKGizmoGeometry.GetNearestHandDigitProximalSpacing(
                    2,
                    digitBases,
                    hasDigitBase,
                    displayRotation),
                Is.EqualTo(0.02f).Within(0.0001f));
        }

        [Test]
        public void GizmoGeometry_SyntheticHandDigitTipUsesDistalRotationWithoutEndTransform()
        {
            var previousPoint = Vector3.zero;
            var distalPoint = Vector3.forward;
            var referenceDistalFrame = Quaternion.identity;
            var posedDistalFrame = Quaternion.Euler(20f, 0f, 0f);

            var tip = HumanoidIKGizmoGeometry.GetSyntheticHandDigitTipPoint(
                previousPoint,
                distalPoint,
                referenceDistalFrame,
                posedDistalFrame);
            var tipDirection = (tip - distalPoint).normalized;

            Assert.That(
                Vector3.Distance(distalPoint, tip),
                Is.EqualTo(HumanoidIKGizmoGeometry.HandDigitTipLengthToPreviousSegment)
                    .Within(0.000001f));
            Assert.That(Vector3.Angle(Vector3.forward, tipDirection), Is.EqualTo(20f).Within(0.0001f));
        }

        [Test]
        public void GizmoGeometry_SimpleToeFootRootsStayForwardOfMappedToeBasePivot()
        {
            var scale = new Vector3(0.8f, 0.9f, 1.1f);
            var mappedToePivot = new Vector3(0.003f, -0.018f, 0.137f);

            foreach (var isLeftFoot in new[] { true, false })
            {
                var canonicalPivot = HumanoidIKGizmoGeometry.GetCanonicalToeBasePivot(isLeftFoot);
                for (var toeIndex = 0;
                     toeIndex < HumanoidIKGizmoGeometry.CanonicalToeCount;
                     toeIndex++)
                {
                    var canonicalToe = HumanoidIKGizmoGeometry.GetCanonicalToe(isLeftFoot, toeIndex);
                    var fittedBase = HumanoidIKGizmoGeometry.GetFittedToeGroupBase(
                        isLeftFoot,
                        toeIndex,
                        scale,
                        mappedToePivot);

                    Assert.That(fittedBase.y, Is.EqualTo(mappedToePivot.y).Within(0.000001f));
                    Assert.That(
                        fittedBase.x - mappedToePivot.x,
                        Is.EqualTo((canonicalToe.BasePosition.x - canonicalPivot.x) * scale.x)
                            .Within(0.000001f));
                    Assert.That(
                        fittedBase.z - mappedToePivot.z,
                        Is.EqualTo((canonicalToe.BasePosition.z - canonicalPivot.z) * scale.z)
                            .Within(0.000001f));
                    Assert.That(fittedBase.z, Is.GreaterThan(mappedToePivot.z));
                }
            }
        }

        [Test]
        public void GizmoGeometry_FittedToeBaseBendsAroundScaledSharedPivot()
        {
            var fit = new HumanoidIKCanonicalFootFit(new Vector3(0.9f, 0.8f, 0.7f));
            var pose = default(HumanoidIKDigitBendPose);
            var toe = fit.GetToe(true, 0);
            var pivot = fit.GetToeBasePivot(true);
            var neutral = new List<Vector3>();
            var bent = new List<Vector3>();

            HumanoidIKGizmoGeometry.BuildCanonicalToePoints(
                true,
                0,
                in toe,
                pivot,
                HumanoidIKToeRigKind.ArticulatedToes,
                in pose,
                0f,
                0f,
                neutral);
            HumanoidIKGizmoGeometry.BuildCanonicalToePoints(
                true,
                0,
                in toe,
                pivot,
                HumanoidIKToeRigKind.ArticulatedToes,
                in pose,
                1f,
                0f,
                bent);

            Assert.That(Vector3.Distance(neutral[0], pivot),
                Is.EqualTo(Vector3.Distance(bent[0], pivot)).Within(0.000001f));
            Assert.That(Vector3.Distance(neutral[neutral.Count - 1], bent[bent.Count - 1]),
                Is.GreaterThan(0.001f));
        }

        [Test]
        public void GizmoGeometry_CanonicalToesAlwaysMirrorAsFiveTwoSegmentChains()
        {
            var pose = default(HumanoidIKDigitBendPose);
            var leftPoints = new List<Vector3>();
            var rightPoints = new List<Vector3>();
            for (var toeIndex = 0;
                 toeIndex < HumanoidIKGizmoGeometry.CanonicalToeCount;
                 toeIndex++)
            {
                HumanoidIKGizmoGeometry.BuildCanonicalToePoints(
                    true,
                    toeIndex,
                    HumanoidIKToeRigKind.None,
                    in pose,
                    0f,
                    0f,
                    leftPoints);
                HumanoidIKGizmoGeometry.BuildCanonicalToePoints(
                    false,
                    toeIndex,
                    HumanoidIKToeRigKind.None,
                    in pose,
                    0f,
                    0f,
                    rightPoints);

                Assert.That(leftPoints.Count, Is.EqualTo(3));
                Assert.That(rightPoints.Count, Is.EqualTo(3));
                Assert.That(
                    Vector3.Distance(leftPoints[0], leftPoints[leftPoints.Count - 1]),
                    Is.EqualTo(
                        HumanoidIKGizmoGeometry.GetCanonicalToe(true, toeIndex).Length *
                        HumanoidIKGizmoGeometry.CanonicalToeDisplayLengthRatio)
                        .Within(0.000001f));
                for (var pointIndex = 0; pointIndex < leftPoints.Count; pointIndex++)
                {
                    Assert.That(leftPoints[pointIndex].x, Is.EqualTo(-rightPoints[pointIndex].x).Within(0.000001f));
                    Assert.That(leftPoints[pointIndex].y, Is.EqualTo(rightPoints[pointIndex].y).Within(0.000001f));
                    Assert.That(leftPoints[pointIndex].z, Is.EqualTo(rightPoints[pointIndex].z).Within(0.000001f));
                }
            }
        }

        [Test]
        public void GizmoGeometry_ToeRigChangesAnglesWithoutChangingCanonicalProportions()
        {
            var pose = new HumanoidIKDigitBendPose
            {
                thumbOrBigToe = new HumanoidIKJointBend
                {
                    proximal = new Vector3(12f, 3f, 0f)
                },
                littleOrFifthToe = new HumanoidIKJointBend
                {
                    proximal = new Vector3(-8f, -2f, 0f)
                }
            };
            var bigToePoints = new List<Vector3>();
            var littleToePoints = new List<Vector3>();
            HumanoidIKGizmoGeometry.BuildCanonicalToePoints(
                true,
                0,
                HumanoidIKToeRigKind.ToeFoot,
                in pose,
                0f,
                0f,
                bigToePoints);
            HumanoidIKGizmoGeometry.BuildCanonicalToePoints(
                true,
                4,
                HumanoidIKToeRigKind.ToeFoot,
                in pose,
                0f,
                0f,
                littleToePoints);

            var bigDirection = (bigToePoints[1] - bigToePoints[0]).normalized;
            var littleDirection = (littleToePoints[1] - littleToePoints[0]).normalized;
            Assert.That(Vector3.Dot(bigDirection, littleDirection), Is.GreaterThan(0.99999f));
            Assert.That(
                Mathf.Abs(bigToePoints[0].x - littleToePoints[0].x),
                Is.EqualTo(0.072f).Within(0.000001f));
            Assert.That(bigToePoints[0].z, Is.GreaterThan(littleToePoints[0].z));

            HumanoidIKGizmoGeometry.BuildCanonicalToePoints(
                true,
                0,
                HumanoidIKToeRigKind.ArticulatedToes,
                in pose,
                0f,
                1f,
                bigToePoints);
            HumanoidIKGizmoGeometry.BuildCanonicalToePoints(
                true,
                4,
                HumanoidIKToeRigKind.ArticulatedToes,
                in pose,
                0f,
                1f,
                littleToePoints);
            bigDirection = (bigToePoints[1] - bigToePoints[0]).normalized;
            littleDirection = (littleToePoints[1] - littleToePoints[0]).normalized;
            Assert.That(Vector3.Dot(bigDirection, littleDirection), Is.LessThan(0.999f));
        }

        [Test]
        public void GizmoGeometry_TwoSegmentToeUsesReferenceDirectionAndMergedDistalBend()
        {
            var direction = new Vector3(0.25f, 0f, 1f).normalized;
            var toe = new HumanoidIKCanonicalToe(
                Vector3.zero,
                direction,
                0.04f,
                0.003f);
            var pose = new HumanoidIKDigitBendPose
            {
                thumbOrBigToe = new HumanoidIKJointBend
                {
                    distal = new Vector3(20f, 0f, 0f)
                }
            };
            var points = new List<Vector3>();

            HumanoidIKGizmoGeometry.BuildCanonicalToePoints(
                true,
                0,
                in toe,
                Vector3.zero,
                HumanoidIKToeRigKind.ArticulatedToes,
                in pose,
                0f,
                0f,
                points);

            Assert.That(points.Count, Is.EqualTo(3));
            Assert.That(
                Vector3.Dot((points[1] - points[0]).normalized, direction),
                Is.GreaterThan(0.99999f));
            Assert.That(
                Vector3.Dot(
                    (points[2] - points[1]).normalized,
                    (points[1] - points[0]).normalized),
                Is.LessThan(0.999f));
        }
        [Test]
        public void GizmoGeometry_RigKindDoesNotChangeNeutralCanonicalShape()
        {
            var pose = default(HumanoidIKDigitBendPose);
            var nonePoints = new List<Vector3>();
            var simplePoints = new List<Vector3>();
            var articulatedPoints = new List<Vector3>();
            for (var toeIndex = 0;
                 toeIndex < HumanoidIKGizmoGeometry.CanonicalToeCount;
                 toeIndex++)
            {
                HumanoidIKGizmoGeometry.BuildCanonicalToePoints(
                    true,
                    toeIndex,
                    HumanoidIKToeRigKind.None,
                    in pose,
                    0f,
                    0f,
                    nonePoints);
                HumanoidIKGizmoGeometry.BuildCanonicalToePoints(
                    true,
                    toeIndex,
                    HumanoidIKToeRigKind.ToeFoot,
                    in pose,
                    0f,
                    0f,
                    simplePoints);
                HumanoidIKGizmoGeometry.BuildCanonicalToePoints(
                    true,
                    toeIndex,
                    HumanoidIKToeRigKind.ArticulatedToes,
                    in pose,
                    0f,
                    0f,
                    articulatedPoints);

                CollectionAssert.AreEqual(nonePoints, simplePoints);
                CollectionAssert.AreEqual(nonePoints, articulatedPoints);
            }
        }

        [Test]
        public void GizmoGeometry_ToeBaseBendsOnlyForefootAndCanonicalToes()
        {
            for (var slabIndex = 0;
                 slabIndex < HumanoidIKGizmoGeometry.CanonicalFootSlabCount;
                 slabIndex++)
            {
                HumanoidIKGizmoGeometry.GetCanonicalFootSlabPose(
                    true,
                    slabIndex,
                    HumanoidIKToeRigKind.ArticulatedToes,
                    1f,
                    out var slab,
                    out var center,
                    out var rotation);

                if (slab.BendsAtToeBase)
                {
                    Assert.That(
                        Mathf.Abs(Quaternion.Dot(rotation, Quaternion.identity)),
                        Is.LessThan(0.999f));
                    Assert.That(
                        Vector3.Distance(center, slab.Center),
                        Is.GreaterThan(0.0001f));
                }
                else
                {
                    Assert.That(rotation, Is.EqualTo(Quaternion.identity));
                    Assert.That(center, Is.EqualTo(slab.Center));
                }
            }

            var pose = default(HumanoidIKDigitBendPose);
            var neutral = new List<Vector3>();
            var bent = new List<Vector3>();
            for (var toeIndex = 0;
                 toeIndex < HumanoidIKGizmoGeometry.CanonicalToeCount;
                 toeIndex++)
            {
                HumanoidIKGizmoGeometry.BuildCanonicalToePoints(
                    true,
                    toeIndex,
                    HumanoidIKToeRigKind.ArticulatedToes,
                    in pose,
                    0f,
                    0f,
                    neutral);
                HumanoidIKGizmoGeometry.BuildCanonicalToePoints(
                    true,
                    toeIndex,
                    HumanoidIKToeRigKind.ArticulatedToes,
                    in pose,
                    1f,
                    0f,
                    bent);
                Assert.That(
                    Vector3.Distance(neutral[neutral.Count - 1], bent[bent.Count - 1]),
                    Is.GreaterThan(0.001f));
            }

            HumanoidIKGizmoGeometry.GetFittedToeBridgeSlabPose(
                true,
                null,
                HumanoidIKToeRigKind.ArticulatedToes,
                in pose,
                1f,
                out var bridge,
                out var bridgeCenter,
                out var bridgeRotation);
            Assert.That(bridge.BendsAtToeBase, Is.True);
            Assert.That(
                Mathf.Abs(Quaternion.Dot(bridgeRotation, Quaternion.identity)),
                Is.LessThan(0.999f));
            Assert.That(
                Vector3.Distance(bridgeCenter, bridge.Center),
                Is.GreaterThan(0.0001f));
        }

        [Test]
        public void GizmoGeometry_ToeBridgeFillsToeBaseGapAndMirrors()
        {
            var left = HumanoidIKGizmoGeometry.GetCanonicalToeBridgeSlab(true);
            var right = HumanoidIKGizmoGeometry.GetCanonicalToeBridgeSlab(false);
            var leftPivot = HumanoidIKGizmoGeometry.GetCanonicalToeBasePivot(true);
            var leftForefoot = HumanoidIKGizmoGeometry.GetCanonicalFootSlab(
                true,
                HumanoidIKGizmoGeometry.CanonicalFootSlabCount - 1);
            var rootZSum = 0f;
            for (var toeIndex = 0;
                 toeIndex < HumanoidIKGizmoGeometry.CanonicalToeCount;
                 toeIndex++)
            {
                rootZSum += HumanoidIKGizmoGeometry
                    .GetCanonicalToe(true, toeIndex)
                    .BasePosition.z;
            }

            var averageRootZ =
                rootZSum / HumanoidIKGizmoGeometry.CanonicalToeCount;
            Assert.That(
                left.Center.z - left.Size.z * 0.5f,
                Is.EqualTo(leftPivot.z).Within(0.000001f));
            Assert.That(
                left.Center.z + left.Size.z * 0.5f,
                Is.EqualTo(averageRootZ).Within(0.000001f));
            Assert.That(left.Size.x, Is.EqualTo(leftForefoot.Size.x));
            Assert.That(left.Size.y, Is.EqualTo(leftForefoot.Size.y));
            Assert.That(
                left.Center.x,
                Is.EqualTo(-right.Center.x).Within(0.000001f));
            Assert.That(left.Center.y, Is.EqualTo(right.Center.y).Within(0.000001f));
            Assert.That(left.Center.z, Is.EqualTo(right.Center.z).Within(0.000001f));
            Assert.That(left.Size, Is.EqualTo(right.Size));
        }

        [Test]
        public void GizmoGeometry_SimpleToeStretchMovesBridgeAndToeRootsAsOneGroup()
        {
            var pose = new HumanoidIKDigitBendPose
            {
                thumbOrBigToe = new HumanoidIKJointBend
                {
                    proximal = new Vector3(20f, 0f, 0f)
                }
            };
            var pivot = HumanoidIKGizmoGeometry.GetCanonicalToeBasePivot(true);
            var neutralPoints = new List<Vector3>();
            var bentPoints = new List<Vector3>();
            HumanoidIKGizmoGeometry.BuildCanonicalToePoints(
                true,
                0,
                HumanoidIKToeRigKind.None,
                in pose,
                0f,
                0f,
                neutralPoints);
            HumanoidIKGizmoGeometry.BuildCanonicalToePoints(
                true,
                0,
                HumanoidIKToeRigKind.ToeFoot,
                in pose,
                0f,
                0f,
                bentPoints);
            HumanoidIKGizmoGeometry.GetFittedToeBridgeSlabPose(
                true,
                null,
                HumanoidIKToeRigKind.ToeFoot,
                in pose,
                0f,
                out var bridge,
                out var bridgeCenter,
                out var bridgeRotation);

            Assert.That(
                Mathf.Abs(Quaternion.Dot(bridgeRotation, Quaternion.identity)),
                Is.LessThan(0.999f));
            Assert.That(
                bentPoints[0],
                Is.EqualTo(
                    HumanoidIKGizmoGeometry.RotatePointAroundPivot(
                        neutralPoints[0],
                        pivot,
                        bridgeRotation)));
            Assert.That(
                bridgeCenter,
                Is.EqualTo(
                    HumanoidIKGizmoGeometry.RotatePointAroundPivot(
                        bridge.Center,
                        pivot,
                        bridgeRotation)));
            Assert.That(
                Vector3.Distance(bentPoints[0], pivot),
                Is.EqualTo(Vector3.Distance(neutralPoints[0], pivot))
                    .Within(0.000001f));
            var bentDirection =
                (bentPoints[1] - bentPoints[0]).normalized;
            var expectedDirection =
                bridgeRotation *
                (neutralPoints[1] - neutralPoints[0]).normalized;
            Assert.That(
                Vector3.Dot(bentDirection, expectedDirection),
                Is.GreaterThan(0.99999f));
        }

        [Test]
        public void GizmoGeometry_CanonicalToeSizeUsesRequestedLengthAndRadiusMultipliers()
        {
            var bigToe = HumanoidIKGizmoGeometry.GetCanonicalToe(true, 0);
            var secondToe = HumanoidIKGizmoGeometry.GetCanonicalToe(true, 1);
            var middleToe = HumanoidIKGizmoGeometry.GetCanonicalToe(true, 2);
            var fourthToe = HumanoidIKGizmoGeometry.GetCanonicalToe(true, 3);
            var littleToe = HumanoidIKGizmoGeometry.GetCanonicalToe(true, 4);
            var toeBasePivot = HumanoidIKGizmoGeometry.GetCanonicalToeBasePivot(true);

            Assert.That(bigToe.Length, Is.EqualTo(0.051f).Within(0.000001f));
            Assert.That(littleToe.Length, Is.EqualTo(0.039f).Within(0.000001f));
            Assert.That(bigToe.Radius * 2f, Is.EqualTo(0.01224f).Within(0.000001f));
            Assert.That(secondToe.Radius * 2f, Is.EqualTo(0.006912f).Within(0.000001f));
            Assert.That(middleToe.Radius * 2f, Is.EqualTo(0.00648f).Within(0.000001f));
            Assert.That(fourthToe.Radius * 2f, Is.EqualTo(0.006048f).Within(0.000001f));
            Assert.That(littleToe.Radius * 2f, Is.EqualTo(0.005616f).Within(0.000001f));
            Assert.That(
                HumanoidIKGizmoGeometry.GetCanonicalToeDiameterToLength(1),
                Is.EqualTo(
                    HumanoidIKGizmoGeometry.CanonicalToeDiameterToLength * 0.6f)
                    .Within(0.000001f));
            Assert.That(
                bigToe.BasePosition.x - secondToe.BasePosition.x,
                Is.EqualTo(0.022f).Within(0.000001f));
            Assert.That(
                secondToe.BasePosition.x - middleToe.BasePosition.x,
                Is.EqualTo(0.017f).Within(0.000001f));
            Assert.That(
                middleToe.BasePosition.x - fourthToe.BasePosition.x,
                Is.EqualTo(0.016f).Within(0.000001f));
            Assert.That(
                fourthToe.BasePosition.x - littleToe.BasePosition.x,
                Is.EqualTo(0.017f).Within(0.000001f));
            Assert.That(littleToe.Radius, Is.LessThan(bigToe.Radius));
            Assert.That(
                bigToe.BasePosition.y - bigToe.Radius,
                Is.EqualTo(HumanoidIKGizmoGeometry.CanonicalFootSoleBottom)
                    .Within(0.000001f));
            Assert.That(
                bigToe.BasePosition.z - toeBasePivot.z,
                Is.EqualTo(HumanoidIKGizmoGeometry.CanonicalToeRootForwardOffsetFromBase)
                    .Within(0.000001f));
            Assert.That(bigToe.BasePosition.z, Is.GreaterThan(secondToe.BasePosition.z));
            Assert.That(secondToe.BasePosition.z, Is.GreaterThan(middleToe.BasePosition.z));
            Assert.That(middleToe.BasePosition.z, Is.GreaterThan(fourthToe.BasePosition.z));
            Assert.That(fourthToe.BasePosition.z, Is.GreaterThan(littleToe.BasePosition.z));
            Assert.That(littleToe.BasePosition.z, Is.GreaterThan(toeBasePivot.z));
        }

        [Test]
        public void GizmoGeometry_FittedToeLengthStaysNearCanonicalProportion()
        {
            const float defaultLength = 0.045f;

            Assert.That(
                HumanoidIKGizmoGeometry.GetFittedToeLength(0.001f, defaultLength),
                Is.EqualTo(
                    defaultLength *
                    HumanoidIKGizmoGeometry.FittedToeMinimumLengthScale)
                    .Within(0.000001f));
            Assert.That(
                HumanoidIKGizmoGeometry.GetFittedToeLength(1f, defaultLength),
                Is.EqualTo(
                    defaultLength *
                    HumanoidIKGizmoGeometry.FittedToeMaximumLengthScale)
                    .Within(0.000001f));
            Assert.That(
                HumanoidIKGizmoGeometry.GetFittedToeLength(0f, defaultLength),
                Is.EqualTo(defaultLength).Within(0.000001f));
            Assert.That(
                HumanoidIKGizmoGeometry.GetFittedToeLength(0.035f, defaultLength),
                Is.EqualTo(
                    0.035f * HumanoidIKGizmoGeometry.FittedToeLengthMultiplier)
                    .Within(0.000001f));
        }

        [Test]
        public void ToeBaseBendAngle_UsesProximalToeRangeAndClampsPose()
        {
            Assert.That(HumanoidIKUtility.GetToeBaseBendAngle(-1f), Is.EqualTo(-25f));
            Assert.That(HumanoidIKUtility.GetToeBaseBendAngle(0f), Is.Zero);
            Assert.That(HumanoidIKUtility.GetToeBaseBendAngle(1f), Is.EqualTo(20f));
            Assert.That(HumanoidIKUtility.GetToeBaseBendAngle(2f), Is.EqualTo(20f));
        }

        [Test]
        public void PrimitiveRenderer_QueuesCommandsOnlyForRepaint()
        {
            var renderer = new HumanoidIKPrimitiveRenderer();

            renderer.BeginFrame(EventType.Layout);
            renderer.DrawSphere(Vector3.zero, 1f, Color.red);
            Assert.That(renderer.PendingCommandCount, Is.Zero);

            renderer.BeginFrame(EventType.Repaint);
            renderer.DrawSphere(Vector3.zero, 1f, Color.red);
            Assert.That(renderer.PendingCommandCount, Is.EqualTo(1));
            renderer.CancelFrame();
        }

        [Test]
        public void PrimitiveRenderer_GroupsCommandsByMeshAndColorBeforeFlush()
        {
            var renderer = new HumanoidIKPrimitiveRenderer();
            var translucentRed = new Color(1f, 0f, 0f, 0.5f);

            renderer.BeginFrame(EventType.Repaint);
            renderer.DrawBox(Vector3.zero, Quaternion.identity, Vector3.one, Color.red);
            renderer.DrawSphere(Vector3.one, 0.5f, Color.red);
            renderer.DrawSphere(Vector3.up, 0.4f, Color.red);
            renderer.DrawSphere(Vector3.right, 0.25f, translucentRed);

            Assert.That(renderer.PendingCommandCount, Is.EqualTo(4));
            Assert.That(renderer.PendingColorBatchCount, Is.EqualTo(2));
            Assert.That(renderer.PendingDrawBatchCount, Is.EqualTo(3));
            renderer.CancelFrame();
        }

        [Test]
        public void PrimitiveRenderer_UsesRequestedRadiusForBuiltInRoundMeshes()
        {
            var renderer = new HumanoidIKPrimitiveRenderer();
            try
            {
                renderer.BeginFrame(EventType.Repaint);
                renderer.DrawCylinder(Vector3.zero, Vector3.up * 2f, 0.3f, Color.red);
                renderer.DrawSphere(Vector3.zero, 0.4f, Color.red);

                var commandsField = typeof(HumanoidIKPrimitiveRenderer).GetField(
                    "_commands",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic);
                Assert.That(commandsField, Is.Not.Null);

                var commands = commandsField.GetValue(renderer) as System.Collections.IList;
                Assert.That(commands, Is.Not.Null);
                Assert.That(commands.Count, Is.EqualTo(2));

                var matrixField = commands[0].GetType().GetField("Matrix");
                Assert.That(matrixField, Is.Not.Null);

                var cylinderMatrix = (Matrix4x4)matrixField.GetValue(commands[0]);
                var sphereMatrix = (Matrix4x4)matrixField.GetValue(commands[1]);

                Assert.That(cylinderMatrix.lossyScale.x, Is.EqualTo(0.3f).Within(0.000001f));
                Assert.That(cylinderMatrix.lossyScale.y, Is.EqualTo(1f).Within(0.000001f));
                Assert.That(cylinderMatrix.lossyScale.z, Is.EqualTo(0.3f).Within(0.000001f));
                Assert.That(sphereMatrix.lossyScale.x, Is.EqualTo(0.4f).Within(0.000001f));
                Assert.That(sphereMatrix.lossyScale.y, Is.EqualTo(0.4f).Within(0.000001f));
                Assert.That(sphereMatrix.lossyScale.z, Is.EqualTo(0.4f).Within(0.000001f));
            }
            finally
            {
                renderer.Dispose();
            }
        }

        [Test]
        public void GizmoDrawer_DefaultFootQueuesSoleSlabsAfterBindingCacheReset()
        {
            var drawer = new HumanoidIKGizmoDrawer();
            try
            {
                drawer.ClearPreviewContexts();
                drawer.BeginFrame(EventType.Repaint);
                drawer.DrawDefaultEndShape(
                    HumanoidIKTarget.LeftFoot,
                    Vector3.zero,
                    Quaternion.identity,
                    Color.blue);

                Assert.That(
                    drawer.PendingBoxCommandCount,
                    Is.EqualTo(
                        HumanoidIKGizmoGeometry.CanonicalFootSlabCount + 1));
            }
            finally
            {
                drawer.Dispose();
            }
        }

        [Test]
        public void TimelineGizmoRegistry_SelectedClipKeepsFullOpacityAndOthersUseHalf()
        {
            Assert.That(
                HumanoidIKTimelineGizmoRegistry.GetOpacityMultiplier(isSelected: true),
                Is.EqualTo(1f));
            Assert.That(
                HumanoidIKTimelineGizmoRegistry.GetOpacityMultiplier(isSelected: false),
                Is.EqualTo(0.5f));
        }

        [Test]
        public void TimelineGizmoRegistry_ClipVisibilityExpiresAfterGuiStopsDrawing()
        {
            const double lastSeen = 10d;
            Assert.That(
                HumanoidIKTimelineGizmoRegistry.IsWithinVisibilityWindow(lastSeen, lastSeen),
                Is.True);
            Assert.That(
                HumanoidIKTimelineGizmoRegistry.IsWithinVisibilityWindow(
                    lastSeen + HumanoidIKTimelineGizmoRegistry.VisibilityTimeout,
                    lastSeen),
                Is.True);
            Assert.That(
                HumanoidIKTimelineGizmoRegistry.IsWithinVisibilityWindow(
                    lastSeen + HumanoidIKTimelineGizmoRegistry.VisibilityTimeout + 0.001d,
                    lastSeen),
                Is.False);
        }

        [Test]
        public void TimelineGizmoRegistry_FrameBoundsCenterOnIKTargetAtGizmoScale()
        {
            var targetPosition = new Vector3(1.25f, -0.5f, 3.75f);

            var bounds = HumanoidIKTimelineGizmoRegistry.GetTargetFrameBounds(targetPosition);

            Assert.That(bounds.center, Is.EqualTo(targetPosition));
            Assert.That(
                bounds.size,
                Is.EqualTo(
                    Vector3.one * HumanoidIKTimelineGizmoRegistry.TargetFrameBoundsSize));
        }

        [Test]
        public void TimelineGizmoRegistry_HandlesFrameSelectedExecuteCommandOnly()
        {
            var executeCommand = new Event
            {
                type = EventType.ExecuteCommand,
                commandName = "FrameSelected"
            };
            var layoutCommand = new Event
            {
                type = EventType.Layout,
                commandName = "FrameSelected"
            };
            var remappedKeyEvent = new Event
            {
                type = EventType.KeyDown,
                keyCode = KeyCode.G
            };

            Assert.That(
                HumanoidIKTimelineGizmoRegistry.IsFrameSelectedCommand(executeCommand),
                Is.True);
            Assert.That(
                HumanoidIKTimelineGizmoRegistry.IsFrameSelectedCommand(layoutCommand),
                Is.False);
            Assert.That(
                HumanoidIKTimelineGizmoRegistry.IsFrameSelectedCommand(remappedKeyEvent),
                Is.False);
        }


        [Test]
        public void TrackIconProvider_UsesTargetSpecificIconsAndCombinedFallback()
        {
            var leftHand = HumanoidIKTrackIconProvider.GetIcon(HumanoidIKTarget.LeftHand);
            var rightHand = HumanoidIKTrackIconProvider.GetIcon(HumanoidIKTarget.RightHand);
            var leftFoot = HumanoidIKTrackIconProvider.GetIcon(HumanoidIKTarget.LeftFoot);
            var rightFoot = HumanoidIKTrackIconProvider.GetIcon(HumanoidIKTarget.RightFoot);
            var combined = HumanoidIKTrackIconProvider.GetIcon((HumanoidIKTarget)int.MaxValue);

            Assert.That(leftHand, Is.Not.Null);
            Assert.That(rightHand, Is.Not.Null);
            Assert.That(leftFoot, Is.Not.Null);
            Assert.That(rightFoot, Is.Not.Null);
            Assert.That(combined, Is.Not.Null);
            Assert.That(rightHand, Is.Not.SameAs(leftHand));
            Assert.That(leftFoot, Is.Not.SameAs(rightFoot));
            Assert.That(leftFoot, Is.Not.SameAs(leftHand));
            Assert.That(leftHand.name, Is.EqualTo("Humanoid IK Track Icon - Hand"));
            Assert.That(rightFoot.name, Is.EqualTo("Humanoid IK Track Icon - Foot"));
            Assert.That(combined.name, Is.EqualTo("Humanoid IK Track Icon"));
        }

        [Test]
        public void DuplicateTargetValidation_FindsOnlySameAnimatorAndTarget()
        {
            var timeline = Track(ScriptableObject.CreateInstance<TimelineAsset>());
            var leftTrack = timeline.CreateTrack<HumanoidIKTrack>(null, "Left Hand A");
            var duplicateTrack = timeline.CreateTrack<HumanoidIKTrack>(null, "Left Hand B");
            var rightTrack = timeline.CreateTrack<HumanoidIKTrack>(null, "Right Hand");
            duplicateTrack.target = HumanoidIKTarget.LeftHand;
            rightTrack.target = HumanoidIKTarget.RightHand;

            var directorObject = Track(new GameObject("Director"));
            var director = directorObject.AddComponent<PlayableDirector>();
            director.playableAsset = timeline;
            var animatorObject = Track(new GameObject("Animator"));
            var animator = animatorObject.AddComponent<Animator>();
            director.SetGenericBinding(leftTrack, animator);
            director.SetGenericBinding(duplicateTrack, animator);
            director.SetGenericBinding(rightTrack, animator);

            Assert.That(
                HumanoidIKTrackValidation.TryFindDuplicateTarget(
                    leftTrack,
                    animator,
                    director,
                    out var duplicate),
                Is.True);
            Assert.That(duplicate, Is.EqualTo(duplicateTrack));

            duplicateTrack.target = HumanoidIKTarget.RightFoot;
            Assert.That(
                HumanoidIKTrackValidation.TryFindDuplicateTarget(
                    leftTrack,
                    animator,
                    director,
                    out _),
                Is.False);
        }


        [Test]
        public void TimelineDriver_RetainsCachesAcrossInactiveGapUntilOwnerReleases()
        {
            var root = Track(new GameObject("IK Driver"));
            var animator = root.AddComponent<Animator>();
            var driver = HumanoidIKLateUpdateDriver.GetOrCreate(animator);

            driver.SetState(
                HumanoidIKTarget.LeftHand,
                new HumanoidIKGoalState { Active = true });
            driver.ClearState(HumanoidIKTarget.LeftHand);

            Assert.That(driver, Is.Not.Null);
            Assert.That(driver.enabled, Is.False);
            Assert.That(driver.TimelineOwnerCount, Is.EqualTo(1));

            driver.ReleaseTimelineOwner();

            Assert.That(driver == null, Is.True);
            Assert.That(
                root.GetComponent<HumanoidIKLateUpdateDriver>(),
                Is.Null);
        }

        [Test]
        public void MuscleBinding_CachesIndexAndRangesForAngleConversion()
        {
            var binding = HumanoidIKMuscleBinding.Create(
                HumanBodyBones.LeftIndexProximal,
                2);

            Assert.That(binding.IsValid, Is.True);
            Assert.That(binding.Index, Is.GreaterThanOrEqualTo(0));
            Assert.That(binding.NegativeRange, Is.GreaterThan(0f));
            Assert.That(binding.PositiveRange, Is.GreaterThan(0f));
            Assert.That(
                binding.GetValue(-binding.NegativeRange * 0.5f),
                Is.EqualTo(-0.5f).Within(0.0001f));
            Assert.That(
                binding.GetValue(binding.PositiveRange * 0.5f),
                Is.EqualTo(0.5f).Within(0.0001f));
        }
        T Track<T>(T value) where T : Object
        {
            _objectsToDestroy.Add(value);
            return value;
        }

        static SkeletonBone Bone(string name, Vector3 position)
        {
            return new SkeletonBone
            {
                name = name,
                position = position,
                rotation = Quaternion.identity,
                scale = Vector3.one
            };
        }
    }
}
