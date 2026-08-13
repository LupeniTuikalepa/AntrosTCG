using System;
using ATCG.Cutscenes;
using System.Collections.Generic;
using System.Threading;
using ATCG.Battle.CapacitySystem.Core.Cutscenes.Elements;
using ATCG.Battle.CapacitySystem.Core.Cutscenes.QTEs;
using ATCG.Battle.CapacitySystem.Core.Cutscenes.Tracks;
using ATCG.Battle.CapacitySystem.Core.Properties;
using ATCG.Battle.Entities.Runtime;
using ATCG.Battle.Players.Local.Runtime;
using ATCG.Metrics;
using Helteix.Tools;
using Unity.Cinemachine;
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

        // The "HeroAnimator" track binds straight onto the real caster's own Animator
        // (see CutsceneChannels.ResolveHeroAnimator) — root motion or keyframed position
        // in the clip moves the actual battle entity, and Timeline never reverts a driven
        // transform when it stops. Cached here and restored in Dispose() so the caster
        // ends up back on its hex cell instead of wherever the clip's last frame left it.
        private IRuntimeEntity runtimeCaster;
        private Vector3 casterInitialPosition;
        private Quaternion casterInitialRotation;

        public bool IsHost => castCapacityPhase.casterPlayerId == ScreenPlayer.BattlePlayer.ID;

        private QteWindowArbiter arbiter;

        private void Awake() => elements = GetComponentsInChildren<ICapacityCutsceneElement>(true);

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

            // Window bookkeeping + press arbitration are generic; delegate to the shared arbiter and
            // re-raise its events with the phase so the capacity HUD (QteUIController) keeps working.
            arbiter = new QteWindowArbiter(screenPlayer, resultReceiver);
            arbiter.WindowOpened += qte => OnQteWindowOpened?.Invoke(castCapacityPhase, qte);
            arbiter.WindowClosed += qte => OnQteWindowClosed?.Invoke(castCapacityPhase, qte);
            arbiter.Resolved += qte => OnQteResolved?.Invoke(castCapacityPhase, qte);

            if (capacityPhase.TryGetRuntimeCaster(screenPlayer, out IRuntimeEntity runtimeEntity))
            {
                transform.position = runtimeEntity.transform.position;
                transform.rotation = runtimeEntity.transform.rotation;

                runtimeCaster = runtimeEntity;
                casterInitialPosition = runtimeEntity.transform.position;
                casterInitialRotation = runtimeEntity.transform.rotation;
            }

            // Build the per-screen context (this is what injects the built-in properties:
            // CASTER, SCREEN_PLAYER, CAST_POINT, COORDINATE_SOLVER...). Without this the
            // elements receive a context that never had the built-ins injected.
            CutsceneCapacityContext context = new(capacityPhase, screenPlayer);
            for (int i = 0; i < elements.Length; i++)
                elements[i]. Connect(context);

            ApplyScreenChannel();
        }

        // The cutscene prefab's vcams default to the Default output channel, which every
        // player's brain listens to — so a per-screen cutscene leaks onto the other screens
        // (all of them ended up on the second player's camera). Re-tag them with THIS screen's
        // pure channel so only its brain shows the cutscene.
        private void ApplyScreenChannel()
        {
            if (ScreenPlayer == null)
                return;

            OutputChannels channel = ScreenPlayer.Camera.Component.OutputChannel;
            foreach (CinemachineCamera vcam in GetComponentsInChildren<CinemachineCamera>(true))
                vcam.OutputChannel = channel;
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


        public void Dispose()
        {
            for (int i = 0; i < elements.Length; i++)
                elements[i].Disconnect();

            if (runtimeCaster != null)
            {
                runtimeCaster.transform.position = casterInitialPosition;
                runtimeCaster.transform.rotation = casterInitialRotation;
            }

            gameObject.Destroy();
        }

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
            => arbiter?.SetQteData(qteID, data, time, duration);

        private void OnQtePerformed(InputAction.CallbackContext _) => arbiter?.ResolvePress();
        public void ResolveBindings()
        {
            if (playableDirector.playableAsset is not TimelineAsset timeline)
                return;

            CutsceneBindContext ctx = new(castCapacityPhase, ScreenPlayer);

            foreach (TrackAsset track in timeline.GetOutputTracks())
            {
                if (!CutsceneChannels.IsAutoBindableTrack(track))
                    continue;

                if (CapacityCutsceneBindings.TryGetBinding(track.name, ctx, out Object binding))
                    playableDirector.SetGenericBinding(track, binding);
                else
                    Debug.LogWarning($"[Cutscene] Channel '{track}' could not resolve a binding.");
            }
        }

    }
}
