using System;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace InkRun.Player
{
    public enum Swipe { None, Left, Right, Up, Down }

    /// <summary>
    /// Touch swipes plus keyboard for the editor. Works with either input backend: the new
    /// Input System package when it is active (Unity 6 templates), otherwise the legacy
    /// Input class. Fires as soon as the finger moves past the threshold, not on release.
    /// </summary>
    public class SwipeInput : MonoBehaviour
    {
        public float ThresholdPixels = 28f;
        public event Action<Swipe> Swiped;

        Vector2 _start;
        bool _tracking;

        /// <summary>Feed a swipe from code (the debug AutoPilot, tests). Goes through the same event as a real swipe.</summary>
        public void Inject(Swipe s) { if (s != Swipe.None) Swiped?.Invoke(s); }

        void Update()
        {
            Swipe s = Swipe.None;
#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.leftArrowKey.wasPressedThisFrame || kb.aKey.wasPressedThisFrame) s = Swipe.Left;
                else if (kb.rightArrowKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame) s = Swipe.Right;
                else if (kb.upArrowKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame) s = Swipe.Up;
                else if (kb.downArrowKey.wasPressedThisFrame || kb.sKey.wasPressedThisFrame) s = Swipe.Down;
            }
            if (s == Swipe.None)
            {
                var ts = Touchscreen.current;
                var mouse = Mouse.current;
                if (ts != null && (ts.primaryTouch.press.isPressed || ts.primaryTouch.press.wasReleasedThisFrame))
                {
                    var pos = ts.primaryTouch.position.ReadValue();
                    if (ts.primaryTouch.press.wasPressedThisFrame) { _start = pos; _tracking = true; }
                    else if (ts.primaryTouch.press.wasReleasedThisFrame) _tracking = false;
                    else if (_tracking) s = Resolve(pos - _start);
                }
                else if (mouse != null)
                {
                    var pos = mouse.position.ReadValue();
                    if (mouse.leftButton.wasPressedThisFrame) { _start = pos; _tracking = true; }
                    else if (mouse.leftButton.wasReleasedThisFrame) _tracking = false;
                    else if (_tracking && mouse.leftButton.isPressed) s = Resolve(pos - _start);
                }
            }
#else
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
#endif
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
