using Helteix.Tools.SerializedComponent;
using UnityEngine;

namespace Helteix.Tools.Samples.SerializedComponents
{
    [System.Serializable, AddSerializedComponentMenu("LTX/Samples/LogString")]
    public struct LogString : ISampleSComponent
    {
        [SerializeField, TextArea]
        private string text;

        public void Log()
        {
            Debug.Log(text);
        }
    }
}