using System;
using Helteix.Tools.SerializedComponent.Containers;
using Helteix.Tools.SerializedComponent;
using UnityEngine;
using UnityEngine.Serialization;

namespace Helteix.Tools.Samples.SerializedComponents
{
    public class SampleSComponentsOnMonoBehaviour : MonoBehaviour
    {
        [SerializeField]
        public SComponentContainer<ISampleSComponent> container;


        private void Awake()
        {
            if(container)
                container.Component.Log();
        }
    }
}