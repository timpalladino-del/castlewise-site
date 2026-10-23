# Ink Run — concept art, round 1

Manga-style endless runner. Black ink on white paper, screentone shading,
one accent color per area (red city, blue harbor, purple night), speed lines
and sound-effect words. Crashing opens a comic-panel "To Be Continued...?"
revive screen.

## Files

- `DESIGN.md`: design doc, draft 1
- `prototype/index.html`: playable browser prototype of the full loop (open it in a phone browser or desktop; swipe or use arrow keys)
- `unity/`: the Unity project code (C# scripts, ink toon shader, setup guide). Not compiled yet; see `unity/README.md`

## Round 1 images

Six images were generated on 2026-10-22. Their hosted links expire after about
two days, so they should be downloaded into this folder with the names below.
The cloud session could not save them because its network policy blocks the
image host (`dnznrvs05pmza.cloudfront.net`).

| File | What it shows |
|---|---|
| `01-character-sheet.png` | Hero turnaround: front, side, run pose, expressions |
| `02-gameplay-rooftops.png` | In-game view, three lanes on city rooftops, HUD |
| `03-revive-screen.png` | "To Be Continued...?" crash screen with the revive button |
| `04-areas-board.png` | City (red), Harbor (blue), Night Alley (purple) |
| `05-key-art.png` | App-store key art: hero leaping, kaiju behind, "INK RUN" title |
| `06-pickups-outfits.png` | Coins, magnet, dash, shield, four obstacles, three outfits |

## Style rules the art follows

- Pure black ink line work on white. No gray fills; shadows are halftone dots.
- Exactly one accent color on screen at a time. It changes per area.
- Speed lines radiate from the vanishing point during a dash.
- Sound-effect words (DASH!, WHAM!, ドン!) pop on jumps, dashes, and crashes.
- UI text is hand-lettered comic style; numbers use a heavier weight.

## Prompts used (for regenerating or iterating)

### 01 Character sheet (16:9)
Character design sheet for a mobile game hero: a scrappy teenage street kid runner, spiky messy hair, oversized hoodie with hood down, baggy cargo pants, high-top sneakers, goggles pushed up on forehead, confident grin. Drawn in classic shonen manga style: crisp black ink linework, pure white paper background, screentone halftone dot shading for shadows, no color except one bold accent red on the hoodie and sneaker laces. Sheet shows front view, side view, and running pose, plus small expression studies (grin, shocked, determined). Hand-lettered label text "VOLUME 1 — THE RUNNER". Clean concept art layout.

### 02 Gameplay, city rooftops (9:16)
Vertical mobile game concept art, gameplay screen mockup of an endless runner. Camera directly behind a teenage street kid in a hoodie sprinting away from the viewer along three parallel lanes on Tokyo rooftops, with water tanks, antenna masts, barriers and a gap to jump. Entire scene drawn as black and white manga ink art: heavy black line work, screentone halftone dots for shading, dramatic radial speed lines rushing toward the center, white sky. The only color is a vivid red accent: red coins floating in the lanes, red hoodie, red neon kanji signs on distant buildings. A big hand-lettered manga sound-effect word "DASH!" and the katakana "ドン!" burst near the runner. Minimal HUD in the corners: distance counter "1,240m" top center and coin count top right, in thick comic lettering. Stylish, high contrast, printed-manga feel.

### 03 Revive screen (9:16)
Vertical mobile game UI concept art: the "game over / revive" screen of a manga-style endless runner. The screen is broken into jagged comic book panels with thick black borders, like a manga page. Panels show: the teenage hoodie runner tripping mid-air with a shocked face, a giant shadowy monster claw reaching from behind, and a close-up of the kid's eye. Black and white ink art with screentone halftone dots, a single accent color red used for the panel gutters glow and a big red splash. Center of the screen has huge hand-lettered title text "TO BE CONTINUED...?" in dramatic manga typography. Below it, a big rounded button reading "▶ KEEP RUNNING (watch ad)" and a smaller text link "Give up". A small countdown "5" inside a circle. Cliffhanger drama, bold composition, printed manga aesthetic.

### 04 Areas board (16:9)
Concept art environment board for a manga-style endless runner, three wide panels side by side on one white page, each drawn in black ink with screentone halftone shading and exactly ONE accent color per panel. Left panel "CITY ROOFTOPS": Tokyo rooftops, water towers, billboards, train tracks below, accent color red on signs and coins. Middle panel "THE HARBOR": shipping containers, cranes, fishing boats, wet docks, accent color electric blue on water reflections and container stripes. Right panel "NIGHT ALLEY": narrow neon alley with hanging lanterns, vending machines, rain puddles, accent color purple on neon glow. Each panel labeled with hand-lettered manga title text at the bottom. Clean concept art sheet layout, high contrast printed-manga style.

### 05 Key art (9:16)
Dramatic manga key art for a mobile endless runner game, vertical poster composition. A teenage street kid in a hoodie leaps across a gap between Tokyo rooftops in the foreground, arms out, hair whipping, coins scattering. Behind him, filling the sky, a colossal shadowy kaiju monster made of ink and screentone dots looms with glowing eyes, chasing him. Pure black and white brush ink artwork with halftone dot screentone shading and heavy radial speed lines, the only color a bold red: the hoodie, the monster's eyes, and a huge red hand-brushed sun circle behind the scene. Big hand-lettered title at the top "INK RUN" in bold manga display lettering with a small subtitle "VOLUME 1". Energetic, iconic app-store key art.

### 06 Pickups and outfits (16:9)
Concept art sheet on white paper showing game pickups and power-ups for a manga-style endless runner, drawn as black ink icons with screentone halftone shading and a single red accent: a spinning coin with a kanji mark, a horseshoe "COIN MAGNET" with little coins flying toward it, a "SPEED DASH" lightning sneaker with speed lines, a "SHIELD" ink splash barrier, and a stack of four obstacle thumbnails labeled "BARRIER", "GAP", "TRAIN", "SIGN". Also a row of three small outfit variants of the same hoodie kid labeled "DEFAULT", "DELIVERY", "RONIN". Hand-lettered manga labels under each item, tidy grid layout, clean concept art style.

## Open decisions before the design doc

All decided: the game is **Ink Run**, the hero is **Nozomi**, the chasers are both
the rival gang and the kaiju, and the engine is Unity. See `DESIGN.md` section 9.
