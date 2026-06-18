using System;
using CollectionDebugger.Core;
using UnityEngine;

public class SampleWatch : MonoBehaviour
{
    [SerializeField]
    private Transform[] sampleArray;

    private void OnEnable()
    {
        sampleArray.Watch(nameof(sampleArray));
    }

    private void OnDisable()
    {
        CollectionDebug.Unwatch(nameof(sampleArray));
    }
}