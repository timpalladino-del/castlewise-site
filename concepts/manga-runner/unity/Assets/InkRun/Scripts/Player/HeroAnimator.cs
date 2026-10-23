using UnityEngine;
using InkRun.Core;

namespace InkRun.Player
{
    /// <summary>
    /// Procedural run cycle for the gray-box hero: legs and arms swing from their joints,
    /// the body bobs, the whole figure leans into lane changes and tucks in the air.
    /// Replace with an Animator when a rigged model exists; PlayerController already sets
    /// Running/Jump/Slide/Crash on one if assigned.
    /// </summary>
    public class HeroAnimator : MonoBehaviour
    {
        public Transform Body, Head, LegL, LegR, ArmL, ArmR;
        public float StrideDegrees = 42f;
        public float ArmDegrees = 38f;
        public float Bob = 0.06f;
        public float LeanDegrees = 14f;

        PlayerController _player;
        float _phase, _lastX, _lean, _fall;
        Vector3 _bodyRest, _headRest;
        Renderer[] _renderers;

        void Awake()
        {
            _player = GetComponentInParent<PlayerController>();
            if (Body != null) _bodyRest = Body.localPosition;
            if (Head != null) _headRest = Head.localPosition;
            _renderers = GetComponentsInChildren<Renderer>(true);
        }

        void Update()
        {
            var gm = GameManager.Instance;
            // crash: tip over and stay down until the next run or the revive
            bool down = gm != null && (gm.State == GameState.Crashed || gm.State == GameState.Revive || gm.State == GameState.Results);
            if (down)
            {
                _fall = Mathf.MoveTowards(_fall, 1f, Time.unscaledDeltaTime * 3f);
                transform.localRotation = Quaternion.Euler(-85f * _fall, 0f, 25f * _fall);
                return;
            }
            _fall = 0f;
            // revive grace: blink
            bool blink = gm != null && gm.InvulnerableTimeLeft > 0f && !gm.IsDashing;
            bool visible = !blink || Mathf.FloorToInt(Time.time * 12f) % 2 == 0;
            foreach (var r in _renderers) if (r != null) r.enabled = visible;

            bool running = gm != null && gm.State == GameState.Running;
            float speed = running ? gm.Speed : 6f;
            _phase += Time.deltaTime * speed * 1.25f;
            float s = Mathf.Sin(_phase), c = Mathf.Cos(_phase);

            bool air = _player != null && _player.IsAirborne;
            bool slide = _player != null && _player.IsSliding;

            float legA = air ? 35f : s * StrideDegrees;
            float legB = air ? -15f : -s * StrideDegrees;
            float armA = air ? -150f : -s * ArmDegrees;
            float armB = air ? -150f : s * ArmDegrees;
            if (slide) { legA = 70f; legB = 70f; armA = -60f; armB = -60f; }

            Swing(LegL, legA); Swing(LegR, legB); Swing(ArmL, armA); Swing(ArmR, armB);

            float bob = air || slide ? 0f : Mathf.Abs(c) * Bob;
            if (Body != null) Body.localPosition = _bodyRest + Vector3.up * bob;
            if (Head != null) Head.localPosition = _headRest + Vector3.up * bob;

            // lean into lane changes
            float x = transform.position.x;
            float vx = (x - _lastX) / Mathf.Max(Time.deltaTime, 0.0001f); _lastX = x;
            _lean = Mathf.Lerp(_lean, Mathf.Clamp(-vx * 2.2f, -LeanDegrees, LeanDegrees), Time.deltaTime * 10f);
            transform.localRotation = Quaternion.Euler(air ? -8f : 6f, 0f, _lean);
        }

        static void Swing(Transform joint, float degrees)
        {
            if (joint != null) joint.localRotation = Quaternion.Euler(degrees, 0f, 0f);
        }
    }
}
