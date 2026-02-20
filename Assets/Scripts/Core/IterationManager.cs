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
        List<ShootEvent> _iteration1HeroShooterRecording; // Store hero's projectile recording
        List<FrameData> _iteration2EnemyRecording;
        List<ShootEvent> _iteration2EnemyShooterRecording; // Store enemy's projectile recording

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
            _iteration1HeroShooterRecording = null;
            _iteration2EnemyRecording = null;
            _iteration2EnemyShooterRecording = null;

            StartIteration1();
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
                _heroShooter.SetTargetTag("Enemy"); // Hero shoots at enemies
                _heroShooter.StartRecording();
            }

            if (_liveEnemy != null)
            {
                _liveEnemy.SetActive(false);
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
                _iteration1HeroShooterRecording = _heroShooter.GetRecording();
                Debug.Log($"[IterationManager] Stored hero shot recording: {_iteration1HeroShooterRecording.Count} shots");
            }

            StartIteration2();
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
                _liveEnemy = Instantiate(enemyPrefab, enemySpawnPoint.position, Quaternion.identity);
                _liveEnemy.name = "LiveEnemy (Player Controlled)";
                _liveEnemy.tag = "Enemy"; // Must have Enemy tag so ghost hero projectiles can hit it
                _liveEnemy.transform.localScale = new Vector3(0.15f, 0.15f, 1f);
                Debug.Log($"[IterationManager] Enemy spawned at {enemySpawnPoint.position}");

                // Ensure enemy has a Health component so ghost projectiles can damage it
                var enemyHealth = _liveEnemy.GetComponent<Health>();
                if (enemyHealth == null)
                {
                    enemyHealth = _liveEnemy.AddComponent<Health>();
                    Debug.Log("[IterationManager] Added Health component to enemy");
                }
                enemyHealth.ResetHealth();
                // Subscribe to death event - if ghost hero kills enemy, enemy loses!
                enemyHealth.OnDeath -= OnLiveEnemyDied; // Prevent double subscription
                enemyHealth.OnDeath += OnLiveEnemyDied;
                Debug.Log("[IterationManager] Enemy Health reset and OnDeath subscribed");

                // Ensure enemy has a collider that can be hit
                var enemyCollider = _liveEnemy.GetComponent<Collider2D>();
                if (enemyCollider != null)
                {
                    enemyCollider.enabled = true;
                    Debug.Log($"[IterationManager] Enemy collider enabled: {enemyCollider.GetType().Name}");
                }
                else
                {
                    // Add a collider if missing
                    var capsuleCollider = _liveEnemy.AddComponent<CapsuleCollider2D>();
                    capsuleCollider.size = new Vector2(1f, 2f);
                    Debug.Log("[IterationManager] Added CapsuleCollider2D to enemy");
                }

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
                    enemyShooter.SetTargetTag("Player"); // Enemy shoots at hero/ghosts
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
                    Debug.Log($"[IterationManager] Stored enemy movement recording: {_iteration2EnemyRecording.Count} frames");
                }

                var enemyShooter = _liveEnemy.GetComponent<ProjectileShooter>();
                if (enemyShooter != null)
                {
                    enemyShooter.StopRecording();
                    _iteration2EnemyShooterRecording = enemyShooter.GetRecording();
                    Debug.Log($"[IterationManager] Stored enemy shooter recording: {_iteration2EnemyShooterRecording.Count} shots");
                }
            }

            StartIteration3();
        }

        void OnLiveEnemyDied()
        {
            if (_currentIteration != 2)
            {
                return;
            }

            Debug.Log("[IterationManager] Live enemy died in Iteration 2! Restarting Iteration 2...");

            // Unsubscribe to prevent issues
            if (_liveEnemy != null)
            {
                var enemyHealth = _liveEnemy.GetComponent<Health>();
                if (enemyHealth != null)
                {
                    enemyHealth.OnDeath -= OnLiveEnemyDied;
                }
            }

            // Use Invoke to delay restart - allows Health.Die() to finish SetActive(false) first
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
        }

        void StartIteration3()
        {
            Debug.Log("=== ITERATION 3: You control HERO (dodge ghost enemy) ===");
            _currentIteration = 3;

            // FIRST: Disable enemy input BEFORE doing anything else
            // This frees up the input devices for the hero
            if (_liveEnemy != null)
            {
                var enemyPlayerInput = _liveEnemy.GetComponent<UnityEngine.InputSystem.PlayerInput>();
                if (enemyPlayerInput != null)
                {
                    enemyPlayerInput.DeactivateInput();
                    enemyPlayerInput.enabled = false;
                    Debug.Log("[IterationManager] Enemy PlayerInput DISABLED FIRST");
                }

                var enemyPlayerController = _liveEnemy.GetComponent<PlayerController>();
                if (enemyPlayerController != null)
                {
                    enemyPlayerController.enabled = false;
                }
            }

            // Clean up ghost hero
            if (_ghostHero != null)
            {
                // Unsubscribe death handler to prevent callbacks
                var ghostHealth = _ghostHero.GetComponent<Health>();
                if (ghostHealth != null)
                {
                    ghostHealth.OnDeath -= OnGhostHeroDied;
                }

                // Disable collider immediately so no more hits can register
                var ghostCollider = _ghostHero.GetComponent<Collider2D>();
                if (ghostCollider != null)
                {
                    ghostCollider.enabled = false;
                }

                // Change tag so projectiles won't target it
                _ghostHero.tag = "Untagged";
                _ghostHero.SetActive(false);
                Destroy(_ghostHero);
                _ghostHero = null;
                Debug.Log("[IterationManager] Ghost hero fully cleaned up for Iteration 3");
            }

            // NOW activate hero
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
                // Unsubscribe any previous handler to prevent duplicates
                heroHealth.OnDeath -= OnHeroDiedInIteration3;
                heroHealth.OnDeath += OnHeroDiedInIteration3;
                Debug.Log("[IterationManager] Hero Health reset and OnDeath subscribed for Iteration 3");
            }
            else
            {
                Debug.LogError("[IterationManager] Hero has NO Health component in Iteration 3! Cannot be killed!");
            }

            // Ensure hero's collider is enabled so ghost enemy projectiles can hit
            var heroCollider = _liveHero.GetComponent<Collider2D>();
            if (heroCollider != null)
            {
                heroCollider.enabled = true;
                // Make sure hero collider is NOT a trigger - it needs to be solid to be hit by trigger projectiles
                heroCollider.isTrigger = false;
                Debug.Log($"[IterationManager] Hero collider enabled: {heroCollider.GetType().Name}, isTrigger={heroCollider.isTrigger}, layer={LayerMask.LayerToName(_liveHero.layer)}");
            }
            else
            {
                Debug.LogError("[IterationManager] Hero has NO Collider2D! Cannot be hit!");
            }

            // Ensure hero has a Rigidbody2D (required for collision detection)
            var heroRb = _liveHero.GetComponent<Rigidbody2D>();
            if (heroRb != null)
            {
                heroRb.bodyType = RigidbodyType2D.Dynamic;
                Debug.Log($"[IterationManager] Hero Rigidbody2D: bodyType={heroRb.bodyType}, simulated={heroRb.simulated}");
            }

            // Verify hero tag is correct
            _liveHero.tag = "Player";
            Debug.Log($"[IterationManager] Hero tag verified: '{_liveHero.tag}'");

            // Re-enable hero input for iteration 3 - enemy input is already disabled
            var heroInput = _liveHero.GetComponent<UnityEngine.InputSystem.PlayerInput>();
            if (heroInput != null)
            {
                heroInput.enabled = false; // Toggle off first
                heroInput.enabled = true;  // Then on to reclaim devices
                heroInput.ActivateInput();
                Debug.Log($"[IterationManager] Hero input re-enabled for Iteration 3. Hero: {_liveHero.name}");
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

            // Configure hero shooter to target enemies
            var heroShooter = _liveHero.GetComponent<ProjectileShooter>();
            if (heroShooter != null)
            {
                heroShooter.SetTargetTag("Enemy");
                heroShooter.StartRecording(); // Allow shooting in iteration 3
            }

            // Convert live enemy to ghost enemy for replay
            if (_liveEnemy != null && _iteration2EnemyRecording != null)
            {
                _ghostEnemy = _liveEnemy;
                _ghostEnemy.name = "GhostEnemy (Iteration 2 Replay)";
                _ghostEnemy.tag = "Enemy"; // Must have Enemy tag so hero projectiles can hit it
                _ghostEnemy.transform.localScale = new Vector3(0.15f, 0.15f, 1f);

                // Finish destroying the input components (already disabled above)
                var enemyPlayerInput = _ghostEnemy.GetComponent<UnityEngine.InputSystem.PlayerInput>();
                if (enemyPlayerInput != null)
                {
                    Destroy(enemyPlayerInput);
                    Debug.Log("[IterationManager] Ghost enemy PlayerInput scheduled for destroy");
                }

                var enemyPlayerController = _ghostEnemy.GetComponent<PlayerController>();
                if (enemyPlayerController != null)
                {
                    Destroy(enemyPlayerController);
                }

                var enemyRecorder = _ghostEnemy.GetComponent<MovementRecorder>();
                if (enemyRecorder != null)
                {
                    enemyRecorder.enabled = false;
                    Destroy(enemyRecorder);
                }

                var ghostEnemyRb = _ghostEnemy.GetComponent<Rigidbody2D>();
                if (ghostEnemyRb != null)
                {
                    ghostEnemyRb.bodyType = RigidbodyType2D.Kinematic;
                }

                // Ensure ghost enemy can be damaged by hero
                var ghostEnemyHealth = _ghostEnemy.GetComponent<Health>();
                if (ghostEnemyHealth != null)
                {
                    ghostEnemyHealth.ResetHealth();
                    // Subscribe to death - ghost enemy just disappears when killed (defensive kill by hero)
                    ghostEnemyHealth.OnDeath -= OnGhostEnemyDied;
                    ghostEnemyHealth.OnDeath += OnGhostEnemyDied;
                    Debug.Log("[IterationManager] Ghost enemy Health reset and OnDeath subscribed");
                }

                // Ensure ghost enemy collider is enabled for projectile hits
                var ghostEnemyCollider = _ghostEnemy.GetComponent<Collider2D>();
                if (ghostEnemyCollider != null)
                {
                    ghostEnemyCollider.enabled = true;
                }

                var ghostReplay = _ghostEnemy.AddComponent<GhostReplay>();
                ghostReplay.StartPlayback(_iteration2EnemyRecording);

                var ghostShooter = _ghostEnemy.GetComponent<ProjectileShooter>();
                if (ghostShooter != null && _iteration2EnemyShooterRecording != null)
                {
                    ghostShooter.SetTargetTag("Player"); // Ghost enemy targets hero
                    ghostShooter.StartReplay(_iteration2EnemyShooterRecording);
                    Debug.Log($"[IterationManager] Ghost enemy shooter replaying {_iteration2EnemyShooterRecording.Count} shots targeting 'Player'");
                }

                // Ghost keeps the same color as the original enemy
                // No color change needed - ghost uses original sprite colors

                _liveEnemy = null;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetState(GameState.HeroTurn);
            }
        }

        void OnHeroDiedInIteration3()
        {
            Debug.Log("[IterationManager] Hero died in Iteration 3! Restarting Iteration 3...");
            // Use Invoke to delay restart - this allows Health.Die() to finish SetActive(false) first
            // Then we restart and SetActive(true) properly
            Invoke(nameof(RestartIteration3), 0.1f);
        }

        void OnGhostEnemyDied()
        {
            if (_currentIteration != 3) return;

            Debug.Log("[IterationManager] Ghost enemy killed by hero in Iteration 3! Hero defended successfully.");
            // Ghost enemy disappears (handled by Health.Die() which deactivates the object)
            // Hero continues trying to reach the flag
            _ghostEnemy = null;
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

            // Create a new ghost enemy from the prefab
            if (enemyPrefab != null && _iteration2EnemyRecording != null)
            {
                _ghostEnemy = Instantiate(enemyPrefab, enemySpawnPoint.position, Quaternion.identity);
                _ghostEnemy.name = "GhostEnemy (Iteration 2 Replay)";
                _ghostEnemy.tag = "Enemy";
                _ghostEnemy.transform.localScale = new Vector3(0.15f, 0.15f, 1f);

                // Remove player control components
                var enemyPlayerInput = _ghostEnemy.GetComponent<UnityEngine.InputSystem.PlayerInput>();
                if (enemyPlayerInput != null)
                {
                    enemyPlayerInput.enabled = false;
                    Destroy(enemyPlayerInput);
                }

                var enemyPlayerController = _ghostEnemy.GetComponent<PlayerController>();
                if (enemyPlayerController != null)
                {
                    enemyPlayerController.enabled = false;
                    Destroy(enemyPlayerController);
                }

                var enemyRecorder = _ghostEnemy.GetComponent<MovementRecorder>();
                if (enemyRecorder != null)
                {
                    Destroy(enemyRecorder);
                }

                var ghostEnemyRb = _ghostEnemy.GetComponent<Rigidbody2D>();
                if (ghostEnemyRb != null)
                {
                    ghostEnemyRb.bodyType = RigidbodyType2D.Kinematic;
                }

                // Setup health and death handler
                var ghostEnemyHealth = _ghostEnemy.GetComponent<Health>();
                if (ghostEnemyHealth == null)
                {
                    ghostEnemyHealth = _ghostEnemy.AddComponent<Health>();
                }
                ghostEnemyHealth.ResetHealth();
                ghostEnemyHealth.OnDeath += OnGhostEnemyDied;

                // Enable collider
                var ghostEnemyCollider = _ghostEnemy.GetComponent<Collider2D>();
                if (ghostEnemyCollider != null)
                {
                    ghostEnemyCollider.enabled = true;
                }

                // Add ghost replay
                var ghostReplay = _ghostEnemy.AddComponent<GhostReplay>();
                ghostReplay.StartPlayback(_iteration2EnemyRecording);

                // Setup projectile replay
                var ghostShooter = _ghostEnemy.GetComponent<ProjectileShooter>();
                if (ghostShooter != null && _iteration2EnemyShooterRecording != null)
                {
                    ghostShooter.SetTargetTag("Player");
                    ghostShooter.StartReplay(_iteration2EnemyShooterRecording);
                }

                Debug.Log("[IterationManager] Ghost enemy created for Iteration 3 restart");
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetState(GameState.HeroTurn);
            }
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

            if (_liveEnemy != null)
            {
                var enemyHealth = _liveEnemy.GetComponent<Health>();
                if (enemyHealth != null)
                {
                    enemyHealth.OnDeath -= OnLiveEnemyDied;
                }
                _liveEnemy.SetActive(false);
                Destroy(_liveEnemy);
                _liveEnemy = null;
            }

            // Start Iteration 2 fresh with the same Iteration 1 recording
            StartIteration2();
        }

        /// <summary>
        /// Restart Iteration 3 when hero dies.
        /// Ghost enemy replays from beginning, hero respawns.
        /// </summary>
        void RestartIteration3()
        {
            Debug.Log("[IterationManager] === RESTARTING ITERATION 3 ===");

            // Clean up current ghost enemy
            if (_ghostEnemy != null)
            {
                var ghostHealth = _ghostEnemy.GetComponent<Health>();
                if (ghostHealth != null)
                {
                    ghostHealth.OnDeath -= OnGhostEnemyDied;
                }
                _ghostEnemy.tag = "Untagged";
                _ghostEnemy.SetActive(false);
                Destroy(_ghostEnemy);
                _ghostEnemy = null;
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

            if (_liveEnemy != null)
            {
                _liveEnemy.SetActive(false);
                Destroy(_liveEnemy);
                _liveEnemy = null;
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
                // Disable and untag so it won't be found by tag searches
                _ghostHero.tag = "Untagged";
                _ghostHero.SetActive(false);
                Destroy(_ghostHero);
                _ghostHero = null;
            }

            if (_ghostEnemy != null)
            {
                _ghostEnemy.tag = "Untagged";
                _ghostEnemy.SetActive(false);
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
            _iteration1HeroShooterRecording?.Clear();
            _iteration2EnemyRecording?.Clear();
            _iteration2EnemyShooterRecording?.Clear();
        }
    }
}
