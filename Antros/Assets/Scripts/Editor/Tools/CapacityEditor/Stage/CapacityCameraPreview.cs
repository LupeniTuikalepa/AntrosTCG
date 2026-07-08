using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace ATCG.Editor.Tools.CapacityEditor
{
    /// <summary>
    /// Renders the cutscene stage's camera into a RenderTexture for the overlay. The key
    /// detail for a PreviewSceneStage: the camera must have its scene culling bound to
    /// the stage scene (camera.scene = stageScene), otherwise it renders the global/open
    /// scene in the background and never sees the stage's objects. URP 17 / Render Graph
    /// then renders on-demand via SubmitRenderRequest. Cinemachine 3.1.7 / URP 17.5.
    /// </summary>
    public sealed class CapacityCameraPreview : System.IDisposable
    {
        private readonly Camera camera;
        private readonly CinemachineBrain brain;
        private readonly PlayableDirector director;
        private readonly Scene stageScene;
        private RenderTexture target;

        public RenderTexture Texture => target;
        public bool IsValid => camera != null;
        public Camera Camera => camera;

        public CapacityCameraPreview(CinemachineBrain brain, PlayableDirector director, Scene stageScene)
        {
            this.brain = brain;
            this.director = director;
            this.stageScene = stageScene;
            camera = brain != null ? brain.GetComponent<Camera>() : null;
        }

        public void Render(int width, int height)
        {
            if (camera == null || width <= 0 || height <= 0)
                return;

            EnsureTexture(width, height);

            // Bind culling to the stage scene so the camera sees the stage's objects and
            // not the open/global scene. This is what makes a preview-scene camera render
            // the isolated content.
            if (stageScene.IsValid())
                camera.scene = stageScene;

            if (director != null && director.playableAsset != null)
                director.Evaluate();

            if (RenderPipelineManager.currentPipeline != null)
            {
                UniversalRenderPipeline.SingleCameraRequest request = new() { destination = target };
                if (RenderPipeline.SupportsRenderRequest(camera, request))
                {
                    RenderPipeline.SubmitRenderRequest(camera, request);
                    return;
                }
            }

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