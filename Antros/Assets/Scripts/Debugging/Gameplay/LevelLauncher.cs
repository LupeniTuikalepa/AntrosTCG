using System.Linq;
using ATCG.Battle;
using ATCG.Battle.Players;
using ATCG.Cards;
using ATCG.GameModes;
using ATCG.Players;
using Helteix.Tools.Phases;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Debugging.Debugging.Gameplay
{
    public class LevelLauncher : MonoBehaviour
    {
        [SerializeField, AssetsOnly]
        private PlayerDeck[] startingDeck;

        private void Awake()
        {

        }
    }
}