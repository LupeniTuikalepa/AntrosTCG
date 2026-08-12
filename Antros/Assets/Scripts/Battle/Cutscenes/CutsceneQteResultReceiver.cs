using ATCG.Battle.Commands;
using ATCG.Battle.Commands.GameCommands.Capacities;
using ATCG.Battle.Players;
using ATCG.Battle.Players.Local.Runtime;
using ATCG.Cutscenes;

namespace ATCG.Battle.Cutscenes
{
    /// <summary>
    /// One per screen. The sink a cutscene's QTE arbiter submits resolved scores to: only the OWNER
    /// screen (the player driving the cutscene, e.g. the attacker) turns a score into a networked
    /// <see cref="QteCommand"/> so both screens receive the same value. Non-owner submissions are
    /// dropped. This mirrors the capacity director's role, generalised for any cutscene.
    /// </summary>
    public sealed class CutsceneQteResultReceiver : IQteResultReceiver
    {
        private readonly RuntimeLocalBattlePlayer screenPlayer;
        private readonly BattleID ownerPlayerId;

        public CutsceneQteResultReceiver(RuntimeLocalBattlePlayer screenPlayer, BattleID ownerPlayerId)
        {
            this.screenPlayer = screenPlayer;
            this.ownerPlayerId = ownerPlayerId;
        }

        private bool IsOwner =>
            screenPlayer != null && screenPlayer.BattlePlayer != null
            && screenPlayer.BattlePlayer.ID == ownerPlayerId;

        public void SubmitQteResult(float score)
        {
            if (!IsOwner)
                return;

            IBattlePlayer player = screenPlayer.BattlePlayer;
            new QteCommand(player, score).Run(player.BattlePhase);
        }
    }
}
