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
        private EntityAddress Address => targetHero.Address;
        
        [SerializeField]
        private RuntimeHero targetHero;
        
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
            var statusApplyCommand = new StatusApplyCommand(Address, data);
            statusApplyCommand.Run(battlePhase);
        }
        
        [Button, DisableInEditorMode]
        private void RemoveStatus()
        {
            var statusRemoveCommand = new StatusRemoveCommand(Address, data);
            statusRemoveCommand.Run(battlePhase);
        }
        
        [Button, DisableInEditorMode]
        private void Tick()
        {
            var statusTickCommand = new StatusTickCommand(Address, data);
            statusTickCommand.Run(battlePhase);
        }
        
        [Button, DisableInEditorMode]
        private void TickAll()
        {
            var statusTickCommand = new StatusTickCommand(Address, data, true);
            statusTickCommand.Run(battlePhase);
        }
    }
    #endif
}
