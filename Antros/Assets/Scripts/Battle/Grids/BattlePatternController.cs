using ATCG.Battle.Grids;
using ATCG.HexGrids;
using ATCG.HexGrids.Grids;
using ATCG.HexGrids.Patterns;

public readonly struct BattlePatternController : IHexPatternController
{
    public HexGrid HexGrid => battleGrid.grid;

    public readonly BattleGrid battleGrid;

    public BattlePatternController(BattleGrid battleGrid)
    {
        this.battleGrid = battleGrid;
    }


    /// <summary>
    /// True if propagation stops at this coordinate. Branch onto your real
    /// BattleGrid blocking method (wall / occupied / off-grid).
    /// </summary>
    public bool Blocks(HexCoordinates c)
    {
        if (battleGrid.TryGetBattleCell(c, out var cell))
        {
            if(cell.HasPhysicalMember())
                return true;
        }

        return false;
    }
}