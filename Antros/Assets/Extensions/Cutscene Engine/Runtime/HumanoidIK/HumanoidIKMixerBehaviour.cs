using UnityEngine;
using UnityEngine.Playables;

namespace CutsceneEngine
{
    public class HumanoidIKMixerBehaviour : PlayableBehaviour
    {
        public HumanoidIKTarget target;

        HumanoidIKSample[] _samples = System.Array.Empty<HumanoidIKSample>();
        HumanoidIKLateUpdateDriver _driver;
        Animator _animator;

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            var animator = playerData as Animator;
            if (!HumanoidIKUtility.IsUsableHumanoid(animator))
            {
                ClearDriver();
                return;
            }

            if (_animator != animator)
            {
                ClearDriver();
                _animator = animator;
            }

            var trackWeight = Mathf.Clamp01(info.effectiveWeight);
            var inputCount = playable.GetInputCount();
            EnsureSampleCapacity(inputCount);
            var sampleCount = 0;

            for (var i = 0; i < inputCount; i++)
            {
                var inputWeight = playable.GetInputWeight(i) * trackWeight;
                if (inputWeight <= 0f) continue;

                var inputPlayable = (ScriptPlayable<HumanoidIKBehaviour>)playable.GetInput(i);
                var input = inputPlayable.GetBehaviour();

                _samples[sampleCount++] = new HumanoidIKSample
                {
                    Anchor = input.anchorTransform,
                    Position = input.position,
                    Rotation = input.rotation,
                    RotationSpace = input.rotationSpace,
                    FootRotationFrameVersion = input.footRotationFrameVersion,
                    BendTarget = input.bendTarget,
                    BendSpace = input.bendSpace,
                    TimelineWeight = inputWeight,
                    PositionWeight = Mathf.Clamp01(input.positionWeight),
                    RotationWeight = Mathf.Clamp01(input.rotationWeight),
                    BendWeight = Mathf.Clamp01(input.bendWeight),
                    DigitWeight = Mathf.Clamp01(input.digitWeight),
                    DigitBends = input.digitBends,
                    ToeBaseBend = Mathf.Clamp(input.toeBaseBend, -1f, 1f),
                    ToeFan = Mathf.Clamp(input.toeFan, -1f, 1f),
                    ToeBendRanges = input.toeBendRanges,
                    ToeBaseBendRange = input.toeBaseBendRange
                };
            }

            if (sampleCount == 0)
            {
                ClearDriverGoal();
                return;
            }

            var state = new HumanoidIKGoalState
            {
                Active = true,
                Samples = _samples,
                SampleCount = sampleCount
            };

            if (!_driver)
            {
                _driver = HumanoidIKLateUpdateDriver.GetOrCreate(animator);
            }
            _driver.SetState(target, state);

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                _driver.ScheduleEditorApplyCurrentStates();
            }
#endif
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            if (info.effectiveWeight <= 0f) ClearDriverGoal();
        }

        public override void OnPlayableDestroy(Playable playable)
        {
            ClearDriver();
        }

        void ClearDriver()
        {
            var driver = _driver;
            _driver = null;
            _animator = null;
            if (!driver) return;

            driver.ClearState(target);
            driver.ReleaseTimelineOwner();
        }

        void ClearDriverGoal()
        {
            if (!_driver) return;
            _driver.ClearState(target);
        }

        void EnsureSampleCapacity(int requiredCapacity)
        {
            if (_samples.Length >= requiredCapacity) return;

            var nextCapacity = Mathf.NextPowerOfTwo(Mathf.Max(requiredCapacity, 1));
            _samples = new HumanoidIKSample[nextCapacity];
        }
    }
}
