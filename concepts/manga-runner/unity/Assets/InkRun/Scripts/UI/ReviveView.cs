using UnityEngine;
using UnityEngine.UI;
using TMPro;
using InkRun.Core;

namespace InkRun.UI
{
    /// <summary>The "To Be Continued...?" page. Countdown, Keep Running (rewarded ad), Give up.</summary>
    public class ReviveView : MonoBehaviour
    {
        public TMP_Text Countdown, DistanceLine, Panel1, Panel2;
        public Button KeepRunning, GiveUp;
        public Image KaijuShadow;     // a soft dark blob behind the panels; Volume 1 never shows more than its shadow
        public GameObject Panel;      // child that is shown/hidden; this object stays active

        float _left, _shown; bool _ticking;

        void Awake()
        {
            KeepRunning.onClick.AddListener(() => { _ticking = false; GameManager.Instance.RequestRevive(); });
            GiveUp.onClick.AddListener(() => { _ticking = false; GameManager.Instance.GiveUp(); });
        }
        void Start()
        {
            GameManager.Instance.StateChanged += OnState; Panel.SetActive(false);
            if (KaijuShadow != null && KaijuShadow.sprite == null) KaijuShadow.sprite = SoftBlob(96);
        }
        void OnDestroy() { if (GameManager.Instance != null) GameManager.Instance.StateChanged -= OnState; }

        void OnState(GameState s)
        {
            bool show = s == GameState.Revive;
            Panel.SetActive(show);
            if (!show) return;
            var gm = GameManager.Instance;
            _left = gm.Tuning.ReviveCountdownSeconds; _ticking = true; _shown = 0f;
            DistanceLine.text = Mathf.FloorToInt(gm.Run.Distance).ToString("N0") + "m and counting.";
            if (Panel1 != null) Panel1.text = Story.RevivePanels[0];
            if (Panel2 != null) Panel2.text = Story.RevivePanels[1];
        }

        void Update()
        {
            if (Panel.activeSelf && KaijuShadow != null)
            {
                _shown += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(_shown / 6f);                                                   // the shadow gets closer...
                KaijuShadow.rectTransform.localScale = Vector3.one * Mathf.Lerp(0.7f, 1.6f, k * k);
                KaijuShadow.rectTransform.anchoredPosition = new Vector2(-60f * k, -120f * k);
            }
            if (!_ticking) return;
            _left -= Time.unscaledDeltaTime;
            Countdown.text = Mathf.CeilToInt(Mathf.Max(0f, _left)).ToString();
            if (_left <= 0f) { _ticking = false; GameManager.Instance.GiveUp(); }
        }

        /// <summary>A radial-falloff circle, so a plain Image reads as a shadow without any art asset.</summary>
        static Sprite SoftBlob(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var px = new Color[size * size];
            float r = size * 0.5f;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float d = Mathf.Sqrt((x - r + 0.5f) * (x - r + 0.5f) + (y - r + 0.5f) * (y - r + 0.5f)) / r;
                    float e = Mathf.Clamp01((d - 0.55f) / 0.45f);                     // 0 inside, 1 at the rim
                    float a = 1f - e * e * (3f - 2f * e);                               // smooth falloff (Mathf.SmoothStep interpolates, so not that)
                    px[y * size + x] = new Color(1f, 1f, 1f, a);
                }
            tex.SetPixels(px); tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }
    }
}
