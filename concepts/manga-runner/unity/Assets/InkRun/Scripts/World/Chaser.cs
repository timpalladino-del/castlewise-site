using UnityEngine;
using InkRun.Core;

namespace InkRun.World
{
    /// <summary>
    /// The Smudge Crew runner on the hero's heels. Starts a stride behind at the run start, drops back as the
    /// run settles, closes in again after a revive, and catches up to stand over the hero on a crash. Drives its
    /// own run cycle on the model's joint nodes (same layout as the hero) so no Animator is needed.
    /// </summary>
    public class Chaser : MonoBehaviour
    {
        public Transform Body, Head, LegL, LegR, ArmL, ArmR;
        public float CloseDistance = 2.4f;     // metres behind the hero when "on his heels"
        public float FarDistance = 6.5f;       // where it drifts to during a clean run (just outside the frame)
        public float BackOffSeconds = 12f;     // time to drift from close to far
        public float CatchUpDistance = 1.3f;   // where it stops on a crash
        public float StrideDegrees = 46f;
        public float ArmDegrees = 40f;

        float _dist, _since, _phase, _x; Vector3 _bodyRest, _headRest; GameState _last = GameState.Idle; Renderer[] _r;

        void Awake()
        {
            if (Body != null) _bodyRest = Body.localPosition;
            if (Head != null) _headRest = Head.localPosition;
            _r = GetComponentsInChildren<Renderer>(true);
            _dist = CloseDistance;
        }

        void LateUpdate()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.Player == null) return;
            var state = gm.State;
            if (state != _last)
            {
                if (state == GameState.Running && (_last == GameState.Idle || _last == GameState.Revive || _last == GameState.Results)) { _dist = CloseDistance; _since = 0f; }
                _last = state;
            }
            bool running = state == GameState.Running;
            bool crashed = state == GameState.Crashed || state == GameState.Revive || state == GameState.Results;
            bool show = running || crashed;
            foreach (var r in _r) if (r != null) r.enabled = show;
            if (!show) return;

            float dt = Time.deltaTime;
            if (running)
            {
                _since += dt;
                float target = Mathf.Lerp(CloseDistance, FarDistance, Mathf.Clamp01(_since / BackOffSeconds));
                if (gm.IsDashing) target = FarDistance + 2f;             // a dash leaves the Crew behind
                _dist = Mathf.MoveTowards(_dist, target, dt * 2.5f);
            }
            else
            {
                _dist = Mathf.MoveTowards(_dist, CatchUpDistance, Time.unscaledDeltaTime * 4f);
            }
            // follow the hero's lane with a lag so it visibly reacts to lane changes
            var hero = gm.Player.transform.position;
            _x = Mathf.Lerp(_x, hero.x, Mathf.Min(1f, dt * 4f));
            transform.position = new Vector3(_x, 0f, hero.z - _dist);

            // run cycle, or a standing "gotcha" pose over the fallen hero
            if (crashed)
            {
                Swing(LegL, 0f); Swing(LegR, 0f); Swing(ArmL, -60f); Swing(ArmR, -60f);
                transform.localRotation = Quaternion.Euler(12f, 0f, 0f);
                return;
            }
            float speed = gm.Speed;
            _phase += dt * speed * 1.2f;
            float s = Mathf.Sin(_phase), c = Mathf.Cos(_phase);
            Swing(LegL, -s * StrideDegrees); Swing(LegR, s * StrideDegrees); Swing(ArmL, s * ArmDegrees); Swing(ArmR, -s * ArmDegrees);
            float bob = Mathf.Abs(c) * 0.07f;
            if (Body != null) Body.localPosition = _bodyRest + Vector3.up * bob;
            if (Head != null) Head.localPosition = _headRest + Vector3.up * bob;
            transform.localRotation = Quaternion.Euler(10f, 0f, 0f);   // leaning into the chase
        }

        static void Swing(Transform joint, float degrees) { if (joint != null) joint.localRotation = Quaternion.Euler(degrees, 0f, 0f); }
    }
}
