using UnityEngine;
using InkRun.Core;

namespace InkRun.FX
{
    /// <summary>
    /// Keeps the camera a little behind the hero's lane with lag, rolls slightly on lane
    /// changes, and widens the field of view during a dash. Put on the Main Camera.
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        public float FollowX = 0.45f;
        public float Lag = 6f;
        public float RollDegrees = 3f;
        public float BaseFov = 60f;
        public float DashFov = 74f;
        public float BobAmount = 0.035f;     // vertical bob with the run cycle

        Camera _cam; Vector3 _rest; float _x, _roll, _phase, _bob;

        void Awake() { _cam = GetComponent<Camera>(); _rest = transform.localPosition; }

        void LateUpdate()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.Player == null) return;
            float targetX = gm.Player.transform.position.x * FollowX;
            float prev = _x;
            _x = Mathf.Lerp(_x, targetX, Time.deltaTime * Lag);
            bool running = gm.State == GameState.Running && !gm.Player.IsAirborne && !gm.Player.IsSliding;
            _phase += Time.deltaTime * gm.Speed * 2.5f;
            _bob = Mathf.Lerp(_bob, running ? Mathf.Sin(_phase) * BobAmount : 0f, Time.deltaTime * 12f);
            transform.localPosition = new Vector3(_rest.x + _x, _rest.y + _bob, _rest.z);
            float vx = (_x - prev) / Mathf.Max(Time.deltaTime, 0.0001f);
            _roll = Mathf.Lerp(_roll, Mathf.Clamp(-vx * 1.5f, -RollDegrees, RollDegrees), Time.deltaTime * 8f);
            var e = transform.localEulerAngles; transform.localRotation = Quaternion.Euler(e.x, e.y, _roll);
            if (_cam != null) _cam.fieldOfView = Mathf.Lerp(_cam.fieldOfView, gm.IsDashing ? DashFov : BaseFov, Time.deltaTime * 5f);
        }
    }
}
