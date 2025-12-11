using UnityEngine;

namespace ATCG.Battle.Players.Runtime.Local.UI.HUD
{
    public class RuntimeLocalHUD : RuntimeLocalBattlePlayerComponent
    {
        [field: SerializeField]
        public Canvas Canvas { get; private set; }

        private IRuntimeLocalHUDElement[] elements;

        protected override void Awake()
        {
            base.Awake();
            elements = GetComponentsInChildren<IRuntimeLocalHUDElement>();
            for (int i = 0; i < elements.Length; i++)
                elements[i].HUD = this;
        }


        protected override void Connect(LocalBattlePlayer player)
        {
            for (int i = 0; i < elements.Length; i++)
                elements[i].Connect(RuntimePlayer, player);
        }

        protected override void Disconnect(LocalBattlePlayer player)
        {
            for (int i = 0; i < elements.Length; i++)
                elements[i].Disconnect(RuntimePlayer, player);
        }

    }
}