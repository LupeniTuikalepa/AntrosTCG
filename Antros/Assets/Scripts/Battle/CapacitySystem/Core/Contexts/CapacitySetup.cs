using ATCG.Battle.Entities;
using ATCG.Battle.GameModes;
using ATCG.Capacities;
using ATCG.HexGrids;

namespace ATCG.Battle.CapacitySystem.Core
{
    public readonly struct CapacitySetup
    {
        public readonly CapacityData data;
        public readonly HexCoordinates castPoint;
        public readonly BattlePhase battlePhase;

        // Caster ENTITY: optional. None for spell cards with no entity.
        public readonly EntityAddress caster;

        // Casting PLAYER: the real routing key. Always set (hero or spell).
        // Directors compare this against each screen's player to decide owner.
        public readonly BattleID casterPlayerId;

        // Spell-style: no caster entity, player id supplied directly.
        public CapacitySetup(CapacityData data, HexCoordinates castPoint, BattlePhase battlePhase, BattleID casterPlayerId)
        {
            this.data = data;
            this.castPoint = castPoint;
            this.battlePhase = battlePhase;
            this.caster = EntityAddress.None;
            this.casterPlayerId = casterPlayerId;
        }

        // Hero-style: caster entity supplied; player id derived from it.
        public CapacitySetup(CapacityData data, HexCoordinates castPoint, BattlePhase battlePhase, EntityAddress caster, BattleID casterPlayerId)
        {
            this.data = data;
            this.castPoint = castPoint;
            this.battlePhase = battlePhase;
            this.caster = caster;
            this.casterPlayerId = casterPlayerId;
        }
    }
}
