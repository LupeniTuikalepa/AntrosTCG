using System;
using UnityEngine;
using UnityEngine.Playables;

namespace CutsceneEngine
{
    [Serializable]
    public class HumanoidIKBehaviour : PlayableBehaviour
    {
        public Transform anchorTransform;
        public Vector3 position;
        public Vector3 rotation;
        public HumanoidIKRotationSpace rotationSpace;
        public int footRotationFrameVersion;
        public Vector3 bendTarget;
        public HumanoidIKBendSpace bendSpace;
        public float positionWeight;
        public float rotationWeight;
        public float bendWeight;
        public float digitWeight;
        public HumanoidIKDigitBendPose digitBends;
        public float toeBaseBend;
        public float toeFan;
        public Vector2[] toeBendRanges;
        public Vector2 toeBaseBendRange;
    }
}
