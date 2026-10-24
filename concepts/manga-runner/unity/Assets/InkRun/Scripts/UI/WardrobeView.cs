using UnityEngine;
using UnityEngine.UI;
using TMPro;
using InkRun.Meta;
using InkRun.Services;

namespace InkRun.UI
{
    /// <summary>Outfit cards: preview prefab, name, description, Buy / Wear / Wearing.</summary>
    public class WardrobeView : MonoBehaviour
    {
        public Transform Container;
        public GameObject CardPrefab;      // children: Preview (Transform), Name, Desc, Button(+Label)
        public TMP_Text BankLine;
        public Button Back;

        void Awake() { Back.onClick.AddListener(() => gameObject.SetActive(false)); }
        void OnEnable() { Refresh(); }

        static readonly Color Ink = new Color(0.07f, 0.07f, 0.07f);
        static readonly Color Tone = new Color(0.75f, 0.75f, 0.75f);

        /// <summary>A small front-view Nozomi drawn from UI rectangles, until there are real preview renders.</summary>
        static void Chibi(Transform parent, string outfit)
        {
            var gm = Core.GameManager.Instance;
            Color accent = gm != null && gm.Areas != null ? gm.Areas.AccentNow : new Color(0.878f, 0.125f, 0.165f);
            Rect_(parent, "Body", new Vector2(0, -40), new Vector2(100, 90), accent);
            Rect_(parent, "ArmL", new Vector2(-62, -30), new Vector2(22, 60), accent); Rect_(parent, "ArmR", new Vector2(62, -30), new Vector2(22, 60), accent);
            Rect_(parent, "LegL", new Vector2(-20, -105), new Vector2(24, 44), Ink); Rect_(parent, "LegR", new Vector2(20, -105), new Vector2(24, 44), Ink);
            Rect_(parent, "FootL", new Vector2(-20, -132), new Vector2(32, 14), Color.white); Rect_(parent, "FootR", new Vector2(20, -132), new Vector2(32, 14), Color.white);
            Rect_(parent, "Head", new Vector2(0, 50), new Vector2(82, 82), Color.white);
            Rect_(parent, "EyeL", new Vector2(-16, 46), new Vector2(8, 14), Ink); Rect_(parent, "EyeR", new Vector2(16, 46), new Vector2(8, 14), Ink);
            Rect_(parent, "HairL", new Vector2(-24, 98), new Vector2(14, 32), Ink, 28f); Rect_(parent, "HairM", new Vector2(0, 104), new Vector2(14, 34), Ink); Rect_(parent, "HairR", new Vector2(24, 98), new Vector2(14, 32), Ink, -28f);
            switch (outfit)
            {
                case "delivery":
                    Rect_(parent, "Cap", new Vector2(0, 92), new Vector2(92, 20), Ink); Rect_(parent, "Brim", new Vector2(40, 84), new Vector2(34, 8), Ink);
                    Rect_(parent, "Parcel", new Vector2(-82, -40), new Vector2(60, 54), Tone); Rect_(parent, "Tape", new Vector2(-82, -40), new Vector2(62, 8), accent);
                    break;
                case "ronin":
                    Rect_(parent, "Headband", new Vector2(0, 62), new Vector2(90, 10), accent); Rect_(parent, "Tail", new Vector2(54, 44), new Vector2(8, 34), accent, -25f);
                    Rect_(parent, "Bokken", new Vector2(70, -30), new Vector2(9, 150), Ink, 22f);
                    break;
                default:
                    Rect_(parent, "Strap", new Vector2(0, 58), new Vector2(86, 8), Ink); Rect_(parent, "Goggles", new Vector2(0, 66), new Vector2(60, 14), accent);
                    break;
            }
        }

        static void Rect_(Transform parent, string name, Vector2 pos, Vector2 size, Color color, float rotation = 0f)
        {
            var go = new GameObject(name, typeof(RectTransform)); go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>(); rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f); rt.anchoredPosition = pos; rt.sizeDelta = size; rt.localRotation = Quaternion.Euler(0f, 0f, rotation);
            var img = go.AddComponent<Image>(); img.color = color; img.raycastTarget = false;
            var o = go.AddComponent<Outline>(); o.effectColor = Ink; o.effectDistance = new Vector2(2.5f, -2.5f);
        }

        void Refresh()
        {
            var save = SaveData.Current;
            BankLine.text = "Coins banked: " + save.Bank.ToString("N0");
            for (int i = Container.childCount - 1; i >= 0; i--) Destroy(Container.GetChild(i).gameObject);
            foreach (var o in Services.Services.Outfits.All)
            {
                var card = Instantiate(CardPrefab, Container);
                card.transform.Find("Name").GetComponent<TMP_Text>().text = o.Name;
                card.transform.Find("Desc").GetComponent<TMP_Text>().text = o.Description;
                var preview = card.transform.Find("Preview");
                if (o.PreviewPrefab != null) Instantiate(o.PreviewPrefab, preview); else Chibi(preview, o.Id);
                var button = card.transform.Find("Button").GetComponent<Button>();
                var label = button.GetComponentInChildren<TMP_Text>();
                bool owned = save.Owns(o.Id), wearing = save.Outfit == o.Id;
                label.text = wearing ? "Wearing" : owned ? "Wear" : "Buy " + o.Price;
                button.interactable = !wearing && (owned || save.Bank >= o.Price);
                button.onClick.AddListener(() => { Services.Services.Outfits.BuyOrWear(o.Id); Refresh(); });
            }
        }
    }
}
