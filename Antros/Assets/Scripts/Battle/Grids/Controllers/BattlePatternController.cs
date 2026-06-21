using System;
using System.Collections.Generic;
using ATCG.Battle.Entities;
using ATCG.Battle.Grids;
using ATCG.Battle.Players;
using ATCG.HexGrids;
using ATCG.HexGrids.Grids;
using ATCG.HexGrids.Patterns;
using UnityEngine;

public class BattlePatternController : IHexPatternController, IDisposable
{
    public HexGrid HexGrid => battleGrid.grid;
    public readonly BattleGrid battleGrid;
    private readonly IBattlePlayer player;



    public BattlePatternController(BattleGrid battleGrid)
    {
        this.battleGrid = battleGrid;
    }


    /// <summary>
    /// True if propagation stops at this coordinate. Branch onto your real
    /// BattleGrid blocking method (wall / occupied / off-grid).
    /// </summary>
    public virtual bool Blocks(HexCoordinates c)
    {
        if (battleGrid.TryGetBattleCell(c, out var cell))
            return !cell.HasPhysicalMember();

        return true;
    }

    public void Dispose()
    {
        // TODO release managed resources here
    }
}