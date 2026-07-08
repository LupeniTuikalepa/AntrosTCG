using System;
using System.Threading;
using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Battle.CapacitySystem.Status;
using ATCG.Battle.CapacitySystem.Status.Berserk;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Components.Implementations;
using ATCG.Battle.Entities.Components.Status;
using ATCG.Battle.GameModes;
using ATCG.Battle.Players.Local.Runtime;
using ATCG.Battle.Players.Runtime;
using ATCG.Battle.Turns;
using ATCG.Metrics;
using Helteix.ChanneledProperties;
using UnityEngine;

namespace ATCG.Battle.Players.Local.Phases
{
    public class LocalPlayerTurnPhase : PlayerTurnPhase
    {
        private class StatusIterator : IStatusComponentIterator
        {
	        private StatusContext statusContext;
	        private bool isStart;

	        public StatusIterator(StatusContext statusContext, bool isStart)
	        {
		        this.statusContext = statusContext;
		        this.isStart = isStart;
	        }

	        public void Process<TStatusComponent>() where TStatusComponent : struct, IStatusComponent
	        {
		        ProcessTurnController<StatusDurationController<TStatusComponent>>();
		        ProcessTurnController<StatusVolatileController<TStatusComponent>>();
	        }

	        private void ProcessTurnController<TTurnController>() where TTurnController : struct, IStatusTurnController, IEntityComponent
	        {
		        foreach (var componentRef in statusContext.battlePhase.world.Query<TTurnController>())
		        {
			        ref TTurnController component = ref componentRef.GetValue();
			        
			        if (isStart)
				        component.OnTurnStarted();
			        else
						component.OnTurnEnded();
		        }
	        }
        }
        private static readonly ChannelKey ChannelKey = ChannelKey.GetUniqueChannelKey("Turn phase");

        public readonly LocalBattlePlayer localPlayerTurn;
        public readonly string turnID;

        //private List<IBattleActionInfos> actionInfosList;
		

        public LocalPlayerTurnPhase(int turnNumber, LocalBattlePlayer localPlayerTurn) : base(turnNumber,
            localPlayerTurn)
        {
            this.localPlayerTurn = localPlayerTurn;
            turnID = Guid.NewGuid().ToString();
        }

        protected override Awaitable Initialize(CancellationToken token)
        {
	        
            //actionInfosList = ListPool<IBattleActionInfos>.Get();
            localPlayerTurn.AddOrRemoveMana(GameMetrics.Current.RecoveredManaOnTurnStart);

            localPlayerTurn.canDeployHeroes.AddCondition(ChannelKey);
            localPlayerTurn.canMoveHeroes.AddCondition(ChannelKey);
            localPlayerTurn.canUseHeroesAbilities.AddCondition(ChannelKey);
            localPlayerTurn.canDoBasicAttack.AddCondition(ChannelKey);

            localPlayerTurn.FillHand();

            var statusContext = new StatusContext(localPlayerTurn.BattlePhase);
            StatusIterator iterator = new StatusIterator( statusContext, true);
            iterator.ForeachStatusComponent();
            
            StatusManager.UpdateControllers(statusContext);
            
            return base.Initialize(token);
        }

        protected override Awaitable Dispose(CancellationToken token)
        {
            //ListPool<IBattleActionInfos>.Release(actionInfosList);

            localPlayerTurn.canDoBasicAttack.RemoveCondition(ChannelKey);
            localPlayerTurn.canDeployHeroes.RemoveCondition(ChannelKey);
            localPlayerTurn.canMoveHeroes.RemoveCondition(ChannelKey);
            localPlayerTurn.canUseHeroesAbilities.RemoveCondition(ChannelKey);

            return base.Dispose(token);
        }

        public void DeployHero()
        {

        }

        public void EndTurn()
        {
            BattleTurn infos = new(turnID, localPlayerTurn.ID);
            StatusIterator iterator = new StatusIterator( new StatusContext(localPlayerTurn.BattlePhase), false);
            iterator.ForeachStatusComponent();
            SetResult(infos);
        }

        public void GiveUp()
        {
            Debug.Log("Giving up is not implemented yet");
        }
    }
}