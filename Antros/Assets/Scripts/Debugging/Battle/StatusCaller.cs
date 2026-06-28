using ATCG.Battle;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Queries;
using ATCG.Battle.Entities.Runtime.Heroes;
using ATCG.Battle.GameModes;
using ATCG.Battle.Players.Local.Phases;
using ATCG.Capacities.Data.Status;
using Helteix.Tools.Phases.Listeners;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace ATCG.Debugging.Debugging.Gameplay
{
    #if UNITY_EDITOR
    public class StatusCaller : MonoPhaseListener<BattlePhase>
    {
        private EntityAddress Address(RuntimeHero hero) => hero.Address;

        [SerializeField]
        private RuntimeHero[] targetHeroes;

        [SerializeField]
        private StatusData data;
        private BattlePhase battlePhase;

        protected override void OnPhaseBegin(BattlePhase phase)
        {
            battlePhase = phase;
            base.OnPhaseBegin(phase);
        }

        protected override void OnPhaseEnd(BattlePhase phase)
        {
            battlePhase = null;
            base.OnPhaseEnd(phase);
        }

        [Button, DisableInEditorMode]
        private void ApplyStatus()
        {
            for (int i = 0; i < targetHeroes.Length; i++)
            {
                RuntimeHero runtimeHero = targetHeroes[i];
                EntityAddress adresse = Address(runtimeHero);
                var statusApplyCommand = new StatusApplyCommand(adresse, data);
                statusApplyCommand.Run(battlePhase);
            }
        }

        [Button, DisableInEditorMode]
        private void RemoveStatus()
        {
            for (int i = 0; i < targetHeroes.Length; i++)
            {
                RuntimeHero runtimeHero = targetHeroes[i];
                EntityAddress address = Address(runtimeHero);
                var statusApplyCommand = new StatusRemoveCommand(address, data);
                statusApplyCommand.Run(battlePhase);
            }
        }

        [Button, DisableInEditorMode]
        private void Tick()
        {
            for (int i = 0; i < targetHeroes.Length; i++)
            {
                RuntimeHero runtimeHero = targetHeroes[i];
                EntityAddress adresse = Address(runtimeHero);
                var statusApplyCommand = new StatusTickCommand(adresse, data);
                statusApplyCommand.Run(battlePhase);
            }
        }

        [Button, DisableInEditorMode]
        private void TickAll()
        {
            var statusApplyCommand = new StatusTickAllCommand(data);
            statusApplyCommand.Run(battlePhase);
        }
    }
    #endif
}