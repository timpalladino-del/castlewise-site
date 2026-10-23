using UnityEngine;
using InkRun.Core;
using InkRun.World;
using InkRun.FX;

namespace InkRun.Player
{
    /// <summary>
    /// The hero stays at z = 0; the world moves toward them. This script handles lanes,
    /// jump, slide, the collider shape per move, and trigger hits with obstacles and pickups.
    /// Attach to the hero root with a CapsuleCollider (isTrigger) and a kinematic Rigidbody.
    /// </summary>
    [RequireComponent(typeof(CapsuleCollider), typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour
    {
        public SwipeInput Input;
        public Animator Animator;            // optional: parameters Running, Jump, Slide, Crash
        public Transform Visual;             // child that gets the jump offset and slide squash

        public int Lane { get; private set; } = 1;          // 0,1,2
        public bool IsAirborne => _jumpT >= 0f;
        public bool IsSliding => _slideT >= 0f;
        public float Height { get; private set; }           // meters above ground

        Tuning T => GameManager.Instance.Tuning;
        float _laneX, _jumpT = -1f, _slideT = -1f;
        CapsuleCollider _capsule;
        float _capsuleHeight, _capsuleCenterY;

        void Awake()
        {
            _capsule = GetComponent<CapsuleCollider>();
            _capsuleHeight = _capsule.height; _capsuleCenterY = _capsule.center.y;
            var rb = GetComponent<Rigidbody>(); rb.isKinematic = true; rb.useGravity = false;
        }

        void OnEnable() { if (Input != null) Input.Swiped += OnSwipe; }
        void OnDisable() { if (Input != null) Input.Swiped -= OnSwipe; }

        public void ResetPlayer() { Lane = 1; _laneX = 0f; ResetMoves(); transform.position = Vector3.zero; }
        public void ResetMoves() { _jumpT = -1f; _slideT = -1f; Height = 0f; }

        void OnSwipe(Swipe s)
        {
            var gm = GameManager.Instance;
            if (gm.State != GameState.Running) return;
            switch (s)
            {
                case Swipe.Left: if (Lane > 0) Lane--; break;
                case Swipe.Right: if (Lane < 2) Lane++; break;
                case Swipe.Up:
                    if (_jumpT < 0f) { _jumpT = 0f; _slideT = -1f; gm.Run.Jumps++; InkAudio.Play("jump"); Animator?.SetTrigger("Jump"); SfxWords.Pop("HUP!", transform.position + new Vector3(0.8f, 2.6f, 0f), 0.6f); }
                    break;
                case Swipe.Down:
                    if (_jumpT >= 0f) _jumpT = Mathf.Max(_jumpT, 0.8f);               // fast fall
                    else if (_slideT < 0f) { _slideT = 0f; gm.Run.Slides++; InkAudio.Play("slide"); Animator?.SetTrigger("Slide"); SfxWords.Pop("SHHK!", transform.position + new Vector3(-0.9f, 0.8f, 0f), 0.55f); }
                    break;
            }
        }

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm.State != GameState.Running) return;
            float dt = Time.deltaTime;

            float targetX = (Lane - 1) * T.LaneWidth;
            _laneX = Mathf.Lerp(_laneX, targetX, Mathf.Min(1f, dt * T.LaneChangeSpeed));

            if (_jumpT >= 0f) { _jumpT += dt / T.JumpDuration; if (_jumpT >= 1f) _jumpT = -1f; }
            Height = _jumpT >= 0f ? T.JumpHeight * 4f * _jumpT * (1f - _jumpT) : 0f;
            if (_slideT >= 0f) { _slideT += dt / T.SlideDuration; if (_slideT >= 1f) _slideT = -1f; }

            transform.position = new Vector3(_laneX, Height, 0f);
            if (Visual != null) Visual.localScale = new Vector3(1f, IsSliding ? 0.55f : 1f, 1f);

            // slide shrinks the collider so overhead signs pass over
            _capsule.height = IsSliding ? _capsuleHeight * 0.5f : _capsuleHeight;
            _capsule.center = new Vector3(0f, IsSliding ? _capsuleCenterY * 0.5f : _capsuleCenterY, 0f);

            Animator?.SetBool("Running", true);
            Animator?.SetFloat("Speed", gm.Speed);
        }

        void OnTriggerEnter(Collider other)
        {
            var gm = GameManager.Instance;
            if (gm.State != GameState.Running) return;

            var pickup = other.GetComponentInParent<Pickup>();
            if (pickup != null) { pickup.Collect(gm); return; }

            var obstacle = other.GetComponentInParent<Obstacle>();
            if (obstacle != null && !obstacle.IsSafe(this)) { if (gm.HitObstacle(obstacle)) Animator?.SetTrigger("Crash"); }
        }
    }
}
