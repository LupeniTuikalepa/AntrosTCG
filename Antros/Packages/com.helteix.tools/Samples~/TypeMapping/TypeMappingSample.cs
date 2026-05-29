using System;
using Helteix.Tools.TypeMapping;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Helteix.Tools.Samples.TypeMapping
{
    public class TypeMappingSample : MonoBehaviour
    {
        [SerializeField, TypeRefOf(typeof(ColorProvider))]
        private TypeRef left;
        [SerializeField, TypeRefOf(typeof(ColorProvider))]
        private TypeRef right;

        [SerializeField]
        private Image leftImage;

        [SerializeField]
        private Image rightImage;

        private void Awake()
        {
            OnValidate();
        }

        private void OnValidate()
        {
            if (left.IsValid)
            {
                ColorProvider provider = left.CreateInstance<ColorProvider>();
                leftImage.color = provider.GetColor();
            }

            if (right.IsValid)
            {
                ColorProvider provider = right.CreateInstance<ColorProvider>();
                rightImage.color = provider.GetColor();
            }
        }
    }

    public abstract class ColorProvider
    {
         public abstract Color GetColor();
    }
}