using System.Collections.Generic;
using UnityEngine;
using GeniesGambit.Core;
using GeniesGambit.Level;

namespace GeniesGambit.Genie
{
    public class GenieManager : MonoBehaviour
    {
        public static GenieManager Instance { get; private set; }

        [Header("Wish Pool (drag all 6 WishData assets here)")]
        [SerializeField] List<WishData> allWishes;

        [Header("Rules")]
        [SerializeField] int wishesPerRound = 3;
        [SerializeField] int wishPickCount = 1;

        [Header("Scene References")]
        [SerializeField] WishTileMap wishTileMap;
        [SerializeField] WishPanel wishPanelUI;

        readonly List<WishData> _offeredWishes = new();
        readonly List<WishData> _chosenWishesThisRound = new();
        readonly List<WishData> _allChosenWishesEver = new();

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        void OnEnable() => GameManager.OnStateChanged += HandleStateChange;
        void OnDisable() => GameManager.OnStateChanged -= HandleStateChange;

        void HandleStateChange(GameState old, GameState newState)
        {
            if (newState == GameState.GenieWishScreen)
                BeginWishSelection();
        }

        void BeginWishSelection()
        {
            _offeredWishes.Clear();
            _chosenWishesThisRound.Clear();

            var availableWishes = new List<WishData>();
            foreach (var wish in allWishes)
            {
                if (!_allChosenWishesEver.Contains(wish))
                    availableWishes.Add(wish);
            }

            if (availableWishes.Count == 0)
            {
                Debug.Log("[Genie] All wishes have been used! Skipping wish screen.");
                // Call OnWishApplied so StartNewRound() is invoked from GenieWishScreen
                // state — ensuring the HeroTurn transition event fires correctly for
                // all listeners (input, coins, flag, etc.).
                if (RoundManager.Instance != null)
                    RoundManager.Instance.OnWishApplied();
                else if (GameManager.Instance != null)
                    GameManager.Instance.SetState(GameState.HeroTurn);
                return;
            }

            var shuffled = new List<WishData>(availableWishes);
            for (int i = shuffled.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
            }

            int wishesToShow = Mathf.Min(wishesPerRound, shuffled.Count);
            for (int i = 0; i < wishesToShow; i++)
                _offeredWishes.Add(shuffled[i]);

            wishPanelUI.ShowWishes(_offeredWishes);
        }

        public void OnWishChosen(WishData wish)
        {
            if (_chosenWishesThisRound.Contains(wish)) return;
            _chosenWishesThisRound.Add(wish);
            _allChosenWishesEver.Add(wish);

            Debug.Log($"[Genie] {wish.wishNameEnglish} chosen ({_chosenWishesThisRound.Count}/{wishPickCount})");

            if (_chosenWishesThisRound.Count >= wishPickCount)
                ConfirmAllWishes();
        }

        void ConfirmAllWishes()
        {
            foreach (var wish in _chosenWishesThisRound)
                ApplyWish(wish);

            wishPanelUI.Hide();

            if (RoundManager.Instance != null)
            {
                RoundManager.Instance.OnWishApplied();
            }
            else
            {
                Debug.LogWarning("[GenieManager] No RoundManager found! Falling back to HeroTurn.");
                GameManager.Instance.SetState(GameState.HeroTurn);
            }
        }

        void ApplyWish(WishData wish)
        {
            if (wish.wishType == WishType.Key)
            {
                KeyWishEffect.ApplyKeyWish();
                return;
            }

            if (wish.wishType == WishType.FlyingCarpet)
            {
                // Logic for flying carpet (placeholder for now)
                Debug.Log("[Genie] Spawning Flying Carpet!");
            }

            if (wish.swapsTiles)
            {
                var cells = GetWishCells(wish.wishType);
                wishTileMap.ApplyWish(wish.wishType, cells);
            }
            if (wish.spawnPrefab != null)
                Instantiate(wish.spawnPrefab, GetWishSpawnPoint(wish.wishType), Quaternion.identity);
        }

        // ─── Platform world positions (read from SampleScene.unity) ──────────────
        //   TM_Platform1  bottom / flag platform   cells Y=-1  →  world Y ≈  0.68
        //   TM_Platform2  main left platform        cells Y= 2  →  world Y ≈  2.93
        //   TM_Platform3  small top-right platform  cells Y= 3  →  world Y ≈  3.678
        // ─────────────────────────────────────────────────────────────────────────

        List<Vector3> GetWishCells(WishType type)
        {
            return type switch
            {
                // Thorns – one spike on the main platform (hero) + one near the flag
                WishType.Thorns => new List<Vector3>
                {
                    new Vector3(-3f, 2.93f,      0),  // Platform2 (hero) – middle
                    new Vector3( 0f, 0.68f,      0)   // Platform1 (flag) – middle
                },

                // BrokenGround – gaps on Platform1 (bottom platform with flag)
                WishType.BrokenGround => new List<Vector3>
                {
                    new Vector3(-1.5f, -0.5f, 0),  // Platform1 tile 3 from left (cell x=-2, y=-1)
                    new Vector3( 1.5f, -0.5f, 0)   // Platform1 tile 6 from left (cell x=1, y=-1)
                },

                _ => new List<Vector3>()
            };
        }

        Vector3 GetWishSpawnPoint(WishType type)
        {
            return type switch
            {
                // Wife monster – right edge of Platform3 (enemy spawn zone)
                WishType.Wife => new Vector3(4f, 4.5f, 0f),

                // Flying carpet – hovers between Platform2 and Platform1
                WishType.FlyingCarpet => new Vector3(-2f, 1.8f, 0f),

                // Wisdom puzzle – above Platform1 near the flag
                WishType.Wisdom => new Vector3(0f, 1.5f, 0f),

                _ => Vector3.zero
            };
        }

        public int GetRemainingWishCount()
        {
            int remaining = 0;
            foreach (var wish in allWishes)
            {
                if (!_allChosenWishesEver.Contains(wish))
                    remaining++;
            }
            return remaining;
        }

        public void ResetAllWishes()
        {
            _allChosenWishesEver.Clear();
            _chosenWishesThisRound.Clear();
            _offeredWishes.Clear();
        }
    }
}