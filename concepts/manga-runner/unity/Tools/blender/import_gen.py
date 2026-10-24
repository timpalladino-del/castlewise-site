"""Turn an AI-generated GLB (Meshy / Tripo / Hunyuan via Higgsfield, or any image-to-3D service) into an
Ink Run game asset. Headless bpy:

    python3 Tools/blender/import_gen.py in.glb out.glb --height 1.7 --yaw 0 --budget 15000 --hero

Steps: import, apply transforms, rotate so the model faces +z (Unity), scale to --height with feet on y 0,
decimate to --budget triangles, quantize the generated texture to the five ink materials (InkPaper, InkAccent,
Ink, InkTone, InkSkin) by sampling the texture at every face so the toon shader can own the look, and, for
--hero, split the mesh into the animator's joint nodes (Body / Head / LegL / LegR / ArmL / ArmR) with their
pivots at the brief's joint positions. Props export as one node named after the output file."""
import sys, os, math, argparse
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import bpy, bmesh
from mathutils import Vector, Matrix
import inklib as K

PALETTE = {  # linear-ish sRGB targets for classification
    'InkPaper': (0.96, 0.96, 0.96), 'InkAccent': (0.85, 0.12, 0.16), 'Ink': (0.08, 0.08, 0.08),
    'InkTone': (0.6, 0.6, 0.6), 'InkSkin': (0.98, 0.9, 0.82),
}
HIP, SH, HEAD, HIP_X, SH_X = 0.84, 1.20, 1.26, 0.11, 0.245   # match hero_bl.py

def args():
    p = argparse.ArgumentParser()
    p.add_argument('src'); p.add_argument('dst')
    p.add_argument('--height', type=float, default=1.7, help='target height in metres (feet to top of mesh)')
    p.add_argument('--yaw', type=float, default=0.0, help='extra yaw in degrees if the model does not face +z after import')
    p.add_argument('--budget', type=int, default=15000, help='target triangle count')
    p.add_argument('--hero', action='store_true', help='split into Body/Head/LegL/LegR/ArmL/ArmR joint nodes')
    p.add_argument('--name', default=None, help='root node name (default: output basename)')
    p.add_argument('--keep-texture', action='store_true', help='keep the generated texture instead of quantizing')
    return p.parse_args(sys.argv[1:])

def import_glb(path):
    bpy.ops.import_scene.gltf(filepath=path)
    meshes = [o for o in bpy.context.scene.objects if o.type == 'MESH']
    # drop armatures/empties from generators; we re-split by region
    for o in list(bpy.context.scene.objects):
        if o.type != 'MESH': 
            for m in meshes:
                if m.parent == o: m.matrix_world = m.matrix_world.copy(); m.parent = None
            bpy.data.objects.remove(o)
    return meshes

def join_all(meshes, name):
    bm = bmesh.new(); slots = []; dg = bpy.context.evaluated_depsgraph_get()
    uv_layer_name = 'UVMap'
    for ob in meshes:
        eo = ob.evaluated_get(dg); me = eo.to_mesh()
        tmp = bmesh.new(); tmp.from_mesh(me); bmesh.ops.transform(tmp, matrix=eo.matrix_world, verts=tmp.verts)
        remap = {}
        for i, m in enumerate(me.materials):
            if m not in slots: slots.append(m)
            remap[i] = slots.index(m)
        for f in tmp.faces: f.material_index = remap.get(f.material_index, 0)
        tmp_me = bpy.data.meshes.new('_t'); tmp.to_mesh(tmp_me); tmp.free(); bm.from_mesh(tmp_me); bpy.data.meshes.remove(tmp_me); eo.to_mesh_clear()
        bpy.data.objects.remove(ob)
    me = bpy.data.meshes.new(name); bm.to_mesh(me); bm.free()
    for m in slots: me.materials.append(m)
    ob = bpy.data.objects.new(name, me); bpy.context.scene.collection.objects.link(ob)
    return ob

def normalize(ob, height, yaw):
    """Face +z (Unity) = -y (Blender); feet on the ground; scale to height."""
    if yaw: ob.data.transform(Matrix.Rotation(math.radians(yaw), 4, 'Z'))
    xs = [v.co for v in ob.data.vertices]
    lo = Vector((min(v.x for v in xs), min(v.y for v in xs), min(v.z for v in xs))); hi = Vector((max(v.x for v in xs), max(v.y for v in xs), max(v.z for v in xs)))
    s = height / max(1e-6, (hi.z - lo.z))
    ob.data.transform(Matrix.Translation((-(lo.x + hi.x)/2, -(lo.y + hi.y)/2, -lo.z)) )
    ob.data.transform(Matrix.Diagonal((s, s, s, 1)))
    return s

def decimate(ob, budget):
    """Quadric edge-collapse to `budget` triangles, per material group so face materials survive
    (Blender's Decimate modifier segfaults in the bpy wheel on imported multi-material meshes)."""
    import numpy as np, fast_simplification
    me = ob.data
    bm = bmesh.new(); bm.from_mesh(me); bmesh.ops.triangulate(bm, faces=bm.faces[:]); bm.verts.ensure_lookup_table()
    tris = len(bm.faces)
    if tris <= budget:
        bm.to_mesh(me); bm.free(); return tris
    ratio = budget / tris
    groups = {}
    for f in bm.faces: groups.setdefault(f.material_index, []).append(f)
    out = bmesh.new(); total = 0
    for mi, faces in groups.items():
        vidx = {}; pts = []; fcs = []
        for f in faces:
            tri = []
            for v in f.verts:
                if v.index not in vidx: vidx[v.index] = len(pts); pts.append(v.co[:])
                tri.append(vidx[v.index])
            fcs.append(tri)
        P = np.asarray(pts, dtype=np.float64); F = np.asarray(fcs, dtype=np.int64)
        if len(F) > 20:
            P2, F2 = fast_simplification.simplify(P, F, target_reduction=1.0 - ratio)
        else:
            P2, F2 = P, F
        vs = [out.verts.new(p) for p in P2]
        for a, b, c in F2:
            if len({a, b, c}) < 3: continue
            try: nf = out.faces.new((vs[a], vs[b], vs[c])); nf.material_index = mi; nf.smooth = True; total += 1
            except ValueError: pass
    bm.free()
    bmesh.ops.remove_doubles(out, verts=out.verts, dist=1e-6)
    bmesh.ops.recalc_face_normals(out, faces=out.faces)
    out.to_mesh(me); out.free()
    me.update()
    return total

def texture_pixels(mat):
    if not mat or not mat.node_tree: return None
    for n in mat.node_tree.nodes:
        if n.type == 'TEX_IMAGE' and n.image:
            img = n.image; w, h = img.size
            if w == 0: return None
            px = list(img.pixels); return (w, h, px)
    return None

def quantize(ob):
    """Sample each face's texture colour at its UV centre and reassign the face to the nearest ink material."""
    me = ob.data; uv = me.uv_layers.active
    tex = {i: texture_pixels(m) for i, m in enumerate(me.materials)}
    base = {i: (m.node_tree.nodes.get('Principled BSDF').inputs['Base Color'].default_value[:3] if m and m.node_tree and m.node_tree.nodes.get('Principled BSDF') else (0.8, 0.8, 0.8)) for i, m in enumerate(me.materials)}
    names = list(PALETTE); mats = [K.mat(n) for n in names]
    new_index = []
    for poly in me.polygons:
        t = tex.get(poly.material_index); col = base.get(poly.material_index, (0.8, 0.8, 0.8))
        if t and uv:
            w, h, px = t; u = v = 0.0
            for li in poly.loop_indices: u += uv.data[li].uv.x; v += uv.data[li].uv.y
            u = (u/len(poly.loop_indices)) % 1.0; v = (v/len(poly.loop_indices)) % 1.0
            x = min(w-1, int(u*w)); y = min(h-1, int(v*h)); k = (y*w + x)*4
            col = (px[k], px[k+1], px[k+2])
        # classify: luminance + redness
        r, g, b = col; lum = 0.3*r + 0.59*g + 0.11*b; red = r - max(g, b)
        if red > 0.25 and r > 0.4: n = 'InkAccent'
        elif lum < 0.3: n = 'Ink'
        elif r > 0.75 and g > 0.62 and b > 0.5 and (r - b) > 0.08 and lum < 0.93: n = 'InkSkin'
        elif lum < 0.78: n = 'InkTone'
        else: n = 'InkPaper'
        new_index.append(names.index(n))
    me.materials.clear()
    for m in mats: me.materials.append(m)
    for poly, i in zip(me.polygons, new_index): poly.material_index = i
    me.uv_layers.remove(uv) if uv else None

def split_hero(ob):
    """Cut the single mesh into the six animator nodes by region planes (Unity coords)."""
    def region(p):  # p in Unity coords
        x, y, z = p
        if y > HEAD + 0.02: return 'Head'
        if y > HIP - 0.02 and abs(x) > 0.2 and y < SH + 0.08: return 'ArmL' if x < 0 else 'ArmR'
        if y < HIP + 0.02: return 'LegL' if x < 0 else 'LegR'
        return 'Body'
    bm = bmesh.new(); bm.from_mesh(ob.data)
    # bisect at the joint planes so the cut edges are clean, then classify faces by centre
    planes = [((0, HEAD + 0.02, 0), (0, 1, 0)), ((0, HIP + 0.02, 0), (0, 1, 0)), ((0, HIP - 0.02, 0), (0, 1, 0)), ((0, SH + 0.08, 0), (0, 1, 0)),
              ((-0.2, 0, 0), (1, 0, 0)), ((0.2, 0, 0), (1, 0, 0)), ((0, 0, 0), (1, 0, 0))]
    for co, no in planes:
        geom = bm.verts[:] + bm.edges[:] + bm.faces[:]
        bmesh.ops.bisect_plane(bm, geom=geom, plane_co=K.U(co), plane_no=K.U(no) - K.U((0, 0, 0)), clear_inner=False, clear_outer=False)
    groups = {}
    for f in bm.faces:
        c = f.calc_center_median(); groups.setdefault(region((c.x, c.z, -c.y)), []).append(f)
    pivots = {'Body': (0, 0, 0), 'Head': (0, HEAD, 0), 'LegL': (-HIP_X, HIP, 0), 'LegR': (HIP_X, HIP, 0), 'ArmL': (-SH_X, SH, 0), 'ArmR': (SH_X, SH, 0)}
    out = []
    for name, faces in groups.items():
        sub = bmesh.new(); vmap = {}
        for f in faces:
            vs = []
            for v in f.verts:
                if v not in vmap: vmap[v] = sub.verts.new(v.co)
                vs.append(vmap[v])
            try: nf = sub.faces.new(vs); nf.material_index = f.material_index; nf.smooth = True
            except ValueError: pass
        # close the cut with a cap so the joint never shows a hole when it swings
        bound = [e for e in sub.edges if e.is_boundary]
        if bound:
            try: bmesh.ops.holes_fill(sub, edges=bound, sides=0)
            except Exception: pass
        O = K.U(pivots[name]); bmesh.ops.translate(sub, vec=-O, verts=sub.verts)
        me = bpy.data.meshes.new(name); sub.to_mesh(me); sub.free()
        for m in ob.data.materials: me.materials.append(m)
        o = bpy.data.objects.new(name, me); bpy.context.scene.collection.objects.link(o); o.location = O
        K.shade_smooth(o, math.radians(50)); out.append(o)
    bm.free(); bpy.data.objects.remove(ob)
    return out

def main():
    a = args()
    K.reset()
    meshes = import_glb(a.src)
    name = a.name or os.path.splitext(os.path.basename(a.dst))[0]
    ob = join_all(meshes, name)
    normalize(ob, a.height, a.yaw)
    if not a.keep_texture: quantize(ob)
    tris = decimate(ob, a.budget)
    K.shade_smooth(ob, math.radians(50))
    root = K.empty('Hero' if a.hero else name + 'Root')
    if a.hero:
        for part in split_hero(ob): K.parent(part, root)
    else:
        K.parent(ob, root)
    print(f'{name}: {tris} tris, height {a.height} m, nodes: {[o.name for o in bpy.data.objects if o.type == "MESH"]}')
    K.export_glb(a.dst)

if __name__ == '__main__':
    main()
