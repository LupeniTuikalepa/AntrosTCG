using System;

namespace ATCGModule.Data;

[Serializable]
public struct PlayerBattleInfos
{
    public string playerID;
    public PlayerCardInDeckInfos[] startingDeck;
}