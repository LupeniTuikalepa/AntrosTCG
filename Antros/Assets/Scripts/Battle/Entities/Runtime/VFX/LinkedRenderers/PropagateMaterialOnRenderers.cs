using System.Collections.Generic;
using ATCG.Battle.CapacitySystem.Core.Cutscenes;
using ATCG.Battle.CapacitySystem.Core.Cutscenes.Elements;
using ATCG.Battle.CapacitySystem.Core.Properties;
using ATCG.Battle.CapacitySystem.Status.Status;
using ATCG.Battle.CapacitySystem.Status.Status.Components;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;

using ATCG.Cutscenes;
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
            // Clear() must run BEFORE current is nulled out — it calls GetAllFor(), which
            // reads current.Models. Clearing current first threw a NullReferenceException
            // here, which aborted RuntimeStatus.Remove()'s loop over every
            // IRuntimeStatusComponent partway through and propagated all the way up past
            // RuntimeEntity's StatusRemoveCommand.Play — so Destroy(removeStatus.gameObject)
            // was never reached, and the whole RuntimeStatus (VFX included) never got
            // cleaned up, even though the status itself had already left on the ECS side.
            Clear();
            current = null;
        }

        private IEnumerable<LinkedRenderer> GetAllFor()
        {
            return keys != LinkedRendererKey.None ? current.Models.GetAllFor(keys) : current.Models.GetAll();
        }

        void IRuntimeStatusComponent.OnTickStatus(RuntimeStatusContext context)
        {
        }

        void ICutsceneElement.Connect(ICutsceneContext context)
        {
            if (!context.TryGetProperty(CutsceneContextKeys.CASTER, out ICutsceneActor caster) || caster == null)
                return;

            current = caster;
            Apply();
        }

        void ICutsceneElement.Disconnect()
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
                    // Read the SHARED materials, not .materials: the .materials getter
                    // instantiates a unique clone per renderer, so the asset we add here
                    // would come back as a different reference and Clear()'s Remove(material)
                    // would never match it — the material could never be removed.
                    linkedRenderer.Renderer.GetSharedMaterials(mats);
                    if (!mats.Contains(material)) // guard against double-apply stacking copies
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
                    // Shared materials preserve the asset's reference identity, so
                    // Remove(material) matches and actually strips the status material.
                    linkedRenderer.Renderer.GetSharedMaterials(mats);
                    mats.Remove(material);

                    linkedRenderer.Renderer.SetMaterials(mats);
                    mats.Clear();
                }
            }
        }
    }
}