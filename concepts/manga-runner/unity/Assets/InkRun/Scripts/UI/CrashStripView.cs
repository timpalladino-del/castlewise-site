using UnityEngine;
using UnityEngine.UI;
using TMPro;
using InkRun.Core;

namespace InkRun.UI
{
    /// <summary>
    /// The crash strip: three panels in about a second between the crash freeze-frame and the
    /// "To Be Continued...?" page. Hard cuts with a white flash, a shake on the impact panel.
    /// Uses the generated strip image from Resources/Strips/&lt;Kind&gt;.png (three equal vertical
    /// panels in one 16:9 image) when it exists, otherwise ink text panels from Story.DeathPanels.
    /// </summary>
    public class CrashStripView : MonoBehaviour
    {
        public GameObject Panel;             // shown during Crashed, hidden otherwise
        public RawImage Strip;               // the image; uvRect crops the current third
        public RectTransform StripRect;
        public Image Flash;
        public TMP_Text TextApproach, TextImpact, TextAftermath;
        public GameObject TextRoot;
        public float CutSeconds = 0.3f;

        float _t; int _panel = -1; bool _playing, _image;

        void Start() { GameManager.Instance.StateChanged += OnState; if (Panel != null) Panel.SetActive(false); }
        void OnDestroy() { if (GameManager.Instance != null) GameManager.Instance.StateChanged -= OnState; }

        void OnState(GameState s)
        {
            bool show = s == GameState.Crashed;
            if (Panel != null) Panel.SetActive(show);
            _playing = show;
            if (!show) return;
            var kind = GameManager.Instance.LastCrashKind;
            var tex = Resources.Load<Texture2D>("Strips/" + kind);
            _image = tex != null && Strip != null;
            if (Strip != null) { Strip.enabled = _image; if (_image) Strip.texture = tex; }
            if (TextRoot != null) TextRoot.SetActive(!_image);
            var lines = Story.DeathPanels(kind);
            if (TextApproach != null) TextApproach.text = lines[0];
            if (TextImpact != null) TextImpact.text = lines[1];
            if (TextAftermath != null) TextAftermath.text = lines[2];
            _t = 0f; _panel = -1; ShowPanel(0);
        }

        void Update()
        {
            if (!_playing) return;
            _t += Time.unscaledDeltaTime;                     // the crash runs in slow motion; cuts stay on real time
            int want = Mathf.FloorToInt(_t / CutSeconds); if (want > 2) want = 2;
            if (want != _panel) ShowPanel(want);
            // flash fades, impact shake
            if (Flash != null) { var c = Flash.color; c.a = Mathf.Max(0f, c.a - Time.unscaledDeltaTime * 8f); Flash.color = c; }
            if (StripRect != null)
            {
                float since = _t - _panel * CutSeconds;
                float shake = _panel == 1 && since < 0.15f ? 12f * (1f - since / 0.15f) : 0f;
                StripRect.anchoredPosition = new Vector2(Mathf.Sin(_t * 90f) * shake, Mathf.Cos(_t * 70f) * shake * 0.6f);
            }
        }

        void ShowPanel(int i)
        {
            _panel = i;
            if (Flash != null) Flash.color = new Color(1f, 1f, 1f, 0.9f);
            if (_image && Strip != null) Strip.uvRect = new Rect(i / 3f, 0f, 1f / 3f, 1f);
            if (TextApproach != null) TextApproach.gameObject.SetActive(i >= 0);
            if (TextImpact != null) TextImpact.gameObject.SetActive(i >= 1);
            if (TextAftermath != null) TextAftermath.gameObject.SetActive(i >= 2);
        }
    }
}
