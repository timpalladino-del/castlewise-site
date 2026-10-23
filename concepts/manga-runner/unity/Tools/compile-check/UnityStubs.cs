// Minimal stand-ins for the Unity, TextMeshPro, UGUI and UnityEditor APIs the Ink Run scripts use.
// Only signatures matter; bodies are empty. Used to compile-check the game code outside Unity.
using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine
{
    public class Object { public string name; public static void Destroy(Object o) { } public static void DontDestroyOnLoad(Object o) { }
        public static T Instantiate<T>(T o) where T : Object => o; public static T Instantiate<T>(T o, Transform parent) where T : Object => o;
        public static T Instantiate<T>(T o, Vector3 p, Quaternion q) where T : Object => o;
        public static bool operator ==(Object a, Object b) => ReferenceEquals(a, b); public static bool operator !=(Object a, Object b) => !ReferenceEquals(a, b);
        public override bool Equals(object o) => ReferenceEquals(this, o); public override int GetHashCode() => 0; public static implicit operator bool(Object o) => o != null; }
    public class Component : Object { public GameObject gameObject; public Transform transform; public T GetComponent<T>() => default; public T GetComponentInParent<T>() => default; public T GetComponentInChildren<T>() => default; public T[] GetComponentsInChildren<T>() => new T[0]; }
    public class Behaviour : Component { public bool enabled; }
    public class MonoBehaviour : Behaviour { public Coroutine StartCoroutine(IEnumerator e) => null; public void Invoke(string m, float t) { } public void CancelInvoke(string m) { } }
    public class ScriptableObject : Object { public static T CreateInstance<T>() where T : ScriptableObject, new() => new T(); }
    public class Coroutine { }
    public class WaitForSecondsRealtime { public WaitForSecondsRealtime(float s) { } }
    public class GameObject : Object { public GameObject() { } public GameObject(string n) { } public Transform transform; public void SetActive(bool b) { } public bool activeSelf; public T AddComponent<T>() where T : Component, new() => new T(); public T GetComponent<T>() => default; public T GetComponentInChildren<T>() => default; }
    public class Transform : Component, IEnumerable { public Vector3 position, localPosition, localScale, forward; public Quaternion rotation; public Transform parent; public int childCount; public Transform GetChild(int i) => null; public Transform Find(string n) => null; public void Rotate(float x, float y, float z, Space s) { } public void LookAt(Vector3 v) { } public IEnumerator GetEnumerator() => null; }
    public enum Space { World, Self }
    public struct Vector2 { public float x, y; public Vector2(float x, float y) { this.x = x; this.y = y; } public float magnitude => 0; public static Vector2 operator -(Vector2 a, Vector2 b) => a; public static implicit operator Vector2(Vector3 v) => new Vector2(); public static explicit operator Vector3(Vector2 v) => new Vector3(); }
    public struct Vector3 { public float x, y, z; public Vector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; } public static Vector3 zero, up, right, one, forward;
        public static Vector3 operator +(Vector3 a, Vector3 b) => a; public static Vector3 operator -(Vector3 a, Vector3 b) => a; public static Vector3 operator *(Vector3 a, float f) => a; public static Vector3 operator *(float f, Vector3 a) => a;
        public static Vector3 MoveTowards(Vector3 a, Vector3 b, float d) => a; }
    public struct Quaternion { public static Quaternion identity; public static Quaternion Euler(float x, float y, float z) => identity; }
    public struct Color { public float r, g, b, a; public Color(float r, float g, float b) { this.r = r; this.g = g; this.b = b; a = 1; } public Color(float r, float g, float b, float a) { this.r = r; this.g = g; this.b = b; this.a = a; } public static Color Lerp(Color a, Color b, float t) => a; }
    public static class Mathf { public static float Exp(float f) => 0; public static float Lerp(float a, float b, float t) => 0; public static float Min(float a, float b) => 0; public static float Max(float a, float b) => 0; public static float Clamp01(float f) => 0; public static float Abs(float f) => 0; public static int FloorToInt(float f) => 0; public static int CeilToInt(float f) => 0; public static float InverseLerp(float a, float b, float v) => 0; public static float Pow(float a, float b) => 0; }
    public static class Random { public static float value; public static float Range(float a, float b) => 0; public static int Range(int a, int b) => 0; public static Vector3 insideUnitSphere; }
    public static class Time { public static float deltaTime, unscaledDeltaTime; }
    public static class Screen { public static float dpi; }
    public enum KeyCode { LeftArrow, RightArrow, UpArrow, DownArrow, A, D, W, S, Space }
    public enum TouchPhase { Began, Moved, Stationary, Ended, Canceled }
    public struct Touch { public TouchPhase phase; public Vector2 position; }
    public static class Input { public static bool GetKeyDown(KeyCode k) => false; public static int touchCount; public static Touch GetTouch(int i) => default; public static bool GetMouseButtonDown(int b) => false; public static bool GetMouseButton(int b) => false; public static bool GetMouseButtonUp(int b) => false; public static Vector3 mousePosition; }
    public static class Debug { public static void Log(object o) { } public static void LogWarning(object o) { } }
    public static class PlayerPrefs { public static void SetString(string k, string v) { } public static string GetString(string k, string d) => d; public static void Save() { } }
    public static class JsonUtility { public static string ToJson(object o) => ""; public static T FromJson<T>(string s) => default; }
    public class Camera : Behaviour { public static Camera main; }
    public class Collider : Component { public bool enabled; }
    public class CapsuleCollider : Collider { public float height; public Vector3 center; }
    public class Rigidbody : Component { public bool isKinematic, useGravity; }
    public class Animator : Behaviour { public void SetTrigger(string n) { } public void SetBool(string n, bool v) { } public void SetFloat(string n, float v) { } }
    public class Shader : Object { public static int PropertyToID(string n) => 0; public static void SetGlobalColor(int id, Color c) { } }
    public class Material : Object { public void SetFloat(int id, float v) { } }
    public class SerializeField : Attribute { }
    public class HeaderAttribute : Attribute { public HeaderAttribute(string s) { } }
    public class TooltipAttribute : Attribute { public TooltipAttribute(string s) { } }
    public class RangeAttribute : Attribute { public RangeAttribute(float a, float b) { } }
    public class TextAreaAttribute : Attribute { }
    public class CreateAssetMenuAttribute : Attribute { public string menuName, fileName; }
    public class RequireComponent : Attribute { public RequireComponent(Type a) { } public RequireComponent(Type a, Type b) { } }
    namespace UI
    {
        public class Graphic : MonoBehaviour { public Material material; public Color color; }
        public class Image : Graphic { public float fillAmount; }
        public class RawImage : Graphic { }
        public class Button : MonoBehaviour { public ButtonClickedEvent onClick = new ButtonClickedEvent(); public bool interactable; public class ButtonClickedEvent { public void AddListener(Action a) { } } }
    }
}
namespace TMPro { public class TMP_Text : UnityEngine.MonoBehaviour { public string text; public UnityEngine.Color color; } }
namespace UnityEditor
{
    public static class AssetDatabase { public static bool IsValidFolder(string p) => true; public static string CreateFolder(string a, string b) => ""; public static T LoadAssetAtPath<T>(string p) where T : UnityEngine.Object => null; public static void CreateAsset(UnityEngine.Object o, string p) { } public static void SaveAssets() { } }
    public static class EditorUtility { public static void SetDirty(UnityEngine.Object o) { } }
    public class MenuItem : Attribute { public MenuItem(string s) { } }
}
