# Ink Run — Unity project

This folder holds the game code for the Unity build. It mirrors the browser prototype in
`../prototype/index.html` one to one: same lanes, moves, obstacles, power-ups, areas,
revive flow, missions, outfits and playbook rules. When the prototype feels right,
this is where the real game is built.

**Status:** scripts compile-checked outside Unity against stand-in Unity APIs
(`Tools/compile-check/check.sh`, both the editor and device build paths), so syntax
and type errors are gone. Not yet run inside Unity, so expect small API fixes on the
first open, and the shaders are unverified. This folder is meant to be moved into its
own repository once the Unity project exists.

## 1. Create the project

1. Install Unity Hub and **Unity 6 (6000.x LTS)**.
2. New project, template **Universal 3D** (URP), name `InkRun`.
3. Copy `Assets/InkRun` from here into the project's `Assets` folder.
4. Window > Package Manager: confirm **TextMeshPro** (part of UGUI) is present. Import TMP Essentials when asked.
5. Project Settings > Player: Default Orientation **Portrait**, Target API as current store minimums.

## 2. Scene setup (one scene: `Run`)

Hierarchy:

```
GameManager            (Core/GameManager, assign Tuning asset, Player, World, Spawner, Areas, Outfits)
Input                  (Player/SwipeInput)
Hero                   (Player/PlayerController, CapsuleCollider isTrigger, Rigidbody kinematic)
  └─ Visual           (model + Animator; outfit parts as children named by outfit id)
World                  (World/WorldMover)
Spawner                (World/TrackSpawner: prefabs, patterns, track piece)
Areas                  (World/AreaController: Area assets, SunDisc, MoonMask)
CameraRig              (FX/CameraShake)
  └─ Main Camera      (behind and above the hero, looking slightly down the track)
Sky                    (a flat disc for the sun, material with Use area accent on)
Canvas (Overlay)
  ├─ SpeedLines        (RawImage, material InkRun/InkSpeedLines, FX/SpeedLines)
  ├─ HUD               (UI/HudView, with a child "Panel" it shows and hides)
  ├─ Start             (UI/StartView + UI/MissionListView, child "Panel")
  ├─ Wardrobe          (UI/WardrobeView; this one toggles itself)
  ├─ Revive            (UI/ReviveView, child "Panel")
  └─ Results           (UI/ResultsView, child "Panel")

The view objects stay active so they keep receiving GameManager events; only their
"Panel" child is toggled.
SfxWords               (FX/SfxWords, world-space TMP prefab with the comic font)
```

Geometry: the hero stays at the origin; `WorldMover` scrolls everything else toward -z.
Lanes are at x = -2, 0, +2 (`Tuning.LaneWidth`). The track is a strip of 10 m pieces.

## 3. Data assets (Assets > Create > Ink Run)

- **Tuning**: one asset. Every number Remote Config can change.
- **Area** × 3: City Rooftops (red `#E0202A`), The Harbor (blue `#1E6FE0`),
  Night Alley (purple `#7B2FE0`, Crescent Moon on). Assign side-prop prefabs.
- **Pattern**: menu **Ink Run > Create default patterns** builds the seven starting patterns.
- **Outfit** × 3: `default` (0), `delivery` (300), `ronin` (800), with preview prefabs.

## 4. Prefabs

| Prefab | Components | Collider |
|---|---|---|
| Barrier | Obstacle (Barrier, ClearHeight 0.5) | Box 1.6 × 0.8 × 0.4 at y 0.4 |
| Sign | Obstacle (Sign) | Box 1.6 × 0.9 × 0.3 at y 1.6 (over a sliding hero) |
| Train | Obstacle (Wall) | Box 1.9 × 2.6 × 6 |
| Gap | Obstacle (Gap) | Box 1.8 × 0.3 × 3 at y 0.1 |
| Coin | Pickup (Coin) | Sphere 0.45 |
| Magnet, Dash | Pickup | Sphere 0.6 |
| TrackPiece | plain mesh, 10 m deep | none |

All colliders are triggers. The hero's capsule is the only thing that detects them.

## 5. Look

- Material **InkPaper** (InkRun/InkToon, paper white) for most props.
- Material **InkAccent** (InkRun/InkToon, Use area accent on) for the hoodie, coins, sun,
  sign kanji, barrier stripes, container stripes and lanterns.
- One directional light, slightly to the side, so the halftone shadow lands on one face.
- Comic font: Bangers (SIL OFL) for TMP display text; Patrick Hand for body text.

## 6. SDKs (after the loop plays)

| Service | Package | Define symbol | Where |
|---|---|---|---|
| Ads | AppLovin MAX Unity plugin | `APPLOVIN_MAX` | `Services/MaxAdService.cs` |
| Analytics, Remote Config, Crashlytics | Firebase Unity SDK | `FIREBASE` | `Services/FirebaseRemoteConfigService.cs` |
| Purchases | Unity IAP | implement `IPurchases` | `Services/PlayerGroups.cs` |

Without the defines the editor uses fakes: a 3 s fake ad, logged analytics, local tuning.

Remote Config keys: `base_speed`, `max_speed_bonus`, `ramp_distance`, `pattern_gap_start`,
`pattern_gap_min`, `magnet_duration`, `dash_duration`, `revive_countdown`, `revives_per_run`,
`no_interstitials_first_sessions`, `max_interstitials_per_hour`.

## 7. Playbook mapping

| Playbook | Code |
|---|---|
| Second Wind revive | `GameManager.AfterCrash` → `ReviveView` → `RequestRevive` |
| Save the haul | `GameManager.RequestDoubleCoins` from `ResultsView` |
| Comeback help | `SaveData.ShouldAssist` + `Tuning.Assist*` |
| Almost a record | `GameManager.NearRecord` → `HudView` bubble |
| Player groups | `Services/PlayerGroups.cs` (starter bundle, ad-free offer) |
| Server-side tuning | `Services/FirebaseRemoteConfigService.cs` → `Tuning` |
| Smart ad pacing | `Services/AdPacing.cs`, called from `GameManager.BackToStart` |

## 8. Names

- Game: **Ink Run**, Volume 1.
- Hero: **Nozomi** (望, "hope"; also the fastest Shinkansen, which the train obstacle
  winks at). Used in the revive page copy (`Core/Story.cs`) and the store listing.
- Chasers: the **rival gang** on the ground and the **kaiju** in the sky. Volume 1 shows
  the kaiju only as a shadow on the revive screen; the gang appears in chapter transitions.
