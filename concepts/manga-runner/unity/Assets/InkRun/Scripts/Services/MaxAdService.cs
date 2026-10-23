using System;
using UnityEngine;

namespace InkRun.Services
{
    /// <summary>
    /// AppLovin MAX. Add the MAX Unity plugin, then add APPLOVIN_MAX to
    /// Project Settings > Player > Scripting Define Symbols. Until then this compiles to a
    /// stub that reports no ads ready, so the revive screen is skipped on device builds.
    /// Ad unit ids and the SDK key live here and are not secrets, but keep them per platform.
    /// Check these calls against the installed MAX plugin version; the callback names changed in v6.
    /// </summary>
    public class MaxAdService : IAdService
    {
        public string SdkKey = "";
        public string RewardedUnitId = "";
        public string InterstitialUnitId = "";

        Action<bool> _rewardedDone; bool _gotReward;
        Action _interstitialDone;

#if APPLOVIN_MAX
        public bool IsRewardedReady => MaxSdk.IsRewardedAdReady(RewardedUnitId);
        public bool IsInterstitialReady => MaxSdk.IsInterstitialReady(InterstitialUnitId);

        public void Init()
        {
            MaxSdkCallbacks.OnSdkInitializedEvent += _ =>
            {
                MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += (id, err) => Retry(() => MaxSdk.LoadRewardedAd(RewardedUnitId));
                MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += (id, err, info) => { Finish(false); MaxSdk.LoadRewardedAd(RewardedUnitId); };
                MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += (id, reward, info) => _gotReward = true;
                MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += (id, info) => { Finish(_gotReward); MaxSdk.LoadRewardedAd(RewardedUnitId); };
                MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += (id, err) => Retry(() => MaxSdk.LoadInterstitial(InterstitialUnitId));
                MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += (id, info) => { _interstitialDone?.Invoke(); _interstitialDone = null; MaxSdk.LoadInterstitial(InterstitialUnitId); };
                MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += (id, err, info) => { _interstitialDone?.Invoke(); _interstitialDone = null; MaxSdk.LoadInterstitial(InterstitialUnitId); };
                MaxSdk.LoadRewardedAd(RewardedUnitId);
                MaxSdk.LoadInterstitial(InterstitialUnitId);
            };
            MaxSdk.SetSdkKey(SdkKey);
            MaxSdk.InitializeSdk();
        }

        public void ShowRewarded(string placement, Action<bool> done)
        {
            if (!IsRewardedReady) { done?.Invoke(false); return; }
            _rewardedDone = done; _gotReward = false;
            MaxSdk.ShowRewardedAd(RewardedUnitId, placement);
        }

        public void ShowInterstitial(Action done)
        {
            if (!IsInterstitialReady) { done?.Invoke(); return; }
            _interstitialDone = done;
            MaxSdk.ShowInterstitial(InterstitialUnitId);
        }

        void Retry(Action load) { Services.Runner.StartCoroutine(RetryRoutine(load)); }
        System.Collections.IEnumerator RetryRoutine(Action load) { yield return new WaitForSecondsRealtime(8f); load(); }
#else
        public bool IsRewardedReady => false;
        public bool IsInterstitialReady => false;
        public void Init() { Debug.LogWarning("[Ads] MAX plugin not installed (define APPLOVIN_MAX). Ads disabled."); }
        public void ShowRewarded(string placement, Action<bool> done) => done?.Invoke(false);
        public void ShowInterstitial(Action done) => done?.Invoke();
#endif

        void Finish(bool rewarded)
        {
            var d = _rewardedDone; _rewardedDone = null;
            if (rewarded) { Meta.SaveData.Current.RewardedWatched++; Meta.SaveData.Current.Save(); }
            d?.Invoke(rewarded);
        }
    }
}
