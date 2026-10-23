using UnityEngine;

namespace InkRun.Core
{
    /// <summary>
    /// Every number a designer or Remote Config might want to change. Create one asset
    /// (Assets > Create > Ink Run > Tuning) and assign it to GameManager.
    /// RemoteConfig overrides are applied by Services.RemoteConfig.ApplyTo(tuning).
    /// </summary>
    [CreateAssetMenu(menuName = "Ink Run/Tuning", fileName = "Tuning")]
    public class Tuning : ScriptableObject
    {
        [Header("Speed (meters per second)")]
        public float BaseSpeed = 12f;
        public float MaxSpeedBonus = 12f;       // speed = Base + Bonus * ramp
        public float RampDistance = 2600f;      // ramp = 1 - exp(-distance / RampDistance)
        public float DashMultiplier = 1.7f;
        public float AssistSpeedMultiplier = 0.85f;

        [Header("Lanes and moves")]
        public float LaneWidth = 2.0f;
        public float LaneChangeSpeed = 11f;     // lerp factor per second
        public float JumpDuration = 0.66f;
        public float JumpHeight = 1.6f;
        public float SlideDuration = 0.72f;

        [Header("Spawning (meters)")]
        public float SpawnDistance = 70f;
        public float PatternGapStart = 22f;
        public float PatternGapMin = 13f;
        public float AssistGapMultiplier = 1.3f;
        public float DespawnBehind = 8f;

        [Header("Power-ups (seconds)")]
        public float MagnetDuration = 8f;
        public float MagnetRange = 20f;
        public float DashDuration = 4f;
        public float ReviveInvulnerability = 2.5f;

        [Header("World")]
        public float AreaLength = 1200f;        // meters per chapter
        public float AccentFadeSeconds = 1.2f;

        [Header("Playbook")]
        public int ReviveCountdownSeconds = 5;
        public int RevivesPerRun = 1;
        public float NearRecordMeters = 200f;
        public int AssistAfterQuickRuns = 3;
        public float QuickRunSeconds = 20f;
        public float AssistClearsAfterSeconds = 40f;

        [Header("Ad pacing")]
        public int NoInterstitialsForFirstSessions = 3;
        public int MaxInterstitialsPerHour = 4;
        public float MinSecondsBetweenInterstitials = 180f;

        public float SpeedAt(float distance, bool assist, bool dashing)
        {
            float ramp = 1f - Mathf.Exp(-distance / RampDistance);
            float s = (BaseSpeed + MaxSpeedBonus * ramp) * (assist ? AssistSpeedMultiplier : 1f);
            return dashing ? s * DashMultiplier : s;
        }

        public float PatternGapAt(float distance, bool assist)
        {
            float ramp = 1f - Mathf.Exp(-distance / RampDistance);
            float g = Mathf.Lerp(PatternGapStart, PatternGapMin, ramp);
            return assist ? g * AssistGapMultiplier : g;
        }
    }
}
