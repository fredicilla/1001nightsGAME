using UnityEngine;
using TMPro;
using GeniesGambit.Level;
using GeniesGambit.Core;
using System.Collections;

namespace GeniesGambit.UI
{
    public class CoinCounterUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] TextMeshProUGUI coinText;
        [SerializeField] CanvasGroup panelCanvasGroup;

        bool _coinWishActive = false;

        void OnEnable()
        {
            GameManager.OnStateChanged += HandleStateChange;
            HidePanel();
        }

        void OnDisable()
        {
            GameManager.OnStateChanged -= HandleStateChange;
        }

        void HandleStateChange(GameState oldState, GameState newState)
        {
            if (newState == GameState.GenieWishScreen)
            {
                _coinWishActive = false;
                HidePanel();
            }
            else if (newState == GameState.HeroTurn)
            {
                StartCoroutine(CheckForCoinSpawner());
            }
        }

        IEnumerator CheckForCoinSpawner()
        {
            yield return null;
            
            var coinSpawner = FindFirstObjectByType<CoinSpawner>();
            Debug.Log($"[CoinCounterUI] Checking for CoinSpawner: {(coinSpawner != null ? "FOUND" : "NOT FOUND")}");
            
            if (coinSpawner != null)
            {
                _coinWishActive = true;
                ShowPanel();
                Debug.Log("[CoinCounterUI] Showing coin counter panel");
            }
            else
            {
                _coinWishActive = false;
                HidePanel();
                Debug.Log("[CoinCounterUI] Hiding coin counter panel (no spawner found)");
            }
        }

        void ShowPanel()
        {
            if (panelCanvasGroup != null)
            {
                panelCanvasGroup.alpha = 1f;
                panelCanvasGroup.interactable = true;
                panelCanvasGroup.blocksRaycasts = true;
            }
        }

        void HidePanel()
        {
            if (panelCanvasGroup != null)
            {
                panelCanvasGroup.alpha = 0f;
                panelCanvasGroup.interactable = false;
                panelCanvasGroup.blocksRaycasts = false;
            }
        }

        void Update()
        {
            if (coinText != null)
                coinText.text = $"Coins: {CoinCollectible.CoinsCollected}";
        }
    }
}
