using System.Collections.Generic;
using ATCG.Battle.Entities.Heroes.Runtime;
using ATCG.Battle.Players.Local;
using ATCG.Battle.Players.Local.Runtime;
using Unity.Cinemachine;
using UnityEngine;

namespace ATCG.Battle.Entities.Heroes.UI
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

        private readonly Stack<int> openedPanels = new();

        public LocalBattlePlayer LocalBattlePlayer => runtimeHero.Manager.RuntimeBattleGrid.LocalBattlePlayer;

        private void Awake()
        {
            heroVCam.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            runtimeHero.OnHeroSelected += OnHeroSelected;
            runtimeHero.OnHeroDeselected += OnHeroDeselected;
            runtimeHero.OnConnected += OnConnected;
        }

        private void OnDisable()
        {
            runtimeHero.OnHeroSelected -= OnHeroSelected;
            runtimeHero.OnHeroDeselected -= OnHeroDeselected;
            runtimeHero.OnConnected -= OnConnected;
        }


        private void OnConnected()
        {
            if (RuntimeLocalBattlePlayer.TryGetRuntimeLocalPlayerFor(LocalBattlePlayer,
                    out RuntimeLocalBattlePlayer rlbp))
            {
                heroVCam.OutputChannel = rlbp.Camera.GetOutputChannel();
                canvas.worldCamera = rlbp.Camera.OutputCamera;
            }
        }


        private void OnHeroSelected()
        {
            if (runtimeHero.Hero.Player == LocalBattlePlayer)
            {
                heroVCam.gameObject.SetActive(true);
                Open(panels[0]);
            }
        }

        private void OnHeroDeselected()
        {
            if (runtimeHero.Hero.Player == LocalBattlePlayer)
            {
                heroVCam.gameObject.SetActive(false);
                while (openedPanels.TryPeek(out int panelIndex)) CloseLast();
            }
        }

        public void Open(HeroUIPanel panel)
        {
            int idx = panels.IndexOf(panel);
            if (idx != -1)
            {
                if (openedPanels.TryPeek(out int openedPanelIndex))
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