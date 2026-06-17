using ATCG.Battle.Commands.Infos;
using ATCG.Battle.Commands.Players;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.GameModes;
using ATCG.Battle.Players;
using UnityEngine;

namespace ATCG.Battle.Commands.Core
{
    [System.Serializable]
    public abstract class PlayerCommand<T> : Command<T>, IPlayerCommand
        where T : struct, ICommandInfos
    {
        [field: SerializeField]
        public BattleID PlayerID { get; private set; }

        protected PlayerCommand(IBattlePlayer battlePlayer)
        {
            this.PlayerID = battlePlayer.GetBattleID();
        }

        public IBattlePlayer GetPlayer(in CommandContext context) => GetPlayer(context.battlePhase);
        public IBattlePlayer GetPlayer(BattlePhase battlePhase) => battlePhase.GetPlayer(PlayerID);


    }
}