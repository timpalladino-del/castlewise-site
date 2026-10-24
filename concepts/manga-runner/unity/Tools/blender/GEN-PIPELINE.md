# Ink Run: AI-generated meshes (image-to-3D) pipeline

Procedural geometry hit its ceiling at build 11. From here the hero, chasers and the tram come from an
image-to-3D generator, and `import_gen.py` turns the result into an Ink Run asset.

## Route A: Higgsfield from the cloud session (already wired up)

The cloud Claude session has Higgsfield connected and has already imported the two reference images
(Nozomi front A-pose, tram three-quarter) into Higgsfield storage. Costs measured with `get_cost` on this account:

| Generator (via Higgsfield) | Settings | Credits per model |
|---|---|---|
| Tripo H3.1 image-to-3D | textured, quad, 15k faces | 10.5 |
| Hunyuan3D v3 | LowPoly quad, 40k faces | 18 |
| Meshy 7 image-to-3D | lowpoly quad, 15k, textured, A-pose | 38 |
| Meshy 7 + auto-rig (1.7 m) | as above + rig, optional run animation | 44 |

Plan for the first pass: Tripo for the hero and tram (2 x 10.5), one Meshy rigged hero if the Tripo mesh is
good enough to keep (44), then Tripo for the gang and kaiju chasers (3 x 10.5). About 120 credits with retries.
The account is on the free plan with 0.5 credits; top up first (the session can open the Higgsfield pricing widget).

Flow per asset:
1. `generate_image` (Runway, nano-banana-pro): one clean view, flat ink colours, white background, A-pose for
   characters, three-quarter for vehicles. Prompts are in `README.md` of the concept folder.
2. `media_import_url` into Higgsfield (server-side, works even though the cloud session cannot see the image).
3. `generate_3d` with the chosen model; download the GLB (`show_result` / job url).
4. `python3 Tools/blender/import_gen.py in.glb Assets/InkRun/Models/Hero.glb --height 1.7 --budget 15000 --hero`
   (props: `--height <m>` and no `--hero`; add `--yaw 180` if the model faces away from +z).
5. Rebuild the gray-box scene in Unity; the prefabs pick the new GLBs up by name.

## Route B: from the Mac session (no network restrictions)

The Mac has Unity and can call the generator APIs directly. Paste into the Mac Claude session:

```
Pull branch claude/manga-runner-concept-xzx1kz. Read concepts/manga-runner/unity/Tools/blender/GEN-PIPELINE.md.
Install the bpy wheel (pip install bpy fast-simplification trimesh). Using my Tripo (or Meshy) API key in the
env var TRIPO_API_KEY, generate an image-to-3D GLB from concepts/manga-runner/refs/nozomi_front.png with quad
topology and ~15k faces, then run import_gen.py with --hero to produce Assets/InkRun/Models/Hero.glb, rebuild
the Ink Run gray-box scene in Unity, press Play and send me a screenshot.
```

Direct API docs: Tripo `https://platform.tripo3d.ai/docs`, Meshy `https://docs.meshy.ai`. Both accept a single
image upload and return a GLB URL when the task succeeds.

## What import_gen.py does

Import, apply transforms, face +z, feet on y 0, scale to `--height`, quantize the generated texture to the five ink
materials (so the toon shader and outline own the look and the texture is not needed at runtime), quadric-decimate
to `--budget` triangles per material group, then for `--hero` bisect at the brief's joint planes and emit the
Body / Head / LegL / LegR / ArmL / ArmR nodes with pivots at hips (±0.11, 0.84), shoulders (±0.245, 1.20) and
head (0, 1.26) so `HeroAnimator` works unchanged. Cut faces are capped so joints never show holes.

If a Meshy rig + run animation is used instead, keep `--keep-texture`, skip `--hero`, and drive it with Unity's
Animator (glTFast imports the clip); that path replaces `HeroAnimator` and is the plan once the static mesh is approved.

## Review loop

Same as before: `gates.py` numbers (height, shoulders, mesh report), clay renders, three.js toon sheet, and a
fresh-eyes critic pass before the GLB replaces the procedural one.
