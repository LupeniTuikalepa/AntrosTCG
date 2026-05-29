using System;
using UnityEngine;

namespace Helteix.Tools.Samples.SampleSettings
{
    public class SampleHelteixSettingsLogger : MonoBehaviour
    {
        private void Start()
        {
            Debug.Log($"toggle value : {SampleHelteixSettings.Current.sampleToggle}");
            Debug.Log($"slider slider : {SampleHelteixSettings.Current.sampleSlider}");
        }
    }
}