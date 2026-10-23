using UnityEngine;
using UnityEngine.UI;
using TMPro;
using InkRun.Core;
using InkRun.Meta;
using InkRun.Services;

namespace InkRun.UI
{
    /// <summary>Title page: tap to run, best, bank, today's missions, wardrobe, sound toggle, starter bundle.</summary>
    public class StartView : MonoBehaviour
    {
        public Button TapToRun, WardrobeButton, SoundButton, StarterBundleButton, TapAnywhere;
        public TMP_Text BestText, BankText, SoundLabel;
        public GameObject AssistChip, StarterBundlePanel;
        public MissionListView Missions;
        public GameObject WardrobePanel;
        public GameObject Panel;      // child that is shown/hidden; this object stays active

        void Awake()
        {
            TapToRun.onClick.AddListener(() => { FX.InkAudio.Play("ui"); GameManager.Instance.StartRun(); });
            if (TapAnywhere != null) TapAnywhere.onClick.AddListener(() => { FX.InkAudio.Play("ui"); GameManager.Instance.StartRun(); });
            WardrobeButton.onClick.AddListener(() => WardrobePanel.SetActive(true));
            SoundButton.onClick.AddListener(() => { var s = SaveData.Current; s.SoundOn = !s.SoundOn; s.Save(); Refresh(); });
            StarterBundleButton.onClick.AddListener(() => Services.Services.PlayerGroups.BuyStarterBundle(Refresh));
        }

        void Start() { GameManager.Instance.StateChanged += OnState; OnState(GameManager.Instance.State); }
        void OnDestroy() { if (GameManager.Instance != null) GameManager.Instance.StateChanged -= OnState; }
        void OnState(GameState s) { Panel.SetActive(s == GameState.Idle); if (s == GameState.Idle) Refresh(); }

        void Refresh()
        {
            var save = SaveData.Current;
            BestText.text = save.BestDistance.ToString("N0");
            BankText.text = save.Bank.ToString("N0");
            SoundLabel.text = "Sound: " + (save.SoundOn ? "on" : "off");
            AssistChip.SetActive(save.ShouldAssist(GameManager.Instance.Tuning));
            StarterBundlePanel.SetActive(Services.Services.PlayerGroups.ShowStarterBundle);
            Missions.Refresh();
        }
    }
}
