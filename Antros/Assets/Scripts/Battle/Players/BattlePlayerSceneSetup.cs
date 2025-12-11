using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ATCG.Battle.Players.Runtime;
using ATCG.Battle.Players.Runtime.Local;
using ATCG.Cards;
using ATCG.Players;
using Helteix.Tools;
using Helteix.Tools.Phases;
using Helteix.Tools.Phases.Listeners;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using Random = UnityEngine.Random;

namespace ATCG.Battle.Players
{
    public class BattlePlayerSceneSetup : MonoPhaseListener<BattleGameMode>
    {
        [SerializeField]
        private RuntimeLocalBattlePlayer localBattlePlayerPrefab;

        [SerializeField, ChildGameObjectsOnly]
        private Transform container;

        [SerializeField]
        private PlayerInputManager manager;

        private Dictionary<IBattlePlayer, RuntimeBattlePlayer> runtimeBattlePlayers;

        private void Awake()
        {
            runtimeBattlePlayers = new Dictionary<IBattlePlayer, RuntimeBattlePlayer>();
        }

        private IEnumerator Start()
        {
            yield return new WaitForEndOfFrame();

            if (GameController.GameModeController.Current == null)
            {
                PlayerInfos[] players = new PlayerInfos[]
                {
                    new PlayerInfos()
                    {
                        name = "Player 1"
                    },
                    new PlayerInfos()
                    {
                        name = "Player 2"
                    }
                };
                string[] allCards =
                    GameController.GameDatabase.GetAll<GameCardData>()
                        .Select(ctx => ctx.ID.ToString())
                        .ToArray();

                //PlayerInputPairing[] pairings = result.result;
                IBattlePlayerProfile[] localPlayerProfiles = new IBattlePlayerProfile[players.Length];
                for (int i = 0; i < players.Length; i++)
                {
                    localPlayerProfiles[i] = new LocalPlayerProfile()
                    {
                        ID = i,
                        Infos = players[i],
                        Deck = new PlayerDeck()
                        {
                            cards = allCards,
                        },
                    };
                }
                int seed = Random.Range(int.MinValue, int.MaxValue);
                _ = new OfflineBattleGameMode(seed, localPlayerProfiles).Run();
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            manager.onPlayerJoined += OnPlayerJoined;
            manager.onPlayerLeft += OnPlayerLeft;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            manager.onPlayerJoined -= OnPlayerJoined;
            manager.onPlayerLeft -= OnPlayerLeft;
        }


        private void OnPlayerJoined(PlayerInput playerInput)
        {
            Debug.Log($"New player joined {playerInput.name}_{playerInput.playerIndex}_{playerInput.user.valid}", playerInput);
        }

        private void OnPlayerLeft(PlayerInput playerInput)
        {
            Debug.Log($"Player left {playerInput.name}_{playerInput.playerIndex}_{playerInput.user.valid}", playerInput);
        }


        protected override void OnPhaseBegin(BattleGameMode phase)
        {
            base.OnPhaseBegin(phase);

            runtimeBattlePlayers.Clear();
            container.ClearChildren();

            manager.EnableJoining();
            for (int i = 0; i < phase.Players.Length; i++)
            {
                IBattlePlayer player = phase.Players[i];
                RuntimeBattlePlayer runtimeBattlePlayer = null;
                switch (player)
                {
                    case LocalBattlePlayer localBattlePlayer:
                        InputDevice[] inputDevices = InputUser.all[i].pairedDevices.ToArray();
                        PlayerInput playerInput = PlayerInput.Instantiate(localBattlePlayerPrefab.gameObject, playerIndex:i, splitScreenIndex:i, pairWithDevices: inputDevices);
                        runtimeBattlePlayer = playerInput.GetComponent<RuntimeBattlePlayer>();
                        break;
                }

                if (runtimeBattlePlayer)
                {
                    runtimeBattlePlayer.transform.SetParent(container);
                    runtimeBattlePlayer.Connect(player);
                    runtimeBattlePlayers.Add(player, runtimeBattlePlayer);
                }
            }
            manager.DisableJoining();
        }

        protected override void OnPhaseEnd(BattleGameMode phase)
        {
            foreach ((IBattlePlayer battlePlayer, RuntimeBattlePlayer runtimeBattlePlayer) in runtimeBattlePlayers)
                runtimeBattlePlayer.Disconnect(battlePlayer);

            runtimeBattlePlayers.Clear();

            container.ClearChildren();
            base.OnPhaseEnd(phase);
        }
    }
}