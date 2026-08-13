using ATCG.Battle.Players.Local.Runtime;
using ATCG.HexGrids;
using UnityEngine;

namespace ATCG.Cutscenes
{
    /// <summary>
    /// Game-side coordinate solver: delegates to the screen player's runtime battle
    /// grid. Injected into the cutscene context so elements resolve world positions
    /// without reaching into the player/grid hierarchy themselves.
    /// </summary>
    public sealed class GridCoordinateSolver : ICutsceneCoordinateSolver
    {
        private readonly RuntimeLocalBattlePlayer screenPlayer;

        public GridCoordinateSolver(RuntimeLocalBattlePlayer screenPlayer)
        {
            this.screenPlayer = screenPlayer;
        }

        public Vector3 ToWorld(HexCoordinates coordinates)
        {
            return screenPlayer.RuntimeBattleGrid.RuntimeHexGrid.GetPositionAt(coordinates);
        }

        public float ToWorldDistance(int hexDistance)
        {
            return screenPlayer.RuntimeBattleGrid.RuntimeHexGrid.Current.OuterCellRadius * hexDistance;
        }
    }
}