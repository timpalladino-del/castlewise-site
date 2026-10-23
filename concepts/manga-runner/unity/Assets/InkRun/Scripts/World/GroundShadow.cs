using UnityEngine;

namespace InkRun.World
{
    /// <summary>
    /// Flat disc that stays on the ground under the hero and shrinks as they jump, so the
    /// landing spot reads clearly. Lives outside the hero hierarchy so it ignores the jump height.
    /// </summary>
    public class GroundShadow : MonoBehaviour
    {
        public Transform Target;
        public float BaseScale = 0.9f;
        public float MaxHeight = 1.6f;

        void LateUpdate()
        {
            if (Target == null) return;
            var p = Target.position;
            float k = Mathf.Clamp01(Mathf.Max(0f, p.y) / MaxHeight);
            transform.position = new Vector3(p.x, 0.02f, p.z);
            float s = BaseScale * Mathf.Lerp(1f, 0.55f, k);
            transform.localScale = new Vector3(s, 0.02f, s * 0.8f);
        }
    }
}
