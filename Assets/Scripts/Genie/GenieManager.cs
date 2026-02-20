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
        [SerializeField] int wishPickCount  = 1;

        [Header("Scene References")]
        [SerializeField] WishTileMap wishTileMap;
        [SerializeField] WishPanel   wishPanelUI;

        readonly List<WishData> _offeredWishes = new();
        readonly List<WishData> _chosenWishesThisRound  = new();
        readonly List<WishData> _allChosenWishesEver = new();

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        void OnEnable()  => GameManager.OnStateChanged += HandleStateChange;
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
            if (wish.swapsTiles)
            {
                var cells = GetWishCells(wish.wishType);
                wishTileMap.ApplyWish(wish.wishType, cells);
            }
            if (wish.spawnPrefab != null)
                Instantiate(wish.spawnPrefab, GetWishSpawnPoint(wish.wishType), Quaternion.identity);
        }

        List<Vector3Int> GetWishCells(WishType type)
        {
            return type switch
            {
                WishType.Thorns => new List<Vector3Int>
                {
                    new Vector3Int(-6, -1, 0),
                    new Vector3Int(-4, -1, 0),
                    new Vector3Int(2, -1, 0)
                },
                WishType.BrokenGround => new List<Vector3Int>
                {
                    new Vector3Int(-5, -1, 0),
                    new Vector3Int(-4, -1, 0),
                    new Vector3Int(-3, -1, 0),
                    new Vector3Int(-1, -1, 0),
                    new Vector3Int(0, -1, 0),
                    new Vector3Int(2, -1, 0),
                    new Vector3Int(3, -1, 0)
                },
                _ => new List<Vector3Int>()
            };
        }

        Vector3 GetWishSpawnPoint(WishType type)
        {
            return type switch
            {
                WishType.Wife => new Vector3(6.38f, 5f, 0f),
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