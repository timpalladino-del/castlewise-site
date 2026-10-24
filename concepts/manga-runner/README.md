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

## Round 2 images: crash strips (2026-10-23)

Four three-panel strips, one per obstacle, generated 16:9 in the house style. Save as
`07-crash-barrier.png`, `08-crash-sign.png`, `09-crash-train.png`, `10-crash-gap.png`.
Cut each into its three panels for the crash animation described in `STORY.md`.

Note: a literal "hit by a train" prompt is refused by the image service's safety filter.
The train strip works when written as slapstick (a toy-like tram with a face, Nozomi
bounces off like a rubber ball, nobody hurt), which also fits the game's tone better.

### Shared style prefix
Three-panel manga strip in one wide image, panels side by side with thick black gutters, read left to right. Hero is Nozomi, a teenage street kid: spiky black hair, red hoodie with hood down, goggles pushed up on the forehead, high-top sneakers. Style: pure black ink linework on white paper, screentone halftone dots for shadows, the only color a bold red.

### 07 Barrier
Panel 1: low angle, Nozomi sprinting toward a striped low road barrier on a city rooftop, his sneaker catching the top rail, motion lines. Panel 2: he flips face-first over the barrier, limbs flailing, huge hand-lettered sound effect "WHAM!" bursting across the panel, radial speed lines. Panel 3: Nozomi flat on his back on the rooftop, goggles knocked sideways, small stars circling his head, a single red coin rolling away, tiny caption box "The rail was higher than it looked."

### 08 Sign
Panel 1: an overhead sign board on two posts fills the frame, the word "STOP" on it in red, Nozomi running toward it from below, too late to duck. Panel 2: Nozomi clotheslined by the board at neck height, his feet still running in mid-air, body horizontal, huge hand-lettered katakana sound effect "ガン!" across the panel, impact lines. Panel 3: Nozomi sitting slumped against the sign post, stars circling his head, the sign slightly bent, tiny caption box "He read the sign. Too late."

### 09 Train (slapstick version)
Comedic cartoon tone, nobody is hurt. Panel 1: a boxy toy-like rooftop tram with a big round headlight and a smiling face painted on its nose rolls toward Nozomi, who is turning around mid-run with a comically wide-eyed "uh oh" expression, a red horn sound effect word "PAAAP" above. Panel 2: Nozomi bounces off the flat front of the tram like a rubber ball, squashed into a comic pancake shape, giant hand-lettered katakana "ドン!" filling the panel, red coins popping out of his pockets in a fountain, radial speed lines. Panel 3: Nozomi sitting on the rails dizzy with stars circling his head, hair standing straight up, the tram's tail leaving the frame, one red coin spinning on its edge beside him, tiny caption box "The Nozomi doesn't stop for anyone."

### 10 Gap
Panel 1: close on Nozomi's sneakers skidding at the edge of a rooftop, toes over the edge, the gap between buildings below drawn as solid black with a few white speckles, his face shocked. Panel 2: Nozomi falling into the gap, arms windmilling, drawn from below looking up, three gang silhouettes in black jackets peering down from the roof edge above, hand-lettered katakana sound effect "ヒュー" trailing down the panel. Panel 3: Nozomi caught upside down in a laundry line strung between the buildings two floors down, a red sock hanging next to his head, tiny caption box "Lucky. This time."

## Round 3 images: story strips and chasers (2026-10-23)

Save as `11-story-cold-open.png`, `12-story-city-to-harbor.png`, `13-story-harbor-to-alley.png`,
`14-story-alley-to-city.png`, `15-smudge-crew.png`, `16-the-shadow.png`. Captions match
`STORY.md`; the strips cut into three panels for the chapter transitions.

### 11 Cold open (16:9)
Three-panel manga strip in one wide image, thick black gutters, read left to right. Style: pure black ink linework on white paper, screentone halftone dots for shadows, the only color a bold red. This is the cold open of a story. Panel 1: a Tokyo rooftop at dawn, a huge flat red sun behind water tanks and antennas, four small silhouettes of a street gang in black jackets standing on a far rooftop, caption box "The city never sleeps. Neither does the Smudge Crew." Panel 2: extreme close-up of a teenage hand gripping a small red coin pouch, knuckles tense, caption box "Nozomi has something of theirs." Panel 3: extreme close-up of Nozomi's eye under spiky black hair and a pair of goggles pushed up on the forehead, fierce, radial speed lines, huge hand-lettered katakana "ダッ" and a single-word caption "Run." Printed-manga feel, high contrast.

### 12 City → Harbor (16:9)
Three-panel manga strip in one wide image, thick black gutters, read left to right. Style: pure black ink linework on white paper, screentone halftone dots for shadows. The accent color shifts across the strip: red in panel 1, red and blue in panel 2, only blue in panel 3. Panel 1: Nozomi, a teenage street kid with spiky black hair, red hoodie and goggles on his forehead, skids to a stop at the last rooftop edge, harbor cranes and ships visible far below, caption box "The rooftops run out." Panel 2: four gang silhouettes in black jackets against a big flat red sun, one pointing straight at the viewer, caption box "The Crew is still there. Four of them now." Panel 3: Nozomi landing on wet dock concrete between stacked shipping containers with blue stripes, water reflections in blue, katakana sound effect "ザザッ", caption box "Down to the docks. Don't look at the water." Printed-manga feel, high contrast.

### 13 Harbor → Night Alley (16:9)
Three-panel manga strip in one wide image, thick black gutters, read left to right. Style: pure black ink linework on white paper, screentone halftone dots for shadows. The accent color shifts across the strip: blue in panel 1, blue and purple in panel 2, only purple in panel 3. Panel 1: black harbor water under the hull of a ship, one enormous eye opening just below the surface, ripples in blue, caption box "Something moved under the ships. Something big." Panel 2: three gang members in black jackets frozen on a harbor crane, looking back over their shoulders in fear, caption box "The Crew saw it too. Even they look scared." Panel 3: Nozomi, a teenage street kid with spiky hair, hoodie and goggles, running into a narrow rainy alley lit by hanging paper lanterns glowing purple, puddles reflecting the light, katakana sound effect "ポツ", caption box "Into the alleys. The lanterns are the only light." Printed-manga feel, high contrast.

### 14 Night Alley → City, the loop (16:9)
Three-panel manga strip in one wide image, thick black gutters, read left to right. Style: pure black ink linework on white paper, screentone halftone dots for shadows. The accent color shifts across the strip: purple in panel 1, red from panel 2 on. Panel 1: a narrow alley with purple paper lanterns opening onto an outdoor stairwell, dawn light at the top, caption box "Dawn again. The city looks the same." Panel 2: a city skyline silhouette in solid black against a flat red sun, with a kaiju-shaped gap in the skyline where the buildings are missing, as if something enormous is standing among them, caption box "It isn't. The Shadow is behind the skyline now." Panel 3: Nozomi, a teenage street kid with spiky black hair and red hoodie, pulls his goggles down over his eyes and grins, huge hand-lettered katakana "ドン", caption box "Keep running, Nozomi. Volume 1 isn't over." Printed-manga feel, high contrast.

### 15 The Smudge Crew (16:9)
Character design sheet for a mobile game, the rival gang "THE SMUDGE CREW": four teenage rooftop runners in matching black bomber jackets with a white ink-splat emblem on the back, drawn in classic shonen manga style: crisp black ink linework, white paper background, screentone halftone dots for shadows, the only color a bold red used on one detail per character (a bandana, a cap brim, sneaker laces, a cast on one arm). The four: a tall lanky leader with a long ponytail and a smirk; a short stocky one with a buzz cut and a boombox; a girl with twin buns and a skateboard; a nervous skinny one with round glasses. Each shown full-body front view with a small head-and-shoulders expression study, names hand-lettered beneath: "SUMI", "BOKU", "HANE", "TEN". Title lettering "THE SMUDGE CREW". Clean concept art layout.

### 16 The Shadow (16:9)
Creature concept sheet for a mobile game kaiju called "THE SHADOW": a colossal monster made entirely of black ink and screentone halftone dots, its body half-dissolving into drips and dot patterns, no clear face except two glowing eyes drawn as flat red circles. Drawn in manga style: black ink on white paper, the only color the red eyes. Sheet shows three views: the full silhouette towering over a Tokyo skyline with a tiny running figure for scale; a close-up of one eye emerging from black harbor water; and a study of its limb as a splash of ink turning into a claw. Hand-lettered title "THE SHADOW" and small notes "never fully seen in Volume 1". Clean concept art layout, high contrast.

## Image-to-3D reference prompts (build 12, for Tripo / Meshy via Higgsfield)

Nozomi, front A-pose (nano-banana-pro, 3:4, 2K):
> Character turnaround reference sheet for a 3D game asset: a stylized teenage courier named Nozomi standing in a relaxed A-pose, front view, full body head to toe, centered on a plain pure white background, no shadow. Manga ink style rendered as a clean 3D toon character: flat colors, bold black outlines, only three colors (black ink, white paper, one bright red accent). About 4 heads tall with a big round head, big hands and chunky feet. Spiky black hair with 3 dominant spikes, red-lensed goggles pushed up on the hairline with a grey strap, friendly smirk, big eyes set low on the face. Red hoodie with a hood draped on the shoulders and a kangaroo pocket, white cuffs, black jogger pants with a white side stripe, chunky white sneakers with a red heel tab and thick black sole. Clean silhouette, no text, no props, symmetrical, orthographic look, even lighting.

Tram, three-quarter front (nano-banana-pro, 4:3, 2K):
> Vehicle reference for a 3D game asset: a small stylized Japanese city tram, three-quarter front view showing the nose and one full side, centered on a plain pure white background, no shadow. Manga ink style rendered as a clean 3D toon model: flat colors, bold black outlines, only black ink, white paper and a thin grey waistline band. Rounded loaf-shaped body with a smooth rounded cab, one wide wraparound black cab window, two small rectangular headlights, a white destination box on the roof front, inset black side windows, flush double doors, a dark skirt hiding the wheels, a simple diamond pantograph on the roof. Chunky toy-like proportions, about 6 m long and 2.6 m tall, no text, even lighting, orthographic look.

Both images are imported into Higgsfield storage (Nozomi media 0b876a18-d944-480f-bc8d-dfff6f672c3a, tram 78ba66c9-4ac2-4abf-a30c-230814bd2334). Pipeline: `unity/Tools/blender/GEN-PIPELINE.md`.
