using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ATCG.Editor.Tools.CapacityEditor
{
    /// <summary>
    /// Renders the cutscene stage's camera into a RenderTexture for the overlay.
    /// URP 17 / Render Graph: on-demand render via SubmitRenderRequest.
    /// TEMP: [PreviewDiag]/[PixelProbe] logs to locate the black-screen cause.
    /// </summary>
    public sealed class CapacityCameraPreview : System.IDisposable
    {
        private readonly Camera camera;
        private readonly CinemachineBrain brain;
        private readonly PlayableDirector director;
        private RenderTexture target;
        private bool loggedOnce;

        public RenderTexture Texture => target;
        public bool IsValid => camera != null;
        public Camera Camera => camera;

        public CapacityCameraPreview(CinemachineBrain brain, PlayableDirector director)
        {
            this.brain = brain;
            this.director = director;
            camera = brain != null ? brain.GetComponent<Camera>() : null;
        }

        public void Render(int width, int height)
        {
            if (camera == null || width <= 0 || height <= 0)
                return;

            EnsureTexture(width, height);

            if (director != null && director.playableAsset != null)
                director.Evaluate();

            bool hasPipeline = RenderPipelineManager.currentPipeline != null;
            UniversalRenderPipeline.SingleCameraRequest request = new()
            {
                destination = target
            };
            bool supports = hasPipeline && RenderPipeline.SupportsRenderRequest(camera, request);

            if (!loggedOnce)
            {
                Debug.Log($"[PreviewDiag] cam='{camera.name}' enabled={camera.enabled} " +
                          $"active={camera.gameObject.activeInHierarchy} scene='{camera.scene.name}' " +
                          $"sceneValid={camera.scene.IsValid()} mask={camera.cullingMask} clear={camera.clearFlags} " +
                          $"rt={target.width}x{target.height} fmt={target.format} " +
                          $"hasPipeline={hasPipeline} supports={supports}");
            }

            if (supports)
            {
                RenderPipeline.SubmitRenderRequest(camera, request);
                Probe("SubmitRenderRequest");
                return;
            }

            RenderTexture previousTarget = camera.targetTexture;
            RenderTexture previousActive = RenderTexture.active;
            camera.targetTexture = target;
            camera.Render();
            camera.targetTexture = previousTarget;
            RenderTexture.active = previousActive;
            Probe("camera.Render");
        }

        private void Probe(string path)
        {
            if (loggedOnce)
                return;
            loggedOnce = true;

            RenderTexture prev = RenderTexture.active;
            RenderTexture.active = target;
            Texture2D t = new(1, 1, TextureFormat.RGBAFloat, false);
            t.ReadPixels(new Rect(target.width / 2f, target.height / 2f, 1, 1), 0, 0);
            t.Apply();
            Color c = t.GetPixel(0, 0);
            Object.DestroyImmediate(t);
            RenderTexture.active = prev;

            Debug.Log($"[PixelProbe] path={path} center={c}");
        }

        private void EnsureTexture(int width, int height)
        {
            if (target != null && target.width == width && target.height == height)
                return;

            if (target != null)
                target.Release();

            // No alpha channel on purpose: PreviewSceneStage is a brand-new scene with no
            // RenderSettings.skybox assigned, so a Skybox clear falls back to
            // camera.backgroundColor — whose default alpha is 0. With an alpha-carrying
            // format (DefaultHDR), the UI Toolkit Image control alpha-blends that onto its
            // own black backgroundColor style, so the whole preview reads as black even
            // though the RGB content rendered correctly. Dropping the alpha channel makes
            // the texture always composite as opaque, regardless of what the pipeline
            // writes into alpha.
            target = new RenderTexture(width, height, 24, RenderTextureFormat.RGB111110Float)
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