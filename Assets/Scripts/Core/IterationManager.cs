using System.Collections.Generic;
using UnityEngine;
using GeniesGambit.Player;
using GeniesGambit.Enemies;
using GeniesGambit.Combat;

namespace GeniesGambit.Core
{
    public class IterationManager : MonoBehaviour
    {
        public static IterationManager Instance { get; private set; }

        [Header("Prefabs")]
        [SerializeField] GameObject heroPrefab;
        [SerializeField] GameObject enemyPrefab;
        
        [Header("Spawn Points")]
        [SerializeField] Transform heroSpawnPoint;
        [SerializeField] Transform enemySpawnPoint;

        GameObject _liveHero;
        GameObject _liveEnemy;
        GameObject _ghostHero;
        GameObject _ghostEnemy;
        
        MovementRecorder _heroRecorder;
        ProjectileShooter _heroShooter;
        
        List<FrameData> _iteration1HeroRecording;
        List<FrameData> _iteration2EnemyRecording;

        int _currentIteration = 1;
        bool _heroReachedFlagInIteration1 = false;
        bool _enemyKilledGhostInIteration2 = false;

        public int CurrentIteration => _currentIteration;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void BeginIterationCycle()
        {
            Debug.Log("[IterationManager] === BEGINNING NEW ITERATION CYCLE ===");
            _currentIteration = 0;
            _heroReachedFlagInIteration1 = false;
            _enemyKilledGhostInIteration2 = false;
            _iteration1HeroRecording = null;
            _iteration2EnemyRecording = null;
            
            StartIteration1();
        }

        void StartIteration1()
        {
            Debug.Log("=== ITERATION 1: You control HERO ===");
            _currentIteration = 1;
            _heroReachedFlagInIteration1 = false;

            CleanupGhosts();

            _liveHero = GameObject.FindGameObjectWithTag("Player");
            if (_liveHero == null)
            {
                Debug.LogError("[IterationManager] No Player GameObject found!");
                return;
            }

            _liveHero.SetActive(true);

            _heroRecorder = _liveHero.GetComponent<MovementRecorder>();
            if (_heroRecorder == null)
            {
                _heroRecorder = _liveHero.AddComponent<MovementRecorder>();
            }

            _heroShooter = _liveHero.GetComponent<ProjectileShooter>();
            if (_heroShooter == null)
            {
                Debug.LogWarning("[IterationManager] Hero has no ProjectileShooter component!");
            }

            _heroRecorder.StartRecording();
            if (_heroShooter != null)
            {
                _heroShooter.StartRecording();
            }

            if (_liveEnemy != null)
            {
                Destroy(_liveEnemy);
                _liveEnemy = null;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetState(GameState.HeroTurn);
            }
        }

        public void OnHeroReachedFlag()
        {
            if (_currentIteration != 1)
            {
                Debug.LogWarning("[IterationManager] OnHeroReachedFlag called but not in Iteration 1!");
                return;
            }

            Debug.Log("[IterationManager] Hero reached flag in Iteration 1! Recording complete.");
            _heroReachedFlagInIteration1 = true;

            _heroRecorder.StopRecording();
            _iteration1HeroRecording = _heroRecorder.GetRecording();
            
            if (_heroShooter != null)
            {
                _heroShooter.StopRecording();
            }

            StartIteration2();
        }

        void StartIteration2()
        {
            Debug.Log("=== ITERATION 2: You control ENEMY ===");
            _currentIteration = 2;
            _enemyKilledGhostInIteration2 = false;

            _liveHero.SetActive(false);
            Debug.Log("[IterationManager] Hero deactivated");

            _ghostHero = Instantiate(heroPrefab, heroSpawnPoint.position, Quaternion.identity);
            _ghostHero.name = "GhostHero (Iteration 1 Replay)";
            _ghostHero.tag = "Player";

            Destroy(_ghostHero.GetComponent<PlayerController>());
            Destroy(_ghostHero.GetComponent<UnityEngine.InputSystem.PlayerInput>());
            Destroy(_ghostHero.GetComponent<MovementRecorder>());

            var ghostRb = _ghostHero.GetComponent<Rigidbody2D>();
            if (ghostRb != null)
            {
                ghostRb.bodyType = RigidbodyType2D.Kinematic;
            }

            var ghostHealth = _ghostHero.GetComponent<Health>();
            if (ghostHealth != null)
            {
                ghostHealth.OnDeath += OnGhostHeroDied;
            }

            var ghostReplay = _ghostHero.AddComponent<GhostReplay>();
            ghostReplay.StartPlayback(_iteration1HeroRecording);

            var ghostShooter = _ghostHero.GetComponent<ProjectileShooter>();
            if (ghostShooter != null && _heroShooter != null)
            {
                ghostShooter.StartReplay(_heroShooter.GetRecording());
            }

            var spriteRenderer = _ghostHero.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                var color = spriteRenderer.color;
                color.a = 0.5f;
                spriteRenderer.color = color;
            }

            if (enemyPrefab != null)
            {
                _liveEnemy = Instantiate(enemyPrefab, enemySpawnPoint.position, Quaternion.identity);
                _liveEnemy.name = "LiveEnemy (Player Controlled)";
                Debug.Log($"[IterationManager] Enemy spawned at {enemySpawnPoint.position}");
                
                var enemyInput = _liveEnemy.GetComponent<UnityEngine.InputSystem.PlayerInput>();
                if (enemyInput != null)
                {
                    enemyInput.enabled = true;
                    Debug.Log("[IterationManager] Enemy PlayerInput enabled");
                }
                else
                {
                    Debug.LogError("[IterationManager] Enemy has NO PlayerInput component!");
                }
                
                var enemyController = _liveEnemy.GetComponent<PlayerController>();
                if (enemyController != null)
                {
                    enemyController.enabled = true;
                    Debug.Log("[IterationManager] Enemy PlayerController enabled");
                }
                else
                {
                    Debug.LogWarning("[IterationManager] Enemy has no PlayerController!");
                }
                
                var enemyRecorder = _liveEnemy.GetComponent<MovementRecorder>();
                if (enemyRecorder == null)
                {
                    enemyRecorder = _liveEnemy.AddComponent<MovementRecorder>();
                }
                enemyRecorder.StartRecording();

                var enemyShooter = _liveEnemy.GetComponent<ProjectileShooter>();
                if (enemyShooter != null)
                {
                    enemyShooter.StartRecording();
                }
                
                Debug.Log($"[IterationManager] Enemy fully configured. Components: PlayerInput={enemyInput != null}, PlayerController={enemyController != null}");
            }
            else
            {
                Debug.LogError("[IterationManager] Enemy prefab is NULL!");
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetState(GameState.MonsterTurn);
            }
        }

        void OnGhostHeroDied()
        {
            if (_currentIteration != 2)
            {
                return;
            }

            Debug.Log("[IterationManager] Ghost hero died in Iteration 2!");
            _enemyKilledGhostInIteration2 = true;

            if (_liveEnemy != null)
            {
                var enemyRecorder = _liveEnemy.GetComponent<MovementRecorder>();
                if (enemyRecorder != null)
                {
                    enemyRecorder.StopRecording();
                    _iteration2EnemyRecording = enemyRecorder.GetRecording();
                }

                var enemyShooter = _liveEnemy.GetComponent<ProjectileShooter>();
                if (enemyShooter != null)
                {
                    enemyShooter.StopRecording();
                }
            }

            StartIteration3();
        }

        public void OnGhostReachedFlag()
        {
            if (_currentIteration == 2)
            {
                Debug.Log("[IterationManager] Ghost hero reached the flag in Iteration 2! Enemy failed to stop it.");
                Debug.Log("[IterationManager] Restarting iteration cycle...");
                ResetIterations();
            }
        }

        void StartIteration3()
        {
            Debug.Log("=== ITERATION 3: You control HERO (dodge ghost enemy) ===");
            _currentIteration = 3;

            if (_ghostHero != null)
            {
                Destroy(_ghostHero);
            }

            _liveHero.SetActive(true);

            if (heroSpawnPoint != null)
            {
                _liveHero.transform.position = heroSpawnPoint.position;
            }

            var heroHealth = _liveHero.GetComponent<Health>();
            if (heroHealth != null)
            {
                heroHealth.ResetHealth();
                heroHealth.OnDeath += OnHeroDiedInIteration3;
            }

            if (_liveEnemy != null && _iteration2EnemyRecording != null)
            {
                _ghostEnemy = _liveEnemy;
                _ghostEnemy.name = "GhostEnemy (Iteration 2 Replay)";
                _ghostEnemy.tag = "Enemy";

                Destroy(_ghostEnemy.GetComponent<PlayerController>());
                Destroy(_ghostEnemy.GetComponent<MovementRecorder>());

                var ghostEnemyRb = _ghostEnemy.GetComponent<Rigidbody2D>();
                if (ghostEnemyRb != null)
                {
                    ghostEnemyRb.bodyType = RigidbodyType2D.Kinematic;
                }

                var ghostReplay = _ghostEnemy.AddComponent<GhostReplay>();
                ghostReplay.StartPlayback(_iteration2EnemyRecording);

                var ghostShooter = _ghostEnemy.GetComponent<ProjectileShooter>();
                if (ghostShooter != null && _liveEnemy.GetComponent<ProjectileShooter>() != null)
                {
                    var enemyShooter = _liveEnemy.GetComponent<ProjectileShooter>();
                    ghostShooter.StartReplay(enemyShooter.GetRecording());
                }

                var spriteRenderer = _ghostEnemy.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    var color = spriteRenderer.color;
                    color.a = 0.5f;
                    spriteRenderer.color = color;
                }
                
                _liveEnemy = null;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetState(GameState.HeroTurn);
            }
        }

        void OnHeroDiedInIteration3()
        {
            Debug.Log("[IterationManager] Hero died in Iteration 3! Restarting iteration cycle...");
            ResetIterations();
        }

        public void OnHeroReachedFlagInIteration3()
        {
            if (_currentIteration != 3)
            {
                Debug.LogWarning("[IterationManager] OnHeroReachedFlagInIteration3 called but not in Iteration 3!");
                return;
            }

            Debug.Log("[IterationManager] 🎉 ITERATION CYCLE COMPLETE! Hero reached flag in Iteration 3!");

            if (RoundManager.Instance != null)
            {
                RoundManager.Instance.OnIterationCycleComplete();
            }
            else
            {
                Debug.LogWarning("[IterationManager] No RoundManager found! Falling back to LevelComplete.");
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.SetState(GameState.LevelComplete);
                }
            }
        }

        public void ResetIterations()
        {
            Debug.Log("[IterationManager] Resetting iteration cycle...");

            CleanupGhosts();

            _liveHero.SetActive(true);

            if (heroSpawnPoint != null)
            {
                _liveHero.transform.position = heroSpawnPoint.position;
            }

            var heroHealth = _liveHero.GetComponent<Health>();
            if (heroHealth != null)
            {
                heroHealth.ResetHealth();
            }

            if (_liveEnemy != null)
            {
                Destroy(_liveEnemy);
                _liveEnemy = null;
            }

            _iteration1HeroRecording = null;
            _iteration2EnemyRecording = null;

            StartIteration1();
        }

        void CleanupGhosts()
        {
            if (_ghostHero != null)
            {
                Destroy(_ghostHero);
                _ghostHero = null;
            }

            if (_ghostEnemy != null)
            {
                Destroy(_ghostEnemy);
                _ghostEnemy = null;
            }
        }

        void OnDestroy()
        {
            CleanupGhosts();
            
            if (_liveEnemy != null)
            {
                Destroy(_liveEnemy);
            }

            _iteration1HeroRecording?.Clear();
            _iteration2EnemyRecording?.Clear();
        }
    }
}
