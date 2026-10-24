"""Ink Run props built in Blender via inklib (Unity coordinates). Dimensions match the trigger
colliders in Editor/SceneBootstrap.cs: obstacles present their -z face to the hero; buildings
occupy x 0..w, z 0..d with the -x face toward the track."""
import sys, os, math
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import bpy
import inklib as K
from inklib import box, sphere, loft, ring, tube, torus, spin, cylinder, extrude_profile

A, P, I, T = 'InkAccent', 'InkPaper', 'Ink', 'InkTone'

def window_cut(block, center, size, depth=0.1, face='-x'):
    """Boolean a window reveal into `block` through the given face; returns the cutter."""
    cx, cy, cz = center; w, h = size
    if face == '-x': c = box('Cut', (cx + depth/2 - 0.01, cy, cz), (depth + 0.02, h, w), I)
    elif face == '-z': c = box('Cut', (cx, cy, cz + depth/2 - 0.01), (w, h, depth + 0.02), I)
    elif face == '+z': c = box('Cut', (cx, cy, cz - depth/2 + 0.01), (w, h, depth + 0.02), I)
    K.boolean(block, c)
    return c

def glass(parts, center, size, face='-x', depth=0.1, mullion=True):
    """Dark pane set back in a reveal plus paper mullions."""
    cx, cy, cz = center; w, h = size
    if face == '-x':
        parts.append(box('Pane', (cx + depth - 0.02, cy, cz), (0.02, h, w), I))
        if mullion:
            parts.append(box('Mullion', (cx + depth*0.6, cy, cz), (0.02, h, 0.03), P, 0.004))
            parts.append(box('Mullion', (cx + depth*0.6, cy, cz), (0.02, 0.03, w), P, 0.004))
    else:
        s = 1 if face == '-z' else -1
        parts.append(box('Pane', (cx, cy, cz + s*(depth - 0.02)), (w, h, 0.02), I))
        if mullion:
            parts.append(box('Mullion', (cx, cy, cz + s*depth*0.6), (0.03, h, 0.02), P, 0.004))
            parts.append(box('Mullion', (cx, cy, cz + s*depth*0.6), (w, 0.03, 0.02), P, 0.004))

# ---------------------------------------------------------------- obstacles
def train():
    parts = []
    rings = []
    for z, ry, rx, cy in [(-0.42, 0.55, 0.52, 1.2), (-0.34, 0.86, 0.78, 1.28), (-0.2, 1.05, 0.9, 1.32), (0.0, 1.13, 0.95, 1.34),
                          (0.6, 1.15, 0.95, 1.35), (3.0, 1.15, 0.95, 1.35), (5.6, 1.15, 0.95, 1.35), (5.95, 1.12, 0.93, 1.35), (6.05, 0.95, 0.8, 1.35)]:
        rings.append(ring((0, cy, z), rx, ry, 32, 'xy', squircle=0.9))
    body = loft('Body', rings, P, sharp_deg=90); K.subsurf(body, 1); parts.append(body)
    for sx in (-1, 1): parts.append(box('Band', (sx*0.955, 1.05, 2.95), (0.05, 0.24, 5.9), A, 0.012))
    nb = box('NoseBand', (0, 1.0, -0.36), (1.3, 0.22, 0.08), A, 0.012); K.transform(nb, (0, 1.0, -0.36), 'x', -14); parts.append(nb)
    for z in [0.75, 1.75, 2.9, 3.6, 4.75, 5.6]:
        for sx in (-1, 1):
            parts.append(box('Window', (sx*0.955, 1.85, z), (0.06, 0.6, 0.72 if z not in (1.75, 3.6) else 0.5), I, 0.05, 3))
    for z in [1.2, 4.15]:
        for sx in (-1, 1):
            parts.append(box('Door', (sx*0.958, 1.15, z), (0.05, 1.6, 0.9), T, 0.03))
            parts.append(box('DoorSplit', (sx*0.975, 1.15, z), (0.04, 1.55, 0.03), I, 0.005))
            parts.append(box('DoorGlass', (sx*0.985, 1.85, z), (0.04, 0.45, 0.72), I, 0.04, 3))
            for dz in (-0.12, 0.12): parts.append(box('Handle', (sx*0.99, 1.25, z+dz), (0.03, 0.35, 0.03), P, 0.01))
    ws = box('Windshield', (0, 1.78, -0.36), (1.15, 0.58, 0.1), I, 0.06, 3); K.transform(ws, (0, 1.78, -0.36), 'x', -17); parts.append(ws)
    for sx in (-1, 1):
        parts.append(tube('Wiper', [(sx*0.35, 1.52, -0.41), (sx*0.22, 1.72, -0.44), (sx*0.1, 1.9, -0.47)], 0.012, I))
        parts.append(spin('Headlight', [(0, 0), (0.11, 0), (0.14, 0.03), (0.14, 0.06), (0, 0.06)], P, 24, (sx*0.52, 0.95, -0.46), 'z'))
        parts.append(torus('HeadlightRim', (sx*0.52, 0.95, -0.44), 0.15, 0.02, I, 'z'))
    db = box('DestBoard', (0, 2.28, -0.22), (0.9, 0.24, 0.12), A, 0.03); K.transform(db, (0, 2.28, -0.22), 'x', -11); parts.append(db)
    dp = box('DestPlate', (0, 2.28, -0.285), (0.7, 0.14, 0.02), P, 0.005); K.transform(dp, (0, 2.28, -0.285), 'x', -11); parts.append(dp)
    cow = loft('Cowcatcher', [ring((0, 0.42, -0.28), 0.9, 0.3, 24, 'xy', squircle=0.9), ring((0, 0.33, -0.52), 0.72, 0.14, 24, 'xy', squircle=0.9)], I, sharp_deg=90); K.subsurf(cow, 1); parts.append(cow)
    for sx in (-1, 1):
        s = box('CowStripe', (sx*0.35, 0.36, -0.5), (0.12, 0.2, 0.03), A, 0.01); K.transform(s, (sx*0.35, 0.36, -0.5), 'z', sx*23); parts.append(s)
    parts.append(box('Coupler', (0, 0.38, 6.12), (0.3, 0.2, 0.2), I, 0.03))
    parts.append(box('Underframe', (0, 0.3, 2.9), (1.7, 0.3, 5.9), I, 0.04))
    for z in [1.0, 4.9]:
        parts.append(box('Bogie', (0, 0.36, z), (1.5, 0.28, 1.3), I, 0.05))
        for sx in (-1, 1):
            for dz in (-0.42, 0.42):
                parts.append(cylinder('Wheel', (sx*0.82, 0.3, z+dz), 0.3, 0.14, I, 'x', 24, bevel_w=0.03))
                parts.append(cylinder('Hub', (sx*0.9, 0.3, z+dz), 0.1, 0.04, T, 'x', 16))
    for z in [1.9, 3.4, 4.9]: parts.append(box('Vent', (0, 2.5, z), (0.7, 0.1, 0.9), T, 0.03))
    parts.append(box('PantoBase', (0, 2.5, 2.7), (0.8, 0.06, 0.8), I, 0.02))
    for sx in (-1, 1): parts.append(tube('PantoArm', [(sx*0.3, 2.5, 2.4), (0, 2.95, 2.7), (sx*0.3, 2.5, 3.0)], 0.025, I))
    parts.append(tube('PantoBar', [(-0.45, 3.0, 2.7), (0, 3.0, 2.7), (0.45, 3.0, 2.7)], 0.03, I))
    return K.join('Train', parts)

def barrier():
    parts = []
    board = box('Board', (0, 0.62, 0), (1.6, 0.56, 0.08), P, 0.035, 3); parts.append(board)
    for i in range(4):
        x = -0.51 + i*0.34
        for sz in (-1, 1):
            s = box('Chevron', (x, 0.62, sz*0.045), (0.15, 0.5, 0.02), A, 0.008); K.transform(s, (x, 0.62, sz*0.045), 'z', sz*20); parts.append(s)
    parts.append(tube('Rail', [(-0.84, 0.94, 0), (-0.3, 0.94, 0), (0.3, 0.94, 0), (0.84, 0.94, 0)], 0.055, P, bevel_res=8))
    for sx in (-1, 1):
        parts.append(sphere('RailCap', (sx*0.84, 0.94, 0), 0.055, I, (0.5, 1, 1)))
        parts.append(box('Leg', (sx*0.68, 0.3, 0), (0.09, 0.62, 0.09), I, 0.02))
        parts.append(box('Foot', (sx*0.68, 0.035, 0), (0.12, 0.07, 0.56), I, 0.02))
        for sz in (-1, 1): parts.append(cylinder('Bolt', (sx*0.7, 0.62, sz*0.05), 0.028, 0.02, I, 'z', 10))
    parts.append(cylinder('LampBase', (0.55, 1.03, 0), 0.07, 0.08, I, 'y', 16))
    parts.append(cylinder('LampRing', (0.55, 1.07, 0), 0.08, 0.02, I, 'y', 16))
    dome = sphere('Lamp', (0.55, 1.07, 0), 0.075, A, (1, 1.1, 1)); parts.append(dome)
    return K.join('Barrier', parts)

def sign():
    parts = []
    for sx in (-1, 1):
        parts.append(cylinder('Post', (sx*0.78, 1.15, 0), 0.055, 2.3, I, 'y', 16))
        parts.append(box('Base', (sx*0.78, 0.03, 0), (0.26, 0.06, 0.26), I, 0.02))
    parts.append(box('Board', (0, 1.78, 0), (1.75, 0.95, 0.08), P, 0.04, 3))
    for y in (2.235, 1.325): parts.append(box('Frame', (0, y, 0), (1.8, 0.05, 0.11), I, 0.015))
    for sx in (-1, 1): parts.append(box('Frame', (sx*0.875, 1.78, 0), (0.05, 0.95, 0.11), I, 0.015))
    for sz in (-1, 1):
        parts.append(box('TopBand', (0, 2.15, sz*0.047), (1.7, 0.1, 0.02), A, 0.006))
        for x, y, w, h in [(0, 1.74, 0.09, 0.62), (0.3*sz, 1.63, 0.09, 0.38), (-0.3*sz, 1.7, 0.09, 0.5), (0, 1.45, 0.95, 0.09), (-0.17*sz, 1.9, 0.36, 0.09)]:
            parts.append(box('Glyph', (x, y, sz*0.05), (w, h, 0.03), A, 0.008))
    parts.append(box('Brace', (0, 1.2, 0), (1.6, 0.05, 0.05), I, 0.012))
    parts.append(box('LampHood', (0, 2.32, -0.12), (0.5, 0.05, 0.22), I, 0.012))
    parts.append(box('LampTube', (0, 2.29, -0.2), (0.4, 0.04, 0.06), P, 0.012))
    return K.join('Sign', parts)

# ---------------------------------------------------------------- scenery
def building_city():
    parts = []
    block = box('Block', (1.5, 3.0, 2.0), (3.0, 6.0, 4.0), P)
    cuts = []
    for r in range(4):
        y = 1.1 + r*1.25
        for z in [0.9, 2.0, 3.1]:
            cuts.append(window_cut(block, (0, y, z), (0.66, 0.66), 0.12, '-x')); glass(parts, (0, y, z), (0.66, 0.66), '-x', 0.12)
            parts.append(box('Sill', (-0.03, y-0.37, z), (0.1, 0.05, 0.78), P, 0.012))
        for x in [0.7, 1.5, 2.3]:
            if x == 1.5 and r == 0: continue
            cuts.append(window_cut(block, (x, y, 0), (0.62, 0.66), 0.12, '-z')); glass(parts, (x, y, 0), (0.62, 0.66), '-z', 0.12)
            cuts.append(window_cut(block, (x, y, 4.0), (0.62, 0.66), 0.12, '+z')); glass(parts, (x, y, 4.0), (0.62, 0.66), '+z', 0.12)
    cuts.append(window_cut(block, (1.5, 1.05, 0), (1.0, 2.1), 0.15, '-z'))
    parts.append(box('Door', (1.5, 1.05, 0.12), (0.95, 2.1, 0.04), I))
    parts.append(box('DoorBar', (1.5, 1.05, 0.1), (0.04, 1.9, 0.04), P, 0.01))
    K.bevel(block, 0.035, 2, 30); K.shade_smooth(block, math.radians(35)); parts.append(block)
    parts.append(box('Plinth', (1.5, 0.3, 2.0), (3.08, 0.6, 4.08), T, 0.03))
    parts.append(box('Parapet', (1.5, 6.05, 2.0), (3.1, 0.16, 4.1), I, 0.03))
    aw = box('Awning', (1.5, 2.3, -0.3), (1.3, 0.08, 0.6), A, 0.03); K.transform(aw, (1.5, 2.3, -0.3), 'x', -17); parts.append(aw)
    # roof
    parts.append(spin('Tank', [(0.0, 6.15), (0.45, 6.15), (0.5, 6.3), (0.5, 7.0), (0.46, 7.1), (0.0, 7.1)], P, 24, (1.9, 0, 2.5)))
    parts.append(spin('TankLid', [(0.0, 7.08), (0.52, 7.08), (0.3, 7.3), (0.05, 7.42), (0.0, 7.42)], I, 24, (1.9, 0, 2.5)))
    for dx, dz in [(-0.32, -0.32), (0.32, -0.32), (-0.32, 0.32), (0.32, 0.32)]: parts.append(box('TankLeg', (1.9+dx, 6.5, 2.5+dz), (0.07, 0.8, 0.07), I, 0.01))
    for t in (6.2, 6.7): parts.append(torus('TankHoop', (1.9, t, 2.5), 0.5, 0.025, I))
    parts.append(box('AC', (0.75, 6.4, 1.0), (0.75, 0.55, 0.65), P, 0.05, 3))
    parts.append(torus('ACFan', (0.75, 6.4, 0.66), 0.22, 0.03, I, 'z'))
    parts.append(tube('Antenna', [(0.6, 6.1, 3.2), (0.6, 6.9, 3.2), (0.6, 7.7, 3.2)], 0.025, I, taper_to=0.6))
    for y, w in [(7.5, 0.7), (7.2, 0.5)]: parts.append(box('AntennaBar', (0.6, y, 3.2), (w, 0.025, 0.025), I, 0.005))
    # billboard on the track face, banner on -z, drainpipe
    parts.append(box('Billboard', (-0.1, 3.6, 3.2), (0.12, 2.4, 0.95), P, 0.03))
    parts.append(box('BillBand', (-0.17, 4.65, 3.2), (0.03, 0.32, 0.95), A, 0.008))
    for y, w, h in [(3.85, 0.6, 0.09), (3.45, 0.6, 0.09), (3.62, 0.09, 0.7), (3.0, 0.09, 0.3), (2.7, 0.5, 0.09)]: parts.append(box('BillStroke', (-0.17, y, 3.2), (0.03, h, w), I, 0.006))
    parts.append(box('Banner', (0.45, 4.6, -0.1), (0.7, 2.6, 0.14), A, 0.03))
    for y, w, h in [(5.5, 0.4, 0.09), (5.1, 0.09, 0.5), (4.6, 0.4, 0.09), (4.1, 0.09, 0.5), (3.6, 0.4, 0.09)]: parts.append(box('BannerStroke', (0.45, y, -0.18), (w, h, 0.03), P, 0.006))
    parts.append(tube('Drain', [(-0.08, 0.1, 0.12), (-0.08, 3.0, 0.12), (-0.08, 6.1, 0.12)], 0.06, I))
    return K.join('BuildingCity', parts)

def container(parts, center, size, material, rib_count, stripe_z):
    cx, cy, cz = center; w, h, d = size
    parts.append(box('Container', center, size, material, 0.04, 2))
    x0 = cx - w/2
    for i in range(rib_count):
        z = cz - d/2 + d*(i+0.5)/rib_count
        parts.append(box('Rib', (x0 - 0.005, cy, z), (0.04, h-0.16, 0.1), I, 0.012))
    for sx in (-1, 1):
        for sz in (-1, 1): parts.append(box('Corner', (cx+sx*(w/2-0.06), cy, cz+sz*(d/2-0.06)), (0.14, h+0.02, 0.14), I, 0.02))
    for k in range(4): parts.append(box('DoorBar', (cx - w/2 + w*(k+0.5)/4, cy, cz - d/2 - 0.005), (0.05, h-0.2, 0.04), I, 0.01))
    parts.append(box('Stripe', (x0-0.01, cy, stripe_z), (0.05, h-0.1, 0.5), A, 0.012))

def containers():
    parts = []
    container(parts, (1.5, 0.7, 2.0), (3.0, 1.4, 4.0), P, 9, 2.6)
    container(parts, (1.7, 2.1, 2.3), (2.8, 1.4, 3.2), T, 7, 1.4)
    parts.append(box('CranePost', (2.6, 4.0, 0.6), (0.32, 8.0, 0.32), I, 0.04))
    parts.append(box('CraneBase', (2.6, 0.12, 0.6), (0.9, 0.24, 0.9), I, 0.04))
    parts.append(box('CraneArm', (1.4, 8.0, 0.6), (4.8, 0.26, 0.26), I, 0.04))
    parts.append(box('Counterweight', (3.5, 7.95, 0.6), (0.7, 0.5, 0.5), I, 0.05))
    parts.append(box('Cab', (2.6, 7.4, 1.05), (0.8, 0.7, 0.7), T, 0.06, 3))
    parts.append(box('CabWindow', (2.6, 7.45, 1.41), (0.6, 0.4, 0.04), I, 0.02))
    parts.append(tube('Cable', [(-0.4, 8.0, 0.6), (-0.4, 6.8, 0.6), (-0.4, 5.75, 0.6)], 0.02, I))
    parts.append(sphere('Pulley', (-0.4, 5.7, 0.6), 0.1, I))
    hook = torus('Hook', (-0.4, 5.45, 0.6), 0.16, 0.05, A, 'z', angle=180); K.transform(hook, (-0.4, 5.45, 0.6), 'z', 180); parts.append(hook)
    parts.append(spin('Bollard', [(0, 0), (0.14, 0.0), (0.14, 0.3), (0.11, 0.38), (0.0, 0.4)], I, 16, (-0.5, 0, 3.6)))
    for i, y in enumerate((0.12, 0.34, 0.56)):
        t = torus('Tyre', (-0.45, y, 0.9), 0.24, 0.11, I, 'y', 24, 10); K.transform(t, (-0.45, y, 0.9), 'y', i*17); parts.append(t)
    return K.join('Containers', parts)

def lantern_parts(parts, c):
    cx, cy, cz = c
    body = spin('Lantern', [(0.0, -0.4), (0.13, -0.4), (0.25, -0.3), (0.3, -0.12), (0.3, 0.12), (0.25, 0.3), (0.13, 0.4), (0.0, 0.4)], A, 24, c); K.subsurf(body, 1); parts.append(body)
    for y in (0.42, -0.42): parts.append(cylinder('Cap', (cx, cy+y, cz), 0.13, 0.07, I, 'y', 16))
    for y, R in [(-0.3, 0.24), (-0.15, 0.29), (0.0, 0.3), (0.15, 0.29), (0.3, 0.24)]: parts.append(torus('Rib', (cx, cy+y, cz), R, 0.012, I, 'y', 24, 6))
    parts.append(tube('String', [(cx, cy+0.44, cz), (cx, cy+0.52, cz), (cx, cy+0.6, cz)], 0.012, I))
    parts.append(tube('Tassel', [(cx, cy-0.46, cz), (cx, cy-0.6, cz), (cx, cy-0.72, cz)], 0.03, A, radii=[0.6, 1.0, 0.4]))

def building_alley():
    parts = []
    block = box('Block', (1.2, 4.5, 2.0), (2.4, 9.0, 4.0), P)
    for r in range(6):
        y = 1.6 + r*1.25
        for z in (1.15, 2.85):
            window_cut(block, (0, y, z), (1.1, 0.5), 0.12, '-x'); glass(parts, (0, y, z), (1.1, 0.5), '-x', 0.12)
            parts.append(box('Sill', (-0.04, y-0.3, z), (0.14, 0.05, 1.22), P, 0.012))
    window_cut(block, (0, 1.1, 3.3), (0.9, 2.2), 0.15, '-x')
    parts.append(box('Door', (0.13, 1.1, 3.3), (0.04, 2.2, 0.9), I))
    parts.append(box('DoorBar', (0.1, 1.1, 3.3), (0.04, 2.0, 0.04), P, 0.01))
    K.bevel(block, 0.035, 2, 30); K.shade_smooth(block, math.radians(35)); parts.append(block)
    parts.append(box('Plinth', (1.2, 0.25, 2.0), (2.48, 0.5, 4.08), T, 0.03))
    parts.append(box('Parapet', (1.2, 9.05, 2.0), (2.5, 0.16, 4.1), I, 0.03))
    # vending machine, AC
    parts.append(box('Vending', (-0.4, 0.95, 0.9), (0.75, 1.9, 0.9), A, 0.05, 3))
    parts.append(box('VendPanel', (-0.79, 1.15, 0.9), (0.04, 1.1, 0.7), P, 0.015))
    for i in range(3):
        for j in range(2): parts.append(box('Can', (-0.81, 1.45-i*0.3, 0.72+j*0.36), (0.03, 0.18, 0.22), I, 0.01))
    parts.append(box('VendSlot', (-0.79, 0.35, 0.9), (0.04, 0.3, 0.6), I, 0.012))
    parts.append(box('AC', (-0.3, 5.2, 0.8), (0.55, 0.45, 0.5), P, 0.04, 3))
    parts.append(torus('ACFan', (-0.58, 5.2, 0.8), 0.16, 0.025, I, 'x'))
    # vertical sign
    parts.append(box('SignBoard', (-0.25, 6.5, 3.2), (0.16, 2.6, 0.72), P, 0.03))
    parts.append(box('SignBand', (-0.34, 7.6, 3.2), (0.03, 0.32, 0.72), A, 0.008))
    for y, w, h in [(6.65, 0.45, 0.09), (6.2, 0.09, 0.5), (5.75, 0.45, 0.09), (5.4, 0.3, 0.09)]: parts.append(box('SignStroke', (-0.34, y, 3.2), (0.03, h, w), I, 0.006))
    for y in (7.75, 5.25): parts.append(box('Bracket', (-0.1, y, 3.2), (0.32, 0.05, 0.05), I, 0.012))
    # awning, struts, drainpipe, wire, lanterns
    aw = loft('Awning', [[(0, 2.47, 2.8), (0, 2.47, 3.8), (0, 2.43, 3.8), (0, 2.43, 2.8)], [(-0.9, 2.22, 2.8), (-0.9, 2.22, 3.8), (-0.9, 2.18, 3.8), (-0.9, 2.18, 2.8)]], T, smooth=False); parts.append(aw)
    for z in (2.85, 3.75): parts.append(tube('Strut', [(-0.85, 2.2, z), (-0.5, 1.6, z), (-0.05, 1.3, z)], 0.02, I))
    parts.append(tube('Drain', [(-0.09, 0.3, 3.92), (-0.09, 4.5, 3.92), (-0.09, 9.0, 3.92)], 0.06, I))
    parts.append(tube('Wire', [(0.0, 6.6, 1.0), (-0.8, 6.45, 1.55), (-1.6, 6.6, 2.1)], 0.015, I))
    lantern_parts(parts, (-0.6, 6.0, 1.4)); lantern_parts(parts, (-1.2, 5.95, 1.8))
    return K.join('BuildingAlley', parts)

# ---------------------------------------------------------------- pickups
def badge(parts):
    parts.append(spin('Badge', [(0, 0), (0.5, 0), (0.52, 0.02), (0.52, 0.04), (0.5, 0.06), (0, 0.06)], P, 32, (0, 0.9, 0.13), 'z'))
    parts.append(torus('BadgeRim', (0, 0.9, 0.14), 0.52, 0.035, I, 'z'))

def coin():
    parts = []
    parts.append(spin('Disc', [(0, 0), (0.32, 0), (0.35, 0.03), (0.35, 0.06), (0.32, 0.09), (0, 0.09)], A, 32, (0, 0.6, -0.045), 'z'))
    parts.append(torus('Rim', (0, 0.6, 0), 0.35, 0.028, I, 'z'))
    for sz in (-1, 1):
        parts.append(torus('Inner', (0, 0.6, sz*0.045), 0.25, 0.02, P, 'z', 28, 8))
        zf = sz*0.053
        parts.append(box('Yen', (0, 0.6, zf), (0.055, 0.3, 0.02), P, 0.006))
        parts.append(box('Yen', (0, 0.62, zf), (0.22, 0.045, 0.02), P, 0.006))
        parts.append(box('Yen', (0, 0.53, zf), (0.22, 0.045, 0.02), P, 0.006))
        for sx in (-1, 1):
            b = box('Yen', (sx*0.07, 0.72, zf), (0.055, 0.22, 0.02), P, 0.006); K.transform(b, (sx*0.07, 0.72, zf), 'z', -sx*29); parts.append(b)
    return K.join('Coin', parts)

def magnet():
    parts = []
    badge(parts)
    arc = torus('Arc', (0, 0.95, 0), 0.3, 0.1, A, 'z', 28, 14, angle=180); parts.append(arc)
    for sx in (-1, 1): parts.append(box('Tip', (sx*0.3, 0.72, 0), (0.2, 0.24, 0.2), P, 0.03, 3))
    return K.join('Magnet', parts)

def dash():
    parts = []
    badge(parts)
    bolt = [(0.12, 0.45), (-0.18, 0.02), (0.0, 0.02), (-0.12, -0.45), (0.2, -0.05), (0.03, -0.05)]
    parts.append(extrude_profile('Bolt', bolt, 0.14, (0, 0.9, 0), A, 'xy', bevel_w=0.015))
    return K.join('Dash', parts)

def lantern():
    parts = []; lantern_parts(parts, (0, 0, 0)); return K.join('Lantern', parts)

MODELS = {'Train': train, 'Barrier': barrier, 'Sign': sign, 'BuildingCity': building_city, 'Containers': containers,
          'BuildingAlley': building_alley, 'Coin': coin, 'Magnet': magnet, 'Dash': dash, 'Lantern': lantern}

if __name__ == '__main__':
    out = sys.argv[1] if len(sys.argv) > 1 else '.'
    only = sys.argv[2].split(',') if len(sys.argv) > 2 else list(MODELS)
    for name in only:
        K.reset(); MODELS[name]()
        print(f'{name:14s} {K.tri_count():7d} tris'); K.export_glb(os.path.join(out, name + '.glb'))
