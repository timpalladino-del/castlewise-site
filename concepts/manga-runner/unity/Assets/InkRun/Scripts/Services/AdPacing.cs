using System;
using InkRun.Core;
using InkRun.Meta;

namespace InkRun.Services
{
    /// <summary>
    /// Smart ad pacing from the playbook: interstitials only between runs, never in the first
    /// few sessions, a minimum gap between them, and a cap per hour. Rewarded ads are never paced;
    /// the player chose them.
    /// </summary>
    public class AdPacing
    {
        readonly Tuning _t;
        public AdPacing(Tuning t) { _t = t; }

        public bool CanShowInterstitial()
        {
            var s = SaveData.Current;
            if (s.AdFree) return false;
            if (s.Sessions <= _t.NoInterstitialsForFirstSessions) return false;
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (now - s.LastInterstitialUnix < _t.MinSecondsBetweenInterstitials) return false;
            s.InterstitialTimes.RemoveAll(ts => now - ts > 3600);
            return s.InterstitialTimes.Count < _t.MaxInterstitialsPerHour;
        }

        /// <summary>Call between runs only (results → start). Shows one if the rules allow.</summary>
        public void MaybeShowInterstitial()
        {
            if (!CanShowInterstitial() || !Services.Ads.IsInterstitialReady) return;
            var s = SaveData.Current;
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            s.LastInterstitialUnix = now; s.InterstitialTimes.Add(now); s.Save();
            Services.Analytics.Log("interstitial_shown");
            Services.Ads.ShowInterstitial(null);
        }
    }
}
