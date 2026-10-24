using UnityEngine;

namespace InkRun.FX
{
    /// <summary>
    /// Picks the full or the low-poly visual for a character at startup. Low-end phones (little graphics
    /// memory or few cores) get the decimated mesh; everything else gets the full one. Both visuals share the
    /// same joint layout so the animator drives whichever is active.
    /// </summary>
    public class QualityPicker : MonoBehaviour
    {
        public GameObject Full, Low;
        public static bool ForceLow;

        void Awake()
        {
            bool low = ForceLow || SystemInfo.graphicsMemorySize < 1500 || SystemInfo.processorCount <= 4;
            if (Full != null) Full.SetActive(!low || Low == null);
            if (Low != null) Low.SetActive(low);
        }
    }
}
