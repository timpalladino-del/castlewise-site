# Ink Run — services setup

What to create, in what order, and where each key goes. Nothing here is code; the code
already reads these values from the places named. Do this after the gray-box loop is fun.

## 1. Firebase (analytics, remote config, crashlytics)

1. console.firebase.google.com → Add project "Ink Run". Analytics on.
2. Add an iOS app (bundle id, e.g. `io.kismetstudio.inkrun`) and an Android app (same
   package name). Download `GoogleService-Info.plist` and `google-services.json`.
3. Download the Firebase Unity SDK. Import `FirebaseAnalytics.unitypackage`,
   `FirebaseRemoteConfig.unitypackage`, `FirebaseCrashlytics.unitypackage`.
4. Put the two config files in `Assets/` (root). The SDK picks them up by name.
5. Project Settings → Player → Scripting Define Symbols → add `FIREBASE`.
6. In the Firebase console → Remote Config → add these parameters with the defaults from
   the Tuning asset so a fetch changes nothing until you edit a value:

   | Key | Default | Tuning field |
   |---|---|---|
   | base_speed | 12 | BaseSpeed |
   | max_speed_bonus | 12 | MaxSpeedBonus |
   | ramp_distance | 2600 | RampDistance |
   | pattern_gap_start | 22 | PatternGapStart |
   | pattern_gap_min | 13 | PatternGapMin |
   | magnet_duration | 8 | MagnetDuration |
   | dash_duration | 4 | DashDuration |
   | revive_countdown | 5 | ReviveCountdownSeconds |
   | revives_per_run | 1 | RevivesPerRun |
   | no_interstitials_first_sessions | 3 | NoInterstitialsForFirstSessions |
   | max_interstitials_per_hour | 4 | MaxInterstitialsPerHour |

7. Experiments: use Remote Config A/B tests. Every player in a group gets the same values.
   Never vary prices per player.

Events the game already logs: `session_start`, `run_start`, `crash` (distance, kind),
`revive`, `run_end` (distance, coins, record), `outfit_bought`, `interstitial_shown`,
`starter_bought`, `adfree_bought`, `remote_config_applied`.

## 2. AppLovin MAX (ads)

1. dash.applovin.com → create the app for iOS and Android. Copy the SDK key.
2. Create ad units: Rewarded "revive_haul" and Interstitial "between_runs" for each platform.
3. Download the MAX Unity plugin, import it, run the Integration Manager to add the
   networks you want (start with AppLovin, Google AdMob, Meta, Unity Ads).
4. Scripting Define Symbols → add `APPLOVIN_MAX`.
5. Fill `SdkKey`, `RewardedUnitId`, `InterstitialUnitId` in `Services/MaxAdService.cs`,
   per platform with `#if UNITY_IOS` / `#if UNITY_ANDROID`.
6. iOS: add `NSUserTrackingUsageDescription` and show the ATT prompt once, after the
   first run ends, not at launch. The MAX plugin has a helper for this.
7. Test with MAX's test mode on before any live traffic.

Placements the game uses: rewarded `revive`, rewarded `haul`, interstitial between runs
(gated by `Services/AdPacing.cs`).

## 3. Unity IAP (starter bundle, ad-free)

1. Project Settings → Services → In-App Purchasing → On. Install the package.
2. Products: `inkrun.starter` (consumable, 1,500 coins + Delivery outfit) and
   `inkrun.adfree` (non-consumable). Same price in every store.
3. Create both in App Store Connect and Google Play Console with matching ids.
4. Replace `EditorPurchases` in `Services/Services.cs` with an `IPurchases` built on
   Unity IAP. Restore purchases on launch for `inkrun.adfree`.

## 4. Store accounts

- Apple Developer Program, App Store Connect: app record, bundle id, privacy labels.
- Google Play Console: app, package name, Data safety form.
- Both need the privacy page URL from `STORE.md`.

## 5. Order of operations

1. Gray-box loop is fun (now).
2. Real art in, music and final sounds.
3. Firebase in, play for a week with analytics only.
4. MAX in with test mode, verify revive and haul flows on device.
5. IAP in, sandbox purchase on both stores.
6. Soft launch in one small country, tune with Remote Config, then launch.
