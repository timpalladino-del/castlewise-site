#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using InkRun.World;

namespace InkRun.EditorTools
{
    /// <summary>
    /// Menu: Ink Run > Create default patterns. Builds the spawn patterns (the browser
    /// prototype's plus dash and magnet set-ups), as PatternDefinition assets in Assets/InkRun/Data/Patterns.
    /// Edit them in the inspector afterwards; code never needs to change for new patterns.
    /// </summary>
    public static class PatternBootstrap
    {
        const string Folder = "Assets/InkRun/Data/Patterns";

        [MenuItem("Ink Run/Create default patterns")]
        public static void Create()
        {
            if (!AssetDatabase.IsValidFolder("Assets/InkRun/Data")) AssetDatabase.CreateFolder("Assets/InkRun", "Data");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/InkRun/Data", "Patterns");

            Make("P1_SingleObstacle", 2.2f, 0f, p =>
            {
                p.Entries.Add(new PatternDefinition.Entry { Kind = EntryKind.Barrier, Lane = 1, ZOffset = 0f });
                p.AddCoinLine(0, -2f, 5);
                p.Length = 10f;
            });
            Make("P2_TwoBlockedOneFree", 2.0f, 150f, p =>
            {
                p.Entries.Add(new PatternDefinition.Entry { Kind = EntryKind.Wall, Lane = 0, ZOffset = 0f });
                p.Entries.Add(new PatternDefinition.Entry { Kind = EntryKind.Sign, Lane = 2, ZOffset = 1f });
                p.AddCoinLine(1, -3f, 6);
                p.Length = 12f;
            });
            Make("P3_BarrierRowWithArc", 1.2f, 300f, p =>
            {
                for (int l = 0; l < 3; l++) p.Entries.Add(new PatternDefinition.Entry { Kind = EntryKind.Barrier, Lane = l, ZOffset = 0f });
                p.AddCoinLine(1, -5f, 5, 2.5f, i => 1.6f * (1f - Mathf.Pow((i - 2f) / 2f, 2f)));
                p.Length = 12f;
            });
            Make("P4_SignRow", 0.8f, 400f, p =>
            {
                for (int l = 0; l < 3; l++) p.Entries.Add(new PatternDefinition.Entry { Kind = EntryKind.Sign, Lane = l, ZOffset = 0f });
                p.AddCoinLine(0, -4f, 5);
                p.Length = 10f;
            });
            Make("P5_CoinZigzag", 0.8f, 0f, p =>
            {
                int lane = 0;
                for (int i = 0; i < 8; i++) { p.Entries.Add(new PatternDefinition.Entry { Kind = EntryKind.Coin, Lane = lane, ZOffset = i * 1.6f }); if (i % 3 == 2) lane = lane == 0 ? 1 : lane == 2 ? 1 : (i % 2 == 0 ? 0 : 2); }
                p.Length = 14f;
            });
            Make("P6_PowerUp", 0.5f, 200f, p =>
            {
                p.Entries.Add(new PatternDefinition.Entry { Kind = EntryKind.Magnet, Lane = 2, ZOffset = 0f, Height = 0.6f });
                p.AddCoinLine(0, 0f, 4);
                p.Length = 8f;
            });
            Make("P7_GapAndTrain", 1.0f, 250f, p =>
            {
                p.Entries.Add(new PatternDefinition.Entry { Kind = EntryKind.Gap, Lane = 1, ZOffset = 0f });
                p.Entries.Add(new PatternDefinition.Entry { Kind = EntryKind.Wall, Lane = 2, ZOffset = 0f });
                p.AddCoinLine(0, -2f, 5);
                p.Length = 12f;
            });
            Make("P8_DoubleBarrierRows", 0.9f, 500f, p =>
            {
                for (int l = 0; l < 3; l++) { p.Entries.Add(new PatternDefinition.Entry { Kind = EntryKind.Barrier, Lane = l, ZOffset = 0f }); p.Entries.Add(new PatternDefinition.Entry { Kind = EntryKind.Barrier, Lane = l, ZOffset = 8f }); }
                p.AddCoinLine(1, 2.5f, 3); p.Length = 16f;
            });
            Make("P9_SignsAndTrain", 0.9f, 400f, p =>
            {
                p.Entries.Add(new PatternDefinition.Entry { Kind = EntryKind.Sign, Lane = 0, ZOffset = 0f });
                p.Entries.Add(new PatternDefinition.Entry { Kind = EntryKind.Sign, Lane = 1, ZOffset = 0f });
                p.Entries.Add(new PatternDefinition.Entry { Kind = EntryKind.Wall, Lane = 2, ZOffset = 0f });
                p.AddCoinLine(0, 3f, 4); p.Length = 14f;
            });
            Make("P10_CoinRiver", 0.7f, 0f, p => { for (int l = 0; l < 3; l++) p.AddCoinLine(l, 0f, 6); p.Length = 12f; });
            Make("P11_Gauntlet", 0.8f, 700f, p =>
            {
                p.Entries.Add(new PatternDefinition.Entry { Kind = EntryKind.Wall, Lane = 0, ZOffset = 0f });
                p.Entries.Add(new PatternDefinition.Entry { Kind = EntryKind.Wall, Lane = 2, ZOffset = 0f });
                p.Entries.Add(new PatternDefinition.Entry { Kind = EntryKind.Barrier, Lane = 1, ZOffset = 4f });
                p.AddCoinLine(1, 6f, 4, 1.6f, i => 1.4f * (1f - Mathf.Pow((i - 1.5f) / 1.5f, 2f))); p.Length = 16f;
            });
            Make("P12_GapRow", 0.8f, 350f, p =>
            {
                p.Entries.Add(new PatternDefinition.Entry { Kind = EntryKind.Gap, Lane = 0, ZOffset = 0f });
                p.Entries.Add(new PatternDefinition.Entry { Kind = EntryKind.Gap, Lane = 1, ZOffset = 0f });
                p.AddCoinLine(2, -2f, 6); p.Length = 12f;
            });
            Make("P13_DashRunway", 0.6f, 300f, p =>
            {
                // a dash in the middle, then a wall of things to smash through: the one place the WHAM! pays off
                p.Entries.Add(new PatternDefinition.Entry { Kind = EntryKind.Dash, Lane = 1, ZOffset = 0f, Height = 0.6f });
                for (int l = 0; l < 3; l++) p.Entries.Add(new PatternDefinition.Entry { Kind = EntryKind.Barrier, Lane = l, ZOffset = 14f });
                p.Entries.Add(new PatternDefinition.Entry { Kind = EntryKind.Sign, Lane = 1, ZOffset = 24f });
                p.Entries.Add(new PatternDefinition.Entry { Kind = EntryKind.Wall, Lane = 0, ZOffset = 32f });
                p.Entries.Add(new PatternDefinition.Entry { Kind = EntryKind.Wall, Lane = 2, ZOffset = 32f });
                p.AddCoinLine(1, 16f, 10, 2.2f);
                p.Length = 44f;
            });
            Make("P14_DashSide", 0.4f, 600f, p =>
            {
                p.Entries.Add(new PatternDefinition.Entry { Kind = EntryKind.Dash, Lane = 0, ZOffset = 0f, Height = 0.6f });
                p.Entries.Add(new PatternDefinition.Entry { Kind = EntryKind.Wall, Lane = 1, ZOffset = 2f });
                p.AddCoinLine(2, -2f, 6);
                p.Length = 14f;
            });
            Make("P15_MagnetArc", 0.5f, 450f, p =>
            {
                p.Entries.Add(new PatternDefinition.Entry { Kind = EntryKind.Magnet, Lane = 1, ZOffset = 0f, Height = 0.6f });
                for (int l = 0; l < 3; l++) p.AddCoinLine(l, 6f, 6, 1.8f, i => l == 1 ? 1.5f * (1f - Mathf.Pow((i - 2.5f) / 2.5f, 2f)) : 0f);
                p.Length = 20f;
            });
            Make("P16_TrainCorridor", 0.9f, 1000f, p =>
            {
                // trains on both sides, two barriers down the middle: jump, land, jump
                p.Entries.Add(new PatternDefinition.Entry { Kind = EntryKind.Wall, Lane = 0, ZOffset = 0f });
                p.Entries.Add(new PatternDefinition.Entry { Kind = EntryKind.Wall, Lane = 2, ZOffset = 0f });
                p.Entries.Add(new PatternDefinition.Entry { Kind = EntryKind.Barrier, Lane = 1, ZOffset = 2f });
                p.Entries.Add(new PatternDefinition.Entry { Kind = EntryKind.Barrier, Lane = 1, ZOffset = 12f });
                p.AddCoinLine(1, 5f, 3, 1.8f, i => 1.4f * (1f - Mathf.Pow((i - 1f), 2f)));
                p.Length = 18f;
            });
            Make("P17_Stagger", 1.0f, 1400f, p =>
            {
                // sign, barrier, sign staggered across the lanes: every lane needs a different move
                p.Entries.Add(new PatternDefinition.Entry { Kind = EntryKind.Sign, Lane = 0, ZOffset = 0f });
                p.Entries.Add(new PatternDefinition.Entry { Kind = EntryKind.Barrier, Lane = 1, ZOffset = 4f });
                p.Entries.Add(new PatternDefinition.Entry { Kind = EntryKind.Sign, Lane = 2, ZOffset = 8f });
                p.Entries.Add(new PatternDefinition.Entry { Kind = EntryKind.Gap, Lane = 1, ZOffset = 12f });
                p.AddCoinLine(0, 3f, 4); p.AddCoinLine(2, 11f, 4);
                p.Length = 18f;
            });
            AssetDatabase.SaveAssets();
            Debug.Log("Ink Run: default patterns created in " + Folder);
        }

        static void Make(string name, float weight, float minDistance, System.Action<PatternDefinition> fill)
        {
            string path = Folder + "/" + name + ".asset";
            var p = AssetDatabase.LoadAssetAtPath<PatternDefinition>(path);
            if (p == null) { p = ScriptableObject.CreateInstance<PatternDefinition>(); AssetDatabase.CreateAsset(p, path); }
            p.Entries.Clear(); p.Weight = weight; p.MinDistance = minDistance;
            fill(p);
            EditorUtility.SetDirty(p);
        }
    }
}
#endif
