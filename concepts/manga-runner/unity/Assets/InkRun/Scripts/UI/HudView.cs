using UnityEngine;
using TMPro;
using UnityEngine.UI;
using InkRun.Core;
using InkRun.Meta;

namespace InkRun.UI
{
    /// <summary>Distance, coins, best, power-up bars, the near-record bubble and the chapter card.</summary>
    public class HudView : MonoBehaviour
    {
        public TMP_Text Distance, Coins, Best, ChapterLabel, ChapterName, NearRecordText;
        public GameObject ChapterCard, NearRecordBubble, AssistTag;
        public Image DashBar, MagnetBar;
        public GameObject Panel;      // child that is shown/hidden; this object stays active

        void Start()
        {
            var gm = GameManager.Instance;
            gm.StateChanged += OnState; gm.CoinsChanged += OnCoins; gm.NearRecord += OnNearRecord;
            gm.Areas.ChapterStarted += OnChapter;
            OnState(gm.State);
        }
        void OnDestroy()
        {
            var gm = GameManager.Instance; if (gm == null) return;
            gm.StateChanged -= OnState; gm.CoinsChanged -= OnCoins; gm.NearRecord -= OnNearRecord;
            gm.Areas.ChapterStarted -= OnChapter;
        }

        void OnState(GameState s)
        {
            Panel.SetActive(s == GameState.Running || s == GameState.Crashed);
            if (s == GameState.Running) { Best.text = "BEST " + SaveData.Current.BestDistance.ToString("N0"); AssistTag.SetActive(GameManager.Instance.Run.Assist); }
        }
        void OnCoins(int c) => Coins.text = c.ToString("N0");

        void OnNearRecord()
        {
            int left = Mathf.CeilToInt((SaveData.Current.BestDistance - GameManager.Instance.Run.Distance) / 50f) * 50;
            NearRecordText.text = left + "m to your record!";
            NearRecordBubble.SetActive(true);
            CancelInvoke(nameof(HideBubble)); Invoke(nameof(HideBubble), 2.6f);
        }
        void HideBubble() => NearRecordBubble.SetActive(false);

        void OnChapter(World.AreaDefinition a)
        {
            ChapterLabel.text = a.ChapterLabel; ChapterName.text = a.DisplayName; ChapterName.color = a.Accent;
            ChapterCard.SetActive(true);
            CancelInvoke(nameof(HideChapter)); Invoke(nameof(HideChapter), 1.6f);
        }
        void HideChapter() => ChapterCard.SetActive(false);

        void Update()
        {
            if (!Panel.activeSelf) return;
            var gm = GameManager.Instance;
            Distance.text = Mathf.FloorToInt(gm.Run.Distance).ToString("N0") + "m";
            DashBar.transform.parent.gameObject.SetActive(gm.DashTimeLeft > 0f);
            MagnetBar.transform.parent.gameObject.SetActive(gm.MagnetTimeLeft > 0f);
            DashBar.fillAmount = gm.DashTimeLeft / gm.Tuning.DashDuration;
            MagnetBar.fillAmount = gm.MagnetTimeLeft / gm.Tuning.MagnetDuration;
        }
    }
}
