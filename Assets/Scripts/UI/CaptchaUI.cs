using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace GeniesGambit.UI
{
    public class CaptchaUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] TextMeshProUGUI promptText;
        [SerializeField] TextMeshProUGUI timerText;
        [SerializeField] Button submitButton;
        [SerializeField] GameObject gridContainer;
        [SerializeField] GameObject captchaImagePrefab;
        [SerializeField] CanvasGroup canvasGroup;

        [Header("Visual Feedback")]
        [SerializeField] Color normalColor = Color.white;
        [SerializeField] Color selectedColor = new Color(0.3f, 0.8f, 1f, 1f);
        [SerializeField] Color hoverColor = new Color(0.9f, 0.9f, 0.9f, 1f);

        readonly List<CaptchaImageSlot> _imageSlots = new();
        readonly HashSet<int> _selectedIndices = new();

        public System.Action<bool> OnSubmit;

        void Awake()
        {
            if (submitButton != null)
                submitButton.onClick.AddListener(HandleSubmit);
        }

        public void Initialize(string prompt, Sprite[] images, int gridSize)
        {
            promptText.text = prompt;
            _selectedIndices.Clear();

            ClearGrid();

            GridLayoutGroup grid = gridContainer.GetComponent<GridLayoutGroup>();
            if (grid != null)
                grid.constraintCount = gridSize;

            for (int i = 0; i < images.Length; i++)
            {
                GameObject slotObj = Instantiate(captchaImagePrefab, gridContainer.transform);
                CaptchaImageSlot slot = slotObj.GetComponent<CaptchaImageSlot>();
                
                if (slot == null)
                    slot = slotObj.AddComponent<CaptchaImageSlot>();

                int index = i;
                slot.Initialize(images[i], index, normalColor, selectedColor, hoverColor);
                slot.OnClicked = () => ToggleSelection(index);
                
                _imageSlots.Add(slot);
            }

            Show();
        }

        void ToggleSelection(int index)
        {
            if (_selectedIndices.Contains(index))
            {
                _selectedIndices.Remove(index);
                _imageSlots[index].SetSelected(false);
            }
            else
            {
                _selectedIndices.Add(index);
                _imageSlots[index].SetSelected(true);
            }
        }

        void HandleSubmit()
        {
            OnSubmit?.Invoke(true);
        }

        public HashSet<int> GetSelectedIndices()
        {
            return new HashSet<int>(_selectedIndices);
        }

        public void UpdateTimer(float remainingTime)
        {
            if (timerText != null)
            {
                timerText.text = $"Time: {Mathf.CeilToInt(remainingTime)}s";
                
                if (remainingTime <= 5f)
                    timerText.color = Color.red;
                else
                    timerText.color = Color.white;
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
        }

        void ClearGrid()
        {
            foreach (var slot in _imageSlots)
            {
                if (slot != null)
                    Destroy(slot.gameObject);
            }
            _imageSlots.Clear();
        }

        void OnDestroy()
        {
            if (submitButton != null)
                submitButton.onClick.RemoveAllListeners();
        }
    }

    public class CaptchaImageSlot : MonoBehaviour
    {
        Image _image;
        Button _button;
        int _index;
        bool _isSelected;
        
        Color _normalColor;
        Color _selectedColor;
        Color _hoverColor;

        public System.Action OnClicked;

        public void Initialize(Sprite sprite, int index, Color normal, Color selected, Color hover)
        {
            _index = index;
            _normalColor = normal;
            _selectedColor = selected;
            _hoverColor = hover;

            _image = GetComponent<Image>();
            if (_image == null)
                _image = gameObject.AddComponent<Image>();
            
            _image.sprite = sprite;
            _image.color = _normalColor;

            _button = GetComponent<Button>();
            if (_button == null)
                _button = gameObject.AddComponent<Button>();

            _button.onClick.AddListener(HandleClick);
        }

        void HandleClick()
        {
            OnClicked?.Invoke();
        }

        public void SetSelected(bool selected)
        {
            _isSelected = selected;
            _image.color = _isSelected ? _selectedColor : _normalColor;
        }

        void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveAllListeners();
        }
    }
}
