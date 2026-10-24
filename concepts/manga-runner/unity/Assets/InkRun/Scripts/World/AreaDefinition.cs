using UnityEngine;

namespace InkRun.World
{
    /// <summary>One chapter of the world: name, accent color and the props that dress the track.</summary>
    [CreateAssetMenu(menuName = "Ink Run/Area", fileName = "Area")]
    public class AreaDefinition : ScriptableObject
    {
        public string ChapterLabel = "CHAPTER 1";
        public string DisplayName = "CITY ROOFTOPS";
        public Color Accent = new Color(0.878f, 0.125f, 0.165f);   // #E0202A
        public GameObject[] SideProps;     // buildings, containers, lanterns; one picked per track piece per side
        public bool CrescentMoon;          // night alley draws the sun as a crescent
        public Color Sky = Color.white;    // camera clear color; dark for the night chapter
    }
}
