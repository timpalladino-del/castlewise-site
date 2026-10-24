#!/usr/bin/env python3
"""Procedural low-poly toon models for Ink Run, exported as GLB.

Every model is built from a few shapes, flat-shaded, with two to four materials named so
the Unity scene builder can swap them for the ink shader: InkPaper, InkAccent, Ink, InkTone.
The hero is a node hierarchy whose joint nodes (Body, Head, LegL, LegR, ArmL, ArmR) match
Player/HeroAnimator.cs, so the existing procedural run cycle animates it.

Run: python3 build_models.py <output dir>
"""
import json, math, struct, sys, os

# ---------------- geometry ----------------

class Part:
    """Triangles for one material."""
    def __init__(self, material):
        self.material = material
        self.tris = []  # list of (p0, p1, p2), each a 3-tuple

    def add(self, tris):
        self.tris.extend(tris)
        return self

def v_add(a, b): return (a[0]+b[0], a[1]+b[1], a[2]+b[2])
def v_sub(a, b): return (a[0]-b[0], a[1]-b[1], a[2]-b[2])
def v_cross(a, b): return (a[1]*b[2]-a[2]*b[1], a[2]*b[0]-a[0]*b[2], a[0]*b[1]-a[1]*b[0])
def v_norm(a):
    l = math.sqrt(a[0]**2+a[1]**2+a[2]**2) or 1.0
    return (a[0]/l, a[1]/l, a[2]/l)

def quad(a, b, c, d):
    return [(a, b, c), (a, c, d)]

def box(cx, cy, cz, w, h, d):
    x0, x1 = cx-w/2, cx+w/2; y0, y1 = cy-h/2, cy+h/2; z0, z1 = cz-d/2, cz+d/2
    p = [(x0,y0,z0),(x1,y0,z0),(x1,y1,z0),(x0,y1,z0),(x0,y0,z1),(x1,y0,z1),(x1,y1,z1),(x0,y1,z1)]
    t = []
    t += quad(p[0],p[3],p[2],p[1])  # back (-z)
    t += quad(p[4],p[5],p[6],p[7])  # front (+z)
    t += quad(p[0],p[1],p[5],p[4])  # bottom
    t += quad(p[3],p[7],p[6],p[2])  # top
    t += quad(p[0],p[4],p[7],p[3])  # left
    t += quad(p[1],p[2],p[6],p[5])  # right
    return t

def frustum_box(cx, cy, cz, w0, d0, w1, d1, h):
    """Box tapered from (w0,d0) at the bottom to (w1,d1) at the top."""
    y0, y1 = cy-h/2, cy+h/2
    b = [(cx-w0/2,y0,cz-d0/2),(cx+w0/2,y0,cz-d0/2),(cx+w0/2,y0,cz+d0/2),(cx-w0/2,y0,cz+d0/2)]
    tp = [(cx-w1/2,y1,cz-d1/2),(cx+w1/2,y1,cz-d1/2),(cx+w1/2,y1,cz+d1/2),(cx-w1/2,y1,cz+d1/2)]
    t = []
    t += quad(b[0],b[3],b[2],b[1])
    t += quad(tp[0],tp[1],tp[2],tp[3])
    for i in range(4):
        j = (i+1) % 4
        t += quad(b[i],b[j],tp[j],tp[i])
    return fix_winding(t, (cx, cy, cz))

def rot(p, axis, ang):
    c, s = math.cos(ang), math.sin(ang)
    x, y, z = p
    if axis == 'x': return (x, y*c - z*s, y*s + z*c)
    if axis == 'y': return (x*c + z*s, y, -x*s + z*c)
    return (x*c - y*s, x*s + y*c, z)

def transform(tris, axis=None, ang=0.0, offset=(0,0,0), scale=(1,1,1)):
    out = []
    for tri in tris:
        q = []
        for p in tri:
            p = (p[0]*scale[0], p[1]*scale[1], p[2]*scale[2])
            if axis: p = rot(p, axis, ang)
            q.append(v_add(p, offset))
        out.append(tuple(q))
    return out

def cylinder(r0, r1, h, segs=16, caps=True):
    """Along +y, base at y=0, top at y=h. r0 bottom radius, r1 top radius."""
    t = []
    for i in range(segs):
        a0, a1 = 2*math.pi*i/segs, 2*math.pi*(i+1)/segs
        b0 = (r0*math.cos(a0), 0, r0*math.sin(a0)); b1 = (r0*math.cos(a1), 0, r0*math.sin(a1))
        t0 = (r1*math.cos(a0), h, r1*math.sin(a0)); t1 = (r1*math.cos(a1), h, r1*math.sin(a1))
        t += quad(b0, t0, t1, b1)
        if caps:
            if r0 > 0: t.append(((0,0,0), b0, b1))
            if r1 > 0: t.append(((0,h,0), t1, t0))
    return t

def sphere(r, segs=12, rings=8, phi0=0.0, phi1=math.pi):
    """UV sphere centered at origin. phi is measured from the top (+y)."""
    t = []
    for j in range(rings):
        p0 = phi0 + (phi1-phi0)*j/rings; p1 = phi0 + (phi1-phi0)*(j+1)/rings
        for i in range(segs):
            a0, a1 = 2*math.pi*i/segs, 2*math.pi*(i+1)/segs
            def P(p, a): return (r*math.sin(p)*math.cos(a), r*math.cos(p), r*math.sin(p)*math.sin(a))
            A, B, C, D = P(p0,a0), P(p0,a1), P(p1,a1), P(p1,a0)
            if j == 0 and phi0 == 0: t.append((A, C, D))
            elif j == rings-1 and phi1 >= math.pi-1e-6: t.append((A, B, C))
            else: t += quad(A, B, C, D)
    return t

def torus(R, r, segs=20, tube=8, a0=0.0, a1=2*math.pi):
    """Torus in the xz-plane around +y, optionally a partial arc from a0 to a1."""
    t = []
    for i in range(segs):
        u0 = a0 + (a1-a0)*i/segs; u1 = a0 + (a1-a0)*(i+1)/segs
        for j in range(tube):
            w0 = 2*math.pi*j/tube; w1 = 2*math.pi*(j+1)/tube
            def P(u, w): return ((R + r*math.cos(w))*math.cos(u), r*math.sin(w), (R + r*math.cos(w))*math.sin(u))
            t += quad(P(u0,w0), P(u1,w0), P(u1,w1), P(u0,w1))
    return t

def extrude(poly, depth):
    """Extrude a 2D polygon (x,y) along z from -depth/2 to +depth/2; polygon is convex-fanned."""
    z0, z1 = -depth/2, depth/2
    t = []
    n = len(poly)
    for i in range(1, n-1):
        a, b, c = poly[0], poly[i], poly[i+1]
        t.append(((a[0],a[1],z1), (b[0],b[1],z1), (c[0],c[1],z1)))
        t.append(((a[0],a[1],z0), (c[0],c[1],z0), (b[0],b[1],z0)))
    for i in range(n):
        a, b = poly[i], poly[(i+1) % n]
        t += quad((a[0],a[1],z0), (b[0],b[1],z0), (b[0],b[1],z1), (a[0],a[1],z1))
    return t

def fix_winding(tris, inside_point=None):
    """Make faces point away from a center so flat normals are consistent."""
    if inside_point is None:
        xs = [p[0] for tri in tris for p in tri]; ys = [p[1] for tri in tris for p in tri]; zs = [p[2] for tri in tris for p in tri]
        inside_point = (sum(xs)/len(xs), sum(ys)/len(ys), sum(zs)/len(zs))
    out = []
    for a, b, c in tris:
        n = v_cross(v_sub(b, a), v_sub(c, a))
        center = ((a[0]+b[0]+c[0])/3, (a[1]+b[1]+c[1])/3, (a[2]+b[2]+c[2])/3)
        if sum(n[i]*(center[i]-inside_point[i]) for i in range(3)) < 0: out.append((a, c, b))
        else: out.append((a, b, c))
    return out

# ---------------- materials ----------------

MATERIALS = {
    'InkPaper':  (1.0, 1.0, 1.0),
    'InkAccent': (0.878, 0.125, 0.165),
    'Ink':       (0.07, 0.07, 0.07),
    'InkTone':   (0.72, 0.72, 0.72),
}
MAT_INDEX = {k: i for i, k in enumerate(MATERIALS)}

# ---------------- glb ----------------

class Node:
    def __init__(self, name, translation=(0,0,0), parts=None, children=None):
        self.name = name; self.translation = translation; self.parts = parts or []; self.children = children or []

def write_glb(root_nodes, path):
    bufs = bytearray(); accessors = []; views = []; meshes = []; nodes = []

    def pad():
        while len(bufs) % 4: bufs.extend(b'\0')

    def add_view(data, target):
        pad(); off = len(bufs); bufs.extend(data); views.append({'buffer': 0, 'byteOffset': off, 'byteLength': len(data), 'target': target}); return len(views)-1

    def add_mesh(parts, name):
        prims = []
        for part in parts:
            if not part.tris: continue
            pos = []; nrm = []; idx = []
            for a, b, c in part.tris:
                n = v_norm(v_cross(v_sub(b, a), v_sub(c, a)))
                base = len(pos)
                pos += [a, b, c]; nrm += [n, n, n]; idx += [base, base+1, base+2]
            pdata = b''.join(struct.pack('<3f', *p) for p in pos)
            ndata = b''.join(struct.pack('<3f', *p) for p in nrm)
            idata = b''.join(struct.pack('<I', i) for i in idx)
            pv = add_view(pdata, 34962); nv = add_view(ndata, 34962); iv = add_view(idata, 34963)
            mn = [min(p[i] for p in pos) for i in range(3)]; mx = [max(p[i] for p in pos) for i in range(3)]
            accessors.append({'bufferView': pv, 'componentType': 5126, 'count': len(pos), 'type': 'VEC3', 'min': mn, 'max': mx}); pa = len(accessors)-1
            accessors.append({'bufferView': nv, 'componentType': 5126, 'count': len(nrm), 'type': 'VEC3'}); na = len(accessors)-1
            accessors.append({'bufferView': iv, 'componentType': 5125, 'count': len(idx), 'type': 'SCALAR'}); ia = len(accessors)-1
            prims.append({'attributes': {'POSITION': pa, 'NORMAL': na}, 'indices': ia, 'material': MAT_INDEX[part.material]})
        if not prims: return None
        meshes.append({'name': name, 'primitives': prims}); return len(meshes)-1

    def add_node(n):
        node = {'name': n.name}
        if any(n.translation): node['translation'] = list(n.translation)
        m = add_mesh(n.parts, n.name)
        if m is not None: node['mesh'] = m
        idx = len(nodes); nodes.append(node)
        kids = [add_node(c) for c in n.children]
        if kids: node['children'] = kids
        return idx

    roots = [add_node(n) for n in root_nodes]
    pad()
    gltf = {
        'asset': {'version': '2.0', 'generator': 'InkRun build_models.py'},
        'scene': 0, 'scenes': [{'nodes': roots}], 'nodes': nodes, 'meshes': meshes,
        'materials': [{'name': k, 'pbrMetallicRoughness': {'baseColorFactor': [*v, 1.0], 'metallicFactor': 0.0, 'roughnessFactor': 1.0}} for k, v in MATERIALS.items()],
        'accessors': accessors, 'bufferViews': views, 'buffers': [{'byteLength': len(bufs)}],
    }
    js = json.dumps(gltf, separators=(',', ':')).encode()
    while len(js) % 4: js += b' '
    total = 12 + 8 + len(js) + 8 + len(bufs)
    with open(path, 'wb') as f:
        f.write(struct.pack('<4sII', b'glTF', 2, total))
        f.write(struct.pack('<I4s', len(js), b'JSON')); f.write(js)
        f.write(struct.pack('<I4s', len(bufs), b'BIN\0')); f.write(bufs)

# ---------------- models ----------------

def P(mat, tris): return Part(mat).add(tris)

def hero():
    """1.7 m tall, feet at y 0, facing +z. Joint nodes match HeroAnimator."""
    # body
    torso = frustum_box(0, 1.07, 0, 0.36, 0.22, 0.46, 0.27, 0.42)
    hem = box(0, 0.875, 0, 0.37, 0.05, 0.23)
    pocket = box(0, 0.98, 0.125, 0.22, 0.09, 0.03)
    strings = transform(cylinder(0.012, 0.012, 0.16), offset=(-0.05, 1.08, 0.135)) + transform(cylinder(0.012, 0.012, 0.16), offset=(0.05, 1.08, 0.135))
    hood = transform(sphere(0.17, 14, 7, 0.0, math.pi*0.55), offset=(0, 1.2, -0.13), scale=(1.3, 0.6, 0.9))
    hood_rim = transform(torus(0.16, 0.035, 18, 6, 0, math.pi), axis='y', ang=0, offset=(0, 1.29, -0.08))
    # kanji 望 on the back as raised paper strokes (simplified)
    k = []
    for (x, y, w, h) in [(-0.06, 1.17, 0.08, 0.02), (-0.06, 1.12, 0.08, 0.02), (-0.06, 1.145, 0.02, 0.07), (0.06, 1.14, 0.02, 0.1), (0.045, 1.19, 0.06, 0.02), (0, 1.0, 0.16, 0.02), (0, 1.06, 0.02, 0.1), (-0.05, 1.03, 0.02, 0.06), (0.05, 1.03, 0.02, 0.06)]:
        k += box(x, y, -0.123, w, h, 0.012)
    body = Node('Body', (0, 0, 0), [P('InkAccent', torso + hood + hood_rim), P('Ink', hem), P('InkPaper', pocket + strings + k)])

    # head (node at neck base)
    neck = cylinder(0.055, 0.06, 0.09)
    skull = transform(sphere(0.23, 16, 10), offset=(0, 0.31, 0.0), scale=(1.0, 1.05, 1.0))
    jaw = transform(sphere(0.19, 14, 8, math.pi*0.45, math.pi), offset=(0, 0.3, 0.02), scale=(0.95, 1.0, 0.95))
    ears = transform(sphere(0.035, 8, 5), offset=(-0.22, 0.3, 0)) + transform(sphere(0.035, 8, 5), offset=(0.22, 0.3, 0))
    eyes = box(-0.075, 0.31, 0.21, 0.035, 0.055, 0.02) + box(0.075, 0.31, 0.21, 0.035, 0.055, 0.02)
    brows = box(-0.075, 0.365, 0.215, 0.07, 0.012, 0.015) + box(0.075, 0.365, 0.215, 0.07, 0.012, 0.015)
    mouth = box(0.03, 0.21, 0.22, 0.08, 0.014, 0.012)
    nose = box(0.0, 0.27, 0.23, 0.02, 0.03, 0.02)
    hair_cap = transform(sphere(0.245, 16, 8, 0.0, math.pi*0.5), offset=(0, 0.33, -0.03), scale=(1.0, 1.0, 1.05)) + transform(sphere(0.245, 16, 8, math.pi*0.5, math.pi*0.8), offset=(0, 0.33, -0.09), scale=(1.0, 1.0, 0.7))
    spikes = []
    for i, (ang, tilt, hgt) in enumerate([(0.0, 0.35, 0.22), (0.9, 0.5, 0.2), (-0.9, 0.5, 0.2), (1.9, 0.75, 0.17), (-1.9, 0.75, 0.17), (2.9, 0.9, 0.16), (-2.9, 0.9, 0.16), (0.45, 0.1, 0.25), (-0.45, 0.15, 0.24)]):
        c = cylinder(0.055, 0.0, hgt, 8)
        c = transform(c, axis='z', ang=-tilt * math.cos(ang))
        c = transform(c, axis='x', ang=-tilt * math.sin(ang) * 0.6)
        spikes += transform(c, offset=(0.2*math.sin(ang)*tilt, 0.47 + 0.02*math.cos(ang), -0.06 - 0.18*math.cos(ang)*tilt*0.5))
    strap = transform(torus(0.24, 0.02, 24, 6), offset=(0, 0.42, 0.0))
    buckle = box(0, 0.42, -0.24, 0.05, 0.05, 0.02)
    lens_l = transform(cylinder(0.055, 0.055, 0.03, 12), axis='x', ang=-math.pi/2 + 0.6, offset=(-0.08, 0.46, 0.19))
    lens_r = transform(cylinder(0.055, 0.055, 0.03, 12), axis='x', ang=-math.pi/2 + 0.6, offset=(0.08, 0.46, 0.19))
    lens_rim = transform(torus(0.055, 0.012, 14, 5), axis='x', ang=-math.pi/2 + 0.6 + math.pi/2, offset=(-0.08, 0.46, 0.19)) + transform(torus(0.055, 0.012, 14, 5), axis='x', ang=-math.pi/2 + 0.6 + math.pi/2, offset=(0.08, 0.46, 0.19))
    head = Node('Head', (0, 1.28, 0), [P('InkPaper', neck + skull + jaw + ears), P('Ink', eyes + brows + mouth + hair_cap + spikes + strap + buckle + lens_rim), P('InkAccent', lens_l + lens_r), P('InkTone', nose)])

    def leg(name, x):
        thigh = transform(cylinder(0.085, 0.07, 0.42, 12), axis='x', ang=math.pi, offset=(0, 0, 0))
        knee = transform(sphere(0.07, 10, 6), offset=(0, -0.42, 0))
        shin = transform(cylinder(0.065, 0.052, 0.38, 12), axis='x', ang=math.pi, offset=(0, -0.42, 0))
        cuff = transform(torus(0.06, 0.02, 12, 5), offset=(0, -0.79, 0))
        sock = transform(cylinder(0.05, 0.05, 0.06, 10), offset=(0, -0.86, 0))
        shoe = box(0, -0.865, 0.05, 0.15, 0.1, 0.27) + transform(sphere(0.075, 10, 6), offset=(0, -0.865, 0.17), scale=(1.0, 0.9, 1.0))
        sole = box(0, -0.915, 0.05, 0.16, 0.035, 0.3)
        tab = box(0, -0.84, -0.09, 0.07, 0.07, 0.02)
        laces = box(0, -0.81, 0.09, 0.09, 0.012, 0.1)
        return Node(name, (x, 0.9, 0), [P('Ink', thigh + knee + shin + sole), P('InkPaper', shoe + sock), P('InkAccent', tab + laces + cuff)])

    def arm(name, x, sign):
        upper = transform(cylinder(0.065, 0.055, 0.3, 12), axis='x', ang=math.pi)
        elbow = transform(sphere(0.055, 10, 6), offset=(0, -0.3, 0))
        fore = transform(cylinder(0.052, 0.045, 0.27, 12), axis='x', ang=math.pi, offset=(0, -0.3, 0))
        cuff = transform(torus(0.047, 0.014, 12, 5), offset=(0, -0.56, 0))
        hand = transform(sphere(0.065, 10, 6), offset=(0, -0.62, 0), scale=(0.9, 1.1, 0.9))
        shoulder = transform(sphere(0.075, 10, 6), offset=(0, 0.02, 0))
        return Node(name, (x, 1.25, 0), [P('InkAccent', upper + elbow + fore + shoulder), P('InkPaper', hand + cuff)])

    root = Node('Hero', (0, 0, 0), [], [body, head, leg('LegL', -0.11), leg('LegR', 0.11), arm('ArmL', -0.27, -1), arm('ArmR', 0.27, 1)])
    return [root]

def train():
    body = box(0, 0.95, 3.0, 1.9, 1.5, 6.0)
    roof = transform(cylinder(0.95, 0.95, 6.0, 20), axis='x', ang=-math.pi/2, offset=(0, 1.7, 0.0))
    roof_cut = box(0, 1.25, 3.0, 1.9, 0.9, 6.0)  # fills the gap under the roof cylinder
    nose = frustum_box(0, 1.1, -0.15, 1.9, 0.3, 1.5, 0.1, 1.8)
    nose = transform(nose, axis='x', ang=math.pi/2, offset=(0, 0, 0))  # tapered toward -z
    nose = transform(frustum_box(0, 0, 0, 1.9, 1.8, 1.4, 1.2, 0.3), axis='x', ang=math.pi/2, offset=(0, 1.05, -0.15))
    windshield = box(0, 1.85, -0.22, 1.35, 0.6, 0.06)
    wipers = box(-0.3, 1.6, -0.25, 0.3, 0.02, 0.02) + box(0.3, 1.6, -0.25, 0.3, 0.02, 0.02)
    lights = transform(cylinder(0.16, 0.16, 0.08, 14), axis='x', ang=math.pi/2, offset=(-0.6, 0.95, -0.22)) + transform(cylinder(0.16, 0.16, 0.08, 14), axis='x', ang=math.pi/2, offset=(0.6, 0.95, -0.22))
    light_rims = transform(torus(0.17, 0.02, 16, 5), axis='x', ang=math.pi/2, offset=(-0.6, 0.95, -0.25)) + transform(torus(0.17, 0.02, 16, 5), axis='x', ang=math.pi/2, offset=(0.6, 0.95, -0.25))
    band = box(0, 1.25, 3.0, 1.94, 0.22, 6.02) + box(0, 1.25, -0.17, 1.6, 0.22, 0.1)
    windows = []
    for z in [1.0, 2.2, 3.4, 4.6, 5.6]:
        for x in [-0.96, 0.96]:
            windows += box(x, 1.75, z, 0.04, 0.5, 0.8)
    doors = box(-0.965, 1.0, 1.6, 0.03, 1.6, 0.06) + box(0.965, 1.0, 1.6, 0.03, 1.6, 0.06) + box(-0.965, 1.0, 4.0, 0.03, 1.6, 0.06) + box(0.965, 1.0, 4.0, 0.03, 1.6, 0.06)
    wheels = []
    for z in [1.0, 2.0, 4.0, 5.0]:
        for x in [-0.75, 0.75]:
            wheels += transform(cylinder(0.3, 0.3, 0.2, 16), axis='z', ang=math.pi/2, offset=(x + 0.1, 0.3, z))
    skirt = box(0, 0.35, 3.0, 1.7, 0.3, 5.6)
    plate = box(0, 2.45, 0.3, 0.9, 0.3, 0.08)
    panto = box(0, 2.7, 3.5, 0.6, 0.05, 0.6) + transform(cylinder(0.03, 0.03, 0.35, 8), offset=(-0.25, 2.4, 3.5)) + transform(cylinder(0.03, 0.03, 0.35, 8), offset=(0.25, 2.4, 3.5))
    sign = box(0, 2.45, 0.3, 0.7, 0.18, 0.1)
    return [Node('Train', (0, 0, 0), [P('InkPaper', body + roof + roof_cut + nose + lights + plate), P('Ink', windshield + wipers + windows + doors + wheels + skirt + panto + light_rims), P('InkAccent', band + sign)])]

def barrier():
    board = box(0, 0.62, 0, 1.6, 0.56, 0.08)
    rail = transform(cylinder(0.05, 0.05, 1.64, 12), axis='z', ang=math.pi/2, offset=(0.82, 0.93, 0))
    stripes = []
    for i in range(4):
        stripes += transform(box(0, 0, 0, 0.13, 0.5, 0.02), axis='z', ang=0.35, offset=(-0.5 + i*0.34, 0.62, 0.05)) + transform(box(0, 0, 0, 0.13, 0.5, 0.02), axis='z', ang=-0.35, offset=(-0.5 + i*0.34, 0.62, -0.05))
    legs = box(-0.68, 0.3, 0, 0.08, 0.6, 0.08) + box(0.68, 0.3, 0, 0.08, 0.6, 0.08)
    feet = box(-0.68, 0.03, 0, 0.1, 0.06, 0.5) + box(0.68, 0.03, 0, 0.1, 0.06, 0.5)
    bolts = transform(cylinder(0.025, 0.025, 0.02, 8), axis='x', ang=-math.pi/2, offset=(-0.7, 0.62, 0.04)) + transform(cylinder(0.025, 0.025, 0.02, 8), axis='x', ang=-math.pi/2, offset=(0.7, 0.62, 0.04))
    return [Node('Barrier', (0, 0, 0), [P('InkPaper', board + rail), P('InkAccent', stripes), P('Ink', legs + feet + bolts)])]

def sign():
    posts = transform(cylinder(0.05, 0.05, 2.25, 10), offset=(-0.78, 0, 0)) + transform(cylinder(0.05, 0.05, 2.25, 10), offset=(0.78, 0, 0))
    board = box(0, 1.78, 0, 1.75, 0.95, 0.08)
    frame = box(0, 2.235, 0, 1.79, 0.04, 0.1) + box(0, 1.325, 0, 1.79, 0.04, 0.1) + box(-0.875, 1.78, 0, 0.04, 0.95, 0.1) + box(0.875, 1.78, 0, 0.04, 0.95, 0.1)
    top_band = box(0, 2.16, 0.045, 1.72, 0.1, 0.02)
    # 止 on the front face (toward -z, the side the hero sees)
    g = []
    for (x, y, w, h) in [(0, 1.74, 0.08, 0.62), (-0.3, 1.63, 0.08, 0.38), (0.3, 1.7, 0.08, 0.5), (0, 1.45, 0.95, 0.08), (0.17, 1.9, 0.36, 0.08)]:
        g += box(x, y, -0.05, w, h, 0.03)
    for (x, y, w, h) in [(0, 1.74, 0.08, 0.62), (-0.3, 1.63, 0.08, 0.38), (0.3, 1.7, 0.08, 0.5), (0, 1.45, 0.95, 0.08), (-0.17, 1.9, 0.36, 0.08)]:
        g += box(x, y, 0.05, w, h, 0.03)
    brace = box(0, 1.2, 0, 1.6, 0.05, 0.05)
    return [Node('Sign', (0, 0, 0), [P('Ink', posts + frame + brace), P('InkPaper', board), P('InkAccent', top_band + g)])]

def building_city():
    block = box(1.5, 3.0, 2.0, 3.0, 6.0, 4.0)
    parapet = box(1.5, 6.05, 0.05, 3.0, 0.15, 0.1) + box(1.5, 6.05, 3.95, 3.0, 0.15, 0.1) + box(0.05, 6.05, 2.0, 0.1, 0.15, 4.0) + box(2.95, 6.05, 2.0, 0.1, 0.15, 4.0)
    windows = []
    for r in range(4):
        y = 1.0 + r*1.25
        for z in [0.9, 2.0, 3.1]:
            windows += box(-0.01, y, z, 0.05, 0.6, 0.6)
        for x in [0.7, 1.5, 2.3]:
            windows += box(x, y, -0.01, 0.55, 0.6, 0.05) + box(x, y, 4.01, 0.55, 0.6, 0.05)
    sills = []
    for r in range(4):
        y = 0.66 + r*1.25
        for z in [0.9, 2.0, 3.1]:
            sills += box(-0.04, y, z, 0.08, 0.05, 0.7)
    tank = transform(cylinder(0.5, 0.5, 0.9, 16), offset=(1.9, 6.9, 2.5)) + transform(cylinder(0.5, 0.0, 0.25, 16), offset=(1.9, 7.8, 2.5))
    tank_legs = []
    for dx, dz in [(-0.3, -0.3), (0.3, -0.3), (-0.3, 0.3), (0.3, 0.3)]:
        tank_legs += box(1.9+dx, 6.5, 2.5+dz, 0.06, 0.8, 0.06)
    ac = box(0.7, 6.3, 1.0, 0.7, 0.5, 0.6) + transform(cylinder(0.22, 0.22, 0.03, 14), axis='x', ang=-math.pi/2, offset=(0.7, 6.3, 0.69))
    antenna = transform(cylinder(0.025, 0.025, 1.6, 6), offset=(0.6, 6.1, 3.2)) + box(0.6, 7.5, 3.2, 0.7, 0.02, 0.02) + box(0.6, 7.2, 3.2, 0.5, 0.02, 0.02)
    billboard = box(-0.08, 3.6, 3.2, 0.1, 2.4, 0.9)
    bb_band = box(-0.14, 4.65, 3.2, 0.03, 0.3, 0.9)
    bb_strokes = box(-0.14, 3.8, 3.2, 0.03, 0.08, 0.6) + box(-0.14, 3.4, 3.2, 0.03, 0.08, 0.6) + box(-0.14, 3.6, 3.2, 0.03, 0.6, 0.08) + box(-0.14, 3.0, 3.2, 0.03, 0.3, 0.08)
    door = box(1.5, 1.0, -0.01, 0.9, 2.0, 0.04)
    return [Node('BuildingCity', (0, 0, 0), [P('InkPaper', block + tank + ac + billboard), P('Ink', windows + sills + tank_legs + antenna + parapet + door + bb_strokes), P('InkAccent', bb_band)])]

def containers():
    low = box(1.5, 0.7, 2.0, 3.0, 1.4, 4.0)
    high = box(1.7, 2.1, 2.3, 2.8, 1.4, 3.2)
    ribs = []
    for z in [0.4, 0.9, 1.4, 1.9, 2.4, 2.9, 3.4, 3.9]:
        ribs += box(-0.01, 0.7, z, 0.04, 1.3, 0.12)
    for z in [0.9, 1.4, 1.9, 2.4, 2.9, 3.4]:
        ribs += box(0.29, 2.1, z, 0.04, 1.3, 0.12)
    stripe = box(-0.02, 0.7, 2.0, 0.05, 1.4, 0.5) + box(0.28, 2.1, 1.2, 0.05, 1.4, 0.4)
    corners = []
    for (x, y, z) in [(0.02, 0.02, 0.02), (2.98, 0.02, 0.02), (0.02, 0.02, 3.98), (2.98, 0.02, 3.98)]:
        corners += box(x, y + 0.7, z, 0.12, 1.42, 0.12)
    crane_post = box(2.6, 4.0, 0.6, 0.25, 8.0, 0.25)
    crane_arm = box(1.6, 8.0, 0.6, 4.4, 0.2, 0.2)
    crane_cab = box(2.6, 7.4, 1.0, 0.7, 0.6, 0.6)
    cable = transform(cylinder(0.02, 0.02, 2.2, 6), offset=(-0.4, 5.7, 0.6))
    hook = transform(sphere(0.16, 10, 6), offset=(-0.4, 5.6, 0.6))
    bollard = transform(cylinder(0.12, 0.1, 0.4, 10), offset=(-0.5, 0.0, 3.5))
    return [Node('Containers', (0, 0, 0), [P('InkPaper', low), P('InkTone', high + crane_cab), P('Ink', ribs + corners + crane_post + crane_arm + cable + bollard), P('InkAccent', stripe + hook)])]

def building_alley():
    block = box(1.2, 4.5, 2.0, 2.4, 9.0, 4.0)
    windows = []
    for r in range(6):
        y = 1.2 + r*1.3
        windows += box(-0.01, y, 2.0, 0.05, 0.45, 1.3)
        windows += box(-0.06, y - 0.3, 2.0, 0.12, 0.05, 1.4)
    ac = box(-0.3, 5.2, 0.8, 0.5, 0.45, 0.5)
    sign_board = box(-0.22, 6.5, 3.2, 0.14, 2.6, 0.7)
    sign_band = box(-0.3, 7.6, 3.2, 0.03, 0.3, 0.7)
    sign_strokes = box(-0.3, 6.6, 3.2, 0.03, 0.08, 0.5) + box(-0.3, 6.2, 3.2, 0.03, 0.08, 0.5) + box(-0.3, 6.4, 3.2, 0.03, 0.5, 0.08) + box(-0.3, 5.7, 3.2, 0.03, 0.4, 0.08)
    bracket = box(-0.1, 7.8, 3.2, 0.3, 0.05, 0.05) + box(-0.1, 5.2, 3.2, 0.3, 0.05, 0.05)
    awning = transform(frustum_box(0, 0, 0, 0.3, 1.6, 0.9, 1.6, 0.25), axis='x', ang=0, offset=(-0.4, 2.5, 1.2))
    pipe = transform(cylinder(0.06, 0.06, 9.0, 8), offset=(-0.08, 0, 3.9))
    wire = transform(cylinder(0.015, 0.015, 1.6, 6), axis='z', ang=math.pi/2, offset=(0.0, 6.4, 1.5))
    lantern = transform(sphere(0.3, 12, 8), offset=(-0.9, 5.9, 1.5), scale=(1.0, 1.3, 1.0))
    lantern_caps = transform(cylinder(0.12, 0.12, 0.06, 10), offset=(-0.9, 6.26, 1.5)) + transform(cylinder(0.12, 0.12, 0.06, 10), offset=(-0.9, 5.46, 1.5))
    lantern_string = transform(cylinder(0.012, 0.012, 0.14, 6), offset=(-0.9, 6.3, 1.5))
    return [Node('BuildingAlley', (0, 0, 0), [P('InkPaper', block + sign_board), P('Ink', windows + ac + bracket + pipe + wire + lantern_caps + lantern_string + sign_strokes), P('InkAccent', sign_band + lantern), P('InkTone', awning)])]

def coin():
    disc = transform(cylinder(0.35, 0.35, 0.08, 24), axis='x', ang=math.pi/2, offset=(0, 0.6, -0.04))
    rim = transform(torus(0.35, 0.03, 24, 6), axis='x', ang=math.pi/2, offset=(0, 0.6, 0.0))
    inner = transform(torus(0.24, 0.025, 20, 5), axis='x', ang=math.pi/2, offset=(0, 0.6, 0.045)) + transform(torus(0.24, 0.025, 20, 5), axis='x', ang=math.pi/2, offset=(0, 0.6, -0.045))
    # yen mark as paper bars on both faces
    y = []
    for zf in [0.052, -0.052]:
        y += box(0, 0.6, zf, 0.05, 0.3, 0.02) + box(0, 0.62, zf, 0.22, 0.04, 0.02) + box(0, 0.53, zf, 0.22, 0.04, 0.02)
        y += transform(box(0, 0, 0, 0.05, 0.22, 0.02), axis='z', ang=0.5, offset=(-0.07, 0.72, zf)) + transform(box(0, 0, 0, 0.05, 0.22, 0.02), axis='z', ang=-0.5, offset=(0.07, 0.72, zf))
    return [Node('Coin', (0, 0, 0), [P('InkAccent', disc), P('Ink', rim), P('InkPaper', inner + y)])]

def magnet():
    arc = transform(torus(0.3, 0.09, 20, 8, 0, math.pi), axis='x', ang=-math.pi/2, offset=(0, 0.95, 0))
    arc = transform(torus(0.3, 0.09, 20, 8, 0, math.pi), axis='z', ang=0, offset=(0, 0, 0))
    arc = transform(arc, axis='x', ang=math.pi/2)  # into the xy plane
    arc = transform(arc, axis='z', ang=math.pi, offset=(0, 0.95, 0))  # open end down
    tips = box(-0.3, 0.72, 0, 0.18, 0.22, 0.18) + box(0.3, 0.72, 0, 0.18, 0.22, 0.18)
    badge = transform(cylinder(0.52, 0.52, 0.05, 24), axis='x', ang=math.pi/2, offset=(0, 0.9, 0.16))
    badge_rim = transform(torus(0.52, 0.03, 24, 6), axis='x', ang=math.pi/2, offset=(0, 0.9, 0.14))
    return [Node('Magnet', (0, 0, 0), [P('InkAccent', arc), P('InkPaper', tips + badge), P('Ink', badge_rim)])]

def dash():
    bolt = [(0.12, 0.45), (-0.18, 0.02), (0.0, 0.02), (-0.12, -0.45), (0.2, -0.05), (0.03, -0.05)]
    # fan triangulation needs a convex-ish polygon; split the bolt into two convex parts
    upper = [(0.12, 0.45), (-0.18, 0.02), (0.0, 0.02), (0.08, 0.1)]
    lower = [(0.0, 0.02), (-0.12, -0.45), (0.2, -0.05), (0.03, -0.05), (0.08, 0.1)]
    geo = fix_winding(extrude(upper, 0.12)) + fix_winding(extrude(lower, 0.12), (0.03, -0.1, 0))
    geo = transform(geo, offset=(0, 0.9, 0))
    badge = transform(cylinder(0.52, 0.52, 0.05, 24), axis='x', ang=math.pi/2, offset=(0, 0.9, 0.16))
    badge_rim = transform(torus(0.52, 0.03, 24, 6), axis='x', ang=math.pi/2, offset=(0, 0.9, 0.14))
    return [Node('Dash', (0, 0, 0), [P('InkAccent', geo), P('InkPaper', badge), P('Ink', badge_rim)])]

def lantern():
    body = transform(sphere(0.3, 12, 8), offset=(0, 0, 0), scale=(1.0, 1.3, 1.0))
    caps = transform(cylinder(0.12, 0.12, 0.06, 10), offset=(0, 0.37, 0)) + transform(cylinder(0.12, 0.12, 0.06, 10), offset=(0, -0.43, 0))
    ribs = []
    for i in range(5):
        ribs += transform(torus(0.3 * math.sin((i+1) * math.pi/6), 0.008, 16, 4), offset=(0, 0.39 * math.cos((i+1) * math.pi/6) * -1, 0))
    return [Node('Lantern', (0, 0, 0), [P('InkAccent', body), P('Ink', caps + ribs)])]

MODELS = {
    'Hero': hero, 'Train': train, 'Barrier': barrier, 'Sign': sign, 'BuildingCity': building_city,
    'Containers': containers, 'BuildingAlley': building_alley, 'Coin': coin, 'Magnet': magnet, 'Dash': dash, 'Lantern': lantern,
}

if __name__ == '__main__':
    out = sys.argv[1] if len(sys.argv) > 1 else '.'
    os.makedirs(out, exist_ok=True)
    for name, fn in MODELS.items():
        path = os.path.join(out, name + '.glb')
        write_glb(fn(), path)
        tris = 0
        def count(n):
            global tris
            tris += sum(len(p.tris) for p in n.parts)
            for c in n.children: count(c)
        for n in fn(): count(n)
        print(f'{name:14s} {tris:6d} tris  {os.path.getsize(path)//1024:5d} KB')
