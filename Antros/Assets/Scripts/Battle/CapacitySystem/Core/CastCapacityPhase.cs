using System.Collections.Generic;
using System.Threading;
using ATCG.Battle.CapacitySystem.Capacities;
using ATCG.Battle.CapacitySystem.Core.Directors;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.GameCommands;
using ATCG.Battle.Entities;
using ATCG.Battle.GameModes;
using ATCG.Capacities;
using ATCG.HexGrids;
using Helteix.ChanneledProperties.Priorities;
using Helteix.Tools.DataMapping;
using Helteix.Tools.Phases;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.CapacitySystem.Core
{
    public class CastCapacityPhase : Phase
    {
        public readonly BattlePhase battlePhase;
        public readonly CapacityData data;

        public readonly HexCoordinates castPoint;
        public readonly EntityAddress caster;

        public List<ICapacityDirector> directors;

        private List<float> QTEs;

        public CastCapacityPhase(BattlePhase battlePhase, CapacityData data, HexCoordinates castPoint, EntityAddress caster)
        {
            this.battlePhase = battlePhase;
            this.data = data;
            this.castPoint = castPoint;
            this.caster = caster;
        }

        protected override Awaitable Initialize(CancellationToken token)
        {
            QTEs = ListPool<float>.Get();
            directors = ListPool<ICapacityDirector>.Get();
            return base.Initialize(token);
        }

        protected override async Awaitable ExecuteNoResult(CancellationToken token)
        {
            using (CommandManager.BeginGroup($"[Cast Capacity] {data.Name}"))
            {
                if (data.TryGet(out ICapacityContainer capacityContainer))
                {
                    foreach (ICapacityStep stepHolder in capacityContainer.Run(data, this))
                    {
                        if (data.TryGetStep(stepHolder.StepName, out CapacityStepData stepData))
                        {
                            float effectiveness = FlushQtes();

                            CapacityStepContext stepContext = new CapacityStepContext(this, effectiveness, stepData);
                            stepHolder.RunStep(stepContext);
                        }
                        else
                        {
                            Debug.LogError($"Could not find data for step: {stepHolder.StepName}. Skipping it");
                        }
                    }
                }
            }
        }

        protected override Awaitable Dispose(CancellationToken token)
        {
            ListPool<ICapacityDirector>.Release(directors);
            ListPool<float>.Release(QTEs);

            return base.Dispose(token);
        }


        public void AddQteResult(float qte) => QTEs.Add(qte);

        private float FlushQtes()
        {
            if (QTEs.Count == 0)
                return 1;

            float result = 0;
            foreach (var qte in QTEs)
                result += qte;
            result /= QTEs.Count;

            QTEs.Clear();
            return result;
        }
    }
}