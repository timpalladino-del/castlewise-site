using UnityEngine;
using UnityEngine.UI;
using InkRun.Core;

namespace InkRun.UI
{
    /// <summary>Shown when the app loses focus mid-run or the pause button is tapped. Tap anywhere to resume.</summary>
    public class PauseView : MonoBehaviour
    {
        public GameObject Panel;
        public Button Resume;

        void Awake() { Resume.onClick.AddListener(() => GameManager.Instance.Resume()); }
        void Start() { GameManager.Instance.StateChanged += OnState; Panel.SetActive(GameManager.Instance.State == GameState.Paused); }
        void OnDestroy() { if (GameManager.Instance != null) GameManager.Instance.StateChanged -= OnState; }
        void OnState(GameState s) { Panel.SetActive(s == GameState.Paused); }
    }
}
