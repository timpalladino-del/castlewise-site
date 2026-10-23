using UnityEngine;

namespace InkRun.World
{
    /// <summary>
    /// Parent of everything that scrolls (track pieces, obstacles, pickups, buildings).
    /// GameManager calls Advance(meters) each frame; children move toward -z.
    /// Keeping the hero at the origin avoids floating-point drift on long runs.
    /// </summary>
    public class WorldMover : MonoBehaviour
    {
        public float Traveled { get; private set; }

        public void Advance(float meters)
        {
            Traveled += meters;
            transform.position -= new Vector3(0f, 0f, meters);
        }

        public void ResetWorld()
        {
            Traveled = 0f;
            transform.position = Vector3.zero;
            for (int i = transform.childCount - 1; i >= 0; i--) Destroy(transform.GetChild(i).gameObject);
        }

        /// <summary>World z of a scrolled object expressed as distance ahead of the hero.</summary>
        public float Ahead(Transform t) => t.position.z;
    }
}
