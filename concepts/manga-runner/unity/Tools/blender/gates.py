"""Numeric gates + review renders for Ink Run models (headless bpy). Adapted from the
reference-modeling / blender-image-to-3d skills so they run without an MCP viewport.

  from gates import mesh_report, dim_gate, extent_profile, review
Dims are measured in Unity space (x width, y height, z depth)."""
import bpy, bmesh, math, os, json
from mathutils import Vector

def _eval_mesh(ob):
    dg = bpy.context.evaluated_depsgraph_get(); eo = ob.evaluated_get(dg)
    me = eo.to_mesh(); bm = bmesh.new(); bm.from_mesh(me)
    bmesh.ops.transform(bm, matrix=eo.matrix_world, verts=bm.verts); eo.to_mesh_clear()
    return bm

def mesh_report(ob):
    bm = _eval_mesh(ob); tris = quads = ngons = 0
    for f in bm.faces:
        n = len(f.verts); tris += n == 3; quads += n == 4; ngons += n > 4
    r = {'object': ob.name, 'verts': len(bm.verts), 'faces': len(bm.faces), 'tris': tris, 'quads': quads, 'ngons': ngons,
         'non_manifold_edges': sum(1 for e in bm.edges if not e.is_manifold), 'loose_verts': sum(1 for v in bm.verts if not v.link_edges)}
    bm.free(); return r

def bbox_unity(objs):
    lo = Vector((1e9, 1e9, 1e9)); hi = -lo
    for ob in objs:
        bm = _eval_mesh(ob)
        for v in bm.verts:
            p = Vector((v.co.x, v.co.z, -v.co.y))   # Blender -> Unity
            lo = Vector(map(min, lo, p)); hi = Vector(map(max, hi, p))
        bm.free()
    return lo, hi

def dim_gate(name, objs, target_wh_d, tol=0.05):
    """target (w, h, d) in Unity metres; None skips an axis. Returns dict with pass/fail per axis."""
    lo, hi = bbox_unity(objs); size = hi - lo; out = {'part': name, 'size': [round(s, 3) for s in size], 'min_y': round(lo.y, 3)}
    ok = True
    for i, ax in enumerate('whd'):
        t = target_wh_d[i]
        if t is None: continue
        err = (size[i] - t)/t; out[ax] = f'{size[i]:.3f} vs {t:.3f} -> {err*100:+.1f}%'; ok &= abs(err) <= tol
    out['passed'] = ok; return out

def extent_profile(ob, along='y', measure='x', n=8):
    """Cross extent sampled along an axis (Unity letters). A flat list means a tube."""
    ai = 'xyz'.index(along); mi = 'xyz'.index(measure)
    bm = _eval_mesh(ob); pts = [(v.co.x, v.co.z, -v.co.y) for v in bm.verts]; bm.free()
    lo = min(p[ai] for p in pts); hi = max(p[ai] for p in pts); out = []
    for k in range(n):
        a0 = lo + (hi-lo)*k/n; a1 = lo + (hi-lo)*(k+1)/n
        sl = [p[mi] for p in pts if a0 <= p[ai] <= a1]
        out.append(round(max(sl)-min(sl), 3) if sl else None)
    return out

# ---------------------------------------------------------------- review renders (Workbench, headless)
def review(out_dir, views=('front', 'side', 'back', 'threequarter'), res=900, mode='clay', gameplay_px=0, objs=None, tag=''):
    """Clay (grey, studio light) or silhouette (flat black) renders from fixed cameras, orthographic for
    front/side/back. Returns the file list. Cameras in Unity terms: front looks along -z at the hero's face."""
    os.makedirs(out_dir, exist_ok=True)
    sc = bpy.context.scene
    objs = objs or [o for o in sc.objects if o.type in ('MESH', 'CURVE') and not o.get('_cutter')]
    # bounds (Blender space)
    lo = Vector((1e9,)*3); hi = -lo
    for ob in objs:
        bm = _eval_mesh(ob)
        for v in bm.verts: lo = Vector(map(min, lo, v.co)); hi = Vector(map(max, hi, v.co))
        bm.free()
    c = (lo + hi)/2; ext = max(hi - lo)
    sc.render.engine = 'BLENDER_WORKBENCH'; sc.render.resolution_x = sc.render.resolution_y = res; sc.render.film_transparent = False
    sh = sc.display.shading; sh.light = 'STUDIO' if mode == 'clay' else 'FLAT'; sh.color_type = 'SINGLE'
    sh.single_color = (0.55, 0.55, 0.55) if mode == 'clay' else (0.0, 0.0, 0.0); sh.show_object_outline = mode == 'clay'; sh.show_cavity = mode == 'clay'
    sc.display.render_aa = '8'
    try: sc.view_settings.view_transform = 'Standard'
    except Exception: pass
    sc.world = sc.world or bpy.data.worlds.new('W'); sc.world.color = (1, 1, 1)
    cam = bpy.data.objects.get('_ReviewCam') or bpy.data.objects.new('_ReviewCam', bpy.data.cameras.new('_ReviewCam'))
    if cam.name not in sc.objects: sc.collection.objects.link(cam)
    sc.camera = cam
    # Unity-facing: front = camera on +z (Blender -y) looking toward -z
    dirs = {'front': Vector((0, -1, 0)), 'back': Vector((0, 1, 0)), 'side': Vector((1, 0, 0)), 'left': Vector((-1, 0, 0)),
            'top': Vector((0, 0, 1)), 'threequarter': Vector((0.6, -0.7, 0.4)).normalized(), 'game': Vector((0, -0.84, 0.54)).normalized()}
    files = []
    for v in views:
        d = dirs[v]; ortho = v in ('front', 'back', 'side', 'left', 'top')
        cam.data.type = 'ORTHO' if ortho else 'PERSP'
        if ortho: cam.data.ortho_scale = ext * 1.15; cam.location = c + d * (ext*3)
        else: cam.data.lens = 50; cam.location = c + d * (ext*2.4)
        cam.rotation_euler = (c - cam.location).to_track_quat('-Z', 'Y').to_euler()
        cam.data.clip_end = ext*20
        path = os.path.join(out_dir, f'{tag}{mode}_{v}.png'); sc.render.filepath = path
        bpy.ops.render.render(write_still=True); files.append(path)
    if gameplay_px:
        # same views, subject at gameplay pixel height, greyscale readability strip
        sc.render.resolution_x = sc.render.resolution_y = int(gameplay_px*1.15)
        for v in ('front', 'back', 'threequarter'):
            d = dirs[v]; cam.data.type = 'ORTHO' if v != 'threequarter' else 'PERSP'
            if v != 'threequarter': cam.data.ortho_scale = ext*1.15; cam.location = c + d*(ext*3)
            else: cam.data.lens = 50; cam.location = c + d*(ext*2.4)
            cam.rotation_euler = (c - cam.location).to_track_quat('-Z', 'Y').to_euler()
            path = os.path.join(out_dir, f'{tag}{mode}_{v}_{gameplay_px}px.png'); sc.render.filepath = path
            bpy.ops.render.render(write_still=True); files.append(path)
    return files
