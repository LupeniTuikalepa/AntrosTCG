using System.Threading;
using System.Threading.Tasks;
using Helteix.Tools.Phases;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.InputSystem.Utilities;

namespace ATCG.Players.InputAssigning
{
    public class PlayerInputsPairingPhase : IPhase<InputUser[]>
    {
        public readonly int playerCount;
        public readonly InputActionAsset asset;

        public PlayerInputsPairingPhase(int playerCount, InputActionAsset asset)
        {
            this.playerCount = playerCount;
            this.asset = asset;
        }

        async Awaitable IPhase<InputUser[]>.Initialize(CancellationToken token)
        {
            ReadOnlyArray<InputUser> users = InputUser.all;
            for (int i = 0; i < users.Count; i++)
                users[i].UnpairDevicesAndRemoveUser();

            await Task.CompletedTask;
        }

        async Awaitable<InputUser[]> IPhase<InputUser[]>.Execute(CancellationToken token)
        {
            InputUser[] pairings = new InputUser[playerCount];

            for (int i = 0; i < playerCount; i++)
            {
                InputUser inputUser = InputUser.CreateUserWithoutPairedDevices();
                PlayerInputPairingPhase pairingPhase = new PlayerInputPairingPhase(asset, i);
                InputDevice device = await pairingPhase.Run();

                pairings[i] = InputUser.PerformPairingWithDevice(device, inputUser);
            }

            return pairings;
        }
        async Awaitable IPhase<InputUser[]>.Dispose(CancellationToken token)
        {
            await Task.CompletedTask;
        }
    }
}