using System;
using ATCG.Battle.Entities.Runtime.Grid;
using ATCG.Battle.Grids.Runtime;
using ATCG.Battle.Players.Local.Phases;
using ATCG.Metrics;
using Helteix.ChanneledProperties.Priorities;
using Helteix.Tools.Phases;
using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

namespace ATCG.Battle.Players.Local.Runtime
{
    [AddComponentMenu("ATCG/Gameplay/Player/Runtime/Local Player Camera")]
    public class PlayerCamera : RuntimeLocalPlayerComponent
    {
        [BoxGroup("Setup"), SerializeField, Range(0, 10)]
        private float boundsExpansion;

        [BoxGroup("Movements"), SerializeField, Min(0)]
        private float accelerationSpeed = 5;

        [BoxGroup("Movements"), SerializeField, Min(0)]
        private float decelerationSpeed = 2;

        [BoxGroup("Movements"), SerializeField, Min(0)]
        private float maxSpeed = 15;

        [BoxGroup("Zoom"), SerializeField, MinMaxSlider(0, 100)]
        private Vector2 minMaxZoom;

        [BoxGroup("Zoom"), SerializeField]
        private float maxZoomSpeed = 10;

        [BoxGroup("Zoom"), SerializeField]
        private float zoomAcceleration;

        [BoxGroup("Zoom"), SerializeField]
        private float zoomDeceleration;

        [BoxGroup("Cinemachine"), SerializeField]
        private CinemachineBrain renderCamera;


        [BoxGroup("Cinemachine"), SerializeField]
        private CinemachineCamera cinemachineCamera;
        [BoxGroup("Cinemachine"), SerializeField]
        private CinemachinePositionComposer positionComposer;

        [BoxGroup("Cinemachine"), SerializeField, TableList(AlwaysExpanded = true), ListDrawerSettings(ShowFoldout = false)]
        private CinemachineOutputChannels[] channels;

        [ShowInInspector, HideInEditorMode, ReadOnly]
        private RuntimeBattleGrid grid;

        private float currentZoomSpeed;

        private Vector3 lastSpeed;

        private InputAction PanAction => RuntimeLocalPlayer.Controls.Component.Pan;
        private InputAction MoveAction => RuntimeLocalPlayer.Controls.Component.Move;
        private InputAction ZoomAction => RuntimeLocalPlayer.Controls.Component.Zoom;

        private InputUser PlayerInputUser => RuntimeLocalPlayer.Controls.Component.PlayerInputUser;

        public Camera OutputCamera => renderCamera.OutputCamera;

        protected override void Awake()
        {
            base.Awake();
            var metrics = GameMetrics.Current;
        }


        private void LateUpdate()
        {
            if (cinemachineCamera.IsLive)
            {
                Vector2 input = MoveAction.ReadValue<Vector2>();

                MoveCameraWithPlayerInput(input);

                float zoom = ZoomAction.ReadValue<float>();
                ZoomCamera(zoom);
            }
            else
            {
                currentZoomSpeed = 0;
                lastSpeed = Vector2.zero;

                CinemachineBrain brain = CinemachineCore.FindPotentialTargetBrain(cinemachineCamera);
                Plane plane = new(brain.transform.forward, cinemachineCamera.transform.position);

                cinemachineCamera.Target.TrackingTarget.transform.position =
                    plane.ClosestPointOnPlane(brain.transform.position);
            }
        }

        private void MoveCameraWithPlayerInput(Vector2 input)
        {
            bool isAccelerating = input.sqrMagnitude > 0;

            float delta = isAccelerating ? accelerationSpeed : decelerationSpeed;

            Vector3 normInput = new Vector3(input.x, 0, input.y).normalized;

            Camera cam = Camera.main;

            if (cam == null)
                return;

            Vector3 targetSpeed = normInput.normalized * (isAccelerating ? maxSpeed : 0);

            Vector3 nextSpeed = Vector3.MoveTowards(lastSpeed, targetSpeed, delta * Time.deltaTime);
            Transform moveableTarget = cinemachineCamera.Target.TrackingTarget;

            Vector3 currentPosition = moveableTarget.position;
            Vector3 nextPosition = currentPosition + nextSpeed * Time.deltaTime;

            lastSpeed = (nextPosition - currentPosition) / Time.deltaTime;

            Bounds bounds = new(Vector3.zero, Vector3.one);
            foreach (RuntimeBattleCell r in grid.BattleCells)
                bounds.Encapsulate(r.Model.bounds);

            bounds.Expand(boundsExpansion);
            bounds.max = new Vector3(bounds.max.x, 60, bounds.max.z);
            if (!bounds.Contains(nextPosition))
                nextPosition = bounds.ClosestPoint(nextPosition);

            moveableTarget.position = nextPosition;
        }

        private void ZoomCamera(float zoomInput)
        {
            float targetZoomSpeed = currentZoomSpeed;

            //Debug.Log(currentZoomSpeed);
            if (Mathf.Approximately(0, zoomInput))
                targetZoomSpeed = Mathf.MoveTowards(currentZoomSpeed, 0, zoomDeceleration * Time.deltaTime);
            else
                targetZoomSpeed += zoomInput * zoomAcceleration * Time.deltaTime;

            if (targetZoomSpeed > maxZoomSpeed)
                targetZoomSpeed = maxZoomSpeed;
            if (targetZoomSpeed < -maxZoomSpeed)
                targetZoomSpeed = -maxZoomSpeed;

            float currentZoom = positionComposer.CameraDistance;
            float targetZoom = Mathf.Clamp(currentZoom + targetZoomSpeed, minMaxZoom.x, minMaxZoom.y);
            currentZoomSpeed = targetZoom - currentZoom;

            positionComposer.CameraDistance = targetZoom;
        }
        public OutputChannels GetOutputChannel()
        {
            OutputChannels outputChannels = OutputChannels.Channel01;
            for (int i = 0; i < channels.Length; i++)
                if (channels[i].PlayerID == Player.Profile.ID)
                {
                    outputChannels = channels[i].Channels;
                    break;
                }

            cinemachineCamera.OutputChannel = outputChannels;
            return outputChannels;
        }

        protected override void Connect(LocalBattlePlayer player)
        {
            OutputChannels outputChannels = GetOutputChannel();

            grid = RuntimeLocalPlayer.GetComponentInChildren<RuntimeBattleGrid>();
            renderCamera.ChannelMask = outputChannels | OutputChannels.Default;
            renderCamera.OutputCamera.targetDisplay = RuntimeLocalPlayer.LocalID;
        }


        protected override void Disconnect(LocalBattlePlayer battlePlayer)
        {
            cinemachineCamera.OutputChannel = OutputChannels.Default;
            renderCamera.ChannelMask = OutputChannels.Default;
        }


        [Serializable]
        public struct CinemachineOutputChannels
        {
            [field: SerializeField]
            public int PlayerID { get; private set; }

            [field: SerializeField]
            public OutputChannels Channels { get; private set; }
        }
    }
}