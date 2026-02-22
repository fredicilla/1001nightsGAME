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
        [SerializeField] Transform enemy1SpawnPoint;
        [SerializeField] Transform enemy2SpawnPoint;

        IterationTimer _iterationTimer;

        GameObject _liveHero;
        GameObject _liveEnemy1;
        GameObject _liveEnemy2;
        GameObject _ghostHero;
        GameObject _ghostEnemy1;
        GameObject _ghostEnemy2;

        MovementRecorder _heroRecorder;
        ProjectileShooter _heroShooter;

        List<FrameData> _iteration1HeroRecording;
        List<ShootEvent> _iteration1HeroShooterRecording;
        List<FrameData> _iteration2EnemyRecording;
        List<ShootEvent> _iteration2EnemyShooterRecording;
        List<FrameData> _iteration3HeroRecording;
        List<ShootEvent> _iteration3HeroShooterRecording;
        List<FrameData> _iteration4Enemy2Recording;
        List<ShootEvent> _iteration4Enemy2ShooterRecording;

        int _currentIteration = 1;
        int _totalIterations = 3;
        bool _heroReachedFlagInIteration1 = false;
        bool _enemyKilledGhostInIteration2 = false;
        bool _heroReachedFlagInIteration3 = false;
        bool _enemy2KilledGhostInIteration4 = false;

        List<IterationState> _iterationSnapshots = new List<IterationState>();

        public int CurrentIteration => _currentIteration;
        public int TotalIterations => _totalIterations;

        public bool CanRewind => _currentIteration > 1;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _iterationTimer = GetComponent<IterationTimer>();
            if (_iterationTimer != null)
            {
                _iterationTimer.OnTimerExpired.AddListener(OnIterationTimerExpired);
            }
        }

        public void BeginIterationCycle(int iterationCount)
        {
            Debug.Log($"[IterationManager] === BEGINNING NEW ITERATION CYCLE ({iterationCount} iterations) ===");
            _currentIteration = 0;
            _totalIterations = iterationCount;
            _heroReachedFlagInIteration1 = false;
            _enemyKilledGhostInIteration2 = false;
            _heroReachedFlagInIteration3 = false;
            _enemy2KilledGhostInIteration4 = false;
            _iteration1HeroRecording = null;
            _iteration1HeroShooterRecording = null;
            _iteration2EnemyRecording = null;
            _iteration2EnemyShooterRecording = null;
            _iteration3HeroRecording = null;
            _iteration3HeroShooterRecording = null;
            _iteration4Enemy2Recording = null;
            _iteration4Enemy2ShooterRecording = null;
            ClearIterationHistory();

            StartIteration1();
        }

        void ClearIterationHistory()
        {
            _iterationSnapshots.Clear();
            Debug.Log("[IterationManager] Iteration history cleared");
        }

        void SaveIterationSnapshot()
        {
            Vector3 heroPos = _liveHero != null ? _liveHero.transform.position : Vector3.zero;
            Vector3 enemy1Pos = _liveEnemy1 != null ? _liveEnemy1.transform.position : Vector3.zero;

            var snapshot = new IterationState(
                _currentIteration,
                heroPos,
                enemy1Pos,
                _iteration1HeroRecording,
                _iteration1HeroShooterRecording,
                _iteration2EnemyRecording,
                _iteration2EnemyShooterRecording,
                _heroReachedFlagInIteration1,
                _enemyKilledGhostInIteration2
            );

            _iterationSnapshots.Add(snapshot);
            Debug.Log($"[IterationManager] Saved snapshot for Iteration {_currentIteration}. Total snapshots: {_iterationSnapshots.Count}");
        }

        public void RestartCurrentIteration()
        {
            Debug.Log($"[IterationManager] === RESTARTING ITERATION {_currentIteration} ===");

            if (_iterationTimer != null)
            {
                _iterationTimer.StopTimer();
            }

            switch (_currentIteration)
            {
                case 1:
                    StartIteration1();
                    break;
                case 2:
                    RestartIteration2();
                    break;
                case 3:
                    RestartIteration3();
                    break;
                case 4:
                    RestartIteration4();
                    break;
                case 5:
                    RestartIteration5();
                    break;
                default:
                    Debug.LogWarning($"[IterationManager] Cannot restart Iteration {_currentIteration}");
                    break;
            }
        }

        public void RewindToIteration(int targetIteration)
        {
            if (targetIteration >= _currentIteration)
            {
                Debug.LogWarning($"[IterationManager] Cannot rewind to Iteration {targetIteration} from Iteration {_currentIteration}");
                return;
            }

            if (targetIteration < 1 || targetIteration > _totalIterations)
            {
                Debug.LogError($"[IterationManager] Invalid target iteration: {targetIteration}");
                return;
            }

            Debug.Log($"[IterationManager] === REWINDING FROM ITERATION {_currentIteration} TO ITERATION {targetIteration} ===");

            var iterationUI = FindFirstObjectByType<GeniesGambit.UI.IterationUI>();
            if (iterationUI != null)
            {
                iterationUI.ShowRewindBanner(targetIteration);
            }

            if (_iterationTimer != null)
            {
                _iterationTimer.StopTimer();
            }

            CleanupGhosts();

            if (_liveEnemy1 != null)
            {
                _liveEnemy1.SetActive(false);
                Destroy(_liveEnemy1);
                _liveEnemy1 = null;
            }

            if (_liveEnemy2 != null)
            {
                _liveEnemy2.SetActive(false);
                Destroy(_liveEnemy2);
                _liveEnemy2 = null;
            }

            while (_iterationSnapshots.Count > targetIteration - 1)
            {
                _iterationSnapshots.RemoveAt(_iterationSnapshots.Count - 1);
            }

            if (targetIteration == 1)
            {
                _iteration1HeroRecording = null;
                _iteration1HeroShooterRecording = null;
                _iteration2EnemyRecording = null;
                _iteration2EnemyShooterRecording = null;
                _heroReachedFlagInIteration1 = false;
                _enemyKilledGhostInIteration2 = false;
                StartIteration1();
            }
            else if (targetIteration == 2)
            {
                _iteration2EnemyRecording = null;
                _iteration2EnemyShooterRecording = null;
                _enemyKilledGhostInIteration2 = false;
                StartIteration2();
            }
        }

        void StartIteration1()
        {
            Debug.Log("=== ITERATION 1: You control HERO ===");
            _currentIteration = 1;
            _heroReachedFlagInIteration1 = false;

            CleanupGhosts();

            // Only search for hero if we don't have a valid reference
            // This prevents finding ghosts that haven't been destroyed yet
            if (_liveHero == null || !_liveHero.CompareTag("Player"))
            {
                _liveHero = GameObject.FindGameObjectWithTag("Player");
            }

            if (_liveHero == null)
            {
                Debug.LogError("[IterationManager] No Player GameObject found!");
                return;
            }

            // Make sure we have the real hero, not a ghost
            if (_liveHero.name.Contains("Ghost"))
            {
                Debug.LogWarning("[IterationManager] Found ghost instead of real hero! Searching again...");
                var allPlayers = GameObject.FindGameObjectsWithTag("Player");
                foreach (var player in allPlayers)
                {
                    if (!player.name.Contains("Ghost"))
                    {
                        _liveHero = player;
                        break;
                    }
                }
            }

            _liveHero.SetActive(true);
            _liveHero.transform.localScale = new Vector3(0.15f, 0.15f, 1f);

            // Reset hero position to spawn point for new round
            if (heroSpawnPoint != null)
            {
                _liveHero.transform.position = heroSpawnPoint.position;
            }

            // Reset hero health for new round
            var heroHealth = _liveHero.GetComponent<Health>();
            if (heroHealth != null)
            {
                heroHealth.ResetHealth();
                // Subscribe to death in Iteration 1 to restart recording
                heroHealth.OnDeath -= OnHeroDiedInIteration1;
                heroHealth.OnDeath += OnHeroDiedInIteration1;
            }

            // Reset collectibles for new round
            GeniesGambit.Level.KeyCollectible.ResetKey();
            GeniesGambit.Level.CoinCollectible.ResetCoins();

            // Ensure hero's input is properly enabled
            var heroInput = _liveHero.GetComponent<UnityEngine.InputSystem.PlayerInput>();
            if (heroInput != null)
            {
                heroInput.enabled = true;
                heroInput.ActivateInput();
                Debug.Log("[IterationManager] Hero PlayerInput re-enabled and activated");
            }

            var heroController = _liveHero.GetComponent<PlayerController>();
            if (heroController != null)
            {
                heroController.enabled = true;
            }

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
                _heroShooter.SetTargetTag("Enemy");
                _heroShooter.StartRecording();
            }

            if (_liveEnemy1 != null)
            {
                _liveEnemy1.SetActive(false);
                Destroy(_liveEnemy1);
                _liveEnemy1 = null;
            }

            if (_liveEnemy2 != null)
            {
                _liveEnemy2.SetActive(false);
                Destroy(_liveEnemy2);
                _liveEnemy2 = null;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetState(GameState.HeroTurn);
            }

            if (_iterationTimer != null)
            {
                _iterationTimer.StartTimer();
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

            if (_iterationTimer != null)
            {
                _iterationTimer.StopTimer();
            }

            _heroRecorder.StopRecording();
            _iteration1HeroRecording = _heroRecorder.GetRecording();

            if (_heroShooter != null)
            {
                _heroShooter.StopRecording();
                _iteration1HeroShooterRecording = _heroShooter.GetRecording();
                Debug.Log($"[IterationManager] Stored hero shot recording: {_iteration1HeroShooterRecording.Count} shots");
            }

            SaveIterationSnapshot();
            StartIteration2();
        }

        void OnHeroDiedInIteration1()
        {
            if (_currentIteration != 1) return;

            Debug.Log("[IterationManager] Hero died in Iteration 1! Clearing recording and restarting...");
            
            // Stop current recording
            if (_heroRecorder != null)
            {
                _heroRecorder.StopRecording();
            }
            if (_heroShooter != null)
            {
                _heroShooter.StopRecording();
            }

            // Wait a frame then restart recording fresh
            Invoke(nameof(RestartRecordingAfterDeath), 0.1f);
        }

        void RestartRecordingAfterDeath()
        {
            // Reactivate hero (it was deactivated by Health.Die())
            if (_liveHero != null)
            {
                _liveHero.SetActive(true);
            }

            // Reset hero to spawn point
            if (_liveHero != null && heroSpawnPoint != null)
            {
                _liveHero.transform.position = heroSpawnPoint.position;
            }

            // Reset hero velocity
            var heroRb = _liveHero.GetComponent<Rigidbody2D>();
            if (heroRb != null)
            {
                heroRb.linearVelocity = Vector2.zero;
            }

            // Reset health
            var heroHealth = _liveHero.GetComponent<Health>();
            if (heroHealth != null)
            {
                heroHealth.ResetHealth();
            }

            // Restart recording from scratch
            if (_heroRecorder != null)
            {
                _heroRecorder.StartRecording();
            }
            if (_heroShooter != null)
            {
                _heroShooter.StartRecording();
            }

            // Restart timer
            if (_iterationTimer != null)
            {
                _iterationTimer.StartTimer();
            }

            Debug.Log("[IterationManager] Recording restarted fresh after death");
        }

        void StartIteration2()
        {
            Debug.Log("=== ITERATION 2: You control ENEMY ===");
            _currentIteration = 2;
            _enemyKilledGhostInIteration2 = false;

            // Validate recordings exist
            if (_iteration1HeroRecording == null || _iteration1HeroRecording.Count == 0)
            {
                Debug.LogError("[IterationManager] ERROR: Hero movement recording is null or empty! Cannot replay.");
                ResetIterations();
                return;
            }
            Debug.Log($"[IterationManager] Hero recording valid: {_iteration1HeroRecording.Count} frames, {_iteration1HeroShooterRecording?.Count ?? 0} shots");

            // Unsubscribe death handler from Iteration 1
            var liveHeroHealth = _liveHero.GetComponent<Health>();
            if (liveHeroHealth != null)
            {
                liveHeroHealth.OnDeath -= OnHeroDiedInIteration1;
            }

            _liveHero.SetActive(false);
            Debug.Log("[IterationManager] Hero deactivated");

            _ghostHero = Instantiate(heroPrefab, heroSpawnPoint.position, Quaternion.identity);
            _ghostHero.name = "GhostHero (Iteration 1 Replay)";
            _ghostHero.tag = "Player"; // Must have Player tag so enemy projectiles can hit it
            _ghostHero.transform.localScale = new Vector3(0.15f, 0.15f, 1f);

            // Disable components immediately to prevent input conflicts, then Destroy
            var ghostPlayerController = _ghostHero.GetComponent<PlayerController>();
            if (ghostPlayerController != null)
            {
                ghostPlayerController.enabled = false;
                Destroy(ghostPlayerController);
            }

            var ghostPlayerInput = _ghostHero.GetComponent<UnityEngine.InputSystem.PlayerInput>();
            if (ghostPlayerInput != null)
            {
                ghostPlayerInput.enabled = false;
                Destroy(ghostPlayerInput);
            }

            var ghostRecorder = _ghostHero.GetComponent<MovementRecorder>();
            if (ghostRecorder != null)
            {
                ghostRecorder.enabled = false;
                Destroy(ghostRecorder);
            }

            var ghostRb = _ghostHero.GetComponent<Rigidbody2D>();
            if (ghostRb != null)
            {
                ghostRb.bodyType = RigidbodyType2D.Kinematic;
            }

            var ghostHealth = _ghostHero.GetComponent<Health>();
            if (ghostHealth != null)
            {
                ghostHealth.ResetHealth(); // Ensure ghost can take damage
                ghostHealth.OnDeath += OnGhostHeroDied;
                Debug.Log("[IterationManager] Ghost hero Health reset and OnDeath subscribed");
            }
            else
            {
                Debug.LogError("[IterationManager] Ghost hero has NO Health component! Cannot be killed!");
            }

            // Ensure ghost has a collider that can be hit
            var ghostCollider = _ghostHero.GetComponent<Collider2D>();
            if (ghostCollider != null)
            {
                ghostCollider.enabled = true;
                Debug.Log($"[IterationManager] Ghost collider enabled: {ghostCollider.GetType().Name}");
            }
            else
            {
                Debug.LogError("[IterationManager] Ghost hero has NO Collider2D! Cannot be hit!");
            }

            var ghostReplay = _ghostHero.AddComponent<GhostReplay>();
            ghostReplay.StartPlayback(_iteration1HeroRecording);

            var ghostShooter = _ghostHero.GetComponent<ProjectileShooter>();
            if (ghostShooter != null && _iteration1HeroShooterRecording != null)
            {
                Debug.Log($"[IterationManager] Ghost hero shooter found. Using stored shot recording: {_iteration1HeroShooterRecording.Count} shots");
                ghostShooter.SetTargetTag("Enemy"); // Ghost hero targets enemies
                ghostShooter.StartReplay(_iteration1HeroShooterRecording);
            }
            else
            {
                Debug.LogWarning($"[IterationManager] Ghost shooter setup failed! ghostShooter={ghostShooter != null}, hasShooterRecording={_iteration1HeroShooterRecording != null}");
            }

            // Ghost keeps the same color as the original player
            // var spriteRenderer = _ghostHero.GetComponent<SpriteRenderer>();
            // No color change needed - ghost uses original sprite colors

            if (enemyPrefab != null)
            {
                _liveEnemy1 = Instantiate(enemyPrefab, enemy1SpawnPoint.position, Quaternion.identity);
                _liveEnemy1.name = "LiveEnemy1 (Player Controlled)";
                _liveEnemy1.tag = "Enemy";
                _liveEnemy1.transform.localScale = new Vector3(0.15f, 0.15f, 1f);
                Debug.Log($"[IterationManager] Enemy #1 spawned at {enemy1SpawnPoint.position}");

                var enemyHealth = _liveEnemy1.GetComponent<Health>();
                if (enemyHealth == null)
                {
                    enemyHealth = _liveEnemy1.AddComponent<Health>();
                    Debug.Log("[IterationManager] Added Health component to enemy #1");
                }
                enemyHealth.ResetHealth();
                enemyHealth.OnDeath -= OnLiveEnemyDied;
                enemyHealth.OnDeath += OnLiveEnemyDied;
                Debug.Log("[IterationManager] Enemy #1 Health reset and OnDeath subscribed");

                var enemyCollider = _liveEnemy1.GetComponent<Collider2D>();
                if (enemyCollider != null)
                {
                    enemyCollider.enabled = true;
                    Debug.Log($"[IterationManager] Enemy #1 collider enabled: {enemyCollider.GetType().Name}");
                }
                else
                {
                    var capsuleCollider = _liveEnemy1.AddComponent<CapsuleCollider2D>();
                    capsuleCollider.size = new Vector2(1f, 2f);
                    Debug.Log("[IterationManager] Added CapsuleCollider2D to enemy #1");
                }

                var enemyInput = _liveEnemy1.GetComponent<UnityEngine.InputSystem.PlayerInput>();
                if (enemyInput != null)
                {
                    enemyInput.enabled = true;
                    Debug.Log("[IterationManager] Enemy #1 PlayerInput enabled");
                }
                else
                {
                    Debug.LogError("[IterationManager] Enemy #1 has NO PlayerInput component!");
                }

                var enemyController = _liveEnemy1.GetComponent<PlayerController>();
                if (enemyController != null)
                {
                    enemyController.enabled = true;
                    Debug.Log("[IterationManager] Enemy #1 PlayerController enabled");
                }
                else
                {
                    Debug.LogWarning("[IterationManager] Enemy #1 has no PlayerController!");
                }

                var enemyRecorder = _liveEnemy1.GetComponent<MovementRecorder>();
                if (enemyRecorder == null)
                {
                    enemyRecorder = _liveEnemy1.AddComponent<MovementRecorder>();
                }
                enemyRecorder.StartRecording();

                var enemyShooter = _liveEnemy1.GetComponent<ProjectileShooter>();
                if (enemyShooter != null)
                {
                    enemyShooter.SetTargetTag("Player");
                    enemyShooter.StartRecording();
                }

                Debug.Log($"[IterationManager] Enemy #1 fully configured. Components: PlayerInput={enemyInput != null}, PlayerController={enemyController != null}");
            }
            else
            {
                Debug.LogError("[IterationManager] Enemy prefab is NULL!");
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetState(GameState.MonsterTurn);
            }

            if (_iterationTimer != null)
            {
                _iterationTimer.StartTimer();
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

            if (_iterationTimer != null)
            {
                _iterationTimer.StopTimer();
            }

            if (_liveEnemy1 != null)
            {
                var enemyRecorder = _liveEnemy1.GetComponent<MovementRecorder>();
                if (enemyRecorder != null)
                {
                    enemyRecorder.StopRecording();
                    _iteration2EnemyRecording = enemyRecorder.GetRecording();
                    Debug.Log($"[IterationManager] Stored enemy #1 movement recording: {_iteration2EnemyRecording.Count} frames");
                }

                var enemyShooter = _liveEnemy1.GetComponent<ProjectileShooter>();
                if (enemyShooter != null)
                {
                    enemyShooter.StopRecording();
                    _iteration2EnemyShooterRecording = enemyShooter.GetRecording();
                    Debug.Log($"[IterationManager] Stored enemy #1 shooter recording: {_iteration2EnemyShooterRecording.Count} shots");
                }
            }

            SaveIterationSnapshot();
            StartIteration3();
        }

        void OnLiveEnemyDied()
        {
            if (_currentIteration != 2)
            {
                return;
            }

            Debug.Log("[IterationManager] Live enemy #1 died in Iteration 2! Restarting Iteration 2...");

            if (_liveEnemy1 != null)
            {
                var enemyHealth = _liveEnemy1.GetComponent<Health>();
                if (enemyHealth != null)
                {
                    enemyHealth.OnDeath -= OnLiveEnemyDied;
                }
            }

            Invoke(nameof(RestartIteration2), 0.1f);
        }

        public void OnGhostReachedFlag()
        {
            if (_currentIteration == 2)
            {
                Debug.Log("[IterationManager] Ghost hero reached the flag in Iteration 2! Enemy failed to stop it.");
                Debug.Log("[IterationManager] Restarting Iteration 2...");
                RestartIteration2();
            }
            else if (_currentIteration == 4)
            {
                Debug.Log("[IterationManager] Ghost hero #2 reached the flag in Iteration 4! Enemy #2 failed to stop it.");
                Debug.Log("[IterationManager] Restarting Iteration 4...");
                RestartIteration4();
            }
        }

        void StartIteration3()
        {
            Debug.Log("=== ITERATION 3: You control HERO (dodge ghost enemy #1) ===");
            _currentIteration = 3;

            if (_liveEnemy1 != null)
            {
                var enemyPlayerInput = _liveEnemy1.GetComponent<UnityEngine.InputSystem.PlayerInput>();
                if (enemyPlayerInput != null)
                {
                    enemyPlayerInput.DeactivateInput();
                    enemyPlayerInput.enabled = false;
                    Debug.Log("[IterationManager] Enemy #1 PlayerInput DISABLED");
                }

                var enemyPlayerController = _liveEnemy1.GetComponent<PlayerController>();
                if (enemyPlayerController != null)
                {
                    enemyPlayerController.enabled = false;
                }
            }

            if (_ghostHero != null)
            {
                var ghostHealth = _ghostHero.GetComponent<Health>();
                if (ghostHealth != null)
                {
                    ghostHealth.OnDeath -= OnGhostHeroDied;
                }

                var ghostCollider = _ghostHero.GetComponent<Collider2D>();
                if (ghostCollider != null)
                {
                    ghostCollider.enabled = false;
                }

                _ghostHero.tag = "Untagged";
                _ghostHero.SetActive(false);
                Destroy(_ghostHero);
                _ghostHero = null;
                Debug.Log("[IterationManager] Ghost hero fully cleaned up for Iteration 3");
            }

            _liveHero.SetActive(true);
            _liveHero.transform.localScale = new Vector3(0.15f, 0.15f, 1f);

            if (heroSpawnPoint != null)
            {
                _liveHero.transform.position = heroSpawnPoint.position;
            }

            var heroHealth = _liveHero.GetComponent<Health>();
            if (heroHealth != null)
            {
                heroHealth.ResetHealth();
                heroHealth.OnDeath -= OnHeroDiedInIteration3;
                heroHealth.OnDeath += OnHeroDiedInIteration3;
                Debug.Log("[IterationManager] Hero Health reset and OnDeath subscribed for Iteration 3");
            }
            else
            {
                Debug.LogError("[IterationManager] Hero has NO Health component in Iteration 3!");
            }

            var heroCollider = _liveHero.GetComponent<Collider2D>();
            if (heroCollider != null)
            {
                heroCollider.enabled = true;
                heroCollider.isTrigger = false;
                Debug.Log($"[IterationManager] Hero collider enabled: {heroCollider.GetType().Name}, isTrigger={heroCollider.isTrigger}");
            }
            else
            {
                Debug.LogError("[IterationManager] Hero has NO Collider2D!");
            }

            var heroRb = _liveHero.GetComponent<Rigidbody2D>();
            if (heroRb != null)
            {
                heroRb.bodyType = RigidbodyType2D.Dynamic;
            }

            _liveHero.tag = "Player";

            var heroInput = _liveHero.GetComponent<UnityEngine.InputSystem.PlayerInput>();
            if (heroInput != null)
            {
                heroInput.enabled = false;
                heroInput.enabled = true;
                heroInput.ActivateInput();
                Debug.Log($"[IterationManager] Hero input re-enabled for Iteration 3");
            }
            else
            {
                Debug.LogError("[IterationManager] Hero has NO PlayerInput component in Iteration 3!");
            }

            var heroController = _liveHero.GetComponent<PlayerController>();
            if (heroController != null)
            {
                heroController.enabled = true;
                Debug.Log("[IterationManager] Hero PlayerController enabled for Iteration 3");
            }

            var heroShooter = _liveHero.GetComponent<ProjectileShooter>();
            if (heroShooter != null)
            {
                heroShooter.SetTargetTag("Enemy");
                heroShooter.StartRecording();
            }

            if (_liveEnemy1 != null && _iteration2EnemyRecording != null)
            {
                _ghostEnemy1 = _liveEnemy1;
                _ghostEnemy1.name = "GhostEnemy1 (Iteration 2 Replay - PERMANENT)";
                _ghostEnemy1.tag = "Enemy";
                _ghostEnemy1.transform.localScale = new Vector3(0.15f, 0.15f, 1f);

                var enemyPlayerInput = _ghostEnemy1.GetComponent<UnityEngine.InputSystem.PlayerInput>();
                if (enemyPlayerInput != null)
                {
                    Destroy(enemyPlayerInput);
                    Debug.Log("[IterationManager] Ghost enemy #1 PlayerInput destroyed");
                }

                var enemyPlayerController = _ghostEnemy1.GetComponent<PlayerController>();
                if (enemyPlayerController != null)
                {
                    Destroy(enemyPlayerController);
                }

                var enemyRecorder = _ghostEnemy1.GetComponent<MovementRecorder>();
                if (enemyRecorder != null)
                {
                    enemyRecorder.enabled = false;
                    Destroy(enemyRecorder);
                }

                var ghostEnemyRb = _ghostEnemy1.GetComponent<Rigidbody2D>();
                if (ghostEnemyRb != null)
                {
                    ghostEnemyRb.bodyType = RigidbodyType2D.Kinematic;
                }

                var ghostEnemyHealth = _ghostEnemy1.GetComponent<Health>();
                if (ghostEnemyHealth != null)
                {
                    ghostEnemyHealth.ResetHealth();
                    ghostEnemyHealth.OnDeath -= OnGhostEnemy1Died;
                    ghostEnemyHealth.OnDeath += OnGhostEnemy1Died;
                    Debug.Log("[IterationManager] Ghost enemy #1 Health reset and OnDeath subscribed");
                }

                var ghostEnemyCollider = _ghostEnemy1.GetComponent<Collider2D>();
                if (ghostEnemyCollider != null)
                {
                    ghostEnemyCollider.enabled = true;
                }

                var ghostReplay = _ghostEnemy1.AddComponent<GhostReplay>();
                ghostReplay.StartPlayback(_iteration2EnemyRecording);

                var ghostShooter = _ghostEnemy1.GetComponent<ProjectileShooter>();
                if (ghostShooter != null && _iteration2EnemyShooterRecording != null)
                {
                    ghostShooter.SetTargetTag("Player");
                    ghostShooter.StartReplay(_iteration2EnemyShooterRecording);
                    Debug.Log($"[IterationManager] Ghost enemy #1 shooter replaying {_iteration2EnemyShooterRecording.Count} shots");
                }

                _liveEnemy1 = null;
                Debug.Log("[IterationManager] Enemy #1 is now a PERMANENT GHOST for all remaining iterations!");
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetState(GameState.HeroTurn);
            }

            if (_iterationTimer != null)
            {
                _iterationTimer.StartTimer();
            }
        }

        void OnHeroDiedInIteration3()
        {
            Debug.Log("[IterationManager] Hero died in Iteration 3! Restarting Iteration 3...");
            Invoke(nameof(RestartIteration3), 0.1f);
        }

        void OnGhostEnemy1Died()
        {
            Debug.Log("[IterationManager] Ghost enemy #1 killed by hero! Hero defended successfully.");
            _ghostEnemy1 = null;
        }
        }

        /// <summary>
        /// Start Iteration 3 from a restart (when hero died).
        /// Creates a new ghost enemy from the prefab since _liveEnemy no longer exists.
        /// </summary>
        void StartIteration3FromRestart()
        {
            Debug.Log("=== ITERATION 3 (RESTART): You control HERO (dodge ghost enemy) ===");
            _currentIteration = 3;

            // IMPORTANT: Reactivate hero (was deactivated when they died)
            _liveHero.SetActive(true);
            _liveHero.transform.localScale = new Vector3(0.15f, 0.15f, 1f);

            // Reset hero position and physics state
            if (heroSpawnPoint != null)
            {
                _liveHero.transform.position = heroSpawnPoint.position;
            }

            // Reset rigidbody velocity
            var heroRb = _liveHero.GetComponent<Rigidbody2D>();
            if (heroRb != null)
            {
                heroRb.linearVelocity = Vector2.zero;
                heroRb.bodyType = RigidbodyType2D.Dynamic;
            }

            var heroHealth = _liveHero.GetComponent<Health>();
            if (heroHealth != null)
            {
                heroHealth.ResetHealth();
                heroHealth.OnDeath -= OnHeroDiedInIteration3;
                heroHealth.OnDeath += OnHeroDiedInIteration3;
                Debug.Log($"[IterationManager] Hero Health reset - isDead={heroHealth.IsDead}, health={heroHealth.CurrentHealth}");
            }

            // Ensure hero's collider is enabled and verify its state
            var heroCollider = _liveHero.GetComponent<Collider2D>();
            if (heroCollider != null)
            {
                heroCollider.enabled = true;
                heroCollider.isTrigger = false; // Must NOT be trigger to be hit by trigger projectiles
                Debug.Log($"[IterationManager] Hero collider state - enabled={heroCollider.enabled}, isTrigger={heroCollider.isTrigger}, type={heroCollider.GetType().Name}, layer={LayerMask.LayerToName(_liveHero.layer)}");
            }
            else
            {
                Debug.LogError("[IterationManager] Hero has NO collider!");
            }

            _liveHero.tag = "Player";

            // Force physics system to update after position change and collider enable
            Physics2D.SyncTransforms();

            Debug.Log($"[IterationManager] Hero tag='{_liveHero.tag}', active={_liveHero.activeInHierarchy}, position={_liveHero.transform.position}");

            // Re-enable hero input
            var heroInput = _liveHero.GetComponent<UnityEngine.InputSystem.PlayerInput>();
            if (heroInput != null)
            {
                heroInput.enabled = false;
                heroInput.enabled = true;
                heroInput.ActivateInput();
            }

            var heroController = _liveHero.GetComponent<PlayerController>();
            if (heroController != null)
            {
                heroController.enabled = true;
            }

            // Configure hero shooter
            var heroShooter = _liveHero.GetComponent<ProjectileShooter>();
            if (heroShooter != null)
            {
                heroShooter.SetTargetTag("Enemy");
            }

            if (enemyPrefab != null && _iteration2EnemyRecording != null)
            {
                _ghostEnemy1 = Instantiate(enemyPrefab, enemy1SpawnPoint.position, Quaternion.identity);
                _ghostEnemy1.name = "GhostEnemy1 (Iteration 2 Replay)";
                _ghostEnemy1.tag = "Enemy";
                _ghostEnemy1.transform.localScale = new Vector3(0.15f, 0.15f, 1f);

                var enemyPlayerInput = _ghostEnemy1.GetComponent<UnityEngine.InputSystem.PlayerInput>();
                if (enemyPlayerInput != null)
                {
                    enemyPlayerInput.enabled = false;
                    Destroy(enemyPlayerInput);
                }

                var enemyPlayerController = _ghostEnemy1.GetComponent<PlayerController>();
                if (enemyPlayerController != null)
                {
                    enemyPlayerController.enabled = false;
                    Destroy(enemyPlayerController);
                }

                var enemyRecorder = _ghostEnemy1.GetComponent<MovementRecorder>();
                if (enemyRecorder != null)
                {
                    Destroy(enemyRecorder);
                }

                var ghostEnemyRb = _ghostEnemy1.GetComponent<Rigidbody2D>();
                if (ghostEnemyRb != null)
                {
                    ghostEnemyRb.bodyType = RigidbodyType2D.Kinematic;
                }

                var ghostEnemyHealth = _ghostEnemy1.GetComponent<Health>();
                if (ghostEnemyHealth == null)
                {
                    ghostEnemyHealth = _ghostEnemy1.AddComponent<Health>();
                }
                ghostEnemyHealth.ResetHealth();
                ghostEnemyHealth.OnDeath += OnGhostEnemy1Died;

                var ghostEnemyCollider = _ghostEnemy1.GetComponent<Collider2D>();
                if (ghostEnemyCollider != null)
                {
                    ghostEnemyCollider.enabled = true;
                }

                var ghostReplay = _ghostEnemy1.AddComponent<GhostReplay>();
                ghostReplay.StartPlayback(_iteration2EnemyRecording);

                var ghostShooter = _ghostEnemy1.GetComponent<ProjectileShooter>();
                if (ghostShooter != null && _iteration2EnemyShooterRecording != null)
                {
                    ghostShooter.SetTargetTag("Player");
                    ghostShooter.StartReplay(_iteration2EnemyShooterRecording);
                }

                Debug.Log("[IterationManager] Ghost enemy #1 created for Iteration 3 restart");
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetState(GameState.HeroTurn);
            }

            if (_iterationTimer != null)
            {
                _iterationTimer.StartTimer();
            }
        }

        public void OnHeroReachedFlagInIteration3()
        {
            if (_currentIteration != 3)
            {
                Debug.LogWarning("[IterationManager] OnHeroReachedFlagInIteration3 called but not in Iteration 3!");
                return;
            }

            if (_iterationTimer != null)
                _iterationTimer.StopTimer();

            if (_totalIterations == 3)
            {
                Debug.Log("[IterationManager] Hero reached flag in Iteration 3 (3-iteration mode)! Round complete!");
                
                CleanupGhosts();

                if (_liveHero != null)
                {
                    var heroInput = _liveHero.GetComponent<UnityEngine.InputSystem.PlayerInput>();
                    if (heroInput != null) heroInput.DeactivateInput();

                    var heroRb = _liveHero.GetComponent<Rigidbody2D>();
                    if (heroRb != null) heroRb.linearVelocity = Vector2.zero;
                }

                if (RoundManager.Instance != null)
                {
                    RoundManager.Instance.OnIterationCycleComplete();
                }
                else
                {
                    Debug.LogWarning("[IterationManager] No RoundManager found! Falling back to LevelComplete.");
                    if (GameManager.Instance != null)
                        GameManager.Instance.SetState(GameState.LevelComplete);
                }
            }
            else
            {
                Debug.Log("[IterationManager] Hero reached flag in Iteration 3 (5-iteration mode)! Recording complete. Moving to Iteration 4.");
                _heroReachedFlagInIteration3 = true;

                var heroRecorder = _liveHero.GetComponent<MovementRecorder>();
                if (heroRecorder != null)
                {
                    heroRecorder.StopRecording();
                    _iteration3HeroRecording = heroRecorder.GetRecording();
                    Debug.Log($"[IterationManager] Stored hero recording #2: {_iteration3HeroRecording.Count} frames");
                }

                var heroShooter = _liveHero.GetComponent<ProjectileShooter>();
                if (heroShooter != null)
                {
                    heroShooter.StopRecording();
                    _iteration3HeroShooterRecording = heroShooter.GetRecording();
                    Debug.Log($"[IterationManager] Stored hero shooter recording #2: {_iteration3HeroShooterRecording.Count} shots");
                }

                SaveIterationSnapshot();
                StartIteration4();
            }
        }


        /// <summary>
        /// Restart Iteration 2 when enemy dies or ghost reaches flag.
        /// Ghost hero replays from beginning, enemy respawns.
        /// </summary>
        void RestartIteration2()
        {
            Debug.Log("[IterationManager] === RESTARTING ITERATION 2 ===");

            // Clean up current ghost and enemy
            if (_ghostHero != null)
            {
                var ghostHealth = _ghostHero.GetComponent<Health>();
                if (ghostHealth != null)
                {
                    ghostHealth.OnDeath -= OnGhostHeroDied;
                }
                _ghostHero.tag = "Untagged";
                _ghostHero.SetActive(false);
                Destroy(_ghostHero);
                _ghostHero = null;
            }

            if (_liveEnemy1 != null)
            {
                var enemyHealth = _liveEnemy1.GetComponent<Health>();
                if (enemyHealth != null)
                {
                    enemyHealth.OnDeath -= OnLiveEnemyDied;
                }
                _liveEnemy1.SetActive(false);
                Destroy(_liveEnemy1);
                _liveEnemy1 = null;
            }

            StartIteration2();
        }

        void RestartIteration3()
        {
            Debug.Log("[IterationManager] === RESTARTING ITERATION 3 ===");

            if (_ghostEnemy1 != null)
            {
                var ghostHealth = _ghostEnemy1.GetComponent<Health>();
                if (ghostHealth != null)
                {
                    ghostHealth.OnDeath -= OnGhostEnemy1Died;
                }
                _ghostEnemy1.tag = "Untagged";
                _ghostEnemy1.SetActive(false);
                Destroy(_ghostEnemy1);
                _ghostEnemy1 = null;
            }

            // Hero needs to be re-enabled (it was deactivated when it died)
            _liveHero.SetActive(true);
            _liveHero.transform.localScale = new Vector3(0.15f, 0.15f, 1f);

            // Start Iteration 3 fresh with the same Iteration 2 recording
            StartIteration3FromRestart();
        }

        public void ResetIterations()
        {
            Debug.Log("[IterationManager] Resetting iteration cycle...");

            CleanupGhosts();

            _liveHero.SetActive(true);
            _liveHero.transform.localScale = new Vector3(0.15f, 0.15f, 1f);

            if (heroSpawnPoint != null)
            {
                _liveHero.transform.position = heroSpawnPoint.position;
            }

            var heroHealth = _liveHero.GetComponent<Health>();
            if (heroHealth != null)
            {
                heroHealth.ResetHealth();
            }

            if (_liveEnemy1 != null)
            {
                _liveEnemy1.SetActive(false);
                Destroy(_liveEnemy1);
                _liveEnemy1 = null;
            }

            if (_liveEnemy2 != null)
            {
                _liveEnemy2.SetActive(false);
                Destroy(_liveEnemy2);
                _liveEnemy2 = null;
            }

            _iteration1HeroRecording = null;
            _iteration1HeroShooterRecording = null;
            _iteration2EnemyRecording = null;
            _iteration2EnemyShooterRecording = null;

            StartIteration1();
        }

        void CleanupGhosts()
        {
            if (_ghostHero != null)
            {
                _ghostHero.tag = "Untagged";
                _ghostHero.SetActive(false);
                Destroy(_ghostHero);
                _ghostHero = null;
            }

            if (_ghostHero2 != null)
            {
                _ghostHero2.tag = "Untagged";
                _ghostHero2.SetActive(false);
                Destroy(_ghostHero2);
                _ghostHero2 = null;
            }

            if (_ghostEnemy1 != null)
            {
                _ghostEnemy1.tag = "Untagged";
                _ghostEnemy1.SetActive(false);
                Destroy(_ghostEnemy1);
                _ghostEnemy1 = null;
            }

            if (_ghostEnemy2 != null)
            {
                _ghostEnemy2.tag = "Untagged";
                _ghostEnemy2.SetActive(false);
                Destroy(_ghostEnemy2);
                _ghostEnemy2 = null;
            }
        }

        void OnDestroy()
        {
            CleanupGhosts();

            if (_liveEnemy1 != null)
            {
                Destroy(_liveEnemy1);
            }

            if (_liveEnemy2 != null)
            {
                Destroy(_liveEnemy2);
            }

            _iteration1HeroRecording?.Clear();
            _iteration1HeroShooterRecording?.Clear();
            _iteration2EnemyRecording?.Clear();
            _iteration2EnemyShooterRecording?.Clear();
        }

        void OnIterationTimerExpired()
        {
            Debug.Log($"[IterationTimer] Timer expired in Iteration {_currentIteration}!");

            switch (_currentIteration)
            {
                case 1:
                    Debug.Log("[IterationTimer] Iteration 1 failed - Time's up! Hero didn't reach the flag in time.");
                    ResetIterations();
                    break;
                case 2:
                    Debug.Log("[IterationTimer] Iteration 2 failed - Time's up! Enemy didn't stop the ghost in time.");
                    RestartIteration2();
                    break;
                case 3:
                    Debug.Log("[IterationTimer] Iteration 3 failed - Time's up! Hero didn't reach the flag in time.");
                    RestartIteration3();
                    break;
                case 4:
                    Debug.Log("[IterationTimer] Iteration 4 failed - Time's up! Enemy #2 didn't stop the ghost in time.");
                    RestartIteration4();
                    break;
                case 5:
                    Debug.Log("[IterationTimer] Iteration 5 failed - Time's up! Hero didn't reach the flag in time.");
                    RestartIteration5();
                    break;
            }
        }

        void StartIteration4()
        {
            Debug.Log("=== ITERATION 4: You control ENEMY #2 (Ghost Enemy #1 still active!) ===");
            _currentIteration = 4;
            _enemy2KilledGhostInIteration4 = false;

            if (_iteration3HeroRecording == null || _iteration3HeroRecording.Count == 0)
            {
                Debug.LogError("[IterationManager] ERROR: Hero recording #2 is null or empty! Cannot replay.");
                ResetIterations();
                return;
            }
            Debug.Log($"[IterationManager] Hero recording #2 valid: {_iteration3HeroRecording.Count} frames, {_iteration3HeroShooterRecording?.Count ?? 0} shots");

            if (_liveHero != null)
            {
                var liveHeroHealth = _liveHero.GetComponent<Health>();
                if (liveHeroHealth != null)
                {
                    liveHeroHealth.OnDeath -= OnHeroDiedInIteration3;
                }
                _liveHero.SetActive(false);
                Debug.Log("[IterationManager] Hero deactivated");
            }

            _ghostHero2 = Instantiate(heroPrefab, heroSpawnPoint.position, Quaternion.identity);
            _ghostHero2.name = "GhostHero2 (Iteration 3 Replay)";
            _ghostHero2.tag = "Player";
            _ghostHero2.transform.localScale = new Vector3(0.15f, 0.15f, 1f);

            var ghostPlayerController = _ghostHero2.GetComponent<PlayerController>();
            if (ghostPlayerController != null)
            {
                ghostPlayerController.enabled = false;
                Destroy(ghostPlayerController);
            }

            var ghostPlayerInput = _ghostHero2.GetComponent<UnityEngine.InputSystem.PlayerInput>();
            if (ghostPlayerInput != null)
            {
                ghostPlayerInput.enabled = false;
                Destroy(ghostPlayerInput);
            }

            var ghostRecorder = _ghostHero2.GetComponent<MovementRecorder>();
            if (ghostRecorder != null)
            {
                ghostRecorder.enabled = false;
                Destroy(ghostRecorder);
            }

            var ghostRb = _ghostHero2.GetComponent<Rigidbody2D>();
            if (ghostRb != null)
            {
                ghostRb.bodyType = RigidbodyType2D.Kinematic;
            }

            var ghostHealth = _ghostHero2.GetComponent<Health>();
            if (ghostHealth != null)
            {
                ghostHealth.ResetHealth();
                ghostHealth.OnDeath += OnGhostHero2Died;
                Debug.Log("[IterationManager] Ghost hero #2 Health reset and OnDeath subscribed");
            }
            else
            {
                Debug.LogError("[IterationManager] Ghost hero #2 has NO Health component!");
            }

            var ghostCollider = _ghostHero2.GetComponent<Collider2D>();
            if (ghostCollider != null)
            {
                ghostCollider.enabled = true;
                Debug.Log($"[IterationManager] Ghost hero #2 collider enabled: {ghostCollider.GetType().Name}");
            }
            else
            {
                Debug.LogError("[IterationManager] Ghost hero #2 has NO Collider2D!");
            }

            var ghostReplay = _ghostHero2.AddComponent<GhostReplay>();
            ghostReplay.StartPlayback(_iteration3HeroRecording);

            var ghostShooter = _ghostHero2.GetComponent<ProjectileShooter>();
            if (ghostShooter != null && _iteration3HeroShooterRecording != null)
            {
                Debug.Log($"[IterationManager] Ghost hero #2 shooter found. Using stored shot recording: {_iteration3HeroShooterRecording.Count} shots");
                ghostShooter.SetTargetTag("Enemy");
                ghostShooter.StartReplay(_iteration3HeroShooterRecording);
            }

            if (_ghostEnemy1 != null)
            {
                Debug.Log("[IterationManager] ⚠️ Ghost enemy #1 REMAINS ACTIVE in Iteration 4!");
            }

            if (enemyPrefab != null)
            {
                _liveEnemy2 = Instantiate(enemyPrefab, enemy2SpawnPoint.position, Quaternion.identity);
                _liveEnemy2.name = "LiveEnemy2 (Player Controlled)";
                _liveEnemy2.tag = "Enemy";
                _liveEnemy2.transform.localScale = new Vector3(0.15f, 0.15f, 1f);
                Debug.Log($"[IterationManager] Enemy #2 spawned at {enemy2SpawnPoint.position}");

                var enemyHealth = _liveEnemy2.GetComponent<Health>();
                if (enemyHealth == null)
                {
                    enemyHealth = _liveEnemy2.AddComponent<Health>();
                }
                enemyHealth.ResetHealth();
                enemyHealth.OnDeath -= OnLiveEnemy2Died;
                enemyHealth.OnDeath += OnLiveEnemy2Died;

                var enemyCollider = _liveEnemy2.GetComponent<Collider2D>();
                if (enemyCollider != null)
                {
                    enemyCollider.enabled = true;
                }
                else
                {
                    var capsuleCollider = _liveEnemy2.AddComponent<CapsuleCollider2D>();
                    capsuleCollider.size = new Vector2(1f, 2f);
                }

                var enemyInput = _liveEnemy2.GetComponent<UnityEngine.InputSystem.PlayerInput>();
                if (enemyInput != null)
                {
                    enemyInput.enabled = true;
                }

                var enemyController = _liveEnemy2.GetComponent<PlayerController>();
                if (enemyController != null)
                {
                    enemyController.enabled = true;
                }

                var enemyRecorder = _liveEnemy2.GetComponent<MovementRecorder>();
                if (enemyRecorder == null)
                {
                    enemyRecorder = _liveEnemy2.AddComponent<MovementRecorder>();
                }
                enemyRecorder.StartRecording();

                var enemyShooter = _liveEnemy2.GetComponent<ProjectileShooter>();
                if (enemyShooter != null)
                {
                    enemyShooter.SetTargetTag("Player");
                    enemyShooter.StartRecording();
                }
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetState(GameState.MonsterTurn);
            }

            if (_iterationTimer != null)
            {
                _iterationTimer.StartTimer();
            }
        }

        void OnGhostHero2Died()
        {
            if (_currentIteration != 4)
            {
                return;
            }

            Debug.Log("[IterationManager] Ghost hero #2 died in Iteration 4!");
            _enemy2KilledGhostInIteration4 = true;

            if (_iterationTimer != null)
            {
                _iterationTimer.StopTimer();
            }

            if (_liveEnemy2 != null)
            {
                var enemyRecorder = _liveEnemy2.GetComponent<MovementRecorder>();
                if (enemyRecorder != null)
                {
                    enemyRecorder.StopRecording();
                    _iteration4Enemy2Recording = enemyRecorder.GetRecording();
                    Debug.Log($"[IterationManager] Stored enemy #2 movement recording: {_iteration4Enemy2Recording.Count} frames");
                }

                var enemyShooter = _liveEnemy2.GetComponent<ProjectileShooter>();
                if (enemyShooter != null)
                {
                    enemyShooter.StopRecording();
                    _iteration4Enemy2ShooterRecording = enemyShooter.GetRecording();
                    Debug.Log($"[IterationManager] Stored enemy #2 shooter recording: {_iteration4Enemy2ShooterRecording.Count} shots");
                }
            }

            SaveIterationSnapshot();
            StartIteration5();
        }

        void OnLiveEnemy2Died()
        {
            if (_currentIteration != 4)
            {
                return;
            }

            Debug.Log("[IterationManager] Live enemy #2 died in Iteration 4! Restarting Iteration 4...");

            if (_liveEnemy2 != null)
            {
                var enemyHealth = _liveEnemy2.GetComponent<Health>();
                if (enemyHealth != null)
                {
                    enemyHealth.OnDeath -= OnLiveEnemy2Died;
                }
            }

            Invoke(nameof(RestartIteration4), 0.1f);
        }

        void StartIteration5()
        {
            Debug.Log("=== ITERATION 5: You control HERO (dodge both ghost enemies) ===");
            _currentIteration = 5;

            if (_liveEnemy2 != null)
            {
                var enemyPlayerInput = _liveEnemy2.GetComponent<UnityEngine.InputSystem.PlayerInput>();
                if (enemyPlayerInput != null)
                {
                    enemyPlayerInput.DeactivateInput();
                    enemyPlayerInput.enabled = false;
                    Debug.Log("[IterationManager] Enemy #2 PlayerInput DISABLED");
                }

                var enemyPlayerController = _liveEnemy2.GetComponent<PlayerController>();
                if (enemyPlayerController != null)
                {
                    enemyPlayerController.enabled = false;
                }
            }

            if (_ghostHero2 != null)
            {
                var ghostHealth = _ghostHero2.GetComponent<Health>();
                if (ghostHealth != null)
                {
                    ghostHealth.OnDeath -= OnGhostHero2Died;
                }

                var ghostCollider = _ghostHero2.GetComponent<Collider2D>();
                if (ghostCollider != null)
                {
                    ghostCollider.enabled = false;
                }

                _ghostHero2.tag = "Untagged";
                _ghostHero2.SetActive(false);
                Destroy(_ghostHero2);
                _ghostHero2 = null;
                Debug.Log("[IterationManager] Ghost hero #2 cleaned up for Iteration 5");
            }

            _liveHero.SetActive(true);
            _liveHero.transform.localScale = new Vector3(0.15f, 0.15f, 1f);

            if (heroSpawnPoint != null)
            {
                _liveHero.transform.position = heroSpawnPoint.position;
            }

            var heroHealth = _liveHero.GetComponent<Health>();
            if (heroHealth != null)
            {
                heroHealth.ResetHealth();
                heroHealth.OnDeath -= OnHeroDiedInIteration5;
                heroHealth.OnDeath += OnHeroDiedInIteration5;
                Debug.Log("[IterationManager] Hero Health reset and OnDeath subscribed for Iteration 5");
            }

            var heroCollider = _liveHero.GetComponent<Collider2D>();
            if (heroCollider != null)
            {
                heroCollider.enabled = true;
                heroCollider.isTrigger = false;
            }

            var heroRb = _liveHero.GetComponent<Rigidbody2D>();
            if (heroRb != null)
            {
                heroRb.bodyType = RigidbodyType2D.Dynamic;
            }

            _liveHero.tag = "Player";

            var heroInput = _liveHero.GetComponent<UnityEngine.InputSystem.PlayerInput>();
            if (heroInput != null)
            {
                heroInput.enabled = false;
                heroInput.enabled = true;
                heroInput.ActivateInput();
                Debug.Log($"[IterationManager] Hero input re-enabled for Iteration 5");
            }

            var heroController = _liveHero.GetComponent<PlayerController>();
            if (heroController != null)
            {
                heroController.enabled = true;
            }

            var heroShooter = _liveHero.GetComponent<ProjectileShooter>();
            if (heroShooter != null)
            {
                heroShooter.SetTargetTag("Enemy");
                heroShooter.StartRecording();
            }

            if (_liveEnemy2 != null && _iteration4Enemy2Recording != null)
            {
                _ghostEnemy2 = _liveEnemy2;
                _ghostEnemy2.name = "GhostEnemy2 (Iteration 4 Replay)";
                _ghostEnemy2.tag = "Enemy";
                _ghostEnemy2.transform.localScale = new Vector3(0.15f, 0.15f, 1f);

                var enemyPlayerInput = _ghostEnemy2.GetComponent<UnityEngine.InputSystem.PlayerInput>();
                if (enemyPlayerInput != null)
                {
                    Destroy(enemyPlayerInput);
                }

                var enemyPlayerController = _ghostEnemy2.GetComponent<PlayerController>();
                if (enemyPlayerController != null)
                {
                    Destroy(enemyPlayerController);
                }

                var enemyRecorder = _ghostEnemy2.GetComponent<MovementRecorder>();
                if (enemyRecorder != null)
                {
                    enemyRecorder.enabled = false;
                    Destroy(enemyRecorder);
                }

                var ghostEnemy2Rb = _ghostEnemy2.GetComponent<Rigidbody2D>();
                if (ghostEnemy2Rb != null)
                {
                    ghostEnemy2Rb.bodyType = RigidbodyType2D.Kinematic;
                }

                var ghostEnemy2Health = _ghostEnemy2.GetComponent<Health>();
                if (ghostEnemy2Health != null)
                {
                    ghostEnemy2Health.ResetHealth();
                    ghostEnemy2Health.OnDeath -= OnGhostEnemy2Died;
                    ghostEnemy2Health.OnDeath += OnGhostEnemy2Died;
                }

                var ghostEnemy2Collider = _ghostEnemy2.GetComponent<Collider2D>();
                if (ghostEnemy2Collider != null)
                {
                    ghostEnemy2Collider.enabled = true;
                }

                var ghostReplay = _ghostEnemy2.AddComponent<GhostReplay>();
                ghostReplay.StartPlayback(_iteration4Enemy2Recording);

                var ghostShooter = _ghostEnemy2.GetComponent<ProjectileShooter>();
                if (ghostShooter != null && _iteration4Enemy2ShooterRecording != null)
                {
                    ghostShooter.SetTargetTag("Player");
                    ghostShooter.StartReplay(_iteration4Enemy2ShooterRecording);
                    Debug.Log($"[IterationManager] Ghost enemy #2 shooter replaying {_iteration4Enemy2ShooterRecording.Count} shots");
                }

                _liveEnemy2 = null;
                Debug.Log("[IterationManager] Ghost enemy #2 ready for Iteration 5");
            }

            if (_ghostEnemy1 != null)
            {
                Debug.Log("[IterationManager] ⚠️ Ghost enemy #1 REMAINS ACTIVE in Iteration 5!");
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetState(GameState.HeroTurn);
            }

            if (_iterationTimer != null)
            {
                _iterationTimer.StartTimer();
            }
        }

        void OnHeroDiedInIteration5()
        {
            Debug.Log("[IterationManager] Hero died in Iteration 5! Restarting Iteration 5...");
            Invoke(nameof(RestartIteration5), 0.1f);
        }

        void OnGhostEnemy2Died()
        {
            if (_currentIteration != 5) return;

            Debug.Log("[IterationManager] Ghost enemy #2 killed by hero in Iteration 5!");
            _ghostEnemy2 = null;
        }

        public void OnHeroReachedFlagInIteration5()
        {
            if (_currentIteration != 5)
            {
                Debug.LogWarning("[IterationManager] OnHeroReachedFlagInIteration5 called but not in Iteration 5!");
                return;
            }

            Debug.Log("[IterationManager] Hero reached flag in Iteration 5! Round complete!");

            if (_iterationTimer != null)
                _iterationTimer.StopTimer();

            CleanupGhosts();

            if (_liveHero != null)
            {
                var heroInput = _liveHero.GetComponent<UnityEngine.InputSystem.PlayerInput>();
                if (heroInput != null) heroInput.DeactivateInput();

                var heroRb = _liveHero.GetComponent<Rigidbody2D>();
                if (heroRb != null) heroRb.linearVelocity = Vector2.zero;
            }

            if (RoundManager.Instance != null)
            {
                RoundManager.Instance.OnIterationCycleComplete();
            }
            else
            {
                Debug.LogWarning("[IterationManager] No RoundManager found!");
                if (GameManager.Instance != null)
                    GameManager.Instance.SetState(GameState.LevelComplete);
            }
        }

        void RestartIteration4()
        {
            Debug.Log("[IterationManager] === RESTARTING ITERATION 4 ===");

            if (_ghostHero2 != null)
            {
                var ghostHealth = _ghostHero2.GetComponent<Health>();
                if (ghostHealth != null)
                {
                    ghostHealth.OnDeath -= OnGhostHero2Died;
                }
                _ghostHero2.tag = "Untagged";
                _ghostHero2.SetActive(false);
                Destroy(_ghostHero2);
                _ghostHero2 = null;
            }

            if (_liveEnemy2 != null)
            {
                var enemyHealth = _liveEnemy2.GetComponent<Health>();
                if (enemyHealth != null)
                {
                    enemyHealth.OnDeath -= OnLiveEnemy2Died;
                }
                _liveEnemy2.SetActive(false);
                Destroy(_liveEnemy2);
                _liveEnemy2 = null;
            }

            StartIteration4();
        }

        void RestartIteration5()
        {
            Debug.Log("[IterationManager] === RESTARTING ITERATION 5 ===");

            if (_ghostEnemy2 != null)
            {
                var ghostHealth = _ghostEnemy2.GetComponent<Health>();
                if (ghostHealth != null)
                {
                    ghostHealth.OnDeath -= OnGhostEnemy2Died;
                }
                _ghostEnemy2.tag = "Untagged";
                _ghostEnemy2.SetActive(false);
                Destroy(_ghostEnemy2);
                _ghostEnemy2 = null;
            }

            _liveHero.SetActive(true);
            _liveHero.transform.localScale = new Vector3(0.15f, 0.15f, 1f);

            StartIteration5FromRestart();
        }

        void StartIteration5FromRestart()
        {
            Debug.Log("=== ITERATION 5 (RESTART): You control HERO (dodge both ghost enemies) ===");
            _currentIteration = 5;

            _liveHero.SetActive(true);
            _liveHero.transform.localScale = new Vector3(0.15f, 0.15f, 1f);

            if (heroSpawnPoint != null)
            {
                _liveHero.transform.position = heroSpawnPoint.position;
            }

            var heroRb = _liveHero.GetComponent<Rigidbody2D>();
            if (heroRb != null)
            {
                heroRb.linearVelocity = Vector2.zero;
                heroRb.bodyType = RigidbodyType2D.Dynamic;
            }

            var heroHealth = _liveHero.GetComponent<Health>();
            if (heroHealth != null)
            {
                heroHealth.ResetHealth();
                heroHealth.OnDeath -= OnHeroDiedInIteration5;
                heroHealth.OnDeath += OnHeroDiedInIteration5;
            }

            var heroCollider = _liveHero.GetComponent<Collider2D>();
            if (heroCollider != null)
            {
                heroCollider.enabled = true;
                heroCollider.isTrigger = false;
            }

            _liveHero.tag = "Player";
            Physics2D.SyncTransforms();

            var heroInput = _liveHero.GetComponent<UnityEngine.InputSystem.PlayerInput>();
            if (heroInput != null)
            {
                heroInput.enabled = false;
                heroInput.enabled = true;
                heroInput.ActivateInput();
            }

            var heroController = _liveHero.GetComponent<PlayerController>();
            if (heroController != null)
            {
                heroController.enabled = true;
            }

            var heroShooter = _liveHero.GetComponent<ProjectileShooter>();
            if (heroShooter != null)
            {
                heroShooter.SetTargetTag("Enemy");
            }

            if (enemyPrefab != null && _iteration4Enemy2Recording != null)
            {
                _ghostEnemy2 = Instantiate(enemyPrefab, enemy2SpawnPoint.position, Quaternion.identity);
                _ghostEnemy2.name = "GhostEnemy2 (Iteration 4 Replay)";
                _ghostEnemy2.tag = "Enemy";
                _ghostEnemy2.transform.localScale = new Vector3(0.15f, 0.15f, 1f);

                var enemyPlayerInput = _ghostEnemy2.GetComponent<UnityEngine.InputSystem.PlayerInput>();
                if (enemyPlayerInput != null)
                {
                    enemyPlayerInput.enabled = false;
                    Destroy(enemyPlayerInput);
                }

                var enemyPlayerController = _ghostEnemy2.GetComponent<PlayerController>();
                if (enemyPlayerController != null)
                {
                    enemyPlayerController.enabled = false;
                    Destroy(enemyPlayerController);
                }

                var enemyRecorder = _ghostEnemy2.GetComponent<MovementRecorder>();
                if (enemyRecorder != null)
                {
                    Destroy(enemyRecorder);
                }

                var ghostEnemy2Rb = _ghostEnemy2.GetComponent<Rigidbody2D>();
                if (ghostEnemy2Rb != null)
                {
                    ghostEnemy2Rb.bodyType = RigidbodyType2D.Kinematic;
                }

                var ghostEnemy2Health = _ghostEnemy2.GetComponent<Health>();
                if (ghostEnemy2Health == null)
                {
                    ghostEnemy2Health = _ghostEnemy2.AddComponent<Health>();
                }
                ghostEnemy2Health.ResetHealth();
                ghostEnemy2Health.OnDeath += OnGhostEnemy2Died;

                var ghostEnemy2Collider = _ghostEnemy2.GetComponent<Collider2D>();
                if (ghostEnemy2Collider != null)
                {
                    ghostEnemy2Collider.enabled = true;
                }

                var ghostReplay = _ghostEnemy2.AddComponent<GhostReplay>();
                ghostReplay.StartPlayback(_iteration4Enemy2Recording);

                var ghostShooter = _ghostEnemy2.GetComponent<ProjectileShooter>();
                if (ghostShooter != null && _iteration4Enemy2ShooterRecording != null)
                {
                    ghostShooter.SetTargetTag("Player");
                    ghostShooter.StartReplay(_iteration4Enemy2ShooterRecording);
                }

                Debug.Log("[IterationManager] Ghost enemy #2 created for Iteration 5 restart");
            }

            if (enemyPrefab != null && _iteration2EnemyRecording != null)
            {
                _ghostEnemy1 = Instantiate(enemyPrefab, enemy1SpawnPoint.position, Quaternion.identity);
                _ghostEnemy1.name = "GhostEnemy1 (Iteration 2 Replay)";
                _ghostEnemy1.tag = "Enemy";
                _ghostEnemy1.transform.localScale = new Vector3(0.15f, 0.15f, 1f);

                var enemyPlayerInput = _ghostEnemy1.GetComponent<UnityEngine.InputSystem.PlayerInput>();
                if (enemyPlayerInput != null)
                {
                    enemyPlayerInput.enabled = false;
                    Destroy(enemyPlayerInput);
                }

                var enemyPlayerController = _ghostEnemy1.GetComponent<PlayerController>();
                if (enemyPlayerController != null)
                {
                    enemyPlayerController.enabled = false;
                    Destroy(enemyPlayerController);
                }

                var enemyRecorder = _ghostEnemy1.GetComponent<MovementRecorder>();
                if (enemyRecorder != null)
                {
                    Destroy(enemyRecorder);
                }

                var ghostEnemyRb = _ghostEnemy1.GetComponent<Rigidbody2D>();
                if (ghostEnemyRb != null)
                {
                    ghostEnemyRb.bodyType = RigidbodyType2D.Kinematic;
                }

                var ghostEnemyHealth = _ghostEnemy1.GetComponent<Health>();
                if (ghostEnemyHealth == null)
                {
                    ghostEnemyHealth = _ghostEnemy1.AddComponent<Health>();
                }
                ghostEnemyHealth.ResetHealth();
                ghostEnemyHealth.OnDeath += OnGhostEnemy1Died;

                var ghostEnemyCollider = _ghostEnemy1.GetComponent<Collider2D>();
                if (ghostEnemyCollider != null)
                {
                    ghostEnemyCollider.enabled = true;
                }

                var ghostReplay = _ghostEnemy1.AddComponent<GhostReplay>();
                ghostReplay.StartPlayback(_iteration2EnemyRecording);

                var ghostShooter = _ghostEnemy1.GetComponent<ProjectileShooter>();
                if (ghostShooter != null && _iteration2EnemyShooterRecording != null)
                {
                    ghostShooter.SetTargetTag("Player");
                    ghostShooter.StartReplay(_iteration2EnemyShooterRecording);
                }

                Debug.Log("[IterationManager] Ghost enemy #1 created for Iteration 5 restart");
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetState(GameState.HeroTurn);
            }

            if (_iterationTimer != null)
            {
                _iterationTimer.StartTimer();
            }
        }
    }
}
