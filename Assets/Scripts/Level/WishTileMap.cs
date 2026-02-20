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

        [Header("Ground Tilemap for Breaking")]
        [SerializeField] Tilemap groundTilemap;

        Tilemap _tilemap;
        readonly Dictionary<Vector3Int, TileBase> _snapshot = new();
        readonly Dictionary<Vector3Int, TileBase> _groundSnapshot = new();

        void Awake() => _tilemap = GetComponent<Tilemap>();

        public void ApplyWish(WishType wishType, IEnumerable<Vector3> worldPositions)
        {
            if (wishType == WishType.BrokenGround && groundTilemap != null)
            {
                foreach (var worldPos in worldPositions)
                {
                    Vector3Int cell = groundTilemap.WorldToCell(worldPos);
                    if (!_groundSnapshot.ContainsKey(cell))
                        _groundSnapshot[cell] = groundTilemap.GetTile(cell);

                    groundTilemap.SetTile(cell, null);
                }
                Debug.Log("[WishTileMap] Broke ground into platforms - created gaps!");
                return;
            }

            foreach (var worldPos in worldPositions)
            {
                Vector3Int cell = _tilemap.WorldToCell(worldPos);
                if (!_snapshot.ContainsKey(cell))
                    _snapshot[cell] = _tilemap.GetTile(cell);

                TileBase newTile = wishType switch
                {
                    WishType.Thorns => thornTile,
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

            foreach (var (cell, tile) in _groundSnapshot)
                groundTilemap.SetTile(cell, tile);
            _groundSnapshot.Clear();
        }
    }
}