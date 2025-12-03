using System;
using System.Collections.Generic;
using ATCG.Battle.HexGrids.Runtime;
using ATCG.HexGrids.Runtime;
using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ATCG.Battle.Players.Local.Runtime
{
    public class RuntimePlayerCamera : RuntimeLocalBattlePlayerComponent
    {
        [Serializable]
        public struct CinemachineOutputChannels
        {
            [field: SerializeField]
            public int PlayerID { get; private set; }

            [field: SerializeField]
            public OutputChannels Channels { get; private set; }
        }

        [BoxGroup("Setup")]
        [SerializeField, Range(0, 10)]
        private float boundsExpansion;

        [BoxGroup("Movements")]
        [SerializeField, Min(0)]
        private float accelerationSpeed = 5;
        [BoxGroup("Movements")]
        [SerializeField, Min(0)]
        private float decelerationSpeed = 2;
        [BoxGroup("Movements")]
        [SerializeField, Min(0)]
        private float maxSpeed = 15;

        [BoxGroup("Cinemachine")]
        [SerializeField]
        private CinemachineBrain renderCamera;

        [BoxGroup("Cinemachine")]
        [SerializeField]
        private BoxCollider2D confiner;

        [BoxGroup("Cinemachine")]
        [SerializeField, ListDrawerSettings(ShowFoldout = false)]
        private CinemachineCamera cinemachineCamera;

        [BoxGroup("Cinemachine")]
        [SerializeField, TableList(AlwaysExpanded = true), ListDrawerSettings(ShowFoldout = false)]
        private CinemachineOutputChannels[] channels;

        private Vector3 lastSpeed;

        [ShowInInspector]
        private RuntimeBattleGrid grid;

        private InputAction panAction;
        private InputAction moveAction;
        private InputAction zoomAction;



        protected override void Awake()
        {
            base.Awake();
            grid = FindFirstObjectByType<RuntimeBattleGrid>();
        }

        private void Start()
        {
            panAction = RuntimePlayer.Controls.Pan;
            moveAction = RuntimePlayer.Controls.Move;
        }

        private void LateUpdate()
        {
            Vector2 input = moveAction.ReadValue<Vector2>();
            MoveCameraWithPlayerInput(input);
        }

        private void MoveCameraWithPlayerInput(Vector2 input)
        {
            bool isAccelerating = input.sqrMagnitude > 0;

            float delta = isAccelerating ? accelerationSpeed : decelerationSpeed;

            Vector2 normInput = input.normalized;

            Camera cam = Camera.main;

            if (cam == null)
                return;

            Vector2 targetSpeed = normInput.normalized * (isAccelerating ? maxSpeed : 0);

            Vector2 nextSpeed = Vector3.MoveTowards(lastSpeed, targetSpeed, delta * Time.deltaTime);
            Transform moveableTarget = cinemachineCamera.transform;

            Vector2 currentPosition = moveableTarget.position;
            Vector2 nextPosition = currentPosition + nextSpeed * Time.deltaTime;

            lastSpeed = (nextPosition - currentPosition) / Time.deltaTime;

            Bounds bounds = new Bounds(Vector3.zero, Vector3.one);
            foreach (var r in grid.BattleCells)
                bounds.Encapsulate(r.SpriteRenderer.bounds);

            bounds.Expand(boundsExpansion);

            confiner.transform.position = bounds.center;
            confiner.size = bounds.size;

            if(!bounds.Contains(nextPosition))
                nextPosition = bounds.ClosestPoint(nextPosition);

            moveableTarget.position = nextPosition;
        }

        public override void Connect(IBattlePlayer player)
        {
            OutputChannels outputChannels = OutputChannels.Channel01;
            for (int i = 0; i < channels.Length; i++)
            {
                if (channels[i].PlayerID == player.Profile.ID)
                {
                    outputChannels = channels[i].Channels;
                    break;
                }
            }

            cinemachineCamera.OutputChannel = outputChannels;
            renderCamera.ChannelMask = outputChannels | OutputChannels.Default;
        }

        public override void Disconnect(IBattlePlayer battlePlayer)
        {
            cinemachineCamera.OutputChannel = OutputChannels.Default;
            renderCamera.ChannelMask = OutputChannels.Default;
        }

        public void SetMovementInput(InputAction.CallbackContext context)
        {

        }
    }
}