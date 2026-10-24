# gen/ — drop folder for AI-generated meshes (free route)

1. Generate a mesh for free on the web, from the reference images listed in `REFS.md`:
   - Tripo: https://www.tripo3d.ai (free monthly credits; "Image to 3D", upload front + back + side as multi-view if offered,
     pick Quad / low-poly ~15k faces, textured, export GLB).
   - Meshy: https://www.meshy.ai (free monthly credits; Image to 3D, A-pose for the hero, GLB export).
   - Or run an open-source generator on the Mac (Hunyuan3D-2, TripoSR) — see `../unity/Tools/blender/GEN-PIPELINE.md`.
2. Save the GLB into `gen/in/` with one of these names (the ingest maps names to heights and node layouts):

   | file | what | height (m) | pivot | layout |
   |---|---|---|---|---|
   | `Hero.glb` | Nozomi (done, build 16) | 1.95 incl. hair | centre | 6 joint nodes |
   | `Crew.glb` (or `Gang.glb`) | Smudge Crew chaser, black jacket, white bandana, red scarf | 1.75 | centre | 6 joint nodes |
   | `Train.glb` | tram, nose toward the camera (-z) | 3.0 | under the nose, body extends +z | single |
   | `BuildingCity.glb` | chapter 1 facade: 2 storeys + shopfront, ~3.6 m wide, 4.6 m deep | 7.7 | track-side front corner, extends +x and +z | single |
   | `BuildingAlley.glb` | chapter 3 facade: narrow 3 storeys, lantern arm over the street | 9.1 | track-side front corner | single |
   | `Containers.glb` | chapter 2 harbor: stacked containers + crane | 8.2 | track-side front corner | single |
   | `Barrier.glb`, `Sign.glb`, `Coin.glb`, `Lantern.glb` | props | 1.2 / 2.3 / 0.75 / 1.3 | centre | single |
   | `Kaiju.glb` | the kaiju itself (Volume 2; only its shadow is used now) | 6.0 | centre | single |

   Side buildings must face the track: generate them so the shopfront is the long face, and run the ingest with
   `YAW=90` or `YAW=-90` if the review sheet shows the facade pointing down the track instead of across it.

3. Run the ingest (needs `pip install bpy fast-simplification trimesh`):

       bash unity/Tools/blender/ingest.sh

   It writes `unity/Assets/InkRun/Models/<name>.glb`, prints the triangle count and renders a review sheet into
   `gen/review/`. Commit and push, or tell the cloud session the files are there.
4. If a model comes in facing the wrong way, re-run with `YAW=180 bash unity/Tools/blender/ingest.sh Hero`.
