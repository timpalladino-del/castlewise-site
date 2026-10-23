using UnityEngine;

namespace InkRun.Meta
{
    /// <summary>One outfit (Assets > Create > Ink Run > Outfit). The hero prefab swaps parts by Id.</summary>
    [CreateAssetMenu(menuName = "Ink Run/Outfit", fileName = "Outfit")]
    public class OutfitDefinition : ScriptableObject
    {
        public string Id = "default";
        public string Name = "Default";
        [TextArea] public string Description = "Hoodie and goggles";
        public int Price = 0;
        public GameObject PreviewPrefab;    // small posed hero for the wardrobe card
        public GameObject[] PartsToEnable;  // optional: hero child objects enabled by name when worn
    }
}
