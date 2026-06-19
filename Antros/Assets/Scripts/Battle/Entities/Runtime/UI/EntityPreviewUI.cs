using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.Core.Players;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Commands.Players;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.UI;
using TMPro;
using UnityEngine;

namespace ATCG.Battle
{
	public class EntityPreviewUI : BarUI, IEntityCommandListener<DamageCommand>,IEntityCommandListener<DeathCommand>
	{
		private EntityPreviewListUI entityPreviewList;
		public Entity Entity { get; private set; }

		[SerializeField] private TMP_Text nameText;
		private void OnEnable()
		{
			this.RegisterListener();
		}

		private void OnDisable()
		{
			this.UnregisterListener();
		}


		public async Awaitable Connect(EntityPreviewListUI entityPreviewListUI, EntityAddress address)
		{
			Entity = address;
			if (address.TryGetComponentRO(out BattleCardComponent battleCardComponent))
			{
				nameText.text = battleCardComponent.battleCard.Title;
			}
			//await Tween.PunchScale(transform,Vector3.one * 0.2f, 0.3f);
			entityPreviewList = entityPreviewListUI;
			
		}

		public async Awaitable Disconnect(EntityPreviewListUI entityPreviewListUI, Entity entity)
		{
			//await Tween.ScaleY(transform,0 , 0.2f);
			entityPreviewList = null;
			await Awaitable.MainThreadAsync();
		}

		public async Awaitable Play(CommandListenerState state, CommandContext context, DamageCommand command)
		{
			await Awaitable.MainThreadAsync();

			state.CompleteAll(this);

			var info = command.GetInfos();
			CurrentValue = info.to;
			MaxValue = info.max;

			await RefreshAsync();
		}
		
		public async Awaitable Play(CommandListenerState state, CommandContext context, DeathCommand command)
		{
			state.CompleteAll(this);
			await entityPreviewList.DestroyPreviewAsync(Entity);
		}
	}
}