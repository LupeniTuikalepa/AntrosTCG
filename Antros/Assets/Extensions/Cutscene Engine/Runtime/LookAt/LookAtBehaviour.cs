using System;
using UnityEngine;
using UnityEngine.Playables;

namespace CutsceneEngine
{
    [Serializable]
    public sealed class LookAtBehaviour : PlayableBehaviour
    {
        internal LookAtClip sourceClip;
        internal IExposedPropertyTable resolver;
        public Transform target;
        public Transform directorTransform;
        public Vector3 position;
        public float eyesWeight;
        public float headWeight;
        public float neckWeight;
        public float bodyWeight;
        public float chinOffset;
        public LookAtAngleLimits eyesAngleLimits;
        public LookAtAngleLimits headAngleLimits;
        public LookAtAngleLimits neckAngleLimits;
        public LookAtAngleLimits bodyAngleLimits;
        public string[] blinkBlendShapeKeys;
        public LookAtBlinkMode blinkMode;
        public AnimationCurve blinkCurve;
        public float blinkFrequency;
        public float blinkDuration;
        public AnimationCurve automaticBlinkCurve;
        public float blinkNoiseOffset;
        public string[] upperEyelidFollowBlendShapeKeys;
        internal LookAtEyelidBlendShapeKey[] upperEyelidFollowKeyCache;
        public float upperEyelidFollowWeight;
        public AnimationCurve upperEyelidFollowCurve;
        public string[] lowerEyelidFollowBlendShapeKeys;
        internal LookAtEyelidBlendShapeKey[] lowerEyelidFollowKeyCache;
        public float lowerEyelidFollowWeight;
        public AnimationCurve lowerEyelidFollowCurve;
        public string[] horizontalEyelidFollowBlendShapeKeys;
        internal LookAtEyelidBlendShapeKey[] horizontalEyelidFollowKeyCache;
        public float horizontalEyelidFollowWeight;
        public AnimationCurve horizontalEyelidFollowCurve;

        internal LookAtSample CreateSample(
            float timelineWeight,
            double localTime,
            double localDuration)
        {
            return new LookAtSample
            {
                SourceClip = sourceClip,
                Resolver = resolver,
                Target = target,
                DirectorTransform = directorTransform,
                Position = position,
                TimelineWeight = timelineWeight,
                LocalTime = localTime,
                LocalDuration = localDuration,
                EyesWeight = eyesWeight,
                HeadWeight = headWeight,
                NeckWeight = neckWeight,
                BodyWeight = bodyWeight,
                ChinOffset = chinOffset,
                EyesAngleLimits = eyesAngleLimits,
                HeadAngleLimits = headAngleLimits,
                NeckAngleLimits = neckAngleLimits,
                BodyAngleLimits = bodyAngleLimits,
                BlinkBlendShapeKeys = blinkBlendShapeKeys,
                BlinkMode = blinkMode,
                BlinkCurve = blinkCurve,
                BlinkFrequency = blinkFrequency,
                BlinkDuration = blinkDuration,
                AutomaticBlinkCurve = automaticBlinkCurve,
                BlinkNoiseOffset = blinkNoiseOffset,
                UpperEyelidFollowBlendShapeKeys =
                    upperEyelidFollowBlendShapeKeys,
                UpperEyelidFollowKeyCache =
                    upperEyelidFollowKeyCache,
                UpperEyelidFollowWeight =
                    upperEyelidFollowWeight,
                UpperEyelidFollowCurve =
                    upperEyelidFollowCurve,
                LowerEyelidFollowBlendShapeKeys =
                    lowerEyelidFollowBlendShapeKeys,
                LowerEyelidFollowKeyCache =
                    lowerEyelidFollowKeyCache,
                LowerEyelidFollowWeight =
                    lowerEyelidFollowWeight,
                LowerEyelidFollowCurve =
                    lowerEyelidFollowCurve,
                HorizontalEyelidFollowBlendShapeKeys =
                    horizontalEyelidFollowBlendShapeKeys,
                HorizontalEyelidFollowKeyCache =
                    horizontalEyelidFollowKeyCache,
                HorizontalEyelidFollowWeight =
                    horizontalEyelidFollowWeight,
                HorizontalEyelidFollowCurve =
                    horizontalEyelidFollowCurve
            };
        }

        internal Vector3 ResolveTargetPosition()
        {
            return LookAtUtility.ResolveTargetPosition(
                target,
                directorTransform,
                position);
        }
    }
}
