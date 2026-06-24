using ATCG.Battle.Players.Local.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;

public class ICheatCode : RuntimeLocalPlayerComponent
{
	
	private PlayerInput cheatCodeInputAction;
	public string name;
	public string description;

	public void Execute()
	{
		
	}

	protected override void Connect(RuntimeLocalBattlePlayer runtimeLocalBattlePlayer)
	{
		
	}

	protected override void Disconnect(RuntimeLocalBattlePlayer runtimeLocalBattlePlayer)
	{
	}
}
