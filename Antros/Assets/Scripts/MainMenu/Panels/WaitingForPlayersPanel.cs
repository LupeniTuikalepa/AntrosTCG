using System.Collections.Generic;
using ATCG.GameModes;
using ATCG.Utilities;
using Helteix.Tools;
using Helteix.Tools.Phases.Listeners;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UI;

namespace ATCG.MainMenu.MainMenu.Panels
{
    public class WaitingForPlayersPanel : MonoPhaseListener<WaitForEveryPlayerPhase>
    {
        [SerializeField]
        private CanvasGroup canvasGroup;
        [SerializeField]
        private float fadeDuration = 0.25f;

        [SerializeField]
        private Transform container;
        [SerializeField]
        private PlayerSessionReadyUI playerReadyUI;
        [SerializeField]
        private Button readyButton;
        [SerializeField]
        private Button waitButton;

        protected WaitForEveryPlayerPhase CurrentPhase { get; private set; }

        private Dictionary<string, PlayerSessionReadyUI> playerSessionReadyUis;

        private void Awake()
        {
            playerSessionReadyUis = new Dictionary<string, PlayerSessionReadyUI>();
            canvasGroup.Hide(fadeDuration);
        }

        protected override void OnPhaseBegin(WaitForEveryPlayerPhase phase)
        {
            CurrentPhase = phase;
            canvasGroup.Show(fadeDuration);
            ISession session = phase.Session;
            container.ClearChildren();

            session.PlayerJoined += AddPlayer;
            session.PlayerLeaving += RemovePlayer;

            foreach (var player in session.Players)
                AddPlayer(player.Id);
        }

        protected override void OnPhaseEnd(WaitForEveryPlayerPhase phase)
        {
            canvasGroup.Hide(fadeDuration);
            ISession session = phase.Session;

            session.PlayerJoined -= AddPlayer;
            session.PlayerLeaving -= RemovePlayer;
        }

        private void AddPlayer(string playerID)
        {
            IReadOnlyPlayer player = CurrentPhase.Session.GetPlayer(playerID);
            PlayerSessionReadyUI instance = Instantiate(playerReadyUI, container);

            instance.Sync(CurrentPhase.Session, player.Id);
            playerSessionReadyUis.Add(playerID, instance);
        }

        private void RemovePlayer(string playerID)
        {
            if (playerSessionReadyUis.Remove(playerID, out PlayerSessionReadyUI instance))
            {
                instance.Dispose();
                Destroy(instance.gameObject);
            }
        }

        public void Leave()
        {
            CurrentPhase?.Leave();
        }

        public void SetReady()
        {
            CurrentPhase?.SetReadyState(true);
            waitButton.gameObject.SetActive(true);
            readyButton.gameObject.SetActive(false);
        }

        public void SetToWait()
        {
            CurrentPhase?.SetReadyState(false);
            waitButton.gameObject.SetActive(false);
            readyButton.gameObject.SetActive(true);
        }
    }
}