using UnityEngine;
using TMPro;

namespace InkRun.World
{
    /// <summary>Tints a TextMeshPro text with the current chapter accent, so sign kanji recolor with the area like the ink materials do.</summary>
    [RequireComponent(typeof(TMP_Text))]
    public class AccentText : MonoBehaviour
    {
        TMP_Text _text; Color _last;
        void Awake() { _text = GetComponent<TMP_Text>(); }
        void LateUpdate()
        {
            var gm = Core.GameManager.Instance; if (gm == null || gm.Areas == null) return;
            var c = gm.Areas.AccentNow;
            if (c != _last) { _last = c; _text.color = c; }
        }
    }
}
