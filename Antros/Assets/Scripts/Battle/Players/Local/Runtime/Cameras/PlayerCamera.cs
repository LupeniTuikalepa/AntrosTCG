using System;
using ATCG.Battle.Entities.Runtime;
using ATCG.Battle.Entities.Runtime.Grid;
using ATCG.Battle.Grids.Runtime;
using ATCG.Battle.Players.Local.Phases;
using ATCG.Metrics;
using Helteix.ChanneledProperties.Priorities;
using Helteix.Tools.Phases;
using PrimeTween;
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
        public InputAction PanAction => RuntimeLocalBattlePlayer.Controls.Component.Pan;
        public InputAction MoveAction => RuntimeLocalBattlePlayer.Controls.Component.Move;
        public InputAction ZoomAction => RuntimeLocalBattlePlayer.Controls.Component.Zoom;
        public InputAction RotationAction => RuntimeLocalBattlePlayer.Controls.Component.Rotate;
        public Camera OutputCamera => renderCamera.OutputCamera;

        private InputUser PlayerInputUser => RuntimeLocalBattlePlayer.Controls.Component.PlayerInputUser;
        private Transform TrackingTarget => cinemachineCamera.Target.TrackingTarget;


        [BoxGroup("Setup"), SerializeField, Range(0, 10)]
        private float boundsExpansion;

        [BoxGroup("Movements"), SerializeField, Min(0)]
        private float accelerationSpeed = 5;

        [BoxGroup("Movements"), SerializeField, Min(0)]
        private float decelerationSpeed = 2;

        [SerializeField, Min(0)]
        private float maxSpeed = 15;
        [BoxGroup("Movements"), SerializeField]
        private float targetPositionYOffset = 60;

        [BoxGroup("Cinemachine"), SerializeField]
        private CinemachineBrain renderCamera;
        [BoxGroup("Cinemachine"), SerializeField]
        private CinemachineCamera cinemachineCamera;

        [BoxGroup("Cinemachine"), SerializeField, TableList(AlwaysExpanded = true), ListDrawerSettings(ShowFoldout = false)]
        private CinemachineOutputChannels[] channels;


        private Vector3 lastSpeed;



        private void LateUpdate()
        {
            if (cinemachineCamera.IsLive)
            {
                Vector2 input = MoveAction.ReadValue<Vector2>();

                MoveCameraWithPlayerInput(input);
            }
            else
            {
                lastSpeed = Vector2.zero;
            }
        }

        public Plane GetTargetTransformPlane() => new(Vector3.up, targetPositionYOffset);


        public void LookAt(Vector3 position)
        {
            Plane plane = GetTargetTransformPlane();

            Tween.StopAll(TrackingTarget.transform);
            Tween.Position(TrackingTarget.transform, plane.ClosestPointOnPlane(position), .3f);
        }



        private void MoveCameraWithPlayerInput(Vector2 input)
        {
            bool isAccelerating = input.sqrMagnitude > 0;

            float delta = isAccelerating ? accelerationSpeed : decelerationSpeed;


            Camera cam = OutputCamera;
            if (cam == null)
                return;

            Vector3 camForward = Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up).normalized;
            Vector3 camRight = Vector3.ProjectOnPlane(cam.transform.right, Vector3.up).normalized;
            input.Normalize();

            Vector3 worldInput = (camForward * input.y) + (camRight * input.x);
            Vector3 targetSpeed = worldInput.normalized * (isAccelerating ? maxSpeed : 0);

            Vector3 nextSpeed = Vector3.MoveTowards(lastSpeed, targetSpeed, delta * Time.deltaTime);
            Vector3 currentPosition = TrackingTarget.position;
            Vector3 nextPosition = currentPosition + nextSpeed * Time.deltaTime;

            lastSpeed = (nextPosition - currentPosition) / Time.deltaTime;

            Bounds bounds = new(Vector3.zero, Vector3.one);

            foreach (RuntimeBattleCell r in RuntimeLocalBattlePlayer.RuntimeBattleGrid.BattleCells)
                bounds.Encapsulate(r.Model.bounds);

            bounds.Expand(boundsExpansion);
            bounds.max = new Vector3(bounds.max.x, targetPositionYOffset, bounds.max.z);
            if (!bounds.Contains(nextPosition))
                nextPosition = bounds.ClosestPoint(nextPosition);

            nextPosition.y = 0;
            TrackingTarget.position = nextPosition;

            if(lastSpeed.sqrMagnitude >= .3f)
                Tween.StopAll(TrackingTarget.transform);
        }

        public OutputChannels GetOutputChannel()
        {
            OutputChannels outputChannels = OutputChannels.Channel01;
            for (int i = 0; i < channels.Length; i++)
            {
                if (channels[i].PlayerID == Player.GetPlayerNumber())
                {
                    outputChannels = channels[i].Channels;
                    break;
                }
            }

            cinemachineCamera.OutputChannel = outputChannels;
            return outputChannels;
        }

        protected override void Connect(RuntimeLocalBattlePlayer runtimeLocalBattlePlayer)
        {
            OutputChannels outputChannels = GetOutputChannel();

            renderCamera.ChannelMask = outputChannels | OutputChannels.Default;
            renderCamera.OutputCamera.targetDisplay = RuntimeLocalBattlePlayer.LocalID;
        }


        protected override void Disconnect(RuntimeLocalBattlePlayer runtimeLocalBattlePlayer)
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