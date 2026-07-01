using UnityEngine;

namespace Helteix.Tools
{
    public static class RenderingLayerMaskExtensions
    {
        public static void EnableRenderingLayer(this Renderer r, RenderingLayerMask renderingLayerMask)
            => r.renderingLayerMask |= renderingLayerMask.value;

        public static void EnableRenderingLayer(this Renderer r, int layerIndex)
            => r.renderingLayerMask |= (1u << layerIndex);

        public static void DisableRenderingLayer(this Renderer r, RenderingLayerMask renderingLayerMask)
            => r.renderingLayerMask &= ~renderingLayerMask.value;

        public static void DisableRenderingLayer(this Renderer r, int layerIndex)
            => r.renderingLayerMask &= ~(1u << layerIndex);

        public static void ToggleRenderingLayer(this Renderer r, RenderingLayerMask renderingLayerMask)
            => r.renderingLayerMask ^= renderingLayerMask.value;

        public static void ToggleRenderingLayer(this Renderer r, int layerIndex)
            => r.renderingLayerMask ^= (1u << layerIndex);

        public static bool HasRenderingLayer(this Renderer r, RenderingLayerMask renderingLayerMask)
            => (r.renderingLayerMask & renderingLayerMask.value) != 0;

        public static bool HasRenderingLayer(this Renderer r, int layerIndex)
            => (r.renderingLayerMask & (1u << layerIndex)) != 0;

        public static void SetRenderingLayers(this Renderer r, params int[] layerIndices)
        {
            uint mask = 0u;
            foreach (int idx in layerIndices) mask |= (1u << idx);
            r.renderingLayerMask = mask;
        }
    }
}