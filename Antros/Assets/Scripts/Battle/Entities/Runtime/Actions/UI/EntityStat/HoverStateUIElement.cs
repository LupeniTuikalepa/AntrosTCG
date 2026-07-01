using System;
using ATCG.Battle.Players.Local.Phases;
using UnityEngine;

namespace ATCG.Battle.Entities.Runtime.Actions.UI.EntityStat
{
	public abstract class HoverStateUIElement : MonoBehaviour
	{
		protected StateUIController StateUIController { get; private set; }
		public HoverEntityPhase EntityPhase { get; private set; }

		public void Connect(HoverEntityPhase phase)
		{
			EntityPhase = phase;
			bool succes = Build();
		} 
		public void Disconnect (HoverEntityPhase phase) => EntityPhase = null;
		protected void Awake()
		{
			StateUIController = GetComponentInParent<StateUIController>();
		}

		public abstract bool Build();

	}
}