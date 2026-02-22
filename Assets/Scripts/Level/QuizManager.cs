using UnityEngine;
using GeniesGambit.Core;
using GeniesGambit.UI;
using GeniesGambit.Player;
using GeniesGambit.Combat;

namespace GeniesGambit.Level
{
    public class QuizManager : MonoBehaviour
    {
        [Header("UI Reference")]
        [SerializeField] QuizUI quizUI;

        [Header("Settings")]
        [SerializeField] bool showAtStartOfEachRound = true;

        bool _quizActive = false;
        bool _quizShownThisRound = false;
        int _currentTrackedRound = -1;
        PlayerController _playerController;
        ProjectileShooter _projectileShooter;
        static QuizManager _activeInstance;

        void Awake()
        {
            if (_activeInstance != null)
            {
                Debug.LogWarning("[QuizManager] Duplicate quiz manager! Destroying.");
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
            if (quizUI == null)
            {
                Debug.LogError("[QuizManager] QuizUI reference missing!");
                return;
            }

            quizUI.OnCorrectAnswer = OnCorrectAnswer;
            quizUI.OnWrongAnswer = OnWrongAnswer;
            
            FindPlayerComponents();
            CheckAndShowQuiz();
        }

        void HandleStateChange(GameState oldState, GameState newState)
        {
            Debug.Log($"[QuizManager] State change: {oldState} → {newState}");

            if (newState == GameState.HeroTurn)
            {
                FindPlayerComponents();
                CheckAndShowQuiz();
            }
        }

        void CheckAndShowQuiz()
        {
            if (!showAtStartOfEachRound)
                return;

            if (RoundManager.Instance == null)
            {
                Debug.LogWarning("[QuizManager] No RoundManager found!");
                return;
            }

            int currentRound = RoundManager.Instance.CurrentRound;

            if (_currentTrackedRound != currentRound)
            {
                Debug.Log($"[QuizManager] New round detected! Previous: {_currentTrackedRound}, Current: {currentRound}");
                _currentTrackedRound = currentRound;
                _quizShownThisRound = false;
            }

            if (_quizShownThisRound)
            {
                Debug.Log($"[QuizManager] Quiz already shown for Round {currentRound}, skipping.");
                return;
            }

            _quizShownThisRound = true;
            ShowQuiz();
        }

        void FindPlayerComponents()
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                _playerController = playerObj.GetComponent<PlayerController>();
                _projectileShooter = playerObj.GetComponent<ProjectileShooter>();
            }
        }

        void ShowQuiz()
        {
            if (_quizActive) return;

            _quizActive = true;
            
            if (quizUI != null)
                quizUI.Show();

            DisablePlayerMovement();

            Debug.Log("[QuizManager] Quiz shown - player control disabled!");
        }

        void OnCorrectAnswer()
        {
            Debug.Log("[QuizManager] Correct answer! Closing quiz.");
            CloseQuiz();
        }

        void OnWrongAnswer()
        {
            Debug.Log("[QuizManager] Wrong answer! Quiz stays open - choose again.");
        }

        void CloseQuiz()
        {
            _quizActive = false;

            if (quizUI != null)
                quizUI.Hide();

            EnablePlayerMovement();
        }

        void DisablePlayerMovement()
        {
            if (_playerController != null)
                _playerController.enabled = false;

            if (_projectileShooter != null)
                _projectileShooter.enabled = false;
        }

        void EnablePlayerMovement()
        {
            if (_playerController != null)
                _playerController.enabled = true;

            if (_projectileShooter != null)
                _projectileShooter.enabled = true;
        }

        void OnDestroy()
        {
            if (_activeInstance == this)
                _activeInstance = null;

            EnablePlayerMovement();
        }
    }
}
