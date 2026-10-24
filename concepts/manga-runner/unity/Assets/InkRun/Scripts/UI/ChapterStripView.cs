using UnityEngine;
using TMPro;
using InkRun.Core;

namespace InkRun.UI
{
    /// <summary>
    /// Story panels over the live run: the cold open on the first run of a session and three captions at
    /// every chapter change, one every 0.7 s at the top of the screen. The run never pauses; a crash cuts
    /// the sequence instantly (the crash strip takes over).
    /// </summary>
    public class ChapterStripView : MonoBehaviour
    {
        public GameObject Panel;
        public TMP_Text[] Captions = new TMP_Text[3];
        public float PanelSeconds = 0.7f;

        string[] _lines; float _t; int _shown; bool _playing; static bool _coldOpenDone;

        void Start()
        {
            var gm = GameManager.Instance;
            gm.StateChanged += OnState;
            if (gm.Areas != null) gm.Areas.ChapterStarted += OnChapter;
            if (Panel != null) Panel.SetActive(false);
        }
        void OnDestroy()
        {
            var gm = GameManager.Instance; if (gm == null) return;
            gm.StateChanged -= OnState; if (gm.Areas != null) gm.Areas.ChapterStarted -= OnChapter;
        }

        void OnState(GameState s)
        {
            if (s == GameState.Running && !_coldOpenDone) { _coldOpenDone = true; Play(Story.ColdOpen); }
            else if (s != GameState.Running) Stop();
        }

        void OnChapter(World.AreaDefinition a)
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.State != GameState.Running || gm.Areas == null) return;
            if (gm.Areas.CurrentIndex == 0 && gm.Run.Distance < 10f) return;      // run start, not a transition
            Play(Story.TransitionPanels(gm.Areas.CurrentIndex));
        }

        void Play(string[] lines)
        {
            _lines = lines; _t = 0f; _shown = 0; _playing = true;
            if (Panel != null) Panel.SetActive(true);
            for (int i = 0; i < Captions.Length; i++) if (Captions[i] != null) { Captions[i].text = i < lines.Length ? lines[i] : ""; Captions[i].gameObject.SetActive(i == 0); }
        }

        void Stop() { _playing = false; if (Panel != null) Panel.SetActive(false); }

        void Update()
        {
            if (!_playing) return;
            _t += Time.deltaTime;
            int want = Mathf.FloorToInt(_t / PanelSeconds);
            while (_shown < want && _shown < Captions.Length - 1) { _shown++; if (Captions[_shown] != null) Captions[_shown].gameObject.SetActive(true); }
            if (_t > PanelSeconds * (Captions.Length + 1.2f)) Stop();
        }
    }
}
