using System.Collections.Generic;
using ATCG.Battle.CapacitySystem.Core.Cutscenes;
using ATCG.Battle.CapacitySystem.Core.Cutscenes.Elements;
using ATCG.Battle.CapacitySystem.Core.Properties;
using ATCG.Battle.Entities.Runtime.Status;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.Entities.Runtime.VFX
{
    public class PropagateMaterialOnRenderers : MonoBehaviour, ICapacityCutsceneElement, IRuntimeStatusComponent
    {
        [SerializeField]
        private Material material;

        [Space]
        [SerializeField, EnumToggleButtons]
        private LinkedRendererKey keys;


        private ILinkedRendererSource current;

        void IRuntimeStatusComponent.OnApplyStatus(RuntimeStatusContext context)
        {
            current = context.runtimeEntity;
            Apply();
        }


        void IRuntimeStatusComponent.OnRemoveStatus(RuntimeStatusContext context)
        {
            current = null;
            Clear();
        }

        private IEnumerable<LinkedRenderer> GetAllFor()
        {
            return keys != LinkedRendererKey.None ? current.Models.GetAllFor(keys) : current.Models.GetAll();
        }

        void IRuntimeStatusComponent.OnTickStatus(RuntimeStatusContext context)
        {
        }

        void ICapacityCutsceneElement.Connect(ICapacityContext context)
        {
            if (!context.TryGetProperty(CapacityContextKeys.CASTER, out ICutsceneActor caster) || caster == null)
                return;

            current = caster;
            Apply();
        }

        void ICapacityCutsceneElement.Disconnect()
        {
            Clear();
        }

        private void Reset()
        {
            keys = LinkedRendererKey.Body;
        }
        private void Apply()
        {
            IEnumerable<LinkedRenderer> entityRenderers = GetAllFor();

            using (ListPool<Material>.Get(out var mats))
            {
                foreach (LinkedRenderer linkedRenderer in entityRenderers)
                {
                    mats.AddRange(linkedRenderer.Renderer.materials);
                    mats.Add(material);

                    linkedRenderer.Renderer.SetMaterials(mats);
                    mats.Clear();
                }
            }
        }
        private void Clear()
        {
            using (ListPool<Material>.Get(out var mats))
            {
                foreach (LinkedRenderer linkedRenderer in GetAllFor())
                {
                    mats.AddRange(linkedRenderer.Renderer.materials);
                    mats.Remove(material);

                    linkedRenderer.Renderer.SetMaterials(mats);
                    mats.Clear();
                }
            }
        }
    }
}