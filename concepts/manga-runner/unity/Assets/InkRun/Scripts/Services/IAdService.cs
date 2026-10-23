using System;

namespace InkRun.Services
{
    /// <summary>
    /// The game only talks to this. Editor and tests use EditorAdService (fake, instant);
    /// device builds use MaxAdService. Rewarded placements: "revive", "haul".
    /// </summary>
    public interface IAdService
    {
        void Init();
        bool IsRewardedReady { get; }
        bool IsInterstitialReady { get; }
        /// <param name="done">true if the player earned the reward.</param>
        void ShowRewarded(string placement, Action<bool> done);
        void ShowInterstitial(Action done);
    }

    public interface IAnalytics
    {
        void Log(string eventName, params (string key, object value)[] parameters);
    }

    public interface IRemoteConfig
    {
        void FetchAndApply(Core.Tuning tuning, Action done);
    }
}
