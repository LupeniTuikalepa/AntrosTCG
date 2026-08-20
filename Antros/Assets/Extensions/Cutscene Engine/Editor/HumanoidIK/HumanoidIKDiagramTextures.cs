using UnityEditor;
using UnityEngine;

namespace CutsceneEngineEditor
{
    [InitializeOnLoad]
    internal static class HumanoidIKDiagramTextures
    {
        const string DarkHandResourcePath = "HumanoidIK/hand_dark";
        const string LightHandResourcePath = "HumanoidIK/hand_light";
        const string DarkFootResourcePath = "HumanoidIK/foot_dark";
        const string LightFootResourcePath = "HumanoidIK/foot_light";

        static Texture2D _darkHand;
        static Texture2D _lightHand;
        static Texture2D _mirroredDarkHand;
        static Texture2D _mirroredLightHand;
        static Texture2D _darkFoot;
        static Texture2D _lightFoot;
        static Texture2D _mirroredDarkFoot;
        static Texture2D _mirroredLightFoot;

        static HumanoidIKDiagramTextures()
        {
            AssemblyReloadEvents.beforeAssemblyReload += Cleanup;
            EditorApplication.quitting += Cleanup;
        }

        public static Texture2D GetHand(bool mirror)
        {
            if (EditorGUIUtility.isProSkin)
            {
                _darkHand = Load(_darkHand, DarkHandResourcePath);
                return mirror ? GetMirrored(_darkHand, ref _mirroredDarkHand) : _darkHand;
            }

            _lightHand = Load(_lightHand, LightHandResourcePath);
            return mirror ? GetMirrored(_lightHand, ref _mirroredLightHand) : _lightHand;
        }

        public static Texture2D GetFoot(bool mirror)
        {
            if (EditorGUIUtility.isProSkin)
            {
                _darkFoot = Load(_darkFoot, DarkFootResourcePath);
                return mirror ? GetMirrored(_darkFoot, ref _mirroredDarkFoot) : _darkFoot;
            }

            _lightFoot = Load(_lightFoot, LightFootResourcePath);
            return mirror ? GetMirrored(_lightFoot, ref _mirroredLightFoot) : _lightFoot;
        }

        static Texture2D Load(Texture2D current, string path)
        {
            return current ? current : Resources.Load<Texture2D>(path);
        }

        static Texture2D GetMirrored(Texture2D source, ref Texture2D mirrored)
        {
            if (!mirrored) mirrored = CreateMirrored(source);
            return mirrored ? mirrored : source;
        }

        static Texture2D CreateMirrored(Texture2D source)
        {
            if (!source || !source.isReadable) return source;

            var pixels = source.GetPixels32();
            var width = source.width;
            var height = source.height;
            for (var y = 0; y < height; y++)
            {
                var rowStart = y * width;
                for (var x = 0; x < width / 2; x++)
                {
                    var leftIndex = rowStart + x;
                    var rightIndex = rowStart + width - 1 - x;
                    (pixels[leftIndex], pixels[rightIndex]) = (pixels[rightIndex], pixels[leftIndex]);
                }
            }

            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                name = $"{source.name} (Mirrored)",
                filterMode = source.filterMode,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            return texture;
        }

        static void Cleanup()
        {
            DestroyGenerated(ref _mirroredDarkHand);
            DestroyGenerated(ref _mirroredLightHand);
            DestroyGenerated(ref _mirroredDarkFoot);
            DestroyGenerated(ref _mirroredLightFoot);
            _darkHand = null;
            _lightHand = null;
            _darkFoot = null;
            _lightFoot = null;
        }

        static void DestroyGenerated(ref Texture2D texture)
        {
            if (texture) Object.DestroyImmediate(texture);
            texture = null;
        }
    }
}
