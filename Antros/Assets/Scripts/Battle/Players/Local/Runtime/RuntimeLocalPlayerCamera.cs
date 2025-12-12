using System;
using ATCG.Battle.Grids.Runtime;
using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ATCG.Battle.Players.Local.CameraControls
{
    public class RuntimeLocalPlayerCamera : RuntimeLocalPlayerComponent
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

        [BoxGroup("Zoom")]
        [SerializeField, MinMaxSlider(0, 100)]
        private Vector2 minMaxZoom;
        [BoxGroup("Zoom")]
        [SerializeField]
        private float maxZoomSpeed = 10;
        [BoxGroup("Zoom")]
        [SerializeField]
        private float zoomAcceleration;
        [BoxGroup("Zoom")]
        [SerializeField]
        private float zoomDeceleration;

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
        private float currentZoomSpeed;

        [ShowInInspector, HideInEditorMode, ReadOnly]
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
            panAction = RuntimeLocalPlayer.Controls.Pan;
            moveAction = RuntimeLocalPlayer.Controls.Move;
            zoomAction = RuntimeLocalPlayer.Controls.Zoom;
        }

        private void LateUpdate()
        {
            Vector2 input = moveAction.ReadValue<Vector2>();
            MoveCameraWithPlayerInput(input);

            float zoom = zoomAction.ReadValue<float>();
            ZoomCamera(zoom);
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

        private void ZoomCamera(float zoom)
        {
            //Debug.Log(currentZoomSpeed);
            if (Mathf.Approximately(0, zoom))
            {
                if(currentZoomSpeed > 0)
                    Debug.Log("decelerating");

                currentZoomSpeed = Mathf.MoveTowards(currentZoomSpeed, 0, decelerationSpeed * Time.deltaTime);
            }
            else
                currentZoomSpeed += zoom * zoomAcceleration * Time.deltaTime;

            if(currentZoomSpeed > maxZoomSpeed)
                currentZoomSpeed = maxZoomSpeed;
            if(currentZoomSpeed < -maxZoomSpeed)
                currentZoomSpeed = -maxZoomSpeed;

            float targetZoom = Mathf.Clamp(cinemachineCamera.Lens.OrthographicSize + currentZoomSpeed, minMaxZoom.x, minMaxZoom.y);

            cinemachineCamera.Lens.OrthographicSize = targetZoom;
        }

        protected override void Connect(LocalBattlePlayer player)
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
            renderCamera.OutputCamera.targetDisplay = (int)RuntimeLocalPlayer.Controls.InputUser.index;
        }

        protected override void Disconnect(LocalBattlePlayer battlePlayer)
        {
            cinemachineCamera.OutputChannel = OutputChannels.Default;
            renderCamera.ChannelMask = OutputChannels.Default;
        }

        public void SetMovementInput(InputAction.CallbackContext context)
        {

        }
    }
}