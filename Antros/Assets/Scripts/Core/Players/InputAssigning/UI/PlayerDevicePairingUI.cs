using System;
using ATCG.Metrics;
using ATCG.Utilities;
using Helteix.Tools.Phases.Listeners;
using PrimeTween;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.Switch;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.InputSystem.XInput;
using UnityEngine.UI;

namespace ATCG.Players.InputAssigning.UI
{
    public class PlayerDevicePairingUI : MonoPhaseListener<PlayerInputPairingPhase>
    {
        [ShowInInspector, ReadOnly]
        private int playerID;

        [SerializeField]
        private CanvasGroup waitingGroup;
        [SerializeField]
        private CanvasGroup listeningGroup;
        [SerializeField]
        private CanvasGroup validatedGroup;

        [SerializeField]
        private Image controlImage;
        [SerializeField]
        private TMP_Text playerName;

        private InputDevice device;
        private IDisposable inputListener;

        private void Awake()
        {
            waitingGroup.Show(.2f);
            listeningGroup.Hide(.2f);
            validatedGroup.Hide(.2f);
        }

        public void Init(int id)
        {
            playerID = id;
            playerName.text = $"Player {id + 1}";
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            inputListener = InputSystem.onAnyButtonPress.Call(OnAnyButtonPressed);
        }


        protected override void OnDisable()
        {
            base.OnDisable();
            inputListener?.Dispose();
        }

        protected override void OnPhaseBegin(PlayerInputPairingPhase phase)
        {
            if (phase.playerID == playerID)
            {
                phase.OnDeviceSelected += OnDeviceSelected;

                waitingGroup.Hide(.2f);
                listeningGroup.Show(.2f);
                validatedGroup.Hide(.2f);
            }
            base.OnPhaseBegin(phase);
        }

        protected override void OnPhaseEnd(PlayerInputPairingPhase phase)
        {
            if (phase.playerID == playerID)
            {
                phase.OnDeviceSelected -= OnDeviceSelected;

                waitingGroup.Hide(.2f);
                listeningGroup.Hide(.2f);
                validatedGroup.Show(.2f);
            }
            base.OnPhaseEnd(phase);
        }

        private void OnDeviceSelected(InputDevice obj)
        {
            device = obj;
            controlImage.sprite = obj switch
            {
                SwitchProControllerHID => GameAssets.Current.SwitchGamepad,
                XInputController => GameAssets.Current.XboxGamepad,
                DualShockGamepad => GameAssets.Current.PSGamepad,
                Gamepad => GameAssets.Current.DefaultGamepad,
                Keyboard => GameAssets.Current.Keyboard,
                _ => null,
            };
        }

        private void OnAnyButtonPressed(InputControl control)
        {
            if (control.device == device)
            {
                Tween.CompleteAll(controlImage.transform);
                Tween.PunchScale(controlImage.transform, Vector3.one * .3f, .2f);
            }
        }

    }
}