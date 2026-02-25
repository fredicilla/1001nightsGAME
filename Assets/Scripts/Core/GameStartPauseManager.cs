using UnityEngine;
using UnityEngine.InputSystem;

namespace GeniesGambit.Core
{
    public class GameStartPauseManager : MonoBehaviour
    {
        public static GameStartPauseManager Instance { get; private set; }
        public GameObject overlayUI; // Assign a UI Canvas with a "Press any key to start" message
        private bool _waitingForInput = true;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            PauseGameAndShowOverlay();
        }

        void Update()
        {
            if (!_waitingForInput) return;
            if (Keyboard.current.anyKey.wasPressedThisFrame || Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame)
            {
                ResumeGameAndHideOverlay();
            }
        }

        public void PauseGameAndShowOverlay()
        {
            Time.timeScale = 0f;
            if (overlayUI != null) overlayUI.SetActive(true);
            _waitingForInput = true;
        }

        public void ResumeGameAndHideOverlay()
        {
            Time.timeScale = 1f;
            if (overlayUI != null) overlayUI.SetActive(false);
            _waitingForInput = false;
        }

        public void TriggerPauseForNextRound()
        {
            PauseGameAndShowOverlay();
        }
    }
}
