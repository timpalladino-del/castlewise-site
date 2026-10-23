using System;
using System.Collections.Generic;
using InkRun.Core;

namespace InkRun.Meta
{
    /// <summary>
    /// Three missions a day, picked from the pool with a seed from the date, so every player
    /// gets the same three. Progress counts across runs and resets at local midnight.
    /// </summary>
    public class MissionService
    {
        public class Def { public string Id; public Func<int, string> Text; public int[] Targets; public Func<RunStats, int> Stat; public int Reward; }
        public class View { public string Id, Text; public int Target, Progress, Reward; public bool Done; }
        public class Result { public int CoinsEarned; public string[] Cleared = Array.Empty<string>(); }

        static readonly Def[] Pool =
        {
            new Def { Id = "jump",   Text = n => $"Jump {n} times",                 Targets = new[] { 30, 50 },     Stat = r => r.Jumps,  Reward = 100 },
            new Def { Id = "coins",  Text = n => $"Collect {n} coins",              Targets = new[] { 150, 300 },   Stat = r => r.Coins,  Reward = 150 },
            new Def { Id = "slide",  Text = n => $"Slide {n} times",                Targets = new[] { 20, 40 },     Stat = r => r.Slides, Reward = 100 },
            new Def { Id = "dist",   Text = n => $"Run {n:N0}m in total",           Targets = new[] { 2000, 4000 }, Stat = r => (int)r.Distance, Reward = 150 },
            new Def { Id = "harbor", Text = n => "Reach The Harbor in one run",     Targets = new[] { 1 },          Stat = r => r.ReachedHarbor ? 1 : 0, Reward = 200 },
            new Def { Id = "smash",  Text = n => $"Smash {n} obstacles with a dash", Targets = new[] { 3, 6 },      Stat = r => r.Smashes, Reward = 150 },
            new Def { Id = "revive", Text = n => "Keep running after a crash",      Targets = new[] { 1 },          Stat = r => r.Revives, Reward = 100 },
        };

        static string TodayKey() => DateTime.Now.ToString("yyyy-MM-dd");

        /// <summary>FNV-1a. string.GetHashCode can differ between runtimes, this cannot.</summary>
        public static int StableHash(string s)
        {
            unchecked { uint h = 2166136261; foreach (char c in s) { h ^= c; h *= 16777619; } return (int)h; }
        }

        static Def Find(string id) { foreach (var d in Pool) if (d.Id == id) return d; return null; }

        void EnsureToday()
        {
            var save = SaveData.Current;
            string day = TodayKey();
            if (save.MissionDay == day && save.Missions.Count == 3) return;
            var rng = new System.Random(StableHash(day));   // same three missions on every device
            var chosen = new List<Def>();
            while (chosen.Count < 3) { var d = Pool[rng.Next(Pool.Length)]; if (!chosen.Contains(d)) chosen.Add(d); }
            save.MissionDay = day;
            save.Missions = new List<SaveData.MissionSave>();
            foreach (var d in chosen) save.Missions.Add(new SaveData.MissionSave { Id = d.Id, Target = d.Targets[rng.Next(d.Targets.Length)] });
            save.Save();
        }

        public List<View> Today()
        {
            EnsureToday();
            var list = new List<View>();
            foreach (var m in SaveData.Current.Missions)
            {
                var d = Find(m.Id); if (d == null) continue;
                list.Add(new View { Id = m.Id, Text = d.Text(m.Target), Target = m.Target, Progress = m.Progress, Reward = d.Reward, Done = m.Done });
            }
            return list;
        }

        public Result ApplyRun(RunStats run)
        {
            EnsureToday();
            var res = new Result(); var cleared = new List<string>();
            foreach (var m in SaveData.Current.Missions)
            {
                if (m.Done) continue;
                var d = Find(m.Id); if (d == null) continue;
                m.Progress = Math.Min(m.Target, m.Progress + d.Stat(run));
                if (m.Progress >= m.Target) { m.Done = true; res.CoinsEarned += d.Reward; cleared.Add(d.Text(m.Target)); }
            }
            res.Cleared = cleared.ToArray();
            return res;
        }
    }
}
