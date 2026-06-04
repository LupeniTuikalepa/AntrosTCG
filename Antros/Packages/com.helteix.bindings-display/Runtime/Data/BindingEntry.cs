using System;
using System.Text.RegularExpressions;
using Helteix.ControlDisplay.UI.Interfaces;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;

namespace Helteix.ControlDisplay.Data
{
	[Serializable]
	public struct BindingEntry
	{
		[field: SerializeField, InputControl]
		public string ControlPath { get; private set; }

		[field: SerializeField]
		public GameObject OverridePrefab { get; private set; }

		[field: SerializeField]
		public BindingIcons Icons { get; private set; }

		public bool Matches(BindingDescription input) => InputControlPath.Matches(ControlPath, input.control);
	}
}