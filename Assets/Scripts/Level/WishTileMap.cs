using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using GeniesGambit.Genie;

namespace GeniesGambit.Level
{
    [RequireComponent(typeof(Tilemap))]
    public class WishTileMap : MonoBehaviour
    {
        [Header("Tile References")]
        [SerializeField] TileBase thornTile;
        [SerializeField] TileBase coinTile;
        [SerializeField] TileBase brokenPlatTile;

        [Header("Ground Tilemaps for Breaking (Platform1, Platform2, etc.)")]
        [SerializeField] Tilemap[] groundTilemaps;

        Tilemap _tilemap;

        // For thorn/coin tiles on TM_Wishes
        readonly Dictionary<Vector3Int, TileBase> _snapshot = new();

        // For broken-ground: track which tilemap each removed tile came from
        readonly Dictionary<(Tilemap, Vector3Int), TileBase> _groundSnapshot = new();

        void Awake() => _tilemap = GetComponent<Tilemap>();

        public void ApplyWish(WishType wishType, IEnumerable<Vector3> worldPositions)
        {
            if (wishType == WishType.BrokenGround)
            {
                foreach (var worldPos in worldPositions)
                {
                    // Try each registered ground tilemap — remove from whichever has a tile there
                    foreach (var gt in groundTilemaps)
                    {
                        if (gt == null) continue;
                        Vector3Int cell = gt.WorldToCell(worldPos);
                        if (gt.GetTile(cell) != null)
                        {
                            var key = (gt, cell);
                            if (!_groundSnapshot.ContainsKey(key))
                                _groundSnapshot[key] = gt.GetTile(cell);
                            gt.SetTile(cell, null);
                        }
                    }
                }
                return;
            }

            foreach (var worldPos in worldPositions)
            {
                Vector3Int cell = _tilemap.WorldToCell(worldPos);
                if (!_snapshot.ContainsKey(cell))
                    _snapshot[cell] = _tilemap.GetTile(cell);

                TileBase newTile = wishType switch
                {
                    WishType.Thorns      => thornTile,
                    WishType.FallingCoins => coinTile,
                    _ => null
                };
                _tilemap.SetTile(cell, newTile);
            }
        }

        public void RevertAll()
        {
            foreach (var (cell, tile) in _snapshot)
                _tilemap.SetTile(cell, tile);
            _snapshot.Clear();

            foreach (var ((gt, cell), tile) in _groundSnapshot)
                if (gt != null) gt.SetTile(cell, tile);
            _groundSnapshot.Clear();
        }
    }
}
