# Emberfall release plan

Status: in progress, as of v0.5.4 (code 13) on the closed testing track. Week 1 admin and crash reporting are done; Google sign-in coding starts Nov 2.

## Where things stand

| Area | Current state in the repo |
|---|---|
| Login | `LoginManager` signs in **anonymously** with Unity Authentication, then asks the player to type a name. `UnityAuthIdentity` returns `"offline"` when not signed in. |
| Google Play Games | Plugin v2.1.0 is imported (`Assets/GooglePlayGames`, manifest androidlib, `GPGSIds.cs` with one leaderboard ID), but `PlayGamesManager` is commented out, so it isn't used. The console side (game project, credentials, UGS provider, testers) is set up. |
| Save | `SaveService` writes `{playerId}_save.json` to `persistentDataPath` (local only, save version 4). |
| Talents / research data | `Talents.json` / `Research.json` are bundled Addressables. |
| Leaderboards | Unity Leaderboards (`High_Scores_<Difficulty>`). The GPGS leaderboard in `GPGSIds` isn't used. |
| Currencies | `Cinders`, `Embers` live in the local save. Nothing is validated on a server. |
| Achievements / shop / ads | None. |
| Crash reporting | Unity Diagnostics (since v0.5.4). |
| Android build | v0.5.4 / versionCode 13 on the closed track, minSdk 25, targetSdk = "highest installed". |
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

- [x] Play Console → Play Games Services: create or confirm the game project and link it to the app. Add OAuth credentials: an Android client for **both** the upload key SHA-1 and the Play App Signing SHA-1, plus a **web client** for the server auth code.
  - Done (Oct 28): Play Games project `emberfall-dev` (player-facing name Emberfall) created and linked. Three credentials: Android with the Play App Signing SHA-1, Android with the upload key SHA-1, and a Game server (web) client. You and the closed track are added under Testers. OAuth consent screen is "In production" with app name Emberfall (scopes `games`, `games_lite`, `drive.appdata`, no logo). Sideloaded debug-keystore builds need a fourth Android credential with that SHA-1. Publish the project only right before the sign-in build ships.
- [x] Unity Dashboard → Authentication → add the Google Play Games identity provider (web client ID and secret).
  - Done (Oct 28). Authentication isn't environment-scoped, so the provider and player accounts are shared by `development` and `production`. Use a throwaway account when testing account deletion. Leaderboards are recreated in `development`.
- [ ] Optional, before launch: submit **branding verification** for the OAuth consent screen (Google Cloud → Google Auth Platform → Branding; the console says the branding must be verified before it is shown to users). It needs a homepage URL, the privacy policy link, a support email and an authorized domain verified in Search Console, and can take days or weeks. Scope verification is not needed (the scopes are non-sensitive). Check on a real device with a non-tester account whether a consent prompt appears at all.
- [ ] Wire up GPGS v2: `PlayGamesPlatform.Activate()`. v2 signs in automatically at launch. Then `RequestServerSideAccess` → `AuthenticationService.SignInWithGooglePlayGamesAsync(authCode)`.
- [ ] **Account linking:** existing internal testers have anonymous accounts. Use `LinkWithGooglePlayGamesAsync` on the anonymous session so progress (and leaderboard entries) carry over. Handle the "already linked to another account" error.
  - Decided (Oct 29): sign-in is the player's choice. The Login screen offers "Sign in with Play Games" or "Continue as guest" on first launch (more providers later), and cached sessions are restored silently. So guests aren't linked automatically: they link from the Account popup (tap the username → "Link Play Games", Android only; code in Oct 29, awaiting a device test). On "already linked" the player stays on the device account, because there's no cloud save yet. The guest option may be dropped later; revisit with cloud save. Android uses Play Games, not regular Google sign-in (auto sign-in, needed for achievements); pick a cross-platform login (Google, Apple, Unity Player Accounts) with iOS.
- [ ] Keep anonymous sign-in as the fallback if GPGS sign-in fails or is declined. Offer "Sign in with Google" later from settings.
- [ ] Username: default to the GPGS display name, or keep the custom name prompt (and its validation).
  - Decided (Oct 29): keep the prompt; for new Play Games accounts it's prefilled with the Play Games name (whitespace removed, cut to 12 characters). Awaiting a device test.
  - Names are checked by `ProfanityFilter` against `Assets/Data/ProfanityWords.txt` (curated LDNOOBW list plus an allowlist).
- [ ] Replace the `"offline"` player ID in `UnityAuthIdentity`. Offline play needs a defined behavior (play locally with a cached ID, then sync later).
  - Code in (Oct 29), awaiting a device test: if restoring a saved session fails, `LoginManager` plays offline with the last player ID and name cached in PlayerPrefs (`UnityAuthIdentity`); leaderboards are unavailable and sign-in is retried on the next launch. A first launch still needs a connection. Scores from offline matches are still dropped (see "Offline and failure handling" under Release readiness).
- [ ] **Account deletion** (required by both Google Play and Apple when accounts exist): an in-app "delete account" option that calls `DeleteAccountAsync` and clears Cloud Save, plus a web URL/form for the Play Console data-deletion field.
  - Done in code (Oct 29), tested in the Editor: tap the username in the top bar → Account popup → Delete account → confirm. `AccountService` deletes the UGS account, the local save and backups, the cached ID/name and the session, then restarts from Bootstrap. Leaderboard entries stay behind (see the next item). Cloud Save clearing comes with workstream 2.
- [ ] **Delete the player's leaderboard scores on account deletion (Cloud Code).** `DeleteAccountAsync` only deletes the Authentication account; Unity's docs say the game must delete the player's data in other UGS services. The Leaderboards client SDK can't delete scores, only the Admin API can (`DeleteLeaderboardPlayerScoreAllLiveLeaderboards`), and that needs server credentials. Do this before the sign-in build ships, since the deletion page and Data safety form will say the data is removed.
  - Add `com.unity.services.cloudcode` and one Cloud Code function (JS script or C# module) that deletes the calling player's scores from all live leaderboards.
  - Give it Admin API access (probably a service account key in Secret Manager; confirm whether Cloud Code can call the Admin API without one) and deploy it.
  - `AccountService`: call it before `DeleteAccountAsync`, while still signed in; if it fails, don't delete the account.
  - Workstream 2: extend the same function to delete the player's Cloud Save items (Cloud Save Admin API). Analytics deletion is a separate request (different user ID).
  - Rough effort: half a day to a day, mostly first-time Cloud Code and service account setup.
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
  - Progress (Oct 28): closed track submitted and approved; 8 testers confirmed, still recruiting. The 14-day clock starts when 12 are opted in (write the date here). Aim for 15–20 testers as a buffer.
- [ ] Finish developer identity/contact verification if the console still shows it as pending.

The App content section in Play Console needs all of these:

- [x] Privacy policy URL (hosted page covering UGS, ads and IAP data). Live and covers UGS only; update it with crash reporting, Google sign-in, cloud save, IAP and ads as they ship.
- [x] App access (explain the login, or provide test credentials). Answered for the current build (anonymous sign-in, nothing restricted); re-check when Google sign-in, IAP and ads ship.
- [x] Ads declaration. "No ads" for the current build; redo in the ads release together with the Data safety form and advertising ID answer.
- [x] Content rating (IARC questionnaire)
- [x] Target audience and content
- [x] Data safety form (account info, game progress, purchase history, ad IDs, crash/diagnostics data). Answered for v0.5.4 (UGS data plus crash logs, diagnostics and the installation ID); update it as features ship, per [Re-check when features ship](#re-check-when-features-ship).
- [x] Data deletion URL (from workstream 1). Filled in, pointing to the support site's `delete-account.html`; update that page when Google sign-in and cloud save ship.

Store listing and build:

- [ ] Store listing: title, short and full description, 512×512 icon, 1024×500 feature graphic, phone screenshots (tablet optional), category.
- [ ] Build: AAB, Play App Signing enabled, **upload keystore backed up outside the project**. targetSdk must meet the current Play requirement (pin it explicitly instead of "highest installed"). 64-bit (IL2CPP, ARM64).
- [x] Version hygiene: `bundleVersion` is plain `MAJOR.MINOR.PATCH` (players see it as the Play versionName), and versionCode goes up on every upload. See [Releasing](#releasing).
- [ ] Pre-launch report: fix the crashes and ANRs it reports.
- [ ] **16 KB memory page size:** Play requires native code to support it for apps targeting Android 15+. Unity 6 handles the engine, but every native plugin must too (GPGS, ads SDK, IAP). Check the APK Analyzer or the Play Console warning on the next upload.
- [ ] **Target SDK 36 on large screens:** Android ignores orientation locks on screens 600dp and wider (tablets, foldables). Test the game there, or limit supported devices.
- [ ] **Edge-to-edge** is enforced from targetSdk 35, so check the safe area on Android too.
- [ ] Device catalog: exclude devices that are too weak to run the game well.
- [ ] Release mechanics: use a **staged rollout** (e.g. 10% → 50% → 100%) and choose which countries to release in and what to charge. Pre-registration is optional, and it stays locked until production access is granted (like the production and open testing tracks). The earliest you could start it is after approval, around late November, to build an audience before launch.

### 7. Business, legal and policy

- [ ] **EU trader status (Digital Services Act)** declaration in Play Console, and later in App Store Connect. A monetizing developer counts as a trader, and the store page then shows your name, address, email and phone publicly. Decide whether to use a business address or a company first.
  - Progress (Oct 28): the console shows no trader field, banner or alert. Re-check Account details when IAP or ads are added (by Week 6) and before production.
- [ ] Payments profile and bank details for each store, plus tax forms (W-8BEN if you're outside the US) to avoid US withholding. Store income is also taxable where you live.
  - Progress (Oct 28): payments profile created and bank details submitted, awaiting verification. W-8BEN still to do once the console offers it. Must be approved before Week 6 (Nov 30). Open question for SKAT or an accountant: CVR number, VAT and how Play payouts are reported (agreement 3.5 makes the developer responsible for transaction taxes in some countries).
- [ ] Apple Small Business Program: you must enroll to get the 15% fee instead of 30%. Google's 15% on the first $1M a year is automatic.
- [x] **Support email** in the store listing (required). One simple website can hold the privacy policy, the account deletion page and the support contact.
- [ ] **Loot box odds:** if anything you sell is randomized, both stores require the odds to be shown before purchase.
- [ ] **Asset licenses:** confirm that all art, music, fonts and asset packs (e.g. `CrystalFramework`) allow commercial use. Add open-source attributions (Newtonsoft, GPGS, ad/IAP SDKs, and the LDNOOBW word list behind the name filter, CC BY 4.0) to a credits/licenses screen.
- [ ] Unity license: Personal is fine below $200k revenue a year; above that you need Pro.
- [ ] Name check: search the stores and trademark databases for "Emberfall" before spending on marketing.
- [ ] **Username moderation:** custom names show on public leaderboards, which counts as user-generated content under store rules. Add a profanity/length filter and a way to rename or remove a name.
- [ ] Support and refund process ("I paid and didn't get my Embers"). Keep purchase logs so you can look it up.

### 8. Release readiness (not in your list, worth doing)

- [x] Crash reporting: Unity Diagnostics, shipped in v0.5.4 (code 13). Sessions, managed exceptions and native crashes are reported, and native stack traces are symbolicated once the build's symbols are uploaded (see Releasing). There's no in-game opt-out yet. That belongs on the settings screen below. Note: a report's scene field is the scene open when the report is sent on the next launch, not where the crash happened.
- [ ] Analytics: Unity Analytics or similar, still to decide.
- [ ] Offline and failure handling: no internet at launch, a service outage, or sign-in failure shouldn't hard-block the game. The leaderboard already has a TODO for queueing scores.
- [ ] Settings screen: audio, account (link Google, delete account), privacy/consent, restore purchases, credits/licenses.
- [ ] Remove debug tools from release builds (`Game/Debug`, the `ClearSessionToken` comment in `LoginManager`) and check that Development Build is off. `CrashReportingTester` (on GameManager in `Game.unity`) is already safe: release builds compile only a stub that destroys itself.
- [ ] Performance pass on low-end Android (profile captures already exist).
- [x] **Separate UGS environments** (`development` and `production`), so test saves and test scores don't mix with real players'. Check free-tier quotas and rate limits and set a billing alert. Created Oct 28: `production` (the original, holds the existing leaderboards and accounts) and `development` (empty; recreate leaderboards there before using it).
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

## Re-check when features ship

Store declarations and the privacy policy must match the build on the track. When a feature below lands in a build, update everything listed for it in the same release. The daily briefing should remind about these when the matching work starts or ships.

**Crash reporting** (Week 1)
- [x] Data safety form: crash logs and diagnostics, device or other IDs the tool uses (done for v0.5.4)
- [x] Privacy policy: name the crash tool and the data it receives, update the "Last updated" date (October 28, 2026: Unity Diagnostics, installation ID, 90-day retention, no opt-out yet)

**Google Play Games sign-in, cloud save, account deletion** (Weeks 2–4)
- [ ] Publish the Play Games project right before the sign-in build ships (add testers under Testers until then)
- [ ] App access declaration: re-check (anonymous fallback should keep it unrestricted)
- [ ] Data safety form: email or Google account info, game progress stored on the server
- [ ] Privacy policy: Google sign-in, cloud save (progress is no longer device-only), in-app account deletion; update the section 6 deletion wording
- [ ] Data deletion URL in Play Console

**Achievements** (Week 5)
- [ ] Data safety form and privacy policy: achievement progress

**IAP** (Weeks 6–7)
- [ ] Trader status and public address: re-check Account details, make sure the payments profile is approved
- [ ] Data safety form: purchase history
- [ ] Privacy policy: purchases and payment processing by Google
- [ ] App access declaration: re-check
- [ ] Tax category per product (agreement 3.5 makes the developer responsible)
- [ ] Credit card statement name in the payments profile

**Ads and consent** (Week 8)
- [ ] "Contains ads" declaration in Play Console
- [ ] Advertising ID answer, and check the merged manifest for `AD_ID`
- [ ] Data safety form: device or other IDs and the ad SDK's data collection
- [ ] Privacy policy: remove "we don't show ads" and "no advertising IDs", describe the ad provider and the consent form
- [ ] Target audience stays 13+ (avoid the Families program)

**Renaming the game**
- [ ] OAuth consent screen app name (must match the Play Games project name), store listing, credit card statement name, privacy policy

## Releasing

Releases are tags on `master`; there are no release branches.

**Versions**
- Version name (`bundleVersion` in Player Settings): `MAJOR.MINOR.PATCH`. Bump PATCH for fixes, MINOR for features.
- Version code (`AndroidBundleVersionCode`): +1 for every upload to Play, on any track (internal testing included). Play rejects a code that has been uploaded before.
- `gameVersion` in `Talents.json` and `Research.json` matches the version name. Bump `definitionsVersion` in a file whenever its content changes.

**Steps**
1. Commit the version bump on `master`: version name, version code, and the JSON versions.
2. Build Addressables (Window → Asset Management → Addressables → Groups → Build → New Build → Default Build Script). They are not built with the player, so skipping this ships the old data.
3. Run the EditMode tests.
4. Build the AAB from that commit and upload it.
   - Build Profiles → Android: Development Build off (the `CrashReportingTester` trigger only exists in development builds), Debug Symbols Level `Full`, Format `Zip` and `Include In Bundle`.
   - `Include In Bundle` gives Play the symbols for Android vitals crashes and ANRs. If Play still warns that debug symbols are missing (bundle over `androidSymbolsSizeThreshold`, 800 MB), upload the `*.symbols.zip` under App bundle explorer → the version code → Downloads → Native debug symbols.
   - Upload the same `*.symbols.zip` to Unity Dashboard → Diagnostics → Debugging Symbols. Use that zip, not the `.so` files under `Library/`, which are stripped. Each build has its own symbols, so upload them for every build you ship.
5. Tag the commit you built and push the tag:
   ```
   git tag -a v0.5.2 -m "v0.5.2 (code 11)"
   git push origin v0.5.2
   ```
6. Optional: create a GitHub Release from the tag with the release notes (reuse them for Play's "What's new").

If you rebuild after a fix, either move the tag before pushing it or make the fix a new version. Only tag what was uploaded.

**Hotfixes**
If `master` has unreleased work when a released version needs a fix, branch from the tag, fix there, release, and merge back:
```
git switch -c hotfix/0.5.3 v0.5.2
# fix, bump to 0.5.3 / next code, build, upload, tag v0.5.3
git switch master && git merge hotfix/0.5.3
```
When `master` has nothing unreleased, fix on `master` instead.

`git log v0.5.1..v0.5.2` lists what changed between two releases, and `git checkout v0.5.2` gives you the exact code players are running.

## Open questions

- Game data: Remote Config (simple key/value JSON) or CCD remote Addressables (more setup, also handles icons)?
- How server-authoritative should the economy be at launch?
- Keep the custom username prompt, or use the Google/Game Center display name?
- What does Embers buy, and what does a "fair" free-to-play pace look like?
- Should cross-platform progress (Android ⇄ iOS) work at launch? If so, players need a cross-platform login option (e.g. Unity Player Accounts) and not just GPGS/Game Center.