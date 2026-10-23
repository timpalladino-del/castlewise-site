using System;
using System.Collections.Generic;
using UnityEngine;

namespace InkRun.World
{
    public enum EntryKind { Barrier, Sign, Wall, Gap, Coin, Magnet, Dash }

    /// <summary>
    /// One spawn pattern as data (Assets > Create > Ink Run > Pattern). A pattern is a
    /// list of entries placed relative to the pattern's start. Designers build these;
    /// code never needs to change to add a pattern. Keep at least one survivable lane.
    /// </summary>
    [CreateAssetMenu(menuName = "Ink Run/Pattern", fileName = "Pattern")]
    public class PatternDefinition : ScriptableObject
    {
        [Serializable]
        public class Entry
        {
            public EntryKind Kind;
            [Range(0, 2)] public int Lane = 1;
            public float ZOffset;          // meters from pattern start
            public float Height;           // coins only: meters above ground (for arcs)
        }

        public List<Entry> Entries = new List<Entry>();
        [Tooltip("Relative chance of this pattern being picked.")]
        public float Weight = 1f;
        [Tooltip("Only pick once the run is at least this far (meters).")]
        public float MinDistance = 0f;
        public float Length = 12f;         // meters the pattern occupies

        /// <summary>Helper used by the editor menu and tests to build the prototype's patterns.</summary>
        public void AddCoinLine(int lane, float z, int count, float spacing = 1.6f, Func<int, float> height = null)
        {
            for (int i = 0; i < count; i++)
                Entries.Add(new Entry { Kind = EntryKind.Coin, Lane = lane, ZOffset = z + i * spacing, Height = height != null ? height(i) : 0f });
        }
    }
}
