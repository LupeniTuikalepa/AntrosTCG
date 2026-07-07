// BoneScaleController.cs
// Applies runtime scale to a bone, driven by a UMotion custom property curve.
using UnityEngine;

public class BoneScaleController : MonoBehaviour
{
    [SerializeField] private Transform targetBone;
    [SerializeField] private Vector3 baseScale = Vector3.one;

    // Driven by UMotion's Custom Property Constraint curve
    public float ScaleMultiplier { get; set; } = 1f;

    private void LateUpdate()
    {
        if (targetBone == null) return;
        targetBone.localScale = baseScale * ScaleMultiplier;
    }
}