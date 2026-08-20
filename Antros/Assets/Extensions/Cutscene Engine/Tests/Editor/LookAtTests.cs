using System.Collections.Generic;
using System.Linq;
using CutsceneEngine;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace CutsceneEngineEditor.Tests
{
    public sealed class LookAtTests
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
        public void StateEvaluation_BlendsTargetAndWeightPerBodyPart()
        {
            var firstPosition = new Vector3(0f, 1f, 2f);
            var secondPosition = new Vector3(10f, 5f, -2f);

            var state = new LookAtState
            {
                Active = true,
                Samples = new[]
                {
                    new LookAtSample
                    {
                        Position = firstPosition,
                        TimelineWeight = 0.75f,
                        EyesWeight = 1f,
                        HeadWeight = 0f,
                        NeckWeight = 0.5f,
                        BodyWeight = 0f
                    },
                    new LookAtSample
                    {
                        Position = secondPosition,
                        TimelineWeight = 0.25f,
                        EyesWeight = 0f,
                        HeadWeight = 1f,
                        NeckWeight = 0.5f,
                        BodyWeight = 1f
                    }
                },
                SampleCount = 2
            };

            Assert.That(
                LookAtUtility.TryEvaluateState(in state, out var evaluated),
                Is.True);
            Assert.That(evaluated.Eyes.TargetPosition, Is.EqualTo(firstPosition));
            Assert.That(evaluated.Eyes.Weight, Is.EqualTo(0.75f).Within(0.0001f));
            Assert.That(evaluated.Head.TargetPosition, Is.EqualTo(secondPosition));
            Assert.That(evaluated.Head.Weight, Is.EqualTo(0.25f).Within(0.0001f));
            Assert.That(
                Vector3.Distance(
                    evaluated.Neck.TargetPosition,
                    Vector3.Lerp(firstPosition, secondPosition, 0.25f)),
                Is.LessThan(0.0001f));
            Assert.That(evaluated.Neck.Weight, Is.EqualTo(0.5f).Within(0.0001f));
            Assert.That(evaluated.Body.TargetPosition, Is.EqualTo(secondPosition));
            Assert.That(evaluated.Body.Weight, Is.EqualTo(0.25f).Within(0.0001f));
        }

        [Test]
        public void StateEvaluation_BlendsAngleLimitsPerBodyPart()
        {
            var state = new LookAtState
            {
                Active = true,
                Samples = new[]
                {
                    new LookAtSample
                    {
                        Position = Vector3.forward,
                        TimelineWeight = 0.25f,
                        HeadWeight = 1f,
                        HeadAngleLimits = new LookAtAngleLimits(
                            new Vector2(-80f, 20f),
                            new Vector2(-40f, 10f))
                    },
                    new LookAtSample
                    {
                        Position = Vector3.forward,
                        TimelineWeight = 0.75f,
                        HeadWeight = 1f,
                        HeadAngleLimits = new LookAtAngleLimits(
                            new Vector2(-20f, 60f),
                            new Vector2(-10f, 50f))
                    }
                },
                SampleCount = 2
            };

            Assert.That(
                LookAtUtility.TryEvaluateState(in state, out var evaluated),
                Is.True);
            Assert.That(
                evaluated.Head.AngleLimits.horizontal.x,
                Is.EqualTo(-35f).Within(0.0001f));
            Assert.That(
                evaluated.Head.AngleLimits.horizontal.y,
                Is.EqualTo(50f).Within(0.0001f));
            Assert.That(
                evaluated.Head.AngleLimits.vertical.x,
                Is.EqualTo(-17.5f).Within(0.0001f));
            Assert.That(
                evaluated.Head.AngleLimits.vertical.y,
                Is.EqualTo(40f).Within(0.0001f));
        }

        [Test]
        public void StateEvaluation_BlendsChinOffsetForHeadOnly()
        {
            var state = new LookAtState
            {
                Active = true,
                Samples = new[]
                {
                    new LookAtSample
                    {
                        Position = Vector3.forward,
                        TimelineWeight = 0.25f,
                        EyesWeight = 1f,
                        HeadWeight = 1f,
                        ChinOffset = -1f
                    },
                    new LookAtSample
                    {
                        Position = Vector3.forward,
                        TimelineWeight = 0.75f,
                        EyesWeight = 1f,
                        HeadWeight = 1f,
                        ChinOffset = 0.5f
                    }
                },
                SampleCount = 2
            };

            Assert.That(
                LookAtUtility.TryEvaluateState(in state, out var evaluated),
                Is.True);
            Assert.That(
                evaluated.Head.PitchOffsetDegrees,
                Is.EqualTo(3.75f).Within(0.0001f));
            Assert.That(evaluated.Eyes.PitchOffsetDegrees, Is.EqualTo(0f));
        }

        [Test]
        public void ClampTargetDirection_ClampsHorizontalAndVerticalAxes()
        {
            var referenceRotation = Quaternion.Euler(12f, 35f, -7f);
            var yawRadians = 80f * Mathf.Deg2Rad;
            var pitchRadians = 40f * Mathf.Deg2Rad;
            var targetLocal = new Vector3(
                Mathf.Sin(yawRadians) * Mathf.Cos(pitchRadians),
                Mathf.Sin(pitchRadians),
                Mathf.Cos(yawRadians) * Mathf.Cos(pitchRadians));
            var clampedDirection = LookAtUtility.ClampTargetDirection(
                referenceRotation * Vector3.forward,
                referenceRotation * targetLocal,
                referenceRotation,
                new LookAtAngleLimits(
                    new Vector2(-30f, 30f),
                    new Vector2(-10f, 20f)));

            LookAtUtility.GetYawPitch(
                Quaternion.Inverse(referenceRotation) * clampedDirection,
                out var yaw,
                out var pitch);

            Assert.That(yaw, Is.EqualTo(30f).Within(0.001f));
            Assert.That(pitch, Is.EqualTo(20f).Within(0.001f));
        }

        [TestCase(-1f, -30f)]
        [TestCase(1f, 30f)]
        public void ClampTargetDirection_AppliesChinOffsetBeforeLimits(
            float chinOffset,
            float expectedPitch)
        {
            var clampedDirection = LookAtUtility.ClampTargetDirection(
                Vector3.forward,
                Vector3.forward,
                Quaternion.identity,
                LookAtAngleLimits.Unrestricted,
                LookAtUtility.SanitizeChinOffset(chinOffset) *
                LookAtClip.MaximumChinPitchOffsetDegrees);

            LookAtUtility.GetYawPitch(
                clampedDirection,
                out var yaw,
                out var pitch);

            Assert.That(yaw, Is.EqualTo(0f).Within(0.001f));
            Assert.That(pitch, Is.EqualTo(expectedPitch).Within(0.001f));
        }

        [Test]
        public void UninitializedAngleLimits_AreUnrestrictedForExistingClips()
        {
            var limits = default(LookAtAngleLimits).Sanitized();

            Assert.That(
                limits.horizontal,
                Is.EqualTo(new Vector2(
                    LookAtAngleLimits.MinimumAngle,
                    LookAtAngleLimits.MaximumAngle)));
            Assert.That(
                limits.vertical,
                Is.EqualTo(new Vector2(
                    LookAtAngleLimits.MinimumAngle,
                    LookAtAngleLimits.MaximumAngle)));
        }

        [Test]
        public void StateEvaluation_BlendsDirectorLocalAndTransformTargets()
        {
            var director = Track(new GameObject("Director"));
            director.transform.SetPositionAndRotation(
                new Vector3(3f, -2f, 5f),
                Quaternion.Euler(12f, 70f, -8f));
            director.transform.localScale = new Vector3(2f, 3f, 4f);

            var transformTarget = Track(new GameObject("Transform Target"));
            transformTarget.transform.position = new Vector3(-4f, 2f, 7f);

            var localTarget = new LookAtBehaviour
            {
                directorTransform = director.transform,
                position = new Vector3(0.5f, 1.25f, 2f)
            };
            var transformTargetBehaviour = new LookAtBehaviour
            {
                target = transformTarget.transform,
                directorTransform = director.transform,
                position = new Vector3(99f, 99f, 99f)
            };
            var localWorldPosition = director.transform.TransformPoint(localTarget.position);

            var state = new LookAtState
            {
                Active = true,
                Samples = new[]
                {
                    new LookAtSample
                    {
                        DirectorTransform = localTarget.directorTransform,
                        Position = localTarget.position,
                        TimelineWeight = 0.25f,
                        HeadWeight = 1f
                    },
                    new LookAtSample
                    {
                        Target = transformTargetBehaviour.target,
                        DirectorTransform = transformTargetBehaviour.directorTransform,
                        Position = transformTargetBehaviour.position,
                        TimelineWeight = 0.75f,
                        HeadWeight = 1f
                    }
                },
                SampleCount = 2
            };

            Assert.That(
                LookAtUtility.TryEvaluateState(in state, out var evaluated),
                Is.True);
            Assert.That(
                Vector3.Distance(
                    evaluated.Head.TargetPosition,
                    Vector3.Lerp(localWorldPosition, transformTarget.transform.position, 0.75f)),
                Is.LessThan(0.0001f));
            Assert.That(evaluated.Head.Weight, Is.EqualTo(1f).Within(0.0001f));
        }

        [Test]
        public void StateEvaluation_UsesLatestTransformPositionWithoutTimelineReevaluation()
        {
            var target = Track(new GameObject("Live Target"));
            var initialPosition = new Vector3(1f, 2f, 3f);
            var movedPosition = new Vector3(-4f, 5f, 6f);
            target.transform.position = initialPosition;

            var state = new LookAtState
            {
                Active = true,
                Samples = new[]
                {
                    new LookAtSample
                    {
                        Target = target.transform,
                        TimelineWeight = 1f,
                        HeadWeight = 1f
                    }
                },
                SampleCount = 1
            };

            Assert.That(
                LookAtUtility.TryEvaluateState(in state, out var initialState),
                Is.True);
            Assert.That(initialState.Head.TargetPosition, Is.EqualTo(initialPosition));

            target.transform.position = movedPosition;

            Assert.That(
                LookAtUtility.TryEvaluateState(in state, out var movedState),
                Is.True);
            Assert.That(movedState.Head.TargetPosition, Is.EqualTo(movedPosition));
        }

        [Test]
        public void Driver_MovingStoredTargetQueuesEditorApplyWithoutTimelineReevaluation()
        {
            var host = Track(new GameObject("Look At Driver"));
            var target = Track(new GameObject("Live Target"));
            var driver = host.AddComponent<LookAtLateUpdateDriver>();
            driver.SetState(new LookAtState
            {
                Active = true,
                Samples = new[]
                {
                    new LookAtSample
                    {
                        Target = target.transform,
                        TimelineWeight = 1f,
                        HeadWeight = 1f
                    }
                },
                SampleCount = 1
            });

            Assert.That(driver.EditorApplyPending, Is.True);
            driver.ApplyPendingEditorState();
            Assert.That(driver.EditorApplyPending, Is.False);

            target.transform.position = Vector3.right;

            Assert.That(driver.RefreshEditorInputs(), Is.True);
            Assert.That(driver.EditorApplyPending, Is.True);
        }

        [Test]
        public void StateEvaluation_UsesLatestSourceClipPositionWithoutGraphRebuild()
        {
            var clip = Track(ScriptableObject.CreateInstance<LookAtClip>());
            clip.position = new Vector3(1f, 2f, 3f);
            clip.eyesWeight = 0f;
            clip.headWeight = 1f;
            clip.neckWeight = 0f;
            clip.bodyWeight = 0f;

            var state = new LookAtState
            {
                Active = true,
                Samples = new[]
                {
                    new LookAtSample
                    {
                        SourceClip = clip,
                        Position = new Vector3(99f, 99f, 99f),
                        TimelineWeight = 1f
                    }
                },
                SampleCount = 1
            };

            Assert.That(
                LookAtUtility.TryEvaluateState(in state, out var initialState),
                Is.True);
            Assert.That(initialState.Head.TargetPosition, Is.EqualTo(clip.position));

            clip.position = new Vector3(-4f, 5f, 6f);

            Assert.That(
                LookAtUtility.TryEvaluateState(in state, out var movedState),
                Is.True);
            Assert.That(movedState.Head.TargetPosition, Is.EqualTo(clip.position));
        }

        [Test]
        public void PreviewUpdater_OnlyTreatsEffectiveLookAtClipAsActiveAtCurrentTime()
        {
            var timeline = Track(
                ScriptableObject.CreateInstance<TimelineAsset>());
            var track = timeline.CreateTrack<LookAtTrack>();
            var timelineClip = track.CreateClip<LookAtClip>();
            timelineClip.start = 2d;
            timelineClip.duration = 3d;

            Assert.That(
                LookAtTimelinePreviewUpdater.IsClipActiveAtTime(
                    timelineClip,
                    1.999d),
                Is.False);
            Assert.That(
                LookAtTimelinePreviewUpdater.IsClipActiveAtTime(
                    timelineClip,
                    2d),
                Is.True);
            Assert.That(
                LookAtTimelinePreviewUpdater.IsClipActiveAtTime(
                    timelineClip,
                    4.999d),
                Is.True);
            Assert.That(
                LookAtTimelinePreviewUpdater.IsClipActiveAtTime(
                    timelineClip,
                    5d),
                Is.False);
            Assert.That(
                LookAtTimelinePreviewUpdater.IsClipActiveAtTime(
                    timelineClip,
                    5.001d),
                Is.False);

            var clip = (LookAtClip)timelineClip.asset;
            clip.eyesWeight = 0f;
            clip.headWeight = 0f;
            clip.neckWeight = 0f;
            clip.bodyWeight = 0f;
            clip.blinkFrequency = 0f;
            clip.upperEyelidFollowWeight = 0f;
            clip.lowerEyelidFollowWeight = 0f;
            clip.horizontalEyelidFollowWeight = 0f;

            Assert.That(
                LookAtTimelinePreviewUpdater.IsClipActiveAtTime(
                    timelineClip,
                    3d),
                Is.False);

            clip.blinkBlendShapeKeys =
                new[] { "Eye_Blink_L", "Eye_Blink_R" };
            clip.blinkFrequency = 0.5f;
            Assert.That(
                LookAtTimelinePreviewUpdater.IsClipActiveAtTime(
                    timelineClip,
                    3d),
                Is.True);

            clip.blinkFrequency = 0f;
            Assert.That(
                LookAtTimelinePreviewUpdater.IsClipActiveAtTime(
                    timelineClip,
                    3d),
                Is.False);

            clip.upperEyelidFollowBlendShapeKeys =
                new[] { "UpperLidFollow" };
            clip.upperEyelidFollowWeight = 0.5f;
            Assert.That(
                LookAtTimelinePreviewUpdater.IsClipActiveAtTime(
                    timelineClip,
                    3d),
                Is.True);

            clip.upperEyelidFollowBlendShapeKeys =
                System.Array.Empty<string>();
            Assert.That(
                LookAtTimelinePreviewUpdater.IsClipActiveAtTime(
                    timelineClip,
                    3d),
                Is.False);

            clip.lowerEyelidFollowBlendShapeKeys =
                new[] { "LowerLidFollow" };
            clip.lowerEyelidFollowWeight = 0.5f;
            Assert.That(
                LookAtTimelinePreviewUpdater.IsClipActiveAtTime(
                    timelineClip,
                    3d),
                Is.True);

            clip.lowerEyelidFollowBlendShapeKeys =
                System.Array.Empty<string>();
            clip.horizontalEyelidFollowBlendShapeKeys =
                new[] { "Eye_Look_Left", "Eye_Look_Right" };
            clip.horizontalEyelidFollowWeight = 0.5f;
            Assert.That(
                LookAtTimelinePreviewUpdater.IsClipActiveAtTime(
                    timelineClip,
                    3d),
                Is.True);
        }

        [Test]
        public void BodyWeight_IsDistributedWithoutChangingCombinedStrength()
        {
            const float requestedWeight = 0.65f;
            const int boneCount = 3;
            var combinedWeight = 0f;
            for (var i = 0; i < boneCount; i++)
            {
                combinedWeight += LookAtUtility.GetGradualBoneWeight(requestedWeight, i, boneCount);
            }

            Assert.That(combinedWeight, Is.EqualTo(requestedWeight).Within(0.0001f));
        }

        [Test]
        public void ReferenceForward_MapsCharacterForwardIntoImportedBoneFrame()
        {
            var rootToBoneRotation = Quaternion.Euler(37f, -64f, 18f);
            var forwardInBone = LookAtUtility.GetForwardInBone(rootToBoneRotation);
            var resolvedRootForward = rootToBoneRotation * forwardInBone;

            Assert.That(
                Vector3.Distance(resolvedRootForward, Vector3.forward),
                Is.LessThan(0.0001f));
        }

        [Test]
        public void GenericRig_AutoDetectsHeadParentChainAndNamedEyes()
        {
            var root = Track(new GameObject("Character"));
            var animator = root.AddComponent<Animator>();
            var hips = CreateChild(root.transform, "Hips");
            var spine = CreateChild(hips, "Spine");
            var chest = CreateChild(spine, "Chest");
            var neck = CreateChild(chest, "Neck");
            var head = CreateChild(neck, "Head");
            CreateChild(head, "Head_End");
            var leftEye = CreateChild(
                head,
                "Eye_L",
                new Vector3(-0.05f, 0.05f, 0.1f));
            var rightEye = CreateChild(
                head,
                "Eye_R",
                new Vector3(0.05f, 0.05f, 0.1f));
            CreateChild(head, "Upper_Eyelid_L");
            CreateChild(head, "Ear_L");
            var jaw = CreateChild(head, "Jaw");
            CreateChild(jaw, "Tongue");
            var track = Track(
                ScriptableObject.CreateInstance<LookAtTrack>());

            Assert.That(
                LookAtGenericRigUtility.DetectHead(animator),
                Is.SameAs(head));
            Assert.That(
                LookAtGenericRigUtility.DetectPelvis(animator, head),
                Is.SameAs(hips));
            Assert.That(
                LookAtGenericRigUtility.TryResolve(
                    animator,
                    track,
                    out var rig),
                Is.True);
            Assert.That(rig.Head, Is.SameAs(head));
            Assert.That(rig.Neck, Is.SameAs(neck));
            Assert.That(
                rig.Body,
                Is.EqualTo(new[] { spine, chest }));
            Assert.That(rig.LeftEye, Is.SameAs(leftEye));
            Assert.That(rig.RightEye, Is.SameAs(rightEye));
            Assert.That(
                System.Array.IndexOf(rig.Body, hips),
                Is.EqualTo(-1));
        }

        [TestCase("L Eye", "R Eye")]
        [TestCase("Eye-L", "Eye.R")]
        [TestCase("left_eye", "right.eye")]
        public void GenericRig_DetectsNamedEyeSidesBeforePositionFallback(
            string leftEyeName,
            string rightEyeName)
        {
            var root = Track(new GameObject("Prefix Eye Creature"));
            var animator = root.AddComponent<Animator>();
            var neck = CreateChild(root.transform, "Neck");
            var head = CreateChild(neck, "Head");
            var leftEye = CreateChild(
                head,
                leftEyeName,
                new Vector3(0.1f, 0.1f, 0f));
            var rightEye = CreateChild(
                head,
                rightEyeName,
                new Vector3(0.2f, 0.1f, 0f));
            var track = Track(
                ScriptableObject.CreateInstance<LookAtTrack>());

            Assert.That(
                LookAtGenericRigUtility.TryResolve(
                    animator,
                    track,
                    out var rig),
                Is.True);
            Assert.That(rig.LeftEye, Is.SameAs(leftEye));
            Assert.That(rig.RightEye, Is.SameAs(rightEye));
        }

        [Test]
        public void GenericRig_DetectsLowestSpineAsPelvisFallback()
        {
            var root = Track(new GameObject("Creature"));
            var animator = root.AddComponent<Animator>();
            var rig = CreateChild(root.transform, "Rig");
            var pelvis = CreateChild(rig, "DEF-spine.004");
            var lowerBody = CreateChild(pelvis, "DEF-spine.006");
            var upperBody = CreateChild(lowerBody, "DEF-spine.007");
            var neck = CreateChild(upperBody, "DEF-spine.008");
            var head = CreateChild(neck, "Head");
            var track = Track(
                ScriptableObject.CreateInstance<LookAtTrack>());

            Assert.That(
                LookAtGenericRigUtility.DetectPelvis(animator, head),
                Is.SameAs(pelvis));
            Assert.That(
                LookAtGenericRigUtility.TryResolve(
                    animator,
                    track,
                    out var definition),
                Is.True);
            Assert.That(definition.Body, Is.EqualTo(new[] { lowerBody, upperBody }));
            Assert.That(definition.Neck, Is.SameAs(neck));
        }

        [Test]
        public void GenericRig_SameTrackUsesEachBoundAnimatorMapping()
        {
            var firstRoot = Track(new GameObject("First Creature"));
            var firstAnimator = firstRoot.AddComponent<Animator>();
            var firstNeck = CreateChild(firstRoot.transform, "JointA");
            var firstHead = CreateChild(firstNeck, "CraniumA");
            var firstMapping =
                firstRoot.AddComponent<LookAtGenericRigMapping>();
            firstMapping.initialized = true;
            firstMapping.head = firstHead;
            firstMapping.bodyBones = new[] { firstNeck };

            var secondRoot = Track(new GameObject("Second Creature"));
            var secondAnimator = secondRoot.AddComponent<Animator>();
            var secondPelvis = CreateChild(secondRoot.transform, "Hips");
            var secondNeck = CreateChild(secondPelvis, "NeckB");
            var secondHead = CreateChild(secondNeck, "CreatureHeadB");
            var track = Track(
                ScriptableObject.CreateInstance<LookAtTrack>());

            Assert.That(
                LookAtGenericRigUtility.TryResolve(
                    firstAnimator,
                    track,
                    out var firstRig),
                Is.True);
            Assert.That(firstRig.Head, Is.SameAs(firstHead));
            Assert.That(firstRig.Neck, Is.SameAs(firstNeck));

            Assert.That(
                LookAtGenericRigUtility.TryResolve(
                    secondAnimator,
                    track,
                    out var secondRig),
                Is.True);
            Assert.That(secondRig.Head, Is.SameAs(secondHead));
            Assert.That(secondRig.Neck, Is.SameAs(secondNeck));
        }

        [Test]
        public void GenericRig_MappedBonesResolveUnnamedChain()
        {
            var root = Track(new GameObject("Creature"));
            var animator = root.AddComponent<Animator>();
            var baseBone = CreateChild(root.transform, "JointA");
            var body = CreateChild(baseBone, "JointB");
            var neck = CreateChild(body, "JointC");
            var head = CreateChild(neck, "Cranium");
            var leftEye = CreateChild(head, "Eye_L");
            var rightEye = CreateChild(head, "Eye_R");
            var track = Track(
                ScriptableObject.CreateInstance<LookAtTrack>());
            var mapping = root.AddComponent<LookAtGenericRigMapping>();
            mapping.initialized = true;
            mapping.pelvis = baseBone;
            mapping.head = head;
            mapping.bodyBones = new[] { body, neck };
            mapping.leftEye = leftEye;
            mapping.rightEye = rightEye;

            Assert.That(
                LookAtGenericRigUtility.TryResolve(
                    animator,
                    track,
                    out var rig),
                Is.True);
            Assert.That(rig.Head, Is.SameAs(head));
            Assert.That(rig.Body, Is.EqualTo(new[] { body }));
            Assert.That(rig.Neck, Is.SameAs(neck));
            Assert.That(rig.LeftEye, Is.SameAs(leftEye));
            Assert.That(rig.RightEye, Is.SameAs(rightEye));
        }

        [Test]
        public void GenericRig_StoredBonesReplaceAutomaticDerivedBones()
        {
            var root = Track(new GameObject("Manual Generic"));
            var animator = root.AddComponent<Animator>();
            var pelvis = CreateChild(root.transform, "Pelvis");
            var spine = CreateChild(pelvis, "Spine");
            var chest = CreateChild(spine, "Chest");
            var automaticNeck = CreateChild(chest, "Neck");
            var head = CreateChild(automaticNeck, "Head");
            CreateChild(head, "Eye_L");
            CreateChild(head, "Eye_R");
            var manualLeftEye = CreateChild(head, "Optic_L");
            var manualRightEye = CreateChild(head, "Optic_R");
            var track = Track(
                ScriptableObject.CreateInstance<LookAtTrack>());
            var mapping = root.AddComponent<LookAtGenericRigMapping>();
            mapping.initialized = true;
            mapping.pelvis = pelvis;
            mapping.head = head;
            mapping.bodyBones = new[] { spine, chest };
            mapping.leftEye = manualLeftEye;
            mapping.rightEye = manualRightEye;

            Assert.That(
                LookAtGenericRigUtility.TryResolve(
                    animator,
                    track,
                    out var rig),
                Is.True);
            Assert.That(rig.Head, Is.SameAs(head));
            Assert.That(rig.Body, Is.EqualTo(new[] { spine }));
            Assert.That(rig.Neck, Is.SameAs(chest));
            Assert.That(rig.LeftEye, Is.SameAs(manualLeftEye));
            Assert.That(rig.RightEye, Is.SameAs(manualRightEye));
        }

        [Test]
        public void GenericRig_EmptyStoredBonesAreIgnored()
        {
            var root = Track(new GameObject("Manual Ignore"));
            var animator = root.AddComponent<Animator>();
            var pelvis = CreateChild(root.transform, "Pelvis");
            var spine = CreateChild(pelvis, "Spine");
            var neck = CreateChild(spine, "Neck");
            var head = CreateChild(neck, "Head");
            CreateChild(head, "Eye_L");
            CreateChild(head, "Eye_R");
            var track = Track(
                ScriptableObject.CreateInstance<LookAtTrack>());
            var mapping = root.AddComponent<LookAtGenericRigMapping>();
            mapping.initialized = true;
            mapping.pelvis = pelvis;
            mapping.head = head;

            Assert.That(
                LookAtGenericRigUtility.TryResolve(
                    animator,
                    track,
                    out var rig),
                Is.True);
            Assert.That(rig.Head, Is.SameAs(head));
            Assert.That(rig.Body, Is.Empty);
            Assert.That(rig.Neck, Is.Null);
            Assert.That(rig.LeftEye, Is.Null);
            Assert.That(rig.RightEye, Is.Null);
        }

        [Test]
        public void GenericRig_EmptyFieldsRemainIgnoredAfterInitialization()
        {
            var root = Track(new GameObject("Generic Ignore"));
            var animator = root.AddComponent<Animator>();
            var pelvis = CreateChild(root.transform, "Pelvis");
            var neck = CreateChild(pelvis, "Neck");
            var head = CreateChild(neck, "Head");
            var track = Track(
                ScriptableObject.CreateInstance<LookAtTrack>());
            var mapping = root.AddComponent<LookAtGenericRigMapping>();
            mapping.initialized = true;

            Assert.That(
                LookAtGenericRigUtility.TryResolve(
                    animator,
                    track,
                    out _),
                Is.False);

            mapping.head = head;
            Assert.That(
                LookAtGenericRigUtility.TryResolve(
                    animator,
                    track,
                    out var rig),
                Is.True);
            Assert.That(rig.Head, Is.SameAs(head));
            Assert.That(rig.Neck, Is.Null);
            Assert.That(rig.Body, Is.Empty);
        }

        [Test]
        public void GenericReferenceForward_UsesAnimatorLocalPositiveZ()
        {
            var root = Track(new GameObject("Generic Forward"));
            root.transform.rotation = Quaternion.Euler(11f, 73f, -8f);
            var animator = root.AddComponent<Animator>();
            var head = CreateChild(root.transform, "Head");
            head.localRotation = Quaternion.Euler(34f, -51f, 19f);
            var referencePose = new LookAtGenericReferencePose(animator);

            Assert.That(
                referencePose.TryGetRootToBoneRotation(
                    head,
                    out var rootToHeadRotation),
                Is.True);
            var forwardInHead =
                LookAtUtility.GetForwardInBone(rootToHeadRotation);

            Assert.That(
                Vector3.Distance(
                    head.TransformDirection(forwardInHead),
                    root.transform.forward),
                Is.LessThan(0.0001f));
        }

        [Test]
        public void GenericReferencePose_PrefersMeshBindPoseOverCurrentRotation()
        {
            var root = Track(new GameObject("Generic Bind Pose"));
            var animator = root.AddComponent<Animator>();
            var head = CreateChild(root.transform, "Head");
            head.localRotation = Quaternion.Euler(0f, 80f, 0f);
            var bindRotation = Quaternion.Euler(23f, -37f, 12f);
            var mesh = Track(new Mesh());
            mesh.vertices = new[] { Vector3.zero };
            mesh.bindposes = new[]
            {
                Matrix4x4.TRS(
                    Vector3.zero,
                    bindRotation,
                    Vector3.one).inverse
            };
            var renderer = root.AddComponent<SkinnedMeshRenderer>();
            renderer.sharedMesh = mesh;
            renderer.bones = new[] { head };
            var referencePose = new LookAtGenericReferencePose(animator);

            Assert.That(
                referencePose.TryGetRootToBoneRotation(
                    head,
                    out var resolvedRotation),
                Is.True);
            Assert.That(
                Quaternion.Angle(resolvedRotation, bindRotation),
                Is.LessThan(0.001f));
        }

        [Test]
        public void GenericReferencePose_AlignsBindSpaceThroughRendererRootBone()
        {
            var root = Track(new GameObject("Generic Root Alignment"));
            var animator = root.AddComponent<Animator>();
            var skeletonRoot = CreateChild(root.transform, "Skeleton Root");
            var rootToSkeletonRotation = Quaternion.Euler(270f, 90f, 0f);
            skeletonRoot.localRotation = rootToSkeletonRotation;

            var head = CreateChild(skeletonRoot, "Head");
            var expectedRootToHead =
                Quaternion.FromToRotation(Vector3.up, Vector3.forward);
            head.localRotation =
                Quaternion.Inverse(rootToSkeletonRotation) *
                expectedRootToHead;

            var bindSpaceToSkeleton = Quaternion.Euler(0f, 0f, 90f);
            var rootToBindSpace =
                rootToSkeletonRotation *
                Quaternion.Inverse(bindSpaceToSkeleton);
            var bindSpaceToHead =
                Quaternion.Inverse(rootToBindSpace) *
                expectedRootToHead;
            var mesh = Track(new Mesh());
            mesh.vertices = new[] { Vector3.zero };
            mesh.bindposes = new[]
            {
                Matrix4x4.TRS(
                    Vector3.zero,
                    bindSpaceToHead,
                    Vector3.one).inverse,
                Matrix4x4.TRS(
                    Vector3.zero,
                    bindSpaceToSkeleton,
                    Vector3.one).inverse
            };
            var renderer = root.AddComponent<SkinnedMeshRenderer>();
            renderer.sharedMesh = mesh;
            renderer.bones = new[] { head, skeletonRoot };
            renderer.rootBone = skeletonRoot;
            var referencePose = new LookAtGenericReferencePose(animator);

            Assert.That(
                referencePose.TryGetRootToBoneRotation(
                    head,
                    out var resolvedRotation),
                Is.True);
            Assert.That(
                Quaternion.Angle(
                    resolvedRotation,
                    expectedRootToHead),
                Is.LessThan(0.001f));

            var forwardInHead =
                LookAtUtility.GetForwardInBone(resolvedRotation);
            Assert.That(
                Vector3.Distance(forwardInHead, Vector3.up),
                Is.LessThan(0.0001f));
        }

        [Test]
        public void GenericDriver_RotatesHeadFromReferencePositiveZ()
        {
            var root = Track(new GameObject("Generic Driver"));
            var animator = root.AddComponent<Animator>();
            var head = CreateChild(
                root.transform,
                "Head",
                new Vector3(0f, 1f, 0f));
            head.localRotation = Quaternion.Euler(20f, -45f, 10f);
            var track = Track(
                ScriptableObject.CreateInstance<LookAtTrack>());
            var referencePose = new LookAtGenericReferencePose(animator);
            Assert.That(
                referencePose.TryGetRootToBoneRotation(
                    head,
                    out var rootToHeadRotation),
                Is.True);
            var forwardInHead =
                LookAtUtility.GetForwardInBone(rootToHeadRotation);
            var targetDirection = root.transform.right;
            var driver = root.AddComponent<LookAtLateUpdateDriver>();
            driver.SetState(new LookAtState
            {
                Active = true,
                SourceTrack = track,
                Samples = new[]
                {
                    new LookAtSample
                    {
                        Position = head.position + targetDirection * 10f,
                        TimelineWeight = 1f,
                        EyesWeight = 0f,
                        HeadWeight = 1f,
                        NeckWeight = 0f,
                        BodyWeight = 0f,
                        HeadAngleLimits = LookAtAngleLimits.Unrestricted
                    }
                },
                SampleCount = 1
            });

            Assert.That(driver.ApplyPendingEditorState(), Is.True);
            Assert.That(
                Vector3.Dot(
                    head.TransformDirection(forwardInHead).normalized,
                    targetDirection),
                Is.GreaterThan(0.999f));
        }

        [Test]
        public void GenericDriver_RebuildsRigWhenManualHeadMappingChanges()
        {
            var root = Track(new GameObject("Generic Remap"));
            var animator = root.AddComponent<Animator>();
            var firstHead = CreateChild(
                root.transform,
                "HeadA",
                new Vector3(0f, 1f, 0f));
            var secondHead = CreateChild(
                root.transform,
                "HeadB",
                new Vector3(0f, 1f, 0f));
            var track = Track(
                ScriptableObject.CreateInstance<LookAtTrack>());
            var mapping = root.AddComponent<LookAtGenericRigMapping>();
            mapping.initialized = true;
            mapping.head = firstHead;
            var driver = root.AddComponent<LookAtLateUpdateDriver>();
            driver.SetState(new LookAtState
            {
                Active = true,
                SourceTrack = track,
                Samples = new[]
                {
                    new LookAtSample
                    {
                        Position = firstHead.position + Vector3.right * 10f,
                        TimelineWeight = 1f,
                        EyesWeight = 0f,
                        HeadWeight = 1f,
                        NeckWeight = 0f,
                        BodyWeight = 0f,
                        HeadAngleLimits = LookAtAngleLimits.Unrestricted
                    }
                },
                SampleCount = 1
            });

            Assert.That(driver.ApplyPendingEditorState(), Is.True);
            Assert.That(
                Vector3.Dot(firstHead.forward, Vector3.right),
                Is.GreaterThan(0.999f));
            Assert.That(
                Vector3.Dot(secondHead.forward, Vector3.forward),
                Is.GreaterThan(0.999f));

            mapping.head = secondHead;
            driver.RequestEditorApply();

            Assert.That(driver.ApplyPendingEditorState(), Is.True);
            Assert.That(
                Vector3.Dot(firstHead.forward, Vector3.forward),
                Is.GreaterThan(0.999f));
            Assert.That(
                Vector3.Dot(secondHead.forward, Vector3.right),
                Is.GreaterThan(0.999f));
        }

        [Test]
        public void TrackMixer_TransportsSourceTrackForGenericRigResolution()
        {
            var graph = PlayableGraph.Create(
                "Look At Generic Track Context Test");
            try
            {
                var track = Track(
                    ScriptableObject.CreateInstance<LookAtTrack>());
                var playable =
                    (ScriptPlayable<LookAtMixerBehaviour>)
                    track.CreateTrackMixer(
                        graph,
                        null,
                        0);

                Assert.That(
                    playable.GetBehaviour().sourceTrack,
                    Is.SameAs(track));
            }
            finally
            {
                graph.Destroy();
            }
        }

        [Test]
        public void EyeForwardDefault_IsStoredInDirectorLocalSpace()
        {
            var director = Track(new GameObject("Director"));
            director.transform.SetPositionAndRotation(
                new Vector3(8f, -3f, 2f),
                Quaternion.Euler(15f, 110f, -7f));
            director.transform.localScale = new Vector3(1.5f, 2f, 0.75f);

            var eyeCenterLocal = new Vector3(0.4f, 1.6f, -0.25f);
            var eyeCenterWorld = director.transform.TransformPoint(eyeCenterLocal);
            var initialPosition = LookAtUtility.GetEyeForwardLocalPosition(
                director.transform,
                eyeCenterWorld);

            Assert.That(
                Vector3.Distance(initialPosition, eyeCenterLocal + Vector3.forward),
                Is.LessThan(0.0001f));
            Assert.That(
                LookAtUtility.GetEyeForwardLocalPosition(null, eyeCenterWorld),
                Is.EqualTo(LookAtClip.DefaultLocalPosition));
        }

        [Test]
        public void ChinOffset_UsesSignedSliderAndSlightlyLoweredDefault()
        {
            var clip = Track(
                ScriptableObject.CreateInstance<LookAtClip>());
            var field = typeof(LookAtClip).GetField(
                nameof(LookAtClip.chinOffset));
            var range = field
                .GetCustomAttributes(
                    typeof(UnityEngine.RangeAttribute),
                    false)
                .Cast<UnityEngine.RangeAttribute>()
                .Single();

            Assert.That(range.min, Is.EqualTo(-1f));
            Assert.That(range.max, Is.EqualTo(1f));
            Assert.That(
                clip.chinOffset,
                Is.EqualTo(LookAtClip.DefaultChinOffset));
        }

        [Test]
        public void NewClip_UsesAuthoredDefaults()
        {
            var clip = Track(
                ScriptableObject.CreateInstance<LookAtClip>());

            Assert.That(clip.eyesWeight, Is.EqualTo(1f));
            Assert.That(clip.headWeight, Is.EqualTo(0.5f));
            Assert.That(clip.neckWeight, Is.EqualTo(0.2f));
            Assert.That(clip.bodyWeight, Is.EqualTo(0.05f));
            Assert.That(
                clip.chinOffset,
                Is.EqualTo(LookAtClip.DefaultChinOffset));

            Assert.That(
                clip.eyesAngleLimits.horizontal,
                Is.EqualTo(new Vector2(-40f, 40f)));
            Assert.That(
                clip.eyesAngleLimits.vertical,
                Is.EqualTo(new Vector2(-25f, 25f)));
            Assert.That(
                clip.headAngleLimits.horizontal,
                Is.EqualTo(new Vector2(-90f, 90f)));
            Assert.That(
                clip.headAngleLimits.vertical,
                Is.EqualTo(new Vector2(-90f, 90f)));
            Assert.That(
                clip.neckAngleLimits.horizontal,
                Is.EqualTo(new Vector2(-90f, 90f)));
            Assert.That(
                clip.neckAngleLimits.vertical,
                Is.EqualTo(new Vector2(-90f, 90f)));
            Assert.That(
                clip.bodyAngleLimits.horizontal,
                Is.EqualTo(new Vector2(-90f, 90f)));
            Assert.That(
                clip.bodyAngleLimits.vertical,
                Is.EqualTo(new Vector2(-90f, 90f)));

            Assert.That(
                clip.blinkBlendShapeKeys,
                Is.EqualTo(new[] { "Eye_Blink_L", "Eye_Blink_R" }));
            Assert.That(
                clip.upperEyelidFollowBlendShapeKeys,
                Is.EqualTo(new[]
                {
                    "Eye_Lid_Upper_L",
                    "Eye_Lid_Upper_R"
                }));
            Assert.That(
                clip.lowerEyelidFollowBlendShapeKeys,
                Is.EqualTo(new[]
                {
                    "Eye_Lid_Lower_L",
                    "Eye_Lid_Lower_R"
                }));
            Assert.That(
                clip.horizontalEyelidFollowBlendShapeKeys,
                Is.EqualTo(new[]
                {
                    "Eye_L_Look_L",
                    "Eye_R_Look_L",
                    "Eye_L_Look_R",
                    "Eye_R_Look_R"
                }));
            Assert.That(
                clip.upperEyelidFollowBlendShapeKeys,
                Is.Not.SameAs(clip.blinkBlendShapeKeys));
            Assert.That(
                clip.lowerEyelidFollowBlendShapeKeys,
                Is.Not.SameAs(clip.blinkBlendShapeKeys));
            Assert.That(
                clip.lowerEyelidFollowBlendShapeKeys,
                Is.Not.SameAs(
                    clip.upperEyelidFollowBlendShapeKeys));

            Assert.That(
                clip.blinkMode,
                Is.EqualTo(LookAtBlinkMode.Automatic));
            Assert.That(clip.blinkFrequency, Is.EqualTo(0.5f));
            Assert.That(clip.blinkDuration, Is.EqualTo(0.185f));
            Assert.That(clip.blinkNoiseOffset, Is.EqualTo(0f));

            Assert.That(
                clip.upperEyelidFollowWeight,
                Is.EqualTo(0.5f));
            Assert.That(
                clip.upperEyelidFollowCurve,
                Is.Not.Null);
            Assert.That(
                clip.upperEyelidFollowCurve.length,
                Is.EqualTo(3));
            Assert.That(
                clip.upperEyelidFollowCurve.keys[0].value,
                Is.EqualTo(1f));
            Assert.That(
                clip.upperEyelidFollowCurve.keys[1].time,
                Is.EqualTo(0.45f));
            Assert.That(
                clip.upperEyelidFollowCurve.keys[1].value,
                Is.EqualTo(0f));
            Assert.That(
                clip.upperEyelidFollowCurve.keys[2].value,
                Is.EqualTo(0f));

            Assert.That(
                clip.lowerEyelidFollowWeight,
                Is.EqualTo(0.5f));
            Assert.That(
                clip.lowerEyelidFollowCurve,
                Is.Not.Null);
            Assert.That(
                clip.lowerEyelidFollowCurve.length,
                Is.EqualTo(3));
            Assert.That(
                clip.lowerEyelidFollowCurve.keys[0].value,
                Is.EqualTo(0f));
            Assert.That(
                clip.lowerEyelidFollowCurve.keys[1].time,
                Is.EqualTo(0.55f));
            Assert.That(
                clip.lowerEyelidFollowCurve.keys[1].value,
                Is.EqualTo(0f));
            Assert.That(
                clip.lowerEyelidFollowCurve.keys[2].value,
                Is.EqualTo(1f));

            Assert.That(
                clip.horizontalEyelidFollowWeight,
                Is.EqualTo(0.5f));
            Assert.That(
                clip.horizontalEyelidFollowCurve,
                Is.Not.Null);
            Assert.That(
                clip.horizontalEyelidFollowCurve.length,
                Is.EqualTo(4));
            Assert.That(
                clip.horizontalEyelidFollowCurve.keys[0].value,
                Is.EqualTo(1f));
            Assert.That(
                clip.horizontalEyelidFollowCurve.keys[1].time,
                Is.EqualTo(0.45f));
            Assert.That(
                clip.horizontalEyelidFollowCurve.keys[1].value,
                Is.EqualTo(0f));
            Assert.That(
                clip.horizontalEyelidFollowCurve.keys[2].time,
                Is.EqualTo(0.55f));
            Assert.That(
                clip.horizontalEyelidFollowCurve.keys[2].value,
                Is.EqualTo(0f));
            Assert.That(
                clip.horizontalEyelidFollowCurve.keys[3].value,
                Is.EqualTo(1f));

            Assert.That(clip.blinkCurve, Is.Not.Null);
            Assert.That(clip.blinkCurve.length, Is.EqualTo(4));
            Assert.That(clip.automaticBlinkCurve, Is.Not.Null);
            Assert.That(
                clip.automaticBlinkCurve.length,
                Is.EqualTo(3));
            Assert.That(
                clip.automaticBlinkCurve.keys[0].value,
                Is.EqualTo(1f));
            Assert.That(
                clip.automaticBlinkCurve.keys[0].outTangent,
                Is.EqualTo(0f));
            Assert.That(
                clip.automaticBlinkCurve.keys[1].time,
                Is.EqualTo(0.38f));
            Assert.That(
                clip.automaticBlinkCurve.keys[1].value,
                Is.EqualTo(0.1f).Within(0.0001f));
            Assert.That(
                clip.automaticBlinkCurve.keys[1].inTangent,
                Is.LessThan(0f));
            Assert.That(
                clip.automaticBlinkCurve.keys[1].outTangent,
                Is.GreaterThan(0f));
            Assert.That(
                clip.automaticBlinkCurve.keys[2].value,
                Is.EqualTo(1f));
            Assert.That(
                clip.automaticBlinkCurve.keys[2].inTangent,
                Is.EqualTo(0f));
        }

        [Test]
        public void ClipPlayable_CopiesLookAtBlinkAndEyelidSettings()
        {
            var graph = PlayableGraph.Create("Look At Test");
            try
            {
                var owner = Track(new GameObject("Owner"));
                var lookTarget = Track(new GameObject("Target"));
                var clip = Track(
                    ScriptableObject.CreateInstance<LookAtClip>());
                clip.target = new ExposedReference<Transform>
                {
                    defaultValue = lookTarget.transform
                };
                clip.eyesWeight = 1.5f;
                clip.headWeight = 0.8f;
                clip.neckWeight = -0.5f;
                clip.bodyWeight = 0.2f;
                clip.chinOffset = -1.5f;
                clip.eyesAngleLimits = new LookAtAngleLimits(
                    new Vector2(-240f, 30f),
                    new Vector2(50f, -25f));
                clip.blinkBlendShapeKeys = new[]
                {
                    "Blink_L",
                    "Blink_L_Extra",
                    "Blink_R"
                };
                clip.blinkMode =
                    LookAtBlinkMode.AnimationCurve;
                clip.blinkCurve =
                    AnimationCurve.Linear(0f, 0f, 1f, 1f);
                clip.blinkFrequency = 1.5f;
                clip.blinkDuration = 1f;
                clip.automaticBlinkCurve =
                    AnimationCurve.Linear(0f, 0f, 1f, 1f);
                clip.blinkNoiseOffset = 12.5f;

                clip.upperEyelidFollowBlendShapeKeys = new[]
                {
                    "UpperLidFollow_L",
                    "UpperLidFollow_R"
                };
                clip.upperEyelidFollowWeight = 1.5f;
                clip.upperEyelidFollowCurve =
                    AnimationCurve.Linear(0f, 1f, 1f, 0f);
                clip.lowerEyelidFollowBlendShapeKeys = new[]
                {
                    "LowerLidFollow_L",
                    "LowerLidFollow_R"
                };
                clip.lowerEyelidFollowWeight = 0.6f;
                clip.lowerEyelidFollowCurve =
                    AnimationCurve.Linear(0f, 0f, 1f, 1f);
                clip.horizontalEyelidFollowBlendShapeKeys = new[]
                {
                    "eye_look_left.L",
                    "eye_look_left.R",
                    "eye_look_right.L",
                    "eye_look_right.R"
                };
                clip.horizontalEyelidFollowWeight = 0.4f;
                clip.horizontalEyelidFollowCurve =
                    AnimationCurve.Linear(0f, 1f, 1f, 1f);

                var playable =
                    (ScriptPlayable<LookAtBehaviour>)
                    clip.CreatePlayable(graph, owner);
                var behaviour = playable.GetBehaviour();

                Assert.That(
                    behaviour.target,
                    Is.SameAs(lookTarget.transform));
                Assert.That(behaviour.eyesWeight, Is.EqualTo(1f));
                Assert.That(
                    behaviour.headWeight,
                    Is.EqualTo(0.8f).Within(0.0001f));
                Assert.That(behaviour.neckWeight, Is.EqualTo(0f));
                Assert.That(
                    behaviour.bodyWeight,
                    Is.EqualTo(0.2f).Within(0.0001f));
                Assert.That(behaviour.chinOffset, Is.EqualTo(-1f));
                Assert.That(
                    behaviour.eyesAngleLimits.horizontal,
                    Is.EqualTo(new Vector2(-180f, 30f)));
                Assert.That(
                    behaviour.eyesAngleLimits.vertical,
                    Is.EqualTo(new Vector2(-25f, 50f)));
                Assert.That(
                    behaviour.headAngleLimits.horizontal,
                    Is.EqualTo(new Vector2(-90f, 90f)));

                Assert.That(
                    behaviour.blinkBlendShapeKeys,
                    Is.SameAs(clip.blinkBlendShapeKeys));
                Assert.That(
                    behaviour.blinkMode,
                    Is.EqualTo(
                        LookAtBlinkMode.AnimationCurve));
                Assert.That(
                    behaviour.blinkCurve,
                    Is.SameAs(clip.blinkCurve));
                Assert.That(
                    behaviour.blinkFrequency,
                    Is.EqualTo(1f));
                Assert.That(
                    behaviour.blinkDuration,
                    Is.EqualTo(0.5f));
                Assert.That(
                    behaviour.automaticBlinkCurve,
                    Is.SameAs(clip.automaticBlinkCurve));
                Assert.That(
                    behaviour.blinkNoiseOffset,
                    Is.EqualTo(12.5f));

                Assert.That(
                    behaviour.upperEyelidFollowBlendShapeKeys,
                    Is.SameAs(
                        clip.upperEyelidFollowBlendShapeKeys));
                Assert.That(
                    behaviour.upperEyelidFollowWeight,
                    Is.EqualTo(1f));
                Assert.That(
                    behaviour.upperEyelidFollowCurve,
                    Is.SameAs(
                        clip.upperEyelidFollowCurve));
                Assert.That(
                    behaviour.lowerEyelidFollowBlendShapeKeys,
                    Is.SameAs(
                        clip.lowerEyelidFollowBlendShapeKeys));
                Assert.That(
                    behaviour.lowerEyelidFollowWeight,
                    Is.EqualTo(0.6f));
                Assert.That(
                    behaviour.lowerEyelidFollowCurve,
                    Is.SameAs(
                        clip.lowerEyelidFollowCurve));
                Assert.That(
                    behaviour.horizontalEyelidFollowBlendShapeKeys,
                    Is.SameAs(
                        clip.horizontalEyelidFollowBlendShapeKeys));
                Assert.That(
                    behaviour.horizontalEyelidFollowWeight,
                    Is.EqualTo(0.4f));
                Assert.That(
                    behaviour.horizontalEyelidFollowCurve,
                    Is.SameAs(
                        clip.horizontalEyelidFollowCurve));
                Assert.That(
                    behaviour.upperEyelidFollowKeyCache.Length,
                    Is.EqualTo(2));
                Assert.That(
                    behaviour.upperEyelidFollowKeyCache[0].Key,
                    Is.EqualTo("UpperLidFollow_L"));
                Assert.That(
                    behaviour.upperEyelidFollowKeyCache[0].Direction,
                    Is.EqualTo(LookAtEyelidDirection.Down));
                Assert.That(
                    behaviour.upperEyelidFollowKeyCache[0].Side,
                    Is.EqualTo(LookAtEyelidSide.Left));
                Assert.That(
                    behaviour.lowerEyelidFollowKeyCache[1].Direction,
                    Is.EqualTo(LookAtEyelidDirection.Up));
                Assert.That(
                    behaviour.lowerEyelidFollowKeyCache[1].Side,
                    Is.EqualTo(LookAtEyelidSide.Right));
                Assert.That(
                    behaviour.horizontalEyelidFollowKeyCache.Length,
                    Is.EqualTo(4));
                Assert.That(
                    behaviour.horizontalEyelidFollowKeyCache[0].Direction,
                    Is.EqualTo(LookAtEyelidDirection.Left));
                Assert.That(
                    behaviour.horizontalEyelidFollowKeyCache[0].Side,
                    Is.EqualTo(LookAtEyelidSide.Left));
                Assert.That(
                    behaviour.horizontalEyelidFollowKeyCache[1].Side,
                    Is.EqualTo(LookAtEyelidSide.Right));
                Assert.That(
                    behaviour.horizontalEyelidFollowKeyCache[2].Direction,
                    Is.EqualTo(LookAtEyelidDirection.Right));
                Assert.That(
                    behaviour.horizontalEyelidFollowKeyCache[3].Side,
                    Is.EqualTo(LookAtEyelidSide.Right));

                clip.upperEyelidFollowBlendShapeKeys =
                    new[] { "ChangedAfterGraphBuild" };
                clip.horizontalEyelidFollowBlendShapeKeys =
                    new[] { "ChangedHorizontalAfterGraphBuild" };
                var cachedSample = behaviour.CreateSample(1f, 0d, 1d);
                Assert.That(
                    cachedSample
                        .ResolveUpperEyelidFollowKeyCache()[0]
                        .Key,
                    Is.EqualTo("UpperLidFollow_L"));
                Assert.That(
                    cachedSample
                        .ResolveHorizontalEyelidFollowKeyCache()[0]
                        .Key,
                    Is.EqualTo("eye_look_left.L"));

                Assert.That(
                    clip.clipCaps,
                    Is.EqualTo(
                        ClipCaps.Blending |
                        ClipCaps.Extrapolation));
            }
            finally
            {
                graph.Destroy();
            }
        }

        [Test]
        public void ClipPlayable_UsesDirectorLocalPositionWhenTargetIsMissing()
        {
            var graph = PlayableGraph.Create("Look At Local Position Test");
            try
            {
                var owner = Track(new GameObject("Director"));
                owner.transform.SetPositionAndRotation(
                    new Vector3(4f, 2f, -3f),
                    Quaternion.Euler(0f, 90f, 0f));
                owner.transform.localScale = new Vector3(2f, 1.5f, 0.5f);

                var clip = Track(ScriptableObject.CreateInstance<LookAtClip>());
                clip.position = new Vector3(0.25f, 1.4f, 3f);

                var playable = (ScriptPlayable<LookAtBehaviour>)clip.CreatePlayable(graph, owner);
                var behaviour = playable.GetBehaviour();

                Assert.That(behaviour.target, Is.Null);
                Assert.That(behaviour.directorTransform, Is.SameAs(owner.transform));
                Assert.That(behaviour.position, Is.EqualTo(clip.position));
                Assert.That(
                    Vector3.Distance(
                        behaviour.ResolveTargetPosition(),
                        owner.transform.TransformPoint(clip.position)),
                    Is.LessThan(0.0001f));
            }
            finally
            {
                graph.Destroy();
            }
        }

        [Test]
        public void TimelineGizmoRegistry_SelectedClipIsMoreOpaqueThanVisibleUnselectedClip()
        {
            var selectedOpacity = LookAtTimelineGizmoRegistry.GetOpacityMultiplier(
                isSelected: true);
            var unselectedOpacity = LookAtTimelineGizmoRegistry.GetOpacityMultiplier(
                isSelected: false);

            Assert.That(selectedOpacity, Is.EqualTo(1f));
            Assert.That(unselectedOpacity, Is.EqualTo(0.35f));
            Assert.That(selectedOpacity, Is.GreaterThan(unselectedOpacity));
        }

        [Test]
        public void GizmoColor_DrivesTimelineAccentAndSceneOpacity()
        {
            var clip = Track(
                ScriptableObject.CreateInstance<LookAtClip>());
            clip.gizmoColor = new Color(0.1f, 0.35f, 0.8f, 0.6f);

            Assert.That(
                LookAtTimelineGizmoRegistry.ResolveGizmoColor(clip, 1f),
                Is.EqualTo(clip.gizmoColor));

            var unselectedColor = LookAtTimelineGizmoRegistry.ResolveGizmoColor(
                clip,
                LookAtTimelineGizmoRegistry.UnselectedOpacityMultiplier);
            Assert.That(unselectedColor.r, Is.EqualTo(clip.gizmoColor.r));
            Assert.That(unselectedColor.g, Is.EqualTo(clip.gizmoColor.g));
            Assert.That(unselectedColor.b, Is.EqualTo(clip.gizmoColor.b));
            Assert.That(
                unselectedColor.a,
                Is.EqualTo(
                    clip.gizmoColor.a *
                    LookAtTimelineGizmoRegistry.UnselectedOpacityMultiplier));
        }

        [Test]
        public void GizmoLineOrigins_UseEyesThenHeadEndThenHead()
        {
            var root = Track(new GameObject("Generic Gizmo Origins"));
            var animator = root.AddComponent<Animator>();
            var pelvis = CreateChild(root.transform, "Hips");
            var head = CreateChild(pelvis, "Head");
            var headEnd = CreateChild(head, "Head_End");
            var leftEye = CreateChild(head, "L Eye");
            var rightEye = CreateChild(head, "R Eye");
            var track = Track(
                ScriptableObject.CreateInstance<LookAtTrack>());
            var mapping = root.AddComponent<LookAtGenericRigMapping>();
            mapping.initialized = true;
            mapping.pelvis = pelvis;
            mapping.head = head;
            mapping.leftEye = leftEye;
            mapping.rightEye = rightEye;

            Assert.That(
                LookAtGizmoUtility.TryGetLineOrigins(
                    animator,
                    track,
                    out var primary,
                    out var secondary),
                Is.True);
            Assert.That(primary, Is.SameAs(leftEye));
            Assert.That(secondary, Is.SameAs(rightEye));

            mapping.rightEye = null;
            Assert.That(
                LookAtGizmoUtility.TryGetLineOrigins(
                    animator,
                    track,
                    out primary,
                    out secondary),
                Is.True);
            Assert.That(primary, Is.SameAs(leftEye));
            Assert.That(secondary, Is.Null);

            mapping.leftEye = null;
            Assert.That(
                LookAtGizmoUtility.TryGetLineOrigins(
                    animator,
                    track,
                    out primary,
                    out secondary),
                Is.True);
            Assert.That(primary, Is.SameAs(headEnd));
            Assert.That(secondary, Is.Null);

            headEnd.name = "Hair";
            Assert.That(
                LookAtGizmoUtility.TryGetLineOrigins(
                    animator,
                    track,
                    out primary,
                    out secondary),
                Is.True);
            Assert.That(primary, Is.SameAs(head));
            Assert.That(secondary, Is.Null);
        }

        [Test]
        public void TimelineGizmoRegistry_ClipVisibilityExpiresAfterGuiStopsDrawing()
        {
            const double lastSeen = 10d;
            Assert.That(
                LookAtTimelineGizmoRegistry.IsWithinVisibilityWindow(lastSeen, lastSeen),
                Is.True);
            Assert.That(
                LookAtTimelineGizmoRegistry.IsWithinVisibilityWindow(
                    lastSeen + LookAtTimelineGizmoRegistry.VisibilityTimeout,
                    lastSeen),
                Is.True);
            Assert.That(
                LookAtTimelineGizmoRegistry.IsWithinVisibilityWindow(
                    lastSeen + LookAtTimelineGizmoRegistry.VisibilityTimeout + 0.001d,
                    lastSeen),
                Is.False);
        }

        T Track<T>(T value) where T : Object
        {
            _objectsToDestroy.Add(value);
            return value;
        }

        static Transform CreateChild(
            Transform parent,
            string name,
            Vector3 localPosition = default)
        {
            var child = new GameObject(name).transform;
            child.SetParent(parent, false);
            child.localPosition = localPosition;
            return child;
        }

        Mesh CreateBlendShapeMesh(params string[] keys)
        {
            var mesh = Track(new Mesh());
            mesh.vertices = new[] { Vector3.zero };
            var deltaVertices = new[] { Vector3.zero };
            var deltaNormals = new[] { Vector3.zero };
            var deltaTangents = new[] { Vector3.zero };

            for (var i = 0; i < keys.Length; i++)
            {
                mesh.AddBlendShapeFrame(
                    keys[i],
                    100f,
                    deltaVertices,
                    deltaNormals,
                    deltaTangents);
            }

            return mesh;
        }



        [Test]
        public void AnimationCurveBlink_UsesNormalizedTimeAndOpenness()
        {
            var curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

            Assert.That(
                LookAtUtility.EvaluateBlink(
                    LookAtBlinkMode.AnimationCurve,
                    curve,
                    frequency: 1f,
                    blinkDuration: 0.1f,
                    localTime: 0d,
                    localDuration: 2d),
                Is.EqualTo(1f).Within(0.0001f));
            Assert.That(
                LookAtUtility.EvaluateBlink(
                    LookAtBlinkMode.AnimationCurve,
                    curve,
                    frequency: 1f,
                    blinkDuration: 0.1f,
                    localTime: 1d,
                    localDuration: 2d),
                Is.EqualTo(0.5f).Within(0.0001f));
            Assert.That(
                LookAtUtility.EvaluateBlink(
                    LookAtBlinkMode.AnimationCurve,
                    curve,
                    frequency: 1f,
                    blinkDuration: 0.1f,
                    localTime: 2d,
                    localDuration: 2d),
                Is.EqualTo(0f).Within(0.0001f));
        }

        [Test]
        public void AnimationCurveBlink_ClampsKeysToNormalizedRange()
        {
            var curve = new AnimationCurve(
                new Keyframe(-1f, 2f),
                new Keyframe(2f, -1f));

            LookAtUtility.ClampCurve01(curve);

            Assert.That(curve.keys[0].time, Is.EqualTo(0f));
            Assert.That(curve.keys[0].value, Is.EqualTo(1f));
            Assert.That(curve.keys[1].time, Is.EqualTo(1f));
            Assert.That(curve.keys[1].value, Is.EqualTo(0f));
        }

        [Test]
        public void AutomaticBlink_FrequencyZeroDisablesBlinking()
        {
            Assert.That(
                LookAtUtility.EvaluateAutomaticBlink(
                    localTime: 1000d,
                    frequency: 0f,
                    blinkDuration: 0.1f),
                Is.EqualTo(0f));
            Assert.That(
                LookAtUtility.IsAutomaticBlinkTrigger(
                    sampleIndex: 100,
                    frequency: 0f),
                Is.False);
        }

        [Test]
        public void AutomaticBlink_UsesNonPeriodicPerlinThresholdCrossings()
        {
            var triggerTimes = new List<float>();
            const float frequency = 0.8f;
            var sampleCount = Mathf.CeilToInt(
                60f / LookAtUtility.AutomaticBlinkSampleInterval);
            for (var sampleIndex = 1;
                 sampleIndex <= sampleCount;
                 sampleIndex++)
            {
                if (LookAtUtility.IsAutomaticBlinkTrigger(
                        sampleIndex,
                        frequency))
                {
                    triggerTimes.Add(
                        sampleIndex *
                        LookAtUtility.AutomaticBlinkSampleInterval);
                }
            }

            Assert.That(triggerTimes.Count, Is.GreaterThanOrEqualTo(3));

            var intervals = new List<float>();
            for (var i = 1; i < triggerTimes.Count; i++)
            {
                intervals.Add(triggerTimes[i] - triggerTimes[i - 1]);
            }

            Assert.That(
                intervals.Max() - intervals.Min(),
                Is.GreaterThan(0.1f));
        }

        [Test]
        public void AutomaticBlink_DurationControlsPulseLength()
        {
            Assert.That(
                LookAtUtility.EvaluateAutomaticBlinkPulse(
                    elapsed: 0.11f,
                    blinkDuration: 0.1f),
                Is.EqualTo(0f));
            Assert.That(
                LookAtUtility.EvaluateAutomaticBlinkPulse(
                    elapsed: 0.11f,
                    blinkDuration: 0.2f),
                Is.GreaterThan(0f));
        }

        [Test]
        public void AutomaticBlink_CurveControlsNormalizedPulseShape()
        {
            var curve = AnimationCurve.Linear(0f, 1f, 1f, 0f);

            Assert.That(
                LookAtUtility.EvaluateAutomaticBlinkPulse(
                    elapsed: 0.025f,
                    blinkDuration: 0.1f,
                    curve: curve),
                Is.EqualTo(0.25f).Within(0.0001f));
            Assert.That(
                LookAtUtility.EvaluateAutomaticBlinkPulse(
                    elapsed: 0.075f,
                    blinkDuration: 0.1f,
                    curve: curve),
                Is.EqualTo(0.75f).Within(0.0001f));
        }

        [Test]
        public void AutomaticBlink_LegacyClosureCurveMigratesToOpenness()
        {
            var clip = Track(
                ScriptableObject.CreateInstance<LookAtClip>());
            clip.automaticBlinkCurve = new AnimationCurve(
                new Keyframe(0f, 0f, 0f, 0f),
                new Keyframe(0.38f, 0.9f, 3.2f, -1.7f),
                new Keyframe(1f, 0f, 0f, 0f));
            clip.automaticBlinkCurveSemanticsVersion = 0;

            clip.UpgradeAutomaticBlinkCurveSemantics();

            Assert.That(
                clip.automaticBlinkCurve.keys[0].value,
                Is.EqualTo(1f));
            Assert.That(
                clip.automaticBlinkCurve.keys[1].value,
                Is.EqualTo(0.1f).Within(0.0001f));
            Assert.That(
                clip.automaticBlinkCurve.keys[1].inTangent,
                Is.EqualTo(-3.2f).Within(0.0001f));
            Assert.That(
                clip.automaticBlinkCurve.keys[1].outTangent,
                Is.EqualTo(1.7f).Within(0.0001f));
            Assert.That(
                clip.automaticBlinkCurve.keys[2].value,
                Is.EqualTo(1f));
            Assert.That(
                clip.automaticBlinkCurveSemanticsVersion,
                Is.EqualTo(
                    LookAtClip.CurrentAutomaticBlinkCurveSemanticsVersion));
        }

        [Test]
        public void AutomaticBlink_NoiseOffsetMovesRuntimeAndTimelineMarkers()
        {
            var baseline = new List<double>();
            var shifted = new List<double>();

            LookAtUtility.CollectAutomaticBlinkTriggerTimes(
                startTime: 0d,
                endTime: 60d,
                frequency: 0.8f,
                noiseOffset: 0f,
                destination: baseline);
            LookAtUtility.CollectAutomaticBlinkTriggerTimes(
                startTime: 0d,
                endTime: 60d,
                frequency: 0.8f,
                noiseOffset: 4.25f,
                destination: shifted);

            Assert.That(baseline.Count, Is.GreaterThan(0));
            Assert.That(shifted.Count, Is.GreaterThan(0));
            Assert.That(shifted.SequenceEqual(baseline), Is.False);

            var firstShiftedTrigger = shifted[0];
            Assert.That(
                LookAtUtility.EvaluateAutomaticBlink(
                    localTime: firstShiftedTrigger + 0.03d,
                    frequency: 0.8f,
                    blinkDuration: 0.1f,
                    noiseOffset: 4.25f),
                Is.GreaterThan(0f));
        }



        [Test]
        public void AutomaticBlink_HigherFrequencyProducesMoreTriggers()
        {
            var lowFrequencyCount = 0;
            var highFrequencyCount = 0;
            var sampleCount = Mathf.CeilToInt(
                60f / LookAtUtility.AutomaticBlinkSampleInterval);
            for (var sampleIndex = 1;
                 sampleIndex <= sampleCount;
                 sampleIndex++)
            {
                if (LookAtUtility.IsAutomaticBlinkTrigger(
                        sampleIndex,
                        0.2f))
                {
                    lowFrequencyCount++;
                }

                if (LookAtUtility.IsAutomaticBlinkTrigger(
                        sampleIndex,
                        0.8f))
                {
                    highFrequencyCount++;
                }
            }

            Assert.That(
                highFrequencyCount,
                Is.GreaterThan(lowFrequencyCount));
        }

        [Test]
        public void BlendShapeWeight_BlendsFromAnimatedBaseAndBetweenClips()
        {
            Assert.That(
                LookAtUtility.BlendBlendShapeWeight(
                    baseWeight: 20f,
                    weightedTargetSum: 50f,
                    totalTimelineWeight: 0.5f),
                Is.EqualTo(60f).Within(0.0001f));
            Assert.That(
                LookAtUtility.BlendBlendShapeWeight(
                    baseWeight: 20f,
                    weightedTargetSum: 25f,
                    totalTimelineWeight: 1f),
                Is.EqualTo(25f).Within(0.0001f));
        }

        [Test]
        public void EyelidFollow_UsesOpposedUpperAndLowerResponseCurves()
        {
            var upperCurve =
                LookAtUtility.CreateDefaultUpperEyelidFollowCurve();
            var lowerCurve =
                LookAtUtility.CreateDefaultLowerEyelidFollowCurve();
            var limits = LookAtClip.DefaultEyesAngleLimits;

            Assert.That(
                LookAtUtility.NormalizeEyePitch(-25f, limits),
                Is.EqualTo(0f).Within(0.0001f));
            Assert.That(
                LookAtUtility.NormalizeEyePitch(0f, limits),
                Is.EqualTo(0.5f).Within(0.0001f));
            Assert.That(
                LookAtUtility.NormalizeEyePitch(25f, limits),
                Is.EqualTo(1f).Within(0.0001f));

            Assert.That(
                LookAtUtility.EvaluateEyelidFollow(
                    -25f,
                    limits,
                    0.15f,
                    upperCurve),
                Is.EqualTo(0.15f).Within(0.0001f));
            Assert.That(
                LookAtUtility.EvaluateEyelidFollow(
                    0f,
                    limits,
                    0.15f,
                    upperCurve),
                Is.EqualTo(0f).Within(0.0001f));
            Assert.That(
                LookAtUtility.EvaluateEyelidFollow(
                    25f,
                    limits,
                    0.15f,
                    upperCurve),
                Is.EqualTo(0f).Within(0.0001f));

            Assert.That(
                LookAtUtility.EvaluateEyelidFollow(
                    -25f,
                    limits,
                    0.08f,
                    lowerCurve),
                Is.EqualTo(0f).Within(0.0001f));
            Assert.That(
                LookAtUtility.EvaluateEyelidFollow(
                    0f,
                    limits,
                    0.08f,
                    lowerCurve),
                Is.EqualTo(0f).Within(0.0001f));
            Assert.That(
                LookAtUtility.EvaluateEyelidFollow(
                    25f,
                    limits,
                    0.08f,
                    lowerCurve),
                Is.EqualTo(0.08f).Within(0.0001f));
        }

        [Test]
        public void EyelidKeyCache_ClassifiesFourAndTwoChannelLayouts()
        {
            var fourChannel =
                LookAtUtility.CacheEyelidBlendShapeKeys(
                    new[]
                    {
                        "Eye_L_Look_Up",
                        "Eye_R_Look_Up",
                        "Eye_L_Look_Down",
                        "Eye_R_Look_Down"
                    },
                    LookAtEyelidDirection.Down);

            Assert.That(fourChannel.Length, Is.EqualTo(4));
            Assert.That(
                fourChannel[0].Side,
                Is.EqualTo(LookAtEyelidSide.Left));
            Assert.That(
                fourChannel[0].Direction,
                Is.EqualTo(LookAtEyelidDirection.Up));
            Assert.That(
                fourChannel[1].Side,
                Is.EqualTo(LookAtEyelidSide.Right));
            Assert.That(
                fourChannel[2].Direction,
                Is.EqualTo(LookAtEyelidDirection.Down));
            Assert.That(
                fourChannel[3].Side,
                Is.EqualTo(LookAtEyelidSide.Right));

            var twoChannel =
                LookAtUtility.CacheEyelidBlendShapeKeys(
                    new[]
                    {
                        "Eye_Look_Up",
                        "Eye_Look_Down"
                    },
                    LookAtEyelidDirection.Down);

            Assert.That(twoChannel.Length, Is.EqualTo(2));
            Assert.That(
                twoChannel[0].Side,
                Is.EqualTo(LookAtEyelidSide.Both));
            Assert.That(
                twoChannel[0].Direction,
                Is.EqualTo(LookAtEyelidDirection.Up));
            Assert.That(
                twoChannel[1].Side,
                Is.EqualTo(LookAtEyelidSide.Both));
            Assert.That(
                twoChannel[1].Direction,
                Is.EqualTo(LookAtEyelidDirection.Down));

            var legacyKeys =
                LookAtUtility.CacheEyelidBlendShapeKeys(
                    new[]
                    {
                        "UpperLidFollow_L",
                        "UpperLidFollowRight"
                    },
                    LookAtEyelidDirection.Down);
            Assert.That(
                legacyKeys[0].Direction,
                Is.EqualTo(LookAtEyelidDirection.Down));
            Assert.That(
                legacyKeys[0].Side,
                Is.EqualTo(LookAtEyelidSide.Left));
            Assert.That(
                legacyKeys[1].Side,
                Is.EqualTo(LookAtEyelidSide.Right));
        }

        [Test]
        public void HorizontalEyelidKeyCache_SeparatesDirectionFromEyeSide()
        {
            var currentModel =
                LookAtUtility.CacheEyelidBlendShapeKeys(
                    new[]
                    {
                        "Eye_L_Look_L",
                        "Eye_R_Look_L",
                        "Eye_L_Look_R",
                        "Eye_R_Look_R"
                    },
                    LookAtEyelidDirection.Horizontal);

            Assert.That(currentModel.Length, Is.EqualTo(4));
            Assert.That(
                currentModel[0].Direction,
                Is.EqualTo(LookAtEyelidDirection.Left));
            Assert.That(
                currentModel[0].Side,
                Is.EqualTo(LookAtEyelidSide.Left));
            Assert.That(
                currentModel[1].Direction,
                Is.EqualTo(LookAtEyelidDirection.Left));
            Assert.That(
                currentModel[1].Side,
                Is.EqualTo(LookAtEyelidSide.Right));
            Assert.That(
                currentModel[2].Direction,
                Is.EqualTo(LookAtEyelidDirection.Right));
            Assert.That(
                currentModel[2].Side,
                Is.EqualTo(LookAtEyelidSide.Left));
            Assert.That(
                currentModel[3].Direction,
                Is.EqualTo(LookAtEyelidDirection.Right));
            Assert.That(
                currentModel[3].Side,
                Is.EqualTo(LookAtEyelidSide.Right));

            var suffixSide =
                LookAtUtility.CacheEyelidBlendShapeKeys(
                    new[]
                    {
                        "eye_look_left.L",
                        "eye_look_left.R",
                        "eye_look_right.L",
                        "eye_look_right.R"
                    },
                    LookAtEyelidDirection.Horizontal);

            Assert.That(
                suffixSide[0].Direction,
                Is.EqualTo(LookAtEyelidDirection.Left));
            Assert.That(
                suffixSide[0].Side,
                Is.EqualTo(LookAtEyelidSide.Left));
            Assert.That(
                suffixSide[1].Side,
                Is.EqualTo(LookAtEyelidSide.Right));
            Assert.That(
                suffixSide[2].Direction,
                Is.EqualTo(LookAtEyelidDirection.Right));
            Assert.That(
                suffixSide[2].Side,
                Is.EqualTo(LookAtEyelidSide.Left));
            Assert.That(
                suffixSide[3].Side,
                Is.EqualTo(LookAtEyelidSide.Right));

            var twoChannel =
                LookAtUtility.CacheEyelidBlendShapeKeys(
                    new[]
                    {
                        "Eye_Look_Left",
                        "Eye_Look_Right"
                    },
                    LookAtEyelidDirection.Horizontal);

            Assert.That(
                twoChannel[0].Direction,
                Is.EqualTo(LookAtEyelidDirection.Left));
            Assert.That(
                twoChannel[0].Side,
                Is.EqualTo(LookAtEyelidSide.Both));
            Assert.That(
                twoChannel[1].Direction,
                Is.EqualTo(LookAtEyelidDirection.Right));
            Assert.That(
                twoChannel[1].Side,
                Is.EqualTo(LookAtEyelidSide.Both));
        }

        [Test]
        public void HorizontalEyelidKeyCache_ClassifiesAbbreviatedTwoChannelLayout()
        {
            var cached =
                LookAtUtility.CacheEyelidBlendShapeKeys(
                    new[]
                    {
                        "Eye_Look_L",
                        "Eye_Look_R"
                    },
                    LookAtEyelidDirection.Horizontal);

            Assert.That(
                cached[0].Direction,
                Is.EqualTo(LookAtEyelidDirection.Left));
            Assert.That(
                cached[0].Side,
                Is.EqualTo(LookAtEyelidSide.Both));
            Assert.That(
                cached[1].Direction,
                Is.EqualTo(LookAtEyelidDirection.Right));
            Assert.That(
                cached[1].Side,
                Is.EqualTo(LookAtEyelidSide.Both));
        }



        [Test]
        public void EyeDirectionState_UsesPerEyeOrSharedValuesForCachedSide()
        {
            var directions = new LookAtEyeDirectionState(
                hasLeft: true,
                leftPitch: -20f,
                leftYaw: -30f,
                hasRight: true,
                rightPitch: 10f,
                rightYaw: 20f);

            Assert.That(
                directions.TryResolvePitch(
                    LookAtEyelidSide.Left,
                    out var leftPitch),
                Is.True);
            Assert.That(leftPitch, Is.EqualTo(-20f));
            Assert.That(
                directions.TryResolvePitch(
                    LookAtEyelidSide.Right,
                    out var rightPitch),
                Is.True);
            Assert.That(rightPitch, Is.EqualTo(10f));
            Assert.That(
                directions.TryResolvePitch(
                    LookAtEyelidSide.Both,
                    out var sharedPitch),
                Is.True);
            Assert.That(sharedPitch, Is.EqualTo(-5f));

            Assert.That(
                directions.TryResolveYaw(
                    LookAtEyelidSide.Left,
                    out var leftYaw),
                Is.True);
            Assert.That(leftYaw, Is.EqualTo(-30f));
            Assert.That(
                directions.TryResolveYaw(
                    LookAtEyelidSide.Right,
                    out var rightYaw),
                Is.True);
            Assert.That(rightYaw, Is.EqualTo(20f));
            Assert.That(
                directions.TryResolveYaw(
                    LookAtEyelidSide.Both,
                    out var sharedYaw),
                Is.True);
            Assert.That(sharedYaw, Is.EqualTo(-5f));

            var rightOnly = new LookAtEyeDirectionState(
                hasLeft: false,
                leftPitch: 0f,
                leftYaw: 0f,
                hasRight: true,
                rightPitch: 12f,
                rightYaw: 18f);
            Assert.That(
                rightOnly.TryResolvePitch(
                    LookAtEyelidSide.Left,
                    out var pitchFallback),
                Is.True);
            Assert.That(pitchFallback, Is.EqualTo(12f));
            Assert.That(
                rightOnly.TryResolveYaw(
                    LookAtEyelidSide.Left,
                    out var yawFallback),
                Is.True);
            Assert.That(yawFallback, Is.EqualTo(18f));
        }

        [Test]
        public void DirectionalEyelidFollow_ActivatesOnlyMatchingDirection()
        {
            var limits = LookAtClip.DefaultEyesAngleLimits;
            var upperCurve =
                LookAtUtility.CreateDefaultUpperEyelidFollowCurve();
            var lowerCurve =
                LookAtUtility.CreateDefaultLowerEyelidFollowCurve();

            Assert.That(
                LookAtUtility.EvaluateDirectionalEyelidFollow(
                    -25f,
                    limits,
                    0.15f,
                    upperCurve,
                    LookAtEyelidDirection.Down,
                    LookAtEyelidDirection.Down),
                Is.EqualTo(0.15f).Within(0.0001f));
            Assert.That(
                LookAtUtility.EvaluateDirectionalEyelidFollow(
                    25f,
                    limits,
                    0.15f,
                    upperCurve,
                    LookAtEyelidDirection.Up,
                    LookAtEyelidDirection.Down),
                Is.EqualTo(0.15f).Within(0.0001f));
            Assert.That(
                LookAtUtility.EvaluateDirectionalEyelidFollow(
                    25f,
                    limits,
                    0.15f,
                    upperCurve,
                    LookAtEyelidDirection.Down,
                    LookAtEyelidDirection.Down),
                Is.EqualTo(0f));
            Assert.That(
                LookAtUtility.EvaluateDirectionalEyelidFollow(
                    -25f,
                    limits,
                    0.08f,
                    lowerCurve,
                    LookAtEyelidDirection.Down,
                    LookAtEyelidDirection.Up),
                Is.EqualTo(0.08f).Within(0.0001f));
        }

        [Test]
        public void HorizontalEyelidFollow_ActivatesOnlyMatchingDirection()
        {
            var limits = LookAtClip.DefaultEyesAngleLimits;
            var curve =
                LookAtUtility.CreateDefaultHorizontalEyelidFollowCurve();

            Assert.That(
                LookAtUtility.EvaluateHorizontalEyelidFollow(
                    -40f,
                    limits,
                    0.08f,
                    curve,
                    LookAtEyelidDirection.Left),
                Is.EqualTo(0.08f).Within(0.0001f));
            Assert.That(
                LookAtUtility.EvaluateHorizontalEyelidFollow(
                    -40f,
                    limits,
                    0.08f,
                    curve,
                    LookAtEyelidDirection.Right),
                Is.EqualTo(0f));
            Assert.That(
                LookAtUtility.EvaluateHorizontalEyelidFollow(
                    40f,
                    limits,
                    0.08f,
                    curve,
                    LookAtEyelidDirection.Right),
                Is.EqualTo(0.08f).Within(0.0001f));
            Assert.That(
                LookAtUtility.EvaluateHorizontalEyelidFollow(
                    40f,
                    limits,
                    0.08f,
                    curve,
                    LookAtEyelidDirection.Left),
                Is.EqualTo(0f));
            Assert.That(
                LookAtUtility.EvaluateHorizontalEyelidFollow(
                    20f,
                    LookAtAngleLimits.Unrestricted,
                    1f,
                    curve,
                    LookAtEyelidDirection.Right),
                Is.GreaterThan(0f));
        }


        [Test]
        public void EyelidFollow_WideEyeLimitsDoNotFlattenNormalMovement()
        {
            var curve =
                LookAtUtility.CreateDefaultLowerEyelidFollowCurve();

            Assert.That(
                LookAtUtility.NormalizeEyelidFollowPitch(
                    12f,
                    LookAtAngleLimits.Unrestricted),
                Is.EqualTo(0.74f).Within(0.0001f));
            Assert.That(
                LookAtUtility.EvaluateDirectionalEyelidFollow(
                    12f,
                    LookAtAngleLimits.Unrestricted,
                    1f,
                    curve,
                    LookAtEyelidDirection.Up,
                    LookAtEyelidDirection.Up),
                Is.GreaterThan(0f));

            var narrowLimits = new LookAtAngleLimits(
                new Vector2(-40f, 40f),
                new Vector2(-10f, 10f));
            Assert.That(
                LookAtUtility.NormalizeEyelidFollowPitch(
                    10f,
                    narrowLimits),
                Is.EqualTo(1f).Within(0.0001f));
        }



        [Test]
        public void EyelidFollow_UsesIndependentVerticalAndHorizontalKeys()
        {
            var sample = new LookAtSample
            {
                BlinkBlendShapeKeys = new[] { "Blink" },
                UpperEyelidFollowBlendShapeKeys =
                    System.Array.Empty<string>(),
                UpperEyelidFollowWeight = 0.5f,
                UpperEyelidFollowCurve =
                    LookAtUtility
                        .CreateDefaultUpperEyelidFollowCurve(),
                LowerEyelidFollowBlendShapeKeys =
                    System.Array.Empty<string>(),
                LowerEyelidFollowWeight = 0.25f,
                LowerEyelidFollowCurve =
                    LookAtUtility
                        .CreateDefaultLowerEyelidFollowCurve(),
                HorizontalEyelidFollowBlendShapeKeys =
                    System.Array.Empty<string>(),
                HorizontalEyelidFollowWeight = 0.2f,
                HorizontalEyelidFollowCurve =
                    LookAtUtility
                        .CreateDefaultHorizontalEyelidFollowCurve()
            };

            Assert.That(
                sample.HasBlinkConfiguration(),
                Is.False);
            Assert.That(
                sample.HasEyelidFollowConfiguration(),
                Is.False);

            sample.UpperEyelidFollowBlendShapeKeys =
                new[] { "UpperLidFollow" };

            Assert.That(
                sample.HasUpperEyelidFollowConfiguration(),
                Is.True);
            Assert.That(
                sample.HasLowerEyelidFollowConfiguration(),
                Is.False);
            Assert.That(
                sample.HasHorizontalEyelidFollowConfiguration(),
                Is.False);
            Assert.That(
                sample.HasEyelidFollowConfiguration(),
                Is.True);

            sample.UpperEyelidFollowBlendShapeKeys =
                System.Array.Empty<string>();
            sample.LowerEyelidFollowBlendShapeKeys =
                new[] { "LowerLidFollow" };

            Assert.That(
                sample.HasUpperEyelidFollowConfiguration(),
                Is.False);
            Assert.That(
                sample.HasLowerEyelidFollowConfiguration(),
                Is.True);
            Assert.That(
                sample.HasHorizontalEyelidFollowConfiguration(),
                Is.False);
            Assert.That(
                sample.HasEyelidFollowConfiguration(),
                Is.True);

            sample.LowerEyelidFollowBlendShapeKeys =
                System.Array.Empty<string>();
            sample.HorizontalEyelidFollowBlendShapeKeys =
                new[] { "Eye_Look_Left", "Eye_Look_Right" };

            Assert.That(
                sample.HasUpperEyelidFollowConfiguration(),
                Is.False);
            Assert.That(
                sample.HasLowerEyelidFollowConfiguration(),
                Is.False);
            Assert.That(
                sample.HasHorizontalEyelidFollowConfiguration(),
                Is.True);
            Assert.That(
                sample.HasEyelidFollowConfiguration(),
                Is.True);
        }

        [Test]
        public void BlendShapeAutoDetection_SeparatesBlinkUpperAndLowerLids()
        {
            var root = Track(new GameObject("Character"));
            var animator = root.AddComponent<Animator>();
            var rendererObject = new GameObject("Face");
            rendererObject.transform.SetParent(root.transform);
            var renderer =
                rendererObject.AddComponent<SkinnedMeshRenderer>();
            renderer.sharedMesh = CreateBlendShapeMesh(
                "EyeBlink",
                "EyeBlinkLeft",
                "EyeBlinkRight",
                "MouthSmile",
                "Eye_Lid_Upper_L",
                "Eye_Lid_Upper_R",
                "Upper_Lid_Down_L",
                "Upper_Lid_Down_R",
                "Eye_Lid_Lower_L",
                "Eye_Lid_Lower_R",
                "Lower_Lid_Up_L",
                "Lower_Lid_Up_R");

            Assert.That(
                LookAtClipInspector.FindLikelyBlendShapeKeys(
                    animator,
                    LookAtClipInspector.BlendShapeKeyRole.Blink),
                Is.EqualTo(new[]
                {
                    "EyeBlinkLeft",
                    "EyeBlinkRight"
                }));
            Assert.That(
                LookAtClipInspector.FindLikelyBlendShapeKeys(
                    animator,
                    LookAtClipInspector.BlendShapeKeyRole
                        .UpperEyelidFollow),
                Is.EqualTo(new[]
                {
                    "Upper_Lid_Down_L",
                    "Upper_Lid_Down_R"
                }));
            Assert.That(
                LookAtClipInspector.FindLikelyBlendShapeKeys(
                    animator,
                    LookAtClipInspector.BlendShapeKeyRole
                        .LowerEyelidFollow),
                Is.EqualTo(new[]
                {
                    "Lower_Lid_Up_L",
                    "Lower_Lid_Up_R"
                }));
        }

        [Test]
        public void BlendShapeAutoDetection_DoesNotReuseBlinkKeysForFollow()
        {
            var root = Track(new GameObject("Character"));
            var animator = root.AddComponent<Animator>();
            var rendererObject = new GameObject("Face");
            rendererObject.transform.SetParent(root.transform);
            var renderer =
                rendererObject.AddComponent<SkinnedMeshRenderer>();
            renderer.sharedMesh = CreateBlendShapeMesh(
                "Eye_Blink_L",
                "Eye_Blink_R",
                "Jaw_Open");

            Assert.That(
                LookAtClipInspector.FindLikelyBlendShapeKeys(
                    animator,
                    LookAtClipInspector.BlendShapeKeyRole
                        .UpperEyelidFollow),
                Is.Empty);
            Assert.That(
                LookAtClipInspector.FindLikelyBlendShapeKeys(
                    animator,
                    LookAtClipInspector.BlendShapeKeyRole
                        .LowerEyelidFollow),
                Is.Empty);
        }

        [Test]
        public void BlendShapeAutoDetection_DetectsDirectionalEyeLookKeys()
        {
            var root = Track(new GameObject("Character"));
            var animator = root.AddComponent<Animator>();
            var rendererObject = new GameObject("Face");
            rendererObject.transform.SetParent(root.transform);
            var renderer =
                rendererObject.AddComponent<SkinnedMeshRenderer>();
            renderer.sharedMesh = CreateBlendShapeMesh(
                "Eye_L_Look_Up",
                "Eye_R_Look_Up",
                "Eye_L_Look_Down",
                "Eye_R_Look_Down");

            Assert.That(
                LookAtClipInspector.FindLikelyBlendShapeKeys(
                    animator,
                    LookAtClipInspector.BlendShapeKeyRole
                        .UpperEyelidFollow),
                Is.EqualTo(new[]
                {
                    "Eye_L_Look_Down",
                    "Eye_R_Look_Down"
                }));
            Assert.That(
                LookAtClipInspector.FindLikelyBlendShapeKeys(
                    animator,
                    LookAtClipInspector.BlendShapeKeyRole
                        .LowerEyelidFollow),
                Is.EqualTo(new[]
                {
                    "Eye_L_Look_Up",
                    "Eye_R_Look_Up"
                }));
        }

        [Test]
        public void BlendShapeAutoDetection_DetectsSharedDirectionalEyeLookKeys()
        {
            var root = Track(new GameObject("Character"));
            var animator = root.AddComponent<Animator>();
            var rendererObject = new GameObject("Face");
            rendererObject.transform.SetParent(root.transform);
            var renderer =
                rendererObject.AddComponent<SkinnedMeshRenderer>();
            renderer.sharedMesh = CreateBlendShapeMesh(
                "Eye_Look_Up",
                "Eye_Look_Down");

            Assert.That(
                LookAtClipInspector.FindLikelyBlendShapeKeys(
                    animator,
                    LookAtClipInspector.BlendShapeKeyRole
                        .UpperEyelidFollow),
                Is.EqualTo(new[] { "Eye_Look_Down" }));
            Assert.That(
                LookAtClipInspector.FindLikelyBlendShapeKeys(
                    animator,
                    LookAtClipInspector.BlendShapeKeyRole
                        .LowerEyelidFollow),
                Is.EqualTo(new[] { "Eye_Look_Up" }));
        }

        [Test]
        public void BlendShapeAutoDetection_DetectsHorizontalEyeLookKeys()
        {
            var root = Track(new GameObject("Character"));
            var animator = root.AddComponent<Animator>();
            var rendererObject = new GameObject("Face");
            rendererObject.transform.SetParent(root.transform);
            var renderer =
                rendererObject.AddComponent<SkinnedMeshRenderer>();
            renderer.sharedMesh = CreateBlendShapeMesh(
                "Eye_L_Look_L",
                "Eye_R_Look_L",
                "Eye_L_Look_R",
                "Eye_R_Look_R",
                "Eye_Look_Up",
                "Eye_Look_Down");

            Assert.That(
                LookAtClipInspector.FindLikelyBlendShapeKeys(
                    animator,
                    LookAtClipInspector.BlendShapeKeyRole
                        .HorizontalEyelidFollow),
                Is.EqualTo(new[]
                {
                    "Eye_L_Look_L",
                    "Eye_L_Look_R",
                    "Eye_R_Look_L",
                    "Eye_R_Look_R"
                }));
        }






        [Test]
        public void EyelidClosure_CombinesFollowWithoutWeakeningBlink()
        {
            Assert.That(
                LookAtUtility.CombineEyelidClosures(0.9f, 0.15f),
                Is.EqualTo(0.915f).Within(0.0001f));
            Assert.That(
                LookAtUtility.CombineEyelidClosures(1f, 0.15f),
                Is.EqualTo(1f).Within(0.0001f));
            Assert.That(
                LookAtUtility.CombineEyelidClosures(0f, 0.15f),
                Is.EqualTo(0.15f).Within(0.0001f));
        }

        [Test]
        public void RelativeEyePitch_IsMeasuredFromHeadInCharacterSpace()
        {
            var referenceRotation = Quaternion.Euler(13f, 67f, -8f);
            var headForward = referenceRotation * new Vector3(
                0f,
                Mathf.Sin(10f * Mathf.Deg2Rad),
                Mathf.Cos(10f * Mathf.Deg2Rad));
            var eyeForward = referenceRotation * new Vector3(
                0f,
                Mathf.Sin(-15f * Mathf.Deg2Rad),
                Mathf.Cos(-15f * Mathf.Deg2Rad));

            Assert.That(
                LookAtUtility.GetRelativeEyePitch(
                    referenceRotation,
                    headForward,
                    eyeForward),
                Is.EqualTo(-25f).Within(0.0001f));
        }

        [Test]
        public void RelativeEyeYaw_IsMeasuredFromHeadInCharacterSpace()
        {
            var referenceRotation = Quaternion.Euler(13f, 67f, -8f);
            var headForward = referenceRotation * new Vector3(
                Mathf.Sin(10f * Mathf.Deg2Rad),
                0f,
                Mathf.Cos(10f * Mathf.Deg2Rad));
            var eyeForward = referenceRotation * new Vector3(
                Mathf.Sin(-15f * Mathf.Deg2Rad),
                0f,
                Mathf.Cos(-15f * Mathf.Deg2Rad));

            Assert.That(
                LookAtUtility.GetRelativeEyeYaw(
                    referenceRotation,
                    headForward,
                    eyeForward),
                Is.EqualTo(-25f).Within(0.0001f));
        }


        [Test]
        public void TimelineDriver_RetainsCachesAcrossInactiveGapUntilOwnerReleases()
        {
            var root = Track(new GameObject("Look At Driver"));
            var animator = root.AddComponent<Animator>();
            var driver = LookAtLateUpdateDriver.GetOrCreate(animator);

            driver.SetState(new LookAtState { Active = true });
            driver.ClearState();

            Assert.That(driver, Is.Not.Null);
            Assert.That(driver.enabled, Is.False);
            Assert.That(driver.TimelineOwnerCount, Is.EqualTo(1));

            driver.ReleaseTimelineOwner();

            Assert.That(driver == null, Is.True);
            Assert.That(
                root.GetComponent<LookAtLateUpdateDriver>(),
                Is.Null);
        }

        [Test]
        public void BlendShapeCache_ResolvesOnlyConfiguredKeys()
        {
            var root = Track(new GameObject("Character"));
            root.AddComponent<Animator>();
            var rendererObject = new GameObject("Face");
            rendererObject.transform.SetParent(root.transform);
            var renderer =
                rendererObject.AddComponent<SkinnedMeshRenderer>();
            renderer.sharedMesh = CreateBlendShapeMesh(
                "Blink",
                "Unused A",
                "Unused B");
            var driver = root.AddComponent<LookAtLateUpdateDriver>();

            driver.CacheBlendShapeBindingsForKeys(
                new[] { "Blink", "Missing" });

            Assert.That(driver.CachedBlendShapeKeyCount, Is.EqualTo(2));
            Assert.That(
                driver.CachedBlendShapeBindingCount,
                Is.EqualTo(1));
        }

        [Test]
        public void PreviewPolling_RunsContinuouslyOnlyWhileActiveOrRequested()
        {
            Assert.That(
                LookAtTimelinePreviewUpdater.ShouldPollPreview(
                    now: 1d,
                    hasAppliedDrivers: true,
                    updateRequested: false,
                    nextIdleProbe: 10d),
                Is.True);
            Assert.That(
                LookAtTimelinePreviewUpdater.ShouldPollPreview(
                    now: 1d,
                    hasAppliedDrivers: false,
                    updateRequested: true,
                    nextIdleProbe: 10d),
                Is.True);
            Assert.That(
                LookAtTimelinePreviewUpdater.ShouldPollPreview(
                    now: 1d,
                    hasAppliedDrivers: false,
                    updateRequested: false,
                    nextIdleProbe: 10d),
                Is.False);
            Assert.That(
                LookAtTimelinePreviewUpdater.ShouldPollPreview(
                    now: 10d,
                    hasAppliedDrivers: false,
                    updateRequested: false,
                    nextIdleProbe: 10d),
                Is.True);
        }
        [Test]
        public void TrackIconProvider_LoadsLookAtIcon()
        {
            var icon = LookAtTrackIconProvider.GetIcon();

            Assert.That(icon, Is.Not.Null);
            Assert.That(icon.name, Is.EqualTo("Look At Track Icon"));
        }
    }
}
