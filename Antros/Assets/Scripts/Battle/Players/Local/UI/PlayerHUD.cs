using System;
using ATCG.Battle.Players.Local;
using ATCG.Battle.Players.Local.Phases;
using Helteix.Tools.Phases;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Battle.Players.Runtime.UI
{

    [AddComponentMenu("ATCG/Gameplay/Player/UI/Player HUD")]
    public class PlayerHUD : RuntimeLocalPlayerComponent
    {
        [field: SerializeField]
        public Canvas Canvas { get; private set; }

        [ShowInInspector, HideInEditorMode]
        private PlayerHUDElement[] elements;


        protected override void Awake()
        {
            elements = GetComponentsInChildren<PlayerHUDElement>();
            for (int i = 0; i < elements.Length; i++)
                elements[i].Initialize(this);
        }

        protected override void Connect(LocalBattlePlayer player)
        {
            Canvas.targetDisplay = RuntimeLocalPlayer.LocalID;
            for (int i = 0; i < elements.Length; i++)
                elements[i].Connect(player);
        }

        protected override void Disconnect(LocalBattlePlayer player)
        {
            Canvas.targetDisplay = 0;
            for (int i = 0; i < elements.Length; i++)
            {
                if(player == elements[i].Player)
                    elements[i].Disconnect();
            }
        }
    }
}