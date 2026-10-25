using UnityEngine;
using InkRun.Core;

namespace InkRun.World
{
    /// <summary>
    /// The rival gang: three ink silhouettes jeering from the roof edge beside the track when a
    /// chapter starts. They scroll past with the world and hide once behind the camera. Volume 1
    /// shows them only here. Built from primitives at runtime; materials come from the scene builder.
    /// </summary>
    public class RivalGang : MonoBehaviour
    {
        public Material Ink, Paper;
        public GameObject ThugPrefab;          // the generated thug mesh (Head/Body/ArmL/ArmR/LegL/LegR nodes); primitives when null
        public float SpawnZ = 70f, HideZ = -9f, EdgeX = -3.45f;
        public float CardDelay = 2.6f;        // the chapter card covers the roofline; the cameo waits for it to fade
        bool _pending; float _wait; Vector3 _spawn;

        Transform[] _guys; Transform[][] _arms; bool _live; float _phase;

        void Start()
        {
            var gm = GameManager.Instance;
            gm.Areas.ChapterStarted += OnChapter; gm.StateChanged += OnState;
            Build();
            SetVisible(false);
        }
        void OnDestroy() { var gm = GameManager.Instance; if (gm == null) return; gm.Areas.ChapterStarted -= OnChapter; gm.StateChanged -= OnState; }

        void OnChapter(AreaDefinition a)
        {
            var gm = GameManager.Instance; if (gm == null || gm.State != GameState.Running) return;
            // stand on the roof edge of this chapter's side building: its height comes from the prop prefab's meshes
            float roofY = 0f;
            if (a != null && a.SideProps != null)
                foreach (var prop in a.SideProps)
                {
                    if (prop == null) continue;
                    foreach (var mf in prop.GetComponentsInChildren<MeshFilter>(true))
                        if (mf.sharedMesh != null) roofY = Mathf.Max(roofY, mf.transform.position.y + mf.sharedMesh.bounds.max.y * Mathf.Abs(mf.transform.lossyScale.y));
                }
            float x = roofY > 0.5f && gm.Tuning != null ? -(gm.Tuning.LaneWidth * 2.05f + 0.35f) : EdgeX;   // AreaController places props at 2.05 lane widths; right on the roof edge so the portrait camera sees them
            _spawn = new Vector3(x, roofY, SpawnZ); _pending = true; _wait = CardDelay; _live = false; SetVisible(false);
        }
        void OnState(GameState s) { if (s != GameState.Running) { _live = false; _pending = false; SetVisible(false); } }

        void Build()
        {
            _guys = new Transform[3]; _arms = new Transform[3][];
            for (int i = 0; i < 3; i++)
            {
                var g = new GameObject("Thug" + i).transform; g.SetParent(transform, false);
                g.localPosition = new Vector3(i == 1 ? -0.5f : 0f, 0f, i * 1.1f);
                g.localRotation = Quaternion.Euler(0f, 90f, 0f);                                             // face the track
                if (ThugPrefab != null)
                {
                    var model = Instantiate(ThugPrefab, g); model.name = "Model"; model.SetActive(true);
                    var l = FindDeep(model.transform, "ArmL"); var r = FindDeep(model.transform, "ArmR");
                    _arms[i] = new[] { l != null ? l : g, r != null ? r : g };
                    _guys[i] = g;
                    continue;
                }
                Prim(PrimitiveType.Capsule, g, new Vector3(0, 0.95f, 0), new Vector3(0.62f, 0.46f, 0.5f), Ink);
                Prim(PrimitiveType.Sphere, g, new Vector3(0, 1.6f, 0), Vector3.one * 0.46f, Ink);
                Prim(PrimitiveType.Cube, g, new Vector3(0, 1.66f, 0), new Vector3(0.5f, 0.07f, 0.5f), Paper);   // bandana
                Prim(PrimitiveType.Cube, g, new Vector3(-0.15f, 0.3f, 0), new Vector3(0.17f, 0.6f, 0.17f), Ink);
                Prim(PrimitiveType.Cube, g, new Vector3(0.15f, 0.3f, 0), new Vector3(0.17f, 0.6f, 0.17f), Ink);
                _arms[i] = new[] { Arm(g, -0.36f), Arm(g, 0.36f) };
                _guys[i] = g;
            }
        }
        static Transform FindDeep(Transform t, string name)
        {
            if (t.name == name) return t;
            for (int i = 0; i < t.childCount; i++) { var f = FindDeep(t.GetChild(i), name); if (f != null) return f; }
            return null;
        }
        Transform Arm(Transform parent, float x)
        {
            var pivot = new GameObject("Arm").transform; pivot.SetParent(parent, false); pivot.localPosition = new Vector3(x, 1.35f, 0);
            Prim(PrimitiveType.Cube, pivot, new Vector3(0, -0.28f, 0), new Vector3(0.14f, 0.56f, 0.14f), Ink);
            return pivot;
        }
        void Prim(PrimitiveType type, Transform parent, Vector3 pos, Vector3 scale, Material m)
        {
            var go = GameObject.CreatePrimitive(type); Destroy(go.GetComponent<Collider>());
            go.transform.SetParent(parent, false); go.transform.localPosition = pos; go.transform.localScale = scale;
            if (m != null) go.GetComponent<Renderer>().sharedMaterial = m;
        }

        void SetVisible(bool v) { if (_guys != null) foreach (var g in _guys) g.gameObject.SetActive(v); }

        void Update()
        {
            if (_pending)
            {
                _wait -= Time.deltaTime;
                if (_wait > 0f) return;
                _pending = false; transform.position = _spawn; _live = true; SetVisible(true);
            }
            if (!_live) return;
            var gm = GameManager.Instance; if (gm == null) return;
            transform.position -= new Vector3(0f, 0f, gm.Speed * Time.deltaTime);      // scroll with the world
            _phase += Time.deltaTime * 9f;
            for (int i = 0; i < _guys.Length; i++)
            {
                float ph = _phase + i * 2.1f;
                var g = _guys[i]; var lp = g.localPosition; lp.y = Mathf.Max(0f, Mathf.Sin(ph)) * 0.18f; g.localPosition = lp;   // hopping and jeering
                _arms[i][0].localRotation = Quaternion.Euler(0, 0, 150f + Mathf.Sin(ph) * 20f);                                    // fist in the air
                _arms[i][1].localRotation = Quaternion.Euler(0, 0, -40f + Mathf.Sin(ph * 1.3f) * 25f);                             // pointing at the hero
            }
            if (transform.position.z < HideZ) { _live = false; SetVisible(false); }
        }
    }
}
