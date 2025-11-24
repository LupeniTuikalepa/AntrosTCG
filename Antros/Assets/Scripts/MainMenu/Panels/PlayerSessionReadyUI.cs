using ATCG.GameModes;
using TMPro;
using Unity.Services.Multiplayer;
using UnityEngine;

namespace ATCG.MainMenu.MainMenu.Panels
{
    public class PlayerSessionReadyUI : MonoBehaviour
    {
        private ISession currentSession;
        private string currentPlayerID;

        [SerializeField]
        private GameObject readyIcon;
        [SerializeField]
        private GameObject waitingIcon;

        [SerializeField]
        private TMP_Text playerName;

        public void Sync(ISession session, string playerID)
        {
            currentSession = session;
            currentPlayerID = playerID;
            session.Changed += UpdateUI;
            UpdateUI();
        }

        public void Dispose()
        {
            if (currentSession != null)
            {
                currentSession.Changed -= UpdateUI;
            }
        }
        private void UpdateUI()
        {
            if (currentSession != null && currentSession.HasPlayer(currentPlayerID))
            {
                var currentPlayer = currentSession.GetPlayer(currentPlayerID);

                if (currentPlayer.Properties.TryGetValue(WaitForEveryPlayerPhase.PLAYER_IS_READY, out PlayerProperty property))
                {
                    bool value = property.Value == bool.TrueString;
                    readyIcon.SetActive(value);
                    waitingIcon.SetActive(!value);
                    return;
                }
            }

            readyIcon.SetActive(false);
            waitingIcon.SetActive(false);
        }
    }
}