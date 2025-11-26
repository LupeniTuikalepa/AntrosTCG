using System;
using System.Collections.Generic;
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
        [BoxGroup("Movements")]
        [SerializeField]
        private Transform moveTarget;

        [BoxGroup("Cinemachine")]
        [SerializeField]
        private CinemachineBrain renderCamera;

        [BoxGroup("Cinemachine")]
        [SerializeField, ListDrawerSettings(ShowFoldout = false)]
        private CinemachineCamera[] cinemachineCameras;

        [BoxGroup("Cinemachine")]
        [SerializeField, TableList(AlwaysExpanded = true), ListDrawerSettings(ShowFoldout = false)]
        private CinemachineOutputChannels[] channels;

        private InputAction moveAction;

        private Vector3 lastSpeed;
        private List<MeshRenderer> levelRenderersInBounds;
        private RuntimeHexGrid grid;



        protected override void Awake()
        {
            base.Awake();
            levelRenderersInBounds = new List<MeshRenderer>();
            grid = FindFirstObjectByType<RuntimeHexGrid>();
        }

        public override void Connect(IBattlePlayer player)
        {
            OutputChannels outputChannels = OutputChannels.Channel01;
            for (int i = 0; i < channels.Length; i++)
            {
                if (channels[i].PlayerID == player.Profile.id)
                {
                    outputChannels = channels[i].Channels;
                    break;
                }
            }

            for (int i = 0; i < cinemachineCameras.Length; i++)
            {
                CinemachineCamera cinemachineCamera = cinemachineCameras[i];
                cinemachineCamera.OutputChannel = outputChannels;
            }

            renderCamera.ChannelMask = outputChannels;
        }

        public override void Disconnect(IBattlePlayer battlePlayer)
        {
            for (int i = 0; i < cinemachineCameras.Length; i++)
            {
                CinemachineCamera cinemachineCamera = cinemachineCameras[i];
                cinemachineCamera.OutputChannel = OutputChannels.Default;
            }

            renderCamera.ChannelMask = OutputChannels.Default;
        }

        private void Start()
        {
            moveAction = RuntimePlayer.Controls.Move;
        }

        private void OnEnable()
        {
            grid.OnCellAdded += AddCellBounds;
            grid.OnCellRemoved += RemoveCellBounds;
        }

        private void OnDisable()
        {
            grid.OnCellAdded -= AddCellBounds;
            grid.OnCellRemoved -= RemoveCellBounds;
        }

        private void AddCellBounds(RuntimeHexCell cell)
        {
            levelRenderersInBounds.Add(cell.MeshRenderer);
        }
        private void RemoveCellBounds(RuntimeHexCell cell)
        {
            levelRenderersInBounds.Remove(cell.MeshRenderer);
        }

        private void LateUpdate()
        {
            Vector2 input = moveAction.ReadValue<Vector2>();
            bool isAccelerating = input.sqrMagnitude > 0;

            float delta = isAccelerating ? accelerationSpeed : decelerationSpeed;

            Vector2 normInput = input.normalized;

            Camera cam = Camera.main;

            if (cam == null)
                return;

            Vector3 rightSpeed = Vector3.ProjectOnPlane(cam.transform.right, Vector3.up) * normInput.x;
            Vector3 forwardSpeed = Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up) * normInput.y;
            Vector3 targetSpeed = (rightSpeed + forwardSpeed).normalized * (isAccelerating ? maxSpeed : 0);

            Vector3 nextSpeed = Vector3.RotateTowards(lastSpeed, targetSpeed, 15 * Time.deltaTime, delta * Time.deltaTime);
            Vector3 nextPosition = moveTarget.position + nextSpeed * Time.deltaTime;

            Bounds bounds = new Bounds(Vector3.zero, Vector3.one);
            foreach (var r in levelRenderersInBounds)
                bounds.Encapsulate(r.bounds);
            bounds.Expand(boundsExpansion);

            if(!bounds.Contains(nextPosition))
                nextPosition = bounds.ClosestPoint(nextPosition);

            nextPosition.y = transform.position.y;
            lastSpeed = (nextPosition - transform.position) / Time.deltaTime;

            transform.position = nextPosition;
        }
    }
}