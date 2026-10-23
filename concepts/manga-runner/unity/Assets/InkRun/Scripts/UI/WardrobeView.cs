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
                if (o.PreviewPrefab != null) Instantiate(o.PreviewPrefab, card.transform.Find("Preview"));
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
