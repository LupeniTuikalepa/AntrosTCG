using Helteix.Tools.Settings;
using UnityEngine;

namespace Helteix.Tools.Samples.SampleSettings
{
    [CreateAssetMenu(fileName = "New Sample Settings", menuName = "Helteix/Samples/Settings")]
    [AutoGenerateGameSettings]
    public class SampleHelteixSettings : GameSettings<SampleHelteixSettings>
    {
        [SerializeField]
        public bool sampleToggle;
        [SerializeField, Range(0, 15)]
        public int sampleSlider;
    }
}