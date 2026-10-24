# Ink Run asset briefs (Phase 0, per the game-asset-modeling / blender-image-to-3d skills)

Conventions: Unity metres, y up, hero faces +z. 1 unit = 1 m. Author in Unity space with `inklib.U()`.
Primary view for everything: the game camera, behind and above the hero (0, 3.1, -4.6) looking at (0, 1.3, 6),
portrait 1080x1920, 58 deg vertical FOV. Hero is ~450 px tall on screen; an obstacle 10 m ahead is ~150 px.
Style: ink + paper + one accent red, 3-step toon ramp, inverted-hull outline (6-8 mm). Flat-colour regions;
the silhouette and the value pattern (black / white / red) must carry the read, never surface detail.

Sources used for the rules below: game-asset-modeling SKILL + references (budgets, mid-poly bevel, review critic),
blender-image-to-3d SKILL + categories.md (skin chains, loft stations, prop/vehicle/architecture construction,
acceptance), reference-modeling SKILL (numeric gates, bevel-after-form, fresh-eyes critic), chibi/stylised
proportion guides (2.5-4 heads, eyes >= 1/4 head height set low on the skull, small nose, soft tapering limbs,
oversized hands/feet, egg torso), shape-language guides (dominant + secondary shape, silhouette read in black).

## CH_Nozomi (hero)

ASSET        CH_Nozomi
CATEGORY     character, biped, stylised courier teen
VIEWS        game camera from behind (primary); front/side/back ortho for gates
SCALE        1.70 m to top of skull (hair adds ~0.25). Feet at y 0.
PROPORTIONS  (head = skull height H = 0.46 m -> 3.7 heads; stylised range 3-4 heads)
  - skull height 0.46, skull width 0.42, depth 0.40 (eyes at 0.40 of skull height from the chin -> set low)
  - eye height 0.11 (= 0.24 H), eye width 0.10, gap between eyes 0.07; nose tiny (0.03); mouth wide 0.14
  - neck short and thick: 0.13 wide, visible 0.05
  - shoulders 0.60 wide (1.4 skull widths); chest depth 0.30; waist 0.40 wide; hips 0.42 wide
  - torso (shoulder line to crotch) 0.42 = 0.9 H; legs (crotch to sole) 0.82 = 1.8 H; arms wrist at mid-thigh
  - hands: mitt 0.16 long x 0.12 wide (about 1/3 of skull height, oversized); feet 0.34 long x 0.15 wide
  - limb taper: upper arm 0.19 diameter -> wrist 0.11; thigh 0.24 -> ankle 0.13; no muscle definition
  - hair spikes add 0.25 above the skull; total silhouette height ~1.95
PARTS        Body (torso + hoodie + pants/hips) / Head (skull, face, hair, goggles, neck) / LegL LegR (pant leg + sneaker) /
             ArmL ArmR (sleeve, cuff, mitt). Pivots: hips at (±0.11, 0.84, 0), shoulders at (±0.30, 1.22, 0), head at (0, 1.30, 0).
SILHOUETTE   1) spiky hair crown with 3 dominant spikes, 2) goggle strap band, 3) big round mitts, 4) hoodie hem flaring
             over the hips, 5) chunky sneakers with a thick sole. Dominant shape: circle (head, mitts, shoes); secondary:
             rectangle (torso); accent: triangles (hair).
MATERIALS    InkAccent hoodie + sneaker heel tab + goggle lenses; Ink hair, pants, strap, pupils; InkPaper cuffs, collar,
             sneaker, eye whites, glyph; InkSkin skull, neck, mitts; InkTone nose.
ARTICULATION Node rotation only (HeroAnimator): hips swing ±40, shoulders ±45, body bob. Sleeves and pant legs must
             be long enough that the joint caps stay inside the torso/hips when swung 45 deg (overlap >= 0.06).
BUDGET       LOD0 <= 24k tris (standard character, mobile), LOD1 <= 8k (Unity LODGroup later).
GATES        dim_gate: height 1.70 ±3%, shoulders 0.60 ±5%, skull 0.46/0.42/0.40 ±5%, hand length 0.16 ±10%, foot 0.34 ±5%.
             extent_profile on the torso: width must vary (hem > waist < chest), not a tube.
             mesh_report: no ngons on curved surfaces, no loose verts; every closed part manifold.
             Review: clay + silhouette renders front/side/back/three-quarter + 450-px gameplay strip in greyscale.
NEVER        a straight-cut hair line; eyes above skull mid-height; sleeves abutting the torso (overlap them);
             detail smaller than 2 cm (invisible at 450 px); pure cylinders for limbs.

## Props (invented track, per tracks.md §invented) — dims fixed by SceneBootstrap colliders

| id | dims (w h d) | collider | silhouette features | budget |
|---|---|---|---|---|
| VH_Tram | 1.9 x 2.6 x 6.0, nose at z<0 | box 1.9x2.6x6 @ (0,1.3,3) | rounded cab, waist band, big round headlights, pantograph | 12k |
| PR_Barrier | 1.6 x 1.0 x 0.5 | box 1.6x0.8x0.4 @ (0,0.5,0) | chevron board, round rail, A-legs, lamp | 3k |
| PR_Sign | 1.8 x 2.3 x 0.3 | box 1.75x0.95x0.3 @ (0,1.78,0) | board on two posts, red band + 止 | 2k |
| AR_BuildingCity | 3 x 6 x 4 (x 0..3) | none | recessed windows, rooftop tank, billboard | 15k |
| AR_Containers | 3 x 3.5 x 4 + crane 8 | none | two stacked boxes with ribs, crane arm + hook | 8k |
| AR_BuildingAlley | 2.4 x 9 x 4 | none | tall slab, vertical sign, lanterns, vending machine | 15k |
| PR_Coin | 0.7 disc @ y 0.6 | sphere r0.45 @ (0,0.6,0) | thick red disc, yen bars | 2k |
| PR_Magnet / PR_Dash | 1.04 badge @ y 0.9 | sphere r0.6 @ (0,0.8,0) | badge + icon | 2k |
| PR_Lantern | 0.6 x 1.3 | none | ribbed red ovoid, tassel | 2k |

Prop rules: every exposed edge bevelled (2-4 cm architecture, 1-2 cm vehicles, 5 mm small props); big-medium-small
detail hierarchy (one dominant mass, 2-3 secondary forms, few tertiary details >= 5 cm); parts overlap at joins
(>= 1 cm) instead of abutting; no faces under the ground or inside other parts; silhouette must read in black at 150 px.
Scale cues: door 0.9 x 2.1, storey 3.0, tram door 1.3 x 1.9, headlight 0.28, rail 0.11 diameter.
