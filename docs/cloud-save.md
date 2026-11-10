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

## Definitions: Remote Config

The server needs the same costs the client shows, so the talent/research definitions move to **Remote Config**. This settles the plan's "Remote Config or CCD" question:

- Two settings of type **JSON**: `talents`, `research`. Strings are capped at 10,000 characters, so they must be JSON settings (`Talents.json` is ~74 KB; the docs give no JSON size limit — verify it uploads).
- Don't put Game Overrides on these keys: the module caches them in memory, which the docs only allow for configs that target all players.
- Module: `RemoteConfigSettings.AssignSettingsGetAsync(...)` with an in-memory cache of a few minutes (as in Unity's quest-system sample).
- Client: fetch at boot (`Bootstrapper`), fall back to the bundled Addressables JSON when offline. The bundled copy stays the source we edit; upload it to Remote Config as part of the release process.

## Shared logic

Cost/duration/reward calculations must give the same answer on client and server. The module's `.csproj` compiles plain C# files from `Assets/` via `<Compile Include="..." Link="..." />`, so both sides run the same file. Those files must not use `UnityEngine`:

- Talent cost: currently `TalentTree.GetCostModel` used from `TalentService` (uses `UnityEngine`/Addressables; extract the math).
- Research cost/time: `ResearchDefinition.cs` (uses `UnityEngine`; extract the math).
- `CinderRewardCalculator`: reads `GameSettingsService`; change it to take the difficulty as a parameter.
- Validation rules (can buy talent? can start research?) as plain C# classes, covered by EditMode tests in Unity. Module functions stay thin: load → validate with shared code → write.

## Tooling

- **.NET SDK** on Windows (free dev tool, nothing ships in the APK). Version: the docs disagree (the current module docs say modules target .NET 10, the overview says up to .NET 9). Pick it from the module template when creating the project.
- **Editor deploy:** the new Cloud Code 3.0 editor workflow (`.ccmu`) needs Unity 6.7+ (alpha/beta as of Nov 2026), so it doesn't apply to our 6000.6.8f1. Use the **module reference workflow**: `com.unity.services.deployment` + a `.ccmr` file in `Assets/` pointing at the module solution (Unity 2021.3+, Cloud Code package 2.6.1+; we have 2.10.4). Alternative: UGS CLI `ugs deploy <module>.ccm`.
- **Repo layout:** `CloudCode/EmberfallModule/` (outside `Assets/`, so Unity doesn't compile it). `.gitignore` ignores `*.csproj`/`*.sln`; add exceptions for `CloudCode/**`, and ignore its `bin/`/`obj/`.
- Packages: `Com.Unity.Services.CloudCode.Core` (`[CloudCodeFunction("Name")]`) and the Cloud Code APIs package for Cloud Save/Remote Config clients.
- Client calls: `CloudCodeService.Instance.CallModuleEndpointAsync("EmberfallModule", "BuyTalent", args)`, wrapped in one `ProgressService` (or similar) that owns the cache.
- Limits (docs): 15 s per call, 10 MB per module, 20 modules per project, 600 Cloud Code requests per minute per player.
- `DeletePlayerScores.js` can stay a JS script, or move into the module later.

## Implementation order

1. `.NET` SDK, module skeleton with one `GetProgress`, `.ccmr` deploy, called from the client. Proves the pipeline end to end.
2. Shared plain-C# logic (cost, research, rewards) extracted with tests; the client switches to it first with no behavior change.
3. Save v5 split (`ServerProgress`/`ClientState`/`LocalCache`), migration and tests.
4. Remote Config definitions (module + client fetch with bundled fallback).
5. `BuyTalent` end to end, then `RefundTalents`, research, `ClaimTutorialGift`.
6. `ClaimRunRewards` with the pending-run queue.
7. `ImportLocalSave` for existing players, `DeleteMyData` in account deletion.
8. Lock `client` writes to debounced points; remove direct `SaveService.Save` calls for progress.
9. Android auto-backup: exclude the cache file.
10. Device test: fresh install, existing v4 save import, offline run then reconnect, two devices.

## Release checklist when this ships

From "Re-check when features ship" in `docs/release-plan.md`: Data safety form (game progress stored on the server), privacy policy (progress is no longer device-only), delete-account page (cloud data is deleted too).

## Still to verify while building

- Client SDK: exact `LoadAsync` call for Protected data (`LoadOptions` with `ProtectedReadAccessClassOptions`, Cloud Save 3.x).
- Module: `SetProtectedItemAsync` with `context.ServiceToken` for writes, `GetProtectedItemsAsync` for reads, and whether `writeLock` is on `SetItemBody` (docs: protected writes need a server authority; omitting `writeLock` ignores conflicts).
- Whether deleting a UGS player also deletes their Cloud Save data (we delete explicitly either way).
- Remote Config JSON setting size with the full `Talents.json`.

## Decisions

- [x] Server-authoritative progress via Cloud Code + Protected data (Nov 9).
- [x] Cloud Code language: C# module (Nov 9).
- [ ] Offline run rewards: queued and bounded by server time (recommended), or offline runs earn nothing.
- [ ] Existing local saves: one-time import (recommended) or fresh start.
- [ ] `MaxBankedHours` and the per-run minute cap.
