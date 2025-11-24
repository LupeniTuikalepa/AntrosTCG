using System;
using ATCG.Multiplayer;
using PrimeTween;
using UnityEngine;

namespace ATCG.MainMenu.MainMenu
{
    public class MainMenuManager : MonoBehaviour
    {
        [field: SerializeField]
        public CanvasGroup SigningInGroup { get; private set; }

        void Start()
        {
            SigningInGroup.alpha = 0;
            SigningInGroup.blocksRaycasts = false;
            SigningInGroup.interactable = false;
            if (!MultiplayerManager.Global.IsSignedIn)
            {
                _ = SignIn();
            }
        }

        private async Awaitable SignIn()
        {
            try
            {
                SigningInGroup.blocksRaycasts = true;
                SigningInGroup.interactable = true;
                SigningInGroup.alpha = 1;

                await MultiplayerManager.Global.Connect();

                await Tween.Alpha(SigningInGroup, 0, .2f);

                SigningInGroup.blocksRaycasts = false;
                SigningInGroup.interactable = false;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}