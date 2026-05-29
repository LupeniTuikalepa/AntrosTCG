using System;
using UnityEngine;

namespace Helteix.Tools.Samples.SampleSettings
{
    public class SampleHelteixUtilities : MonoBehaviour
    {
        private SampleHelteixSettings sampleHelteixSettings;

        private void Start()
        {
            CreateNewSettings();

            sampleHelteixSettings.SetActive();
        }

        private void CreateNewSettings()
        {
            sampleHelteixSettings = ScriptableObject.CreateInstance<SampleHelteixSettings>();
        }
    }
}