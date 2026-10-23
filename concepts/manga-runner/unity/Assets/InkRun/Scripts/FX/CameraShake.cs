using UnityEngine;
using InkRun.Core;

namespace InkRun.FX
{
    /// <summary>Short shake on crash and on dash smashes. Put on the camera rig, not the camera.</summary>
    public class CameraShake : MonoBehaviour
    {
        public float CrashAmount = 0.35f;
        float _amount;
        Vector3 _rest;

        void Awake() { _rest = transform.localPosition; }
        void Start() { GameManager.Instance.StateChanged += OnState; GameManager.Instance.SfxWord += OnWord; }
        void OnDestroy() { if (GameManager.Instance == null) return; GameManager.Instance.StateChanged -= OnState; GameManager.Instance.SfxWord -= OnWord; }

        void OnState(GameState s) { if (s == GameState.Crashed) _amount = CrashAmount; }
        void OnWord(string text, Vector3 at, float size) { if (text == "WHAM!" && size < 1.5f) _amount = Mathf.Max(_amount, CrashAmount * 0.4f); }

        void Update()
        {
            if (_amount <= 0f) { transform.localPosition = _rest; return; }
            transform.localPosition = _rest + Random.insideUnitSphere * _amount;
            _amount = Mathf.Max(0f, _amount - Time.deltaTime * 0.8f);
        }
    }
}
