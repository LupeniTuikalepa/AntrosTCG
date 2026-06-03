using UnityEngine;

namespace Helteix.Tools
{
    public static class RenderingLayerMaskExtensions
    {
        public static void EnableRenderingLayer(this MeshRenderer r, RenderingLayerMask renderingLayerMask)
            => r.renderingLayerMask |= renderingLayerMask.value;

        public static void EnableRenderingLayer(this MeshRenderer r, int layerIndex)
            => r.renderingLayerMask |= (1u << layerIndex);

        public static void DisableRenderingLayer(this MeshRenderer r, RenderingLayerMask renderingLayerMask)
            => r.renderingLayerMask &= ~renderingLayerMask.value;

        public static void DisableRenderingLayer(this MeshRenderer r, int layerIndex)
            => r.renderingLayerMask &= ~(1u << layerIndex);

        public static void ToggleRenderingLayer(this MeshRenderer r, RenderingLayerMask renderingLayerMask)
            => r.renderingLayerMask ^= renderingLayerMask.value;

        public static void ToggleRenderingLayer(this MeshRenderer r, int layerIndex)
            => r.renderingLayerMask ^= (1u << layerIndex);

        public static bool HasRenderingLayer(this MeshRenderer r, RenderingLayerMask renderingLayerMask)
            => (r.renderingLayerMask & renderingLayerMask.value) != 0;

        public static bool HasRenderingLayer(this MeshRenderer r, int layerIndex)
            => (r.renderingLayerMask & (1u << layerIndex)) != 0;

        public static void SetRenderingLayers(this MeshRenderer r, params int[] layerIndices)
        {
            uint mask = 0u;
            foreach (int idx in layerIndices) mask |= (1u << idx);
            r.renderingLayerMask = mask;
        }
    }
}