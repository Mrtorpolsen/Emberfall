# Cloud save design (server-authoritative)

Status: design, Nov 9 2026. Branch `feat/cloud-save`. Replaces the "sync the local JSON" plan in workstream 2 of `docs/release-plan.md`.

## Goal

Player progress lives on the server and **the client can't write it**. Storing the save in normal Cloud Save player data isn't enough: that data is client-writable, so a modded APK or a REST call with the player's own token could upload maxed talents. Instead:

- Progress (currencies, talents, research, reward bookkeeping) is Cloud Save **Protected** player data: the client can read it, only server code can write it.
- Every change to progress is a **Cloud Code C# module** function that validates the request against the definitions and writes the result.
- The local JSON becomes a **read-only cache** of the last server state (fast menus, offline runs). It is never uploaded as-is.

Side effects: fewer writes (one call per real action instead of one per currency change), research timers use the server clock, and Embers are already server-owned when IAP arrives.

What the server can't do: verify that a run really happened. The game simulates on the device, so run rewards are only **rate-limited** by server time (see [Run rewards](#run-rewards)). The most a cheater can get is what a real player gets for playing that long.

## Storage

| Key | Access class | Written by | Contents |
|---|---|---|---|
| `progress` | Protected | Cloud Code only | `ServerProgress`: `Currency`, `Talents`, `Research`, `HasReceivedLoginGift`, `RewardClockUtc`, recent claimed run IDs, `Version` |
| `client` | Default | Client | `ClientState`: `Loadouts`, `HasCompletedTutorial`, `Version` |

- One key for all progress: a talent purchase changes currency and talents together, and one key keeps that atomic.
- Loadouts and the tutorial flag aren't worth cheating (a loadout can only hold unlocked definitions; `LoadoutPruner` already enforces that), so the client writes them directly. `client` is written debounced (after leaving the loadout screen, on app pause), not on every change.
- Limits (docs): Protected data up to 5 MB and 2000 keys per player. The save is a few KB.
- Server writes use `writeLock` (optimistic concurrency): read item + lock → validate → write with that lock. On a conflict (two devices acting at once) retry the whole function once, then fail.

### Save version 5

`SaveGame` splits into `ServerProgress` + `ClientState`. The local cache file holds both plus sync metadata:

```
LocalCache { Version 5, Progress (last server copy), Client, PendingRuns[], LastServerTimeUtc }
```

Per the save-shape convention: keep the v4 shape as `SaveGame4`, add `Migrate4To5`. Like the current save classes, the new ones are plain C# without `[Serializable]` (Newtonsoft serializes them, Unity never does; see CLAUDE.md). `SaveSerializer` keeps parsing/migrating the cache; `ServerProgress` gets its own version so the module can migrate it too.

### Existing players

Closed-test players have local v4 saves. On the first cloud login with no `progress` key, the client sends its local save once to `ImportLocalSave`. The server only accepts it when no `progress` exists yet, and clamps it to sane limits (currency cap, talent levels ≤ max, research levels ≤ max). That one import is trusting the client; it's acceptable for a closed test. **Decision needed:** import (recommended) or start everyone fresh.

## Cloud Code module `EmberfallModule`

Every function returns the new `ServerProgress` (or an error code); the client replaces its cache with it and refreshes the UI. No optimistic UI: buttons wait for the response (show a short busy state; failures use `Toast.Show`).

| Function | Replaces | Server checks | Writes |
|---|---|---|---|
| `GetProgress()` | `SaveService.Load` | — (creates an empty `progress` on first call) | — |
| `BuyTalent(unitKey, talentId, fromLevel)` | `TalentService` buy (`TalentService.cs:235`) | cost from definitions, max level, prerequisites, enough Cinders | Cinders −cost, talent +1 |
| `RefundTalents()` | `TalentService.RefundAllAsync` (`TalentService.cs:202`) | refunds `CurrencySpent` as recorded on the server | talents cleared, currency back |
| `StartResearch(researchId, fromLevel)` | `ResearchUIController.cs:60` | cost, max level, active-research slot free | Cinders −cost, `ActiveResearch` with **server** start time |
| `CompleteResearch(researchId)` | research completion in `ResearchService` | duration elapsed on the **server** clock | level +1, active cleared |
| `ClaimRunRewards(runs[])` | `GameManager.AwardReward` (`GameManager.cs:184`) | see below | Cinders +reward, reward clock |
| `ClaimTutorialGift()` | `GameManager.EndOfTutorial` (`GameManager.cs:167`) | `HasReceivedLoginGift` false | +2000 Cinders, flag set |
| `ImportLocalSave(save)` | first cloud login | no `progress` yet, clamps | `progress` |
| `DeleteMyData()` | `AccountService` | — | deletes `progress` and `client` before the account is deleted |

**Retries are safe.** A call that timed out may still have been applied. Each mutating call carries the state it expects (`fromLevel`, run IDs), so a retry of an already-applied action is a no-op that returns the current progress instead of buying a second level.

**Definitions version.** Each call carries the client's `definitionsVersion` (already in `Talents.json`/`Research.json`). If it differs from the server's, the server rejects with `DefinitionsOutdated` and the client refetches definitions before retrying, so the cost shown is the cost charged.

### Run rewards

Rewards are purely time-based (`CinderRewardCalculator`: Cinders per minute survived × difficulty multiplier), so the server bounds them by real time:

- Each run ends with a record `{ runId, difficulty, minutes, endedAtClient }` appended to `LocalCache.PendingRuns`, then `ClaimRunRewards` is called (immediately when online, on reconnect when not; this follows the `PendingScoreQueue` pattern).
- The server keeps `RewardClockUtc`. Claiming runs totalling `m` minutes requires `RewardClockUtc + m ≤ now`, then advances the clock by `m`. The clock can't fall more than `MaxBankedHours` (e.g. 12 h, tunable) behind `now`, so a cheater can't save up a week of rewards.
- Per-run cap on `minutes` (longest plausible run). Runs over the budget are capped, not rejected, so an honest player is never refused.
- `runId` (GUID from the client) makes claims idempotent; the server keeps the last N claimed IDs.
- Difficulty is whatever the client says; the server applies that difficulty's `RewardMultiplier`.

Online and offline runs use the same path, so there's one code path to test. Offline players keep their rewards; they just arrive on reconnect (`IdentityService.Reconnected`).

### Offline

- Runs: playable, rewards queued (above).
- Talents, research, refunds: need a connection. Buttons show "Connect to buy" when signed out. Research that finishes while offline completes on the next `CompleteResearch` call.
- Menus show the cached progress.

## Definitions: embedded in the module

The server needs the same costs the client shows. Decided Nov 10: instead of Remote Config, the module **embeds** `Talents.json` and `Research.json` from `Assets/Addressables/Data/` at build time (`EmbeddedResource` in `EmberfallModule.csproj`). One source of truth in git, checked by `DataValidationTests`, no Dashboard copy, no size question. This settles the plan's "Remote Config or CCD" question (neither). What we give up: editing values in the Dashboard without a deploy, and Remote Config A/B overrides.

- Module: `DefinitionsStore` reads and parses them once per worker; `GetDefinitions` returns `{ "talents": ..., "research": ... }`. The module's `TestProject` checks they parse on the server's .NET.
- Client: `Bootstrapper` calls `ServerDefinitions.FetchAsync()` before loading the save (skipped when signed out, 4 s timeout). `TalentService`/`ResearchService` use the server's copy when `DefinitionsSelector` prefers it: same `schemaVersion` and a higher `definitionsVersion` than the bundled Addressables copy, which stays the offline fallback.
- **A balance change**: edit the JSON, bump its `definitionsVersion`, run the tests, deploy the module (development, then production). Players get it on their next launch without a store update; the next build bundles it too.
- Old builds keep calling the newest module, so `schemaVersion` must change whenever the JSON's shape changes in a way old builds can't read; they then keep their bundled copy.

## Shared logic

Cost/duration/reward calculations must give the same answer on client and server. The module's `.csproj` compiles plain C# files from `Assets/` via `<Compile Include="..." Link="..." />`, so both sides run the same file. Those files must not use `UnityEngine`:

- Talent rules: done (Nov 10). `TalentTree.BuildTalentsByUnit` (talent IDs), `Talent.GetCurrentCost` (flat per tier, as the game has always charged; `CostMultiplier` unused) and `TalentRules` (`CanPurchase`: known talent, purchasable type (stat or unit unlock), `MaxPurchases` (0 = not for sale), tier and talent prerequisites, enough Cinders; `ApplyPurchase`). `TalentUnlockManager` is gone; tier points are computed from the state.
- Research rules: done (Nov 10). `ResearchDefinition.cs` is plain C# (missing scaling preset throws, unknown categories are skipped for older clients) and `ResearchRules` (`CanStart`: known research, category free, below `MaxLevel`, prerequisites, enough Cinders; `Start` with a caller-given time, `GetEndTime`/`IsDone`, `Complete`).
- `CinderRewardCalculator`: done (Nov 10), takes the difficulty as a parameter.
- Validation rules (can buy talent? can start research?) as plain C# classes, covered by EditMode tests in Unity. Module functions stay thin: load → validate with shared code → write.

## Tooling

- **.NET SDK** on Windows (free dev tool, nothing ships in the APK). Version: the docs disagree (the current module docs say modules target .NET 10, the overview says up to .NET 9). Pick it from the module template when creating the project.
- **Editor deploy:** the new Cloud Code 3.0 editor workflow (`.ccmu`) needs Unity 6.7+ (alpha/beta as of Nov 2026), so it doesn't apply to our 6000.6.8f1. Use the **module reference workflow**: `com.unity.services.deployment` + a `.ccmr` file in `Assets/` pointing at the module solution (Unity 2021.3+, Cloud Code package 2.6.1+; we have 2.10.4). Alternative: UGS CLI `ugs deploy <module>.ccm`.
- **Repo layout:** `CloudCode/EmberfallModule/` (outside `Assets/`, so Unity doesn't compile it). `.gitignore` ignores `*.csproj`/`*.sln`; add exceptions for `CloudCode/**`, and ignore its `bin/`/`obj/`.
- Packages: `Com.Unity.Services.CloudCode.Core` (`[CloudCodeFunction("Name")]`) and the Cloud Code APIs package for the Cloud Save client.
- Client calls: `CloudCodeService.Instance.CallModuleEndpointAsync("EmberfallModule", "BuyTalent", args)`, wrapped in one `ProgressService` (or similar) that owns the cache.
- Limits (docs): 15 s per call, 10 MB per module, 20 modules per project, 600 Cloud Code requests per minute per player.
- **Environments:** the editor and development builds use UGS `development`, release builds `production` (`ServicesEnvironment`). Develop and deploy against `development`; deploy the module to `production` before the release that needs it (see Releasing in `docs/release-plan.md`).
- `DeletePlayerScores.js` can stay a JS script, or move into the module later.

## Implementation order

1. `.NET` SDK, module skeleton with one `GetProgress`, `.ccmr` deploy, called from the client. Proves the pipeline end to end.
2. Shared plain-C# logic (cost, research, rewards) extracted with tests; the client switches to it first with no behavior change.
3. Save v5 split (`ServerProgress`/`ClientState`/`LocalCache`), migration and tests. Done (Nov 10): the local cache kept the name `SaveGame` (`Progress`, `Client`, `PendingRuns`); `LastServerTimeUtc` comes with the step that uses it.
4. Definitions from the module (embedded JSON + client fetch with bundled fallback). Done (Nov 10).
5. Server-owned progress, in parts: 5a `GetProgress` + `ImportLocalSave` (moved up from step 7) and `SaveService.Load` taking the server's progress — done (Nov 11; import caps: 250,000 Cinders, about what every talent costs, and 1,000 Embers); 5b `BuyTalent` — done (Nov 11: `TalentPurchase.Apply` + `MutateAsync` with the write lock and one retry; the game sends it through `ServerActions` and needs a connection); 5c research — done (Nov 11: `StartResearch` charges and stamps the server's time; `CompleteResearch` checks it on the server's clock, the game retries on `NotDone` with backoff and on reconnect, and counts down on `ServerClock` (device time corrected by `ProgressResponse.ServerTime`, which every module answer carries; `GetProgress`/`ImportLocalSave` return a `ProgressResponse` too); the tutorial's local research reset is gone, and a player who already has research is treated as past the tutorial); then `RefundTalents`, `ClaimTutorialGift`. Until 5c and step 6 are done, local changes to progress are overwritten by the server copy on the next launch, so nothing ships from this branch before then.
6. `ClaimRunRewards` with the pending-run queue.
7. `DeleteMyData` in account deletion.
8. Lock `client` writes to debounced points; remove direct `SaveService.Save` calls for progress.
9. Android auto-backup: exclude the cache file.
10. Device test: fresh install, existing v4 save import, offline run then reconnect, two devices.

## Release checklist when this ships

From "Re-check when features ship" in `docs/release-plan.md`: Data safety form (game progress stored on the server), privacy policy (progress is no longer device-only), delete-account page (cloud data is deleted too).

## Still to verify while building

- Client SDK: exact `LoadAsync` call for Protected data (`LoadOptions` with `ProtectedReadAccessClassOptions`, Cloud Save 3.x).
- Module: `SetProtectedItemAsync` (with `context.ServiceToken`?) for writes and how `writeLock` is passed. The methods and the write-lock conflict fields exist in `Com.Unity.Services.CloudCode.Apis` 0.0.26; `GetProtectedItemsAsync` with `context.AccessToken` works (`GetProgress`).
- Whether deleting a UGS player also deletes their Cloud Save data (we delete explicitly either way).

## Decisions

- [x] Server-authoritative progress via Cloud Code + Protected data (Nov 9).
- [x] Cloud Code language: C# module (Nov 9).
- [x] Offline run rewards: queued and bounded by server time (Nov 10).
- [x] Existing local saves: one-time import, clamped (Nov 10).
- [x] `MaxBankedHours` = 12 (Nov 10).
- [x] Per-run minute cap: a single constant, 120 minutes for now (Nov 10). The game is likely moving from endless runs to levels of about 10–15 minutes, which will change how run rewards are earned; revisit the reward claim then rather than tuning the cap.
