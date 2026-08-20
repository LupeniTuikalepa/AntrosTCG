using System;
#if TMP
using TMPro;
#endif
using UnityEngine;
using UnityEngine.Playables;
#if URP
using UnityEngine.Rendering.Universal;
#endif
#if HDRP
using UnityEngine.Rendering.HighDefinition;
#endif
using UnityEngine.Timeline;
using UnityEngine.UI;
using UnityEngine.UIElements;
#if VFX
using UnityEngine.VFX;
#endif


namespace CutsceneEngine
{
    [TrackColor(1f, 0f, 1f)]
    [TrackClipType(typeof(ColorClip))]
    [TrackBindingType(typeof(GameObject))]
    public class ColorTrack : TrackAsset
    {
        [Tooltip("If this value is true, " +
                 "the existing color will be multiplied by the color of the clip, if false, it will be replaced.")]
        public bool isTint;
        [Tooltip("The index of the material in the material array of a given renderer to which to apply the color change. " +
                 "If this value is less than 0, the change will be applied to all materials.")]
        public int materialIndex = -1;
        [Tooltip("The name of the color property of the material. " +
                 "If this field is empty, it will perform changes to material.color.")]
        public string propertyName = "_BaseColor";

        [Tooltip("The name or path of the UI Element to which you want to apply the color change. " +
                 "If there are multiple elements with the same name, enter the path using '/' as a separator.")]
        public string elementName = "UI_ELEMENT_NAME";
        [Tooltip("The property to apply color changes to in a UI Element.")]
        public UIElementColorTarget uiElementColorTarget;
        
        [Tooltip("For UI graphic elements or SpriteRenderer, if you are using a custom material, decide whether to apply color to the UI element or the material. " +
                 "If this value is true, apply color to a property of the material.")]
        public bool applyToMaterialProperty;
#if URP || HDRP
        [Tooltip("If this value is true, the alpha value of the color is also applied to the Opacity of the DecalProjector.")]
        public bool applyAlphaToDecalOpacity = true;
#endif


        protected override void OnCreateClip(TimelineClip clip)
        {
            clip.displayName = " ";
            clip.duration = 1;
        }

        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            var playable = ScriptPlayable<ColorMixerBehaviour>.Create(graph, inputCount);
            var behaviour = playable.GetBehaviour();
            behaviour.isTint = isTint;
#if URP || HDRP
            behaviour.applyAlphaToDecalOpacity = applyAlphaToDecalOpacity;
#endif
            behaviour.applyToMaterialProperty = applyToMaterialProperty;
            behaviour.materialIndex = materialIndex;
            behaviour.propertyName = propertyName;
            behaviour.elementName = elementName;
            behaviour.uiElementColorTarget = uiElementColorTarget;
            return playable;
        }

        protected override Playable CreatePlayable(PlayableGraph graph, GameObject gameObject, TimelineClip clip)
        {
            var c = clip.asset as ColorClip;
            c.start = clip.start;
            c.end = clip.end;
            return base.CreatePlayable(graph, gameObject, clip);
        }

        public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
        {
            var binding = director.GetGenericBinding(this) as GameObject;
            if(binding)
            {
                driver.AddFromName<SpriteRenderer>(binding.gameObject, "m_Color");
                driver.AddFromName<Graphic>(binding.gameObject, "m_Color");
                driver.AddFromName<Graphic>(binding.gameObject, "m_Material");
#if URP || HDRP
                if(applyAlphaToDecalOpacity) driver.AddFromName<DecalProjector>(binding.gameObject, "m_FadeFactor");
#endif
                
#if TMP
                driver.AddFromName<TMP_Text>(binding.gameObject, "m_fontColor");
#endif
            }
        }
    }
}
