using System;
using System.Threading.Tasks;
using ATCGModule.Data;
using Microsoft.Extensions.Logging;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;
using Unity.Services.CloudCode.Shared;
using Unity.Services.CloudSave.Model;

namespace ATCGModule;


public class BattleSessionModule
{
    private static readonly string BattleSessionID = "BattleSession";
    private static readonly string Player_CurrentSessionID = "CurrentSession";
    private static ILogger<BattleSessionModule> _logger;

    public BattleSessionModule(ILogger<BattleSessionModule> logger)
    {
        _logger = logger;
    }

    [CloudCodeFunction("InitiateSession")]
    public async Task<string> InitiateSessionAsync(IExecutionContext context, IGameApiClient gameApiClient, string gameModeType, PlayerBattleInfos[] playerInfos)
    {
        string sessionId = Guid.NewGuid().ToString();
        string customId = $"BattleSession_{sessionId}";

        try
        {
            await gameApiClient.CloudSaveData.SetCustomItemAsync(context, context.ServiceToken, context.ProjectId, customId, new SetItemBody("gameModeType", gameModeType));
            await gameApiClient.CloudSaveData.SetCustomItemAsync(context, context.ServiceToken, context.ProjectId, customId, new SetItemBody("ID", sessionId));
            for (int i = 0; i < playerInfos.Length; i++)
            {
                SetItemBody itemBody = new SetItemBody($"Player_{i + 1}", playerInfos[i]);
                await gameApiClient.CloudSaveData.SetCustomItemAsync(context, context.ServiceToken, context.ProjectId, customId, itemBody);
            }
        }
        catch (ApiException ex)
        {
            _logger.LogError("Failed to save data. Error: {Error}", ex.Message);
            throw new Exception($"Failed to save data for playerId {context.PlayerId}. Error: {ex.Message}");
        }

        return sessionId;
    }

    [CloudCodeFunction("ConnectPlayerToSession")]
    public async Task ConnectPlayerToSession(IExecutionContext context, IGameApiClient gameApiClient,string sessionId)
    {
        try
        {
            await gameApiClient.CloudSaveData.SetItemAsync(context, context.ServiceToken, context.ProjectId, context.PlayerId, new SetItemBody(Player_CurrentSessionID, sessionId));
        }
        catch (ApiException ex)
        {
            _logger.LogError($"Failed to connect player to session {sessionId}. Error: {{Error}}", ex.Message);
            throw new Exception($"Failed to connect player to session {sessionId}. Error: {ex.Message}");
        }
    }

    [CloudCodeFunction("DisconnectPlayerToSession")]
    public async Task DisconnectPlayerToSession(IExecutionContext context, IGameApiClient gameApiClient)
    {
        try
        {
            await gameApiClient.CloudSaveData.SetItemAsync(context, context.ServiceToken, context.ProjectId,  context.PlayerId, new SetItemBody(Player_CurrentSessionID, string.Empty));
        }
        catch (ApiException ex)
        {
            _logger.LogError("Failed to disconnect player from session Error: {Error}", ex.Message);
            throw new Exception($"Failed to connect player to session Error: {ex.Message}");
        }
    }
}