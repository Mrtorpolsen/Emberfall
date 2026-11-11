using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;
using Unity.Services.CloudCode.Shared;
using Unity.Services.CloudSave.Model;

namespace EmberfallModule;

// Reads and writes the player's "progress" key: Cloud Save Protected player data, which the client can read
// but only server code (the service token) can write.
public class ProgressStore
{
    public const string ProgressKey = "progress";

    // ClientState (loadouts, tutorial flag): Default access, written by the game itself (docs/cloud-save.md)
    public const string ClientKey = "client";

    readonly IGameApiClient m_GameApiClient;

    public ProgressStore(IGameApiClient gameApiClient)
    {
        m_GameApiClient = gameApiClient;
    }

    // The stored progress and its write lock, or (null, null) when the player has none
    public async Task<(ServerProgress? Progress, string? WriteLock)> LoadAsync(IExecutionContext context)
    {
        var response = await m_GameApiClient.CloudSaveData.GetProtectedItemsAsync(
            context,
            context.AccessToken,
            context.ProjectId,
            context.PlayerId!,
            new List<string> { ProgressKey });

        var item = response.Data.Results.FirstOrDefault();

        if (item?.Value == null)
            return (null, null);

        var progress = JsonConvert.DeserializeObject<ServerProgress>(item.Value.ToString()!);
        return (progress, item.WriteLock);
    }

    // Writes the progress. With a write lock, fails if someone else wrote it since it was read.
    public async Task SaveAsync(IExecutionContext context, ServerProgress progress, string? writeLock)
    {
        JObject value = JObject.Parse(JsonConvert.SerializeObject(progress));

        await m_GameApiClient.CloudSaveData.SetProtectedItemAsync(
            context,
            context.ServiceToken,
            context.ProjectId,
            context.PlayerId!,
            new SetItemBody(ProgressKey, value, writeLock!));
    }

    // Deletes the progress key and the client key (account deletion). A key that doesn't exist is fine.
    public async Task DeleteAllAsync(IExecutionContext context)
    {
        // Named: every parameter is a string, so a wrong order would still compile
        await IgnoreNotFound(m_GameApiClient.CloudSaveData.DeleteProtectedItemAsync(
            context, accessToken: context.ServiceToken, projectId: context.ProjectId, playerId: context.PlayerId!, key: ProgressKey));

        await IgnoreNotFound(m_GameApiClient.CloudSaveData.DeleteItemAsync(
            context, accessToken: context.AccessToken, projectId: context.ProjectId, playerId: context.PlayerId!, key: ClientKey));
    }

    static async Task IgnoreNotFound(Task call)
    {
        try
        {
            await call;
        }
        catch (ApiException e) when (e.Response?.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
        }
    }

    public static string ToJson(ServerProgress progress) => JsonConvert.SerializeObject(progress);

    // Cloud Save refuses a write whose write lock no longer matches (someone wrote since it was read)
    public static bool IsWriteConflict(ApiException e) => e.Response?.StatusCode == System.Net.HttpStatusCode.Conflict;
}
