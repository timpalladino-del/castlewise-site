using System.Collections.Generic;
using UnityEngine;
using InkRun.Core;

namespace InkRun.World
{
    /// <summary>
    /// Spawns patterns ahead of the hero and the scrolling track and building pieces.
    /// Objects are parented to WorldMover so they scroll. Destroyed when far behind.
    /// Pooling can be added later behind Spawn()/Despawn() without touching callers.
    /// </summary>
    public class TrackSpawner : MonoBehaviour
    {
        [Header("Wiring")]
        public WorldMover World;
        public AreaController Areas;

        [Header("Prefabs by kind")]
        public GameObject BarrierPrefab, SignPrefab, WallPrefab, GapPrefab, CoinPrefab, MagnetPrefab, DashPrefab;

        [Header("Patterns")]
        public List<PatternDefinition> Patterns = new List<PatternDefinition>();

        [Header("Track pieces")]
        public GameObject TrackPiecePrefab;      // one rooftop segment, TrackPieceLength deep
        public float TrackPieceLength = 10f;

        Tuning T => GameManager.Instance.Tuning;
        float _untilNextPattern;
        float _nextTrackAhead;      // meters ahead of the hero where the next track piece starts
        readonly List<GameObject> _live = new List<GameObject>();
        /// <summary>Everything spawned and not yet despawned (track pieces, obstacles, pickups). Read-only; for the debug AutoPilot.</summary>
        public IReadOnlyList<GameObject> Live => _live;

        public void ResetTrack()
        {
            foreach (var go in _live) if (go != null) Destroy(go);
            _live.Clear();
            World.ResetWorld();
            _untilNextPattern = 25f;
            _nextTrackAhead = -TrackPieceLength * 2f;
            FillTrack();
        }

        public void Tick(float moved, float gap)
        {
            _untilNextPattern -= moved;
            _nextTrackAhead -= moved;
            if (_untilNextPattern <= 0f)
            {
                var p = Pick(GameManager.Instance.Run.Distance);
                SpawnPattern(p);
                // the gap is measured from the end of this pattern, so long patterns never overlap the next one
                _untilNextPattern = (p != null ? p.Length : 0f) + gap + Random.Range(0f, 4f);
            }
            FillTrack();
            Despawn();
        }

        void FillTrack()
        {
            if (TrackPiecePrefab == null) return;
            float needed = T.SpawnDistance + TrackPieceLength * 2f;
            while (_nextTrackAhead < needed)
            {
                var piece = Spawn(TrackPiecePrefab, 1, _nextTrackAhead, 0f);
                Areas.DressTrackPiece(piece);
                _nextTrackAhead += TrackPieceLength;
            }
        }

        PatternDefinition Pick(float distance)
        {
            float total = 0f;
            foreach (var p in Patterns) if (distance >= p.MinDistance) total += p.Weight;
            float r = Random.value * total;
            foreach (var p in Patterns)
            {
                if (distance < p.MinDistance) continue;
                r -= p.Weight;
                if (r <= 0f) return p;
            }
            return Patterns.Count > 0 ? Patterns[0] : null;
        }

        void SpawnPattern(PatternDefinition p)
        {
            if (p == null) return;
            float startZ = T.SpawnDistance;
            foreach (var e in p.Entries)
            {
                var prefab = PrefabFor(e.Kind);
                if (prefab == null) continue;
                Spawn(prefab, e.Lane, startZ + e.ZOffset, e.Height);
            }
        }

        GameObject Spawn(GameObject prefab, int lane, float worldZ, float height)
        {
            var go = Instantiate(prefab, World.transform);
            go.transform.position = new Vector3((lane - 1) * T.LaneWidth, height, worldZ);
            _live.Add(go);
            return go;
        }

        void Despawn()
        {
            for (int i = _live.Count - 1; i >= 0; i--)
            {
                var go = _live[i];
                if (go == null) { _live.RemoveAt(i); continue; }
                if (go.transform.position.z < -T.DespawnBehind) { Destroy(go); _live.RemoveAt(i); }
            }
        }

        /// <summary>After a revive: remove obstacles within the next N meters so the hero gets a clean restart.</summary>
        public void ClearObstaclesAhead(float meters)
        {
            foreach (var go in _live)
            {
                if (go == null) continue;
                if (go.GetComponent<Obstacle>() != null && go.transform.position.z < meters) go.SetActive(false);
            }
        }

        GameObject PrefabFor(EntryKind k)
        {
            switch (k)
            {
                case EntryKind.Barrier: return BarrierPrefab;
                case EntryKind.Sign: return SignPrefab;
                case EntryKind.Wall: return WallPrefab;
                case EntryKind.Gap: return GapPrefab;
                case EntryKind.Coin: return CoinPrefab;
                case EntryKind.Magnet: return MagnetPrefab;
                case EntryKind.Dash: return DashPrefab;
            }
            return null;
        }

    }
}
