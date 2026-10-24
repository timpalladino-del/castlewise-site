using UnityEngine;
using InkRun.Core;

namespace InkRun.World
{
    /// <summary>
    /// Switches chapters every Tuning.AreaLength meters, cross-fades the global accent
    /// color the ink shader reads (_InkAccent), and dresses track pieces with the
    /// current area's props. Also fires the CHAPTER card.
    /// </summary>
    public class AreaController : MonoBehaviour
    {
        public AreaDefinition[] Areas;
        public Transform SunDisc;               // a flat disc in the sky tinted by the accent
        public Transform MoonMask;              // white disc offset over the sun for the crescent

        public int CurrentIndex { get; private set; }
        public AreaDefinition Current => Areas[CurrentIndex];
        public Color AccentNow { get; private set; }

        public event System.Action<AreaDefinition> ChapterStarted;

        static readonly int AccentId = Shader.PropertyToID("_InkAccent");
        Color _from; float _fadeT = 1f;

        void Awake()
        {
            // the ink shader reads this global; set it before the first frame so the title screen is not black
            if (Areas != null && Areas.Length > 0) { AccentNow = _from = Areas[0].Accent; Shader.SetGlobalColor(AccentId, AccentNow); }
        }

        public void ResetToFirst()
        {
            CurrentIndex = 0; _fadeT = 1f;
            AccentNow = _from = Areas[0].Accent;
            Shader.SetGlobalColor(AccentId, AccentNow);
            ApplySky();
            ChapterStarted?.Invoke(Areas[0]);
        }

        public void Tick(float distance, float dt)
        {
            var t = GameManager.Instance.Tuning;
            int idx = Mathf.FloorToInt(distance / t.AreaLength) % Areas.Length;
            if (idx != CurrentIndex)
            {
                CurrentIndex = idx; _from = AccentNow; _fadeT = 0f;
                ApplySky();
                ChapterStarted?.Invoke(Current);
            }
            if (_fadeT < 1f)
            {
                _fadeT = Mathf.Min(1f, _fadeT + dt / t.AccentFadeSeconds);
                AccentNow = Color.Lerp(_from, Current.Accent, _fadeT);
                Shader.SetGlobalColor(AccentId, AccentNow);
            }
        }

        void ApplySky()
        {
            if (MoonMask != null) MoonMask.gameObject.SetActive(Current.CrescentMoon);
        }

        public void DressTrackPiece(GameObject piece)
        {
            if (Current.SideProps == null || Current.SideProps.Length == 0) return;
            var t = GameManager.Instance.Tuning;
            foreach (int side in new[] { -1, 1 })
            {
                var prefab = Current.SideProps[Random.Range(0, Current.SideProps.Length)];
                var prop = Instantiate(prefab, piece.transform);
                prop.transform.localPosition = new Vector3(side * (t.LaneWidth * 2.05f), 0f, Random.Range(0f, 4f));   // modeled facades overhang toward the track; keep them off the roof edge
                if (side < 0) prop.transform.localScale = new Vector3(-1f, 1f, 1f);   // props extend toward +x; mirror the left one outward
            }
        }
    }
}
