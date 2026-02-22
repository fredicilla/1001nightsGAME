using UnityEngine;
using UnityEngine.Tilemaps;
using GeniesGambit.Genie;
namespace GeniesGambit.Genie
{
    public enum WishType
    {
        BrokenGround,  // اراضي      — splits ground into platforms
        Thorns,        // العسل      — floor becomes spike/thorn surface
        Wife,          // زوجة       — spawns extra chasing monster
        Wisdom,        // حكمة       — triggers password puzzle
        FallingCoins,  // المال      — coins slow player down when collected
        Key,            // مفتاح      — locks gates, requires key collection
        FlyingCarpet    // بساط       — spawns a moving platform
    }

    [CreateAssetMenu(
        fileName = "WishData_New",
        menuName  = "GeniesGambit/Wish Data",
        order     = 1)]
    public class WishData : ScriptableObject
    {
        [Header("Identity")]
        public WishType  wishType;
        public string    wishNameArabic;
        public string    wishNameEnglish;
        public Sprite    wishIcon;

        [Header("Flavour")]
        [TextArea(2, 4)]
        public string    genieQuote;

        [Header("Effect Config")]
        public bool      affectsHero;
        public bool      affectsMonster;
        public GameObject spawnPrefab;

        [Header("Tile Swap (if tile-based)")]
        public bool      swapsTiles;
        public TileBase  replacementTile;
    }
}