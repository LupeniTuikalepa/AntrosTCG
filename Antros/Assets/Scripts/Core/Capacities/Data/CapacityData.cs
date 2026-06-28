using System;
using System.Collections.Generic;
using ATCG.Databases;
using ATCG.HexGrids.Patterns.Building;
using Helteix.Tools.DataMapping;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Capacities
{
    public abstract class CapacityData : GameDatabaseObject, IData
    {
        [field: SerializeField, PropertyRange(0, 10), BoxGroup("Base")]
        public int Cost { get; private set; }

        [field: SerializeField, BoxGroup("Base")]
        public string Name { get; private set; }

        [field: SerializeField, TextArea, BoxGroup("Base")]
        public string Description { get; private set; }

        [field: BoxGroup("Base")]
        [field: SerializeField, Tooltip("Patterns of cells that can be selected by the player."), InlineProperty, ListDrawerSettings(ShowFoldout = false)]
        public PatternGroup CastPatterns { get; private set; }

        [field: BoxGroup("Base")]
        [field: SerializeField, Tooltip("Steps of this capacity. Structure is driven by [WithStep] on the concrete data type; only metrics are editable.")]
        private CapacityStepData[] steps;

        private Dictionary<string, CapacityStepData> mappedSteps;

        // OnEnable runs both in editor AND in build when the SO is loaded,
        // unlike OnValidate (editor only) or an empty Awake.
        protected virtual void OnEnable() => RebuildStepMap();

        protected override void OnValidate()
        {
            base.OnValidate();
            RebuildStepMap();
        }

        private void RebuildStepMap()
        {
            mappedSteps ??= new Dictionary<string, CapacityStepData>();
            mappedSteps.Clear();

            if (steps == null)
                return;

            for (int i = 0; i < steps.Length; i++)
            {
                string key = steps[i].StepName;
                if (string.IsNullOrEmpty(key))
                    continue;

                if (!mappedSteps.TryAdd(key, steps[i]))
                    Debug.LogError($"[{name}] Duplicate step '{key}' in '{GetType().Name}'. " +
                                   $"Only the first occurrence is kept.", this);
            }
        }

        public bool TryGetStep(string stepName, out CapacityStepData step)
        {
            mappedSteps ??= new Dictionary<string, CapacityStepData>();
            return mappedSteps.TryGetValue(stepName, out step);
        }

        /// <summary>
        /// Resolves a declared step or throws. Generated accessors
        /// (XxxStepData) route through here, so a step declared via
        /// [WithStep] but missing from the asset fails loudly instead of
        /// silently returning a default CapacityStepData.
        /// </summary>
        protected CapacityStepData GetStepOrThrow(string stepName)
        {
            if (TryGetStep(stepName, out CapacityStepData step))
                return step;

            throw new InvalidOperationException(
                $"[{name}] Step '{stepName}' is declared on '{GetType().Name}' but " +
                $"absent from the asset. Resync the steps array.");
        }
    }
}