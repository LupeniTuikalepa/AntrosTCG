using UnityEngine;

namespace ATCG.Battle.Players.Local.Runtime.UI
{
    public class RuntimeLocalPlayerHUD : RuntimeLocalBattlePlayerComponent
    {
        [field: SerializeField]
        public Canvas Canvas { get; private set; }

        private IRuntimeLocalPlayerHUDElement[] elements;

        private void Start()
        {
            elements = GetComponentsInChildren<IRuntimeLocalPlayerHUDElement>();
            for (int i = 0; i < elements.Length; i++)
                elements[i].HUD = this;
        }

        public override void Connect(IBattlePlayer player)
        {
            Canvas.targetDisplay = Player.Display;
            for (int i = 0; i < elements.Length; i++)
                elements[i].Disconnect(Player);
        }

        public override void Disconnect(IBattlePlayer battlePlayer)
        {
            for (int i = 0; i < elements.Length; i++)
                elements[i].Disconnect(Player);
        }
    }
}