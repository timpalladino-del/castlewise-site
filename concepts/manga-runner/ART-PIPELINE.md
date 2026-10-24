# Ink Run — 3D art pipeline

The bar is a studio runner (Subway Surfers City). The hand-drawn prototype defines the
look; the shipping look is real 3D models in Unity with the ink toon shader on top.
This is how the models get made without a modeling team.

## The look, restated for 3D

- Stylized toon proportions: big head, long legs, chunky hands and shoes.
- Smooth low-detail surfaces. Detail comes from the shader: flat paper color, one
  halftone shadow step, thick ink outline. No PBR, no normal maps.
- One accent color per area, set globally by the shader (`_InkAccent`). Everything that
  should take the accent uses the InkAccent material: hoodie, coins, sun, stripes, signs.
- 3,000 to 8,000 triangles for the hero, 300 to 2,000 for props. Mobile budget.

## Pipeline

1. **Reference image** (done for the first four, prompts below): one clean cel-shaded
   render on white, A-pose for characters, three-quarter view for props.
2. **Image to 3D**: Meshy or Tripo via Higgsfield, textured. For characters add
   rigging (`enable_rigging`, `pose_mode: a-pose`) and the run clip (`animation_action_id`
   16 RunFast); request jump (466) and a slide clip as separate rig jobs.
   Cost: about 9 credits per model on Higgsfield. The account has 0.5 right now.
3. **Cleanup in Blender** (Mac session, scripted): decimate to budget, recenter at the
   feet, scale to meters (hero 1.7 m), merge materials into two (Paper, Accent) by
   color, export FBX or GLB.
4. **Unity**: import, assign InkPaper / InkAccent materials, drop into the prefab slot the
   scene builder made (replace the primitive children, keep the collider and script).
5. **Animation**: Animator with Running, Jump, Slide, Crash states driven by the
   parameters `PlayerController` already sets.

## Asset list for the first pass

| Asset | Reference | Tris | Notes |
|---|---|---|---|
| Nozomi (default) | hero A-pose render | 6k | rigged, run + jump + slide + fall clips |
| Nozomi Delivery outfit | same body, cap + box | 6k | swap parts, same rig |
| Nozomi Ronin outfit | same body, headband + bokken | 6k | swap parts, same rig |
| Barrier | barrier render | 400 | |
| Sign | post + board, 止 as a decal texture | 400 | |
| Train (N700 nose) | train render | 1.5k | front car only, 6 m long |
| Gap lip | none, built in Unity | | black decal + hazard stripe strip |
| Coin | none, built in Unity | 200 | disc with ¥ decal |
| Magnet, Dash badge | icon render | 400 | |
| City block A/B | building render + variant | 1.5k | tank, AC, antenna, billboard |
| Harbor container stack | containers render | 800 | accent stripe |
| Harbor crane | crane render | 600 | |
| Alley building | narrow building render | 1.5k | sign, AC, wires as a separate strip |
| Lantern | small sphere, built in Unity | 100 | accent |
| Skyline strip | flat silhouette mesh | 200 | far background |
| Sun disc | built in Unity | | accent, outline via shader |
| Smudge Crew (4) | character sheet | 4k each | later: appear in transitions |

## Reference prompts (image generation, white background)

Shared prefix: "Clean 3D reference on a plain white background, polished stylized toon
game asset like a modern mobile runner, cel-shaded with soft flat lighting and bold black
outlines, smooth clean surfaces, no ground shadow, no text, centered, whole object visible."

- Hero: "a stylized teenage boy, relaxed A-pose, facing camera, spiky black hair, red hoodie hood down with white drawstrings, dark grey cargo pants, white high-top sneakers with red laces, red-lens goggles on the forehead, confident grin, full body."
- Train: "a boxy Japanese commuter train front car, rounded nose, large windshield, two round headlights, red stripe along the side, destination board, three-quarter front view slightly above."
- Barrier: "a low road barrier, white board with red diagonal hazard stripes on two black metal legs, three-quarter view."
- Building: "a chunky three-story concrete rooftop block, cylindrical water tank on steel legs, small AC unit, TV antenna, rows of square windows, a red vertical sign board, three-quarter view slightly above."

## Checks before a model goes in

- Silhouette reads at 1/6 of screen height (the hero's on-screen size).
- Outline pass has no cracks (check for split normals after decimation).
- Only two materials on the mesh.
- Hero's feet at y 0, facing +z, 1.7 m tall.
