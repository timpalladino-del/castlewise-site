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
        static Material _paper, _accent, _ink, _tone;

        [MenuItem("Ink Run/Build gray-box scene")]
        public static void Build()
        {
            if (TMP_Settings.instance == null)
            {
                Debug.LogWarning("Ink Run: import TextMeshPro first (Window > TextMeshPro > Import TMP Essential Resources), then run this menu again.");
                return;
            }
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
            EditorUtility.SetDirty(a);
            return a;
        }

        // ---------- prefabs ----------

        struct PrefabSet { public GameObject barrier, sign, wall, gap, coin, magnet, dash, track, building, container, alley, hero, word; }

        static PrefabSet Prefabs()
        {
            var p = new PrefabSet();
            {
                var root = Obstacle(ObstacleKind.Barrier, Box("Body", new Vector3(1.6f, 0.8f, 0.4f), new Vector3(0, 0.4f, 0), _paper, true));
                for (int i = 0; i < 3; i++) { var stripe = Box("Stripe" + i, new Vector3(0.22f, 0.82f, 0.42f), new Vector3(-0.5f + i * 0.5f, 0.4f, 0), _accent, false); stripe.transform.SetParent(root.transform, false); stripe.transform.localRotation = Quaternion.Euler(0, 0, 18f); }
                Box("FootL", new Vector3(0.1f, 0.1f, 0.6f), new Vector3(-0.65f, 0.05f, 0), _ink, false).transform.SetParent(root.transform, false);
                Box("FootR", new Vector3(0.1f, 0.1f, 0.6f), new Vector3(0.65f, 0.05f, 0), _ink, false).transform.SetParent(root.transform, false);
                p.barrier = Save("Barrier", root);
            }
            {
                var root = new GameObject("Sign");
                Box("Board", new Vector3(1.6f, 0.9f, 0.3f), new Vector3(0, 1.75f, 0), _paper, true).transform.SetParent(root.transform, false);
                Box("PostL", new Vector3(0.1f, 2.2f, 0.1f), new Vector3(-0.75f, 1.1f, 0), _ink, false).transform.SetParent(root.transform, false);
                Box("PostR", new Vector3(0.1f, 2.2f, 0.1f), new Vector3(0.75f, 1.1f, 0), _ink, false).transform.SetParent(root.transform, false);
                var kanji = new GameObject("Kanji"); kanji.transform.SetParent(root.transform, false); kanji.transform.localPosition = new Vector3(0, 1.75f, -0.17f);
                var kt = kanji.AddComponent<TextMeshPro>(); kt.text = "STOP"; kt.fontSize = 4;   // swap for 止 once a Japanese TMP font is added kt.alignment = TextAlignmentOptions.Center; kt.color = new Color(0.878f, 0.125f, 0.165f);
                root.AddComponent<Obstacle>().Kind = ObstacleKind.Sign;
                p.sign = Save("Sign", root);
            }
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
                for (int i = 0; i < 5; i++) Box("Tick" + i, new Vector3(6.6f, 0.02f, 0.07f), new Vector3(0, 0.01f, i * 2f), _ink, false).transform.SetParent(root.transform, false);
                for (int i = 0; i < 10; i++) foreach (float x in new[] { -1f, 1f }) Box("Dash" + i + (x > 0 ? "R" : "L"), new Vector3(0.05f, 0.015f, 0.5f), new Vector3(x, 0.01f, 0.25f + i), _tone, false).transform.SetParent(root.transform, false);
                p.track = Save("TrackPiece", root);
            }
            {
                var root = new GameObject("Building");
                Box("Block", new Vector3(3f, 6f, 4f), new Vector3(1.5f, 3f, 2f), _paper, false).transform.SetParent(root.transform, false);
                Box("Shade", new Vector3(1.2f, 6f, 4.02f), new Vector3(0.6f, 3f, 2f), _tone, false).transform.SetParent(root.transform, false);
                for (int r = 0; r < 4; r++) for (int c = 0; c < 2; c++) Box("Win" + r + c, new Vector3(0.06f, 0.5f, 0.6f), new Vector3(-0.01f, 1.2f + r * 1.2f, 1.0f + c * 1.6f), _ink, false).transform.SetParent(root.transform, false);
                Box("Tank", new Vector3(1.2f, 1.2f, 1.2f), new Vector3(1.5f, 6.6f, 2f), _paper, false).transform.SetParent(root.transform, false);
                p.building = Save("Building", root);
            }
            {
                var root = new GameObject("Containers");
                Box("Low", new Vector3(3f, 1.4f, 4f), new Vector3(1.5f, 0.7f, 2f), _paper, false).transform.SetParent(root.transform, false);
                Box("LowStripe", new Vector3(3.02f, 0.3f, 1f), new Vector3(1.5f, 0.7f, 2f), _accent, false).transform.SetParent(root.transform, false);
                Box("High", new Vector3(3f, 1.4f, 3f), new Vector3(1.6f, 2.1f, 2.4f), _tone, false).transform.SetParent(root.transform, false);
                Box("Crane", new Vector3(0.2f, 7f, 0.2f), new Vector3(2.8f, 3.5f, 0.5f), _ink, false).transform.SetParent(root.transform, false);
                Box("Arm", new Vector3(4f, 0.15f, 0.15f), new Vector3(1f, 7f, 0.5f), _ink, false).transform.SetParent(root.transform, false);
                p.container = Save("Containers", root);
            }
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
                t.outlineWidth = 0.3f; t.outlineColor = new Color32(18, 18, 18, 255);
                p.word = Save("SfxWord", go);
            }
            return p;
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
            Box("Body", new Vector3(0.6f, 0.6f, 0.6f), new Vector3(0, 0.8f, 0), _accent, false).transform.SetParent(root.transform, false);
            var sc = root.AddComponent<SphereCollider>(); sc.isTrigger = true; sc.radius = 0.6f; sc.center = new Vector3(0, 0.8f, 0);
            root.AddComponent<Pickup>().Kind = kind;
            return root;
        }

        static GameObject Hero()
        {
            var root = new GameObject("Hero");
            var cap = root.AddComponent<CapsuleCollider>(); cap.isTrigger = true; cap.height = 1.8f; cap.radius = 0.35f; cap.center = new Vector3(0, 0.9f, 0);
            var rb = root.AddComponent<Rigidbody>(); rb.isKinematic = true; rb.useGravity = false;
            var visual = new GameObject("Visual"); visual.transform.SetParent(root.transform, false);
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule); body.name = "Body";
            Object.DestroyImmediate(body.GetComponent<Collider>());
            body.transform.SetParent(visual.transform, false); body.transform.localPosition = new Vector3(0, 0.9f, 0); body.transform.localScale = new Vector3(0.6f, 0.42f, 0.5f);
            body.GetComponent<Renderer>().sharedMaterial = _accent;
            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere); head.name = "Head";
            Object.DestroyImmediate(head.GetComponent<Collider>());
            head.transform.SetParent(visual.transform, false); head.transform.localPosition = new Vector3(0, 1.55f, 0); head.transform.localScale = Vector3.one * 0.5f;
            head.GetComponent<Renderer>().sharedMaterial = _paper;
            var anim = visual.AddComponent<HeroAnimator>(); anim.Body = body.transform; anim.Head = head.transform;
            anim.LegL = Limb(visual, "LegL", new Vector3(-0.14f, 0.55f, 0), 0.5f, 0.16f, _ink);
            anim.LegR = Limb(visual, "LegR", new Vector3(0.14f, 0.55f, 0), 0.5f, 0.16f, _ink);
            anim.ArmL = Limb(visual, "ArmL", new Vector3(-0.36f, 1.15f, 0), 0.45f, 0.13f, _accent);
            anim.ArmR = Limb(visual, "ArmR", new Vector3(0.36f, 1.15f, 0), 0.45f, 0.13f, _accent);
            var parts = new GameObject("Parts"); parts.transform.SetParent(visual.transform, false);
            Part(parts, "default", new Vector3(0.5f, 0.08f, 0.3f), new Vector3(0, 1.72f, 0.05f), _accent);
            Part(parts, "delivery", new Vector3(0.55f, 0.12f, 0.55f), new Vector3(0, 1.8f, 0), _accent);
            Part(parts, "ronin", new Vector3(0.56f, 0.06f, 0.56f), new Vector3(0, 1.66f, 0), _accent);
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
            var scaler = canvasGo.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1080, 1920); scaler.matchWidthOrHeight = 0.5f;
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
            var hudView = new GameObject("HUD").AddComponent<HudView>(); hudView.transform.SetParent(root, false);
            var hud = Safe(hudView.transform, "Panel"); hudView.Panel = hud.gameObject;
            hudView.PauseButton = Button(hud, "PauseButton", "II", new Vector2(1f, 1f), new Vector2(-70, -150), new Vector2(90, 70), Color.white, Ink);
            hudView.Distance = Text(hud, "Distance", "0m", 72, new Vector2(0.5f, 1f), new Vector2(0, -90));
            hudView.Coins = Text(hud, "Coins", "0", 52, new Vector2(1f, 1f), new Vector2(-90, -80));
            hudView.Best = Text(hud, "Best", "BEST 0", 34, new Vector2(0f, 1f), new Vector2(160, -80));
            var assist = Text(hud, "AssistTag", "ASSIST", 28, new Vector2(0f, 1f), new Vector2(110, -130)); hudView.AssistTag = assist.gameObject; assist.gameObject.SetActive(false);
            hudView.DashBar = Bar(hud, "DashBar", new Vector2(0, -170)); hudView.MagnetBar = Bar(hud, "MagnetBar", new Vector2(0, -205));
            var card = Group(hud, "ChapterCard", new Vector2(0.5f, 0.78f), new Vector2(900, 220));
            hudView.ChapterLabel = Text(card, "Label", "CHAPTER 1", 64, new Vector2(0.5f, 0.5f), new Vector2(0, 50));
            hudView.ChapterName = Text(card, "Name", "CITY ROOFTOPS", 48, new Vector2(0.5f, 0.5f), new Vector2(0, -40)); hudView.ChapterName.color = Red;
            hudView.ChapterCard = card.gameObject; card.gameObject.SetActive(false);
            var bubble = Group(hud, "NearRecordBubble", new Vector2(0.5f, 0.45f), new Vector2(760, 160)); Panel(bubble, Color.white, Ink);
            hudView.NearRecordText = Text(bubble, "Text", "200m to your record!", 40, new Vector2(0.5f, 0.5f), Vector2.zero);
            hudView.NearRecordBubble = bubble.gameObject; bubble.gameObject.SetActive(false);

            // Start
            var startView = new GameObject("Start").AddComponent<StartView>(); startView.transform.SetParent(root, false);
            var start = Safe(startView.transform, "Panel"); startView.Panel = start.gameObject; Panel(start, new Color(1, 1, 1, 0.92f), Color.clear);
            start.gameObject.AddComponent<Button>().onClick.AddListener(() => GameManager.Instance.StartRun());   // tap anywhere to run
            Text(start, "Kicker", "VOLUME 1", 36, new Vector2(0.5f, 1f), new Vector2(0, -330));
            Text(start, "Title", "INK\nRUN", 180, new Vector2(0.5f, 1f), new Vector2(0, -560));
            Text(start, "Hint", "Swipe to change lanes, up to jump, down to slide.", 34, new Vector2(0.5f, 1f), new Vector2(0, -790));
            startView.TapToRun = Button(start, "TapToRun", "TAP TO RUN", new Vector2(0.5f, 1f), new Vector2(0, -930), new Vector2(560, 130), Red, Color.white);
            startView.BestText = Text(start, "BestValue", "0", 70, new Vector2(0.5f, 1f), new Vector2(-180, -1080)); Text(start, "BestLabel", "BEST M", 26, new Vector2(0.5f, 1f), new Vector2(-180, -1140));
            startView.BankText = Text(start, "BankValue", "0", 70, new Vector2(0.5f, 1f), new Vector2(180, -1080)); Text(start, "BankLabel", "COINS BANKED", 26, new Vector2(0.5f, 1f), new Vector2(180, -1140));
            var missions = Group(start, "Missions", new Vector2(0.5f, 1f), new Vector2(900, 330)); missions.anchoredPosition = new Vector2(0, -1370);
            var vlg = missions.gameObject.AddComponent<VerticalLayoutGroup>(); vlg.spacing = 12; vlg.childForceExpandHeight = false; vlg.childControlHeight = false; vlg.childControlWidth = true; vlg.childForceExpandWidth = true;
            var mlv = startView.gameObject.AddComponent<MissionListView>(); mlv.Container = missions; mlv.RowPrefab = MissionRow(); startView.Missions = mlv;
            startView.WardrobeButton = Button(start, "WardrobeButton", "WARDROBE", new Vector2(0.5f, 0f), new Vector2(-200, 200), new Vector2(360, 100), Color.white, Ink);
            startView.SoundButton = Button(start, "SoundButton", "SOUND: ON", new Vector2(0.5f, 0f), new Vector2(200, 200), new Vector2(360, 100), Color.white, Ink);
            startView.SoundLabel = startView.SoundButton.GetComponentInChildren<TMP_Text>();
            var chip = Text(start, "AssistChip", "Assist on: slower start, fewer obstacles", 28, new Vector2(0.5f, 0f), new Vector2(0, 110)); startView.AssistChip = chip.gameObject; chip.gameObject.SetActive(false);
            var bundle = Group(start, "StarterBundle", new Vector2(0.5f, 0f), new Vector2(900, 120)); bundle.anchoredPosition = new Vector2(0, 340); Panel(bundle, Color.white, Ink);
            Text(bundle, "Text", "Starter bundle: 1,500 coins + Delivery outfit", 30, new Vector2(0.35f, 0.5f), Vector2.zero);
            startView.StarterBundleButton = Button(bundle, "Buy", "BUY", new Vector2(0.85f, 0.5f), Vector2.zero, new Vector2(180, 80), Red, Color.white);
            startView.StarterBundlePanel = bundle.gameObject;

            // Wardrobe (toggles itself)
            var wardrobe = Safe(root, "Wardrobe"); var wv = wardrobe.gameObject.AddComponent<WardrobeView>(); Panel(wardrobe, new Color(1, 1, 1, 0.95f), Color.clear);
            Text(wardrobe, "Kicker", "WARDROBE", 48, new Vector2(0.5f, 1f), new Vector2(0, -330));
            var cards = Group(wardrobe, "Cards", new Vector2(0.5f, 0.55f), new Vector2(1000, 520));
            var hlg = cards.gameObject.AddComponent<HorizontalLayoutGroup>(); hlg.spacing = 20; hlg.childAlignment = TextAnchor.MiddleCenter; hlg.childForceExpandWidth = false; hlg.childForceExpandHeight = false; hlg.childControlWidth = false; hlg.childControlHeight = false;
            wv.Container = cards; wv.CardPrefab = OutfitCard();
            wv.BankLine = Text(wardrobe, "BankLine", "Coins banked: 0", 34, new Vector2(0.5f, 0f), new Vector2(0, 420));
            wv.Back = Button(wardrobe, "Back", "BACK", new Vector2(0.5f, 0f), new Vector2(0, 260), new Vector2(360, 110), Red, Color.white);
            startView.WardrobePanel = wardrobe.gameObject; wardrobe.gameObject.SetActive(false);

            // Revive
            var reviveView = new GameObject("Revive").AddComponent<ReviveView>(); reviveView.transform.SetParent(root, false);
            var revive = Safe(reviveView.transform, "Panel"); reviveView.Panel = revive.gameObject; Panel(revive, new Color(1, 1, 1, 0.92f), Color.clear);
            var p1 = Group(revive, "Panel1", new Vector2(0.5f, 1f), new Vector2(940, 180)); p1.anchoredPosition = new Vector2(0, -420); Panel(p1, Ink, Ink);
            reviveView.Panel1 = Text(p1, "Text", Story.RevivePanels[0], 34, new Vector2(0.5f, 0.5f), Vector2.zero); reviveView.Panel1.color = Color.white;
            var p2 = Group(revive, "Panel2", new Vector2(0.5f, 1f), new Vector2(940, 160)); p2.anchoredPosition = new Vector2(0, -610); Panel(p2, Color.white, Ink);
            reviveView.Panel2 = Text(p2, "Text", Story.RevivePanels[1], 32, new Vector2(0.5f, 0.5f), Vector2.zero);
            reviveView.DistanceLine = Text(revive, "DistanceLine", "0m and counting.", 34, new Vector2(0.5f, 1f), new Vector2(0, -740));
            var title = Text(revive, "Title", "TO BE\nCONTINUED...?", 110, new Vector2(0.5f, 0.5f), new Vector2(0, 80)); title.color = Red;
            reviveView.Countdown = Text(revive, "Countdown", "5", 80, new Vector2(0.5f, 0.5f), new Vector2(0, -150));
            reviveView.KeepRunning = Button(revive, "KeepRunning", "▶ KEEP RUNNING", new Vector2(0.5f, 0.5f), new Vector2(0, -330), new Vector2(760, 140), Red, Color.white);
            Text(revive, "Note", "Revive = one rewarded ad", 28, new Vector2(0.5f, 0.5f), new Vector2(0, -440));
            reviveView.GiveUp = Button(revive, "GiveUp", "Give up", new Vector2(0.5f, 0.5f), new Vector2(0, -560), new Vector2(300, 90), Color.clear, Ink);

            // Results
            var resultsView = new GameObject("Results").AddComponent<ResultsView>(); resultsView.transform.SetParent(root, false);
            var results = Safe(resultsView.transform, "Panel"); resultsView.Panel = results.gameObject; Panel(results, new Color(1, 1, 1, 0.92f), Color.clear);
            var burst = Text(results, "RecordBurst", "NEW RECORD!", 56, new Vector2(0.2f, 0.85f), Vector2.zero); burst.color = Red; resultsView.RecordBurst = burst.gameObject; burst.gameObject.SetActive(false);
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
            var pauseView = new GameObject("Pause").AddComponent<PauseView>(); pauseView.transform.SetParent(root, false);
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
            var t = go.AddComponent<TextMeshProUGUI>(); t.text = text; t.fontSize = size; t.alignment = TextAlignmentOptions.Center; t.color = Ink; t.raycastTarget = false;
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
            var fill = Full(rootRt, "Fill"); var img = fill.gameObject.AddComponent<Image>(); img.color = Red; img.type = Image.Type.Filled; img.fillMethod = Image.FillMethod.Horizontal;
            Text(rootRt, "Label", name.Replace("Bar", "").ToUpper(), 20, new Vector2(0f, 0.5f), new Vector2(-120, 0));
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
            var bar = Group(row.transform, "Bar", new Vector2(0.5f, 0.15f), new Vector2(860, 12));
            var bi = bar.gameObject.AddComponent<Image>(); bi.color = Red; bi.type = Image.Type.Filled; bi.fillMethod = Image.FillMethod.Horizontal;
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
