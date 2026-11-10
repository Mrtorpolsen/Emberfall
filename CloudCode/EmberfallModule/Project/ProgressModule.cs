using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;

namespace EmberfallModule;

// Server-authoritative player progress (docs/cloud-save.md). Progress is Cloud Save Protected
// player data: the client can read it, only this module writes it.
public class ProgressModule
{
    const string ProgressKey = "progress";

    readonly IGameApiClient m_GameApiClient;
    readonly ILogger<ProgressModule> m_Logger;

    public ProgressModule(IGameApiClient gameApiClient, ILogger<ProgressModule> logger)
    {
        m_GameApiClient = gameApiClient;
        m_Logger = logger;
    }

    // Returns the player's progress JSON, or null if they have none yet
    [CloudCodeFunction("GetProgress")]
    public async Task<string?> GetProgress(IExecutionContext context)
    {
        var response = await m_GameApiClient.CloudSaveData.GetProtectedItemsAsync(
            context,
            context.AccessToken,
            context.ProjectId,
            context.PlayerId!,
            new List<string> { ProgressKey });

        return response.Data.Results.FirstOrDefault()?.Value?.ToString();
    }
}
