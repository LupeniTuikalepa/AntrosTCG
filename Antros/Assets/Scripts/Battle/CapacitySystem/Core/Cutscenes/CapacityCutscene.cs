using System;
using System.Collections.Generic;
using System.Threading;
using ATCG.Battle.CapacitySystem.Core.Cutscenes.CutsceneElement;
using ATCG.Battle.CapacitySystem.Core.Cutscenes.QTEs;
using ATCG.Battle.Entities.Runtime;
using ATCG.Battle.Entities.Runtime.Animations;
using ATCG.Battle.Players.Local.Runtime;
using ATCG.Core.Cutscenes;
using ATCG.Metrics;
using Helteix.Tools;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Object = UnityEngine.Object;

namespace ATCG.Battle.CapacitySystem.Core.Cutscenes
{
    /// <summary>
    /// Plays a capacity timeline for ONE screen. Network-agnostic: it plays the
    /// same on both sides, shows the gauges, reads local input, arbitrates presses
    /// (FIFO) across open QTE windows, and SIGNALS each resolved score to the sink.
    /// The sink (the director) decides whether to emit a QteCommand based on role.
    /// </summary>
    [RequireComponent(typeof(PlayableDirector))]
    [AddComponentMenu("ATCG/Gameplay/Capacities/Capacity Cutscene")]
    public class CapacityCutscene : MonoBehaviour, IQteWindowHost, INotificationReceiver
    {
        public event Action<CastCapacityPhase, Qte> OnQteWindowOpened;
        public event Action<CastCapacityPhase, Qte> OnQteWindowClosed;
        public event Action<CastCapacityPhase, Qte> OnQteResolved;
        public event Action<string> StepReached;

        public RuntimeLocalBattlePlayer ScreenPlayer { get; private set; }

        [SerializeField]
        private PlayableDirector playableDirector;

        private AwaitableCompletionSource finished;

        private CastCapacityPhase castCapacityPhase;
        private IQteResultReceiver resultReceiver;

        private ICapacityCutsceneElement[] elements;

        public bool IsHost => castCapacityPhase.casterPlayerId == ScreenPlayer.BattlePlayer.ID;

        private readonly Dictionary<BattleID, Qte> qtes = new();

        private void Awake() => elements = GetComponentsInChildren<ICapacityCutsceneElement>();

        private void Reset() => playableDirector = GetComponent<PlayableDirector>();

        /// <summary>Injected by the director before Play. The sink receives QTE scores.</summary>
        public void Configure(
            CastCapacityPhase capacityPhase,
            RuntimeLocalBattlePlayer screenPlayer,
            IQteResultReceiver resultReceiver)
        {
            ScreenPlayer = screenPlayer;
            this.castCapacityPhase = capacityPhase;
            this.resultReceiver = resultReceiver;

            if (capacityPhase.TryGetRuntimeCaster(screenPlayer, out IRuntimeEntity runtimeEntity))
            {
                transform.position = runtimeEntity.transform.position;
                transform.rotation = runtimeEntity.transform.rotation;
            }

            for (int i = 0; i < elements.Length; i++)
                elements[i].Connect(screenPlayer, capacityPhase);

        }

        public async Awaitable Play(CancellationToken token)
        {
            playableDirector.playOnAwake = false;
            playableDirector.extrapolationMode = DirectorWrapMode.None;

            ResolveBindings();
            HookInput();

            finished = new AwaitableCompletionSource();
            playableDirector.stopped += OnDirectorStopped;
            playableDirector.Play();

            await finished.Awaitable;
            playableDirector.stopped -= OnDirectorStopped;
            finished = null;

            UnhookInput();
        }

        public async Awaitable Stop(CancellationToken token)
        {
            playableDirector.Stop();
            await Awaitable.MainThreadAsync();
        }

        public void Dispose() => gameObject.Destroy();

        private void OnDirectorStopped(PlayableDirector _) => finished?.TrySetResult();

        public void OnNotify(Playable origin, INotification notification, object context)
        {
            if (notification is StepMarker step)
                StepReached?.Invoke(step.StepName);
        }

        // ---- input (single subscription for all QTEs of this cutscene) ------

        private void HookInput()
        {
            if (IsHost)
            {
                var qteAction = ScreenPlayer?.Controls?.Component?.QTE;
                if (qteAction != null)
                {
                    qteAction.performed += OnQtePerformed;
                }
            }
        }

        private void UnhookInput()
        {
            if (IsHost)
            {
                var qteAction = ScreenPlayer?.Controls?.Component?.QTE;

                if (qteAction != null)
                    qteAction.performed -= OnQtePerformed;
            }
        }

        private void OnDestroy() => UnhookInput();


        public void SetQteData(BattleID qteID, QteClipData data, double time, double duration)
        {
            if (!qtes.TryGetValue(qteID, out Qte target))
            {
                target = new Qte(ScreenPlayer, duration, data);
                qtes.Add(qteID, target);

                OnQteWindowOpened?.Invoke(castCapacityPhase, target);
            }

            target.SetDuration(duration);
            target.SetTime(time);

            if (time >= duration)
            {
                if (!target.IsDone)
                    resultReceiver?.SubmitQteResult(0f);

                qtes.Remove(qteID);
                OnQteWindowClosed?.Invoke(castCapacityPhase, target);
            }
        }

        private void OnQtePerformed(InputAction.CallbackContext _)
        {

            Qte target = null;
            double lastNorm = 0;

            foreach ((BattleID id, Qte qte) in qtes)
            {
                if(qte.NormalizedTime > lastNorm)
                {
                    target = qte;
                    lastNorm = qte.NormalizedTime;
                }
            }
            if (target == null)
                return;

            float criticalPortion = GameMetrics.Current.QTESuccessRange;
            float threshold = 1f - criticalPortion;
            float score = target.NormalizedTime >= threshold ? 1f : 0f;

            target.Resolve();
            resultReceiver?.SubmitQteResult(score);
            OnQteResolved?.Invoke(castCapacityPhase, target);
        }

        public void ResolveBindings()
        {
            if (playableDirector == null)
                return;
            if (playableDirector.playableAsset is not TimelineAsset timeline)
                return;

            foreach (TrackAsset track in timeline.GetOutputTracks())
            {
                switch (track.name)
                {
                    case CutsceneTrackNames.HERO_ANIMATOR:
                        if (!castCapacityPhase.caster.IsValid)
                        {
                            Debug.LogWarning($"[Cutscene] No animator for track '{track.name}': the capacity has no caster.");
                            break;
                        }
                        if (ScreenPlayer.RuntimeEntityManager.TryGetRuntimeEntity(castCapacityPhase.caster, out IRuntimeEntity runtimeEntity))
                        {
                            if (runtimeEntity is IRuntimeEntityWithAnimator withAnimator)
                                playableDirector.SetGenericBinding(track, withAnimator.Animator);
                            else
                                Debug.LogWarning($"[Cutscene] No animator for track '{track.name}'.");
                        }
                        break;

                    case CutsceneTrackNames.MAIN_CAMERA:
                        Object cameraBinding = ScreenPlayer.Camera.Component.CinemachineBrain;
                        if (cameraBinding != null)
                            playableDirector.SetGenericBinding(track, cameraBinding);
                        else
                            Debug.LogWarning($"[Cutscene] No camera for track '{track.name}'.");
                        break;
                }
            }
        }

    }
}