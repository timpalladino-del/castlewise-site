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
HIP_X, SH_X = 0.11, 0.245

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
        parts.append(sphere('EyeWhite', (sx*0.085, EY, 0.183), 0.058, P, (0.95, 1.0, 0.5)))
        parts.append(sphere('Pupil', (sx*0.08, EY - 0.004, 0.212), 0.036, I, (0.8, 1.15, 0.28)))
        parts.append(sphere('Glint', (sx*0.068, EY + 0.02, 0.224), 0.012, P, (0.9, 1.1, 0.4)))
        brow = box('Brow', (sx*0.09, EY + 0.085, 0.178), (0.1, 0.018, 0.016), I, 0.005); K.transform(brow, (sx*0.09, EY + 0.085, 0.178), 'z', -sx*14); parts.append(brow)
    parts.append(sphere('Nose', (0, EY - 0.055, 0.205), 0.011, S, (1, 1.2, 1)))
    parts.append(tube('Smirk', [(-0.035, CHIN + 0.075, 0.196), (0.01, CHIN + 0.062, 0.2), (0.06, CHIN + 0.08, 0.19)], 0.007, I, bevel_res=4))
    # hair: one crown mass hugging the skull from the hairline up, swept back, then interlocking spikes
    HL = CHIN + 0.30     # hairline (above the brows at CHIN+0.27)
    crown = [(HL - 0.02, 0.228, 0.215, -0.02), (HL + 0.05, 0.235, 0.225, -0.03), (HL + 0.11, 0.22, 0.22, -0.045), (HL + 0.15, 0.17, 0.19, -0.06), (HL + 0.17, 0.08, 0.1, -0.07)]
    cr = station_loft('Crown', crown, I, 24, 0.15, caps=False); K.solidify(cr, 0.03, -1.0); parts.append(cr)
    parts.append(sphere('CrownTop', (0, HL + 0.17, -0.07), 0.09, I, (1, 0.6, 1)))
    # nape: back shell down to the nape with a jagged lower edge
    nape = []
    for y, r in [(HL - 0.01, 0.228), (HL - 0.08, 0.225), (HL - 0.15, 0.2), (HL - 0.2, 0.17)]:
        nape.append([(r*math.cos(a), CHIN + 0 + (y - CHIN), -r*0.96*math.sin(a) - 0.015) for a in [math.pi*(0.2 + 0.6*k/10) for k in range(11)]])
    nb = loft('Nape', nape, I, cap_ends=False, sharp_deg=90); K.solidify(nb, 0.03, 1.0); K.subsurf(nb, 1); parts.append(nb)
    def spike(name, base, mid, tip, r):
        parts.append(K.cone_sweep(name, (base, mid, tip), (r, r*0.7, 0.0), I, 10, 7))
    C = (0, HL + 0.02, -0.03); R = 0.23
    # dominant (3), secondary (6): angle around head from +z toward the back, elevation, length, radius, curl back
    for ang, elev, ln, r, curl in [(0.5, 1.0, 0.26, 0.085, 0.5), (0.3, 0.75, 0.22, 0.08, 0.8), (0.7, 0.75, 0.22, 0.08, 0.8),
                                   (0.12, 0.55, 0.16, 0.065, 1.0), (0.88, 0.55, 0.16, 0.065, 1.0), (0.4, 1.25, 0.2, 0.065, 0.3), (0.6, 1.25, 0.2, 0.065, 0.3),
                                   (1.0, 0.75, 0.11, 0.055, 0.6), (1.15, 0.5, 0.1, 0.05, 0.8), (0.85, 1.1, 0.15, 0.06, 0.4), (0.0, 0.45, 0.14, 0.055, 1.1), (1.3, 0.9, 0.1, 0.045, 0.5)]:
        ang *= math.pi
        d = (math.cos(ang)*math.cos(elev), math.sin(elev), -math.sin(ang)*math.cos(elev))
        base = (C[0] + d[0]*R*0.75, C[1] + d[1]*R*0.75, C[2] + d[2]*R*0.75)
        mid = (base[0] + d[0]*ln*0.5, base[1] + (d[1] + 0.4*curl)*ln*0.5, base[2] + (d[2] - 0.3*curl)*ln*0.5)
        tip = (mid[0] + (d[0] + math.cos(ang)*0.25*curl)*ln*0.5, mid[1] + (d[1] + 0.25)*ln*0.5, mid[2] + (d[2] - 0.45*curl)*ln*0.5)
        spike('Spike', base, mid, tip, r)
    # bangs: two side locks framing the face, one short centre tuft; tips end beside the eyes, never over them
    for bx, ln, lean in [(-0.2, 0.14, -0.6), (-0.03, 0.06, -0.1), (0.19, 0.13, 0.6)]:
        a = math.asin(max(-0.95, min(0.95, bx/0.23)))
        base = (0.225*math.sin(a), HL + 0.01, 0.2*math.cos(a) + 0.02)
        spike('Bang', base, (base[0] + lean*0.03, base[1] - ln*0.55, base[2] + 0.02), (base[0] + lean*0.08, base[1] - ln, base[2] - 0.0), 0.05)
    for sx in (-1, 1):
        spike('Sideburn', (sx*0.215, HL - 0.02, 0.02), (sx*0.22, HL - 0.09, 0.03), (sx*0.215, HL - 0.16, 0.035), 0.045)
    for ang, ln in [(0.62, 0.08), (0.75, 0.1), (0.88, 0.11), (1.0, 0.12), (1.12, 0.11), (1.25, 0.1), (1.38, 0.08)]:
        ang *= math.pi; rr = 0.2
        base = (rr*math.cos(ang), HL - 0.17, -rr*0.96*math.sin(ang) - 0.01)
        spike('NapePoint', base, (base[0]*1.03, base[1] - ln*0.5, base[2]*1.03), (base[0]*1.06, base[1] - ln, base[2]*1.06), 0.045)
    # goggles pushed up on the hairline
    parts.append(torus('Strap', (0, HL + 0.015, -0.02), 0.238, 0.018, T, tilt=-8))
    parts.append(box('StrapBuckle', (0, HL + 0.0, -0.26), (0.07, 0.055, 0.03), I, 0.01))
    for sx in (-1, 1):
        lens = spin('Lens', [(0.0, 0.0), (0.062, 0.0), (0.075, 0.012), (0.075, 0.03), (0.062, 0.045), (0.035, 0.06), (0.0, 0.066)], A, 24, axis='z')
        K.transform(lens, (0, 0, 0), 'x', -35); K.transform(lens, (0, 0, 0), move=(sx*0.1, HL + 0.06, 0.17)); parts.append(lens)
        rim = torus('LensRim', (0, 0, 0.024), 0.078, 0.016, I, axis='z'); K.transform(rim, (0, 0, 0), 'x', -35); K.transform(rim, (0, 0, 0), move=(sx*0.1, HL + 0.06, 0.17)); parts.append(rim)
    parts.append(box('Bridge', (0, HL + 0.075, 0.2), (0.05, 0.024, 0.024), I, 0.006))
    return K.join('Head', parts, (0, HEAD, 0))

def body():
    parts = []
    # hoodie torso: stations (y, half-width, half-depth, z-centre); flares at the hem, wide soft shoulders, short neck
    st = [(HIP - 0.07, 0.225, 0.145, 0.0), (HIP - 0.02, 0.215, 0.14, 0.0), (0.92, 0.195, 0.13, 0.0), (1.0, 0.2, 0.135, 0.005), (1.09, 0.225, 0.15, 0.01),
          (1.16, 0.285, 0.155, 0.01), (1.21, 0.3, 0.15, 0.005), (1.245, 0.21, 0.13, 0.0), (TOP, 0.145, 0.11, -0.005), (TOP + 0.04, 0.15, 0.115, -0.005)]
    torso = station_loft('Torso', st, A, 28, 0.3); parts.append(torso); record('torso', [torso])
    parts.append(cylinder('NeckBand', (0, TOP + 0.02, -0.005), 0.1, 0.07, P, 'y', 20))   # ribbed tee inside the collar
    for sx in (-1, 1): parts.append(sphere('ShoulderBall', (sx*SH_X, SH, 0), 0.088, A))   # ball joint volume: sleeves rotate inside it
    for sx in (-1, 1): parts.append(sphere('HipBall', (sx*HIP_X, HIP, 0), 0.125, I))
    # hood: thick ring of cloth around the neck, bulkier at the back, plus a hanging flap
    pts = []
    for k in range(9):
        a = math.pi*0.22 + math.pi*0.56*k/8
        pts.append((0.2*math.cos(a), TOP - 0.06 - 0.04*math.sin(a), -0.02 - 0.15*math.sin(a)))
    parts.append(tube('HoodRoll', pts, 0.06, A, radii=[0.5, 0.8, 0.95, 1.0, 1.0, 1.0, 0.95, 0.8, 0.5], bevel_res=8))
    hood = extrude_profile('HoodFlap', [(-0.16, 0.0), (-0.13, -0.08), (-0.05, -0.19), (0.0, -0.22), (0.05, -0.19), (0.13, -0.08), (0.16, 0.0), (0.09, 0.03), (-0.09, 0.03)], 0.04, (0, TOP - 0.1, -0.16), A, 'xy', bevel_w=0.014)
    K.transform(hood, (0, TOP - 0.1, -0.16), 'x', 8); parts.append(hood)
    for sx in (-1, 1):
        parts.append(tube('Drawstring', [(sx*0.05, TOP - 0.04, 0.12), (sx*0.065, TOP - 0.14, 0.155), (sx*0.06, TOP - 0.23, 0.145)], 0.009, P))
        parts.append(sphere('Aglet', (sx*0.06, TOP - 0.24, 0.145), 0.015, P))
    # kangaroo pocket, hem band, shorts under the hem
    py = HIP + 0.08
    parts.append(box('Pocket', (0, py, 0.115), (0.34, 0.13, 0.05), A, 0.024, 3))
    for sx in (-1, 1):
        s = box('PocketOpening', (sx*0.15, py + 0.005, 0.142), (0.008, 0.1, 0.008), P, 0.003); K.transform(s, (sx*0.15, py, 0.142), 'z', sx*18); parts.append(s)
    hem = loft('Hem', [ring((0, y, 0), rx, rz, 28, 'xz', 0.3) for y, rx, rz in [(HIP - 0.09, 0.235, 0.155), (HIP - 0.05, 0.235, 0.155), (HIP - 0.05, 0.21, 0.13), (HIP - 0.09, 0.21, 0.13)]], I, sharp_deg=90); parts.append(hem)
    pelvis = station_loft('Pelvis', [(HIP + 0.02, 0.22, 0.135, 0.0), (HIP - 0.06, 0.2, 0.13, 0.0), (HIP - 0.12, 0.165, 0.11, 0.0), (HIP - 0.16, 0.12, 0.085, 0.0)], I, 24, 0.3); parts.append(pelvis)
    # glyph on the back (raised paper strokes), scaled to read at 450 px
    cy = HIP + 0.13
    enso = torus('Enso', (0.0, cy, -0.148), 0.1, 0.014, P, 'z', 32, 8, angle=320); K.transform(enso, (0, cy, -0.148), 'z', 200); parts.append(enso)
    slash = box('Slash', (0.0, cy, -0.15), (0.03, 0.17, 0.014), P, 0.006); K.transform(slash, (0, cy, -0.15), 'z', 28); parts.append(slash)
    return K.join('Body', parts, (0, 0, 0))

def leg(name, x):
    parts = []
    # pant leg: thigh 0.24 -> ankle 0.13 diameter, knee pushed forward, cuff flare
    st = [(HIP + 0.04, 0.125, 0.13, 0.0), (HIP - 0.1, 0.12, 0.125, 0.005), (0.52, 0.1, 0.105, 0.02), (0.34, 0.085, 0.09, 0.015), (0.2, 0.078, 0.082, 0.0), (0.11, 0.074, 0.078, 0.0)]
    rings = [ring((x, y, zc), rx, rz, 20, 'xz', 0.15) for y, rx, rz, zc in st]
    pant = loft('Pant', rings, I, sharp_deg=90); K.subsurf(pant, 1); parts.append(pant)
    # sneaker 0.34 long x 0.15 wide, toe lifted; sections along z (x half-width, y half-height, y centre)
    sh = [(-0.13, 0.062, 0.055, 0.08), (-0.07, 0.082, 0.08, 0.095), (0.0, 0.088, 0.085, 0.1), (0.09, 0.085, 0.078, 0.095), (0.17, 0.072, 0.056, 0.085), (0.23, 0.045, 0.03, 0.085)]
    shoe = loft('Shoe', [ring((x, cy, z), rx, ry, 20, 'xy') for z, rx, ry, cy in sh], P, sharp_deg=90); K.subsurf(shoe, 1); parts.append(shoe); record('shoe', [shoe])
    outline = [(0.09*math.copysign(abs(math.cos(t))**0.6, math.cos(t)), 0.19*math.copysign(abs(math.sin(t))**0.6, math.sin(t))) for t in [2*math.pi*i/28 for i in range(28)]]
    parts.append(extrude_profile('Sole', outline, 0.05, (x, 0.025, 0.045), I, 'xz', bevel_w=0.018))
    parts.append(box('HeelTab', (x, 0.11, -0.13), (0.08, 0.08, 0.02), A, 0.01))
    parts.append(box('Tongue', (x, 0.14, 0.07), (0.07, 0.028, 0.1), P, 0.01))
    for zz in (0.03, 0.08, 0.13): parts.append(tube('Lace', [(x - 0.06, 0.13, zz), (x, 0.158, zz), (x + 0.06, 0.13, zz)], 0.009, A))
    return K.join(name, parts, (x, HIP, 0))

def arm(name, x, side):
    parts = []
    # sleeve: shoulder 0.19 -> wrist 0.11 diameter, hangs 10 deg out and a little forward (relaxed A-pose)
    ELB = SH - 0.22
    upper = loft('Upper', [ring((x, y, 0.0), rx, rz, 20, 'xz') for y, rx, rz in [(SH - 0.01, 0.088, 0.088), (SH - 0.1, 0.086, 0.086), (ELB + 0.02, 0.08, 0.08)]], A, sharp_deg=90); K.subsurf(upper, 1); parts.append(upper)
    parts.append(sphere('Elbow', (x, ELB, 0), 0.082, A))
    fore = [loft('Fore', [ring((x, y, 0.0), rx, rz, 20, 'xz') for y, rx, rz in [(ELB + 0.01, 0.08, 0.08), (ELB - 0.1, 0.074, 0.074), (ELB - 0.18, 0.07, 0.07), (ELB - 0.21, 0.078, 0.078)]], A, sharp_deg=90)]
    K.subsurf(fore[0], 1)
    fore.append(cylinder('Cuff', (x, ELB - 0.22, 0), 0.08, 0.045, P, 'y', 20, bevel_w=0.008))
    mitt = sphere('Mitt', (x, ELB - 0.31, 0.01), 0.095, S, (0.8, 1.0, 0.6)); fore.append(mitt)
    fore.append(sphere('Thumb', (x + side*0.07, ELB - 0.27, 0.05), 0.034, S, (1.0, 1.4, 0.9)))
    for p in fore: K.transform(p, (x, ELB, 0), 'x', -65)   # forearm swings forward: runner's bent elbow
    parts += fore; record('mitt', [mitt])
    for p in parts: K.transform(p, (x, SH, 0), 'z', -side*8)   # a little out from the shoulder
    return K.join(name, parts, (x, SH, 0))

def build():
    K.reset(); MEASURE.clear()
    root = K.empty('Hero')
    for ob in [body(), head(), leg('LegL', -HIP_X), leg('LegR', HIP_X), arm('ArmL', -SH_X, -1), arm('ArmR', SH_X, 1)]:
        K.parent(ob, root)
    return root

if __name__ == '__main__':
    out = sys.argv[1] if len(sys.argv) > 1 else 'Hero.glb'
    build(); print('Hero tris', K.tri_count()); K.export_glb(out)
