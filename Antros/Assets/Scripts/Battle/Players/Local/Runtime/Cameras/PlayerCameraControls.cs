using System;
using ATCG.Battle.Players.Local.Runtime;
using Unity.Cinemachine;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ATCG.Battle
{
    public class PlayerCameraControls : InputAxisControllerBase<PlayerCameraControls.AxisReader>
    {
        public enum Axis
        {
            Horizontal,
            Vertical,
            Zoom,
        }

        [Serializable]
        public class AxisReader : IInputAxisReader
        {
            [SerializeField]
            private PlayerCamera playerCamera;
            [SerializeField]
            private Axis axis;
            [SerializeField, Min(0)]
            private float scale = 1;
            [SerializeField]
            private bool invert;

            public float GetValue(Object context, IInputAxisOwner.AxisDescriptor.Hints hint)
            {
                return axis switch
                {
                    Axis.Horizontal => playerCamera.RotationAction.ReadValue<Vector2>().x,
                    Axis.Vertical => playerCamera.RotationAction.ReadValue<Vector2>().y,
                    Axis.Zoom => playerCamera.ZoomAction.ReadValue<float>(),
                    _ => throw new ArgumentOutOfRangeException()
                } * scale * (invert ? -1f : 1f);
            }
        }
        void Update()
        {
            if (Application.isPlaying)
                UpdateControllers();
        }

    }
}