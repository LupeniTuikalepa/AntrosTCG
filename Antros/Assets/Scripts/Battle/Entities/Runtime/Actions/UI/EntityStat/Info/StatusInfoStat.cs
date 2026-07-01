using System;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Entities.Components.Implementations;
using ATCG.Capacities.Data.Status;
using UnityEngine;

namespace ATCG.Battle.Entities.Runtime.Actions.UI.EntityStat
{
	public class StatusInfoStat : HoverStateUIElement
	{
		[SerializeField] private GameObject stausInfoStat;
		public override bool Build()
		{
			StatusData[] datas = Resources.LoadAll<StatusData>("Database/Status");
			if (datas != null || datas.Length != 0)
			{
				stausInfoStat.SetActive(true);
			}
			Debug.Log(datas.Length);
			stausInfoStat.SetActive(false);
			return false;
		}
	}
}