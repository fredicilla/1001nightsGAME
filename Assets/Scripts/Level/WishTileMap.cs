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

        void Awake() => _tilemap = GetComponent<Tilemap>();

        public void ApplyWish(WishType wishType, IEnumerable<Vector3Int> targetCells)
        {
            if (wishType == WishType.BrokenGround && groundTilemap != null)
            {
                foreach (var cell in targetCells)
                {
                    groundTilemap.SetTile(cell, null);
                }
                Debug.Log("[WishTileMap] Broke ground - created platformer gaps!");
                return;
            }

            foreach (var cell in targetCells)
            {
                if (!_snapshot.ContainsKey(cell))
                    _snapshot[cell] = _tilemap.GetTile(cell);

                TileBase newTile = wishType switch
                {
                    WishType.Thorns       => thornTile,
                    WishType.FallingCoins => coinTile,
                    _                     => null
                };
                _tilemap.SetTile(cell, newTile);
            }
        }

        public void RevertAll()
        {
            foreach (var (cell, tile) in _snapshot)
                _tilemap.SetTile(cell, tile);
            _snapshot.Clear();
        }
    }
}