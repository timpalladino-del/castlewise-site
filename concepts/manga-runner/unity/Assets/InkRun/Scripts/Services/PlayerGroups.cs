using System;
using InkRun.Meta;

namespace InkRun.Services
{
    /// <summary>
    /// Player groups from the playbook. Same prices for everyone; groups only decide which
    /// offer is shown. New players (first 3 sessions) see the starter bundle. Players who
    /// watch many rewarded ads see the ad-free offer instead. Real purchases go through
    /// IPurchases; the editor version succeeds instantly.
    /// </summary>
    public class PlayerGroups
    {
        public const int NewPlayerSessions = 3;
        public const int AdWatcherThreshold = 10;
        public const int StarterBundleCoins = 1500;

        readonly IPurchases _iap;
        public PlayerGroups(IPurchases iap) { _iap = iap; }

        public bool IsNewPlayer => SaveData.Current.Sessions <= NewPlayerSessions && !SaveData.Current.StarterBought;
        public bool IsAdWatcher => SaveData.Current.RewardedWatched >= AdWatcherThreshold && !SaveData.Current.AdFree;
        public bool ShowStarterBundle => IsNewPlayer;
        public bool ShowAdFreeOffer => IsAdWatcher;

        public void BuyStarterBundle(Action done)
        {
            _iap.Purchase("inkrun.starter", ok =>
            {
                if (ok) { var s = SaveData.Current; s.StarterBought = true; s.Bank += StarterBundleCoins; if (!s.Owns("delivery")) s.Owned.Add("delivery"); s.Save(); Services.Analytics.Log("starter_bought"); }
                done?.Invoke();
            });
        }

        public void BuyAdFree(Action done)
        {
            _iap.Purchase("inkrun.adfree", ok =>
            {
                if (ok) { var s = SaveData.Current; s.AdFree = true; s.Save(); Services.Analytics.Log("adfree_bought"); }
                done?.Invoke();
            });
        }
    }

    public interface IPurchases { void Purchase(string productId, Action<bool> done); }

    public class EditorPurchases : IPurchases
    {
        public void Purchase(string productId, Action<bool> done) { UnityEngine.Debug.Log("[IAP] (fake) bought " + productId); done?.Invoke(true); }
    }
}
