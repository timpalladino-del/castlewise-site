using UnityEngine;
using InkRun.Core;
using InkRun.Meta;

namespace InkRun.Services
{
    /// <summary>
    /// Tiny service locator so gameplay code never references an SDK directly.
    /// Editor and tests get fakes; device builds get MAX and Firebase when their
    /// define symbols are set. Boot() is called once by GameManager.Awake().
    /// </summary>
    public static class Services
    {
        public static IAdService Ads { get; private set; }
        public static IAnalytics Analytics { get; private set; }
        public static IRemoteConfig RemoteConfig { get; private set; }
        public static AdPacing AdPacing { get; private set; }
        public static PlayerGroups PlayerGroups { get; private set; }
        public static MissionService Missions { get; private set; }
        public static OutfitService Outfits { get; private set; }
        public static MonoBehaviour Runner { get; private set; }

        static bool _booted;

        public static void Boot(Tuning tuning, OutfitDefinition[] outfits)
        {
            if (_booted) return;
            _booted = true;

            var go = new GameObject("InkRunServices");
            Object.DontDestroyOnLoad(go);
            Runner = go.AddComponent<ServiceRunner>();

#if UNITY_EDITOR
            Ads = new EditorAdService();
#else
            Ads = new MaxAdService();
#endif
#if FIREBASE
            Analytics = new FirebaseAnalytics();
            RemoteConfig = new FirebaseRemoteConfigService();
#else
            Analytics = new DebugAnalytics();
            RemoteConfig = new LocalRemoteConfig();
#endif
            AdPacing = new AdPacing(tuning);
            PlayerGroups = new PlayerGroups(new EditorPurchases());
            Missions = new MissionService();
            Outfits = new OutfitService(outfits);

            var save = SaveData.Current;
            save.Sessions++; save.Save();

            Ads.Init();
            RemoteConfig.FetchAndApply(tuning, () => Analytics.Log("remote_config_applied"));
            Analytics.Log("session_start", ("session", save.Sessions));
        }

    }
}
