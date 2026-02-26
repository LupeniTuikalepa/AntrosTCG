using System;
using System.Collections.Generic;
using ATCG.Battle.Heroes.Runtime;
using ATCG.Battle.Players;
using ATCG.Battle.Players.Local;
using ATCG.Battle.Players.Runtime;
using Unity.Cinemachine;
using UnityEngine;

namespace ATCG.Battle.Heroes.Deployed
{
    public class HeroUIController : MonoBehaviour
    {
        [SerializeField]
        private RuntimeHero runtimeHero;

        [SerializeField]
        private CinemachineCamera heroVCam;

        [SerializeField]
        private List<HeroUIPanel> panels;

        [SerializeField]
        private Canvas canvas;

        private Stack<int> openedPanels = new Stack<int>();

        private void Awake()
        {
            heroVCam.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            runtimeHero.OnSelected += OnSelected;
            runtimeHero.OnDeselected += OnDeselected;
            runtimeHero.OnConnected += OnConnected;
            runtimeHero.OnDisconnected += OnDisconnected;
        }

        private void OnDisable()
        {
            runtimeHero.OnSelected -= OnSelected;
            runtimeHero.OnDeselected -= OnDeselected;
            runtimeHero.OnConnected -= OnConnected;
            runtimeHero.OnDisconnected -= OnDisconnected;
        }

        private void OnConnected()
        {
            LocalBattlePlayer battlePlayer = runtimeHero.RuntimeBattleGrid.LocalBattlePlayer;
            if (RuntimeLocalBattlePlayer.TryGetRuntimeLocalPlayerFor(battlePlayer, out RuntimeLocalBattlePlayer rlbp))
            {
                heroVCam.OutputChannel = rlbp.Camera.GetOutputChannel();
                canvas.worldCamera = rlbp.Camera.OutputCamera;
            }
        }

        private void OnDisconnected()
        {
            heroVCam.OutputChannel = OutputChannels.Default;
        }

        private void OnSelected()
        {
            if (runtimeHero.Card.Player == runtimeHero.RuntimeBattleGrid.LocalBattlePlayer)
            {
                heroVCam.gameObject.SetActive(true);
                Open(panels[0]);
            }
        }

        private void OnDeselected()
        {
            if (runtimeHero.Card.Player == runtimeHero.RuntimeBattleGrid.LocalBattlePlayer)
            {
                heroVCam.gameObject.SetActive(false);
                while (openedPanels.TryPeek(out int panelIndex))
                {
                    CloseLast();
                }
            }
        }

        public void Open(HeroUIPanel panel)
        {
            int idx = panels.IndexOf(panel);
            if (idx != -1)
            {
                if(openedPanels.TryPeek(out int openedPanelIndex))
                    panels[openedPanelIndex].Close();

                openedPanels.Push(idx);
                panel.Open();
            }
        }

        public void CloseLast()
        {
            if (openedPanels.TryPop(out int panelIndex))
            {
                panels[panelIndex].Close();
                if (openedPanels.TryPeek(out int openedPanelIndex))
                    panels[openedPanelIndex].Open();
                else if (runtimeHero.IsSelected)
                    runtimeHero.UnSelect();
            }
        }
    }
}