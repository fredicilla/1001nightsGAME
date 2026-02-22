using System.Collections.Generic;
using UnityEngine;
using GeniesGambit.UI;

namespace GeniesGambit.Genie
{
    public class WishPanel : MonoBehaviour
    {
        [SerializeField] GameObject  wishCardPrefab;
        [SerializeField] Transform   cardContainer;
        [SerializeField] GameObject  panelRoot;
        [Header("Visuals")]
        [SerializeField] UnityEngine.UI.Image genieDisplayHolder;
        [SerializeField] Sprite               genieSprite;

        readonly List<WishCardUI> _cards = new();

        public void ShowWishes(List<WishData> wishes)
        {
            foreach (var card in _cards) Destroy(card.gameObject);
            _cards.Clear();

            float[] xOffsets = { -60f, 0f, 60f };

            for (int i = 0; i < wishes.Count; i++)
            {
                var go   = Instantiate(wishCardPrefab, cardContainer);
                var card = go.GetComponent<WishCardUI>();
                card.Populate(wishes[i]);
                _cards.Add(card);

                // Apply X offset based on position (left / center / right)
                if (i < xOffsets.Length)
                {
                    var rt = go.GetComponent<RectTransform>();
                    var pos = rt.anchoredPosition;
                    pos.x += xOffsets[i];
                    rt.anchoredPosition = pos;
                }
            }
            if (genieDisplayHolder != null && genieSprite != null)
                genieDisplayHolder.sprite = genieSprite;

            panelRoot.SetActive(true);
        }

        public void Hide() => panelRoot.SetActive(false);
    }
}