using System.Collections.Generic;
using ATCG.HexGrids.Runtime;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ATCG.Battle.GameCamera
{
    public class GameCameraController : MonoBehaviour
    {
        [BoxGroup("Setup")]
        [SerializeField]
        private InputActionAsset controls;
        [BoxGroup("Setup")]
        [SerializeField]
        private RuntimeHexGrid grid;

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


        private InputAction moveAction;

        private Vector3 lastSpeed;
        private List<MeshRenderer> levelRenderersInBounds;

        private void Awake()
        {
            moveAction = controls.FindAction("Move");
            levelRenderersInBounds = new List<MeshRenderer>();
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