"""Ink Run Blender helper library (run with the `bpy` module or inside Blender 4.2+/5.x).

Authoring convention: every coordinate passed to these helpers is in UNITY / glTF space
(x right, y up, z forward = the direction the hero runs). `U()` converts to Blender space
(x, -z, y); the glTF exporter (Y-up) converts back, so what you author is what Unity loads.

Construction helpers follow the MIT-licensed build template from
majidmanzarpour/blender-game-skills (skin chains, lofts, curve tubes, modifier stacks),
adapted for this project's materials and coordinate convention."""
import bpy, bmesh, math, os
from mathutils import Vector, Matrix

# ---------------------------------------------------------------- space
def U(p):
    """Unity (x, y, z) -> Blender (x, -z, y)."""
    return Vector((p[0], -p[2], p[1]))

def U4(x, y, z, r):
    return (x, -z, y, r)

def USize(s):
    """Unity size (w, h, d) -> Blender size (w, d, h)."""
    return (s[0], s[2], s[1])

# ---------------------------------------------------------------- scene / materials
COLORS = {
    'InkPaper': (1.0, 1.0, 1.0, 1), 'InkAccent': (0.878, 0.125, 0.165, 1), 'Ink': (0.07, 0.07, 0.07, 1),
    'InkTone': (0.72, 0.72, 0.72, 1), 'InkSkin': (1.0, 0.95, 0.9, 1),
}

def reset():
    bpy.ops.wm.read_factory_settings(use_empty=True)
    for n, c in COLORS.items():
        m = bpy.data.materials.new(n)
        m.diffuse_color = c
        bsdf = next((x for x in m.node_tree.nodes if x.type == 'BSDF_PRINCIPLED'), None)
        if bsdf:
            bsdf.inputs['Base Color'].default_value = c
            bsdf.inputs['Roughness'].default_value = 0.8
    return bpy.context.scene

def mat(name):
    return bpy.data.materials[name]

def link(ob):
    bpy.context.scene.collection.objects.link(ob)
    return ob

def new_mesh_object(name, bm, material='InkPaper', smooth=True, sharp_deg=35):
    me = bpy.data.meshes.new(name)
    bm.to_mesh(me); bm.free()
    ob = bpy.data.objects.new(name, me)
    me.materials.append(mat(material))
    link(ob)
    if smooth: shade_smooth(ob, math.radians(sharp_deg))
    return ob

def shade_smooth(ob, sharp_angle=math.radians(35)):
    me = ob.data
    bm = bmesh.new(); bm.from_mesh(me)
    for f in bm.faces: f.smooth = True
    for e in bm.edges:
        if len(e.link_faces) == 2: e.smooth = e.calc_face_angle(0.0) <= sharp_angle
    bm.to_mesh(me); bm.free()
    return ob

def shade_flat(ob):
    for p in ob.data.polygons: p.use_smooth = False
    return ob

# ---------------------------------------------------------------- primitives (Unity coords)
def box(name, center, size, material='InkPaper', bevel_w=0.0, segments=2, subsurf_levels=0):
    """Axis-aligned box. bevel_w > 0 adds a Bevel modifier (and optional SubSurf) -> rounded edges."""
    bm = bmesh.new()
    bmesh.ops.create_cube(bm, size=1.0)
    bmesh.ops.scale(bm, vec=USize(size), verts=bm.verts)
    bmesh.ops.translate(bm, vec=U(center), verts=bm.verts)
    ob = new_mesh_object(name, bm, material, smooth=False)
    if bevel_w > 0:
        bevel(ob, bevel_w, segments)
        if subsurf_levels: subsurf(ob, subsurf_levels)
        shade_smooth(ob, math.radians(35))
    return ob

def cylinder(name, center, radius, height, material='InkPaper', axis='y', segments=24, r_top=None, bevel_w=0.0, bevel_segments=2):
    """Capped cylinder along a Unity axis ('x', 'y' or 'z'), centred at `center`."""
    r_top = radius if r_top is None else r_top
    bm = bmesh.new()
    bmesh.ops.create_cone(bm, cap_ends=True, cap_tris=False, segments=segments, radius1=radius, radius2=r_top, depth=height)
    # cone is along local z (Blender) == Unity y
    rot = {'y': Matrix.Identity(4), 'x': Matrix.Rotation(math.radians(90), 4, 'Y'), 'z': Matrix.Rotation(math.radians(90), 4, 'X')}[axis]
    bmesh.ops.transform(bm, matrix=rot, verts=bm.verts)
    bmesh.ops.translate(bm, vec=U(center), verts=bm.verts)
    ob = new_mesh_object(name, bm, material, smooth=True, sharp_deg=40)
    if bevel_w > 0: bevel(ob, bevel_w, bevel_segments, angle_limit=40)
    return ob

def sphere(name, center, radius, material='InkPaper', scale=(1, 1, 1), u=24, v=16):
    bm = bmesh.new()
    bmesh.ops.create_uvsphere(bm, u_segments=u, v_segments=v, radius=radius)
    bmesh.ops.scale(bm, vec=USize(scale), verts=bm.verts)
    bmesh.ops.translate(bm, vec=U(center), verts=bm.verts)
    return new_mesh_object(name, bm, material, smooth=True, sharp_deg=90)

def loft(name, sections, material='InkPaper', cap_ends=True, smooth=True, sharp_deg=50):
    """Bridge cross-sections (lists of Unity points, equal counts) into a hull."""
    bm = bmesh.new()
    rings = []
    for sec in sections:
        vs = [bm.verts.new(U(p)) for p in sec]
        rings.append([bm.edges.new((vs[i], vs[(i+1) % len(vs)])) for i in range(len(vs))])
    for a, b in zip(rings, rings[1:]):
        bmesh.ops.bridge_loops(bm, edges=a + b)
    if cap_ends:
        bmesh.ops.contextual_create(bm, geom=rings[0])
        bmesh.ops.contextual_create(bm, geom=rings[-1])
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    return new_mesh_object(name, bm, material, smooth, sharp_deg)

def ring(center, rx, ry, n=16, plane='xz', squircle=0.0, phase=0.0):
    """Ellipse / rounded-rectangle ring of Unity points. plane: 'xz' (horizontal), 'xy' (facing z), 'yz' (facing x)."""
    pts = []
    for i in range(n):
        a = 2*math.pi*i/n + phase
        c, s = math.cos(a), math.sin(a)
        if squircle > 0:
            k = 2.0/(1.0 + squircle*2.0)
            c = math.copysign(abs(c)**k, c); s = math.copysign(abs(s)**k, s)
        if plane == 'xz': pts.append((center[0] + rx*c, center[1], center[2] + ry*s))
        elif plane == 'xy': pts.append((center[0] + rx*c, center[1] + ry*s, center[2]))
        else: pts.append((center[0], center[1] + ry*s, center[2] + rx*c))
    return pts

def spin(name, profile, material='InkPaper', steps=32, center=(0, 0, 0), axis='y', smooth=True):
    """Lathe a profile [(radius, h), ...] around a Unity axis through `center`."""
    bm = bmesh.new()
    verts = [bm.verts.new((r, 0, h)) for r, h in profile]
    edges = [bm.edges.new((verts[i], verts[i+1])) for i in range(len(verts)-1)]
    bmesh.ops.spin(bm, geom=verts + edges, cent=(0, 0, 0), axis=(0, 0, 1), angle=2*math.pi, steps=steps, use_merge=True)
    bmesh.ops.remove_doubles(bm, verts=bm.verts, dist=1e-5)
    rot = {'y': Matrix.Identity(4), 'x': Matrix.Rotation(math.radians(90), 4, 'Y'), 'z': Matrix.Rotation(math.radians(90), 4, 'X')}[axis]
    bmesh.ops.transform(bm, matrix=rot, verts=bm.verts)
    bmesh.ops.translate(bm, vec=U(center), verts=bm.verts)
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    return new_mesh_object(name, bm, material, smooth, 50)

def torus(name, center, R, r, material='InkPaper', axis='y', steps=32, tube=12, angle=360.0, tilt=0.0):
    """Torus (or arc of one) around a Unity axis through `center`. `tilt` rotates about Unity x (degrees)."""
    bm = bmesh.new()
    verts = [bm.verts.new((R + r*math.cos(t), 0, r*math.sin(t))) for t in [2*math.pi*i/tube for i in range(tube)]]
    edges = [bm.edges.new((verts[i], verts[(i+1) % tube])) for i in range(tube)]
    full = angle >= 359.9
    bmesh.ops.spin(bm, geom=verts + edges, cent=(0, 0, 0), axis=(0, 0, 1), angle=math.radians(angle), steps=steps, use_merge=full)
    if full: bmesh.ops.remove_doubles(bm, verts=bm.verts, dist=1e-5)
    else:
        bmesh.ops.contextual_create(bm, geom=[e for e in bm.edges if len(e.link_faces) == 1 and e.verts[0].co.z == e.verts[0].co.z])
    rot = {'y': Matrix.Identity(4), 'x': Matrix.Rotation(math.radians(90), 4, 'Y'), 'z': Matrix.Rotation(math.radians(90), 4, 'X')}[axis]
    bmesh.ops.transform(bm, matrix=Matrix.Rotation(math.radians(tilt), 4, 'X') @ rot, verts=bm.verts)
    bmesh.ops.translate(bm, vec=U(center), verts=bm.verts)
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    return new_mesh_object(name, bm, material, True, 60)

def extrude_profile(name, outline, depth, center=(0, 0, 0), material='InkPaper', plane='xy', bevel_w=0.0):
    """Extrude a closed 2D outline [(a, b), ...] by `depth` along the plane's normal (Unity space)."""
    bm = bmesh.new()
    if plane == 'xy': verts = [bm.verts.new(U((a, b, -depth/2))) for a, b in outline]; vec = U((0, 0, depth)) - U((0, 0, 0))
    elif plane == 'xz': verts = [bm.verts.new(U((a, -depth/2, b))) for a, b in outline]; vec = U((0, depth, 0)) - U((0, 0, 0))
    else: verts = [bm.verts.new(U((-depth/2, b, a))) for a, b in outline]; vec = U((depth, 0, 0)) - U((0, 0, 0))
    face = bm.faces.new(verts)
    res = bmesh.ops.extrude_face_region(bm, geom=[face])
    top = [g for g in res['geom'] if isinstance(g, bmesh.types.BMVert)]
    bmesh.ops.translate(bm, vec=vec, verts=top)
    bmesh.ops.translate(bm, vec=U(center), verts=bm.verts)
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    ob = new_mesh_object(name, bm, material, smooth=False)
    if bevel_w > 0: bevel(ob, bevel_w, 2); shade_smooth(ob, math.radians(35))
    return ob

def tube(name, points, radius, material='InkPaper', taper_to=None, resolution=12, bevel_res=6, caps=True, radii=None):
    """Smooth NURBS tube through Unity points: cables, hair spikes, pipes, rails, drawstrings."""
    cu = bpy.data.curves.new(name, 'CURVE'); cu.dimensions = '3D'
    sp = cu.splines.new('NURBS'); sp.points.add(len(points)-1)
    for i, p in enumerate(points):
        b = U(p); sp.points[i].co = (b.x, b.y, b.z, 1.0)
        if radii is not None: sp.points[i].radius = radii[i]
        elif taper_to is not None:
            t = i/max(1, len(points)-1); sp.points[i].radius = 1.0 + (taper_to-1.0)*t
    sp.use_endpoint_u = True; sp.order_u = min(4, len(points))
    cu.resolution_u = resolution; cu.bevel_depth = radius; cu.bevel_resolution = bevel_res; cu.use_fill_caps = caps
    cu.materials.append(mat(material))
    ob = bpy.data.objects.new(name, cu); link(ob)
    return ob

def cone_sweep(name, pts, radii, material='Ink', n=10, steps=7):
    """Closed, manifold tapered spike/lock along a quadratic bezier through 3 Unity points.
    radii: (base, mid, tip) radius multipliers of the base radius; the tip collapses to one vertex."""
    from mathutils import Vector as V
    p0, p1, p2 = (V(U(p)) for p in pts)
    path = [(1-t)**2*p0 + 2*(1-t)*t*p1 + t*t*p2 for t in [i/(steps-1) for i in range(steps)]]
    bm = bmesh.new(); rings = []
    for i, c in enumerate(path):
        t = i/(steps-1)
        tang = (path[min(i+1, steps-1)] - path[max(i-1, 0)]).normalized()
        r = radii[0]*((1-t)**2) + radii[1]*2*(1-t)*t + radii[2]*t*t if len(radii) == 3 else radii[0]*(1-t)
        if i == steps-1: r = max(r, 0.0005)
        ref = V((0, 0, 1)) if abs(tang.z) < 0.9 else V((1, 0, 0))
        u = tang.cross(ref).normalized(); v = u.cross(tang)
        vs = [bm.verts.new(c + (u*math.cos(a) + v*math.sin(a))*r) for a in [2*math.pi*k/n for k in range(n)]]
        rings.append([bm.edges.new((vs[k], vs[(k+1) % n])) for k in range(n)])
    for a, b in zip(rings, rings[1:]): bmesh.ops.bridge_loops(bm, edges=a + b)
    bmesh.ops.contextual_create(bm, geom=rings[0])
    tipverts = list({v for e in rings[-1] for v in e.verts})
    bmesh.ops.pointmerge(bm, verts=tipverts, merge_co=path[-1])
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    return new_mesh_object(name, bm, material, True, 60)

def sweep_loft(name, pts, radii, material='InkPaper', n=20, steps=9, cap_start=True, cap_end=True, subsurf_levels=1):
    """One smooth lofted tube along a quadratic bezier through 3 Unity points, radius interpolated from
    `radii` (a list sampled along t). Use for bent sleeves/legs so there is no seam at the joint."""
    from mathutils import Vector as V
    p0, p1, p2 = (V(U(p)) for p in pts)
    path = [(1-t)**2*p0 + 2*(1-t)*t*p1 + t*t*p2 for t in [i/(steps-1) for i in range(steps)]]
    bm = bmesh.new(); rings = []
    for i, c in enumerate(path):
        t = i/(steps-1); f = t*(len(radii)-1); k = min(int(f), len(radii)-2); r = radii[k] + (radii[k+1]-radii[k])*(f-k)
        tang = (path[min(i+1, steps-1)] - path[max(i-1, 0)]).normalized()
        ref = V((0, 0, 1)) if abs(tang.z) < 0.9 else V((1, 0, 0))
        u = tang.cross(ref).normalized(); v = u.cross(tang)
        vs = [bm.verts.new(c + (u*math.cos(a) + v*math.sin(a))*r) for a in [2*math.pi*j/n for j in range(n)]]
        rings.append([bm.edges.new((vs[j], vs[(j+1) % n])) for j in range(n)])
    for a, b in zip(rings, rings[1:]): bmesh.ops.bridge_loops(bm, edges=a + b)
    if cap_start: bmesh.ops.contextual_create(bm, geom=rings[0])
    if cap_end: bmesh.ops.contextual_create(bm, geom=rings[-1])
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    ob = new_mesh_object(name, bm, material, True, 90)
    if subsurf_levels: subsurf(ob, subsurf_levels)
    return ob

def skin_chain(name, chains, material='InkSkin', subdiv=2, root=0):
    """Organic mass from joint chains [(x, y, z, r), ...] (Unity coords) or a dict of chains that
    share points by position. Skin + Subdivision gives smooth limbs with loops at every joint."""
    me = bpy.data.meshes.new(name); bm = bmesh.new()
    chains = chains if isinstance(chains, dict) else {'main': chains}
    by_pos, order = {}, []
    for pts in chains.values():
        prev = None
        for (x, y, z, r) in pts:
            b = U((x, y, z)); key = (round(b.x, 5), round(b.y, 5), round(b.z, 5))
            v = by_pos.get(key)
            if v is None:
                v = bm.verts.new(b); by_pos[key] = v; order.append((v, r))
            if prev is not None and prev is not v and not bm.edges.get((prev, v)): bm.edges.new((prev, v))
            prev = v
    bm.verts.index_update(); bm.to_mesh(me); bm.free()
    ob = bpy.data.objects.new(name, me); link(ob)
    me.materials.append(mat(material))
    skin = ob.modifiers.new('Skin', 'SKIN'); skin.use_smooth_shade = True
    layer = me.skin_vertices[0].data
    for i, (v, r) in enumerate(order):
        rr = r if isinstance(r, tuple) else (r, r)
        layer[i].radius = rr; layer[i].use_root = (i == root)
    subsurf(ob, subdiv)
    return ob

# ---------------------------------------------------------------- modifiers
def bevel(ob, width=0.01, segments=2, angle_limit=30, harden=True):
    b = ob.modifiers.new('Bevel', 'BEVEL'); b.width = width; b.segments = segments
    b.limit_method = 'ANGLE'; b.angle_limit = math.radians(angle_limit); b.harden_normals = harden
    return b

def subsurf(ob, levels=2):
    s = ob.modifiers.new('Subdivision', 'SUBSURF'); s.levels = levels; s.render_levels = levels; return s

def solidify(ob, thickness=0.01, offset=1.0, rim=True):
    s = ob.modifiers.new('Solidify', 'SOLIDIFY'); s.thickness = thickness; s.offset = offset; s.use_rim = rim; return s

def mirror_x(ob, merge=0.001):
    m = ob.modifiers.new('Mirror', 'MIRROR'); m.use_axis = (True, False, False); m.use_mirror_merge = merge > 0
    m.merge_threshold = merge; m.use_clip = True
    return m

def boolean(ob, cutter, op='DIFFERENCE'):
    b = ob.modifiers.new('Bool_' + cutter.name, 'BOOLEAN'); b.operation = op; b.object = cutter; b.solver = 'EXACT'
    cutter.hide_render = True; cutter.hide_viewport = True; cutter.display_type = 'WIRE'
    cutter['_cutter'] = True
    return b

def array(ob, count, offset):
    """Array along a Unity offset vector (absolute metres)."""
    a = ob.modifiers.new('Array', 'ARRAY'); a.count = count; a.use_relative_offset = False; a.use_constant_offset = True
    a.constant_offset_displace = U(offset)
    return a

def displace(ob, strength):
    """Inflate/deflate along normals (used for clothing shells over a body)."""
    d = ob.modifiers.new('Displace', 'DISPLACE'); d.strength = strength; d.mid_level = 0.0; return d

def apply_all(ob):
    """Bake the modifier stack (and curve geometry) into a plain mesh."""
    dg = bpy.context.evaluated_depsgraph_get()
    eo = ob.evaluated_get(dg)
    me = bpy.data.meshes.new_from_object(eo)
    if ob.type != 'MESH':
        # curve -> mesh: rebuild as a mesh object with the same name/material
        mats = [m for m in ob.data.materials]
        new = bpy.data.objects.new(ob.name + '_m', me); link(new)
        for m in mats: me.materials.append(m)
        new.matrix_world = ob.matrix_world
        bpy.data.objects.remove(ob)
        new.name = new.name[:-2]
        bm = bmesh.new(); bm.from_mesh(me); bmesh.ops.remove_doubles(bm, verts=bm.verts, dist=1e-5); bm.to_mesh(me); bm.free()
        for p in me.polygons: p.use_smooth = True
        return new
    old = ob.data; ob.data = me; ob.modifiers.clear()
    if old.users == 0: bpy.data.meshes.remove(old)
    return ob

def transform(ob, pivot, axis='y', deg=0.0, scale=(1, 1, 1), move=(0, 0, 0)):
    """Rotate `deg` about a Unity axis through `pivot`, scale about pivot, then translate (all Unity)."""
    ax = {'x': 'X', 'y': 'Z', 'z': 'Y'}[axis]
    sign = -1 if axis == 'z' else 1   # Unity z maps to Blender -y
    P = U(pivot)
    M = Matrix.Translation(P + (U(move) - U((0, 0, 0)))) @ Matrix.Rotation(math.radians(deg)*sign, 4, ax) @ Matrix.Diagonal((*USize(scale), 1)) @ Matrix.Translation(-P)
    ob.matrix_world = M @ ob.matrix_world
    return ob

# ---------------------------------------------------------------- assembly
def join(name, objects, origin=(0, 0, 0)):
    """Apply modifiers on every part, merge them into one mesh object with per-face materials,
    origin at `origin` (Unity). Sources are removed."""
    me_out = bpy.data.meshes.new(name)
    bm = bmesh.new()
    slots = []
    for ob in objects:
        if ob.get('_cutter'): bpy.data.objects.remove(ob); continue
        ob = apply_all(ob)
        me = ob.data
        tmp = bmesh.new(); tmp.from_mesh(me)
        bmesh.ops.transform(tmp, matrix=ob.matrix_world, verts=tmp.verts)
        # remap material indices into the joined slot list
        remap = {}
        for i, m in enumerate(me.materials):
            if m not in slots: slots.append(m)
            remap[i] = slots.index(m)
        for f in tmp.faces: f.material_index = remap.get(f.material_index, 0)
        tmp_me = bpy.data.meshes.new('_tmp'); tmp.to_mesh(tmp_me); tmp.free()
        bm.from_mesh(tmp_me); bpy.data.meshes.remove(tmp_me)
        bpy.data.objects.remove(ob)
    O = U(origin)
    bmesh.ops.translate(bm, vec=-O, verts=bm.verts)
    bm.to_mesh(me_out); bm.free()
    for m in slots: me_out.materials.append(m)
    out = bpy.data.objects.new(name, me_out); link(out)
    out.location = O
    return out

def empty(name, location=(0, 0, 0)):
    e = bpy.data.objects.new(name, None); link(e); e.location = U(location); return e

def parent(child, parent_ob):
    child.parent = parent_ob
    child.matrix_parent_inverse = parent_ob.matrix_world.inverted()
    return child

def cleanup_cutters():
    for ob in list(bpy.data.objects):
        if ob.get('_cutter'): bpy.data.objects.remove(ob)

def tri_count():
    dg = bpy.context.evaluated_depsgraph_get(); n = 0
    for ob in bpy.data.objects:
        if ob.type == 'MESH' and not ob.hide_render:
            me = ob.evaluated_get(dg).to_mesh()
            n += sum(len(p.vertices) - 2 for p in me.polygons)
    return n

def export_glb(path):
    cleanup_cutters()
    os.makedirs(os.path.dirname(os.path.abspath(path)), exist_ok=True)
    bpy.ops.export_scene.gltf(filepath=path, export_format='GLB', export_apply=True, export_yup=True,
                              export_materials='EXPORT', export_image_format='NONE', export_normals=True,
                              export_texcoords=False, export_cameras=False, export_lights=False)
    return path
