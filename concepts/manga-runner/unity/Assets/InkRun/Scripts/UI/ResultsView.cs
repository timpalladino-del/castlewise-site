using UnityEngine;
using UnityEngine.UI;
using TMPro;
using InkRun.Core;
using InkRun.Meta;

namespace InkRun.UI
{
    /// <summary>End of chapter: distance, coins, best, record burst, Save the haul, missions cleared, Run again.</summary>
    public class ResultsView : MonoBehaviour
    {
        public TMP_Text Kicker, Distance, Coins, Best, MissionLine, BankLine, DoubleLabel;
        public GameObject RecordBurst;
        public Button DoubleCoins, RunAgain;
        public GameObject Panel;      // child that is shown/hidden; this object stays active

        void Awake()
        {
            RunAgain.onClick.AddListener(() => { GameManager.Instance.BackToStart(); GameManager.Instance.StartRun(); });
            DoubleCoins.onClick.AddListener(() =>
            {
                DoubleCoins.interactable = false;
                GameManager.Instance.RequestDoubleCoins(ok => { if (ok) { DoubleLabel.text = "Haul doubled!"; Coins.text = (GameManager.Instance.Run.Coins * 2).ToString("N0"); BankLine.text = "Coins banked: " + SaveData.Current.Bank.ToString("N0"); } else DoubleCoins.interactable = true; });
            });
        }
        void Start() { GameManager.Instance.StateChanged += OnState; Panel.SetActive(false); }
        void OnDestroy() { if (GameManager.Instance != null) GameManager.Instance.StateChanged -= OnState; }

        void OnState(GameState s)
        {
            bool show = s == GameState.Results;
            Panel.SetActive(show);
            if (!show) return;
            var gm = GameManager.Instance; var r = gm.LastResult; var save = SaveData.Current;
            RecordBurst.SetActive(r.Record);
            Kicker.text = r.Record ? "A new page in the record" : "End of chapter " + (gm.Areas.CurrentIndex + 1);
            Distance.text = Mathf.FloorToInt(gm.Run.Distance).ToString("N0");
            Coins.text = gm.Run.Coins.ToString("N0");
            Best.text = save.BestDistance.ToString("N0");
            MissionLine.gameObject.SetActive(r.MissionsCleared.Length > 0);
            MissionLine.text = "Mission clear: " + string.Join(", ", r.MissionsCleared) + " (+" + r.MissionCoins + " coins)";
            BankLine.text = "Coins banked: " + save.Bank.ToString("N0");
            DoubleLabel.text = "Save the haul: 2× coins (ad)";
            DoubleCoins.interactable = gm.Run.Coins > 0 && Services.Services.Ads.IsRewardedReady;
        }
    }
}
