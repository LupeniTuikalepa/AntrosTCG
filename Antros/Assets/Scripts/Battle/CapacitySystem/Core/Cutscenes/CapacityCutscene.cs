using System;
using System.Threading;
using ATCG.Battle.CapacitySystem.Core.Cutscenes.CutsceneElement;
using ATCG.Battle.CapacitySystem.Core.Cutscenes.Timeline;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.GameCommands.Capacities;
using ATCG.Battle.Entities.Runtime;
using ATCG.Battle.Entities.Runtime.Animations;
using ATCG.Battle.Entities.Runtime.Heroes;
using ATCG.Battle.Players;
using ATCG.Battle.Players.Local;
using ATCG.Battle.Players.Local.Runtime;
using ATCG.Core.Cutscenes;
using Helteix.Tools;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Object = UnityEngine.Object;

namespace ATCG.Battle.CapacitySystem.Core.Cutscenes
{
    /// <summary>
    /// Plays the capacity timeline straight through for one screen.
    ///  - StepMarkers fire StepReached (relayed by the director to the phase).
    ///  - QTE clips drive QTE windows through IQteWindowHost. Only the OWNER screen
    ///    reads input and emits the QteCommand; other screens just play the gauge.
    ///
    /// The owner flag and the casting player are injected by the director (it knows
    /// the role). The critical-window width comes from a global game metric.
    /// </summary>
    [RequireComponent(typeof(PlayableDirector))]
    [AddComponentMenu("ATCG/Gameplay/Capacities/Capacity Cutscene")]
    public class CapacityCutscene : MonoBehaviour, ICapacityCutscene, IQteWindowHost, INotificationReceiver
    {
        public event Action<string> StepReached;

        public bool IsOwner => ScreenPlayer.BattlePlayer.ID == castCapacityPhase.casterPlayerId;
        public IBattlePlayer CastingPlayer => castCapacityPhase.CasterPlayer;

        // Injected by the director before Play.
        public RuntimeLocalBattlePlayer ScreenPlayer { get; private set; }

        [SerializeField]
        private PlayableDirector playableDirector;

        private AwaitableCompletionSource finished;

        private CastCapacityPhase castCapacityPhase;
        private bool pressedThisWindow;
        private float pendingResult;

        private ICapacityCutsceneElement[] elements;

        private void Awake()
        {
            elements = GetComponentsInChildren<ICapacityCutsceneElement>();
        }

        private void Reset()
        {
            playableDirector = GetComponent<PlayableDirector>();
        }

        /// <summary>Called by the director before Play, so the cutscene knows its role.</summary>
        public void Configure(CastCapacityPhase capacityPhase, RuntimeLocalBattlePlayer screenPlayer)
        {
            ScreenPlayer = screenPlayer;
            for (int i = 0; i < elements.Length; i++)
                elements[i].Connect(screenPlayer, capacityPhase);

            this.castCapacityPhase = capacityPhase;

            if (capacityPhase.HasCaster && screenPlayer.RuntimeEntityManager.TryGetRuntimeEntity(capacityPhase.caster, out IRuntimeEntity runtimeEntity))
            {
                transform.position = runtimeEntity.transform.position;
                transform.rotation = runtimeEntity.transform.rotation;
            }
        }

        public async Awaitable Play(CancellationToken token)
        {
            playableDirector.playOnAwake = false;
            playableDirector.extrapolationMode = DirectorWrapMode.None;

            ResolveBindings();

            finished = new AwaitableCompletionSource();
            playableDirector.stopped += OnDirectorStopped;
            playableDirector.Play();

            await finished.Awaitable;
            playableDirector.stopped -= OnDirectorStopped;
            finished = null;
        }

        public async Awaitable Stop(CancellationToken token)
        {
            playableDirector.Stop();
            await Awaitable.MainThreadAsync();
        }

        public void Dispose()
        {
            gameObject.Destroy();
        }

        private void OnDirectorStopped(PlayableDirector _) => finished?.TrySetResult();

        public void OnNotify(Playable origin, INotification notification, object context)
        {
            if (notification is StepMarker step)
                StepReached?.Invoke(step.StepName);
        }

        // ---- IQteWindowHost (QTE clip drives these) -------------------------

        public void OnQteWindowEnter(QteClipData data)
        {
            pressedThisWindow = false;
            pendingResult = 0f;

            // TODO: spawn the gauge (data.gaugePrefab) for visual feedback on this
            // screen. Both screens show the gauge; only the owner reads input.
        }

        public void OnQteWindowTick(QteClipData data, double normalizedTime)
        {
            // TODO: update gauge fill from normalizedTime.

            if (!IsOwner || pressedThisWindow)
                return;

            // TODO: replace with the real input gateway (player's input controller).
            bool pressed = ReadQtePressPlaceholder();
            if (!pressed)
                return;

            pressedThisWindow = true;

            // Critical window = the last `criticalPortion` of the clip. A press is a
            // success (1) iff it lands inside it, else a miss (0).
            float criticalPortion = GetCriticalPortion();
            float threshold = 1f - criticalPortion;
            pendingResult = normalizedTime >= threshold ? 1f : 0f;

            // TODO: if pendingResult == 0, play the early-press miss animation.
        }

        public void OnQteWindowExit(QteClipData data)
        {
            // Only the owner emits; both screens stack via the QteCommand listener.
            if (!IsOwner)
                return;

            // No press at all = miss (0). A press already computed pendingResult.
            float result = pressedThisWindow ? pendingResult : 0f;
            new QteCommand(CastingPlayer, result).Run(CastingPlayer.BattlePhase);
        }

        // ---- placeholders to wire later -------------------------------------

        private bool ReadQtePressPlaceholder() => false;

        private float GetCriticalPortion()
        {
            // TODO: read from the global game metric (e.g. gameMetrics.QteCriticalPortion).
            return 0.2f;
        }

        /// <summary>
        /// Résout les bindings de la timeline par NOM de piste, à l'instanciation.
        /// Le prefab porte la structure (les pistes) ; les objets liés dépendent de
        /// l'écran/du caster, donc on les bind ici au runtime.
        /// </summary>
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
                        if(!castCapacityPhase.caster.IsValid)
                        {
                            Debug.LogWarning($"[Cutscene] Pas d'Animator pour la piste '{track.name}' car la capacité n'a pas de casters.");
                            break;
                        }

                        if (ScreenPlayer.RuntimeEntityManager.TryGetRuntimeEntity(castCapacityPhase.caster, out IRuntimeEntity runtimeEntity))
                        {
                            if (runtimeEntity is IRuntimeEntityWithAnimator runtimeEntityWithAnimator)
                                playableDirector.SetGenericBinding(track, runtimeEntityWithAnimator.Animator);
                            else
                                Debug.LogWarning($"[Cutscene] Pas d'Animator pour la piste '{track.name}'.");
                        }
                        break;

                    case CutsceneTrackNames.MAIN_CAMERA:
                        Object cameraBinding = ScreenPlayer.Camera.Component.CinemachineBrain;
                        if (cameraBinding != null)
                            playableDirector.SetGenericBinding(track, cameraBinding);
                        else
                            Debug.LogWarning($"[Cutscene] Pas de caméra pour la piste '{track.name}'.");
                        break;

                }
            }
        }
    }
}