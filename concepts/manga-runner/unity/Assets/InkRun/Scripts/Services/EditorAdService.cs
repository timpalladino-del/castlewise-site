using System;
using System.Collections;
using UnityEngine;

namespace InkRun.Services
{
    /// <summary>Fake ads for the editor and automated tests: a short pause, then the reward.</summary>
    public class EditorAdService : IAdService
    {
        public float FakeSeconds = 3f;
        public bool IsRewardedReady => true;
        public bool IsInterstitialReady => true;
        public void Init() { }

        public void ShowRewarded(string placement, Action<bool> done)
        {
            Debug.Log($"[Ads] (fake) rewarded ad for '{placement}'");
            Services.Runner.StartCoroutine(After(FakeSeconds, () => done?.Invoke(true)));
        }

        public void ShowInterstitial(Action done)
        {
            Debug.Log("[Ads] (fake) interstitial");
            Services.Runner.StartCoroutine(After(1f, done));
        }

        static IEnumerator After(float s, Action a) { yield return new WaitForSecondsRealtime(s); a?.Invoke(); }
    }

    public class DebugAnalytics : IAnalytics
    {
        public void Log(string eventName, params (string key, object value)[] parameters)
        {
            var sb = new System.Text.StringBuilder("[Analytics] ").Append(eventName);
            foreach (var p in parameters) sb.Append(' ').Append(p.key).Append('=').Append(p.value);
            Debug.Log(sb.ToString());
        }
    }

    public class LocalRemoteConfig : IRemoteConfig
    {
        public void FetchAndApply(Core.Tuning tuning, Action done) { done?.Invoke(); }
    }
}
