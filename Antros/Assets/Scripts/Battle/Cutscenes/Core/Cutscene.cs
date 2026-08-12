using System;
using System.Threading;
using ATCG.Battle;
using ATCG.Battle.Players.Local.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;

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
        private PlayableDirector playableDirector;

        private ICutsceneElement[] elements;
        private AwaitableCompletionSource finished;

        private QteWindowArbiter arbiter;
        private RuntimeLocalBattlePlayer screenPlayer;

        private void Awake() => elements = GetComponentsInChildren<ICutsceneElement>(true);

        private void Reset() => playableDirector = GetComponent<PlayableDirector>();

        /// <summary>Binds every element to the driving context. Call before <see cref="Play"/>.</summary>
        public void Configure(ICutsceneContext context)
        {
            elements ??= GetComponentsInChildren<ICutsceneElement>(true);
            for (int i = 0; i < elements.Length; i++)
                elements[i].Connect(context);

            // If the context provides a QTE receiver, host QTE windows for this screen: arbitrate
            // local presses and submit scores (the receiver decides whether to emit a networked
            // command). Cutscenes with no QTE simply never build an arbiter.
            screenPlayer = context.GetScreenBattlePlayer();
            IQteResultReceiver receiver = context.GetQteReceiver();
            if (screenPlayer != null && receiver != null)
            {
                arbiter = new QteWindowArbiter(screenPlayer, receiver);
                HookInput();
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
            playableDirector.playOnAwake = false;
            playableDirector.extrapolationMode = DirectorWrapMode.None;

            finished = new AwaitableCompletionSource();
            playableDirector.stopped += OnStopped;
            playableDirector.Play();

            await finished.Awaitable;

            playableDirector.stopped -= OnStopped;
            finished = null;
        }

        /// <summary>Disconnects the elements and destroys the instance.</summary>
        public void Dispose()
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
