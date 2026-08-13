using System.Collections.Generic;
using System.Threading;
using ATCG;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Players.Local;
using ATCG.Battle.Players.Local.Phases;
using ATCG.Capacities;
using StealCapa;
using StealCapa.UI;
using UnityEngine;

namespace CopyCapa
{
	public class CopyCapaPhase : LocalPlayerPhaseCompletionSource<CapacityData>
	{
		private readonly GetAllCapa panelUI;
		private readonly EntityAddress address;
		private readonly List<CapacityData> capacities;

		public CopyCapaPhase(LocalBattlePlayer localBattlePlayer,EntityAddress address,GetAllCapa panelUI) : base(localBattlePlayer)
		{
			this.address = address;
			this.panelUI = panelUI;
		}
		protected override async Awaitable Initialize(CancellationToken token)
		{
			await base.Initialize(token);
			
			foreach (CapacityData capacityData in GameController.GameDatabase.GetAll<CapacityData>())
			{
				capacities.Add(capacityData);
			}
			
			panelUI.gameObject.SetActive(true);
			panelUI.PopulatePanel(this);
		}
		
		public override void SetResult(in CapacityData capacity)
		{
			if (capacity != null)
			{
				if(!address.TryGetComponentRO(out CapacityCasterComponent caster))
					return;
				
				caster.capacities [2] = capacity;
				address.AddOrSetComponent(caster);
			}
			
			base.SetResult(capacity);
		}

		protected override async Awaitable Dispose(CancellationToken token)
		{
			panelUI.gameObject.SetActive(false);
			await base.Dispose(token);
		}
	}
}