using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace InkRun.EditorTools
{
    /// <summary>
    /// The models in Assets/InkRun/Models are .glb files, which Unity only imports with the glTFast package.
    /// A fresh project made from the Universal 3D template does not have it, so every model silently falls
    /// back to primitives. This adds the package once per editor session if it is missing, then tells you
    /// to re-run the scene build so the imported models get picked up.
    /// </summary>
    [InitializeOnLoad]
    public static class PackageEnsure
    {
        const string Package = "com.unity.cloud.gltfast";
        const string Key = "inkrun.packageEnsure.ran";
        static ListRequest _list;
        static AddRequest _add;

        static PackageEnsure()
        {
            if (SessionState.GetBool(Key, false)) return;
            SessionState.SetBool(Key, true);
            _list = Client.List(true, true);
            EditorApplication.update += Tick;
        }

        [MenuItem("Ink Run/Install glTF importer (glTFast)")]
        public static void InstallNow()
        {
            _add = Client.Add(Package);
            EditorApplication.update -= Tick;
            EditorApplication.update += Tick;
            Debug.Log("Ink Run: adding " + Package + "...");
        }

        static void Tick()
        {
            if (_list != null)
            {
                if (!_list.IsCompleted) return;
                bool has = _list.Status == StatusCode.Success && _list.Result.Any(p => p.name == Package);
                _list = null;
                if (has) { EditorApplication.update -= Tick; return; }
                Debug.Log("Ink Run: the glTF importer (" + Package + ") is missing, so the .glb models cannot import. Adding it now.");
                _add = Client.Add(Package);
                return;
            }
            if (_add != null && _add.IsCompleted)
            {
                if (_add.Status == StatusCode.Success)
                    Debug.Log("Ink Run: " + Package + " " + _add.Result.version + " installed. Models import on the next refresh; then run Ink Run > Build gray-box scene again.");
                else
                    Debug.LogWarning("Ink Run: could not add " + Package + ": " + (_add.Error != null ? _add.Error.message : "unknown error") + ". Add it from Window > Package Manager > Unity Registry > glTFast.");
                _add = null;
                EditorApplication.update -= Tick;
            }
        }
    }
}
