"""Smudge Crew runner (rival gang chaser), derived from the Nozomi build: bulkier, black bomber jacket with a white
zip and red bandana, backwards cap instead of spikes, same joint nodes so the chaser animator can drive it.
"""
"""Nozomi v2, built to BRIEF.md. Unity coords: feet at y 0, faces +z. Joint nodes Body/Head/LegL/LegR/ArmL/ArmR.
Construction: lofted cross-sections at measured stations (torso, limbs, skull), tubes for hair, bevelled boxes for
hard details. Every number is a metre from the brief; `# inferred` marks guesses."""
import sys, os, math
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import bpy
import inklib as K
from inklib import box, sphere, loft, ring, tube, torus, spin, cylinder, extrude_profile
import gates as G

A, P, I, T, S = 'InkAccent', 'InkPaper', 'Ink', 'InkTone', 'InkSkin'
MEASURE = {}
def record(name, obs): MEASURE[name] = G.bbox_unity(obs)

# brief numbers
HEIGHT = 1.70; SKULL_H = 0.46; CHIN = HEIGHT - SKULL_H          # 1.24
HIP = 0.84; SH = 1.20; TOP = 1.26; HEAD = 1.26                    # joints
HIP_X, SH_X = 0.125, 0.285

def station_loft(name, stations, material, n=24, squircle=0.0, plane='xz', subsurf=1, caps=True):
    """stations: (y, rx, rz, zc) for plane 'xz' (horizontal rings) -> smooth lofted mass."""
    rings = [ring((0 if plane == 'xz' else s[3], s[0], s[3] if plane == 'xz' else 0), s[1], s[2], n, plane, squircle) for s in stations]
    ob = loft(name, rings, material, cap_ends=caps, sharp_deg=90)
    if subsurf: K.subsurf(ob, subsurf)
    return ob

def head():
    parts = []
    # skull: egg with a tapered jaw; width 0.42, depth 0.40, height 0.46 (brief)
    sk = [(0.0, 0.045, 0.04, 0.02), (0.04, 0.11, 0.10, 0.02), (0.10, 0.16, 0.15, 0.015), (0.17, 0.195, 0.185, 0.01), (0.24, 0.21, 0.20, 0.0),
          (0.31, 0.21, 0.20, -0.005), (0.37, 0.195, 0.19, -0.01), (0.42, 0.15, 0.15, -0.012), (0.455, 0.07, 0.07, -0.012)]
    skull = station_loft('Skull', [(CHIN + y, rx, rz, zc) for y, rx, rz, zc in sk], S); parts.append(skull); record('skull', [skull])
    parts.append(cylinder('Neck', (0, CHIN - 0.03, 0.0), 0.07, 0.1, S, 'y', 16))
    for sx in (-1, 1): parts.append(sphere('Ear', (sx*0.2, CHIN + 0.2, -0.035), 0.038, S, (0.6, 1.1, 0.8)))
    # face: big eyes set low (centre at 0.40 of skull height), small nose, wide grin
    EY = CHIN + 0.185
    for sx in (-1, 1):
        parts.append(sphere('EyeWhite', (sx*0.085, EY, 0.183), 0.058, P, (0.95, 0.7, 0.5)))
        parts.append(sphere('Pupil', (sx*0.08, EY - 0.004, 0.212), 0.036, I, (0.8, 1.15, 0.28)))
        parts.append(sphere('Glint', (sx*0.068, EY + 0.02, 0.224), 0.012, P, (0.9, 1.1, 0.4)))
        brow = box('Brow', (sx*0.09, EY + 0.075, 0.178), (0.11, 0.022, 0.016), I, 0.005); K.transform(brow, (sx*0.09, EY + 0.075, 0.178), 'z', sx*22); parts.append(brow)
    parts.append(sphere('Nose', (0, EY - 0.055, 0.205), 0.011, S, (1, 1.2, 1)))
    parts.append(tube('Grimace', [(-0.06, CHIN + 0.085, 0.19), (0.0, CHIN + 0.07, 0.2), (0.06, CHIN + 0.085, 0.19)], 0.008, I, bevel_res=4))
    parts.append(box('Teeth', (0.0, CHIN + 0.075, 0.2), (0.07, 0.012, 0.006), P, 0.002))
    # short hair under a backwards cap: cap crown, band, and the brim pointing back
    HL = CHIN + 0.30
    crown = [(HL - 0.03, 0.232, 0.22, -0.02), (HL + 0.04, 0.24, 0.23, -0.025), (HL + 0.1, 0.225, 0.22, -0.035), (HL + 0.145, 0.16, 0.17, -0.045), (HL + 0.165, 0.06, 0.07, -0.05)]
    cap = station_loft('Cap', crown, I, 24, 0.2, caps=False); K.solidify(cap, 0.03, -1.0); parts.append(cap)
    parts.append(sphere('CapTop', (0, HL + 0.165, -0.05), 0.08, I, (1, 0.5, 1)))
    parts.append(sphere('CapButton', (0, HL + 0.19, -0.05), 0.02, A))
    parts.append(torus('CapBand', (0, HL - 0.02, -0.02), 0.236, 0.022, A, tilt=-6))
    brim = extrude_profile('Brim', [(-0.15, 0.0), (-0.17, -0.09), (-0.12, -0.2), (0.0, -0.24), (0.12, -0.2), (0.17, -0.09), (0.15, 0.0)], 0.025, (0, HL - 0.02, -0.21), I, 'xz', bevel_w=0.008)
    K.transform(brim, (0, HL - 0.02, -0.21), 'x', 12); parts.append(brim)
    nape = []
    for y, r in [(HL - 0.02, 0.226), (HL - 0.09, 0.22), (HL - 0.15, 0.195)]:
        nape.append([(r*math.cos(a), y, -r*0.96*math.sin(a) - 0.015) for a in [math.pi*(0.2 + 0.6*k/10) for k in range(11)]])
    nb = loft('Nape', nape, I, cap_ends=False, sharp_deg=90); K.solidify(nb, 0.025, 1.0); K.subsurf(nb, 1); parts.append(nb)
    for sx in (-1, 1):
        parts.append(K.cone_sweep('Sideburn', ((sx*0.215, HL - 0.03, 0.02), (sx*0.22, HL - 0.07, 0.03), (sx*0.215, HL - 0.11, 0.035)), (0.04, 0.03, 0.0), I, 10, 6))
    sc = box('Scar', (0.12, CHIN + 0.13, 0.19), (0.012, 0.09, 0.01), T, 0.003); K.transform(sc, (0.12, CHIN + 0.13, 0.19), 'z', -25); parts.append(sc)
    parts.append(box('Chin', (0.0, CHIN + 0.03, 0.17), (0.12, 0.05, 0.03), T, 0.012))
    return K.join('Head', parts, (0, HEAD, 0))

def body():
    parts = []
    # hoodie torso: stations (y, half-width, half-depth, z-centre); flares at the hem, wide soft shoulders, short neck
    st = [(HIP - 0.06, 0.25, 0.165, 0.0), (HIP - 0.02, 0.245, 0.16, 0.0), (0.92, 0.235, 0.155, 0.0), (1.0, 0.24, 0.16, 0.005), (1.09, 0.27, 0.17, 0.01),
          (1.16, 0.33, 0.175, 0.01), (1.21, 0.34, 0.17, 0.005), (1.245, 0.24, 0.14, 0.0), (TOP, 0.15, 0.11, -0.005), (TOP + 0.03, 0.155, 0.115, -0.005)]
    torso = station_loft('Torso', st, I, 28, 0.3); parts.append(torso); record('torso', [torso])
    # bomber details: white zip line, red bandana knotted at the neck, chest patch
    parts.append(box('Zip', (0, HIP + 0.2, 0.17), (0.02, 0.4, 0.012), P, 0.004))
    parts.append(torus('Bandana', (0, TOP - 0.015, 0.0), 0.15, 0.045, A, tilt=-10))
    parts.append(K.cone_sweep('BandanaTail', ((0.1, TOP - 0.04, -0.12), (0.18, TOP - 0.16, -0.2), (0.24, TOP - 0.3, -0.22)), (0.045, 0.035, 0.0), A, 10, 7))
    parts.append(K.cone_sweep('BandanaTail2', ((0.13, TOP - 0.04, -0.1), (0.24, TOP - 0.12, -0.14), (0.33, TOP - 0.22, -0.1)), (0.04, 0.03, 0.0), A, 10, 7))
    parts.append(cylinder('NeckBand', (0, TOP + 0.02, -0.005), 0.1, 0.07, T, 'y', 20))
    for sx in (-1, 1): parts.append(sphere('ShoulderBall', (sx*SH_X, SH, 0), 0.1, I))   # ball joint volume: sleeves rotate inside it
    for sx in (-1, 1): parts.append(sphere('HipBall', (sx*HIP_X, HIP, 0), 0.125, T))
    # hood: thick ring of cloth around the neck, bulkier at the back, plus a hanging flap
    hem = loft('Hem', [ring((0, y, 0), rx, rz, 28, 'xz', 0.3) for y, rx, rz in [(HIP - 0.09, 0.265, 0.175), (HIP - 0.05, 0.265, 0.175), (HIP - 0.05, 0.24, 0.15), (HIP - 0.09, 0.24, 0.15)]], T, sharp_deg=90); parts.append(hem)
    pelvis = station_loft('Pelvis', [(HIP + 0.02, 0.24, 0.15, 0.0), (HIP - 0.06, 0.22, 0.14, 0.0), (HIP - 0.12, 0.18, 0.12, 0.0), (HIP - 0.16, 0.13, 0.09, 0.0)], T, 24, 0.3); parts.append(pelvis)
    # glyph on the back (raised paper strokes), scaled to read at 450 px
    cy = HIP + 0.14
    enso = torus('Enso', (0.0, cy, -0.146), 0.115, 0.022, I, 'z', 40, 8, angle=315); K.transform(enso, (0, cy, -0.146), 'z', 200); parts.append(enso)
    slash = box('Slash', (0.0, cy, -0.148), (0.045, 0.2, 0.018), I, 0.008); K.transform(slash, (0, cy, -0.148), 'z', 28); parts.append(slash)
    return K.join('Body', parts, (0, 0, 0))

def leg(name, x):
    parts = []
    # pant leg: thigh 0.24 -> ankle 0.13 diameter, knee pushed forward, cuff flare
    st = [(HIP + 0.04, 0.125, 0.13, 0.0), (HIP - 0.1, 0.12, 0.125, 0.005), (0.52, 0.1, 0.105, 0.02), (0.34, 0.085, 0.09, 0.015), (0.2, 0.078, 0.082, 0.0), (0.11, 0.074, 0.078, 0.0)]
    rings = [ring((x, y, zc), rx, rz, 20, 'xz', 0.15) for y, rx, rz, zc in st]
    pant = loft('Pant', rings, T, sharp_deg=90); K.subsurf(pant, 1); parts.append(pant)
    sx = 1 if x > 0 else -1
    parts.append(box('CargoPocket', (x + sx*0.09, 0.56, 0.02), (0.06, 0.12, 0.14), T, 0.012))
    # sneaker 0.34 long x 0.15 wide, toe lifted; sections along z (x half-width, y half-height, y centre)
    sh = [(-0.13, 0.062, 0.055, 0.08), (-0.07, 0.082, 0.08, 0.095), (0.0, 0.088, 0.088, 0.105), (0.09, 0.086, 0.082, 0.1), (0.17, 0.076, 0.062, 0.09), (0.23, 0.05, 0.036, 0.085)]
    shoe = loft('Shoe', [ring((x, cy, z), rx, ry, 20, 'xy') for z, rx, ry, cy in sh], I, sharp_deg=90); K.subsurf(shoe, 1); parts.append(shoe); record('shoe', [shoe])
    outline = [(0.09*math.copysign(abs(math.cos(t))**0.6, math.cos(t)), 0.19*math.copysign(abs(math.sin(t))**0.6, math.sin(t))) for t in [2*math.pi*i/28 for i in range(28)]]
    parts.append(extrude_profile('Sole', outline, 0.05, (x, 0.025, 0.045), P, 'xz', bevel_w=0.018))
    parts.append(box('BootStrap', (x, 0.13, 0.05), (0.19, 0.03, 0.2), A, 0.01))
    pass
    return K.join(name, parts, (x, HIP, 0))

def arm(name, x, side):
    parts = []
    # sleeve: shoulder 0.19 -> wrist 0.11 diameter, hangs 10 deg out and a little forward (relaxed A-pose)
    ELB = SH - 0.24
    # one continuous sleeve: shoulder -> elbow (slightly back) -> wrist forward of the hip, no seam at the joint
    parts.append(K.sweep_loft('Sleeve', [(x, SH + 0.01, 0.0), (x, ELB, -0.04), (x, ELB - 0.03, 0.2)], [0.105, 0.1, 0.092, 0.086, 0.082], I, 20, 11))
    fore = []
    fore.append(cylinder('Cuff', (x, ELB - 0.03, 0.215), 0.09, 0.045, T, 'z', 20, bevel_w=0.008))
    mitt = sphere('Mitt', (x, ELB - 0.03, 0.3), 0.1, I, (0.78, 0.62, 1.0)); fore.append(mitt)   # fingerless gloves
    fore.append(sphere('Thumb', (x + side*0.07, ELB + 0.01, 0.265), 0.034, I, (1.0, 0.9, 1.4)))
    parts += fore; record('mitt', [mitt])
    for p in parts: K.transform(p, (x, SH, 0), 'z', -side*6)   # a little out from the shoulder
    return K.join(name, parts, (x, SH, 0))

def build():
    K.reset(); MEASURE.clear()
    root = K.empty('Crew')
    for ob in [body(), head(), leg('LegL', -HIP_X), leg('LegR', HIP_X), arm('ArmL', -SH_X, -1), arm('ArmR', SH_X, 1)]:
        K.parent(ob, root)
    return root

if __name__ == '__main__':
    out = sys.argv[1] if len(sys.argv) > 1 else 'Crew.glb'
    build(); print('Crew tris', K.tri_count()); K.export_glb(out)
