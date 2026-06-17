using System;
using ATCG.Battle.Entities.Runtime;
using ATCG.Battle.Players.Local;
using ATCG.Battle.Players.Local.Runtime;
using UnityEngine;

namespace ATCG.Battle
{
	public class EntityWorldUI : MonoBehaviour
	{
		private Camera cam;
		private IRuntimeEntity runtimeEntity;

		private void Awake()
		{
			runtimeEntity = GetComponentInParent<IRuntimeEntity>();
		}

		private void Start()
		{
			if (RuntimeLocalBattlePlayer.TryGetRuntimeLocalPlayerFor( runtimeEntity.BattlePlayer as LocalBattlePlayer, out var runtimePlayer))
			{
				cam = runtimePlayer.Camera.Component.OutputCamera;
			}
		}
		private void LateUpdate()
		{
			transform.LookAt(cam.transform.position);
		}
	}
}