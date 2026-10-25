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
    p.add_argument('--keep-texture', action='store_true', help='keep the generated texture instead of quantizing (the bpy wheel exporter crashes on textured imports; use only with a full Blender)')
    p.add_argument('--ref', default=None, help='front reference drawing: paints an untextured mesh by projection')
    p.add_argument('--auto-joints', action='store_true', help='measure hip/shoulder/neck from the mesh instead of the brief constants')
    p.add_argument('--anchor', default='center', choices=['center', 'front', 'corner'], help='pivot placement: center (characters, props), front (trains), corner (side buildings)')
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

def normalize(ob, height, yaw, anchor='center'):
    """Face +z (Unity) = -y (Blender); feet on the ground; scale to height.
    anchor: 'center' (characters, props: pivot under the middle), 'front' (trains: pivot under the nose, body
    extends +z), 'corner' (side buildings: pivot at the track-side front-bottom corner, body extends +x and +z,
    the way AreaController places SideProps)."""
    if yaw: ob.data.transform(Matrix.Rotation(math.radians(yaw), 4, 'Z'))
    xs = [v.co for v in ob.data.vertices]
    lo = Vector((min(v.x for v in xs), min(v.y for v in xs), min(v.z for v in xs))); hi = Vector((max(v.x for v in xs), max(v.y for v in xs), max(v.z for v in xs)))
    s = height / max(1e-6, (hi.z - lo.z))
    dx = -(lo.x + hi.x)/2 if anchor != 'corner' else -lo.x
    dy = -(lo.y + hi.y)/2 if anchor == 'center' else -hi.y          # Unity z = -Blender y: z-min at the pivot means Blender y-max = 0
    ob.data.transform(Matrix.Translation((dx, dy, -lo.z)))
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
        elif r > 0.75 and g > 0.62 and b > 0.5 and (r - b) > 0.06 and lum < 0.985: n = 'InkSkin'
        elif lum < 0.78: n = 'InkTone'
        else: n = 'InkPaper'
        new_index.append(names.index(n))
    me.materials.clear()
    for m in mats: me.materials.append(m)
    for poly, i in zip(me.polygons, new_index): poly.material_index = i
    me.uv_layers.remove(uv) if uv else None


def boundary_loops(bm):
    """Ordered vertex loops along the open boundary of a bmesh."""
    bm.edges.ensure_lookup_table()
    edges = {e for e in bm.edges if e.is_boundary}
    loops = []
    while edges:
        e = edges.pop(); start = e.verts[0]; v = e.verts[1]; loop = [start, v]; prev = e
        guard = 0
        while v is not start and guard < 5000:
            guard += 1
            nxt = next((x for x in v.link_edges if x in edges), None)
            if nxt is None: break
            edges.discard(nxt); v = nxt.other_vert(v)
            if v is not start: loop.append(v)
            prev = nxt
        if v is start and len(loop) >= 3: loops.append(loop)
    return loops

def split_hero(ob, J=None):
    """Cut the single mesh at the joint planes, then label connected components (head, torso, two arms with
    hands, two legs) by where they sit. Pivots at the joints so HeroAnimator can swing them."""
    J = J or {'HIP': HIP, 'SH': SH, 'HEAD': HEAD, 'HIP_X': HIP_X, 'SH_X': SH_X, 'ARM_PLANE': 0.2}
    HIPj, SHj, HEADj, HIPXj, SHXj, ARM = J['HIP'], J['SH'], J['HEAD'], J['HIP_X'], J['SH_X'], J['ARM_PLANE']
    bm = bmesh.new(); bm.from_mesh(ob.data)
    def ucenter(f):
        c = f.calc_center_median(); return (c.x, c.z, -c.y)
    def cut(co, no, pick=None):
        faces = [f for f in bm.faces if pick is None or pick(ucenter(f))]
        verts = {v for f in faces for v in f.verts}; edges = {e for f in faces for e in f.edges}
        geom = list(verts) + list(edges) + faces
        res = bmesh.ops.bisect_plane(bm, geom=geom, plane_co=K.U(co), plane_no=K.U(no) - K.U((0, 0, 0)), clear_inner=False, clear_outer=False)
        cut_edges = [g for g in res['geom_cut'] if isinstance(g, bmesh.types.BMEdge)]
        if cut_edges: bmesh.ops.split_edges(bm, edges=cut_edges)      # actually separate the two sides
    # where do the hands hang? histogram |x| of face centres in the thigh band; the widest empty gap outside the
    # legs is the leg/hand separation (Tripo meshes have the hands touching the thighs, so connectivity is useless)
    def hand_plane():
        xs = sorted(abs(ucenter(f)[0]) for f in bm.faces if HIPj - 0.32 < ucenter(f)[1] < HIPj - 0.02)
        if not xs: return HIPXj + 0.1
        step = 0.01; bins = int(xs[-1] / step) + 2; hist = [0] * bins
        for x in xs: hist[int(x / step)] += 1
        best = (0, None); run = 0; i0 = int((HIPXj + 0.03) / step)
        for i in range(i0, bins):
            run = run + 1 if hist[i] == 0 else 0
            if run > best[0]: best = (run, i)
        if best[1] is None or best[0] < 2: return HIPXj + 0.1
        return (best[1] - best[0] / 2 + 0.5) * step
    HAND_X = hand_plane(); print('hand plane |x| =', round(HAND_X, 3))
    SLEEVE = SHXj - 0.02                                                                      # just inside the shoulder socket: the sleeve wraps the torso side
    cut((0, HEADj + 0.02, 0), (0, 1, 0))                                                   # head off
    cut((0, HIPj + 0.02, 0), (0, 1, 0), lambda p: abs(p[0]) < HAND_X)                        # legs off the pelvis (not the hands)
    cut((0, 0, 0), (1, 0, 0), lambda p: p[1] < HIPj + 0.03)                                 # left / right leg
    for sx in (-1, 1):
        cut((sx*SLEEVE, 0, 0), (1, 0, 0), lambda p, sx=sx: HIPj - 0.1 < p[1] < SHj + 0.1 and p[0]*sx > 0)   # sleeves off the torso
        cut((sx*HAND_X, 0, 0), (1, 0, 0), lambda p, sx=sx: HIPj - 0.33 < p[1] < HIPj + 0.03 and p[0]*sx > 0)  # hands off the thighs (not the shoes)
    bm.faces.ensure_lookup_table()
    # label every face by the region its centre falls in (connectivity is not trusted: sleeves touch hips,
    # hands touch thighs), then clean up slivers by merging tiny same-label islands into their neighbour
    def region(c):
        cx, cy = c[0], c[1]
        if cy > HEADj + 0.02: return 'Head'
        if cy < HIPj + 0.02:
            if abs(cx) > HAND_X and cy > HIPj - 0.33: return 'ArmL' if cx < 0 else 'ArmR'   # hands hang beside the thighs; shoes are wider but lower
            return 'LegL' if cx < 0 else 'LegR'
        if abs(cx) > SLEEVE: return 'ArmL' if cx < 0 else 'ArmR'
        return 'Body'
    lab = {f.index: region(ucenter(f)) for f in bm.faces}
    total = len(bm.faces)
    for _ in range(3):
        seen = set(); changed = 0
        for f in bm.faces:
            if f.index in seen: continue
            stack = [f]; members = []; seen.add(f.index); L = lab[f.index]
            while stack:
                g = stack.pop(); members.append(g)
                for e in g.edges:
                    for h in e.link_faces:
                        if h.index not in seen and lab[h.index] == L: seen.add(h.index); stack.append(h)
            if len(members) >= max(40, 0.01 * total): continue
            border = {}
            for g in members:
                for e in g.edges:
                    for h in e.link_faces:
                        if lab[h.index] != L: border[lab[h.index]] = border.get(lab[h.index], 0) + 1
            if border:
                nl = max(border, key=border.get)
                for g in members: lab[g.index] = nl
                changed += len(members)
        if not changed: break
    groups = {}
    for f in bm.faces: groups.setdefault(lab[f.index], []).append(f)
    print('split faces per node', {k: len(v) for k, v in groups.items()})
    pivots = {'Body': (0, 0, 0), 'Head': (0, HEADj, 0), 'LegL': (-HIPXj, HIPj, 0), 'LegR': (HIPXj, HIPj, 0), 'ArmL': (-SHXj, SHj, 0), 'ArmR': (SHXj, SHj, 0)}
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
        for loop in boundary_loops(sub):
            if not (3 <= len(loop) <= 200): continue
            xs = [v.co.x for v in loop]; ys = [v.co.y for v in loop]; zs = [v.co.z for v in loop]
            thick = min(max(xs)-min(xs), max(ys)-min(ys), max(zs)-min(zs)); span = max(max(xs)-min(xs), max(ys)-min(ys), max(zs)-min(zs))
            if thick > 0.012 or span > 0.6: continue
            mats = [f.material_index for v in loop for f in v.link_faces]
            try: sub.faces.new(loop).material_index = max(set(mats), key=mats.count) if mats else 0
            except ValueError: pass
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
    normalize(ob, a.height, a.yaw, a.anchor)
    J = None
    if a.ref or a.auto_joints:
        import paint_ref as PR
        J = (PR.measure_joints_from_ref(ob, a.ref) if a.ref else PR.measure_joints(ob)) if a.hero else None
        if J: print('joints', {k: round(v, 3) for k, v in J.items()})
    if a.ref:
        counts = PR.paint(ob, a.ref, J if a.hero else None); print('painted faces per material', counts)
    elif not a.keep_texture: quantize(ob)
    tris = decimate(ob, a.budget)
    K.shade_smooth(ob, math.radians(50))
    root = K.empty('_root')
    if a.hero:
        for part in split_hero(ob, J if a.auto_joints else None): K.parent(part, root)
        root.name = 'Hero'
    else:
        K.parent(ob, root)
    print(f'{name}: {tris} tris, height {a.height} m, nodes: {[o.name for o in bpy.data.objects if o.type == "MESH"]}')
    K.export_glb(a.dst)

if __name__ == '__main__':
    main()
