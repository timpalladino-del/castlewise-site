"""Rebuild every Ink Run model into Assets/InkRun/Models with the bpy module.
    pip install bpy            # Blender 4.2+ as a Python module (tested with 5.2)
    python3 Tools/blender/build_all.py [out_dir]"""
import sys, os
here = os.path.dirname(os.path.abspath(__file__)); sys.path.insert(0, here)
out = sys.argv[1] if len(sys.argv) > 1 else os.path.join(here, '..', '..', 'Assets', 'InkRun', 'Models')
import inklib as K, hero_bl, crew_bl, props_bl, outfits_bl
hero_bl.build(); print(f'{"Hero":14s} {K.tri_count():7d} tris'); K.export_glb(os.path.join(out, 'Hero.glb'))
crew_bl.build(); print(f'{"Crew":14s} {K.tri_count():7d} tris'); K.export_glb(os.path.join(out, 'Crew.glb'))
for name, fn in props_bl.MODELS.items():
    K.reset(); fn(); print(f'{name:14s} {K.tri_count():7d} tris'); K.export_glb(os.path.join(out, name + '.glb'))
for name, fn in outfits_bl.OUTFITS.items():
    outfits_bl.build(fn, name); print(f'{name:14s} {K.tri_count():7d} tris'); K.export_glb(os.path.join(out, name + '.glb'))
