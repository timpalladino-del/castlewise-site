# Ink Run — design doc, draft 1

Working title. A manga-style three-lane endless runner for iPhone and Android.
Black ink on white paper, screentone shadows, one accent color per area, speed
lines on boosts, sound-effect words on actions, and a crash screen that is a
comic cliffhanger with a revive button.

## 1. Pillars

1. **It looks like a printed manga page in motion.** No gradients, no soft
   shading. Ink, halftone dots, one accent color. Every screen could be a page.
2. **Thirty-second runs, one-thumb controls.** Swipe left, right, up, down.
   Nothing else to learn.
3. **The story is the retention hook.** Each season is a volume. Crashing is a
   cliffhanger, not a game over.

## 2. Core loop

Run → dodge and collect → crash → "To Be Continued...?" → revive (ad) or end →
results, coins banked → spend on outfits, chase missions → run again.

### Controls
| Input | Action |
|---|---|
| Swipe left / right | Change lane |
| Swipe up | Jump (clears barriers and gaps) |
| Swipe down | Slide (clears overhead signs); while airborne, drops fast |

### Obstacles (first area)
| Obstacle | Beat it by | Notes |
|---|---|---|
| Barrier | Jump | Low striped road barrier |
| Sign | Slide | Overhead board, kanji in accent color |
| Train / wall | Change lane | Full height, never passable |
| Gap | Jump | Missing rooftop, drawn as black ink hole |

Patterns always leave at least one survivable lane. Difficulty ramps by speed
(about 22 m/s to 41 m/s over the first 2.5 km) and by shorter gaps between
patterns.

### Pickups
- **Coins**: on the ground and in arcs over barrier rows.
- **Coin magnet** (8 s): pulls coins from all lanes.
- **Speed dash** (4 s): faster, invincible, smashes obstacles with a WHAM!

## 3. World

| Chapter | Area | Accent | Set dressing |
|---|---|---|---|
| 1 | City Rooftops | Red | Water tanks, antennas, billboards, red sun |
| 2 | The Harbor | Blue | Containers, cranes, wet docks |
| 3 | Night Alley | Purple | Lanterns, vending machines, crescent moon |

Areas rotate every 1,200 m. The accent color cross-fades over about a second
and a CHAPTER card pops. Launch ships with City Rooftops fully dressed; Harbor
and Night Alley can start as palette swaps and get real props after launch.

### Hero
Street kid, spiky hair, hoodie (accent color), goggles on forehead, high-tops.
Three outfits at launch: Default, Delivery, Ronin. The chaser is not shown at
launch except as a shadow on the revive screen. Decide later whether it is a
rival gang, a kaiju, or both (the key art shows a kaiju).

## 4. Meta and retention

- **Daily missions**: three a day picked from a pool (jump N times, collect N
  coins, slide N times, run N m in total, reach The Harbor, smash N obstacles
  with a dash, revive once). Progress counts across runs and resets at local
  midnight. Rewards are coins, paid on the results screen.
- **Outfits**: Default (free), Delivery (300 coins), Ronin (800 coins), bought in
  the Wardrobe with banked coins. Cosmetic only.
- **Record chase**: a speech bubble appears when within 200 m of the best run.
- **Comeback help**: after three runs under 20 s, the next run starts slower with
  fewer obstacles and shows a small ASSIST tag. Clears after a run over 40 s.

## 5. Monetization (from the playbook)

| Playbook feature | In Ink Run | Prototype status |
|---|---|---|
| Second Wind revive | "To Be Continued...?" screen, one revive per run, 5 s countdown | Built (ad is a 3 s placeholder) |
| Save the haul | "2× coins" button on results after an ad | Built (placeholder) |
| Comeback help | Assist mode after three quick crashes | Built |
| Almost a record | "200m to your record!" bubble | Built |
| Player groups | Starter bundle for new players; ad-free offer for frequent ad watchers | Not yet |
| Server-side tuning | Speed curve, spawn gaps, power-up odds, prices via Remote Config | Not yet |
| Smart ad pacing | Interstitials only between runs, none in the first 3 sessions, capped per hour | Not yet |

Prices are the same for everyone. We test bundles and discounts, never hidden
price differences.

## 6. Technical plan

- **Engine**: Unity 6 LTS, C#, Universal Render Pipeline.
- **Look**: a toon shader with a two-step ramp, screen-space halftone for the
  dark step, inverted-hull outlines. Accent color is a single global shader
  parameter per area, so recoloring an area is free.
- **Ads**: AppLovin MAX (rewarded for revive and haul, interstitial between runs).
- **Backend**: Firebase Remote Config for tuning, Analytics for the funnel,
  Crashlytics.
- **Content**: obstacles and patterns as ScriptableObjects so new patterns need
  no code.
- **Fallback**: if we stay iPhone-only, RealityKit can do the 3D, but the toon
  look and ad SDKs take more custom work.

## 7. First version scope

1. Three lanes, four swipes
2. City Rooftops with four obstacle types
3. Coins, magnet, dash
4. One hero, three outfits
5. Daily missions
6. Revive screen, haul screen, ad pacing, remote config

Seasons, more areas, the chaser reveal, and story panels between chapters come
after launch.

## 8. What the browser prototype proves

`prototype/index.html` is a canvas prototype of the full loop: lanes, swipes,
obstacles, coins, both power-ups, three areas with accent changes, speed lines,
sound-effect words, the revive screen, the haul button, assist mode, the
near-record bubble, daily missions, the wardrobe with three outfits, and small
synthesized sound effects. Everything from the first-version list is in it
except the real ad SDK, player groups, remote config, and ad pacing. It is for
feel and look only. It is not the shipping code.

## 9. Open decisions

1. Final name ("Ink Run" is a placeholder).
2. Hero's name and the chaser.
3. Unity for both stores vs RealityKit for iPhone only.
