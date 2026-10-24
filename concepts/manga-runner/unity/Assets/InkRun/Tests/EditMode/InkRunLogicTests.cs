using NUnit.Framework;
using UnityEngine;
using InkRun.Core;
using InkRun.Meta;
using InkRun.Services;

// Pure-logic tests. Run in Unity's Test Runner (EditMode) or outside Unity with Tools/compile-check/check.sh.
namespace InkRun.Tests
{
    public class TuningTests
    {
        static Tuning T() => ScriptableObject.CreateInstance<Tuning>();

        [Test] public void SpeedStartsAtBase() { var t = T(); Assert.AreEqual(t.BaseSpeed, t.SpeedAt(0f, false, false), 0.001f); }
        [Test] public void SpeedRampsUpAndCaps() { var t = T(); float a = t.SpeedAt(500f, false, false), b = t.SpeedAt(5000f, false, false); Assert.Greater(b, a); Assert.Less(b, t.BaseSpeed + t.MaxSpeedBonus + 0.001f); }
        [Test] public void DashMultiplies() { var t = T(); Assert.AreEqual(t.BaseSpeed * t.DashMultiplier, t.SpeedAt(0f, false, true), 0.001f); }
        [Test] public void AssistSlows() { var t = T(); Assert.Less(t.SpeedAt(1000f, true, false), t.SpeedAt(1000f, false, false)); }
        [Test] public void GapShrinksTowardMinimum() { var t = T(); Assert.AreEqual(t.PatternGapStart, t.PatternGapAt(0f, false), 0.001f); Assert.Greater(t.PatternGapAt(0f, false), t.PatternGapAt(3000f, false)); Assert.GreaterOrEqual(t.PatternGapAt(100000f, false), t.PatternGapMin - 0.001f); }
        [Test] public void AssistWidensGap() { var t = T(); Assert.Greater(t.PatternGapAt(0f, true), t.PatternGapAt(0f, false)); }
    }

    public class SaveDataTests
    {
        [Test] public void AssistAfterThreeQuickRuns()
        {
            SaveData.UseFresh(); var t = ScriptableObject.CreateInstance<Tuning>(); var s = SaveData.Current;
            Assert.IsFalse(s.ShouldAssist(t));
            s.RecordRun(8f, t); s.RecordRun(12f, t); Assert.IsFalse(s.ShouldAssist(t));
            s.RecordRun(5f, t); Assert.IsTrue(s.ShouldAssist(t));
        }
        [Test] public void LongRunClearsAssist()
        {
            SaveData.UseFresh(); var t = ScriptableObject.CreateInstance<Tuning>(); var s = SaveData.Current;
            s.RecordRun(5f, t); s.RecordRun(5f, t); s.RecordRun(5f, t); Assert.IsTrue(s.ShouldAssist(t));
            s.RecordRun(45f, t); Assert.IsFalse(s.ShouldAssist(t));
        }
    }

    public class AdPacingTests
    {
        [Test] public void NoInterstitialsInFirstSessions()
        {
            SaveData.UseFresh(); var t = ScriptableObject.CreateInstance<Tuning>(); var p = new AdPacing(t);
            SaveData.Current.Sessions = t.NoInterstitialsForFirstSessions; Assert.IsFalse(p.CanShowInterstitial());
            SaveData.Current.Sessions = t.NoInterstitialsForFirstSessions + 1; Assert.IsTrue(p.CanShowInterstitial());
        }
        [Test] public void RespectsHourlyCapAndMinimumGap()
        {
            SaveData.UseFresh(); var t = ScriptableObject.CreateInstance<Tuning>(); var p = new AdPacing(t); var s = SaveData.Current;
            s.Sessions = 10; long now = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            s.LastInterstitialUnix = now - 10; Assert.IsFalse(p.CanShowInterstitial());
            s.LastInterstitialUnix = now - 100000;
            for (int i = 0; i < t.MaxInterstitialsPerHour; i++) s.InterstitialTimes.Add(now - 60 * (i + 1));
            Assert.IsFalse(p.CanShowInterstitial());
            s.InterstitialTimes.Clear(); s.InterstitialTimes.Add(now - 7200); Assert.IsTrue(p.CanShowInterstitial());
        }
        [Test] public void AdFreePlayersNeverSeeInterstitials()
        {
            SaveData.UseFresh(); var t = ScriptableObject.CreateInstance<Tuning>(); var p = new AdPacing(t);
            SaveData.Current.Sessions = 50; SaveData.Current.AdFree = true; Assert.IsFalse(p.CanShowInterstitial());
        }
    }

    public class MissionTests
    {
        [Test] public void TodayHasThreeDistinctMissions()
        {
            SaveData.UseFresh(); var m = new MissionService(); var list = m.Today();
            Assert.AreEqual(3, list.Count);
            Assert.AreNotEqual(list[0].Id, list[1].Id); Assert.AreNotEqual(list[1].Id, list[2].Id); Assert.AreNotEqual(list[0].Id, list[2].Id);
            foreach (var v in list) { Assert.IsFalse(v.Done); Assert.Greater(v.Target, 0); }
        }
        [Test] public void StableHashIsDeterministic()
        {
            Assert.AreEqual(MissionService.StableHash("2026-10-23"), MissionService.StableHash("2026-10-23"));
            Assert.AreNotEqual(MissionService.StableHash("2026-10-23"), MissionService.StableHash("2026-10-24"));
        }
        [Test] public void ApplyRunProgressesAndRewards()
        {
            SaveData.UseFresh(); var m = new MissionService(); m.Today();
            var big = new RunStats { Jumps = 999, Coins = 999, Slides = 999, Distance = 99999f, ReachedHarbor = true, Smashes = 99, Revives = 9, CloseCalls = 99 };
            var r = m.ApplyRun(big);
            Assert.AreEqual(3, r.Cleared.Length); Assert.Greater(r.CoinsEarned, 0);
            foreach (var v in m.Today()) Assert.IsTrue(v.Done);
            var again = m.ApplyRun(big); Assert.AreEqual(0, again.CoinsEarned);
        }
        [Test] public void PartialProgressAccumulatesAcrossRuns()
        {
            SaveData.UseFresh(); var m = new MissionService(); var first = m.Today();
            m.ApplyRun(new RunStats { Jumps = 1, Coins = 1, Slides = 1, Distance = 1f });
            foreach (var v in m.Today()) if (v.Id == "jump" || v.Id == "coins" || v.Id == "slide" || v.Id == "dist") Assert.AreEqual(1, v.Progress);
            Assert.AreEqual(3, first.Count);
        }
    }
}
