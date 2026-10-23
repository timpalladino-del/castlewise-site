using UnityEngine;
using InkRun.Core;
using InkRun.Meta;

namespace InkRun.Player
{
    /// <summary>
    /// Shows the child of PartsRoot whose name matches the worn outfit id and hides the others.
    /// Applied when a run starts, so a purchase in the wardrobe shows on the next run.
    /// </summary>
    public class HeroOutfit : MonoBehaviour
    {
        public Transform PartsRoot;

        void Start()
        {
            GameManager.Instance.StateChanged += OnState;
            Apply();
        }
        void OnDestroy() { if (GameManager.Instance != null) GameManager.Instance.StateChanged -= OnState; }
        void OnState(GameState s) { if (s == GameState.Running) Apply(); }

        public void Apply()
        {
            if (PartsRoot == null) return;
            string id = SaveData.Current.Outfit;
            for (int i = 0; i < PartsRoot.childCount; i++)
            {
                var c = PartsRoot.GetChild(i);
                c.gameObject.SetActive(c.name == id);
            }
        }
    }
}
