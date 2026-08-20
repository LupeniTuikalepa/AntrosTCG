using System;
using UnityEngine;

namespace CutsceneEngine
{
    [AddComponentMenu("")]
    [DisallowMultipleComponent]
    public sealed class LookAtGenericRigMapping : MonoBehaviour
    {
        [SerializeField, HideInInspector]
        internal bool initialized;

        [SerializeField, HideInInspector]
        internal Transform pelvis;

        [SerializeField, HideInInspector]
        internal Transform head;

        [SerializeField, HideInInspector]
        internal Transform[] bodyBones = Array.Empty<Transform>();

        [SerializeField, HideInInspector]
        internal Transform leftEye;

        [SerializeField, HideInInspector]
        internal Transform rightEye;

        internal int GetMappingHash()
        {
            unchecked
            {
                var hash = initialized ? 17 : 31;
                hash = hash * 397 ^ GetObjectHash(pelvis);
                hash = hash * 397 ^ GetObjectHash(head);
                hash = hash * 397 ^ GetObjectHash(leftEye);
                hash = hash * 397 ^ GetObjectHash(rightEye);
                if (bodyBones == null) return hash;

                for (var i = 0; i < bodyBones.Length; i++)
                {
                    hash = hash * 397 ^ GetObjectHash(bodyBones[i]);
                }

                return hash;
            }
        }

        static int GetObjectHash(UnityEngine.Object value)
        {
            return value ? value.GetHashCode() : 0;
        }
    }
}
