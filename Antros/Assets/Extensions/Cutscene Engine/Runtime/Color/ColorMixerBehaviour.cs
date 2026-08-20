using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
#if URP
using UnityEngine.Rendering.Universal;
#endif
#if HDRP
using UnityEngine.Rendering.HighDefinition;
#endif
using UnityEngine.UI;
using UnityEngine.UIElements;

#if VFX
using UnityEngine.VFX;
#endif

using Object = UnityEngine.Object;

namespace CutsceneEngine
{
    public class ColorMixerBehaviour : PlayableBehaviour
    {
        public ColorTargetType targetType;
        public UIElementColorTarget uiElementColorTarget;
        public Color initialColor;
        public Color[] initialColors;
        public Material[] originalMaterials;
        public Material originalMaterial;
        public bool isTint;
        public bool applyToMaterialProperty;

        public int materialIndex;
        public int propertyID;
        public string propertyName;
        
        public Renderer renderer;
        public SpriteRenderer spriteRenderer;
#if URP || HDRP
        public  DecalProjector decal;
        public bool applyAlphaToDecalOpacity;
#else
        public Projector decal;
#endif

#if VFX
        public VisualEffect vfx;
#endif
        
        public Graphic graphic;
        public VisualElement uiElement;
        public UIDocument uiDocument;
        public string elementName;

        bool _rendererInitialized;
        bool _decalInitialized;
        bool _uiGraphicInitialized;
        bool _uiElementInitialized;
        bool _spriteRendererInitialized;
        bool _useMaterialProperty;
        GameObject _binding;
#if URP || HDRP
        float _initialDecalOpacity;
#endif

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            var binding = playerData as GameObject;
            if (!binding && playerData is Component component) binding = component.gameObject;
            SetBinding(binding);
            if (targetType == ColorTargetType.InValid) return;
            var inputCount = playable.GetInputCount();
            var color = Color.clear;
            var totalWeight = 0f;

            for (int i = 0; i < inputCount; i++)
            {
                var inputWeight = playable.GetInputWeight(i);
                if(inputWeight <= 0) continue;

                var playableInput = (ScriptPlayable<ColorBehaviour>)playable.GetInput(i);
                var behaviour = playableInput.GetBehaviour();

                var t = playableInput.GetTime() / (behaviour.end - behaviour.start);
                color += behaviour.Evaluate((float)t) * inputWeight;
                totalWeight += inputWeight;
            }
            
            var c = color;
            color = isTint ? 
                initialColor * color :
                initialColor * (1 - totalWeight) + color;
            
            switch (targetType)
            {
                case ColorTargetType.Renderer:
                    InitializeRenderer();
                    var materials = Application.isPlaying ? renderer.materials : renderer.sharedMaterials; 
                    if(materialIndex < 0)
                    {
                        for (int i = 0; i < materials.Length; i++)
                        {
                            color = isTint ? 
                                initialColors[i] * c:
                                initialColors[i] * (1 - totalWeight) + c;

                            if (propertyID == 0) materials[i].color = color;
                            else materials[i].SetColor(propertyID, color);
                        }
                    }
                    else
                    {
                        if (propertyID == 0) materials[materialIndex].color = color;
                        else materials[materialIndex].SetColor(propertyID, color);
                    }
                    break;
                case ColorTargetType.Decal:
                    InitializeDecal();
#if URP || HDRP
                    if (applyAlphaToDecalOpacity) decal.fadeFactor = color.a;
#endif
                    if (propertyID == 0) decal.material.color = color;
                    else decal.material.SetColor(propertyID, color);
                    
                    break;
                case ColorTargetType.UIGraphc:
                    InitializeUIGraphic();
                    
                    if(_useMaterialProperty)
                    {
                        if (propertyID == 0) graphic.material.color = color;
                        else graphic.material.SetColor(propertyID, color);
                    }
                    else
                    {
                        graphic.color = color;
                    }
                    break;
                case ColorTargetType.UIElement:
                    if (uiElement == null)
                    {
                        if (!uiDocument || uiDocument.rootVisualElement == null) break;

                        var paths = elementName.Split('/');
                        VisualElement ve = null;
                        if (paths.Length > 1)
                        {
                            for (int i = 0; i < paths.Length; i++)
                            {
                                ve = uiDocument.rootVisualElement.Q(paths[i]);
                            }
                        }
                        else
                        {
                            ve = uiDocument.rootVisualElement.Q(elementName);
                        }

                        if (ve != null)
                        {
                            uiElement = ve;
                            InitializeUIElement();
                        }
                        else
                        {
                            break;
                        }
                    }
                    
                    
                    if (uiElement != null)
                    {
                        switch (uiElementColorTarget)
                        {
                            case UIElementColorTarget.TextColor:
                                uiElement.style.color = color;
                                break;
                            case UIElementColorTarget.BackgroundColor:
                                uiElement.style.backgroundColor = color;
                                break;
                            case UIElementColorTarget.ImageTint:
                                uiElement.style.unityBackgroundImageTintColor = color;
                                break;
                            case UIElementColorTarget.TextOutlineColor:
                                uiElement.style.unityTextOutlineColor = color;
                                break;
                            case UIElementColorTarget.BorderColor:
                                uiElement.style.borderTopColor = color;
                                uiElement.style.borderBottomColor = color;
                                uiElement.style.borderLeftColor = color;
                                uiElement.style.borderRightColor = color;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                        
                    }
                    break;
                case ColorTargetType.VFX:
#if VFX
                    if (vfx)
                    {
                        vfx.SetVector4(propertyID, color);
                    }              
#endif
                    break;
                case ColorTargetType.SpriteRenderer:
                    InitializeSpriteRenderer();
                    if (spriteRenderer)
                    {
                        if (_useMaterialProperty)
                        {
                            if (propertyID == 0) spriteRenderer.material.color = color;
                            else spriteRenderer.material.SetColor(propertyID, color);
                        }
                        else
                        {
                            spriteRenderer.color = color;
                        }
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            var inputCount = playable.GetInputCount();
            var weightSum = 0f;
            for (int i = 0; i < inputCount; i++)
            {
                weightSum += playable.GetInputWeight(i);
            }
            if (weightSum <= 0)
            {
                UnInitialize();
            }
        }

        public override void OnPlayableDestroy(Playable playable)
        {
            UnInitialize();
        }

        internal void SetBinding(GameObject binding)
        {
            if (ReferenceEquals(_binding, binding)) return;

            UnInitialize();
            ClearBindingState();
            _binding = binding;
            if (!binding) return;

            _useMaterialProperty = applyToMaterialProperty;
            if (binding.TryGetComponent(out Graphic g))
            {
                graphic = g;
                targetType = ColorTargetType.UIGraphc;

                if (!_useMaterialProperty)
                {
                    initialColor = g.color;
                }
                else
                {
                    var mat = g.material;
                    if (!mat || mat == g.defaultMaterial)
                    {
                        _useMaterialProperty = false;
                        initialColor = g.color;
                    }
                    else
                    {
                        originalMaterial = mat;
                        SetInitialMaterialColor(mat);
                    }
                }
            }
#if VFX
            else if (binding.TryGetComponent(out VisualEffect visualEffect))
            {
                targetType = ColorTargetType.VFX;
                vfx = visualEffect;
                propertyID = Shader.PropertyToID(propertyName);
                initialColor = vfx.GetVector4(propertyID);
            }
#endif
            else if (binding.TryGetComponent(out UIDocument document))
            {
                targetType = ColorTargetType.UIElement;
                uiDocument = document;
            }
#if URP || HDRP
            else if (binding.TryGetComponent(out DecalProjector decalProjector))
#else
            else if (binding.TryGetComponent(out Projector decalProjector))
#endif
            {
                targetType = ColorTargetType.Decal;
                decal = decalProjector;
                var mat = decal.material;
                if (!mat)
                {
                    targetType = ColorTargetType.InValid;
                    return;
                }

                originalMaterial = mat;
                SetInitialMaterialColor(mat);
#if URP || HDRP
                _initialDecalOpacity = decal.fadeFactor;
#endif
            }
            else if (binding.TryGetComponent(out SpriteRenderer sprite))
            {
                targetType = ColorTargetType.SpriteRenderer;
                spriteRenderer = sprite;
                initialColor = sprite.color;

                if (_useMaterialProperty)
                {
                    var mat = sprite.sharedMaterial;
                    if (mat)
                    {
                        originalMaterial = mat;
                        SetInitialMaterialColor(mat);
                    }
                    else
                    {
                        _useMaterialProperty = false;
                    }
                }
            }
            else if (binding.TryGetComponent(out Renderer targetRenderer))
            {
                targetType = ColorTargetType.Renderer;
                var materials = targetRenderer.sharedMaterials;
                if (materials == null || materials.Length == 0 || materials.Any(x => x == null))
                {
                    targetType = ColorTargetType.InValid;
                    return;
                }

                originalMaterials = materials;
                renderer = targetRenderer;
                if (materialIndex > -1)
                {
                    if (materialIndex >= materials.Length)
                    {
                        targetType = ColorTargetType.InValid;
                        return;
                    }

                    SetInitialMaterialColor(materials[materialIndex]);
                }
                else if (string.IsNullOrWhiteSpace(propertyName))
                {
                    initialColors = materials.Select(x => x.color).ToArray();
                }
                else
                {
                    propertyID = Shader.PropertyToID(propertyName);
                    initialColors = materials.Select(x =>
                    {
                        if (x.HasColor(propertyID)) return x.GetColor(propertyID);
                        return x.color;
                    }).ToArray();
                }
            }
        }

        void SetInitialMaterialColor(Material material)
        {
            if (string.IsNullOrWhiteSpace(propertyName) || !material.HasColor(propertyName))
            {
                initialColor = material.color;
                return;
            }

            propertyID = Shader.PropertyToID(propertyName);
            initialColor = material.GetColor(propertyID);
        }

        void ClearBindingState()
        {
            targetType = ColorTargetType.InValid;
            initialColor = default;
            initialColors = null;
            originalMaterials = null;
            originalMaterial = null;
            propertyID = 0;
            renderer = null;
            spriteRenderer = null;
            decal = null;
#if VFX
            vfx = null;
#endif
            graphic = null;
            uiElement = null;
            uiDocument = null;
            _useMaterialProperty = false;
            _uiElementInitialized = false;
        }

        void InitializeRenderer()
        {
            if(_rendererInitialized) return;
            if (!Application.isPlaying)
            {
                var materials = new Material[originalMaterials.Length];
                for (int i = 0; i < originalMaterials.Length; i++)
                {
                    var copy = Object.Instantiate(originalMaterials[i]);
                    materials[i] = copy;
                }

                renderer.sharedMaterials = materials;
            }

            _rendererInitialized = true;
        }

        void InitializeDecal()
        {
            if (_decalInitialized) return;
            // 데칼은 sharedMaterial이 없어서 머티리얼을 직접 복제함.
            var copy = Object.Instantiate(originalMaterial);
            decal.material = copy;

            _decalInitialized = true;
        }


        void InitializeUIGraphic()
        {
            if(_uiGraphicInitialized) return;
            if(_useMaterialProperty)
            {
                var copy = Object.Instantiate(originalMaterial);
                graphic.material = copy;
            }

            _uiGraphicInitialized = true;
        }


        void InitializeUIElement()
        {
            if(_uiElementInitialized || uiElement == null) return;
            switch (uiElementColorTarget)
            {
                case UIElementColorTarget.TextColor:
                    initialColor = uiElement.resolvedStyle.color;
                    break;
                case UIElementColorTarget.BackgroundColor:
                    initialColor = uiElement.resolvedStyle.backgroundColor;
                    break;
                case UIElementColorTarget.ImageTint:
                    initialColor = uiElement.resolvedStyle.unityBackgroundImageTintColor;
                    break;
                case UIElementColorTarget.TextOutlineColor:
                    initialColor = uiElement.resolvedStyle.unityTextOutlineColor;
                    break;
                case UIElementColorTarget.BorderColor:
                    initialColor = uiElement.resolvedStyle.borderTopColor;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _uiElementInitialized = true;
        }
        
        void InitializeSpriteRenderer()
        {
            if(_spriteRendererInitialized) return;
            if (!Application.isPlaying && _useMaterialProperty)
            {
                var copy = Object.Instantiate(originalMaterial);
                spriteRenderer.material = copy;
            }

            _spriteRendererInitialized = true;
        }
        
        void UnInitialize()
        {
            switch (targetType)
            {
                case ColorTargetType.Renderer:
                {
                    if (_rendererInitialized && renderer)
                    {
                        if (Application.isPlaying) renderer.materials = originalMaterials;
                        else renderer.sharedMaterials = originalMaterials;
                    }
                    break;
                }
                case ColorTargetType.Decal:
                {
                    if (_decalInitialized && decal)
                    {
                        decal.material = originalMaterial;
#if URP || HDRP
                        if (applyAlphaToDecalOpacity) decal.fadeFactor = _initialDecalOpacity;
#endif
                    }
                    break;
                }
                case ColorTargetType.UIGraphc:
                    if (_uiGraphicInitialized && graphic)
                    {
                        if (_useMaterialProperty) graphic.material = originalMaterial;
                        else graphic.color = initialColor;
                    }
                    break;
                case ColorTargetType.UIElement:
                    if (_uiElementInitialized && uiElement != null)
                    {
                        switch (uiElementColorTarget)
                        {
                            case UIElementColorTarget.TextColor:
                                uiElement.style.color = initialColor;
                                break;
                            case UIElementColorTarget.BackgroundColor:
                                uiElement.style.backgroundColor = initialColor;
                                break;
                            case UIElementColorTarget.ImageTint:
                                uiElement.style.unityBackgroundImageTintColor = initialColor;
                                break;
                            case UIElementColorTarget.TextOutlineColor:
                                uiElement.style.unityTextOutlineColor = initialColor;
                                break;
                            case UIElementColorTarget.BorderColor:
                                uiElement.style.borderTopColor = initialColor;
                                uiElement.style.borderBottomColor = initialColor;
                                uiElement.style.borderLeftColor = initialColor;
                                uiElement.style.borderRightColor = initialColor;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        _uiElementInitialized = false;
                    }
                    break;
                case ColorTargetType.VFX:
#if VFX
                    if(vfx) vfx.SetVector4(propertyID, initialColor);
#endif
                    break;
                case ColorTargetType.SpriteRenderer:
                    if (_spriteRendererInitialized && spriteRenderer)
                    {
                        if (_useMaterialProperty) spriteRenderer.material = originalMaterial;
                        else spriteRenderer.color = initialColor;
                    }
                    break;
                case ColorTargetType.InValid:
                    break;
                default:
                    break;
            }
            _rendererInitialized = false;
            _decalInitialized = false;
            _uiGraphicInitialized = false;
            _spriteRendererInitialized = false;
        }
    }
}
