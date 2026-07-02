using System;
using CollectionDebugger.Core;
using UnityEngine;
using UnityEngine.UI;

public class SampleWatch : MonoBehaviour
{
    private const string LABEL = nameof(sampleArray);
    
    [SerializeField]
    private Transform[] sampleArray;

    [SerializeField]
    private Button button;
    
    private void OnEnable()
    {
        sampleArray.Watch(LABEL);
        button.onClick.AddListener(OnClick);

    }

    private void OnDisable()
    {
        CollectionDebug.Unwatch(LABEL);
        button.onClick.RemoveListener(OnClick);
    }
    
    private void OnClick()
    {
        CollectionDebug.TakeSnapshot(LABEL);
    }
}