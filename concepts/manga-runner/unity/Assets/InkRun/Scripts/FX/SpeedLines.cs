using UnityEngine;
using UnityEngine.UI;
using InkRun.Core;

namespace InkRun.FX
{
    /// <summary>
    /// Drives a full-screen RawImage that uses Shaders/InkSpeedLines.shader. Intensity rises
    /// with speed, jumps during a dash. The shader draws radial ink lines from the vanishing point.
    /// </summary>
    public class SpeedLines : MonoBehaviour
    {
        public RawImage Image;
        public float BaseIntensity = 0.1f;
        public float DashIntensity = 0.75f;

        static readonly int IntensityId = Shader.PropertyToID("_Intensity");
        Material _mat;

        void Awake()
        {
            if (Image == null || Image.material == null) { enabled = false; return; }
            _mat = Instantiate(Image.material); Image.material = _mat;
        }

        void Update()
        {
            var gm = GameManager.Instance;
            if (_mat == null) return;
            if (gm == null || gm.State != GameState.Running) { _mat.SetFloat(IntensityId, 0f); return; }
            float t = gm.Tuning.SpeedAt(gm.Run.Distance, false, false);
            float ramp = Mathf.InverseLerp(gm.Tuning.BaseSpeed, gm.Tuning.BaseSpeed + gm.Tuning.MaxSpeedBonus, t);
            float v = BaseIntensity + ramp * 0.3f + (gm.IsDashing ? DashIntensity : 0f) + (gm.Player.IsAirborne ? 0.12f : 0f);
            _mat.SetFloat(IntensityId, Mathf.Clamp01(v));
        }
    }
}
