using UnityEngine;
using UnityEngine.Playables;

namespace CutsceneEngine
{
    public sealed class LookAtMixerBehaviour : PlayableBehaviour
    {
        internal LookAtTrack sourceTrack;

        LookAtSample[] _samples = System.Array.Empty<LookAtSample>();
        LookAtLateUpdateDriver _driver;
        Animator _animator;

        public override void ProcessFrame(
            Playable playable,
            FrameData info,
            object playerData)
        {
            var animator = playerData as Animator;
            if (!animator)
            {
                ClearDriver();
                return;
            }

            if (_animator != animator)
            {
                ClearDriver();
                _animator = animator;
            }

            var inputCount = playable.GetInputCount();
            EnsureSampleCapacity(inputCount);
            var sampleCount = 0;
            var trackWeight = Mathf.Clamp01(info.effectiveWeight);

            for (var i = 0; i < inputCount; i++)
            {
                var timelineWeight = playable.GetInputWeight(i) * trackWeight;
                if (timelineWeight <= 0f) continue;

                var inputPlayable = (ScriptPlayable<LookAtBehaviour>)playable.GetInput(i);
                var input = inputPlayable.GetBehaviour();
                _samples[sampleCount++] = input.CreateSample(
                    timelineWeight,
                    inputPlayable.GetTime(),
                    inputPlayable.GetDuration());
            }

            if (sampleCount == 0)
            {
                ClearDriverState();
                return;
            }

            if (!_driver)
            {
                _driver = LookAtLateUpdateDriver.GetOrCreate(animator);
            }

            _driver.SetState(new LookAtState
            {
                Active = true,
                Samples = _samples,
                SampleCount = sampleCount,
                SourceTrack = sourceTrack
            });

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                _driver.ScheduleEditorApplyCurrentStates();
            }
#endif
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            if (info.effectiveWeight <= 0f) ClearDriverState();
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

            driver.ClearState();
            driver.ReleaseTimelineOwner();
        }

        void ClearDriverState()
        {
            if (_driver) _driver.ClearState();
        }

        void EnsureSampleCapacity(int requiredCapacity)
        {
            if (_samples.Length >= requiredCapacity) return;

            var nextCapacity = Mathf.NextPowerOfTwo(Mathf.Max(requiredCapacity, 1));
            _samples = new LookAtSample[nextCapacity];
        }
    }
}
