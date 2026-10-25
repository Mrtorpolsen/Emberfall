# Emberfall release plan

Status: planning only, as of v0.5.1 (Android internal testing). Nothing in here has been built yet.

## Where things stand

| Area | Current state in the repo |
|---|---|
| Login | `LoginManager` signs in **anonymously** with Unity Authentication, then asks the player to type a name. `UnityAuthIdentity` returns `"offline"` when not signed in. |
| Google Play Games | Plugin v2.1.0 is imported (`Assets/GooglePlayGames`, manifest androidlib, `GPGSIds.cs` with one leaderboard ID), but `PlayGamesManager` is commented out, so it isn't used. |
| Save | `SaveService` writes `{playerId}_save.json` to `persistentDataPath` (local only, save version 4). |
| Talents / research data | `Talents.json` / `Research.json` are bundled Addressables. |
| Leaderboards | Unity Leaderboards (`High_Scores_<Difficulty>`). The GPGS leaderboard in `GPGSIds` isn't used. |
| Currencies | `Cinders`, `Embers` live in the local save. Nothing is validated on a server. |
| Achievements / shop / ads | None. |
| Android build | versionCode 10, minSdk 25, targetSdk = "highest installed". |
| iOS | Not set up. |

## Two things to decide first

These choices affect almost every other workstream.

1. **Backend: stay on Unity Gaming Services (recommended).** Authentication and Leaderboards already run on UGS. Cloud Save, Remote Config, Economy, Cloud Code and Cloud Content Delivery are in the same dashboard, use the same player ID, and cost nothing at this scale. Firebase and PlayFab would also work, but either one means a second identity system and a migration. Revisit only if UGS is missing something specific.
2. **Separate game data from player data.** The JSON is two different problems:
   - *Talent/research **definitions*** (`Talents.json`, `Research.json`) are game config, the same for every player. Serve them from **Remote Config** or **Cloud Content Delivery** (remote Addressables). Then they can be balanced without a store update, with the bundled copy as an offline fallback.
   - *Purchased talents/research, currencies, loadouts and flags* are per-player state. They go in **Cloud Save** (Player Data).

## Workstreams

### 1. Google sign-in (Play Games Services) (do first)

Everything below depends on a stable, recoverable player ID. Right now, a player who reinstalls loses their anonymous account.

- [ ] Play Console → Play Games Services: create or confirm the game project and link it to the app. Add OAuth credentials: an Android client for **both** the upload key SHA-1 and the Play App Signing SHA-1, plus a **web client** for the server auth code.
- [ ] Unity Dashboard → Authentication → add the Google Play Games identity provider (web client ID and secret).
- [ ] Wire up GPGS v2: `PlayGamesPlatform.Activate()`. v2 signs in automatically at launch. Then `RequestServerSideAccess` → `AuthenticationService.SignInWithGooglePlayGamesAsync(authCode)`.
- [ ] **Account linking:** existing internal testers have anonymous accounts. Use `LinkWithGooglePlayGamesAsync` on the anonymous session so progress (and leaderboard entries) carry over. Handle the "already linked to another account" error.
- [ ] Keep anonymous sign-in as the fallback if GPGS sign-in fails or is declined. Offer "Sign in with Google" later from settings.
- [ ] Username: default to the GPGS display name, or keep the custom name prompt (and its validation).
- [ ] Replace the `"offline"` player ID in `UnityAuthIdentity`. Offline play needs a defined behavior (play locally with a cached ID, then sync later).
- [ ] **Account deletion** (required by both Google Play and Apple when accounts exist): an in-app "delete account" option that calls `DeleteAccountAsync` and clears Cloud Save, plus a web URL/form for the Play Console data-deletion field.
### 2. Cloud save and remote data

- [ ] Add packages: `com.unity.services.cloudsave` (and Remote Config or CCD for definitions).
- [ ] `SaveService`: keep the local JSON as a cache/offline copy and add cloud sync (save `SaveGame` as one key, or one key per section).
  - Add `LastModifiedUtc` and/or a revision counter to `SaveGame` → **save version 5** (`SaveGame4` legacy class plus `Migrate4To5`, per the CLAUDE.md convention).
  - Conflict rule on load: newer revision wins. Decide whether to prompt the player when both copies changed (e.g. two devices).
  - Debounce or batch writes to stay within Cloud Save rate limits. Don't sync on every currency tick.
- [ ] First cloud login migrates the existing local save upward.
- [ ] Move talent/research definitions to Remote Config or CCD, keeping the bundled JSON as a fallback. Version the data so an old client doesn't load an incompatible tree.
- [ ] **Cheating / trust:** a local JSON save is trivially editable. Before real-money purchases exist, decide how much matters:
  - Minimum: Embers (premium) are granted only through validated purchases.
  - Better: Economy and/or Cloud Code own premium currency balances and purchase validation, so the client can't change them directly.
- [ ] Update `WaveGeneratorTests` and the other asset-dependent tests if data loading changes.

### 3. Achievements

- [ ] Design the list (maybe 15–30): first win per difficulty, survive X seconds, max a talent tree, finish the tutorial, beat a specific general, spend N Cinders, and so on.
- [ ] Build a platform-agnostic `AchievementService`:
  - Definitions as ScriptableObjects or JSON (ID, title, description, target, platform IDs).
  - Progress stored in the save (`PlayerAchievementState`, which goes in the same v5 bump as cloud save).
  - Hooks on existing events: wave end/game over in `GameManager`/`WaveController`, talent/research purchase, tutorial completion.
  - Platform adapters: GPGS achievements on Android (regenerate `GPGSIds.cs`), Game Center on iOS. Report on unlock and re-sync on login, because platform calls fail offline.
- [ ] Optional in-game achievements screen (new UI Toolkit screen via `UIScreenRouter`) and rewards (Cinders/Embers).
- [ ] Decide whether GPGS leaderboards replace Unity Leaderboards. Recommendation: keep Unity Leaderboards (works on both platforms) and remove the unused GPGS leaderboard.

### 4. Shop (IAP)

- [ ] Play Console: set up a payments profile/merchant account, then create in-app products (e.g. Ember packs, a "remove ads" non-consumable, maybe a starter bundle).
- [ ] Add Unity IAP (`com.unity.purchasing`, v5 API). It covers Google Play Billing now and the App Store later.
- [ ] Shop screen (UI Toolkit screen, plus a `ShopService` for long-lived state).
- [ ] Purchase flow: buy → **validate receipt** (server side through Cloud Code/Economy if possible, local Google/Apple validation at minimum) → grant → save → confirm.
- [ ] Restore purchases for non-consumables (required on iOS) and handle pending or refunded purchases.
- [ ] Decide what Embers buy and check that the economy works without paying.
- [ ] Test with Play Console license testers on the internal track.

### 5. Ads

- [ ] Pick a network. Unity's current option is **LevelPlay** (mediation, replacing the legacy Unity Ads SDK). **AdMob** is the main alternative. Either fits the Google/Unity stack.
- [ ] Placements that suit a tower defense game: **rewarded** ads (double end-of-run Cinders, revive or extra life, daily free Embers). Use interstitials sparingly, if at all. Honor "remove ads".
- [ ] **Consent:** a GDPR/UMP consent form for EEA/UK users (Google requires a certified CMP), plus ATT on iOS later.
- [ ] Play Console: answer the "Contains ads" declaration, and include ad SDK data collection in the Data safety form.
- [ ] Set the target audience to 13+ unless you deliberately want the Families program (which brings much stricter ad rules).

### 6. Google Play production release

The thing you're thinking of is probably **developer account (identity) verification** plus **production access**. Personal developer accounts created after Nov 2023 must:

- [ ] Run a **closed test with at least 12 opted-in testers for 14 consecutive days**. Internal testing doesn't count. Then answer the production access questionnaire. Start this early, because it's the long pole and other work can happen in parallel.
- [ ] Finish developer identity/contact verification if the console still shows it as pending.

The App content section in Play Console needs all of these:

- [ ] Privacy policy URL (hosted page covering UGS, ads and IAP data)
- [ ] App access (explain the login, or provide test credentials)
- [ ] Ads declaration
- [ ] Content rating (IARC questionnaire)
- [ ] Target audience and content
- [ ] Data safety form (account info, game progress, purchase history, ad IDs, crash/diagnostics data)
- [ ] Data deletion URL (from workstream 1)

Store listing and build:

- [ ] Store listing: title, short and full description, 512×512 icon, 1024×500 feature graphic, phone screenshots (tablet optional), category.
- [ ] Build: AAB, Play App Signing enabled, **upload keystore backed up outside the project**. targetSdk must meet the current Play requirement (pin it explicitly instead of "highest installed"). 64-bit (IL2CPP, ARM64).
- [ ] Version hygiene: `bundleVersion` is `"Emberfall v0.5.1"`, and players see it as the Play versionName. Use plain `1.0.0`. Bump versionCode on every upload.
- [ ] Pre-launch report: fix the crashes and ANRs it reports.
- [ ] **16 KB memory page size:** Play requires native code to support it for apps targeting Android 15+. Unity 6 handles the engine, but every native plugin must too (GPGS, ads SDK, IAP). Check the APK Analyzer or the Play Console warning on the next upload.
- [ ] **Target SDK 36 on large screens:** Android ignores orientation locks on screens 600dp and wider (tablets, foldables). Test the game there, or limit supported devices.
- [ ] **Edge-to-edge** is enforced from targetSdk 35, so check the safe area on Android too.
- [ ] Device catalog: exclude devices that are too weak to run the game well.
- [ ] Release mechanics: use a **staged rollout** (e.g. 10% → 50% → 100%) and choose which countries to release in and what to charge. Pre-registration is optional, and it stays locked until production access is granted (like the production and open testing tracks). The earliest you could start it is after approval, around late November, to build an audience before launch.

### 7. Business, legal and policy

- [ ] **EU trader status (Digital Services Act)** declaration in Play Console, and later in App Store Connect. A monetizing developer counts as a trader, and the store page then shows your name, address, email and phone publicly. Decide whether to use a business address or a company first.
- [ ] Payments profile and bank details for each store, plus tax forms (W-8BEN if you're outside the US) to avoid US withholding. Store income is also taxable where you live.
- [ ] Apple Small Business Program: you must enroll to get the 15% fee instead of 30%. Google's 15% on the first $1M a year is automatic.
- [ ] **Support email** in the store listing (required). One simple website can hold the privacy policy, the account deletion page and the support contact.
- [ ] **Loot box odds:** if anything you sell is randomized, both stores require the odds to be shown before purchase.
- [ ] **Asset licenses:** confirm that all art, music, fonts and asset packs (e.g. `CrystalFramework`) allow commercial use. Add open-source attributions (Newtonsoft, GPGS, ad/IAP SDKs) to a credits/licenses screen.
- [ ] Unity license: Personal is fine below $200k revenue a year; above that you need Pro.
- [ ] Name check: search the stores and trademark databases for "Emberfall" before spending on marketing.
- [ ] **Username moderation:** custom names show on public leaderboards, which counts as user-generated content under store rules. Add a profanity/length filter and a way to rename or remove a name.
- [ ] Support and refund process ("I paid and didn't get my Embers"). Keep purchase logs so you can look it up.

### 8. Release readiness (not in your list, worth doing)

- [ ] Crash reporting and analytics: Unity Analytics plus Cloud Diagnostics, or Firebase Crashlytics. You want this before strangers play.
- [ ] Offline and failure handling: no internet at launch, a service outage, or sign-in failure shouldn't hard-block the game. The leaderboard already has a TODO for queueing scores.
- [ ] Settings screen: audio, account (link Google, delete account), privacy/consent, restore purchases, credits/licenses.
- [ ] Remove debug tools from release builds (`Game/Debug`, the `ClearSessionToken` comment in `LoginManager`) and check that Development Build is off.
- [ ] Performance pass on low-end Android (profile captures already exist).
- [ ] **Separate UGS environments** (`development` and `production`), so test saves and test scores don't mix with real players'. Check free-tier quotas and rate limits and set a billing alert.
- [ ] **Android auto-backup:** once cloud save exists, turn it off or exclude the save file so an OS restore can't conflict with the cloud copy.
- [ ] **Use server time** for anything time-gated (daily rewards, login gifts), because device clocks can be changed. Optionally add the Play Integrity API and a sanity check that rejects impossible leaderboard scores.
- [ ] **Minimum version / forced update** via Remote Config, to block old clients after a save-format change.
- [ ] In-app review prompt (Google/Apple review APIs) after a good moment, such as a win.
- [ ] **Upgrade test before every release that changes the save:** install the previous build on a real device, play, update, and check that progress survives.
- [ ] Accessibility basics: text size, colorblind-safe indicators.
- [ ] Store presence: trailer/video, a short description written for search, store listing experiments, and maybe a localized store listing.
- [ ] Optional: localization, haptics.

### 9. iOS (after the Android release)

- [ ] Apple Developer Program ($99/yr) and an App Store Connect app record.
- [ ] Builds need Xcode on a Mac. Without a Mac, use Unity Build Automation or Codemagic.
- [ ] Sign-in: Game Center (`SignInWithAppleGameCenterAsync`) and/or **Sign in with Apple**. Check App Review guideline 4.8 against whatever login options you show. Link to the same UGS account the way Android does.
- [ ] Platform adapters from earlier workstreams: Game Center achievements, App Store IAP (Unity IAP, restore purchases required), ATT prompt plus SKAdNetwork IDs for ads.
- [ ] Set `ITSAppUsesNonExemptEncryption = NO` in Info.plist (the app only uses standard HTTPS) so builds don't get held at the export compliance question.
- [ ] Privacy: App Privacy "nutrition labels", a privacy manifest (`PrivacyInfo.xcprivacy`) for the app and SDKs, and in-app account deletion.
- [ ] Safe area/notch layout (the safe-area package is already in use), TestFlight beta, then App Review.

## Suggested order

```
Phase 0 (now, in parallel)  Start the 12-tester/14-day closed test · privacy policy/support site · Play App content forms · trader status, payments profile, tax forms · asset license check · UGS dev/prod environments
Phase 1  GPGS sign-in + account linking + account deletion
Phase 2  Cloud Save sync (save v5) + remote talent/research data
Phase 3  Achievements (rides on save v5 + GPGS)
Phase 4  IAP shop with receipt validation → ads + consent
Phase 5  Crash reporting, settings, name filter, 16 KB / large-screen checks, polish → apply for production → staged rollout of 1.0 on Android
Phase 6  iOS port
```

The closed test can run on the current build. Push updated builds to the closed track as phases land, so testers exercise sign-in, cloud save and purchases before production.

## Timeline

Target: **Android 1.0 live before Feb 1, 2027**. iOS follows if time allows. Planned at about 4–5 hours a day (~30 h/week), with a two-week break over Christmas.

| Week | Dates | Work | Milestone |
|---|---|---|---|
| 1 | Oct 26 – Nov 1 | Phase 0 admin (testers, closed track, privacy/support site, Play forms, trader/payments/tax, UGS environments). **Crash reporting** (cheap, and it captures testers' crashes from day one). Play Games console setup. | Closed test running |
| 2 | Nov 2 – Nov 8 | Google Play Games sign-in, linking anonymous accounts, account deletion | |
| 3–4 | Nov 9 – Nov 22 | Cloud save (save v5, sync, conflict handling), remote talent/research data | ~Nov 16: **apply for production access** |
| 5 | Nov 23 – Nov 29 | Achievements | |
| 6–7 | Nov 30 – Dec 13 | IAP shop, receipt validation, restore/pending purchases | Payments profile approved |
| 8 | Dec 14 – Dec 20 | Ads + consent form | **Feature complete**, build on closed track |
| | Dec 21 – Jan 3 | Holidays. Testers play the feature-complete build. Enroll in the Apple Developer Program. | |
| 9 | Jan 4 – Jan 10 | Settings screen, name filter, 16 KB and large-screen checks, upgrade test, store listing assets, fixes from tester feedback | **Release candidate** |
| 10 | Jan 11 – Jan 17 | Final fixes, submit to production, staged rollout | **Android 1.0 submitted** (~Jan 15) |
| 11–12 | Jan 18 – Jan 31 | Buffer for review and rollout issues | **Android 1.0 live by Jan 31** |

**Done means** Android 1.0 is live in production before Feb 1. The iOS port starts once Android is live. That will most likely be February, unless Android ships early enough to start it in January.

**If a phase slips**, cut in this order to protect the date: ads → achievements → remote talent/research data. All three can ship in 1.1.

**The date depends on these:**
- Recruiting 12 testers in week 1. Every day the closed test starts late pushes production access back a day.
- The payments profile and merchant account being approved before week 6.
- Access to a Mac or a cloud build service for iOS

## Open questions

- Game data: Remote Config (simple key/value JSON) or CCD remote Addressables (more setup, also handles icons)?
- How server-authoritative should the economy be at launch?
- Keep the custom username prompt, or use the Google/Game Center display name?
- What does Embers buy, and what does a "fair" free-to-play pace look like?
- Should cross-platform progress (Android ⇄ iOS) work at launch? If so, players need a cross-platform login option (e.g. Unity Player Accounts) and not just GPGS/Game Center.