"""Wardrobe outfit parts for Nozomi. Each GLB has a 'Head' node (pivot at the head joint, 0 1.26 0) and a
'Body' node (pivot at the origin) so SceneBootstrap can parent them straight onto the hero's joints."""
import sys, os, math
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import bpy
import inklib as K
from inklib import box, sphere, loft, ring, tube, torus, spin, cylinder, extrude_profile
import hero_bl as H

A, P, I, T, S = 'InkAccent', 'InkPaper', 'Ink', 'InkTone', 'InkSkin'
CHIN, HEAD, TOP, HIP = H.CHIN, H.HEAD, H.TOP, H.HIP
HL = CHIN + 0.30

def station_loft(name, st, mat, n=24, sq=0.0, caps=True):
    rings = [ring((0, y, zc), rx, rz, n, 'xz', sq) for y, rx, rz, zc in st]
    ob = loft(name, rings, mat, cap_ends=caps, sharp_deg=90); K.subsurf(ob, 1); return ob

def delivery():
    """Courier cap worn forward over the spikes + a strapped delivery box on the back."""
    head = []
    crown = [(HL - 0.01, 0.245, 0.235, -0.015), (HL + 0.06, 0.25, 0.24, -0.02), (HL + 0.13, 0.225, 0.225, -0.03), (HL + 0.18, 0.14, 0.15, -0.04), (HL + 0.2, 0.05, 0.06, -0.045)]
    cap = station_loft('CapCrown', crown, A, 24, 0.2, caps=False); K.solidify(cap, 0.03, -1.0); head.append(cap)
    head.append(sphere('CapTop', (0, HL + 0.2, -0.045), 0.07, A, (1, 0.5, 1)))
    head.append(torus('CapBand', (0, HL + 0.0, -0.015), 0.248, 0.02, I, tilt=-6))
    brim = extrude_profile('Brim', [(-0.16, 0.0), (-0.18, 0.09), (-0.13, 0.21), (0.0, 0.25), (0.13, 0.21), (0.18, 0.09), (0.16, 0.0)], 0.025, (0, HL - 0.005, 0.22), I, 'xz', bevel_w=0.008)
    K.transform(brim, (0, HL - 0.005, 0.22), 'x', -14); head.append(brim)
    head.append(box('CapBadge', (0, HL + 0.09, 0.235), (0.08, 0.06, 0.012), P, 0.008))
    body = []
    body.append(box('Box', (0, HIP + 0.26, -0.29), (0.34, 0.3, 0.2), P, 0.02, 2))
    body.append(box('BoxTape', (0, HIP + 0.26, -0.395), (0.34, 0.05, 0.012), A, 0.004))
    body.append(box('BoxTape2', (0, HIP + 0.42, -0.29), (0.05, 0.012, 0.2), A, 0.004))
    for sx in (-1, 1):
        body.append(tube('Strap', [(sx*0.14, HIP + 0.42, -0.2), (sx*0.17, TOP - 0.06, -0.05), (sx*0.16, TOP - 0.12, 0.15), (sx*0.15, HIP + 0.2, 0.17)], 0.02, I))
    return head, body

def ronin():
    """Headband with tails + a bokken slung across the back on a red cord."""
    head = []
    head.append(torus('Headband', (0, HL - 0.02, -0.01), 0.238, 0.03, A, tilt=-6))
    head.append(K.cone_sweep('Tail1', ((-0.05, HL - 0.03, -0.235), (-0.12, HL - 0.2, -0.3), (-0.16, HL - 0.42, -0.26)), (0.035, 0.03, 0.0), A, 10, 7))
    head.append(K.cone_sweep('Tail2', ((0.02, HL - 0.03, -0.235), (0.1, HL - 0.18, -0.32), (0.2, HL - 0.36, -0.3)), (0.035, 0.03, 0.0), A, 10, 7))
    head.append(box('Knot', (0, HL - 0.02, -0.245), (0.07, 0.06, 0.04), A, 0.015))
    body = []
    sword = cylinder('Bokken', (0, 0, 0), 0.025, 1.05, T, 'y', 12, bevel_w=0.01)
    guard = cylinder('Tsuba', (0, -0.28, 0), 0.055, 0.015, I, 'y', 16)
    grip = cylinder('Grip', (0, -0.4, 0), 0.028, 0.24, I, 'y', 12)
    for ob in (sword, guard, grip):
        K.transform(ob, (0, 0, 0), 'z', 35); K.transform(ob, (0, 0, 0), move=(0.1, HIP + 0.35, -0.22)); body.append(ob)
    body.append(tube('Cord', [(-0.17, TOP - 0.06, 0.1), (-0.2, HIP + 0.3, -0.1), (0.12, HIP + 0.15, -0.2)], 0.014, A))
    return head, body

def build(fn, name):
    K.reset(); root = K.empty(name)
    head, body = fn()
    K.parent(K.join('Head', head, (0, HEAD, 0)), root)
    K.parent(K.join('Body', body, (0, 0, 0)), root)
    return root

OUTFITS = {'Outfit_Delivery': delivery, 'Outfit_Ronin': ronin}

if __name__ == '__main__':
    out = sys.argv[1] if len(sys.argv) > 1 else '.'
    for name, fn in OUTFITS.items():
        build(fn, name); print(f'{name:16s} {K.tri_count():6d} tris'); K.export_glb(os.path.join(out, name + '.glb'))
