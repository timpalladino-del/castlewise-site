# Reference images for image-to-3D (generated with nano-banana-pro, flat ink style, white background)

Download these into `gen/refs/` before using a web generator or the Mac route. The Higgsfield-hosted copies are
stable; the Runway links are signed and expire after about a day (regenerate from the prompts in `../README.md`).

| view | file name to save as | stable URL (Higgsfield storage) |
|---|---|---|
| Nozomi, front A-pose | `nozomi_front.png` | https://d2ol7oe51mr4n9.cloudfront.net/user_3FhoDFj8NgEBiXU1x0R0F1i91cM/0b876a18-d944-480f-bc8d-dfff6f672c3a.png |
| Nozomi, back view | `nozomi_back.png` | https://d2ol7oe51mr4n9.cloudfront.net/user_3FhoDFj8NgEBiXU1x0R0F1i91cM/90924878-28a4-4cf0-92ae-39691de04ef0.png |
| Nozomi, left side | `nozomi_side.png` | https://d2ol7oe51mr4n9.cloudfront.net/user_3FhoDFj8NgEBiXU1x0R0F1i91cM/f2e9682c-cf6f-4f92-a805-f7ef472b79bd.png |
| Tram, three-quarter front | `tram.png` | https://d2ol7oe51mr4n9.cloudfront.net/user_3FhoDFj8NgEBiXU1x0R0F1i91cM/78ba66c9-4ac2-4abf-a30c-230814bd2334.png |

Higgsfield media ids (for the cloud session's `generate_3d` once credits exist): front 0b876a18-d944-480f-bc8d-dfff6f672c3a,
back 90924878-28a4-4cf0-92ae-39691de04ef0, side f2e9682c-cf6f-4f92-a805-f7ef472b79bd, tram 78ba66c9-4ac2-4abf-a30c-230814bd2334.
Multi-view tip: in Tripo, use "Multi-view" and give front, back and left; in Meshy, give the front as the main image.

## Crash strips (16:9, three vertical panels each; the game crops thirds at runtime)

Save these into `unity/Assets/InkRun/Resources/Strips/` with exactly these names so `CrashStripView` finds them:

| obstacle | save as | stable URL |
|---|---|---|
| barrier | `Barrier.png` | https://d2ol7oe51mr4n9.cloudfront.net/user_3FhoDFj8NgEBiXU1x0R0F1i91cM/dc962d19-ed0c-4c45-b77c-8ddceea774e7.png |
| overhead sign | `Sign.png` | https://d2ol7oe51mr4n9.cloudfront.net/user_3FhoDFj8NgEBiXU1x0R0F1i91cM/54fcbb09-37ea-4930-8a59-ff426dd6923a.png |
| tram (ObstacleKind.Wall) | `Wall.png` | https://d2ol7oe51mr4n9.cloudfront.net/user_3FhoDFj8NgEBiXU1x0R0F1i91cM/333e6bd4-93da-44a6-aaff-94a9b53f34ed.png |
| gap | `Gap.png` | https://d2ol7oe51mr4n9.cloudfront.net/user_3FhoDFj8NgEBiXU1x0R0F1i91cM/d1d67747-da55-40b0-b26e-e9f70d59d105.png |

Without the files the strip plays as ink text panels (approach line, red SFX word, aftermath line) with the same cuts.
