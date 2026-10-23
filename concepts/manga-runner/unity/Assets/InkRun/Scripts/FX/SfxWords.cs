using UnityEngine;
using TMPro;
using InkRun.Core;

namespace InkRun.FX
{
    /// <summary>
    /// Manga sound-effect words (WHAM!, DASH!, ドン!). Spawns a TextMeshPro prefab at a
    /// world position, pops it in, then fades. Listens to GameManager.SfxWord and also
    /// exposes a static Pop() for things like the jump "HUP!".
    /// </summary>
    public class SfxWords : MonoBehaviour
    {
        public static SfxWords Instance { get; private set; }
        public TMP_Text WordPrefab;          // world-space TMP with the comic font, black outline
        public float Lifetime = 0.8f;

        void Awake() { Instance = this; }
        void Start() { GameManager.Instance.SfxWord += Spawn; }
        void OnDestroy() { if (GameManager.Instance != null) GameManager.Instance.SfxWord -= Spawn; }

        public static void Pop(string text, Vector3 at, float size) { if (Instance != null) Instance.Spawn(text, at, size); }

        void Spawn(string text, Vector3 at, float size)
        {
            var w = Instantiate(WordPrefab, at, Quaternion.identity);
            w.text = text;
            w.transform.localScale = Vector3.one * size;
            w.transform.LookAt(w.transform.position + Camera.main.transform.forward);   // face the camera
            w.transform.Rotate(0f, 0f, Random.Range(-12f, 12f), Space.Self);
            w.gameObject.AddComponent<SfxWordLife>().Init(size > 1.2f ? Lifetime * 1.5f : Lifetime);
        }
    }
}
