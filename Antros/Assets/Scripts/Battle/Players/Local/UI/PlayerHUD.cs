using ATCG.Battle.Players.Local;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Battle.Players.Runtime.UI
{
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
            for (int i = 0; i < elements.Length; i++)
                elements[i].Connect(player);
        }

        protected override void Disconnect(LocalBattlePlayer player)
        {
            for (int i = 0; i < elements.Length; i++)
            {
                if(player == elements[i].Player)
                    elements[i].Disconnect();
            }
        }
    }
}