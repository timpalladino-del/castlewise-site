using UnityEngine;
using UnityEngine.UI;

namespace InkRun.UI
{
    /// <summary>
    /// Progress bars without sprites: Image.fillAmount only clips when the Image has a sprite, and
    /// the gray-box UI has none, so a bar at 0 would still draw full. This stretches the fill's
    /// RectTransform across its parent instead, which works with a plain colored Image.
    /// </summary>
    public static class UiFill
    {
        public static void Set(Graphic fill, float t)
        {
            if (fill == null) return;
            var rt = fill.rectTransform;
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(Mathf.Clamp01(t), 1f);
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        }
    }
}
