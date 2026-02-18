using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace GeniesGambit.Level
{
    [RequireComponent(typeof(Tilemap))]
    public class BreakablePlatform : MonoBehaviour
    {
        [Header("Breaking Settings")]
        [SerializeField] float breakDelay = 0.3f;
        [SerializeField] float respawnDelay = 3f;
        [SerializeField] bool respawnAfterBreak = true;

        [Header("Visual Feedback")]
        [SerializeField] TileBase crackedTile;
        [SerializeField] Color shakeColor = new Color(1f, 0.8f, 0.8f, 1f);

        Tilemap _tilemap;
        TilemapCollider2D _collider;
        readonly System.Collections.Generic.Dictionary<Vector3Int, TileBase> _brokenTiles = new();
        readonly System.Collections.Generic.HashSet<Vector3Int> _breakingTiles = new();

        void Awake()
        {
            _tilemap = GetComponent<Tilemap>();
            _collider = GetComponent<TilemapCollider2D>();
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            if (!collision.gameObject.CompareTag("Player")) return;

            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y < -0.5f)
                {
                    Vector3 hitPos = contact.point + contact.normal * 0.1f;
                    Vector3Int cellPos = _tilemap.WorldToCell(hitPos);

                    if (_tilemap.HasTile(cellPos) && !_breakingTiles.Contains(cellPos))
                    {
                        StartCoroutine(BreakTileRoutine(cellPos));
                    }
                }
            }
        }

        IEnumerator BreakTileRoutine(Vector3Int cellPos)
        {
            _breakingTiles.Add(cellPos);

            TileBase originalTile = _tilemap.GetTile(cellPos);
            if (originalTile == null)
            {
                _breakingTiles.Remove(cellPos);
                yield break;
            }

            Color originalColor = _tilemap.color;

            if (crackedTile != null)
            {
                _tilemap.SetTile(cellPos, crackedTile);
            }
            else
            {
                _tilemap.color = shakeColor;
            }

            yield return new WaitForSeconds(breakDelay);

            _brokenTiles[cellPos] = originalTile;
            _tilemap.SetTile(cellPos, null);
            _tilemap.color = originalColor;

            _breakingTiles.Remove(cellPos);

            if (respawnAfterBreak)
            {
                yield return new WaitForSeconds(respawnDelay);
                RespawnTile(cellPos);
            }
        }

        void RespawnTile(Vector3Int cellPos)
        {
            if (_brokenTiles.TryGetValue(cellPos, out TileBase originalTile))
            {
                _tilemap.SetTile(cellPos, originalTile);
                _brokenTiles.Remove(cellPos);
            }
        }

        public void MarkTilesAsBreakable(System.Collections.Generic.IEnumerable<Vector3Int> cells)
        {
            Debug.Log("[BreakablePlatform] Marked tiles as breakable");
        }

        public void ResetAllBrokenTiles()
        {
            foreach (var (cellPos, originalTile) in _brokenTiles)
            {
                _tilemap.SetTile(cellPos, originalTile);
            }
            _brokenTiles.Clear();
            _breakingTiles.Clear();
        }
    }
}
