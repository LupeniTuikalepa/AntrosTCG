using ATCG.Battle.Commands;
using ATCG.Battle.Commands.Directors;
using ATCG.Battle.Commands.Entities;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.UI;
using TMPro;
using UnityEngine;

namespace ATCG.Battle
{
	public class EntityPreviewUI : BarUI, IEntityCommandDirector<DamageCommand>,IEntityCommandDirector<DeathCommand>
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
			if (address.TryGetComponentRO(out HealthComponent healthComponent))
			{
				CurrentValue = healthComponent.CurrentHealth;
				MaxValue = healthComponent.MaxHealth;
				await RefreshAsync();
			}
		}

		public async Awaitable Disconnect(EntityPreviewListUI entityPreviewListUI, Entity entity)
		{
			//await Tween.ScaleY(transform,0 , 0.2f);
			entityPreviewList = null;
			await Awaitable.MainThreadAsync();
		}

		public async Awaitable Play(CommandDirectorState state, CommandContext context, DamageCommand command)
		{
			await Awaitable.MainThreadAsync();

			state.CompleteAll(this);

			var info = command.GetInfos();
			CurrentValue = info.to;
			MaxValue = info.max;

			await RefreshAsync();
		}

		public async Awaitable Play(CommandDirectorState state, CommandContext context, DeathCommand command)
		{
			state.CompleteAll(this);
			await entityPreviewList.DestroyPreviewAsync(Entity);
		}
	}
}