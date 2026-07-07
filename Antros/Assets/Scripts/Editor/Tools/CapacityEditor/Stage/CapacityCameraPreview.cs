using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Playables;

namespace ATCG.Editor.Tools.CapacityEditor
{
    /// <summary>
    /// Renders the cutscene stage's camera into a RenderTexture for display in the
    /// tool overlay. In a PreviewSceneStage the camera isn't rendered by the normal
    /// pipeline, so we drive it manually: the Timeline window's scrub evaluates the
    /// director (which drives the CinemachineTrack, hence the brain), then we render
    /// the brain's camera into the texture. Cinemachine 3.1.7 / URP 17.5.
    /// </summary>
    public sealed class CapacityCameraPreview : System.IDisposable
    {
        private readonly Camera camera;
        private readonly CinemachineBrain brain;
        private readonly PlayableDirector director;
        private RenderTexture target;

        public RenderTexture Texture => target;
        public bool IsValid => camera != null;

        public CapacityCameraPreview(CinemachineBrain brain, PlayableDirector director)
        {
            this.brain = brain;
            this.director = director;
            camera = brain != null ? brain.GetComponent<Camera>() : null;
        }

        // Evaluates the director at the current timeline time so the CinemachineTrack
        // updates the brain, then renders the camera into the preview texture.
        public void Render(int width, int height)
        {
            if (camera == null || width <= 0 || height <= 0)
                return;

            EnsureTexture(width, height);

            // The Timeline window already scrubs the director; re-evaluating here keeps
            // the preview in sync even when repainting outside a scrub event.
            if (director != null && director.playableAsset != null)
                director.Evaluate();

            RenderTexture previousTarget = camera.targetTexture;
            RenderTexture previousActive = RenderTexture.active;

            camera.targetTexture = target;
            camera.Render();
            camera.targetTexture = previousTarget;
            RenderTexture.active = previousActive;
        }

        private void EnsureTexture(int width, int height)
        {
            if (target != null && target.width == width && target.height == height)
                return;

            if (target != null)
                target.Release();

            target = new RenderTexture(width, height, 24, RenderTextureFormat.DefaultHDR)
            {
                name = "CapacityCameraPreview",
                antiAliasing = 1
            };
            target.Create();
        }

        public void Dispose()
        {
            if (target != null)
            {
                target.Release();
                Object.DestroyImmediate(target);
                target = null;
            }
        }
    }
}