# Ink Run — Unity project

This folder holds the game code for the Unity build. It mirrors the browser prototype in
`../prototype/index.html` one to one: same lanes, moves, obstacles, power-ups, areas,
revive flow, missions, outfits and playbook rules. When the prototype feels right,
this is where the real game is built.

**Status:** runs in Unity 6 (6000.3) on the Mac. The gray-box scene builds from the menu, the
loop plays end to end (title, run, crash, revive, results, missions, wardrobe), and the scripts are
compile-checked outside Unity by `Tools/compile-check/check.sh` (Mono if installed, otherwise the
Roslyn compiler bundled inside any installed Unity 6 Editor, so it needs no extra install on a Mac).
15 pure-logic tests pass. This folder is meant to be moved into its own repository once the
project is stable.

## 1. Create the project

1. Install Unity Hub and **Unity 6 (6000.x LTS)**.
2. New project, template **Universal 3D** (URP), name `InkRun`.
3. Copy `Assets/InkRun` from here into the project's `Assets` folder.
4. Window > Package Manager: confirm **TextMeshPro** (part of UGUI) is present. Import TMP Essentials when asked.
5. The models are `.glb` files and need the **glTFast** package (`com.unity.cloud.gltfast`). `Scripts/Editor/PackageEnsure.cs` adds it on its own the first time the editor loads the scripts (watch the Console); if that fails, add it from Package Manager > Unity Registry > glTFast, or menu **Ink Run > Install glTF importer**. Without it every model falls back to primitives and the Console says "no imported model for X".
6. Project Settings > Player: Default Orientation **Portrait**, Target API as current store minimums.

## 2. Build the scene with one click

Menu **Ink Run > Build gray-box scene** (`Scripts/Editor/SceneBootstrap.cs`) creates
everything below from code: materials, the Tuning, Area, Outfit and Pattern assets,
prefabs made of primitives, the `Run` scene with every manager wired, and the whole
UI. Then press Play. Swipe or use the arrow keys. Safe to re-run; it overwrites in place.

Tests: Window > General > Test Runner > EditMode runs `Tests/EditMode`.

Game view: set Scale to 1x and pick a portrait size (dropdown next to Free Aspect, press +,
1080 x 1920, Fixed Resolution). The UI is laid out for portrait phones.

After pulling new commits, rerun the menu only when prefabs or the scene changed; script
changes recompile on their own.

### Driving the Editor from a terminal

The project has Unity's `com.unity.pipeline` package (`unity pipeline install`), so the
[Unity CLI](https://unity.com/install.sh) can drive the open Editor without touching the mouse.
The loop used for development:

```bash
unity status                                                   # "ready" once the Editor is focused and compiled
unity command editor invoke_menu --path "Ink Run/Build gray-box scene"
unity command editor set_auto_tick --enable true               # keep ticking while unfocused
unity command editor.playmode set_run_in_background --enable true
unity command editor.playmode start
unity command capture game_view --width 1080 --height 1920 --save_path Temp/shot.png
unity command console get --level warn --tail 20
unity command script eval --code 'new UnityEngine.GameObject("AutoPilot").AddComponent<InkRun.DebugTools.AutoPilot>();'
```

`runtime simulate_key` does not reach the game (it flushes the Input System before the player
loop reads it). To press a key, queue a `KeyboardState` through `script eval` instead:
`InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(Key.UpArrow)); InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState());`

### AutoPilot (debug)

`Scripts/Debug/AutoPilot.cs` plays the game by itself through the real swipe path
(`SwipeInput.Inject`): jumps barriers and gaps, slides under signs, changes lane for trains,
drifts toward coins, taps through the title, revive and results pages. `MistakeChance` makes it
crash on purpose so the revive and results flows get exercised. Add it to any object at runtime,
destroy it to stop. Not part of the shipped game.

Input: works with either backend. Unity 6 templates use the Input System package; the
scripts detect it (`ENABLE_INPUT_SYSTEM`) and fall back to the legacy Input class otherwise.
If the console warns that the `Unity.InputSystem` reference in `InkRun.asmdef` is missing,
either install the Input System package or remove that line from the asmdef.

The rest of this section describes what the menu builds, so you can rebuild or change
it by hand.

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

- **Tuning**: one asset. Every number Remote Config can change. The menu creates it once and
  never overwrites it, so after changing a default in `Core/Tuning.cs` delete
  `Data/Tuning.asset` and rerun the menu (or edit the asset by hand).
- **Area** × 3: City Rooftops (red `#E0202A`), The Harbor (blue `#1E6FE0`),
  Night Alley (purple `#7B2FE0`, Crescent Moon on). Assign side-prop prefabs.
- **Pattern**: menu **Ink Run > Create default patterns** builds the seven starting patterns.
- **Outfit** × 3: `default` (0), `delivery` (300), `ronin` (800), with preview prefabs.

## 3b. Models

`Assets/InkRun/Models/*.glb` are low-poly toon models built by `Tools/blender/build_all.py` (Blender headless via `pip install bpy`; see `Tools/blender/README.md`)
(hero with joint nodes for the procedural run cycle, train, barrier, sign, coin, magnet,
dash, city block, container stack, alley building, lantern). Install the **glTFast**
package (Window > Package Manager > Add by name > `com.unity.cloud.gltfast`) so Unity
imports them; the scene builder then uses them instead of primitives and swaps their
materials for the ink shader. Without glTFast it falls back to primitives and says so.
Edit the Python, rerun it, and rerun the menu to update models.

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
