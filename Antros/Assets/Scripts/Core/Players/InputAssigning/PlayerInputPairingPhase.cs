using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Helteix.Tools.Phases;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.InputSystem.Utilities;

namespace ATCG.Players.InputAssigning
{
    public class PlayerInputPairingPhase : PhaseCompletionSource<InputDevice>
    {
        public event Action<InputDevice> OnDeviceSelected;

        public readonly InputActionAsset asset;
        public readonly int playerID;

        private IDisposable listener;

        public PlayerInputPairingPhase(InputActionAsset asset, int playerID)
        {
            this.asset = asset;
            this.playerID = playerID;
        }

        protected override async Awaitable Initialize(CancellationToken token)
        {
            listener = InputSystem.onAnyButtonPress.Call(OnButtonPressed);
            Debug.Log($"Pairing player with id {playerID}...");
            await base.Initialize(token);
        }

        protected override async Awaitable Dispose(CancellationToken token)
        {
            listener.Dispose();
            await base.Dispose(token);

        }

        private void OnButtonPressed(InputControl button)
        {
            InputDevice device = button.device;
            InputControlScheme? controlScheme = InputControlScheme.FindControlSchemeForDevices(new[] { device }, asset.controlSchemes);

            if (controlScheme != null)
            {
                OnDeviceSelected?.Invoke(device);
                SetResult(device);
            }
        }

    }
}