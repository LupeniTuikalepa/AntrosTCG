using System;
using System.Collections.Generic;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Runtime;
using ATCG.Battle.Players.Local;
using ATCG.Battle.Players.Local.Phases;
using ATCG.Battle.Players.Local.Runtime;
using Helteix.ChanneledProperties.Conditions;
using Helteix.ChanneledProperties.Priorities;
using Helteix.Tools.Phases;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle
{
    public class PlayerInteractions : RuntimeLocalPlayerComponent, IEntitySelectionController, IPhaseListener<LocalPlayerTurnPhase>
    {
        int IEntitySelectionController.MaxSelectableEntities => 1;
        
        [SerializeField]
        private RuntimeEntityManager entityManager;


        protected override void Connect(LocalBattlePlayer player)
        {

        }

        protected override void Disconnect(LocalBattlePlayer player)
        {

        }



        void IEntitySelectionController.OnSelected(IRuntimeEntity runtimeEntity)
        {

        }

        void IEntitySelectionController.OnDeselected(IRuntimeEntity runtimeEntity)
        {
        }

        void IPhaseListener<LocalPlayerTurnPhase>.OnPhaseBegin(LocalPlayerTurnPhase phase)
        {
            if(phase.player == Player)
                entityManager.SelectionController.AddPriority(this, PriorityTags.Small, this);
        }

        void IPhaseListener<LocalPlayerTurnPhase>.OnPhaseEnd(LocalPlayerTurnPhase phase)
        {
            if (phase.player == Player)
                entityManager.SelectionController.RemovePriority(this);
        }
    }
}