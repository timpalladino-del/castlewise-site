using System;
using System.Collections;
using UnityEngine;
using InkRun.FX;
using InkRun.Meta;
using InkRun.Services;
using InkRun.World;
using InkRun.Player;

namespace InkRun.Core
{
    /// <summary>
    /// Owns the run state machine and the numbers the HUD shows. Everything else
    /// (spawner, player, UI) listens to its events. One per scene.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Wiring")]
        public Tuning Tuning;
        public PlayerController Player;
        public WorldMover World;
        public TrackSpawner Spawner;
        public AreaController Areas;
        public OutfitDefinition[] Outfits;

        public GameState State { get; private set; } = GameState.Idle;
        public RunStats Run { get; private set; } = new RunStats();
        public float Speed { get; private set; }
        public float DashTimeLeft { get; private set; }
        public float MagnetTimeLeft { get; private set; }
        public float InvulnerableTimeLeft { get; private set; }
        public int RevivesUsed { get; private set; }
        public ObstacleKind LastCrashKind { get; private set; }
        public bool IsDashing => DashTimeLeft > 0f;
        public bool IsInvulnerable => InvulnerableTimeLeft > 0f || IsDashing;

        public event Action<GameState> StateChanged;
        public event Action<int> CoinsChanged;
        public event Action<string, Vector3, float> SfxWord;   // text, world position, size
        public event Action NearRecord;

        bool _nearRecordShown;

        void Awake()
        {
            Instance = this;
            Services.Services.Boot(Tuning, Outfits);
        }

        void Start()
        {
            // build the world behind the title screen so the hero stands on a rooftop before the first tap
            Spawner.ResetTrack();
            Areas.ResetToFirst();
            Player.ResetPlayer();
            SetState(GameState.Idle);
        }

        public void StartRun()
        {
            Run = new RunStats { Assist = SaveData.Current.ShouldAssist(Tuning) };
            RevivesUsed = 0;
            DashTimeLeft = MagnetTimeLeft = InvulnerableTimeLeft = 0f;
            _nearRecordShown = false;
            Spawner.ResetTrack();
            Player.ResetPlayer();
            Time.timeScale = 1f;
            SetState(GameState.Running);
            Areas.ResetToFirst();   // after the state change so the HUD is visible for the chapter card
            Services.Services.Analytics.Log("run_start", ("assist", Run.Assist));
        }

        void Update()
        {
            if (State != GameState.Running) return;
            float dt = Time.deltaTime;
            Run.Seconds += dt;

            DashTimeLeft = Mathf.Max(0f, DashTimeLeft - dt);
            MagnetTimeLeft = Mathf.Max(0f, MagnetTimeLeft - dt);
            InvulnerableTimeLeft = Mathf.Max(0f, InvulnerableTimeLeft - dt);

            Speed = Tuning.SpeedAt(Run.Distance, Run.Assist, IsDashing);
            float moved = Speed * dt;
            int beforeMilestone = Mathf.FloorToInt(Run.Distance / Tuning.MilestoneMeters);
            Run.Distance += moved;
            int milestone = Mathf.FloorToInt(Run.Distance / Tuning.MilestoneMeters);
            if (milestone > beforeMilestone) { SfxWord?.Invoke((milestone * Tuning.MilestoneMeters).ToString("N0") + "m!", Player.transform.position + new Vector3(1.3f, 2.8f, 1f), 0.8f); InkAudio.Play("ui", 0.7f); }
            World.Advance(moved);
            Spawner.Tick(moved, Tuning.PatternGapAt(Run.Distance, Run.Assist));
            Areas.Tick(Run.Distance, dt);
            if (Areas.CurrentIndex == 1) Run.ReachedHarbor = true;

            int best = SaveData.Current.BestDistance;
            if (!_nearRecordShown && best > Tuning.NearRecordMeters && best - Run.Distance <= Tuning.NearRecordMeters && best - Run.Distance > 0f)
            {
                _nearRecordShown = true;
                NearRecord?.Invoke();
            }
        }

        // ---- called by the world ----

        public void CollectCoin(Vector3 at)
        {
            Run.Coins++;
            CoinsChanged?.Invoke(Run.Coins);
            InkAudio.Play("coin", 0.8f);
        }

        static readonly string[] CloseCallWords = { "NICE!", "WHOOSH!", "CLOSE!", "ZIP!" };
        float _closeCallCooldown;

        /// <summary>An obstacle cleared in the hero's own lane. A small word, nothing more; the reward is the feel.</summary>
        public void CloseCall(Obstacle obstacle)
        {
            if (State != GameState.Running || IsDashing) return;
            Run.CloseCalls++;
            if (Time.time < _closeCallCooldown) return;
            _closeCallCooldown = Time.time + 0.7f;
            InkAudio.Play("whoosh", 0.6f);
            SfxWord?.Invoke(CloseCallWords[UnityEngine.Random.Range(0, CloseCallWords.Length)], Player.transform.position + new Vector3(-1.1f, 2.3f, 0.5f), 0.55f);
        }

        public void ActivateMagnet() { InkAudio.Play("power"); MagnetTimeLeft = Tuning.MagnetDuration; SfxWord?.Invoke("MAGNET!", Player.transform.position + Vector3.up * 2.5f, 1f); }
        public void ActivateDash() { InkAudio.Play("power"); DashTimeLeft = Tuning.DashDuration; SfxWord?.Invoke("DASH!", Player.transform.position + Vector3.up * 2.5f, 1.4f); }

        /// <summary>An obstacle the player did not avoid. Returns true if the run ends.</summary>
        public bool HitObstacle(Obstacle obstacle)
        {
            if (State != GameState.Running) return false;
            if (IsInvulnerable)
            {
                if (IsDashing) Run.Smashes++;
                obstacle.Smash();
                InkAudio.Play("smash");
                SfxWord?.Invoke("WHAM!", obstacle.transform.position + Vector3.up * 1.5f, 1f);
                return false;
            }
            Crash(obstacle);
            return true;
        }

        void Crash(Obstacle obstacle)
        {
            LastCrashKind = obstacle.Kind;
            SfxWord?.Invoke("WHAM!", Player.transform.position + Vector3.up * 2f, 1.8f);
            SfxWord?.Invoke("DON!", Player.transform.position + Vector3.right * 1.2f + Vector3.up * 1f, 0.9f);   // ドン! once a Japanese font is in
            SetState(GameState.Crashed);
            InkAudio.Play("crash");
            Services.Services.Analytics.Log("crash", ("distance", Mathf.FloorToInt(Run.Distance)), ("kind", obstacle.Kind.ToString()));
            StartCoroutine(CrashSlowMo());
        }

        IEnumerator CrashSlowMo()
        {
            Time.timeScale = 0.3f;                       // the manga freeze-frame
            yield return new WaitForSecondsRealtime(0.9f);
            Time.timeScale = 1f;
            AfterCrash();
        }

        void AfterCrash()
        {
            if (RevivesUsed < Tuning.RevivesPerRun && Services.Services.Ads.IsRewardedReady)
                SetState(GameState.Revive);
            else
                EndRun();
        }

        // ---- revive flow (the "To Be Continued...?" screen calls these) ----

        bool _reviveRequested;   // a second tap on Keep Running while the ad is up must not revive twice

        public void RequestRevive()
        {
            if (State != GameState.Revive || _reviveRequested) return;
            _reviveRequested = true;
            Services.Services.Ads.ShowRewarded("revive", rewarded =>
            {
                _reviveRequested = false;
                if (rewarded) Revive(); else EndRun();
            });
        }

        void Revive()
        {
            RevivesUsed++;
            Run.Revives++;
            InvulnerableTimeLeft = Tuning.ReviveInvulnerability;
            Spawner.ClearObstaclesAhead(14f);
            Player.ResetMoves();
            Time.timeScale = 1f;
            InkAudio.Play("revive");
            SetState(GameState.Running);
            SfxWord?.Invoke("AGAIN!", Player.transform.position + Vector3.up * 3f, 1.4f);
            Services.Services.Analytics.Log("revive", ("distance", Mathf.FloorToInt(Run.Distance)));
        }

        public void GiveUp() { if (State == GameState.Revive && !_reviveRequested) EndRun(); }

        // ---- pause ----

        public void Pause()
        {
            if (State != GameState.Running) return;
            Time.timeScale = 0f;
            SetState(GameState.Paused);
        }

        public void Resume()
        {
            if (State != GameState.Paused) return;
            Time.timeScale = 1f;
            SetState(GameState.Running);
        }

        void OnApplicationPause(bool paused) { if (paused) Pause(); }
        void OnApplicationFocus(bool focused) { if (!focused) Pause(); }

        void EndRun()
        {
            var save = SaveData.Current;
            bool record = Run.Distance > save.BestDistance;
            if (record) save.BestDistance = Mathf.FloorToInt(Run.Distance);
            var missions = Services.Services.Missions.ApplyRun(Run);
            save.Bank += Run.Coins + missions.CoinsEarned;
            save.RecordRun(Run.Seconds, Tuning);
            save.Save();
            LastResult = new RunResult { Record = record, MissionsCleared = missions.Cleared, MissionCoins = missions.CoinsEarned };
            if (missions.Cleared.Length > 0) InkAudio.Play("clear");
            SetState(GameState.Results);
            Services.Services.Analytics.Log("run_end", ("distance", Mathf.FloorToInt(Run.Distance)), ("coins", Run.Coins), ("record", record));
        }

        public RunResult LastResult { get; private set; }

        /// <summary>"Save the haul": doubles this run's coins after a rewarded ad.</summary>
        public void RequestDoubleCoins(Action<bool> done)
        {
            Services.Services.Ads.ShowRewarded("haul", rewarded =>
            {
                if (rewarded) { SaveData.Current.Bank += Run.Coins; SaveData.Current.Save(); }
                done?.Invoke(rewarded);
            });
        }

        public void BackToStart()
        {
            Services.Services.AdPacing.MaybeShowInterstitial();
            SetState(GameState.Idle);
        }

        void SetState(GameState s)
        {
            State = s;
            StateChanged?.Invoke(s);
        }

        public class RunResult
        {
            public bool Record;
            public string[] MissionsCleared;
            public int MissionCoins;
        }
    }
}
