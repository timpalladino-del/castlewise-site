using UnityEngine;
using InkRun.Core;
using InkRun.Player;

namespace InkRun.World
{
    public enum ObstacleKind { Barrier, Sign, Wall, Gap }

    /// <summary>
    /// Put on each obstacle prefab root with a trigger collider on a child.
    /// Rules match the prototype: Barrier needs a jump, Sign needs a slide,
    /// Gap needs to be airborne, Wall can only be dodged by changing lane.
    /// </summary>
    public class Obstacle : MonoBehaviour
    {
        public ObstacleKind Kind;
        [Tooltip("Barrier only: how high the hero must be (meters) to clear it.")]
        public float ClearHeight = 0.5f;
        public GameObject SmashedVisual;   // optional broken version shown after a dash hit

        public bool IsSafe(PlayerController p)
        {
            switch (Kind)
            {
                case ObstacleKind.Barrier: return p.Height > ClearHeight;
                case ObstacleKind.Sign: return p.IsSliding;
                case ObstacleKind.Gap: return p.IsAirborne;
                default: return false;
            }
        }

        bool _passed;

        void Update()
        {
            // the moment this passes the hero: cleared in their own lane counts as a close call
            if (_passed || transform.position.z > -0.4f) return;
            _passed = true;
            var gm = GameManager.Instance; if (gm == null || gm.State != GameState.Running || Kind == ObstacleKind.Wall) return;
            var p = gm.Player;
            int lane = Mathf.RoundToInt(transform.position.x / gm.Tuning.LaneWidth) + 1;
            if (lane == p.Lane && IsSafe(p)) gm.CloseCall(this);
        }

        public void Smash()
        {
            foreach (var c in GetComponentsInChildren<Collider>()) c.enabled = false;
            if (SmashedVisual != null)
            {
                foreach (Transform child in transform) child.gameObject.SetActive(false);
                SmashedVisual.SetActive(true);
            }
            else gameObject.SetActive(false);
        }
    }
}
