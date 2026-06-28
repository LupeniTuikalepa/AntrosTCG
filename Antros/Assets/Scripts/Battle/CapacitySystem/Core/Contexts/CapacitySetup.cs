using ATCG.Battle.Entities;
using ATCG.Battle.GameModes;
using ATCG.Capacities;
using ATCG.HexGrids;

namespace ATCG.Battle.Commands.GameCommands
{
    public readonly struct CapacitySetup
    {
        public readonly CapacityData data;
        public readonly HexCoordinates castPoint;
        public readonly BattlePhase battlePhase;
        public readonly EntityAddress caster;

        public CapacitySetup(CapacityData data, HexCoordinates castPoint, BattlePhase battlePhase)
        {
            this.data = data;
            this.castPoint = castPoint;
            this.battlePhase = battlePhase;
            this.caster = EntityAddress.None;
        }

        public CapacitySetup(CapacityData data, HexCoordinates castPoint, BattlePhase battlePhase, EntityAddress caster)
        {
            this.data = data;
            this.castPoint = castPoint;
            this.battlePhase = battlePhase;
            this.caster = caster;
        }
    }
}