using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;

namespace EmberfallModule;

// Server-authoritative player progress (docs/cloud-save.md). Progress is Cloud Save Protected
// player data: the client can read it, only this module writes it.
public class ProgressModule
{
    readonly ProgressStore m_Store;
    readonly ILogger<ProgressModule> m_Logger;

    public ProgressModule(IGameApiClient gameApiClient, ILogger<ProgressModule> logger)
    {
        m_Store = new ProgressStore(gameApiClient);
        m_Logger = logger;
    }

    // The player's progress as ServerProgress JSON, or null if they have none yet (then the game imports its
    // local save with ImportLocalSave)
    [CloudCodeFunction("GetProgress")]
    public async Task<string?> GetProgress(IExecutionContext context)
    {
        var (progress, _) = await m_Store.LoadAsync(context);
        return progress == null ? null : ProgressStore.ToJson(progress);
    }

    // First cloud login: turns the player's local progress (ServerProgress JSON) into their server progress,
    // limited to what the definitions allow (ProgressImport). Only works once: when the player already has
    // server progress it is returned unchanged.
    [CloudCodeFunction("ImportLocalSave")]
    public async Task<string> ImportLocalSave(IExecutionContext context, string progressJson)
    {
        var (existing, _) = await m_Store.LoadAsync(context);

        if (existing != null)
            return ProgressStore.ToJson(existing);

        ServerProgress? local = null;

        try
        {
            local = JsonConvert.DeserializeObject<ServerProgress>(progressJson ?? "");
        }
        catch (JsonException e)
        {
            m_Logger.LogWarning("Unreadable local progress for {PlayerId}, starting fresh: {Error}", context.PlayerId, e.Message);
        }

        var progress = ProgressImport.Sanitize(local, DefinitionsStore.Talents, DefinitionsStore.Research,
            DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        await m_Store.SaveAsync(context, progress, writeLock: null);
        return ProgressStore.ToJson(progress);
    }

    // The talent and research definitions this module checks purchases against, as
    // { "talents": ..., "research": ... }. The game uses them when they're newer than its bundled copy.
    [CloudCodeFunction("GetDefinitions")]
    public string GetDefinitions()
    {
        return DefinitionsStore.CombinedJson;
    }
}
