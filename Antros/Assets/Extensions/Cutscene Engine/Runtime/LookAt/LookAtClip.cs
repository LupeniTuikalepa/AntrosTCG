using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace CutsceneEngine
{
    public enum LookAtBlinkMode
    {
        Automatic,
        AnimationCurve
    }

    public sealed class LookAtClip : PlayableAsset, ITimelineClipAsset
    {
        internal const float MinimumAutomaticBlinkDuration = 0.03f;
        internal const float MaximumAutomaticBlinkDuration = 0.5f;
        internal const float DefaultAutomaticBlinkDuration = 0.185f;
        internal const float DefaultChinOffset = -0.1f;
        internal const float MaximumChinPitchOffsetDegrees = 30f;
        internal const int CurrentAutomaticBlinkCurveSemanticsVersion = 1;
        internal static readonly Vector3 DefaultLocalPosition = new Vector3(0f, 1f, 1f);
        internal static readonly Color DefaultGizmoColor =
            new Color(0.72f, 0.48f, 1f, 0.95f);
        internal static readonly LookAtAngleLimits DefaultEyesAngleLimits =
            new LookAtAngleLimits(new Vector2(-40f, 40f), new Vector2(-25f, 25f));
        internal static readonly LookAtAngleLimits DefaultBodyPartAngleLimits =
            new LookAtAngleLimits(new Vector2(-90f, 90f), new Vector2(-90f, 90f));

        [Tooltip("Optional Transform whose world position the character looks at. When unassigned, Position is used.")]
        public ExposedReference<Transform> target;

        [Tooltip("PlayableDirector-local target position used when Target is unassigned or cannot be resolved.")]
        public Vector3 position = DefaultLocalPosition;

        [Tooltip("Color and opacity shared by this clip's Scene view target gizmo and Timeline accent.")]
        public Color gizmoColor = DefaultGizmoColor;

        [Range(0f, 1f)]
        [Tooltip("How strongly the mapped eye bones turn toward Target.")]
        public float eyesWeight = 1f;

        [Range(0f, 1f)]
        [Tooltip("How strongly the head turns toward Target.")]
        public float headWeight = 0.5f;

        [Range(0f, 1f)]
        [Tooltip("How strongly the neck turns toward Target.")]
        public float neckWeight = 0.2f;

        [Range(0f, 1f)]
        [Tooltip("How strongly the mapped Spine, Chest, and Upper Chest bones share the turn toward Target.")]
        public float bodyWeight = 0.05f;

        [Range(-1f, 1f)]
        [Tooltip("Offsets the head pitch while the eyes keep looking at Target. Negative lowers the chin; positive raises it.")]
        public float chinOffset = DefaultChinOffset;

        [Tooltip("Horizontal (yaw) and vertical (pitch) rotation limits for the eye bones.")]
        public LookAtAngleLimits eyesAngleLimits = DefaultEyesAngleLimits;

        [Tooltip("Horizontal (yaw) and vertical (pitch) rotation limits for the head.")]
        public LookAtAngleLimits headAngleLimits = DefaultBodyPartAngleLimits;

        [Tooltip("Horizontal (yaw) and vertical (pitch) rotation limits for the neck.")]
        public LookAtAngleLimits neckAngleLimits = DefaultBodyPartAngleLimits;

        [Tooltip("Horizontal (yaw) and vertical (pitch) rotation limits shared by the upper-body chain.")]
        public LookAtAngleLimits bodyAngleLimits = DefaultBodyPartAngleLimits;

        [Tooltip("BlendShape names used for blinking. Every matching SkinnedMeshRenderer under the bound character is driven.")]
        public string[] blinkBlendShapeKeys =
        {
            "Eye_Blink_L",
            "Eye_Blink_R"
        };

        [Tooltip("Automatic generates deterministic blinks from clip time. Animation Curve directly authors eyelid openness.")]
        public LookAtBlinkMode blinkMode = LookAtBlinkMode.Automatic;

        [Tooltip("Normalized eyelid openness over normalized clip time: 0 is closed and 1 is open.")]
        public AnimationCurve blinkCurve = new AnimationCurve(
            new Keyframe(0f, 1f),
            new Keyframe(0.35f, 0f),
            new Keyframe(0.55f, 0f),
            new Keyframe(1f, 1f));

        [Range(0f, 1f)]
        [Tooltip("Automatic blink frequency. 0 disables blinking; 1 blinks very frequently.")]
        public float blinkFrequency = 0.5f;

        [Range(MinimumAutomaticBlinkDuration, MaximumAutomaticBlinkDuration)]
        [Tooltip("Total time, in seconds, for one automatic blink to close, briefly hold, and reopen.")]
        public float blinkDuration = DefaultAutomaticBlinkDuration;

        [Tooltip("Normalized eyelid openness over one automatic blink: 0 is closed and 1 is open.")]
        public AnimationCurve automaticBlinkCurve =
            LookAtUtility.CreateDefaultAutomaticBlinkCurve();

        [SerializeField, HideInInspector]
        internal int automaticBlinkCurveSemanticsVersion;

        [Tooltip("Shifts the deterministic Perlin noise pattern used to place automatic blinks.")]
        public float blinkNoiseOffset;

        [Tooltip("Upper-eyelid BlendShape names driven while looking down.")]
        public string[] upperEyelidFollowBlendShapeKeys =
        {
            "Eye_Lid_Upper_L",
            "Eye_Lid_Upper_R"
        };

        [Range(0f, 1f)]
        [Tooltip("Maximum upper-eyelid response while looking down.")]
        public float upperEyelidFollowWeight = 0.5f;

        [Tooltip("Upper-eyelid response by vertical eye direction: 0 is maximum down, 0.5 is forward, and 1 is maximum up.")]
        public AnimationCurve upperEyelidFollowCurve =
            LookAtUtility.CreateDefaultUpperEyelidFollowCurve();

        [Tooltip("Lower-eyelid BlendShape names driven while looking up.")]
        public string[] lowerEyelidFollowBlendShapeKeys =
        {
            "Eye_Lid_Lower_L",
            "Eye_Lid_Lower_R"
        };

        [Range(0f, 1f)]
        [Tooltip("Maximum lower-eyelid response while looking up.")]
        public float lowerEyelidFollowWeight = 0.5f;

        [Tooltip("Lower-eyelid response by vertical eye direction: 0 is maximum down, 0.5 is forward, and 1 is maximum up.")]
        public AnimationCurve lowerEyelidFollowCurve =
            LookAtUtility.CreateDefaultLowerEyelidFollowCurve();

        [Tooltip("Horizontal eyelid-muscle BlendShape names driven while looking left or right.")]
        public string[] horizontalEyelidFollowBlendShapeKeys =
        {
            "Eye_L_Look_L",
            "Eye_R_Look_L",
            "Eye_L_Look_R",
            "Eye_R_Look_R"
        };

        [Range(0f, 1f)]
        [Tooltip("Maximum horizontal eyelid-muscle response while looking left or right.")]
        public float horizontalEyelidFollowWeight = 0.5f;

        [Tooltip("Horizontal eyelid response by eye direction: 0 is maximum left, 0.5 is forward, and 1 is maximum right.")]
        public AnimationCurve horizontalEyelidFollowCurve =
            LookAtUtility.CreateDefaultHorizontalEyelidFollowCurve();

        public ClipCaps clipCaps => ClipCaps.Blending | ClipCaps.Extrapolation;

        internal Transform ResolveTarget(IExposedPropertyTable resolver)
        {
            return target.Resolve(resolver);
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<LookAtBehaviour>.Create(graph);
            var behaviour = playable.GetBehaviour();
            behaviour.sourceClip = this;
            behaviour.resolver = graph.GetResolver();
            behaviour.target = ResolveTarget(graph.GetResolver());
            behaviour.directorTransform = owner ? owner.transform : null;
            behaviour.position = position;
            behaviour.eyesWeight = Mathf.Clamp01(eyesWeight);
            behaviour.headWeight = Mathf.Clamp01(headWeight);
            behaviour.neckWeight = Mathf.Clamp01(neckWeight);
            behaviour.bodyWeight = Mathf.Clamp01(bodyWeight);
            behaviour.chinOffset =
                LookAtUtility.SanitizeChinOffset(chinOffset);
            behaviour.eyesAngleLimits = eyesAngleLimits.Sanitized();
            behaviour.headAngleLimits = headAngleLimits.Sanitized();
            behaviour.neckAngleLimits = neckAngleLimits.Sanitized();
            behaviour.bodyAngleLimits = bodyAngleLimits.Sanitized();
            behaviour.blinkBlendShapeKeys = blinkBlendShapeKeys;
            behaviour.blinkMode = blinkMode;
            behaviour.blinkCurve = blinkCurve;
            behaviour.blinkFrequency = Mathf.Clamp01(blinkFrequency);
            behaviour.blinkDuration = Mathf.Clamp(
                blinkDuration,
                MinimumAutomaticBlinkDuration,
                MaximumAutomaticBlinkDuration);
            behaviour.automaticBlinkCurve = automaticBlinkCurve;
            behaviour.blinkNoiseOffset =
                LookAtUtility.SanitizeBlinkNoiseOffset(blinkNoiseOffset);
            behaviour.upperEyelidFollowBlendShapeKeys =
                upperEyelidFollowBlendShapeKeys;
            behaviour.upperEyelidFollowKeyCache =
                LookAtUtility.CacheEyelidBlendShapeKeys(
                    upperEyelidFollowBlendShapeKeys,
                    LookAtEyelidDirection.Down);
            behaviour.upperEyelidFollowWeight =
                Mathf.Clamp01(upperEyelidFollowWeight);
            behaviour.upperEyelidFollowCurve =
                upperEyelidFollowCurve;
            behaviour.lowerEyelidFollowBlendShapeKeys =
                lowerEyelidFollowBlendShapeKeys;
            behaviour.lowerEyelidFollowKeyCache =
                LookAtUtility.CacheEyelidBlendShapeKeys(
                    lowerEyelidFollowBlendShapeKeys,
                    LookAtEyelidDirection.Up);
            behaviour.lowerEyelidFollowWeight =
                Mathf.Clamp01(lowerEyelidFollowWeight);
            behaviour.lowerEyelidFollowCurve =
                lowerEyelidFollowCurve;
            behaviour.horizontalEyelidFollowBlendShapeKeys =
                horizontalEyelidFollowBlendShapeKeys;
            behaviour.horizontalEyelidFollowKeyCache =
                LookAtUtility.CacheEyelidBlendShapeKeys(
                    horizontalEyelidFollowBlendShapeKeys,
                    LookAtEyelidDirection.Horizontal);
            behaviour.horizontalEyelidFollowWeight =
                Mathf.Clamp01(horizontalEyelidFollowWeight);
            behaviour.horizontalEyelidFollowCurve =
                horizontalEyelidFollowCurve;
            return playable;
        }

        void OnEnable()
        {
            UpgradeAutomaticBlinkCurveSemantics();
        }

        void OnValidate()
        {
            eyesWeight = Mathf.Clamp01(eyesWeight);
            headWeight = Mathf.Clamp01(headWeight);
            neckWeight = Mathf.Clamp01(neckWeight);
            bodyWeight = Mathf.Clamp01(bodyWeight);
            chinOffset = LookAtUtility.SanitizeChinOffset(chinOffset);
            eyesAngleLimits = eyesAngleLimits.Sanitized();
            headAngleLimits = headAngleLimits.Sanitized();
            neckAngleLimits = neckAngleLimits.Sanitized();
            bodyAngleLimits = bodyAngleLimits.Sanitized();
            blinkBlendShapeKeys ??= new[]
            {
                "Eye_Blink_L",
                "Eye_Blink_R"
            };
            blinkCurve ??= new AnimationCurve(
                new Keyframe(0f, 1f),
                new Keyframe(0.35f, 0f),
                new Keyframe(0.55f, 0f),
                new Keyframe(1f, 1f));
            LookAtUtility.ClampCurve01(blinkCurve);
            blinkFrequency = Mathf.Clamp01(blinkFrequency);
            blinkDuration = Mathf.Clamp(
                blinkDuration,
                MinimumAutomaticBlinkDuration,
                MaximumAutomaticBlinkDuration);
            UpgradeAutomaticBlinkCurveSemantics();
            automaticBlinkCurve ??=
                LookAtUtility.CreateDefaultAutomaticBlinkCurve();
            if (automaticBlinkCurve.length == 0)
            {
                automaticBlinkCurve =
                    LookAtUtility.CreateDefaultAutomaticBlinkCurve();
            }
            LookAtUtility.ClampCurve01(automaticBlinkCurve);
            blinkNoiseOffset =
                LookAtUtility.SanitizeBlinkNoiseOffset(blinkNoiseOffset);

            upperEyelidFollowBlendShapeKeys ??= new[]
            {
                "Eye_Lid_Upper_L",
                "Eye_Lid_Upper_R"
            };
            upperEyelidFollowWeight =
                Mathf.Clamp01(upperEyelidFollowWeight);
            upperEyelidFollowCurve ??=
                LookAtUtility.CreateDefaultUpperEyelidFollowCurve();
            if (upperEyelidFollowCurve.length == 0)
            {
                upperEyelidFollowCurve =
                    LookAtUtility.CreateDefaultUpperEyelidFollowCurve();
            }
            LookAtUtility.ClampCurve01(upperEyelidFollowCurve);

            lowerEyelidFollowBlendShapeKeys ??= new[]
            {
                "Eye_Lid_Lower_L",
                "Eye_Lid_Lower_R"
            };
            lowerEyelidFollowWeight =
                Mathf.Clamp01(lowerEyelidFollowWeight);
            lowerEyelidFollowCurve ??=
                LookAtUtility.CreateDefaultLowerEyelidFollowCurve();
            if (lowerEyelidFollowCurve.length == 0)
            {
                lowerEyelidFollowCurve =
                    LookAtUtility.CreateDefaultLowerEyelidFollowCurve();
            }
            LookAtUtility.ClampCurve01(lowerEyelidFollowCurve);

            horizontalEyelidFollowBlendShapeKeys ??= new[]
            {
                "Eye_L_Look_L",
                "Eye_R_Look_L",
                "Eye_L_Look_R",
                "Eye_R_Look_R"
            };
            horizontalEyelidFollowWeight =
                Mathf.Clamp01(horizontalEyelidFollowWeight);
            horizontalEyelidFollowCurve ??=
                LookAtUtility.CreateDefaultHorizontalEyelidFollowCurve();
            if (horizontalEyelidFollowCurve.length == 0)
            {
                horizontalEyelidFollowCurve =
                    LookAtUtility.CreateDefaultHorizontalEyelidFollowCurve();
            }
            LookAtUtility.ClampCurve01(horizontalEyelidFollowCurve);
        }

        internal void UpgradeAutomaticBlinkCurveSemantics()
        {
            if (automaticBlinkCurveSemanticsVersion >=
                CurrentAutomaticBlinkCurveSemanticsVersion)
            {
                return;
            }

            if (automaticBlinkCurve == null ||
                automaticBlinkCurve.length == 0)
            {
                automaticBlinkCurve =
                    LookAtUtility.CreateDefaultAutomaticBlinkCurve();
            }
            else if (!LookAtUtility.UsesAutomaticBlinkOpennessConvention(
                         automaticBlinkCurve))
            {
                automaticBlinkCurve =
                    LookAtUtility.InvertCurveVertically01(
                        automaticBlinkCurve);
            }

            automaticBlinkCurveSemanticsVersion =
                CurrentAutomaticBlinkCurveSemanticsVersion;
        }

        internal bool HasAnyEffect()
        {
            return eyesWeight > 0f ||
                   headWeight > 0f ||
                   neckWeight > 0f ||
                   bodyWeight > 0f ||
                   HasBlinkConfiguration() ||
                   HasEyelidFollowConfiguration();
        }

        internal bool HasBlinkConfiguration()
        {
            if (!LookAtUtility.HasAnyKey(blinkBlendShapeKeys))
            {
                return false;
            }

            return blinkMode == LookAtBlinkMode.AnimationCurve
                ? blinkCurve != null && blinkCurve.length > 0
                : blinkFrequency > 0f;
        }

        internal bool HasEyelidFollowConfiguration()
        {
            return HasEyelidFollowChannel(
                       upperEyelidFollowBlendShapeKeys,
                       upperEyelidFollowWeight,
                       upperEyelidFollowCurve) ||
                   HasEyelidFollowChannel(
                       lowerEyelidFollowBlendShapeKeys,
                       lowerEyelidFollowWeight,
                       lowerEyelidFollowCurve) ||
                   HasEyelidFollowChannel(
                       horizontalEyelidFollowBlendShapeKeys,
                       horizontalEyelidFollowWeight,
                       horizontalEyelidFollowCurve);
        }

        static bool HasEyelidFollowChannel(
            string[] keys,
            float weight,
            AnimationCurve curve)
        {
            return LookAtUtility.HasAnyKey(keys) &&
                   weight > 0f &&
                   curve != null &&
                   curve.length > 0;
        }


    }
}
