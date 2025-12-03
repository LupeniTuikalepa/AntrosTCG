using System;
using ATCG.Utilities;
using Helteix.Tools;
using Helteix.Tools.Phases.Listeners;
using UnityEngine;

namespace ATCG.Players.InputAssigning.UI
{
    [AddComponentMenu(menuName:"ATCG/UI/DevicePairing/DevicePairingUI")]
    public class PlayersInputPairingUI : MonoPhaseListener<PlayerInputsPairingPhase>
    {
        [SerializeField]
        private CanvasGroup canvasGroup;

        [SerializeField]
        private PlayerDevicePairingUI prefab;
        [SerializeField]
        private Transform container;

        private void Awake()
        {
            canvasGroup.Hide(0);
            DontDestroyOnLoad(gameObject);
        }

        protected override void OnPhaseBegin(PlayerInputsPairingPhase phase)
        {
            canvasGroup.Show(.2f);
            container.ClearChildren();
            for (int i = 0; i < phase.playerCount; i++)
            {
                PlayerDevicePairingUI instance = prefab.InstantiatePrefab(container);
                instance.Init(i);
            }
            base.OnPhaseBegin(phase);
        }

        protected override void OnPhaseEnd(PlayerInputsPairingPhase phase)
        {
            canvasGroup.Hide(.2f).OnComplete(() => container.ClearChildren());
            base.OnPhaseEnd(phase);
        }
    }
}