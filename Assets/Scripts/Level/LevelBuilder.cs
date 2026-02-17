using UnityEngine;
using UnityEngine.Tilemaps;

namespace GeniesGambit.Level
{
    [RequireComponent(typeof(Tilemap))]
    public class LevelBuilder : MonoBehaviour
    {
        [Header("Ground Tile")]
        [SerializeField] TileBase groundTile;

        Tilemap _tilemap;

        void Awake()
        {
            _tilemap = GetComponent<Tilemap>();
            BuildInitialGround();
        }

        void BuildInitialGround()
        {
            for (int x = -8; x <= 6; x++)
            {
                _tilemap.SetTile(new Vector3Int(x, -1, 0), groundTile);
            }
            Debug.Log("[LevelBuilder] Created flat ground from x=-8 to x=6");
        }
    }
}
