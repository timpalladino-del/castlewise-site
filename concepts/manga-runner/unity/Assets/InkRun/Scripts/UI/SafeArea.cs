using UnityEngine;

namespace InkRun.UI
{
    /// <summary>Fits a full-screen RectTransform to the phone's safe area (notch, home bar).</summary>
    [RequireComponent(typeof(RectTransform))]
    public class SafeArea : MonoBehaviour
    {
        RectTransform _rt; Rect _last;

        void Awake() { _rt = GetComponent<RectTransform>(); Apply(); }
        void Update() { if (Screen.safeArea != _last) Apply(); }

        void Apply()
        {
            var sa = Screen.safeArea; _last = sa;
            if (Screen.width <= 0 || Screen.height <= 0) return;
            var min = sa.position; var max = sa.position + sa.size;
            min.x /= Screen.width; min.y /= Screen.height; max.x /= Screen.width; max.y /= Screen.height;
            _rt.anchorMin = min; _rt.anchorMax = max;
            _rt.offsetMin = Vector2.zero; _rt.offsetMax = Vector2.zero;
        }
    }
}
