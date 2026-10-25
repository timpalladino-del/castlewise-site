#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using InkRun.Core;
using InkRun.Player;
using InkRun.World;
using InkRun.FX;
using InkRun.UI;
using InkRun.Meta;

namespace InkRun.EditorTools
{
    /// <summary>
    /// Menu: Ink Run > Build gray-box scene. Creates materials, data assets, prefabs made of
    /// primitives, the Run scene with every manager wired, and the whole UI. After it runs,
    /// press Play. Replace primitives with real models later; the wiring stays.
    /// Safe to re-run: assets are overwritten in place.
    /// </summary>
    public static class SceneBootstrap
    {
        const string Root = "Assets/InkRun";
        static Material _paper, _accent, _ink, _tone, _skin;

        [MenuItem("Ink Run/Build gray-box scene")]
        public static void Build()
        {
            if (TMP_Settings.instance == null)
            {
                Debug.LogWarning("Ink Run: import TextMeshPro first (Window > TextMeshPro > Import TMP Essential Resources), then run this menu again.");
                return;
            }
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);   // assets deleted outside the editor (e.g. Data/Tuning.asset to reset defaults) are otherwise still served stale
            Folders();
            Materials();
            PatternBootstrap.Create();
            var tuning = Asset<Tuning>("Data/Tuning.asset");
            var outfits = Outfits();
            var prefabs = Prefabs();
            var areas = Areas(prefabs);
            Scene(tuning, outfits, prefabs, areas);
            Debug.Log("Ink Run: gray-box scene built. Press Play.");
        }

        // ---------- assets ----------

        static void Folders()
        {
            foreach (var f in new[] { "Data", "Data/Patterns", "Data/Areas", "Data/Outfits", "Prefabs", "Materials", "Scenes" })
            {
                string full = Root + "/" + f;
                if (!AssetDatabase.IsValidFolder(full)) AssetDatabase.CreateFolder(System.IO.Path.GetDirectoryName(full).Replace('\\', '/'), System.IO.Path.GetFileName(full));
            }
        }

        static T Asset<T>(string rel) where T : ScriptableObject
        {
            string path = Root + "/" + rel;
            var a = AssetDatabase.LoadAssetAtPath<T>(path);
            if (a == null) { a = ScriptableObject.CreateInstance<T>(); AssetDatabase.CreateAsset(a, path); }
            return a;
        }

        static Material Mat(string name, Color baseColor, bool accent)
        {
            string path = Root + "/Materials/" + name + ".mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            var shader = Shader.Find("InkRun/InkToon") ?? Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            if (m == null) { m = new Material(shader); AssetDatabase.CreateAsset(m, path); }
            m.shader = shader;
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", baseColor);
            if (m.HasProperty("_UseAccent")) m.SetFloat("_UseAccent", accent ? 1f : 0f);
            if (m.HasProperty("_Color")) m.SetColor("_Color", baseColor);
            EditorUtility.SetDirty(m);
            return m;
        }

        static void Materials()
        {
            _paper = Mat("InkPaper", Color.white, false);
            _accent = Mat("InkAccent", Color.white, true);
            _ink = Mat("InkBlack", new Color(0.07f, 0.07f, 0.07f), false);
            _tone = Mat("InkTone", new Color(0.75f, 0.75f, 0.75f), false);
            _skin = Mat("InkSkin", new Color(1f, 0.95f, 0.9f), false);
        }

        static OutfitDefinition[] Outfits()
        {
            var list = new[]
            {
                Outfit("default", "Default", "Hoodie and goggles", 0),
                Outfit("delivery", "Delivery", "Cap and a delivery box", 300),
                Outfit("ronin", "Ronin", "Headband and a bokken", 800),
            };
            return list;
        }
        static OutfitDefinition Outfit(string id, string name, string desc, int price)
        {
            var o = Asset<OutfitDefinition>("Data/Outfits/" + name + ".asset");
            o.Id = id; o.Name = name; o.Description = desc; o.Price = price; EditorUtility.SetDirty(o);
            return o;
        }

        static AreaDefinition[] Areas(PrefabSet p)
        {
            return new[]
            {
                Area("City", "CHAPTER 1", "CITY ROOFTOPS", new Color(0.878f, 0.125f, 0.165f), false, p.building),
                Area("Harbor", "CHAPTER 2", "THE HARBOR", new Color(0.118f, 0.435f, 0.878f), false, p.container),
                Area("NightAlley", "CHAPTER 3", "NIGHT ALLEY", new Color(0.482f, 0.184f, 0.878f), true, p.alley),
            };
        }
        static AreaDefinition Area(string file, string chapter, string name, Color accent, bool moon, GameObject prop)
        {
            var a = Asset<AreaDefinition>("Data/Areas/" + file + ".asset");
            a.ChapterLabel = chapter; a.DisplayName = name; a.Accent = accent; a.CrescentMoon = moon; a.SideProps = new[] { prop };
            a.Sky = moon ? new Color(0.11f, 0.10f, 0.15f) : Color.white;   // night alley: ink sky, white shapes
            EditorUtility.SetDirty(a);
            return a;
        }

        // ---------- prefabs ----------

        struct PrefabSet { public GameObject barrier, sign, wall, gap, coin, magnet, dash, track, building, container, alley, hero, word; }

        static PrefabSet Prefabs()
        {
            var p = new PrefabSet();
            var barrierModel = Model("Barrier");
            if (barrierModel != null)
            {
                var root = new GameObject("Barrier"); barrierModel.transform.SetParent(root.transform, false); Shadow(root, 1.8f, 0.7f);
                var bc = root.AddComponent<BoxCollider>(); bc.isTrigger = true; bc.size = new Vector3(1.6f, 0.8f, 0.4f); bc.center = new Vector3(0, 0.5f, 0);
                root.AddComponent<Obstacle>().Kind = ObstacleKind.Barrier;
                p.barrier = Save("Barrier", root);
            }
            else
            {
                var root = Obstacle(ObstacleKind.Barrier, Box("Body", new Vector3(1.6f, 0.8f, 0.4f), new Vector3(0, 0.4f, 0), _paper, true));
                for (int i = 0; i < 3; i++) { var stripe = Box("Stripe" + i, new Vector3(0.22f, 0.82f, 0.42f), new Vector3(-0.5f + i * 0.5f, 0.4f, 0), _accent, false); stripe.transform.SetParent(root.transform, false); stripe.transform.localRotation = Quaternion.Euler(0, 0, 18f); }
                Box("FootL", new Vector3(0.1f, 0.1f, 0.6f), new Vector3(-0.65f, 0.05f, 0), _ink, false).transform.SetParent(root.transform, false);
                Box("FootR", new Vector3(0.1f, 0.1f, 0.6f), new Vector3(0.65f, 0.05f, 0), _ink, false).transform.SetParent(root.transform, false);
                p.barrier = Save("Barrier", root);
            }
            var signModel = Model("Sign");
            if (signModel != null)
            {
                var root = new GameObject("Sign"); signModel.transform.SetParent(root.transform, false);
                var bc = root.AddComponent<BoxCollider>(); bc.isTrigger = true; bc.size = new Vector3(1.75f, 0.95f, 0.3f); bc.center = new Vector3(0, 1.78f, 0);
                root.AddComponent<Obstacle>().Kind = ObstacleKind.Sign;
                p.sign = Save("Sign", root);
            }
            else
            {
                var root = new GameObject("Sign");
                Box("Board", new Vector3(1.6f, 0.9f, 0.3f), new Vector3(0, 1.75f, 0), _paper, true).transform.SetParent(root.transform, false);
                Box("PostL", new Vector3(0.1f, 2.2f, 0.1f), new Vector3(-0.75f, 1.1f, 0), _ink, false).transform.SetParent(root.transform, false);
                Box("PostR", new Vector3(0.1f, 2.2f, 0.1f), new Vector3(0.75f, 1.1f, 0), _ink, false).transform.SetParent(root.transform, false);
                var kanji = new GameObject("Kanji"); kanji.transform.SetParent(root.transform, false); kanji.transform.localPosition = new Vector3(0, 1.75f, -0.17f);
                var kt = kanji.AddComponent<TextMeshPro>(); kt.text = "STOP"; kt.fontSize = 4; kt.alignment = TextAlignmentOptions.Center; kt.color = new Color(0.878f, 0.125f, 0.165f); kanji.AddComponent<AccentText>();   // swap for 止 once a Japanese TMP font is added
                root.AddComponent<Obstacle>().Kind = ObstacleKind.Sign;
                p.sign = Save("Sign", root);
            }
            var trainModel = Model("Train");
            if (trainModel != null)
            {
                var root = new GameObject("Train"); trainModel.transform.SetParent(root.transform, false); Shadow(root, 2.1f, 6.6f, 0, 3f);
                var bc = root.AddComponent<BoxCollider>(); bc.isTrigger = true; bc.size = new Vector3(1.9f, 2.6f, 6f); bc.center = new Vector3(0, 1.3f, 3f);
                root.AddComponent<Obstacle>().Kind = ObstacleKind.Wall;
                p.wall = Save("Train", root);
            }
            else
            {
                var root = Obstacle(ObstacleKind.Wall, Box("Body", new Vector3(1.9f, 2.6f, 6f), new Vector3(0, 1.3f, 3f), _paper, true));
                Box("Band", new Vector3(1.92f, 0.3f, 6.02f), new Vector3(0, 0.9f, 3f), _accent, false).transform.SetParent(root.transform, false);
                for (int i = 0; i < 3; i++) Box("Window" + i, new Vector3(0.5f, 0.6f, 6.02f), new Vector3(-0.6f + i * 0.6f, 1.9f, 3f), _ink, false).transform.SetParent(root.transform, false);
                Box("Nose", new Vector3(1.9f, 0.8f, 0.3f), new Vector3(0, 0.4f, -0.1f), _ink, false).transform.SetParent(root.transform, false);
                p.wall = Save("Train", root);
            }
            {
                // thin black slab to see, plus an invisible trigger tall enough to touch the hero's capsule
                var root = Obstacle(ObstacleKind.Gap, Box("Hole", new Vector3(1.8f, 0.3f, 3f), new Vector3(0, 0.08f, 1.2f), _ink, false));
                var bc = root.AddComponent<BoxCollider>(); bc.isTrigger = true; bc.size = new Vector3(1.8f, 1.2f, 3f); bc.center = new Vector3(0, 0.5f, 1.2f);
                p.gap = Save("Gap", root);
            }
            var coinModel = Model("Coin");
            if (coinModel != null)
            {
                var root = new GameObject("Coin"); coinModel.transform.SetParent(root.transform, false);
                var sc = root.AddComponent<SphereCollider>(); sc.isTrigger = true; sc.radius = 0.45f; sc.center = new Vector3(0, 0.6f, 0);
                root.AddComponent<Pickup>().Kind = PickupKind.Coin;
                p.coin = Save("Coin", root);
            }
            else
            {
                var root = new GameObject("Coin");
                var disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder); disc.name = "Disc";
                Object.DestroyImmediate(disc.GetComponent<Collider>());
                disc.transform.SetParent(root.transform, false);
                disc.transform.localScale = new Vector3(0.5f, 0.05f, 0.5f);
                disc.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                disc.transform.localPosition = new Vector3(0, 0.6f, 0);
                disc.GetComponent<Renderer>().sharedMaterial = _accent;
                var sc = root.AddComponent<SphereCollider>(); sc.isTrigger = true; sc.radius = 0.45f; sc.center = new Vector3(0, 0.6f, 0);
                root.AddComponent<Pickup>().Kind = PickupKind.Coin;
                p.coin = Save("Coin", root);
            }
            p.magnet = Save("Magnet", PickupPrefab(PickupKind.Magnet));
            p.dash = Save("Dash", PickupPrefab(PickupKind.Dash));
            {
                var root = new GameObject("TrackPiece");
                Box("Roof", new Vector3(6.6f, 0.2f, 10f), new Vector3(0, -0.1f, 5f), _paper, false).transform.SetParent(root.transform, false);
                Box("EdgeL", new Vector3(0.12f, 0.25f, 10f), new Vector3(-3.3f, 0.02f, 5f), _ink, false).transform.SetParent(root.transform, false);
                Box("EdgeR", new Vector3(0.12f, 0.25f, 10f), new Vector3(3.3f, 0.02f, 5f), _ink, false).transform.SetParent(root.transform, false);
                for (int i = 0; i < 5; i++) Box("Tick" + i, new Vector3(6.6f, 0.012f, 0.03f), new Vector3(0, 0.008f, i * 2f), _tone, false).transform.SetParent(root.transform, false);   // faint paving seams, never black bars (they read as hurdles)
                for (int i = 0; i < 10; i++) foreach (float x in new[] { -1f, 1f }) Box("Dash" + i + (x > 0 ? "R" : "L"), new Vector3(0.05f, 0.015f, 0.5f), new Vector3(x, 0.01f, 0.25f + i), _tone, false).transform.SetParent(root.transform, false);
                p.track = Save("TrackPiece", root);
            }
            var cityModel = Model("BuildingCity");
            if (cityModel != null) { var root = new GameObject("Building"); cityModel.transform.SetParent(root.transform, false); p.building = Save("Building", root); }
            else
            {
                var root = new GameObject("Building");
                Box("Block", new Vector3(3f, 6f, 4f), new Vector3(1.5f, 3f, 2f), _paper, false).transform.SetParent(root.transform, false);
                Box("Shade", new Vector3(1.2f, 6f, 4.02f), new Vector3(0.6f, 3f, 2f), _tone, false).transform.SetParent(root.transform, false);
                for (int r = 0; r < 4; r++) for (int c = 0; c < 2; c++) Box("Win" + r + c, new Vector3(0.06f, 0.5f, 0.6f), new Vector3(-0.01f, 1.2f + r * 1.2f, 1.0f + c * 1.6f), _ink, false).transform.SetParent(root.transform, false);
                Box("Tank", new Vector3(1.2f, 1.2f, 1.2f), new Vector3(1.5f, 6.6f, 2f), _paper, false).transform.SetParent(root.transform, false);
                p.building = Save("Building", root);
            }
            var containerModel = Model("Containers");
            if (containerModel != null) { var root = new GameObject("Containers"); containerModel.transform.SetParent(root.transform, false); p.container = Save("Containers", root); }
            else
            {
                var root = new GameObject("Containers");
                Box("Low", new Vector3(3f, 1.4f, 4f), new Vector3(1.5f, 0.7f, 2f), _paper, false).transform.SetParent(root.transform, false);
                Box("LowStripe", new Vector3(3.02f, 0.3f, 1f), new Vector3(1.5f, 0.7f, 2f), _accent, false).transform.SetParent(root.transform, false);
                Box("High", new Vector3(3f, 1.4f, 3f), new Vector3(1.6f, 2.1f, 2.4f), _tone, false).transform.SetParent(root.transform, false);
                Box("Crane", new Vector3(0.2f, 7f, 0.2f), new Vector3(2.8f, 3.5f, 0.5f), _ink, false).transform.SetParent(root.transform, false);
                Box("Arm", new Vector3(4f, 0.15f, 0.15f), new Vector3(1f, 7f, 0.5f), _ink, false).transform.SetParent(root.transform, false);
                p.container = Save("Containers", root);
            }
            var alleyModel = Model("BuildingAlley");
            if (alleyModel != null) { var root = new GameObject("Alley"); alleyModel.transform.SetParent(root.transform, false); p.alley = Save("Alley", root); }
            else
            {
                var root = new GameObject("Alley");
                Box("Block", new Vector3(2.4f, 9f, 4f), new Vector3(1.2f, 4.5f, 2f), _paper, false).transform.SetParent(root.transform, false);
                Box("Shade", new Vector3(0.9f, 9f, 4.02f), new Vector3(0.45f, 4.5f, 2f), _tone, false).transform.SetParent(root.transform, false);
                for (int r = 0; r < 6; r++) Box("Win" + r, new Vector3(0.06f, 0.4f, 1.2f), new Vector3(-0.01f, 1.0f + r * 1.3f, 2f), _ink, false).transform.SetParent(root.transform, false);
                Box("Wire", new Vector3(0.04f, 0.04f, 1.2f), new Vector3(-0.6f, 4.2f, 2f), _ink, false).transform.SetParent(root.transform, false);
                var lantern = GameObject.CreatePrimitive(PrimitiveType.Sphere); lantern.name = "Lantern"; Object.DestroyImmediate(lantern.GetComponent<Collider>());
                lantern.transform.SetParent(root.transform, false); lantern.transform.localPosition = new Vector3(-1.2f, 3.7f, 2f); lantern.transform.localScale = new Vector3(0.6f, 0.8f, 0.6f);
                lantern.GetComponent<Renderer>().sharedMaterial = _accent;
                p.alley = Save("Alley", root);
            }
            p.hero = Save("Hero", Hero());
            {
                var go = new GameObject("SfxWord");
                var t = go.AddComponent<TextMeshPro>();
                t.text = "WHAM!"; t.fontSize = 8; t.alignment = TextAlignmentOptions.Center; t.color = Color.white;
                var font = TMP_Settings.defaultFontAsset;
                if (font != null && font.material != null)
                {
                    // a saved material preset with the ink outline; setting outlineWidth here would instantiate one per word
                    string fp = Root + "/Materials/SfxWordFont.mat";
                    var fm = AssetDatabase.LoadAssetAtPath<Material>(fp);
                    if (fm == null) { fm = new Material(font.material); AssetDatabase.CreateAsset(fm, fp); }
                    fm.CopyPropertiesFromMaterial(font.material);
                    fm.EnableKeyword("OUTLINE_ON"); fm.SetFloat("_OutlineWidth", 0.3f); fm.SetColor("_OutlineColor", new Color(0.07f, 0.07f, 0.07f, 1f));
                    EditorUtility.SetDirty(fm);
                    t.fontSharedMaterial = fm;
                }
                p.word = Save("SfxWord", go);
            }
            return p;
        }

        // ---------- GLB models (Assets/InkRun/Models, imported by the glTFast package) ----------


        // Flat contact shadow under a prefab: cheap, reads at gameplay size, and grounds props on the paper track.
        static void Shadow(GameObject root, float sx, float sz, float x = 0, float z = 0)
        {
            var q = GameObject.CreatePrimitive(PrimitiveType.Quad); q.name = "Shadow";
            Object.DestroyImmediate(q.GetComponent<Collider>());
            q.transform.SetParent(root.transform, false);
            q.transform.localPosition = new Vector3(x, 0.012f, z);
            q.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            q.transform.localScale = new Vector3(sx, sz, 1f);
            q.GetComponent<Renderer>().sharedMaterial = _tone;
        }

        // Outfit GLBs carry 'Head' and 'Body' nodes with the same pivots as the hero's joints: parent each onto its joint.
        static bool AttachOutfit(GameObject heroModel, string id)
        {
            var m = Model("Outfit_" + char.ToUpper(id[0]) + id.Substring(1));
            if (m == null) return false;
            bool any = false;
            foreach (var jointName in new[] { "Head", "Body" })
            {
                var src = Find(m.transform, jointName); var joint = Find(heroModel.transform, jointName);
                if (src == null || joint == null) continue;
                src.SetParent(joint, false); src.localPosition = Vector3.zero; src.localRotation = Quaternion.identity; src.localScale = Vector3.one;
                src.name = "Outfit_" + id + "_" + jointName; src.gameObject.AddComponent<OutfitPart>().Outfit = id; any = true;
            }
            Object.DestroyImmediate(m);
            return any;
        }
        static GameObject Model(string name)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "/Models/" + name + ".glb");
            if (asset == null) { Debug.LogWarning("Ink Run: no imported model for " + name + " (install com.unity.cloud.gltfast); using primitives."); return null; }
            var go = Object.Instantiate(asset); go.name = "Model";
            go.transform.localScale = new Vector3(-1f, 1f, 1f);   // glTFast negates X on import; the models are authored in Unity coordinates, so mirror back (asymmetric facades and overhangs otherwise reach across the track)
            RemapInk(go);
            return go;
        }

        // GLB materials are named InkPaper / InkAccent / Ink / InkTone / InkSkin; swap them for the ink shader materials.
        static void RemapInk(GameObject go)
        {
            foreach (var r in go.GetComponentsInChildren<Renderer>(true))
            {
                var mats = r.sharedMaterials;
                for (int i = 0; i < mats.Length; i++)
                {
                    string n = mats[i] != null ? mats[i].name : "";
                    if (n.StartsWith("InkAccent")) mats[i] = _accent;
                    else if (n.StartsWith("InkPaper")) mats[i] = _paper;
                    else if (n.StartsWith("InkSkin")) mats[i] = _skin != null ? _skin : _paper;
                    else if (n.StartsWith("InkTone")) mats[i] = _tone;
                    else if (n.StartsWith("Ink")) mats[i] = _ink;
                }
                r.sharedMaterials = mats;
            }
        }

        static Transform Find(Transform root, string name)
        {
            foreach (var t in root.GetComponentsInChildren<Transform>(true)) if (t.name == name) return t;
            return null;
        }

        static GameObject Box(string name, Vector3 size, Vector3 pos, Material m, bool trigger)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube); go.name = name;
            go.transform.localScale = size; go.transform.localPosition = pos;
            go.GetComponent<Renderer>().sharedMaterial = m;
            var c = go.GetComponent<Collider>();
            if (trigger) c.isTrigger = true; else Object.DestroyImmediate(c);
            return go;
        }

        static GameObject Obstacle(ObstacleKind kind, GameObject body)
        {
            var root = new GameObject(kind.ToString());
            body.transform.SetParent(root.transform, false);
            root.AddComponent<Obstacle>().Kind = kind;
            return root;
        }

        static GameObject PickupPrefab(PickupKind kind)
        {
            var root = new GameObject(kind.ToString());
            var model = Model(kind.ToString());
            if (model != null) model.transform.SetParent(root.transform, false);
            else Box("Body", new Vector3(0.6f, 0.6f, 0.6f), new Vector3(0, 0.8f, 0), _accent, false).transform.SetParent(root.transform, false);
            var sc = root.AddComponent<SphereCollider>(); sc.isTrigger = true; sc.radius = 0.6f; sc.center = new Vector3(0, 0.8f, 0);
            root.AddComponent<Pickup>().Kind = kind;
            return root;
        }

        static GameObject Hero()
        {
            var root = new GameObject("Hero");
            var cap = root.AddComponent<CapsuleCollider>(); cap.isTrigger = true; cap.height = 1.8f; cap.radius = 0.35f; cap.center = new Vector3(0, 0.9f, 0);
            var rb = root.AddComponent<Rigidbody>(); rb.isKinematic = true; rb.useGravity = false;
            var heroModel = Model("Hero");
            if (heroModel != null)
            {
                heroModel.name = "Visual"; heroModel.transform.SetParent(root.transform, false);
                var heroAnim = heroModel.AddComponent<HeroAnimator>();
                heroAnim.Body = Find(heroModel.transform, "Body"); heroAnim.Head = Find(heroModel.transform, "Head");
                heroAnim.LegL = Find(heroModel.transform, "LegL"); heroAnim.LegR = Find(heroModel.transform, "LegR");
                heroAnim.ArmL = Find(heroModel.transform, "ArmL"); heroAnim.ArmR = Find(heroModel.transform, "ArmR");
                var partsM = new GameObject("Parts"); partsM.transform.SetParent(heroModel.transform, false);
                Part(partsM, "default", new Vector3(0.01f, 0.01f, 0.01f), new Vector3(0, 1.0f, 0), _accent);   // goggles are modeled; keep the node (inside the body) for the outfit swap
                if (!AttachOutfit(heroModel, "delivery")) Part(partsM, "delivery", new Vector3(0.55f, 0.12f, 0.55f), new Vector3(0, 1.78f, 0), _accent);
                if (!AttachOutfit(heroModel, "ronin")) Part(partsM, "ronin", new Vector3(0.56f, 0.06f, 0.56f), new Vector3(0, 1.64f, 0), _accent);
                root.AddComponent<HeroOutfit>().PartsRoot = partsM.transform;
                var pcM = root.AddComponent<PlayerController>(); pcM.Visual = heroModel.transform;
                return root;
            }
            var visual = new GameObject("Visual"); visual.transform.SetParent(root.transform, false);
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule); body.name = "Body";
            Object.DestroyImmediate(body.GetComponent<Collider>());
            body.transform.SetParent(visual.transform, false); body.transform.localPosition = new Vector3(0, 0.9f, 0); body.transform.localScale = new Vector3(0.6f, 0.42f, 0.5f);
            body.GetComponent<Renderer>().sharedMaterial = _accent;
            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere); head.name = "Head";
            Object.DestroyImmediate(head.GetComponent<Collider>());
            head.transform.SetParent(visual.transform, false); head.transform.localPosition = new Vector3(0, 1.55f, 0); head.transform.localScale = Vector3.one * 0.5f;
            head.GetComponent<Renderer>().sharedMaterial = _paper;
            // spiky hair: three ink wedges on top, parented to the head so they bob with it (head is scaled 0.5, so local sizes are doubled)
            float[] hairX = { -0.3f, 0f, 0.3f }, hairZ = { -0.1f, -0.05f, -0.1f }, hairRz = { 32f, 0f, -32f };
            for (int hi = 0; hi < 3; hi++)
            {
                float hx = hairX[hi], hz = hairZ[hi], rz = hairRz[hi];
                var spike = Box("Hair", new Vector3(0.22f, 0.5f, 0.2f), new Vector3(hx, 0.62f, hz), _ink, false);
                spike.transform.SetParent(head.transform, false); spike.transform.localRotation = Quaternion.Euler(-12f, 0f, rz);
            }
            var anim = visual.AddComponent<HeroAnimator>(); anim.Body = body.transform; anim.Head = head.transform;
            anim.LegL = Limb(visual, "LegL", new Vector3(-0.14f, 0.55f, 0), 0.5f, 0.16f, _ink);
            anim.LegR = Limb(visual, "LegR", new Vector3(0.14f, 0.55f, 0), 0.5f, 0.16f, _ink);
            anim.ArmL = Limb(visual, "ArmL", new Vector3(-0.36f, 1.15f, 0), 0.45f, 0.13f, _accent);
            anim.ArmR = Limb(visual, "ArmR", new Vector3(0.36f, 1.15f, 0), 0.45f, 0.13f, _accent);
            var parts = new GameObject("Parts"); parts.transform.SetParent(visual.transform, false);
            {
                var d = new GameObject("default"); d.transform.SetParent(parts.transform, false);
                Box("Strap", new Vector3(0.54f, 0.06f, 0.54f), new Vector3(0, 1.66f, 0), _ink, false).transform.SetParent(d.transform, false);       // goggle strap, seen from behind
                Box("Hood", new Vector3(0.46f, 0.3f, 0.2f), new Vector3(0, 1.5f, -0.2f), _accent, false).transform.SetParent(d.transform, false);     // hood bunched at the back of the neck
            }
            {
                // delivery: a cap with a brim and a parcel strapped to the back
                var d = new GameObject("delivery"); d.transform.SetParent(parts.transform, false);
                Box("Cap", new Vector3(0.56f, 0.12f, 0.56f), new Vector3(0, 1.8f, 0), _ink, false).transform.SetParent(d.transform, false);
                Box("Brim", new Vector3(0.34f, 0.04f, 0.22f), new Vector3(0, 1.76f, 0.34f), _ink, false).transform.SetParent(d.transform, false);
                Box("Parcel", new Vector3(0.5f, 0.42f, 0.3f), new Vector3(0, 1.05f, -0.42f), _tone, false).transform.SetParent(d.transform, false);
                Box("Tape", new Vector3(0.52f, 0.06f, 0.32f), new Vector3(0, 1.05f, -0.42f), _accent, false).transform.SetParent(d.transform, false);
            }
            {
                // ronin: a headband with tails and a bokken across the back
                var r = new GameObject("ronin"); r.transform.SetParent(parts.transform, false);
                Box("Headband", new Vector3(0.56f, 0.07f, 0.56f), new Vector3(0, 1.62f, 0), _accent, false).transform.SetParent(r.transform, false);
                var tail = Box("Tail", new Vector3(0.06f, 0.34f, 0.03f), new Vector3(0.18f, 1.46f, -0.3f), _accent, false); tail.transform.SetParent(r.transform, false); tail.transform.localRotation = Quaternion.Euler(10f, 0f, -20f);
                var bokken = Box("Bokken", new Vector3(0.06f, 1.15f, 0.06f), new Vector3(0.2f, 1.0f, -0.36f), _ink, false); bokken.transform.SetParent(r.transform, false); bokken.transform.localRotation = Quaternion.Euler(0f, 0f, 32f);
            }
            root.AddComponent<HeroOutfit>().PartsRoot = parts.transform;
            var pc = root.AddComponent<PlayerController>(); pc.Visual = visual.transform;
            return root;
        }
        // a limb is a joint pivot with a box hanging below it, so rotating the joint swings the limb
        static Transform Limb(GameObject parent, string name, Vector3 joint, float length, float thickness, Material m)
        {
            var pivot = new GameObject(name); pivot.transform.SetParent(parent.transform, false); pivot.transform.localPosition = joint;
            var box = Box("Bone", new Vector3(thickness, length, thickness), new Vector3(0, -length / 2f, 0), m, false);
            box.transform.SetParent(pivot.transform, false);
            var foot = Box("Tip", new Vector3(thickness * 1.3f, thickness * 0.8f, thickness * 2f), new Vector3(0, -length, thickness * 0.4f), name.StartsWith("Leg") ? _paper : m, false);
            foot.transform.SetParent(pivot.transform, false);
            return pivot.transform;
        }

        static void Part(GameObject parent, string id, Vector3 size, Vector3 pos, Material m)
        {
            var go = Box(id, size, pos, m, false); go.transform.SetParent(parent.transform, false);
        }

        static GameObject Save(string name, GameObject go)
        {
            string path = Root + "/Prefabs/" + name + ".prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        // ---------- scene ----------

        static void Scene(Tuning tuning, OutfitDefinition[] outfits, PrefabSet p, AreaDefinition[] areas)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var light = new GameObject("Directional Light").AddComponent<Light>();
            light.type = LightType.Directional; light.transform.rotation = Quaternion.Euler(50f, -35f, 0f);

            var rig = new GameObject("CameraRig"); rig.AddComponent<CameraShake>();
            var cam = new GameObject("Main Camera").AddComponent<Camera>(); cam.tag = "MainCamera";
            cam.transform.SetParent(rig.transform, false);
            cam.transform.position = new Vector3(0, 3.4f, -6.5f); cam.transform.LookAt(new Vector3(0, 1.2f, 10f));
            cam.backgroundColor = Color.white; cam.clearFlags = CameraClearFlags.SolidColor;
            cam.gameObject.AddComponent<CameraFollow>();
            cam.gameObject.AddComponent<AudioListener>();

            // distant skyline silhouette, two rows, does not scroll
            var sky = new GameObject("Skyline");
            var rng = new System.Random(7);
            for (int row = 0; row < 2; row++)
                for (float x = -160f; x < 160f;)
                {
                    float w = 6f + (float)rng.NextDouble() * 10f, h = 4f + (float)rng.NextDouble() * (row == 0 ? 22f : 14f);
                    var b = Box("Tower", new Vector3(w, h, 4f), new Vector3(x + w / 2f, h / 2f - 0.5f, 150f + row * 30f), row == 0 ? _ink : _tone, false);
                    b.transform.SetParent(sky.transform, false);
                    x += w + (float)rng.NextDouble() * 3f;
                }

            var sun = GameObject.CreatePrimitive(PrimitiveType.Cylinder); sun.name = "Sun";
            Object.DestroyImmediate(sun.GetComponent<Collider>());
            sun.transform.position = new Vector3(18f, 20f, 230f); sun.transform.rotation = Quaternion.Euler(90f, 0f, 0f); sun.transform.localScale = new Vector3(44f, 0.1f, 44f);
            sun.GetComponent<Renderer>().sharedMaterial = _accent;
            var moon = GameObject.CreatePrimitive(PrimitiveType.Cylinder); moon.name = "MoonMask";
            Object.DestroyImmediate(moon.GetComponent<Collider>());
            moon.transform.SetParent(sun.transform, false); moon.transform.localPosition = new Vector3(-0.35f, -0.5f, 0.25f); moon.transform.localScale = new Vector3(0.86f, 1f, 0.86f);
            moon.GetComponent<Renderer>().sharedMaterial = _paper; moon.SetActive(false);

            var worldGo = new GameObject("World"); var world = worldGo.AddComponent<WorldMover>();
            var hero = (GameObject)PrefabUtility.InstantiatePrefab(p.hero);
            var shadow = GameObject.CreatePrimitive(PrimitiveType.Cylinder); shadow.name = "HeroShadow";
            Object.DestroyImmediate(shadow.GetComponent<Collider>());
            shadow.GetComponent<Renderer>().sharedMaterial = _tone;
            shadow.AddComponent<GroundShadow>().Target = hero.transform;
            var crewModel = Model("Crew");
            if (crewModel != null)
            {
                var crew = new GameObject("Crew"); crewModel.name = "Visual"; crewModel.transform.SetParent(crew.transform, false);
                var ch = crew.AddComponent<Chaser>();
                ch.Body = Find(crewModel.transform, "Body"); ch.Head = Find(crewModel.transform, "Head");
                ch.LegL = Find(crewModel.transform, "LegL"); ch.LegR = Find(crewModel.transform, "LegR");
                ch.ArmL = Find(crewModel.transform, "ArmL"); ch.ArmR = Find(crewModel.transform, "ArmR");
                crew.transform.position = new Vector3(0, 0, -2.4f);
            }
            var kaijuModel = Model("KaijuShadow");
            if (kaijuModel != null)
            {
                var kaiju = new GameObject("KaijuShadow"); kaijuModel.transform.SetParent(kaiju.transform, false);
                kaiju.AddComponent<KaijuShadow>();
            }
            var input = new GameObject("Input").AddComponent<SwipeInput>();
            hero.GetComponent<PlayerController>().Input = input;

            var areasGo = new GameObject("Areas"); var areaCtl = areasGo.AddComponent<AreaController>();
            areaCtl.Areas = areas; areaCtl.SunDisc = sun.transform; areaCtl.MoonMask = moon.transform;

            var spawnerGo = new GameObject("Spawner"); var spawner = spawnerGo.AddComponent<TrackSpawner>();
            spawner.World = world; spawner.Areas = areaCtl;
            spawner.BarrierPrefab = p.barrier; spawner.SignPrefab = p.sign; spawner.WallPrefab = p.wall; spawner.GapPrefab = p.gap;
            spawner.CoinPrefab = p.coin; spawner.MagnetPrefab = p.magnet; spawner.DashPrefab = p.dash; spawner.TrackPiecePrefab = p.track;
            foreach (var guid in AssetDatabase.FindAssets("t:PatternDefinition", new[] { Root + "/Data/Patterns" }))
                spawner.Patterns.Add(AssetDatabase.LoadAssetAtPath<PatternDefinition>(AssetDatabase.GUIDToAssetPath(guid)));

            var gmGo = new GameObject("GameManager"); var gm = gmGo.AddComponent<GameManager>();
            gmGo.AddComponent<InkAudio>();
            gm.Tuning = tuning; gm.Player = hero.GetComponent<PlayerController>(); gm.World = world; gm.Spawner = spawner; gm.Areas = areaCtl; gm.Outfits = outfits;

            var words = new GameObject("SfxWords").AddComponent<SfxWords>(); words.WordPrefab = p.word.GetComponent<TMP_Text>();
            var gang = new GameObject("RivalGang").AddComponent<RivalGang>(); gang.Ink = _ink; gang.Paper = _paper;
            var thugModel = Model("Thug");
            if (thugModel != null) { thugModel.name = "Thug"; gang.ThugPrefab = Save("Thug", thugModel); }

            UI(p);

            string path = Root + "/Scenes/Run.unity";
            EditorSceneManager.SaveScene(scene, path);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(path, true) };
            PlayerSettings.productName = "Ink Run";
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        }

        // ---------- UI ----------

        static readonly Color Ink = new Color(0.07f, 0.07f, 0.07f);
        static readonly Color Red = new Color(0.878f, 0.125f, 0.165f);

        static void UI(PrefabSet p)
        {
            var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1080, 1920); scaler.matchWidthOrHeight = 1f;
#if ENABLE_INPUT_SYSTEM
            new GameObject("EventSystem", typeof(EventSystem), typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
#else
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
#endif
            var root = canvasGo.transform;

            // speed lines
            var lines = Full(root, "SpeedLines"); var raw = lines.gameObject.AddComponent<RawImage>(); raw.raycastTarget = false;
            var lineShader = Shader.Find("InkRun/InkSpeedLines");
            if (lineShader != null)
            {
                string mp = Root + "/Materials/InkSpeedLines.mat";
                var lm = AssetDatabase.LoadAssetAtPath<Material>(mp);
                if (lm == null) { lm = new Material(lineShader); AssetDatabase.CreateAsset(lm, mp); }
                raw.material = lm;
            }
            lines.gameObject.AddComponent<SpeedLines>().Image = raw;

            // HUD
            var hudView = Full(root, "HUD").gameObject.AddComponent<HudView>();
            var hud = Safe(hudView.transform, "Panel"); hudView.Panel = hud.gameObject; hud.gameObject.SetActive(false);
            hudView.PauseButton = Button(hud, "PauseButton", "II", new Vector2(1f, 1f), new Vector2(-85, -210), new Vector2(90, 70), Color.white, Ink);
            // caption boxes behind the readouts, so they stay readable on the night chapter's ink sky
            var capDist = Group(hud, "DistanceBox", new Vector2(0.5f, 1f), new Vector2(460, 120)); capDist.anchoredPosition = new Vector2(0, -70); Panel(capDist, Color.white, Ink);
            var capCoins = Group(hud, "CoinsBox", new Vector2(1f, 1f), new Vector2(220, 84)); capCoins.anchoredPosition = new Vector2(-40, -70); Panel(capCoins, Color.white, Ink);
            var capBest = Group(hud, "BestBox", new Vector2(0f, 1f), new Vector2(280, 70)); capBest.anchoredPosition = new Vector2(40, -70); Panel(capBest, Color.white, Ink);
            hudView.Distance = Text(capDist, "Distance", "0m", 72, new Vector2(0.5f, 0.5f), new Vector2(0, 0));
            hudView.Coins = Text(capCoins, "Coins", "0", 52, new Vector2(0.5f, 0.5f), new Vector2(0, 0));
            hudView.Best = Text(capBest, "Best", "BEST 0", 34, new Vector2(0.5f, 0.5f), new Vector2(0, 0));
            var assist = Text(hud, "AssistTag", "ASSIST", 28, new Vector2(0f, 1f), new Vector2(180, -160)); hudView.AssistTag = assist.gameObject; assist.gameObject.SetActive(false);
            hudView.DashBar = Bar(hud, "DashBar", new Vector2(0, -225)); hudView.MagnetBar = Bar(hud, "MagnetBar", new Vector2(0, -262));
            var card = Group(hud, "ChapterCard", new Vector2(0.5f, 0.78f), new Vector2(900, 220)); Panel(card, Color.white, Ink);
            hudView.ChapterLabel = Text(card, "Label", "CHAPTER 1", 64, new Vector2(0.5f, 0.5f), new Vector2(0, 50));
            hudView.ChapterName = Text(card, "Name", "CITY ROOFTOPS", 48, new Vector2(0.5f, 0.5f), new Vector2(0, -40)); hudView.ChapterName.color = Red;
            hudView.ChapterCard = card.gameObject; card.gameObject.SetActive(false);
            var bubble = Group(hud, "NearRecordBubble", new Vector2(0.5f, 0.45f), new Vector2(760, 160)); Panel(bubble, Color.white, Ink);
            hudView.NearRecordText = Text(bubble, "Text", "200m to your record!", 40, new Vector2(0.5f, 0.5f), Vector2.zero);
            hudView.NearRecordBubble = bubble.gameObject; bubble.gameObject.SetActive(false);

            // Start
            var startView = Full(root, "Start").gameObject.AddComponent<StartView>();
            var start = Safe(startView.transform, "Panel"); startView.Panel = start.gameObject; Panel(start, new Color(1, 1, 1, 0.92f), Color.clear);
            startView.TapAnywhere = start.gameObject.AddComponent<Button>();   // tap anywhere to run; StartView wires it
            Text(start, "Kicker", "VOLUME 1", 36, new Vector2(0.5f, 1f), new Vector2(0, -300));
            Text(start, "Title", "INK\nRUN", 180, new Vector2(0.5f, 1f), new Vector2(0, -380));       // two lines, about 420 px tall
            Text(start, "Hint", "Swipe to change lanes, up to jump, down to slide.", 34, new Vector2(0.5f, 1f), new Vector2(0, -850));
            startView.TapToRun = Button(start, "TapToRun", "TAP TO RUN", new Vector2(0.5f, 1f), new Vector2(0, -980), new Vector2(560, 130), Red, Color.white);
            startView.BestText = Text(start, "BestValue", "0", 70, new Vector2(0.5f, 1f), new Vector2(-180, -1110)); Text(start, "BestLabel", "BEST M", 26, new Vector2(0.5f, 1f), new Vector2(-180, -1200));
            startView.BankText = Text(start, "BankValue", "0", 70, new Vector2(0.5f, 1f), new Vector2(180, -1110)); Text(start, "BankLabel", "COINS BANKED", 26, new Vector2(0.5f, 1f), new Vector2(180, -1200));
            var missions = Group(start, "Missions", new Vector2(0.5f, 1f), new Vector2(900, 330)); missions.anchoredPosition = new Vector2(0, -1400);
            var vlg = missions.gameObject.AddComponent<VerticalLayoutGroup>(); vlg.spacing = 12; vlg.childForceExpandHeight = false; vlg.childControlHeight = false; vlg.childControlWidth = true; vlg.childForceExpandWidth = true;
            var mlv = startView.gameObject.AddComponent<MissionListView>(); mlv.Container = missions; mlv.RowPrefab = MissionRow(); startView.Missions = mlv;
            startView.WardrobeButton = Button(start, "WardrobeButton", "WARDROBE", new Vector2(0.5f, 0f), new Vector2(-200, 130), new Vector2(360, 100), Color.white, Ink);
            startView.SoundButton = Button(start, "SoundButton", "SOUND: ON", new Vector2(0.5f, 0f), new Vector2(200, 130), new Vector2(360, 100), Color.white, Ink);
            startView.SoundLabel = startView.SoundButton.GetComponentInChildren<TMP_Text>();
            startView.VersionText = Text(start, "Version", Story.Build, 30, new Vector2(0.5f, 0f), new Vector2(0, 30)); startView.VersionText.fontStyle = FontStyles.Bold;
            var chip = Text(start, "AssistChip", "Assist on: slower start, fewer obstacles", 28, new Vector2(0.5f, 1f), new Vector2(0, -1250)); startView.AssistChip = chip.gameObject; chip.gameObject.SetActive(false);
            var bundle = Group(start, "StarterBundle", new Vector2(0.5f, 1f), new Vector2(900, 100)); bundle.anchoredPosition = new Vector2(0, -1290); Panel(bundle, Color.white, Ink);
            Text(bundle, "Text", "Starter bundle: 1,500 coins + Delivery outfit", 30, new Vector2(0.35f, 0.5f), Vector2.zero);
            startView.StarterBundleButton = Button(bundle, "Buy", "BUY", new Vector2(0.85f, 0.5f), Vector2.zero, new Vector2(180, 80), Red, Color.white);
            startView.StarterBundlePanel = bundle.gameObject;

            // Wardrobe (toggles itself)
            var wardrobe = Safe(root, "Wardrobe"); var wv = wardrobe.gameObject.AddComponent<WardrobeView>(); Panel(wardrobe, Color.white, Color.clear);
            Text(wardrobe, "Kicker", "WARDROBE", 48, new Vector2(0.5f, 1f), new Vector2(0, -330));
            var cards = Group(wardrobe, "Cards", new Vector2(0.5f, 0.55f), new Vector2(1000, 520));
            var hlg = cards.gameObject.AddComponent<HorizontalLayoutGroup>(); hlg.spacing = 20; hlg.childAlignment = TextAnchor.MiddleCenter; hlg.childForceExpandWidth = false; hlg.childForceExpandHeight = false; hlg.childControlWidth = false; hlg.childControlHeight = false;
            wv.Container = cards; wv.CardPrefab = OutfitCard();
            wv.BankLine = Text(wardrobe, "BankLine", "Coins banked: 0", 34, new Vector2(0.5f, 0f), new Vector2(0, 420));
            wv.Back = Button(wardrobe, "Back", "BACK", new Vector2(0.5f, 0f), new Vector2(0, 260), new Vector2(360, 110), Red, Color.white);
            startView.WardrobePanel = wardrobe.gameObject; wardrobe.gameObject.SetActive(false);

            // Chapter strip: three captions over the live run at a chapter change (and the cold open)
            {
                var csRoot = Full(root, "ChapterStrip"); var cs = csRoot.gameObject.AddComponent<ChapterStripView>();
                var csPanel = Full(csRoot, "Panel"); cs.Panel = csPanel.gameObject;
                for (int i = 0; i < 3; i++)
                {
                    var t = Text(csPanel, "Caption" + i, "", 34, new Vector2(0.5f, 1f), new Vector2(0, -230 - i * 84)); t.color = Color.black;
                    var bgGo = new GameObject("Box"); bgGo.transform.SetParent(t.transform, false); bgGo.transform.SetAsFirstSibling();
                    var bgRt = bgGo.AddComponent<RectTransform>(); bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one; bgRt.sizeDelta = new Vector2(40f, 20f);
                    var bgImg = bgGo.AddComponent<Image>(); bgImg.color = new Color(1f, 1f, 1f, 0.92f); bgImg.raycastTarget = false;
                    cs.Captions[i] = t;
                }
            }
            // Crash strip (plays during the Crashed freeze-frame, under the revive page)
            {
                var stripRoot = Full(root, "CrashStrip"); var csv = stripRoot.gameObject.AddComponent<CrashStripView>();
                var stripPanel = Full(stripRoot, "Panel"); csv.Panel = stripPanel.gameObject;
                var bg = stripPanel.gameObject.AddComponent<Image>(); bg.color = new Color(1f, 1f, 1f, 0.80f);   // the crash freeze-frame stays visible behind the strip
                var imgGo = new GameObject("Strip"); imgGo.transform.SetParent(stripPanel, false);
                var imgRt = imgGo.AddComponent<RectTransform>(); imgRt.anchorMin = imgRt.anchorMax = new Vector2(0.5f, 0.55f); imgRt.sizeDelta = new Vector2(1000f, 1000f * 9f / 16f * 3f); // one third of a 16:9 strip, shown as a tall panel
                var stripImg = imgGo.AddComponent<RawImage>(); csv.Strip = stripImg; csv.StripRect = imgRt;
                var textRoot = new GameObject("Text"); textRoot.transform.SetParent(stripPanel, false); textRoot.AddComponent<RectTransform>(); csv.TextRoot = textRoot;
                csv.TextApproach = Text(textRoot.transform, "Approach", "", 40, new Vector2(0.5f, 0.78f), Vector2.zero); csv.TextApproach.color = Color.black;
                csv.TextImpact = Text(textRoot.transform, "Impact", "", 140, new Vector2(0.5f, 0.52f), Vector2.zero); csv.TextImpact.color = new Color(0.878f, 0.125f, 0.165f);
                csv.TextAftermath = Text(textRoot.transform, "Aftermath", "", 36, new Vector2(0.5f, 0.26f), Vector2.zero); csv.TextAftermath.color = Color.black;
                var flashGo = new GameObject("Flash"); flashGo.transform.SetParent(stripPanel, false); var flashRt = flashGo.AddComponent<RectTransform>(); flashRt.anchorMin = Vector2.zero; flashRt.anchorMax = Vector2.one; flashRt.sizeDelta = Vector2.zero;
                var flash = flashGo.AddComponent<Image>(); flash.color = new Color(1f, 1f, 1f, 0f); flash.raycastTarget = false; csv.Flash = flash;
            }
            // Revive
            var reviveView = Full(root, "Revive").gameObject.AddComponent<ReviveView>();
            var revive = Safe(reviveView.transform, "Panel"); reviveView.Panel = revive.gameObject; Panel(revive, new Color(1, 1, 1, 0.84f), Color.clear); revive.gameObject.SetActive(false);   // the crash freeze-frame stays visible
            var shadow = Group(revive, "KaijuShadow", new Vector2(0.78f, 0.62f), new Vector2(1100, 1100));   // the kaiju, only ever a shadow in Volume 1
            var shadowImg = shadow.gameObject.AddComponent<Image>(); shadowImg.color = new Color(0.07f, 0.07f, 0.07f, 0.5f); shadowImg.raycastTarget = false; reviveView.KaijuShadow = shadowImg;
            var p1 = Group(revive, "Panel1", new Vector2(0.5f, 1f), new Vector2(940, 190)); p1.anchoredPosition = new Vector2(0, -360); Panel(p1, Ink, Ink);
            reviveView.Panel1 = Text(p1, "Text", Story.RevivePanels[0], 36, new Vector2(0.5f, 0.5f), Vector2.zero); reviveView.Panel1.color = Color.white; reviveView.Panel1.rectTransform.sizeDelta = new Vector2(880, 160);
            var p2 = Group(revive, "Panel2", new Vector2(0.5f, 1f), new Vector2(940, 190)); p2.anchoredPosition = new Vector2(0, -570); Panel(p2, Color.white, Ink);
            reviveView.Panel2 = Text(p2, "Text", Story.RevivePanels[1], 34, new Vector2(0.5f, 0.5f), Vector2.zero); reviveView.Panel2.rectTransform.sizeDelta = new Vector2(880, 160);
            reviveView.DistanceLine = Text(revive, "DistanceLine", "0m and counting.", 34, new Vector2(0.5f, 1f), new Vector2(0, -790));
            var title = Text(revive, "Title", "TO BE\nCONTINUED...?", 110, new Vector2(0.5f, 0.5f), new Vector2(0, -80)); title.color = Red;
            reviveView.Countdown = Text(revive, "Countdown", "5", 80, new Vector2(0.5f, 0.5f), new Vector2(0, -300));
            reviveView.KeepRunning = Button(revive, "KeepRunning", "KEEP RUNNING", new Vector2(0.5f, 0.5f), new Vector2(0, -460), new Vector2(760, 140), Red, Color.white);
            Text(revive, "Note", "Revive = one rewarded ad", 28, new Vector2(0.5f, 0.5f), new Vector2(0, -560));
            reviveView.GiveUp = Button(revive, "GiveUp", "Give up", new Vector2(0.5f, 0.5f), new Vector2(0, -670), new Vector2(300, 90), Color.clear, Ink);

            // Results
            var resultsView = Full(root, "Results").gameObject.AddComponent<ResultsView>();
            var results = Safe(resultsView.transform, "Panel"); resultsView.Panel = results.gameObject; Panel(results, new Color(1, 1, 1, 0.92f), Color.clear); results.gameObject.SetActive(false);
            var burst = Text(results, "RecordBurst", "NEW RECORD!", 84, new Vector2(0.5f, 0.84f), Vector2.zero); burst.color = Red; burst.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 7f); resultsView.RecordBurst = burst.gameObject; burst.gameObject.SetActive(false);
            resultsView.Kicker = Text(results, "Kicker", "END OF CHAPTER 1", 40, new Vector2(0.5f, 0.5f), new Vector2(0, 330));
            resultsView.Distance = Text(results, "Distance", "0", 90, new Vector2(0.5f, 0.5f), new Vector2(-300, 180)); Text(results, "DistanceLabel", "METERS", 26, new Vector2(0.5f, 0.5f), new Vector2(-300, 110));
            resultsView.Coins = Text(results, "Coins", "0", 90, new Vector2(0.5f, 0.5f), new Vector2(0, 180)); Text(results, "CoinsLabel", "COINS", 26, new Vector2(0.5f, 0.5f), new Vector2(0, 110));
            resultsView.Best = Text(results, "Best", "0", 90, new Vector2(0.5f, 0.5f), new Vector2(300, 180)); Text(results, "BestLabel", "BEST", 26, new Vector2(0.5f, 0.5f), new Vector2(300, 110));
            resultsView.DoubleCoins = Button(results, "DoubleCoins", "SAVE THE HAUL: 2× COINS (AD)", new Vector2(0.5f, 0.5f), new Vector2(0, -20), new Vector2(860, 120), Color.white, Ink);
            resultsView.DoubleLabel = resultsView.DoubleCoins.GetComponentInChildren<TMP_Text>();
            resultsView.RunAgain = Button(results, "RunAgain", "RUN AGAIN", new Vector2(0.5f, 0.5f), new Vector2(0, -190), new Vector2(560, 130), Red, Color.white);
            resultsView.MissionLine = Text(results, "MissionLine", "", 30, new Vector2(0.5f, 0.5f), new Vector2(0, -320));
            resultsView.BankLine = Text(results, "BankLine", "Coins banked: 0", 30, new Vector2(0.5f, 0.5f), new Vector2(0, -380));

            // Pause
            var pauseView = Full(root, "Pause").gameObject.AddComponent<PauseView>();
            var pause = Safe(pauseView.transform, "Panel"); pauseView.Panel = pause.gameObject; Panel(pause, new Color(1, 1, 1, 0.92f), Color.clear);
            pauseView.Resume = pause.gameObject.AddComponent<Button>();
            Text(pause, "Title", "PAUSED", 110, new Vector2(0.5f, 0.5f), new Vector2(0, 60));
            Text(pause, "Hint", "Tap to keep running", 36, new Vector2(0.5f, 0.5f), new Vector2(0, -60));
            pause.gameObject.SetActive(false);
        }

        static RectTransform Safe(Transform parent, string name)
        {
            var rt = Full(parent, name);
            rt.gameObject.AddComponent<SafeArea>();
            return rt;
        }

        static RectTransform Full(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform)); go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>(); rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            return rt;
        }

        static RectTransform Group(Transform parent, string name, Vector2 anchor, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform)); go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>(); rt.anchorMin = rt.anchorMax = anchor; rt.pivot = anchor; rt.sizeDelta = size; rt.anchoredPosition = Vector2.zero;
            return rt;
        }

        static void Panel(RectTransform rt, Color fill, Color border)
        {
            var img = rt.gameObject.AddComponent<Image>(); img.color = fill;
            if (border.a > 0f) { var o = rt.gameObject.AddComponent<Outline>(); o.effectColor = border; o.effectDistance = new Vector2(4, -4); }
        }

        static TMP_Text Text(Transform parent, string name, string text, float size, Vector2 anchor, Vector2 pos)
        {
            var go = new GameObject(name, typeof(RectTransform)); go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>(); rt.anchorMin = rt.anchorMax = anchor; rt.pivot = new Vector2(0.5f, anchor.y); rt.anchoredPosition = pos; rt.sizeDelta = new Vector2(980, size * 1.4f * (text.Split('\n').Length) + 10f);
            var t = go.AddComponent<TextMeshProUGUI>(); t.text = text; t.fontSize = size; t.alignment = TextAlignmentOptions.Center; t.color = Ink; t.raycastTarget = false; t.textWrappingMode = TextWrappingModes.Normal;
            return t;
        }

        static Button Button(Transform parent, string name, string label, Vector2 anchor, Vector2 pos, Vector2 size, Color fill, Color textColor)
        {
            var go = new GameObject(name, typeof(RectTransform)); go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>(); rt.anchorMin = rt.anchorMax = anchor; rt.pivot = new Vector2(0.5f, 0.5f); rt.anchoredPosition = pos; rt.sizeDelta = size;
            var img = go.AddComponent<Image>(); img.color = fill;
            if (fill.a > 0f) { var o = go.AddComponent<Outline>(); o.effectColor = Ink; o.effectDistance = new Vector2(5, -5); }
            var b = go.AddComponent<Button>();
            var t = Text(go.transform, "Label", label, size.y * 0.42f, new Vector2(0.5f, 0.5f), Vector2.zero); t.color = textColor; t.rectTransform.sizeDelta = size;
            return b;
        }

        static Image Bar(Transform parent, string name, Vector2 pos)
        {
            var rootRt = Group(parent, name + "Root", new Vector2(0.5f, 1f), new Vector2(300, 24)); rootRt.anchoredPosition = pos;
            var bg = rootRt.gameObject.AddComponent<Image>(); bg.color = Color.white;
            var o = rootRt.gameObject.AddComponent<Outline>(); o.effectColor = Ink; o.effectDistance = new Vector2(2, -2);
            var fill = Full(rootRt, "Fill"); var img = fill.gameObject.AddComponent<Image>(); img.color = Red;
            var label = Text(rootRt, "Label", name.Replace("Bar", "").ToUpper(), 20, new Vector2(0f, 0.5f), new Vector2(-70, 0)); label.alignment = TextAlignmentOptions.Right; label.rectTransform.sizeDelta = new Vector2(120, 30);
            rootRt.gameObject.SetActive(false);
            return img;
        }

        static GameObject MissionRow()
        {
            var row = new GameObject("MissionRow", typeof(RectTransform)); row.GetComponent<RectTransform>().sizeDelta = new Vector2(900, 100);
            var le = row.AddComponent<LayoutElement>(); le.preferredHeight = 100;
            var img = row.AddComponent<Image>(); img.color = Color.white; var o = row.AddComponent<Outline>(); o.effectColor = Ink; o.effectDistance = new Vector2(3, -3);
            var t = Text(row.transform, "Text", "Jump 50 times", 30, new Vector2(0f, 0.7f), new Vector2(330, 0)); t.alignment = TextAlignmentOptions.Left; t.rectTransform.sizeDelta = new Vector2(640, 44);
            var c = Text(row.transform, "Count", "0/50", 30, new Vector2(1f, 0.7f), new Vector2(-100, 0)); c.alignment = TextAlignmentOptions.Right; c.rectTransform.sizeDelta = new Vector2(180, 44);
            var track = Group(row.transform, "Track", new Vector2(0.5f, 0.15f), new Vector2(860, 12)); track.gameObject.AddComponent<Image>().color = new Color(0.88f, 0.88f, 0.88f);
            var bar = Full(track, "Bar"); var bi = bar.gameObject.AddComponent<Image>(); bi.color = Red;
            return Save("MissionRow", row);
        }

        static GameObject OutfitCard()
        {
            var card = new GameObject("OutfitCard", typeof(RectTransform)); card.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 460);
            var le = card.AddComponent<LayoutElement>(); le.preferredWidth = 300; le.preferredHeight = 460;
            var img = card.AddComponent<Image>(); img.color = Color.white; var o = card.AddComponent<Outline>(); o.effectColor = Ink; o.effectDistance = new Vector2(4, -4);
            Group(card.transform, "Preview", new Vector2(0.5f, 1f), new Vector2(240, 220)).anchoredPosition = new Vector2(0, -20);
            Text(card.transform, "Name", "Default", 40, new Vector2(0.5f, 1f), new Vector2(0, -260));
            Text(card.transform, "Desc", "Hoodie and goggles", 24, new Vector2(0.5f, 1f), new Vector2(0, -315));
            Button(card.transform, "Button", "WEAR", new Vector2(0.5f, 0f), new Vector2(0, 60), new Vector2(220, 80), Red, Color.white);
            return Save("OutfitCard", card);
        }
    }
}
#endif
