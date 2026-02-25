using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GeniesGambit.Genie;

namespace GeniesGambit.UI
{
    public class WishCardUI : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] Image iconImage;
        [SerializeField] Button cardButton;
        [SerializeField] Image selectedOverlay;

        WishData _data;
        bool _selected;

        public void Populate(WishData wish)
        {
            _data = wish;
            iconImage.sprite = wish.wishIcon;
            selectedOverlay.gameObject.SetActive(false);
            cardButton.onClick.AddListener(OnClicked);
        }

        void OnClicked()
        {
            if (_selected) return;
            _selected = true;
            selectedOverlay.gameObject.SetActive(true);
            GenieManager.Instance.OnWishChosen(_data);
        }

        void OnDestroy() => cardButton.onClick.RemoveAllListeners();
    }
}