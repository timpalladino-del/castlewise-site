using System;
using System.Collections.Generic;
using UnityEngine;
using InkRun.Core;

namespace InkRun.Meta
{
    /// <summary>
    /// Everything that persists between runs, as one JSON blob in PlayerPrefs.
    /// Cloud save can replace Load/Save later without touching callers.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        const string Key = "inkrun.save.v1";
        static SaveData _current;
        public static SaveData Current => _current ??= Load();

        public int BestDistance;
        public int Bank;
        public bool SoundOn = true;
        public string Outfit = "default";
        public List<string> Owned = new List<string> { "default" };
        public List<float> RecentRunSeconds = new List<float>();
        public int Sessions;
        public bool StarterBought;
        public bool AdFree;
        public int RewardedWatched;
        public long LastInterstitialUnix;
        public List<long> InterstitialTimes = new List<long>();
        public string MissionDay = "";
        public List<MissionSave> Missions = new List<MissionSave>();

        [Serializable] public class MissionSave { public string Id; public int Target; public int Progress; public bool Done; }

        public bool Owns(string outfitId) => Owned.Contains(outfitId);

        public bool ShouldAssist(Tuning t)
        {
            int n = t.AssistAfterQuickRuns;
            if (RecentRunSeconds.Count < n) return false;
            for (int i = RecentRunSeconds.Count - n; i < RecentRunSeconds.Count; i++) if (RecentRunSeconds[i] >= t.QuickRunSeconds) return false;
            return true;
        }

        public void RecordRun(float seconds, Tuning t)
        {
            if (seconds > t.AssistClearsAfterSeconds) RecentRunSeconds.Clear();
            RecentRunSeconds.Add(seconds);
            while (RecentRunSeconds.Count > 5) RecentRunSeconds.RemoveAt(0);
        }

        public void Save()
        {
            try { PlayerPrefs.SetString(Key, JsonUtility.ToJson(this)); PlayerPrefs.Save(); }
            catch (Exception e) { Debug.LogWarning("Save failed: " + e.Message); }
        }

        static SaveData Load()
        {
            try
            {
                var json = PlayerPrefs.GetString(Key, "");
                if (!string.IsNullOrEmpty(json)) return JsonUtility.FromJson<SaveData>(json) ?? new SaveData();
            }
            catch (Exception e) { Debug.LogWarning("Load failed, starting fresh: " + e.Message); }
            return new SaveData();
        }
    }
}
