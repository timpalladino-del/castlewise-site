using System.Collections.Generic;
using UnityEngine;
using InkRun.Core;
using InkRun.Player;
using InkRun.World;

namespace InkRun.DebugTools
{
    /// <summary>
    /// Plays the game by itself through the real swipe path (SwipeInput.Inject), for soak tests,
    /// screenshots and tuning sessions where nobody is holding the phone. Add it to any object at
    /// runtime (for example from the Unity CLI: new GameObject("AutoPilot").AddComponent&lt;InkRun.DebugTools.AutoPilot&gt;())
    /// and destroy it to stop. MistakeChance makes it miss on purpose so crash and revive get exercised.
    /// </summary>
    public class AutoPilot : MonoBehaviour
    {
        public bool AutoStart = true;                 // taps through title, revive and results pages
        public float ReactionSeconds = 0.30f;         // acts when the obstacle is this many seconds away
        [Range(0f, 1f)] public float MistakeChance = 0f;
        public bool ChaseCoins = true;
        public bool Revive = true;

        public int Runs { get; private set; }
        public int Crashes { get; private set; }

        float _wait, _laneCooldown; GameState _last;
        string _lastAction = ""; float _lastNearZ, _jumpAtZ, _jumpAtSpeed;
        readonly HashSet<Obstacle> _ignored = new HashSet<Obstacle>();
        readonly HashSet<Obstacle> _rolled = new HashSet<Obstacle>();

        struct Threat { public Obstacle Obstacle; public float Z, ZEnd; public int Lane; }
        readonly List<Threat> _threats = new List<Threat>();
        readonly List<Threat> _coins = new List<Threat>();

        void Update()
        {
            var gm = GameManager.Instance; if (gm == null) return;
            if (gm.State != _last)
            {
                _last = gm.State; _wait = 0f;
                if (gm.State == GameState.Crashed)
                {
                    Crashes++; _ignored.Clear(); _rolled.Clear();
                    Debug.Log($"[AutoPilot] crash at {(int)gm.Run.Distance}m speed={gm.Speed:F1} lane={gm.Player.Lane} x={gm.Player.transform.position.x:F2} height={gm.Player.Height:F2} air={gm.Player.IsAirborne} slide={gm.Player.IsSliding} lastNearZ={_lastNearZ:F2} lastAction={_lastAction} jumpAtZ={_jumpAtZ:F2}@{_jumpAtSpeed:F1}");
                }
            }
            switch (gm.State)
            {
                case GameState.Idle: if (AutoStart && Wait(0.8f)) { gm.StartRun(); Runs++; } return;
                case GameState.Revive: if (AutoStart && Wait(1.0f)) { if (Revive) gm.RequestRevive(); else gm.GiveUp(); } return;
                case GameState.Results: if (AutoStart && Wait(1.2f)) gm.BackToStart(); return;
                case GameState.Crashed: return;
                case GameState.Running: break;
                default: return;
            }
            _wait = 0f;
            Drive(gm);
        }

        bool Wait(float seconds) { _wait += Time.unscaledDeltaTime; if (_wait < seconds) return false; _wait = 0f; return true; }

        void Drive(GameManager gm)
        {
            var p = gm.Player; var input = p.Input; if (input == null) return;
            float laneW = gm.Tuning.LaneWidth, speed = Mathf.Max(1f, gm.Speed), dt = Time.deltaTime;
            _laneCooldown -= dt;
            Scan(gm, laneW);

            float horizon = speed * ReactionSeconds + 1.0f;
            int lane = p.Lane;

            // nearest threat in my target lane or the lane my body is still in mid-change
            int physLane = Mathf.Clamp(Mathf.RoundToInt(p.transform.position.x / laneW) + 1, 0, 2);
            bool found = false; Threat near = default;
            foreach (var t in _threats) if ((t.Lane == lane || t.Lane == physLane) && t.ZEnd > -0.6f && (!found || t.Z < near.Z)) { near = t; found = true; }

            if (found)
            {
                // roll a mistake once per obstacle
                if (!_rolled.Contains(near.Obstacle)) { _rolled.Add(near.Obstacle); if (Random.value < MistakeChance) _ignored.Add(near.Obstacle); }
                if (_ignored.Contains(near.Obstacle)) found = false;
            }

            if (found && gm.IsDashing) found = false;   // dashing: run straight through and let WHAM! happen (revive grace still dodges)

            if (found)
            {
                float wallHorizon = speed * 0.55f + 2.5f;
                switch (near.Obstacle.Kind)
                {
                    case ObstacleKind.Barrier:
                        _lastNearZ = near.Z; _lastAction = "barrier";
                        if (!p.IsAirborne && near.Z < BarrierJumpPoint(near, lane, speed, gm.Tuning.JumpDuration)) { input.Inject(Swipe.Up); _jumpAtZ = near.Z; _jumpAtSpeed = speed; _lastAction = "jump"; }
                        return;
                    case ObstacleKind.Gap:
                        _lastNearZ = near.Z; _lastAction = "gap";
                        // airborne over the whole hole: the trigger spans z-0.65..z+3.05 with the capsule radius
                        if (!p.IsAirborne && near.Z < Mathf.Min(speed * 0.3f + 1f, speed * gm.Tuning.JumpDuration - 3.6f)) { input.Inject(Swipe.Up); _jumpAtZ = near.Z; _jumpAtSpeed = speed; _lastAction = "jump-gap"; }
                        return;
                    case ObstacleKind.Sign:
                        if (near.Z < horizon + 0.5f && !p.IsSliding) input.Inject(Swipe.Down);
                        return;
                    case ObstacleKind.Wall:
                        if (near.Z < wallHorizon && _laneCooldown <= 0f)
                        {
                            int to = FreeLane(lane, speed);
                            if (to != lane) { input.Inject(to < lane ? Swipe.Left : Swipe.Right); _laneCooldown = 0.35f; }
                        }
                        return;
                }
            }

            // nothing urgent: drift toward coins in a safe adjacent lane
            if (ChaseCoins && _laneCooldown <= 0f)
            {
                int best = lane; int bestCount = CoinCount(lane, 12f);
                foreach (int cand in new[] { lane - 1, lane + 1 })
                {
                    if (cand < 0 || cand > 2) continue;
                    if (!LaneClear(cand, speed * 0.9f + 3f)) continue;
                    int c = CoinCount(cand, 12f);
                    if (c > bestCount + 1) { best = cand; bestCount = c; }
                }
                if (best != lane) { input.Inject(best < lane ? Swipe.Left : Swipe.Right); _laneCooldown = 0.5f; }
            }
        }

        /// <summary>
        /// Where to take off for a barrier. The hero is above the 0.5 m clear height from about 9% to 91% of
        /// the jump, so a second barrier within that window is cleared by the same jump if the takeoff is
        /// late enough; one further away needs a landing with room to jump again.
        /// </summary>
        float BarrierJumpPoint(Threat first, int lane, float speed, float jumpSeconds)
        {
            float L = speed * jumpSeconds, safeStart = 0.09f * L, safeEnd = 0.91f * L;
            const float half = 0.55f;                                              // barrier half depth plus capsule radius
            float comfortable = speed * 0.3f + 1f;
            float gap = -1f;
            foreach (var t in _threats)
                if (t.Lane == lane && t.Obstacle.Kind == ObstacleKind.Barrier && t.Z > first.Z + 0.5f && t.Z < first.Z + 2f * L && (gap < 0f || t.Z - first.Z < gap)) gap = t.Z - first.Z;
            if (gap < 0f) return comfortable;
            float latestForBoth = safeEnd - half - gap;                            // take off here or later and one jump clears both rows
            if (latestForBoth >= safeStart + half + 0.3f) return Mathf.Min(comfortable, latestForBoth);
            // otherwise land short of row two with room for a second jump: take off as soon as row one is clearable
            return Mathf.Min(comfortable, safeEnd - half);
        }

        int FreeLane(int lane, float speed)
        {
            float window = speed * 0.9f + 4f;
            int bestLane = lane; float bestScore = -1f;
            foreach (int cand in new[] { lane - 1, lane + 1 })
            {
                if (cand < 0 || cand > 2) continue;
                float score = LaneClear(cand, window) ? 2f : (LaneHasOnlyBeatable(cand, window) ? 1f : 0f);
                if (score > bestScore) { bestScore = score; bestLane = cand; }
            }
            return bestScore > 0f ? bestLane : lane;
        }

        bool LaneClear(int lane, float window)
        {
            foreach (var t in _threats) if (t.Lane == lane && t.ZEnd > -0.6f && t.Z < window) return false;
            return true;
        }
        bool LaneHasOnlyBeatable(int lane, float window)
        {
            foreach (var t in _threats) if (t.Lane == lane && t.ZEnd > -0.6f && t.Z < window && t.Obstacle.Kind == ObstacleKind.Wall) return false;
            return true;
        }
        int CoinCount(int lane, float window)
        {
            int n = 0; foreach (var c in _coins) if (c.Lane == lane && c.Z > -0.5f && c.Z < window) n++; return n;
        }

        void Scan(GameManager gm, float laneW)
        {
            _threats.Clear(); _coins.Clear();
            var live = gm.Spawner.Live; if (live == null) return;
            for (int i = 0; i < live.Count; i++)
            {
                var go = live[i]; if (go == null || !go.activeInHierarchy) continue;
                var pos = go.transform.position; int lane = Mathf.Clamp(Mathf.RoundToInt(pos.x / laneW) + 1, 0, 2);
                var ob = go.GetComponent<Obstacle>();
                if (ob != null)
                {
                    float depth = ob.Kind == ObstacleKind.Wall ? 6.5f : ob.Kind == ObstacleKind.Gap ? 3f : 0.6f;
                    _threats.Add(new Threat { Obstacle = ob, Z = pos.z, ZEnd = pos.z + depth, Lane = lane });
                    continue;
                }
                var pk = go.GetComponent<Pickup>();
                if (pk != null && pk.Kind == PickupKind.Coin) _coins.Add(new Threat { Z = pos.z, ZEnd = pos.z, Lane = lane });
            }
        }
    }
}
