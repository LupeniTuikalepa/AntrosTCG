using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.HexGrids.Patterns
{
	[Serializable]
	public class TridentPatternData : PatternData
	{
		[field: SerializeField, BoxGroup("Specific"), Min(1)]
		public int Range { get; private set; }

		[field: SerializeField, BoxGroup("Specific"), Range(1, 6)]
		[Tooltip("Nombre de branches, doit rester impair pour une fourche symétrique (3, 5, 7...)")]
		public int BranchCount { get; private set; } = 3;

		[field: SerializeField, BoxGroup("Specific"), Range(0, 90)]
		[Tooltip("Écart angulaire entre chaque branche. Sera arrondi au multiple de 60° le plus proche (contrainte de la grille hexagonale).")]
		public int AngleStepDegrees { get; private set; } = 20;

		public int AngleStepInHexDirections => Mathf.Max(1, Mathf.RoundToInt(AngleStepDegrees / 60f));

#if UNITY_EDITOR
		private void OnValidate()
		{
			if (BranchCount % 2 == 0)
				BranchCount++;
		}
#endif
	}
}