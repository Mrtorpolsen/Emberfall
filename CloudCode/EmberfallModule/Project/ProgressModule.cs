using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;
using Unity.Services.CloudCode.Shared;

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

    // The player's progress as a ProgressResponse; its Progress is null if they have none yet (then the game
    // imports its local save with ImportLocalSave)
    [CloudCodeFunction("GetProgress")]
    public async Task<string> GetProgress(IExecutionContext context)
    {
        var (progress, _) = await m_Store.LoadAsync(context);
        return Respond(null, progress);
    }

    // First cloud login: turns the player's local progress (ServerProgress JSON) into their server progress,
    // limited to what the definitions allow (ProgressImport). Only works once: when the player already has
    // server progress it is returned unchanged. Returns a ProgressResponse.
    [CloudCodeFunction("ImportLocalSave")]
    public async Task<string> ImportLocalSave(IExecutionContext context, string progressJson)
    {
        var (existing, _) = await m_Store.LoadAsync(context);

        if (existing != null)
            return Respond(null, existing);

        ServerProgress? local = null;

        try
        {
            local = JsonConvert.DeserializeObject<ServerProgress>(progressJson ?? "");
        }
        catch (JsonException e)
        {
            m_Logger.LogWarning("Unreadable local progress for {PlayerId}, starting fresh: {Error}", context.PlayerId, e.Message);
        }

        var progress = ProgressImport.Sanitize(local, DefinitionsStore.Talents, DefinitionsStore.Research, Now());

        await m_Store.SaveAsync(context, progress, writeLock: null);
        return Respond(null, progress);
    }

    // Buys the next level of a talent: fromLevel is the level the game shows, definitionsVersion the talent
    // definitions it priced it from. Returns a ProgressResponse.
    [CloudCodeFunction("BuyTalent")]
    public Task<string> BuyTalent(IExecutionContext context, string unit, string talentId, int fromLevel, int definitionsVersion)
    {
        TalentTree tree = DefinitionsStore.Talents;

        if (definitionsVersion != tree.DefinitionsVersion)
            return Task.FromResult(Respond(ProgressResponse.DefinitionsOutdated, null));

        return MutateAsync(context, progress => TalentPurchase.Apply(progress, tree, unit, talentId, fromLevel));
    }

    // Starts the next level of a research: fromLevel is the level the game shows, definitionsVersion the research
    // definitions it priced it from. The start time is the server's. Returns a ProgressResponse.
    [CloudCodeFunction("StartResearch")]
    public Task<string> StartResearch(IExecutionContext context, string researchId, int fromLevel, int definitionsVersion)
    {
        ResearchTree tree = DefinitionsStore.Research;

        if (definitionsVersion != tree.DefinitionsVersion)
            return Task.FromResult(Respond(ProgressResponse.DefinitionsOutdated, null));

        return MutateAsync(context, progress => ResearchActions.Start(progress, tree, researchId, fromLevel, Now()));
    }

    // Completes a running research once its time is up on the server's clock (NotDone before that).
    // Returns a ProgressResponse.
    [CloudCodeFunction("CompleteResearch")]
    public Task<string> CompleteResearch(IExecutionContext context, string researchId)
    {
        return MutateAsync(context, progress => ResearchActions.Complete(progress, DefinitionsStore.Research, researchId, Now()));
    }

    // Refunds every talent (the Cinders recorded as spent) and clears them. Returns a ProgressResponse.
    [CloudCodeFunction("RefundTalents")]
    public Task<string> RefundTalents(IExecutionContext context)
    {
        return MutateAsync(context, ProgressActions.RefundTalents);
    }

    // The gameplay tutorial's Cinders, once per player. Returns a ProgressResponse.
    [CloudCodeFunction("ClaimTutorialGift")]
    public Task<string> ClaimTutorialGift(IExecutionContext context)
    {
        return MutateAsync(context, ProgressActions.ClaimTutorialGift);
    }

    static long Now() => DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    // Loads the progress, runs the action on it and writes it back with the write lock it was read with.
    // If another write got in between (the player acting on two devices at once), the whole thing runs once more.
    async Task<string> MutateAsync(IExecutionContext context, Func<ServerProgress, ActionOutcome> action)
    {
        for (int attempt = 0; ; attempt++)
        {
            var (progress, writeLock) = await m_Store.LoadAsync(context);

            if (progress == null)
                return Respond(ProgressResponse.NoProgress, null);

            progress.EnsureDefaults();
            ActionOutcome outcome = action(progress);

            if (!outcome.Changed)
                return Respond(outcome.Error, progress);

            try
            {
                await m_Store.SaveAsync(context, progress, writeLock);
                return Respond(null, progress);
            }
            catch (ApiException e) when (ProgressStore.IsWriteConflict(e))
            {
                if (attempt >= 1)
                {
                    m_Logger.LogWarning("Write conflict on {PlayerId}'s progress, giving up", context.PlayerId);
                    return Respond(ProgressResponse.Conflict, null);
                }
            }
        }
    }

    static string Respond(string? error, ServerProgress? progress) =>
        JsonConvert.SerializeObject(new ProgressResponse { Error = error, Progress = progress, ServerTime = Now() });

    // The talent and research definitions this module checks purchases against, as
    // { "talents": ..., "research": ... }. The game uses them when they're newer than its bundled copy.
    [CloudCodeFunction("GetDefinitions")]
    public string GetDefinitions()
    {
        return DefinitionsStore.CombinedJson;
    }
}
