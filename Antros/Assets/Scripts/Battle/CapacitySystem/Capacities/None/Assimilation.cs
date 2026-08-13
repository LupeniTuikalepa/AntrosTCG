using System.Threading.Tasks;
using ATCG.Battle.CapacitySystem.Core;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Grids;
using ATCG.Battle.Players;
using ATCG.Battle.Players.Local;
using ATCG.Battle.Players.Local.Runtime;
using ATCG.Capacities;
using ATCG.Capacities.None;
using ATCG.HexGrids;
using ATCG.HexGrids.Patterns;
using ATCG.HexGrids.Patterns.Building;
using CopyCapa;
using Helteix.Tools;
using Helteix.Tools.Phases;
using StealCapa;
using UnityEngine;

namespace ATCG.Battle.CapacitySystem.Capacities.None
{
	public partial struct Assimilation : ICapacity<AssimilationData>
	{
		public void GetHitPattern(AssimilationData data, ref HexPatternBuilder builder, BattleGrid battleGrid,
			HexCoordinates castPoint, HexCoordinates casterOrigin)
		{
			builder = builder
				.With(new PointsPattern(castPoint));
		}

		public void GetTargets(AssimilationData data, BattleCellAspect battleCell, CapacityTargets output,
			IBattlePlayer castingPlayer)
		{
			
		}

		private partial void ExecuteAssimilation(AssimilationData data, CapacityStepContext ctx)
		{
			ExecuteAssimilationAsync(ctx).ListenForExceptions();
		}

		private async Awaitable ExecuteAssimilationAsync( CapacityStepContext ctx)
		{
			
			if (ctx.capacityPhase.CasterPlayer is not LocalBattlePlayer localBattlePlayer)
				return;

			if (!RuntimeLocalBattlePlayer.TryGetRuntimeLocalPlayerFor(localBattlePlayer, out RuntimeLocalBattlePlayer runtimePlayer))
				return;
			
			GetAllCapa panelUI = runtimePlayer.HUD.Component.CopyCapaPanel;
			if (panelUI == null)
				return;

			CopyCapaPhase copyPhase = new CopyCapaPhase(localBattlePlayer, ctx.Caster, panelUI);
			await copyPhase;
		}
	}

	
}