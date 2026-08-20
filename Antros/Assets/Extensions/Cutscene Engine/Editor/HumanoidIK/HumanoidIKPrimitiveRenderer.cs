using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace CutsceneEngineEditor
{
    [InitializeOnLoad]
    internal sealed class HumanoidIKPrimitiveRenderer : System.IDisposable
    {
        const int MaxInstancesPerBatch = 1023;

        enum PrimitiveKind
        {
            Box,
            Cylinder,
            Sphere
        }

        readonly struct PrimitiveCommand
        {
            public readonly PrimitiveKind Kind;
            public readonly Matrix4x4 Matrix;
            public readonly Color Color;

            public PrimitiveCommand(PrimitiveKind kind, Matrix4x4 matrix, Color color)
            {
                Kind = kind;
                Matrix = matrix;
                Color = color;
            }
        }

        static readonly int MaterialColorId = Shader.PropertyToID("_Color");
        static readonly int MaterialBaseColorId = Shader.PropertyToID("_BaseColor");
        static readonly int MaterialSurfaceId = Shader.PropertyToID("_Surface");
        static readonly int MaterialBlendId = Shader.PropertyToID("_Blend");
        static readonly int MaterialSmoothnessId = Shader.PropertyToID("_Smoothness");
        static readonly int MaterialGlossinessId = Shader.PropertyToID("_Glossiness");
        static readonly int MaterialMetallicId = Shader.PropertyToID("_Metallic");
        static readonly int MaterialModeId = Shader.PropertyToID("_Mode");
        static readonly int MaterialZTestId = Shader.PropertyToID("_ZTest");

        static Material _material;
        static Material _outlineMaterial;
        static Mesh _cubeMesh;
        static Mesh _cylinderMesh;
        static Mesh _sphereMesh;

        readonly List<PrimitiveCommand> _commands = new List<PrimitiveCommand>(96);
        readonly HashSet<Color> _processedColors = new HashSet<Color>();
        readonly Matrix4x4[] _instanceMatrices = new Matrix4x4[MaxInstancesPerBatch];
        CommandBuffer _commandBuffer;
        bool _acceptCommands;

        internal int PendingCommandCount => _commands.Count;
        internal int PendingBoxCommandCount => CountPendingCommands(PrimitiveKind.Box);
        internal int PendingColorBatchCount
        {
            get
            {
                var count = 0;
                for (var i = 0; i < _commands.Count; i++)
                {
                    var alreadyCounted = false;
                    for (var j = 0; j < i; j++)
                    {
                        if (_commands[j].Color != _commands[i].Color) continue;
                        alreadyCounted = true;
                        break;
                    }

                    if (!alreadyCounted) count++;
                }

                return count;
            }
        }

        internal int PendingDrawBatchCount
        {
            get
            {
                var count = 0;
                for (var i = 0; i < _commands.Count; i++)
                {
                    var alreadyCounted = false;
                    for (var j = 0; j < i; j++)
                    {
                        if (_commands[j].Kind != _commands[i].Kind ||
                            _commands[j].Color != _commands[i].Color)
                        {
                            continue;
                        }

                        alreadyCounted = true;
                        break;
                    }

                    if (!alreadyCounted) count++;
                }

                return count;
            }
        }

        static HumanoidIKPrimitiveRenderer()
        {
            AssemblyReloadEvents.beforeAssemblyReload += Cleanup;
            EditorApplication.quitting += Cleanup;
        }

        int CountPendingCommands(PrimitiveKind kind)
        {
            var count = 0;
            for (var i = 0; i < _commands.Count; i++)
            {
                if (_commands[i].Kind == kind) count++;
            }

            return count;
        }

        internal void BeginFrame(EventType eventType)
        {
            _commands.Clear();
            _acceptCommands = ShouldRender(eventType);
        }

        internal void FlushFrame()
        {
            if (!_acceptCommands || _commands.Count == 0)
            {
                CancelFrame();
                return;
            }

            try
            {
                DrawFillBatches();
                DrawOutlineBatches();
            }
            finally
            {
                CancelFrame();
            }
        }

        internal void CancelFrame()
        {
            _commands.Clear();
            _processedColors.Clear();
            _acceptCommands = false;
        }

        public void Dispose()
        {
            CancelFrame();
            _commandBuffer?.Dispose();
            _commandBuffer = null;
        }

        internal void DrawBox(Vector3 center, Quaternion rotation, Vector3 size, Color color)
        {
            Enqueue(PrimitiveKind.Box, Matrix4x4.TRS(center, rotation, size), color);
        }

        internal void DrawCylinder(Vector3 start, Vector3 end, float radius, Color color)
        {
            var direction = end - start;
            var length = direction.magnitude;
            if (length <= 0.0001f) return;

            var center = Vector3.Lerp(start, end, 0.5f);
            var rotation = Quaternion.FromToRotation(Vector3.up, direction.normalized);
            // Unity's built-in cylinder has radius 1 and height 2.
            var scale = new Vector3(radius, length * 0.5f, radius);
            Enqueue(PrimitiveKind.Cylinder, Matrix4x4.TRS(center, rotation, scale), color);
        }

        internal void DrawSphere(Vector3 center, float radius, Color color)
        {
            Enqueue(
                PrimitiveKind.Sphere,
                // Unity's built-in sphere already has radius 1.
                Matrix4x4.TRS(center, Quaternion.identity, Vector3.one * radius),
                color);
        }

        internal static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }

        internal static bool ShouldRender(EventType eventType)
        {
            return eventType == EventType.Repaint;
        }

        void Enqueue(PrimitiveKind kind, Matrix4x4 matrix, Color color)
        {
            if (!_acceptCommands) return;
            _commands.Add(new PrimitiveCommand(kind, matrix, color));
        }

        void DrawFillBatches()
        {
            var material = GetMaterial();
            if (!material) return;

            DrawInstancedBatches(material, drawOutline: false);
        }

        void DrawOutlineBatches()
        {
            var material = GetOutlineMaterial();
            if (!material) return;

            var previousWireframe = GL.wireframe;
            try
            {
                GL.wireframe = true;
                DrawInstancedBatches(material, drawOutline: true);
            }
            finally
            {
                GL.wireframe = previousWireframe;
            }
        }

        void DrawInstancedBatches(Material material, bool drawOutline)
        {
            var commandBuffer = GetCommandBuffer();
            _processedColors.Clear();
            for (var i = 0; i < _commands.Count; i++)
            {
                var batchColor = drawOutline
                    ? _commands[i].Color
                    : GetFaceColor(_commands[i].Color);
                if (!_processedColors.Add(batchColor)) continue;

                ApplyColor(material, batchColor);
                commandBuffer.Clear();
                for (var kindIndex = (int)PrimitiveKind.Box;
                     kindIndex <= (int)PrimitiveKind.Sphere;
                     kindIndex++)
                {
                    QueueInstances(
                        commandBuffer,
                        material,
                        (PrimitiveKind)kindIndex,
                        batchColor,
                        drawOutline);
                }

                Graphics.ExecuteCommandBuffer(commandBuffer);
            }

            commandBuffer.Clear();
        }

        void QueueInstances(
            CommandBuffer commandBuffer,
            Material material,
            PrimitiveKind kind,
            Color batchColor,
            bool drawOutline)
        {
            var mesh = GetMesh(kind);
            if (!mesh) return;

            var instanceCount = 0;
            for (var i = 0; i < _commands.Count; i++)
            {
                var command = _commands[i];
                var commandColor = drawOutline ? command.Color : GetFaceColor(command.Color);
                if (command.Kind != kind || commandColor != batchColor) continue;

                _instanceMatrices[instanceCount++] = command.Matrix;
                if (instanceCount < MaxInstancesPerBatch) continue;

                commandBuffer.DrawMeshInstanced(
                    mesh,
                    0,
                    material,
                    0,
                    _instanceMatrices,
                    instanceCount);
                instanceCount = 0;
            }

            if (instanceCount == 0) return;
            commandBuffer.DrawMeshInstanced(
                mesh,
                0,
                material,
                0,
                _instanceMatrices,
                instanceCount);
        }

        CommandBuffer GetCommandBuffer()
        {
            if (_commandBuffer != null) return _commandBuffer;

            _commandBuffer = new CommandBuffer
            {
                name = "Humanoid IK Gizmo Primitives"
            };
            return _commandBuffer;
        }

        static Material GetMaterial()
        {
            if (_material) return _material;

            var shader = Shader.Find("Hidden/CutsceneEngine/HumanoidIKGizmoLit")
                         ?? Shader.Find("Universal Render Pipeline/Lit")
                         ?? Shader.Find("Standard")
                         ?? Shader.Find("Hidden/Internal-Colored");
            if (!shader) return null;

            _material = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
            ConfigureMaterial(_material);
            _material.enableInstancing = true;
            return _material;
        }

        static Material GetOutlineMaterial()
        {
            if (_outlineMaterial) return _outlineMaterial;

            var shader = Shader.Find("Hidden/CutsceneEngine/HumanoidIKGizmoLit")
                         ?? Shader.Find("Hidden/Internal-Colored");
            if (!shader) return null;

            _outlineMaterial = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
            _outlineMaterial.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            _outlineMaterial.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            _outlineMaterial.SetInt("_Cull", (int)CullMode.Off);
            _outlineMaterial.SetInt("_ZWrite", 0);
            _outlineMaterial.SetInt("_ZTest", (int)CompareFunction.LessEqual);
            _outlineMaterial.enableInstancing = true;
            return _outlineMaterial;
        }

        static void ConfigureMaterial(Material material)
        {
            material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            material.SetInt("_Cull", (int)CullMode.Back);
            material.SetInt("_ZWrite", 1);
            if (material.HasProperty(MaterialZTestId)) material.SetInt(MaterialZTestId, (int)CompareFunction.LessEqual);

            if (material.HasProperty(MaterialSurfaceId))
            {
                material.SetFloat(MaterialSurfaceId, 1f);
                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            }
            if (material.HasProperty(MaterialBlendId)) material.SetFloat(MaterialBlendId, 0f);
            if (material.HasProperty(MaterialModeId)) material.SetFloat(MaterialModeId, 3f);
            if (material.HasProperty(MaterialSmoothnessId)) material.SetFloat(MaterialSmoothnessId, 0.22f);
            if (material.HasProperty(MaterialGlossinessId)) material.SetFloat(MaterialGlossinessId, 0.22f);
            if (material.HasProperty(MaterialMetallicId)) material.SetFloat(MaterialMetallicId, 0f);

            material.DisableKeyword("_ALPHATEST_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.renderQueue = (int)RenderQueue.Transparent;
        }

        static void ApplyColor(Material material, Color color)
        {
            if (material.HasProperty(MaterialBaseColorId)) material.SetColor(MaterialBaseColorId, color);
            if (material.HasProperty(MaterialColorId)) material.SetColor(MaterialColorId, color);
        }

        static Color GetFaceColor(Color color)
        {
            color.r *= 0.8f;
            color.g *= 0.8f;
            color.b *= 0.8f;
            return color;
        }

        static Mesh GetMesh(PrimitiveKind kind)
        {
            return kind switch
            {
                PrimitiveKind.Box => GetCubeMesh(),
                PrimitiveKind.Cylinder => GetCylinderMesh(),
                PrimitiveKind.Sphere => GetSphereMesh(),
                _ => null
            };
        }

        static Mesh GetCubeMesh()
        {
            if (!_cubeMesh) _cubeMesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
            return _cubeMesh;
        }

        static Mesh GetCylinderMesh()
        {
            if (!_cylinderMesh) _cylinderMesh = Resources.GetBuiltinResource<Mesh>("Cylinder.fbx");
            return _cylinderMesh;
        }

        static Mesh GetSphereMesh()
        {
            if (!_sphereMesh) _sphereMesh = Resources.GetBuiltinResource<Mesh>("Sphere.fbx");
            return _sphereMesh;
        }

        static void Cleanup()
        {
            if (_material) Object.DestroyImmediate(_material);
            if (_outlineMaterial) Object.DestroyImmediate(_outlineMaterial);
            _material = null;
            _outlineMaterial = null;
            _cubeMesh = null;
            _cylinderMesh = null;
            _sphereMesh = null;
        }
    }
}
