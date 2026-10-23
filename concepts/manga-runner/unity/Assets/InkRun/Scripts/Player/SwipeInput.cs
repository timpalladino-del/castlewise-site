using System;
using UnityEngine;

namespace InkRun.Player
{
    public enum Swipe { None, Left, Right, Up, Down }

    /// <summary>
    /// Touch swipes plus keyboard for the editor. Uses the legacy Input API so no
    /// package setup is needed. Fires as soon as the finger moves past the threshold,
    /// not on release, so the game feels responsive.
    /// </summary>
    public class SwipeInput : MonoBehaviour
    {
        public float ThresholdPixels = 28f;
        public event Action<Swipe> Swiped;

        Vector2 _start;
        bool _tracking;

        void Update()
        {
            Swipe s = Swipe.None;

            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) s = Swipe.Left;
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) s = Swipe.Right;
            else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space)) s = Swipe.Up;
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) s = Swipe.Down;

            if (s == Swipe.None)
            {
                if (Input.touchCount > 0)
                {
                    var t = Input.GetTouch(0);
                    if (t.phase == TouchPhase.Began) { _start = t.position; _tracking = true; }
                    else if (_tracking && (t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary)) s = Resolve(t.position - _start);
                    else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled) _tracking = false;
                }
                else if (Input.GetMouseButtonDown(0)) { _start = Input.mousePosition; _tracking = true; }
                else if (_tracking && Input.GetMouseButton(0)) s = Resolve((Vector2)Input.mousePosition - _start);
                else if (Input.GetMouseButtonUp(0)) _tracking = false;
            }

            if (s != Swipe.None) Swiped?.Invoke(s);
        }

        Swipe Resolve(Vector2 d)
        {
            float px = ThresholdPixels * (Screen.dpi > 0 ? Screen.dpi / 160f : 1f);
            if (d.magnitude < px) return Swipe.None;
            _tracking = false;
            if (Mathf.Abs(d.x) > Mathf.Abs(d.y)) return d.x > 0 ? Swipe.Right : Swipe.Left;
            return d.y > 0 ? Swipe.Up : Swipe.Down;
        }
    }
}
