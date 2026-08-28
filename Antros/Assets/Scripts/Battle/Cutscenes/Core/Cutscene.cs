using System;
using System.Threading;
using ATCG.Battle;
using ATCG.Battle.Players.Local.Runtime;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Object = UnityEngine.Object;

namespace ATCG.Cutscenes
{
    /// <summary>
    /// Plays a cutscene timeline for ONE screen: connects the <see cref="ICutsceneElement"/>s to a
    /// context, plays the PlayableDirector, raises <see cref="StepReached"/> when the timeline crosses
    /// a step marker, and — when the context supplies a QTE receiver — hosts the timeline's QTE
    /// windows (arbitrating local input, submitting scores). The generic counterpart of the capacity
    /// CapacityCutscene, used by the standalone <see cref="CutscenePlayer"/>.
    /// </summary>
    [RequireComponent(typeof(PlayableDirector))]
    [AddComponentMenu("ATCG/Cutscenes/Cutscene")]
    public class Cutscene : MonoBehaviour, INotificationReceiver, IQteWindowHost
    {
        public event Action<string> StepReached;

        [SerializeField]
        protected PlayableDirector playableDirector;

        protected ICutsceneElement[] elements;
        protected AwaitableCompletionSource finished;

        protected QteWindowArbiter arbiter;
        protected RuntimeLocalBattlePlayer screenPlayer;

        public RuntimeLocalBattlePlayer ScreenPlayer => screenPlayer;

        private void Awake() => elements = GetComponentsInChildren<ICutsceneElement>(true);

        private void Reset() => playableDirector = GetComponent<PlayableDirector>();

        /// <summary>Binds every element to the driving context. Call before <see cref="Play"/>.</summary>
        public void Configure(ICutsceneContext context)
        {
            EnsureDirector();

            elements ??= GetComponentsInChildren<ICutsceneElement>(true);
            for (int i = 0; i < elements.Length; i++)
                elements[i].Connect(context);

            screenPlayer = context.GetScreenBattlePlayer();

            // Sit the cutscene on the caster (position + rotation), like the capacity flow — otherwise
            // it plays at the world origin instead of at the hero.
            ICutsceneActor caster = context.GetCaster();
            if (caster != null)
                transform.SetPositionAndRotation(caster.transform.position, caster.transform.rotation);

            // Retag the cutscene's vcams to this screen's channel so only its brain shows the cutscene.
            ApplyScreenChannel();

            // Bind the auto-bindable tracks to live objects: HeroAnimator → caster Animator,
            // MainCamera → this screen's Cinemachine brain, Target → target Animator (if any).
            ResolveBindings(caster, context.GetTarget());

            // If the context provides a QTE receiver, host QTE windows for this screen: arbitrate
            // local presses and submit scores (the receiver decides whether to emit a networked
            // command). Cutscenes with no QTE simply never build an arbiter.
            IQteResultReceiver receiver = context.GetQteReceiver();
            if (screenPlayer != null && receiver != null)
            {
                arbiter = new QteWindowArbiter(screenPlayer, receiver);
                OnArbiterBuilt(arbiter);
                if (ShouldHookInput())
                    HookInput();
            }
        }

        protected virtual bool ShouldHookInput() => true;

        protected virtual void OnArbiterBuilt(QteWindowArbiter windowArbiter) { }

        private void EnsureDirector()
        {
            if (playableDirector == null)
                playableDirector = GetComponent<PlayableDirector>();
        }

        // Point this screen's brain at the cutscene by tagging its vcams with the screen's channel.
        private void ApplyScreenChannel()
        {
            if (screenPlayer == null)
                return;

            OutputChannels channel = screenPlayer.Camera.Component.OutputChannel;
            foreach (CinemachineCamera vcam in GetComponentsInChildren<CinemachineCamera>(true))
                vcam.OutputChannel = channel;
        }

        // Resolves the shared auto-bindable channels to live objects for this play.
        private void ResolveBindings(ICutsceneActor caster, ICutsceneActor target)
        {
            EnsureDirector();
            if (playableDirector.playableAsset is not TimelineAsset timeline)
                return;

            foreach (TrackAsset track in timeline.GetOutputTracks())
            {
                if (!CutsceneChannels.IsAutoBindableTrack(track))
                    continue;

                // Target is optional: bind only when the cutscene actually has a target (with an
                // Animator); no warning otherwise, unlike the required caster/camera channels.
                if (track.name == CutsceneChannels.Target.trackName)
                {
                    if (target?.Animator != null)
                        playableDirector.SetGenericBinding(track, target.Animator);
                    continue;
                }

                Object binding = null;
                if (track.name == CutsceneChannels.HeroAnimator.trackName)
                    binding = caster?.Animator;
                else if (track.name == CutsceneChannels.MainCamera.trackName && screenPlayer != null)
                    binding = screenPlayer.Camera.Component.CinemachineBrain;

                if (binding != null)
                    playableDirector.SetGenericBinding(track, binding);
                else
                    Debug.LogWarning($"[Cutscene] Channel '{track.name}' could not resolve a binding.");
            }
        }

        // A QTE clip registers/updates its window here every frame it's active.
        public void SetQteData(BattleID qteID, QteClipData data, double time, double duration)
            => arbiter?.SetQteData(qteID, data, time, duration);

        private void HookInput()
        {
            InputAction qte = screenPlayer?.Controls?.Component?.QTE;
            if (qte != null)
                qte.performed += OnQtePerformed;
        }

        private void UnhookInput()
        {
            InputAction qte = screenPlayer?.Controls?.Component?.QTE;
            if (qte != null)
                qte.performed -= OnQtePerformed;
        }

        private void OnQtePerformed(InputAction.CallbackContext _) => arbiter?.ResolvePress();

        public async Awaitable Play(CancellationToken token = default)
        {
            EnsureDirector();

            playableDirector.playOnAwake = false;
            playableDirector.extrapolationMode = DirectorWrapMode.None;

            finished = new AwaitableCompletionSource();
            playableDirector.stopped += OnStopped;
            playableDirector.Play();

            await finished.Awaitable;

            playableDirector.stopped -= OnStopped;
            finished = null;
        }

        public async Awaitable Stop(CancellationToken token = default)
        {
            EnsureDirector();
            playableDirector.Stop();
            await Awaitable.MainThreadAsync();
        }

        /// <summary>Disconnects the elements and destroys the instance.</summary>
        public virtual void Dispose()
        {
            UnhookInput();

            if (elements != null)
                for (int i = 0; i < elements.Length; i++)
                    elements[i].Disconnect();

            Destroy(gameObject);
        }

        private void OnDestroy() => UnhookInput();

        private void OnStopped(PlayableDirector _) => finished?.TrySetResult();

        public void OnNotify(Playable origin, INotification notification, object context)
        {
            if (notification is StepMarker step)
                StepReached?.Invoke(step.StepName);
        }
        
    }
}
