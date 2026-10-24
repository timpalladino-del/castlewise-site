# gen/ — drop folder for AI-generated meshes (free route)

1. Generate a mesh for free on the web, from the reference images listed in `REFS.md`:
   - Tripo: https://www.tripo3d.ai (free monthly credits; "Image to 3D", upload front + back + side as multi-view if offered,
     pick Quad / low-poly ~15k faces, textured, export GLB).
   - Meshy: https://www.meshy.ai (free monthly credits; Image to 3D, A-pose for the hero, GLB export).
   - Or run an open-source generator on the Mac (Hunyuan3D-2, TripoSR) — see `../unity/Tools/blender/GEN-PIPELINE.md`.
2. Save the GLB into `gen/in/` with one of these names (the ingest maps names to heights and node layouts):

   | file | what | height (m) | layout |
   |---|---|---|---|
   | `Hero.glb` | Nozomi | 1.70 (hair adds to ~1.95; use 1.95 if the mesh includes the full spikes) | 6 joint nodes |
   | `Train.glb` | tram | 2.6 (nose toward -z after `--yaw`) | single |
   | `Gang.glb` | rival courier chaser | 1.75 | 6 joint nodes |
   | `Kaiju.glb` | kaiju chaser | 6.0 | single |
   | `Barrier.glb`, `Sign.glb`, `Coin.glb`, `Lantern.glb` | props | 1.1 / 2.3 / 0.7 / 1.3 | single |

3. Run the ingest (needs `pip install bpy fast-simplification trimesh`):

       bash unity/Tools/blender/ingest.sh

   It writes `unity/Assets/InkRun/Models/<name>.glb`, prints the triangle count and renders a review sheet into
   `gen/review/`. Commit and push, or tell the cloud session the files are there.
4. If a model comes in facing the wrong way, re-run with `YAW=180 bash unity/Tools/blender/ingest.sh Hero`.
