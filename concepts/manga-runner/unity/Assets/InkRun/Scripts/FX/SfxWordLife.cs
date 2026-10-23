using UnityEngine;

namespace InkRun.FX
{
    /// <summary>Pop-in scale then destroy. Added at runtime by SfxWords.</summary>
    public class SfxWordLife : MonoBehaviour
    {
        float _t, _life; Vector3 _scale;
        public void Init(float life) { _life = life; _scale = transform.localScale; }
        void Update()
        {
            _t += Time.deltaTime;
            float k = Mathf.Clamp01(_t / 0.12f);
            transform.localScale = _scale * (1f + 0.6f * (1f - k) * (1f - k));
            if (_t > _life) Destroy(gameObject);
        }
    }
}
