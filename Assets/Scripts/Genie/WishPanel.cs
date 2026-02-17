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

        readonly List<WishCardUI> _cards = new();

        public void ShowWishes(List<WishData> wishes)
        {
            foreach (var card in _cards) Destroy(card.gameObject);
            _cards.Clear();

            foreach (var wish in wishes)
            {
                var go   = Instantiate(wishCardPrefab, cardContainer);
                var card = go.GetComponent<WishCardUI>();
                card.Populate(wish);
                _cards.Add(card);
            }
            panelRoot.SetActive(true);
        }

        public void Hide() => panelRoot.SetActive(false);
    }
}