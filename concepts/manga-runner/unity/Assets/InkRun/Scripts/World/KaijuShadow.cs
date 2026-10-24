using UnityEngine;
using InkRun.Core;
using InkRun.FX;

namespace InkRun.World
{
    /// <summary>
    /// The Shadow: Volume 1 never shows the kaiju, only its shadow sweeping across the rooftops. A flat grey
    /// silhouette slides over the track ahead of the hero every so often, bigger and slower when the run is long.
    /// </summary>
    public class KaijuShadow : MonoBehaviour
    {
        public float MinInterval = 18f, MaxInterval = 40f;
        public float Ahead = 22f;              // metres ahead of the hero where the sweep happens
        public float SweepSpeed = 7f;          // metres per second sideways
        public float Scale = 9f;

        float _timer, _x; bool _sweeping; int _dir = 1; Renderer[] _r;

        void Awake() { _r = GetComponentsInChildren<Renderer>(true); _timer = Random.Range(8f, 16f); Show(false); }

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.Player == null) return;
            if (gm.State != GameState.Running) { if (_sweeping) { _sweeping = false; Show(false); } return; }
            float dt = Time.deltaTime;
            if (!_sweeping)
            {
                _timer -= dt;
                if (_timer > 0f) return;
                _sweeping = true; _dir = Random.value < 0.5f ? 1 : -1; _x = -_dir * 16f;
                transform.localScale = Vector3.one * Scale * Random.Range(0.85f, 1.25f);
                Show(true);
                InkAudio.Play("rumble");
            }
            _x += _dir * SweepSpeed * dt;
            transform.position = new Vector3(_x, 0.015f, gm.Player.transform.position.z + Ahead);
            if (Mathf.Abs(_x) > 16f) { _sweeping = false; Show(false); _timer = Random.Range(MinInterval, MaxInterval); }
        }

        void Show(bool on) { foreach (var r in _r) if (r != null) r.enabled = on; }
    }
}
