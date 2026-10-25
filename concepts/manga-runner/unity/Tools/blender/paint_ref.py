"""Colour an untextured generated character by projecting the front reference drawing onto it, then
measure its joints. Used by import_gen.py (--ref, --auto-joints). Unity coords throughout."""
import math
import numpy as np
import bpy, bmesh
from mathutils import Vector
import inklib as K

NAMES = ['InkPaper', 'InkAccent', 'Ink', 'InkTone', 'InkSkin']

def classify(r, g, b):
    lum = 0.3*r + 0.59*g + 0.11*b; red = r - max(g, b)
    if red > 0.25 and r > 0.4: return 'InkAccent'
    if lum < 0.3: return 'Ink'
    if r > 0.75 and g > 0.62 and b > 0.5 and (r - b) > 0.06 and lum < 0.985: return 'InkSkin'
    if lum < 0.78: return 'InkTone'
    return 'InkPaper'

def load_ref(path):
    """Reference image -> (rgb float array HxWx3, mask of drawn pixels, nearest-drawn-pixel index maps)."""
    from PIL import Image
    from scipy import ndimage
    im = Image.open(path).convert('RGB'); a = np.asarray(im).astype(np.float32)/255.0
    lum = a @ np.array([0.3, 0.59, 0.11], dtype=np.float32)
    sat = a.max(axis=2) - a.min(axis=2)
    ink = (lum < 0.94) | (sat > 0.12)                 # outlines and fills
    ink = ndimage.binary_opening(ink, iterations=2)
    mask = ndimage.binary_fill_holes(ink)              # the whole figure, white regions included
    _, idx = ndimage.distance_transform_edt(~mask, return_indices=True)
    ys, xs = np.where(mask)
    bbox = (xs.min(), xs.max(), ys.min(), ys.max())   # left, right, top, bottom (image rows grow downward)
    return a, mask, idx, bbox

def unity_verts(ob):
    """Vertex positions in Unity coords from the mesh data (no depsgraph: evaluating a freshly imported
    object after data.transform() crashes the bpy wheel)."""
    me = ob.data; n = len(me.vertices)
    co = np.empty(n*3, dtype=np.float32); me.vertices.foreach_get('co', co); co = co.reshape(n, 3)
    M = np.array(ob.matrix_world, dtype=np.float64)
    P_b = np.concatenate([co.astype(np.float64), np.ones((n, 1))], axis=1) @ M.T
    return np.stack([P_b[:, 0], P_b[:, 2], -P_b[:, 1]], axis=1)

def measure_joints(ob):
    """Crotch (hip y), shoulder line, neck, leg and shoulder x from the mesh itself (A-pose, facing +z)."""
    P = unity_verts(ob); y = P[:, 1]; x = P[:, 0]
    h = y.max(); bands = 60
    def band(i): return (y >= h*i/bands) & (y < h*(i+1)/bands)
    # crotch: highest band (from the bottom) where no vertex sits near x = 0
    hip = 0.45*h
    for i in range(bands):
        sel = band(i)
        if sel.sum() < 10: continue
        gap = np.abs(x[sel]).min() > 0.02
        if not gap and i > bands*0.25: hip = h*i/bands; break
    # shoulder line: band where the half-width jumps (arms begin), searched above the hip
    widths = np.array([np.abs(x[band(i)]).max() if band(i).sum() else 0 for i in range(bands)])
    sh_i = int(np.argmax(widths[int(bands*0.55):int(bands*0.85)])) + int(bands*0.55)
    # the top of the arm mass: first band above the widest point where width drops by 25%
    top = sh_i
    for i in range(sh_i, bands):
        if widths[i] < widths[sh_i]*0.75: top = i; break
    SH = h*(top - 0.5)/bands
    # neck: narrowest band between the shoulder top and the head
    neck_i = top + int(np.argmin(widths[top:min(bands, top + int(bands*0.12))]))
    HEAD = h*neck_i/bands
    # x of each leg (mean |x| of vertices in the thigh band) and of the shoulder joint (torso half width at SH)
    thigh = (y > hip*0.7) & (y < hip*0.95); HIP_X = float(np.abs(x[thigh]).mean()) if thigh.any() else 0.11
    chest = (y > SH - 0.08) & (y < SH); torso_half = float(np.percentile(np.abs(x[chest]), 70)) if chest.any() else 0.25
    return {'HIP': float(hip), 'SH': float(SH), 'HEAD': float(HEAD), 'HIP_X': HIP_X, 'SH_X': torso_half, 'ARM_PLANE': torso_half + 0.02}

def paint(ob, ref_path, joints):
    """Assign one of the five ink materials to every face from the projected reference colour, with
    back-of-head and hood rules where the drawing cannot see."""
    a, mask, idx, bbox = load_ref(ref_path)
    H, W = mask.shape; left, right, top, bottom = bbox
    me = ob.data
    bm = bmesh.new(); bm.from_mesh(me); bm.faces.ensure_lookup_table()
    M = ob.matrix_world.copy()
    P = unity_verts(ob); minx, maxx = P[:, 0].min(), P[:, 0].max(); miny, maxy = P[:, 1].min(), P[:, 1].max()
    figure = joints is not None                      # character rules (hair, hood, legs) only for a jointed figure
    HEAD, SH, HIP = (joints['HEAD'], joints['SH'], joints['HIP']) if figure else (maxy + 1, maxy + 1, miny - 1)
    me.materials.clear()
    for n in NAMES: me.materials.append(K.mat(n))
    counts = {n: 0 for n in NAMES}
    R = max(3, int((right - left) * 0.012))      # sampling window half-size: about 1.2% of the figure width
    # what colour is the upper torso in the drawing? the hood-from-behind rule only applies to a red hoodie
    tx = (left + right)//2; ty0 = int(bottom - (SH - 0.28 - miny)/(maxy - miny + 1e-9)*(bottom - top)); ty1 = int(bottom - (SH - 0.06 - miny)/(maxy - miny + 1e-9)*(bottom - top))
    tw = a[max(0, min(ty0, ty1)):max(ty0, ty1)+1, max(0, tx-R):tx+R+1].reshape(-1, 3)
    tv = {}
    for r, g, b in tw: k = classify(r, g, b); tv[k] = tv.get(k, 0) + 1
    hoodie = max(tv, key=tv.get) == 'InkAccent' if tv else False
    eye_line = HEAD + 0.5*(maxy - HEAD)                  # below this the side of the head is cheek and jaw, not hair
    assign = {}
    for f in bm.faces:
        c = f.calc_center_median(); cw = M @ c
        ux, uy, uz = cw.x, cw.z, -cw.y
        nrm = f.normal; nz = -nrm.y   # Unity z component of the face normal (Blender -y)
        px = int(left + (ux - minx)/(maxx - minx + 1e-9)*(right - left)); py = int(bottom - (uy - miny)/(maxy - miny + 1e-9)*(bottom - top))
        px = min(max(px, 0), W-1); py = min(max(py, 0), H-1)
        if not mask[py, px]: py, px = idx[0][py, px], idx[1][py, px]
        # vote over a window, ignoring outline strokes unless they dominate (so lines don't speckle regions)
        win = a[max(0, py-R):py+R+1, max(0, px-R):px+R+1].reshape(-1, 3)
        wmask = mask[max(0, py-R):py+R+1, max(0, px-R):px+R+1].reshape(-1)
        win = win[wmask] if wmask.any() else win
        votes = {}
        for r, g, b in win: k = classify(r, g, b); votes[k] = votes.get(k, 0) + 1
        total = sum(votes.values()) or 1
        if votes.get('Ink', 0) < 0.6*total: votes.pop('Ink', None)
        votes.pop('InkTone', None) if votes.get('InkTone', 0) < 0.5*total else None
        n = max(votes, key=votes.get) if votes else 'InkPaper'
        back = nz < -0.15
        if not figure:
            if (abs(nrm.x) > 0.5 or nz < -0.3) and n != 'InkAccent': n = 'InkPaper'   # a prop's side and rear faces are not in the drawing; keep them paper (outlines carry the form)
        elif uy > HEAD + 0.02:
            if back or (nz < 0.1 and uy > eye_line and n in ('InkSkin', 'InkPaper')): n = 'Ink'
        elif hoodie and uy > SH - 0.12 and back and n in ('InkPaper', 'InkSkin'):
            n = 'InkAccent'            # the hood seen from behind is the hoodie's red, not its white lining
        elif uy < HIP and back and n == 'InkSkin':
            n = 'Ink'
        assign[f.index] = NAMES.index(n)
    # two passes of neighbour-majority smoothing to remove single-face speckle
    for _ in range(2):
        nxt = {}
        for f in bm.faces:
            tally = {}
            tally[assign[f.index]] = tally.get(assign[f.index], 0) + 1.5
            for e in f.edges:
                for g in e.link_faces:
                    if g is not f: tally[assign[g.index]] = tally.get(assign[g.index], 0) + 1
            nxt[f.index] = max(tally, key=tally.get)
        assign = nxt
    for f in bm.faces:
        f.material_index = assign[f.index]; counts[NAMES[assign[f.index]]] += 1
    bm.to_mesh(me); bm.free()
    return counts

def measure_joints_from_ref(ob, ref_path):
    """Joints read off the reference drawing's filled silhouette, mapped onto the mesh through the shared
    bounding box. Crotch: the gap between the legs seen from below; neck: the narrowest row of the upper body;
    shoulder: a fixed drop below the neck; torso width: a fixed fraction (sleeves hide it in silhouette)."""
    a, mask, idx, bbox = load_ref(ref_path)
    left, right, top, bottom = bbox
    P = unity_verts(ob); minx, maxx = P[:, 0].min(), P[:, 0].max(); miny, maxy = P[:, 1].min(), P[:, 1].max()
    Hm = maxy - miny
    def to_y(row): return miny + (bottom - row)/(bottom - top)*Hm
    def to_x(col): return minx + (col - left)/(right - left)*(maxx - minx)
    def runs(row):
        cols = np.where(mask[row])[0]
        if len(cols) == 0: return []
        rs = np.split(cols, np.where(np.diff(cols) > 2)[0] + 1)
        return [r for r in rs if len(r) >= 8]
    h = bottom - top
    # legs: the two runs on the ankle row; the gap column between them leads up to the crotch
    ankle = bottom - int(h*0.12); lr = sorted(runs(ankle), key=lambda r: r.mean())
    if len(lr) >= 2:
        l, r = lr[0], lr[-1]; gap_col = int((l.max() + r.min())/2)
        HIP_X = (abs(to_x(l.mean())) + abs(to_x(r.mean())))/2
    else:
        gap_col = (left + right)//2; HIP_X = 0.06*Hm
    crotch = None
    for row in range(ankle, top, -1):
        if mask[row, gap_col]: crotch = row; break
    HIP = to_y(crotch) + 0.015*Hm if crotch else miny + 0.42*Hm
    if not (miny + 0.36*Hm <= HIP <= miny + 0.52*Hm): HIP = miny + 0.44*Hm   # baggy trousers or crossed legs fool the gap scan; a hip outside 36-52% of the height is wrong
    # neck: narrowest row between 60% and 82% of the height (below the hair, above the shoulders)
    widths = [(runs(top+i)[0].max() - runs(top+i)[-1].min() if False else (max(r.max() for r in runs(top+i)) - min(r.min() for r in runs(top+i)))) if runs(top+i) else 10**9 for i in range(h)]
    lo_i, hi_i = int(h*(1-0.82)), int(h*(1-0.60))
    neck_i = min(range(lo_i, hi_i), key=lambda i: widths[i])
    HEAD = to_y(top + neck_i)
    SH = HEAD - 0.068*Hm
    half = 0.105*Hm
    return {'HIP': float(HIP), 'SH': float(SH), 'HEAD': float(HEAD), 'HIP_X': float(HIP_X), 'SH_X': float(half - 0.015), 'ARM_PLANE': float(half + 0.01)}
