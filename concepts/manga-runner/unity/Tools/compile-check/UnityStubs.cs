// Minimal stand-ins for the Unity, TextMeshPro, UGUI and UnityEditor APIs the Ink Run scripts use.
// Only signatures matter; bodies are empty. Used to compile-check the game code outside Unity.
using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine
{
    public class Object { public string name; public static void Destroy(Object o) { } public static void DontDestroyOnLoad(Object o) { } public static void DestroyImmediate(Object o) { }
        public static T Instantiate<T>(T o) where T : Object => o; public static T Instantiate<T>(T o, Transform parent) where T : Object => o;
        public static T Instantiate<T>(T o, Vector3 p, Quaternion q) where T : Object => o;
        public static bool operator ==(Object a, Object b) => ReferenceEquals(a, b); public static bool operator !=(Object a, Object b) => !ReferenceEquals(a, b);
        public override bool Equals(object o) => ReferenceEquals(this, o); public override int GetHashCode() => 0; public static implicit operator bool(Object o) => o != null; }
    public class Component : Object { public GameObject gameObject; public Transform transform; public string tag; public T GetComponent<T>() => default; public T GetComponentInParent<T>() => default; public T GetComponentInChildren<T>() => default; public T[] GetComponentsInChildren<T>() => new T[0]; public T[] GetComponentsInChildren<T>(bool inactive) => new T[0]; }
    public class Behaviour : Component { public bool enabled; }
    public class MonoBehaviour : Behaviour { public Coroutine StartCoroutine(IEnumerator e) => null; public void Invoke(string m, float t) { } public void CancelInvoke(string m) { } }
    public class ScriptableObject : Object { public static T CreateInstance<T>() where T : ScriptableObject => (T)Activator.CreateInstance(typeof(T)); }
    public class Coroutine { }
    public class WaitForSecondsRealtime { public WaitForSecondsRealtime(float s) { } }
    public class GameObject : Object { public GameObject() { } public GameObject(string n) { } public GameObject(string n, params Type[] c) { } public T[] GetComponentsInChildren<T>(bool inactive) => new T[0]; public T[] GetComponentsInChildren<T>() => new T[0]; public static GameObject CreatePrimitive(PrimitiveType p) => new GameObject(); public Transform transform; public void SetActive(bool b) { } public bool activeSelf; public T AddComponent<T>() where T : Component, new() => new T(); public T GetComponent<T>() => default; public T GetComponentInChildren<T>() => default; }
    public class Transform : Component, IEnumerable { public Vector3 position, localPosition, localScale, forward; public Quaternion rotation, localRotation; public Vector3 localEulerAngles; public void SetParent(Transform p, bool keep) { } public Transform parent; public int childCount; public Transform GetChild(int i) => null; public Transform Find(string n) => null; public void Rotate(float x, float y, float z, Space s) { } public void LookAt(Vector3 v) { } public IEnumerator GetEnumerator() => null; }
    public enum Space { World, Self }
    public struct Vector2 { public float x, y; public static Vector2 zero, one; public Vector2(float x, float y) { this.x = x; this.y = y; } public float magnitude => 0; public static Vector2 operator -(Vector2 a, Vector2 b) => a; public static Vector2 operator +(Vector2 a, Vector2 b) => a; public static implicit operator Vector2(Vector3 v) => new Vector2(); public static explicit operator Vector3(Vector2 v) => new Vector3(); }
    public struct Vector3 { public float x, y, z; public Vector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; } public static Vector3 zero, up, right, one, forward;
        public static Vector3 operator +(Vector3 a, Vector3 b) => a; public static Vector3 operator -(Vector3 a, Vector3 b) => a; public static Vector3 operator *(Vector3 a, float f) => a; public static Vector3 operator *(float f, Vector3 a) => a;
        public static Vector3 MoveTowards(Vector3 a, Vector3 b, float d) => a; }
    public struct Quaternion { public static Quaternion identity; public static Quaternion Euler(float x, float y, float z) => identity; }
    public struct Color { public static Color white, clear, black; public float r, g, b, a; public Color(float r, float g, float b) { this.r = r; this.g = g; this.b = b; a = 1; } public Color(float r, float g, float b, float a) { this.r = r; this.g = g; this.b = b; this.a = a; } public static Color Lerp(Color a, Color b, float t) => a; }
    public static class Mathf { public static float Exp(float f) => (float)Math.Exp(f); public static float Lerp(float a, float b, float t) => a + (b - a) * Clamp01(t); public static float Min(float a, float b) => Math.Min(a, b); public static float Max(float a, float b) => Math.Max(a, b); public static int Max(int a, int b) => Math.Max(a, b); public static int RoundToInt(float f) => (int)Math.Round(f); public static float Clamp01(float f) => f < 0 ? 0 : f > 1 ? 1 : f; public static float Abs(float f) => Math.Abs(f); public static int FloorToInt(float f) => (int)Math.Floor(f); public static int CeilToInt(float f) => (int)Math.Ceiling(f); public static float InverseLerp(float a, float b, float v) => a == b ? 0 : Clamp01((v - a) / (b - a)); public static float Pow(float a, float b) => (float)Math.Pow(a, b); public static float Sin(float f) => (float)Math.Sin(f); public static float Cos(float f) => (float)Math.Cos(f); public const float PI = 3.14159265f; public static float Clamp(float v, float a, float b) => v < a ? a : v > b ? b : v; public static float MoveTowards(float a, float b, float d) => a < b ? Math.Min(a + d, b) : Math.Max(a - d, b); }
    public static class Random { public static float value; public static float Range(float a, float b) => 0; public static int Range(int a, int b) => 0; public static Vector3 insideUnitSphere; }
    public static class Time { public static float deltaTime, unscaledDeltaTime, time, timeScale; }
    public class AudioClip : Object { public static AudioClip Create(string n, int samples, int ch, int hz, bool stream) => new AudioClip(); public void SetData(float[] d, int off) { } }
    public class AudioListener : Behaviour { }
    public class AudioSource : Behaviour { public bool playOnAwake; public void PlayOneShot(AudioClip c, float v) { } }
    public static class Screen { public static float dpi; public static int width, height; public static Rect safeArea; }
    public struct Rect { public Vector2 position, size; public static bool operator ==(Rect a, Rect b) => true; public static bool operator !=(Rect a, Rect b) => false; public override bool Equals(object o) => true; public override int GetHashCode() => 0; }
    public enum UIOrientation { Portrait, LandscapeLeft }
    public enum KeyCode { LeftArrow, RightArrow, UpArrow, DownArrow, A, D, W, S, Space }
    public enum TouchPhase { Began, Moved, Stationary, Ended, Canceled }
    public struct Touch { public TouchPhase phase; public Vector2 position; }
    public static class Input { public static bool GetKeyDown(KeyCode k) => false; public static int touchCount; public static Touch GetTouch(int i) => default; public static bool GetMouseButtonDown(int b) => false; public static bool GetMouseButton(int b) => false; public static bool GetMouseButtonUp(int b) => false; public static Vector3 mousePosition; }
    public static class Debug { public static void Log(object o) { } public static void LogWarning(object o) { } }
    public static class PlayerPrefs { public static void SetString(string k, string v) { } public static string GetString(string k, string d) => d; public static void Save() { } }
    public static class JsonUtility { public static string ToJson(object o) => ""; public static T FromJson<T>(string s) => default; }
    public class Camera : Behaviour { public static Camera main; public Color backgroundColor; public CameraClearFlags clearFlags; public float fieldOfView; }
    public class Collider : Component { public bool enabled, isTrigger; }
    public class SphereCollider : Collider { public float radius; public Vector3 center; }
    public class BoxCollider : Collider { public Vector3 size, center; }
    public class Renderer : Component { public Material sharedMaterial; public Material[] sharedMaterials; public bool enabled; }
    public enum PrimitiveType { Cube, Sphere, Capsule, Cylinder, Quad }
    public class Light : Behaviour { public LightType type; }
    public enum LightType { Directional, Point }
    public enum CameraClearFlags { Skybox, SolidColor }
    public class RectTransform : Transform { public Vector2 anchorMin, anchorMax, offsetMin, offsetMax, pivot, sizeDelta, anchoredPosition; }
    public class Canvas : Behaviour { public RenderMode renderMode; }
    public enum RenderMode { ScreenSpaceOverlay, ScreenSpaceCamera, WorldSpace }
    public enum TextAnchor { UpperLeft, MiddleCenter }
    public struct Color32 { public Color32(byte r, byte g, byte b, byte a) { } }
    public class CapsuleCollider : Collider { public float height, radius; public Vector3 center; }
    public class Rigidbody : Component { public bool isKinematic, useGravity; }
    public class Animator : Behaviour { public void SetTrigger(string n) { } public void SetBool(string n, bool v) { } public void SetFloat(string n, float v) { } }
    public class Shader : Object { public static int PropertyToID(string n) => 0; public static void SetGlobalColor(int id, Color c) { } public static Shader Find(string n) => null; }
    public class Material : Object { public Material(Shader s) { } public Shader shader; public void SetFloat(int id, float v) { } public void SetFloat(string n, float v) { } public void SetColor(string n, Color c) { } public bool HasProperty(string n) => true; }
    public class SerializeField : Attribute { }
    public class HeaderAttribute : Attribute { public HeaderAttribute(string s) { } }
    public class TooltipAttribute : Attribute { public TooltipAttribute(string s) { } }
    public class RangeAttribute : Attribute { public RangeAttribute(float a, float b) { } }
    public class TextAreaAttribute : Attribute { }
    public class CreateAssetMenuAttribute : Attribute { public string menuName, fileName; }
    public class RequireComponent : Attribute { public RequireComponent(Type a) { } public RequireComponent(Type a, Type b) { } }
    namespace UI
    {
        public class Graphic : MonoBehaviour { public Material material; public Color color; public bool raycastTarget; public RectTransform rectTransform; }
        public class Image : Graphic { public float fillAmount; public Type type; public FillMethod fillMethod; public enum Type { Simple, Filled } public enum FillMethod { Horizontal, Vertical } }
        public class RawImage : Graphic { }
        public class Outline : MonoBehaviour { public Color effectColor; public Vector2 effectDistance; }
        public class CanvasScaler : MonoBehaviour { public ScaleMode uiScaleMode; public Vector2 referenceResolution; public float matchWidthOrHeight; public enum ScaleMode { ConstantPixelSize, ScaleWithScreenSize } }
        public class GraphicRaycaster : MonoBehaviour { }
        public class LayoutGroup : MonoBehaviour { public float spacing; public bool childForceExpandHeight, childForceExpandWidth, childControlHeight, childControlWidth; public TextAnchor childAlignment; }
        public class VerticalLayoutGroup : LayoutGroup { }
        public class HorizontalLayoutGroup : LayoutGroup { }
        public class LayoutElement : MonoBehaviour { public float preferredHeight, preferredWidth; }
        public class Button : MonoBehaviour { public ButtonClickedEvent onClick = new ButtonClickedEvent(); public bool interactable; public class ButtonClickedEvent { public void AddListener(Action a) { } } }
    }
}
namespace TMPro
{
    public enum TextAlignmentOptions { Left, Center, Right }
    public class TMP_Text : UnityEngine.MonoBehaviour { public string text; public UnityEngine.Color color; public float fontSize; public TextAlignmentOptions alignment; public bool raycastTarget, enableWordWrapping; public UnityEngine.RectTransform rectTransform; public float outlineWidth; public UnityEngine.Color32 outlineColor; }
    public class TextMeshProUGUI : TMP_Text { }
    public class TMP_Settings { public static TMP_Settings instance; }
    public class TextMeshPro : TMP_Text { }
}
namespace UnityEngine.EventSystems { public class EventSystem : MonoBehaviour { } public class StandaloneInputModule : MonoBehaviour { } }
namespace UnityEngine.SceneManagement { public struct Scene { } }
namespace UnityEditor
{
    public static class AssetDatabase { public static bool IsValidFolder(string p) => true; public static string CreateFolder(string a, string b) => ""; public static T LoadAssetAtPath<T>(string p) where T : UnityEngine.Object => null; public static void CreateAsset(UnityEngine.Object o, string p) { } public static void SaveAssets() { } public static string[] FindAssets(string f, string[] folders) => new string[0]; public static string GUIDToAssetPath(string g) => ""; }
    public static class PrefabUtility { public static UnityEngine.GameObject SaveAsPrefabAsset(UnityEngine.GameObject g, string p) => g; public static UnityEngine.Object InstantiatePrefab(UnityEngine.Object o) => o; }
    public static class EditorBuildSettings { public static EditorBuildSettingsScene[] scenes; }
    public static class PlayerSettings { public static string productName; public static UnityEngine.UIOrientation defaultInterfaceOrientation; }
    public class EditorBuildSettingsScene { public EditorBuildSettingsScene(string p, bool e) { } }
    namespace SceneManagement
    {
        public enum NewSceneSetup { EmptyScene, DefaultGameObjects }
        public enum NewSceneMode { Single, Additive }
        public static class EditorSceneManager { public static UnityEngine.SceneManagement.Scene NewScene(NewSceneSetup s, NewSceneMode m) => default; public static bool SaveScene(UnityEngine.SceneManagement.Scene s, string p) => true; }
    }
    public static class EditorUtility { public static void SetDirty(UnityEngine.Object o) { } }
    public class MenuItem : Attribute { public MenuItem(string s) { } }
}

namespace UnityEngine.InputSystem
{
    public class ButtonControl { public bool wasPressedThisFrame, wasReleasedThisFrame, isPressed; }
    public class Vector2Control { public UnityEngine.Vector2 ReadValue() => default; }
    public class Keyboard { public static Keyboard current; public ButtonControl leftArrowKey, rightArrowKey, upArrowKey, downArrowKey, aKey, dKey, wKey, sKey, spaceKey; }
    public class TouchControl { public ButtonControl press; public Vector2Control position; }
    public class Touchscreen { public static Touchscreen current; public TouchControl primaryTouch; }
    public class Mouse { public static Mouse current; public Vector2Control position; public ButtonControl leftButton; }
    namespace UI { public class InputSystemUIInputModule : UnityEngine.MonoBehaviour { } }
}
