using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Battle.Entities.Runtime.VFX
{
    [RequireComponent(typeof(Renderer))]
    public class LinkedRenderer : MonoBehaviour
    {
        // Flags: a renderer can be tagged with several places at once (e.g. Chest | Clothes).
        [field: SerializeField, EnumToggleButtons]
        public LinkedRendererKey Key { get; private set; }

        [field: SerializeField, ReadOnly]
        public Renderer Renderer { get; private set; }

        private void Reset()
        {
            Renderer = GetComponent<Renderer>();
        }

        public void SetKeys(LinkedRendererKey keys) => Key = keys;

        // Reset() only runs when the component is added through the inspector, not when
        // AddComponent<T>() is called from script (e.g. LinkedRendererMapper) — callers
        // that add this component programmatically must set Renderer explicitly.
        public void SetRenderer(Renderer renderer) => Renderer = renderer;
    }
}