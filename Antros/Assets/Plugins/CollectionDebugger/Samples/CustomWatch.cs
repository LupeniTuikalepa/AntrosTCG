using System;
using CollectionDebugger.Core;
using UnityEngine;
using UnityEngine.Serialization;

public class CustomWatch : MonoBehaviour
{
    [SerializeField] 
    private Transform[] customArray;
    private void OnEnable()
    {
        var collectionWatch = new CustomCollectionWatch(nameof(customArray), customArray);
        collectionWatch.Watch();
    }

    private void OnDisable()
    {
        CollectionDebug.Unwatch(nameof(customArray));
    }
}