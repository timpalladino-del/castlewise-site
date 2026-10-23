using UnityEngine;
using UnityEngine.UI;
using TMPro;
using InkRun.Core;

namespace InkRun.UI
{
    /// <summary>The "To Be Continued...?" page. Countdown, Keep Running (rewarded ad), Give up.</summary>
    public class ReviveView : MonoBehaviour
    {
        public TMP_Text Countdown, DistanceLine;
        public Button KeepRunning, GiveUp;
        public GameObject Panel;      // child that is shown/hidden; this object stays active

        float _left; bool _ticking;

        void Awake()
        {
            KeepRunning.onClick.AddListener(() => { _ticking = false; GameManager.Instance.RequestRevive(); });
            GiveUp.onClick.AddListener(() => { _ticking = false; GameManager.Instance.GiveUp(); });
        }
        void Start() { GameManager.Instance.StateChanged += OnState; Panel.SetActive(false); }
        void OnDestroy() { if (GameManager.Instance != null) GameManager.Instance.StateChanged -= OnState; }

        void OnState(GameState s)
        {
            bool show = s == GameState.Revive;
            Panel.SetActive(show);
            if (!show) return;
            var gm = GameManager.Instance;
            _left = gm.Tuning.ReviveCountdownSeconds; _ticking = true;
            DistanceLine.text = Mathf.FloorToInt(gm.Run.Distance).ToString("N0") + "m and counting.";
        }

        void Update()
        {
            if (!_ticking) return;
            _left -= Time.unscaledDeltaTime;
            Countdown.text = Mathf.CeilToInt(Mathf.Max(0f, _left)).ToString();
            if (_left <= 0f) { _ticking = false; GameManager.Instance.GiveUp(); }
        }
    }
}
