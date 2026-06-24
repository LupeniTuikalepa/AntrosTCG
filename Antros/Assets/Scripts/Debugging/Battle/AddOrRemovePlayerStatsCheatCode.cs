/*using ATCG.Battle.Cards;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Commands.GameCommands.Players;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.GameModes;
using ATCG.Battle.Players;
using ATCG.Battle.Players.Local;
using ATCG.Battle.Players.Local.Phases;
using ATCG.Battle.Players.Local.Runtime;
using ATCG.Battle.Turns;
using ATCG.Debugging.Debugging.Cheat;
using ATCG.Metrics;
using Helteix.Cards.Collections;
using UnityEngine;
using UnityEngine.InputSystem;


public class AddOrRemovePlayerStatsCheatCode : CheatCode
{
	public void OnAddHealth(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			Player.AddOrRemoveHealth(50);
			ModifyPlayerHealthCommand command = new ModifyPlayerHealthCommand(Player, 50);
			command.Run(Player.BattlePhase);
			Debug.Log($"[AddOrRemovePlayerStatsCheatCode] Les PV de {Player} est à {Player.CurrentHealth}");
		}
	}

	public void OnRemoveHealth(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			Player.AddOrRemoveHealth(-50);
			ModifyPlayerHealthCommand command = new ModifyPlayerHealthCommand(Player, -50);
			command.Run(Player.BattlePhase);
			Debug.Log($"[AddOrRemovePlayerStatsCheatCode] Les PV de {Player} est à {Player.CurrentHealth}");
		}
	}

	public void AddMana(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			Player.AddOrRemoveMana(5);
			ModifyPlayerManaCommand command = new ModifyPlayerManaCommand(Player, 50);
			command.Run(Player.BattlePhase);
			Debug.Log($"[AddOrRemovePlayerStatsCheatCode] Mana de {Player} est à {Player.CurrentMana}");
		}
	}

	public void RemoveMana(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			Player.AddOrRemoveMana(-5);
			ModifyPlayerManaCommand command = new ModifyPlayerManaCommand(Player, -50);
			command.Run(Player.BattlePhase);
			Debug.Log($"[AddOrRemovePlayerStatsCheatCode] Mana de {Player} est à {Player.CurrentMana}");
		}
	}*/


