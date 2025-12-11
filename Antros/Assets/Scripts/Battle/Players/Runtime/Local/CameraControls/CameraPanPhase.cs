using System.Threading;
using System.Threading.Tasks;
using Helteix.Tools.Phases;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ATCG.Battle.Players.Runtime.Local.CameraControls
{
    public class CameraPanPhase : IPhase<bool>
    {
        public readonly Transform panTarget;
        public readonly InputAction panAction;
        public readonly Camera cam;
        public readonly Vector3 planePositionReference;


        private Vector3 lastPanPosition;

        public CameraPanPhase(Camera cam, Transform panTarget, InputAction panAction, Vector3 planePositionReference)
        {
            this.cam = cam;
            this.panTarget = panTarget;
            this.panAction = panAction;
            this.planePositionReference = planePositionReference;
        }

        async Awaitable<bool> IPhase<bool>.Execute(CancellationToken token)
        {
            while (panAction.IsPressed())
            {
                if (GetWorldPointerPos(out Vector3 nextPosition))
                {
                    Vector3 delta = nextPosition - lastPanPosition;
                    if (delta.sqrMagnitude >= .05f)
                        panTarget.position += delta;

                    lastPanPosition = nextPosition;
                }

                await Awaitable.NextFrameAsync(token);
            }

            return true;
        }

        async Awaitable IPhase<bool>.Initialize(CancellationToken token)
        {
            if (GetWorldPointerPos(out Vector3 pos))
            {
                lastPanPosition = pos;
            }

            await Task.CompletedTask;
        }

        async Awaitable IPhase<bool>.Dispose(CancellationToken token)
        {
            await Task.CompletedTask;
        }

        private bool GetWorldPointerPos(out Vector3 pos)
        {
            Plane plane = new Plane(-cam.transform.forward, planePositionReference);
            Vector2 pointerPos = panAction.ReadValue<Vector2>();
            Ray ray = cam.ScreenPointToRay(pointerPos);

            if (plane.Raycast(ray, out var distance))
            {
                pos = ray.GetPoint(distance);
                return true;
            }

            pos = default;
            return false;
        }
    }
}