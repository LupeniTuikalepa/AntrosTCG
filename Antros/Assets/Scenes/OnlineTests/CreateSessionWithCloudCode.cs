using System.Collections.Generic;
using ATCG.Players;
using Unity.Services.Authentication;
using Unity.Services.CloudCode;
using Unity.Services.CloudCode.GeneratedBindings;
using Unity.Services.CloudCode.GeneratedBindings.ATCGModule.Data;
using Unity.Services.Core;
using UnityEngine;

public class CreateSessionWithCloudCode : MonoBehaviour
{
    public async void Awake()
    {
        await UnityServices.InitializeAsync();

        // Sign in anonymously into the Authentication service
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            Debug.Log("Not signed in");
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        try
        {
            BattleSessionModuleBindings module = new BattleSessionModuleBindings(CloudCodeService.Instance);
            string sessionId = await module.InitiateSession("Debug", new List<PlayerBattleInfos>()
            {
                new PlayerBattleInfos()
                {
                    playerID = "Player 1",
                    startingDeck = new List<PlayerCardInDeckInfos>()
                    {
                        new PlayerCardInDeckInfos()
                        {
                            cardID = "Card 1",
                            quantity = 15
                        },
                        new PlayerCardInDeckInfos()
                        {
                            cardID = "Card 2",
                            quantity = 15
                        }
                    }
                },
                new PlayerBattleInfos()
                {
                    playerID = "Player 2",
                    startingDeck = new List<PlayerCardInDeckInfos>()
                    {
                        new PlayerCardInDeckInfos()
                        {
                            cardID = "Card 1",
                            quantity = 15
                        },
                        new PlayerCardInDeckInfos()
                        {
                            cardID = "Card 2",
                            quantity = 15
                        }
                    }
                }
            });

            await module.ConnectPlayerToSession(sessionId);
            Debug.Log($"Connected to session {sessionId}");
        } catch (CloudCodeException exception)
        {
            Debug.LogException(exception);
        }

    }
}