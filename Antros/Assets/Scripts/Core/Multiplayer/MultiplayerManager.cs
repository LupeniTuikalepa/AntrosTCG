using System.Threading.Tasks;
using Unity.Services.Authentication;

namespace ATCG.Multiplayer
{
    public class MultiplayerManager
    {
        public static MultiplayerManager Global => GameController.MultiplayerManager;

        public string PlayerName => AuthenticationService.Instance.PlayerName;
        public bool IsSignedIn => AuthenticationService.Instance.IsSignedIn;

        public async Task Connect()
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }


    }
}