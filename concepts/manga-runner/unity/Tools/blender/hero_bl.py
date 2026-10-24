"""Nozomi, built in Blender. Unity coords: feet at y 0, faces +z. Joint nodes Body/Head/LegL/LegR/ArmL/ArmR."""
import sys, os, math
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import bpy
import inklib as K
from inklib import box, sphere, loft, ring, tube, skin_chain, torus, spin, cylinder, extrude_profile

A, P, I, T, S = 'InkAccent', 'InkPaper', 'Ink', 'InkTone', 'InkSkin'
HIP, SH, TOP, HEAD = 0.84, 1.2, 1.27, 1.32   # hip joint, shoulder joint, torso top, head joint
HS = 1.22

def head():
    parts = []
    # skull: toon profile (y offset from head joint, radius, z shift), lofted and smoothed
    prof = [(0.47, 0.03, 0.0), (0.445, 0.11, 0.0), (0.405, 0.17, 0.0), (0.345, 0.205, 0.0), (0.275, 0.215, 0.0),
            (0.205, 0.2, 0.005), (0.145, 0.17, 0.012), (0.095, 0.125, 0.02), (0.055, 0.07, 0.025), (0.035, 0.02, 0.025)]
    def skull_ring(y, r, z, grow=0.0, n=24):
        return ring((0, HEAD + 0.3 + (y-0.3)*HS, z*HS), r*HS + grow, (r*HS + grow)*0.96, n, 'xz')
    skull = loft('Skull', [skull_ring(*p) for p in prof], S, sharp_deg=90); K.subsurf(skull, 1); parts.append(skull)
    parts.append(skin_chain('Neck', [(0, HEAD-0.1, 0, 0.075), (0, HEAD+0.02, 0, 0.07), (0, HEAD+0.1, 0, 0.08)], S, 1))
    for sx in (-1, 1): parts.append(sphere('Ear', (sx*0.245*HS/1.12, HEAD+0.26, -0.01), 0.04, S, (0.6, 1.1, 0.8)))
    # hair cap: a shell over the top of the skull, stops just above the brow
    cap = loft('HairCap', [skull_ring(*p, grow=0.03) for p in prof[:4]], I, cap_ends=False, sharp_deg=90); K.solidify(cap, 0.025, -1.0); K.subsurf(cap, 1); parts.append(cap)
    parts.append(sphere('HairTop', (0, HEAD + 0.3 + 0.17*HS + 0.02, 0), 0.075, I, (1, 0.7, 1)))
    # nape flap: back half shell down to the nape
    flap_rings = []
    for y, r, z in [(0.345, 0.205, 0.0), (0.275, 0.215, 0.0), (0.205, 0.2, 0.005), (0.16, 0.175, 0.012)]:
        rr = r*HS + 0.03
        flap_rings.append([(rr*math.cos(a), HEAD + 0.3 + (y-0.3)*HS + 0.01, -rr*0.96*math.sin(a) + z*HS - 0.02) for a in [math.pi*(0.18 + 0.64*k/10) for k in range(11)]])
    flap = loft('Nape', flap_rings, I, cap_ends=False, sharp_deg=90); K.solidify(flap, 0.03, 1.0); K.subsurf(flap, 1); parts.append(flap)
    # spikes (swept, tapered, slightly curled) + bangs + sideburns + nape points
    def spike(name, base, mid, tip, r):
        parts.append(tube(name, [base, mid, tip], r, I, radii=[1.0, 0.75, 0.0], bevel_res=5))
    C = (0, HEAD + 0.3, -0.01); R = 0.215*HS
    for ang, elev, ln, curl in [(0.5, 0.95, 0.26, 0.6), (0.3, 0.8, 0.22, 0.9), (0.7, 0.8, 0.22, 0.9), (0.15, 0.55, 0.18, 1.1), (0.85, 0.55, 0.18, 1.1),
                                (0.42, 1.15, 0.28, 0.4), (0.6, 1.15, 0.27, 0.4), (1.0, 0.75, 0.18, 0.8), (1.2, 0.5, 0.15, 0.9), (0.8, 1.05, 0.22, 0.5), (0.0, 0.5, 0.16, 1.2)]:
        ang *= math.pi
        d = (math.cos(ang)*math.cos(elev), math.sin(elev), -math.sin(ang)*math.cos(elev))
        base = (C[0] + d[0]*R*0.9, C[1] + d[1]*R*0.9, C[2] + d[2]*R*0.9)
        mid = (base[0] + d[0]*ln*0.5, base[1] + (d[1] + 0.5*curl)*ln*0.5, base[2] + d[2]*ln*0.5)
        tip = (mid[0] + (d[0] + math.cos(ang)*0.3*curl)*ln*0.5, mid[1] + (d[1] + 0.2)*ln*0.5, mid[2] + (d[2] - math.sin(ang)*0.3*curl)*ln*0.5)
        spike('Spike', base, mid, tip, 0.06)
    for bx, ln, lean in [(-0.2, 0.15, -0.6), (-0.02, 0.07, -0.1), (0.19, 0.14, 0.6)]:
        a = math.asin(max(-0.95, min(0.95, bx/0.25)))
        base = (0.25*math.sin(a), HEAD + 0.36, 0.24*math.cos(a))
        spike('Bang', base, (base[0] + lean*0.03, base[1] - ln*0.55, base[2] + 0.02), (base[0] + lean*0.08, base[1] - ln, base[2] - 0.005), 0.042)
    for sx in (-1, 1):
        spike('Sideburn', (sx*0.245, HEAD + 0.34, 0.03), (sx*0.25, HEAD + 0.27, 0.04), (sx*0.245, HEAD + 0.19, 0.045), 0.045)
    for ang, ln in [(0.62, 0.1), (0.74, 0.13), (0.88, 0.14), (1.0, 0.15), (1.12, 0.14), (1.26, 0.13), (1.38, 0.1)]:
        ang *= math.pi; rr = 0.215*HS + 0.025
        base = (rr*math.cos(ang), HEAD + 0.3 + (0.16-0.3)*HS + 0.04, -rr*0.96*math.sin(ang) + 0.012*HS - 0.02)
        spike('NapePoint', base, (base[0]*1.03, base[1] - ln*0.5, base[2]*1.03 - 0.005), (base[0]*1.05, base[1] - ln, base[2]*1.05 - 0.01), 0.05)
    # goggles pushed up on the hairline
    parts.append(torus('Strap', (0, HEAD + 0.385, -0.005), 0.242*HS, 0.024, I, tilt=-7))
    parts.append(box('StrapBuckle', (0, HEAD + 0.385, -0.255*HS), (0.07, 0.055, 0.03), I, 0.01))
    for sx in (-1, 1):
        lens = spin('Lens', [(0.0, 0.0), (0.058, 0.0), (0.072, 0.014), (0.072, 0.034), (0.058, 0.046), (0.0, 0.046)], A, 24, axis='z')
        K.transform(lens, (0, 0, 0), 'x', -46); K.transform(lens, (0, 0, 0), move=(sx*0.1, HEAD + 0.44, 0.2)); parts.append(lens)
        rim = torus('LensRim', (0, 0, 0.024), 0.074, 0.014, I, axis='z')
        K.transform(rim, (0, 0, 0), 'x', -46); K.transform(rim, (0, 0, 0), move=(sx*0.1, HEAD + 0.44, 0.2)); parts.append(rim)
    parts.append(box('Bridge', (0, HEAD + 0.455, 0.225), (0.05, 0.022, 0.022), I, 0.006))
    # face
    for sx in (-1, 1):
        parts.append(sphere('EyeWhite', (sx*0.09, HEAD + 0.265, 0.2), 0.07, P, (0.9, 1.1, 0.5)))
        parts.append(sphere('Pupil', (sx*0.082, HEAD + 0.258, 0.232), 0.04, I, (0.75, 1.25, 0.3)))
        parts.append(sphere('Glint', (sx*0.07, HEAD + 0.278, 0.244), 0.013, P, (0.85, 1.1, 0.4)))
        brow = box('Brow', (sx*0.092, HEAD + 0.352, 0.212), (0.1, 0.016, 0.014), I, 0.004)
        K.transform(brow, (sx*0.092, HEAD + 0.352, 0.212), 'z', -sx*17); parts.append(brow)
    parts.append(box('Nose', (0.0, HEAD + 0.215, 0.238), (0.024, 0.034, 0.02), T, 0.006))
    grin = loft('Grin', [ring((0.03, HEAD + 0.16, 0.205), 0.075, 0.014, 16, 'xy'), ring((0.03, HEAD + 0.16, 0.228), 0.066, 0.024, 16, 'xy')], I, sharp_deg=90); K.subsurf(grin, 1); parts.append(grin)
    parts.append(box('Tooth', (0.03, HEAD + 0.168, 0.232), (0.11, 0.011, 0.006), P, 0.002))
    return K.join('Head', parts, (0, HEAD, 0))

def body():
    parts = []
    # hoodie torso: skin chain pelvis -> chest -> shoulders -> neck, radii (x, z)
    torso = skin_chain('Torso', [(0, HIP-0.03, 0, (0.19, 0.12)), (0, 0.92, 0, (0.195, 0.12)), (0, 1.03, 0, (0.2, 0.125)), (0, 1.13, 0, (0.23, 0.135)),
                                 (0, 1.2, 0, (0.26, 0.14)), (0, TOP-0.01, 0, (0.14, 0.095))], A, 2, root=0)
    parts.append(torso)
    # hem band + collar
    parts.append(torus('Hem', (0, HIP-0.02, 0), 0.0, 0.0, I) if False else loft('Hem', [ring((0, y, 0), rx, rz, 24, 'xz') for y, rx, rz in [(HIP-0.045, 0.2, 0.128), (HIP-0.005, 0.2, 0.128), (HIP-0.005, 0.18, 0.11), (HIP-0.045, 0.18, 0.11)]], I, sharp_deg=90))
    parts.append(torus('Collar', (0, TOP, 0), 0.11, 0.02, P))
    # hood draped behind the neck + flap
    pts = []
    for k in range(9):
        a = math.pi*0.1 + math.pi*0.8*k/8
        pts.append((0.18*math.cos(a), TOP - 0.03 - 0.05*math.sin(a), -0.04 - 0.15*math.sin(a)))
    parts.append(tube('Hood', pts, 0.062, A, radii=[0.5, 0.8, 0.95, 1, 1, 1, 0.95, 0.8, 0.5]))
    parts.append(sphere('HoodFlap', (0, TOP-0.12, -0.17), 0.14, A, (1.0, 0.6, 0.35)))
    for sx in (-1, 1):
        parts.append(tube('Drawstring', [(sx*0.045, TOP-0.03, 0.115), (sx*0.06, TOP-0.13, 0.15), (sx*0.055, TOP-0.21, 0.138)], 0.009, P))
        parts.append(sphere('Aglet', (sx*0.055, TOP-0.22, 0.138), 0.015, P))
    # kangaroo pocket with paper seams
    py = HIP + 0.1
    parts.append(box('Pocket', (0, py, 0.112), (0.26, 0.1, 0.04), A, 0.014))
    parts.append(box('PocketSeam', (0, py+0.05, 0.134), (0.25, 0.006, 0.006), P, 0.002))
    for sx in (-1, 1): parts.append(box('PocketSeamS', (sx*0.13, py, 0.134), (0.006, 0.09, 0.006), P, 0.002))
    # brush-stroke glyph on the back
    z = -0.14; cy = HIP + 0.24
    strokes = [(-0.055, 1.215, 0.09, 0.012), (-0.085, 1.185, 0.012, 0.072), (-0.05, 1.15, 0.082, 0.012), (-0.015, 1.165, 0.012, 0.042),
               (0.03, 1.185, 0.012, 0.085), (0.1, 1.185, 0.012, 0.085), (0.065, 1.222, 0.082, 0.012), (0.065, 1.196, 0.07, 0.012), (0.065, 1.17, 0.07, 0.012), (0.065, 1.147, 0.082, 0.012),
               (0.0, 1.11, 0.2, 0.014), (0.0, 1.07, 0.15, 0.014), (0.0, 1.03, 0.22, 0.014), (0.0, 1.07, 0.014, 0.094)]
    for x, y, w, h in strokes: parts.append(box('Stroke', (x*1.3, cy + (y-1.12)*1.3, z), (w*1.3, h*1.3, 0.012), P, 0.003))
    # hips / shorts under the hem
    parts.append(box('Hips', (0, HIP-0.04, 0), (0.33, 0.1, 0.21), I, 0.03, subsurf_levels=1))
    return K.join('Body', parts, (0, 0, 0))

def leg(name, x):
    parts = []
    parts.append(skin_chain('Pant', [(x, HIP, 0, (0.12, 0.125)), (x, HIP-0.2, 0.012, (0.11, 0.115)), (x, 0.45, 0.02, (0.095, 0.1)), (x, 0.25, 0.01, (0.085, 0.09)), (x, 0.12, 0, (0.09, 0.09))], I, 2))
    # sneaker: rounded sections lofted along z
    rings = []
    for z, rx, ry, cy in [(-0.15, 0.065, 0.055, 0.075), (-0.09, 0.095, 0.08, 0.08), (0.0, 0.105, 0.088, 0.085), (0.11, 0.1, 0.08, 0.08), (0.2, 0.085, 0.056, 0.065), (0.27, 0.05, 0.026, 0.05)]:
        rings.append(ring((x, cy, z), rx, ry, 20, 'xy'))
    shoe = loft('Shoe', rings, P, sharp_deg=90); K.subsurf(shoe, 1); parts.append(shoe)
    sole = extrude_profile('Sole', [(0.11*math.copysign(abs(math.cos(t))**0.7, math.cos(t)), 0.215*math.copysign(abs(math.sin(t))**0.7, math.sin(t))) for t in [2*math.pi*i/24 for i in range(24)]], 0.035, (x, 0.0175, 0.06), I, 'xz', bevel_w=0.012)
    parts.append(sole)
    parts.append(box('HeelTab', (x, 0.09, -0.145), (0.07, 0.065, 0.016), A, 0.006))
    parts.append(box('Tongue', (x, 0.11, 0.08), (0.065, 0.02, 0.12), P, 0.006))
    for zz in (0.03, 0.085, 0.14): parts.append(tube('Lace', [(x-0.06, 0.103, zz), (x, 0.125, zz), (x+0.06, 0.103, zz)], 0.008, A))
    return K.join(name, parts, (x, HIP, 0))

def arm(name, x, side):
    parts = []
    parts.append(skin_chain('Sleeve', [(x, SH, 0, (0.1, 0.095)), (x, SH-0.12, 0.01, (0.085, 0.08)), (x, SH-0.26, 0.02, (0.075, 0.072)), (x, SH-0.42, 0.01, (0.072, 0.07))], A, 2))
    parts.append(torus('Cuff', (x, SH-0.465, 0), 0.064, 0.016, P))
    parts.append(sphere('Mitt', (x, SH-0.55, 0.01), 0.08, S, (1.0, 1.2, 0.72)))
    parts.append(sphere('Thumb', (x + side*0.062, SH-0.525, 0.04), 0.03, S, (1.0, 1.5, 0.9)))
    return K.join(name, parts, (x, SH, 0))

def build():
    K.reset()
    root = K.empty('Hero')
    for ob in [body(), head(), leg('LegL', -0.105), leg('LegR', 0.105), arm('ArmL', -0.28, -1), arm('ArmR', 0.28, 1)]:
        K.parent(ob, root)
    return root

if __name__ == '__main__':
    out = sys.argv[1] if len(sys.argv) > 1 else 'Hero.glb'
    build()
    print('Hero tris', K.tri_count())
    K.export_glb(out)
