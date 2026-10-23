using System;
using UnityEngine;
using InkRun.Core;

namespace InkRun.Services
{
    /// <summary>
    /// Firebase Remote Config. Add the Firebase Unity SDK (Remote Config + Analytics), then
    /// define FIREBASE in Scripting Define Symbols. Keys mirror Tuning field names in snake_case.
    /// Prices and speed are tuned per experiment, never per player: every player in an
    /// experiment group sees the same values.
    /// </summary>
    public class FirebaseRemoteConfigService : IRemoteConfig
    {
        public void FetchAndApply(Tuning t, Action done)
        {
#if FIREBASE
            var rc = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance;
            rc.FetchAndActivateAsync().ContinueWith(task =>
            {
                try
                {
                    float F(string key, float fallback) { var v = rc.GetValue(key); return v.Source == Firebase.RemoteConfig.ValueSource.RemoteValue ? (float)v.DoubleValue : fallback; }
                    int I(string key, int fallback) { var v = rc.GetValue(key); return v.Source == Firebase.RemoteConfig.ValueSource.RemoteValue ? (int)v.LongValue : fallback; }
                    t.BaseSpeed = F("base_speed", t.BaseSpeed);
                    t.MaxSpeedBonus = F("max_speed_bonus", t.MaxSpeedBonus);
                    t.RampDistance = F("ramp_distance", t.RampDistance);
                    t.PatternGapStart = F("pattern_gap_start", t.PatternGapStart);
                    t.PatternGapMin = F("pattern_gap_min", t.PatternGapMin);
                    t.MagnetDuration = F("magnet_duration", t.MagnetDuration);
                    t.DashDuration = F("dash_duration", t.DashDuration);
                    t.ReviveCountdownSeconds = I("revive_countdown", t.ReviveCountdownSeconds);
                    t.RevivesPerRun = I("revives_per_run", t.RevivesPerRun);
                    t.NoInterstitialsForFirstSessions = I("no_interstitials_first_sessions", t.NoInterstitialsForFirstSessions);
                    t.MaxInterstitialsPerHour = I("max_interstitials_per_hour", t.MaxInterstitialsPerHour);
                }
                catch (Exception e) { Debug.LogWarning("Remote config apply failed: " + e.Message); }
                done?.Invoke();
            }, System.Threading.Tasks.TaskScheduler.FromCurrentSynchronizationContext());
#else
            done?.Invoke();
#endif
        }
    }

#if FIREBASE
    public class FirebaseAnalytics : IAnalytics
    {
        public void Log(string eventName, params (string key, object value)[] parameters)
        {
            var ps = new Firebase.Analytics.Parameter[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                var (k, v) = parameters[i];
                ps[i] = v is int iv ? new Firebase.Analytics.Parameter(k, iv) : v is float fv ? new Firebase.Analytics.Parameter(k, fv) : new Firebase.Analytics.Parameter(k, v?.ToString() ?? "");
            }
            Firebase.Analytics.FirebaseAnalytics.LogEvent(eventName, ps);
        }
    }
#endif
}
