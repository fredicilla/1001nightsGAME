using UnityEngine;
using GeniesGambit.Core;
using GeniesGambit.UI;
using GeniesGambit.Player;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GeniesGambit.Level
{
    public class CaptchaManager : MonoBehaviour
    {
        [Header("Captcha Settings")]
        [SerializeField] CaptchaData[] captchaPool;
        [SerializeField] float minTriggerDelay = 3f;
        [SerializeField] float maxTriggerDelay = 8f;
        [SerializeField] float timeSlowFactor = 0.3f;

        [Header("UI Reference")]
        [SerializeField] CaptchaUI captchaUI;

        CaptchaData _currentCaptcha;
        float _captchaTimer;
        bool _captchaActive;
        bool _waitingToTrigger = true;
        float _triggerCountdown;
        float _originalTimeScale = 1f;
        GameState _stateWhenStarted;

        static CaptchaManager _activeInstance;

        void Awake()
        {
            if (_activeInstance != null)
            {
                Debug.LogWarning("[CaptchaManager] Multiple captcha managers detected! Destroying duplicate.");
                Destroy(gameObject);
                return;
            }
            _activeInstance = this;
        }

        void OnEnable()
        {
            GameManager.OnStateChanged += HandleStateChange;
        }

        void OnDisable()
        {
            GameManager.OnStateChanged -= HandleStateChange;
        }

        void Start()
        {
            if (captchaUI != null)
                captchaUI.Hide();

            ScheduleNextCaptcha();
        }

        void HandleStateChange(GameState oldState, GameState newState)
        {
            if (newState == GameState.GenieWishScreen && _captchaActive)
            {
                PauseCaptcha();
            }
            else if ((newState == GameState.HeroTurn || newState == GameState.MonsterTurn) && _captchaActive)
            {
                ResumeCaptcha();
            }

            if (newState != GameState.HeroTurn && newState != GameState.MonsterTurn)
            {
                _waitingToTrigger = false;
            }
        }

        void ScheduleNextCaptcha()
        {
            _triggerCountdown = Random.Range(minTriggerDelay, maxTriggerDelay);
            _waitingToTrigger = true;
        }

        void Update()
        {
            if (!IsGameActive())
                return;

            if (_waitingToTrigger && !_captchaActive)
            {
                _triggerCountdown -= Time.deltaTime;
                if (_triggerCountdown <= 0f)
                {
                    StartCaptcha();
                }
            }

            if (_captchaActive)
            {
                _captchaTimer -= Time.unscaledDeltaTime;
                
                if (captchaUI != null)
                    captchaUI.UpdateTimer(_captchaTimer);

                if (_captchaTimer <= 0f)
                {
                    OnCaptchaTimeout();
                }
            }
        }

        void StartCaptcha()
        {
            if (captchaPool == null || captchaPool.Length == 0)
            {
                Debug.LogWarning("[CaptchaManager] No captcha data available!");
                return;
            }

            _currentCaptcha = captchaPool[Random.Range(0, captchaPool.Length)];
            _captchaTimer = _currentCaptcha.timeLimit;
            _captchaActive = true;
            _waitingToTrigger = false;

            if (GameManager.Instance != null)
                _stateWhenStarted = GameManager.Instance.CurrentState;

            _originalTimeScale = Time.timeScale;
            Time.timeScale = timeSlowFactor;

            int totalImages = _currentCaptcha.gridSize * _currentCaptcha.gridSize;
            Sprite[] displayImages = new Sprite[totalImages];
            System.Array.Copy(_currentCaptcha.allImages, displayImages, Mathf.Min(_currentCaptcha.allImages.Length, totalImages));

            if (captchaUI != null)
            {
                captchaUI.Initialize(_currentCaptcha.promptText, displayImages, _currentCaptcha.gridSize);
                captchaUI.OnSubmit = HandleCaptchaSubmit;
            }

            Debug.Log("[CaptchaManager] Captcha started!");
        }

        void HandleCaptchaSubmit(bool submitted)
        {
            if (!_captchaActive || _currentCaptcha == null)
                return;

            HashSet<int> selected = captchaUI.GetSelectedIndices();
            int[] selectedArray = selected.ToArray();

            bool correct = _currentCaptcha.ValidateAnswer(selectedArray);

            if (correct)
            {
                OnCaptchaSuccess();
            }
            else
            {
                OnCaptchaFailure();
            }
        }

        void OnCaptchaSuccess()
        {
            Debug.Log("[CaptchaManager] Captcha solved correctly!");
            
            EndCaptcha();
            ScheduleNextCaptcha();
        }

        void OnCaptchaFailure()
        {
            Debug.Log("[CaptchaManager] Captcha failed! Killing player...");
            
            EndCaptcha();
            KillCurrentPlayer();
        }

        void OnCaptchaTimeout()
        {
            Debug.Log("[CaptchaManager] Captcha timeout! Killing player...");
            
            EndCaptcha();
            KillCurrentPlayer();
        }

        void EndCaptcha()
        {
            _captchaActive = false;
            
            Time.timeScale = _originalTimeScale;

            if (captchaUI != null)
            {
                captchaUI.Hide();
                captchaUI.OnSubmit = null;
            }

            _currentCaptcha = null;
        }

        void PauseCaptcha()
        {
            Time.timeScale = 0f;
        }

        void ResumeCaptcha()
        {
            if (_captchaActive)
                Time.timeScale = timeSlowFactor;
        }

        void KillCurrentPlayer()
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj == null)
            {
                Debug.LogWarning("[CaptchaManager] No player found to kill!");
                return;
            }

            Rigidbody2D playerRb = playerObj.GetComponent<Rigidbody2D>();
            if (playerRb != null)
                playerRb.linearVelocity = Vector2.zero;

            playerObj.transform.position = new Vector3(-7.5f, 2.278f, 0);

            PlayerController playerController = playerObj.GetComponent<PlayerController>();
            if (playerController != null)
                playerController.ApplyWeightSlowdown(0f);

            KeyCollectible.ResetKey();
            CoinCollectible.ResetCoins();

            CoinSpawner coinSpawner = FindFirstObjectByType<CoinSpawner>();
            if (coinSpawner != null)
                coinSpawner.ResetAllCoins();

            Enemies.ChasingMonster ghost = FindFirstObjectByType<Enemies.ChasingMonster>();
            if (ghost != null)
                ghost.RespawnGhostPublic();
        }

        bool IsGameActive()
        {
            if (GameManager.Instance == null)
                return true;

            GameState state = GameManager.Instance.CurrentState;
            return state == GameState.HeroTurn || state == GameState.MonsterTurn;
        }

        void OnDestroy()
        {
            if (_activeInstance == this)
                _activeInstance = null;

            if (_captchaActive)
            {
                Time.timeScale = _originalTimeScale;
            }
        }
    }
}
