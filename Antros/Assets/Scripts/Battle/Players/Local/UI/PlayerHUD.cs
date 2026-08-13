using ATCG.Battle.Players.Local.Runtime;
using Sirenix.OdinInspector;
using StealCapa;
using UnityEngine;

namespace ATCG.Battle.Players.Local.UI
{
    [AddComponentMenu("ATCG/Gameplay/Player/UI/Player HUD")]
    public class PlayerHUD : RuntimeLocalPlayerComponent
    {
        [field: SerializeField]
        public Canvas Canvas { get; private set; }
	
        [field: SerializeField]
        public GetAllCapa CopyCapaPanel { get;private set; }

        [ShowInInspector, HideInEditorMode]
        private PlayerHUDElement[] elements;


        protected void Awake()
        {
            elements = GetComponentsInChildren<PlayerHUDElement>();
            for (int i = 0; i < elements.Length; i++)
                elements[i].Initialize(this);
        }

        protected override void Connect(RuntimeLocalBattlePlayer runtimeLocalBattlePlayer)
        {
            Canvas.targetDisplay = RuntimeLocalBattlePlayer.LocalID;
            for (int i = 0; i < elements.Length; i++)
                elements[i].Connect(runtimeLocalBattlePlayer);
        }

        protected override void Disconnect(RuntimeLocalBattlePlayer runtimeLocalBattlePlayer)
        {
            Canvas.targetDisplay = 0;
            for (int i = 0; i < elements.Length; i++)
                if (runtimeLocalBattlePlayer == elements[i].RuntimePlayer)
                    elements[i].Disconnect();
        }
    }
}