using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using GeniesGambit.Player;
using GeniesGambit.Enemies;
using GeniesGambit.Combat;

namespace GeniesGambit.Core
{
    // ─── Iteration definition table ──────────────────────────────────────────
    // Each entry describes ONE iteration: who the player controls, what ghosts
    // replay, what gets recorded, and what happens after the goal is met.
    // To add iterations 8, 9, … just append more entries to the list.
    // ─────────────────────────────────────────────────────────────────────────

    public enum IterationRole { Hero, Enemy1, Enemy2 }

    public enum PostGoalAction
    {
        None,             // proceed to next iteration immediately
        ShowWishGenie,    // show wish screen, then proceed
        TriggerBossScene  // game ends, load boss fight
    }

    [System.Serializable]
    public class IterationDef
    {
        public int number;
        public IterationRole role;
        public bool recordHero;
        public bool recordEnemy1;
        public bool recordEnemy2;
        public bool replayHero;         // replay latest hero recording as ghost
        public bool replayEnemy1;       // replay enemy-1 recording as ghost
        public bool replayEnemy2;       // replay enemy-2 recording as ghost
        public PostGoalAction postGoal;

        public IterationDef(int num, IterationRole role,
            bool recH, bool recE1, bool recE2,
            bool repH, bool repE1, bool repE2,
            PostGoalAction post)
        {
            number = num;
            this.role = role;
            recordHero = recH; recordEnemy1 = recE1; recordEnemy2 = recE2;
            replayHero = repH; replayEnemy1 = repE1; replayEnemy2 = repE2;
            postGoal = post;
        }
    }

    // ─── Main manager ────────────────────────────────────────────────────────

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

        [Header("Boss Scene")]
        [SerializeField] string bossIntroVideoSceneName = "BossIntroVideo";
        [SerializeField] string bossSceneName = "BossFight";
        [SerializeField] float bossSceneDelay = 3f;

        [Header("Victory")]
        [SerializeField] GameObject victoryScreen;

        // ── Iteration sequence (scalable — add more entries for 8, 9, …) ────
        static readonly List<IterationDef> _sequence = new List<IterationDef>
        {
            //                              rec              replay
            //  #  role                    H   E1     E2    H   E1  E2   postGoal
            new(1, IterationRole.Hero,   true, false,false, false,false,false, PostGoalAction.None),
            new(2, IterationRole.Enemy1, false,true, false, true, false,false, PostGoalAction.ShowWishGenie),
            new(3, IterationRole.Hero,   true, false,false, false,true, false, PostGoalAction.None),
            new(4, IterationRole.Enemy2, false,false,true,  true, true, false, PostGoalAction.ShowWishGenie),
            new(5, IterationRole.Hero,   false,false,false, false,true, true,  PostGoalAction.ShowWishGenie),
            new(6, IterationRole.Hero,   false,false,false, false,true, true,  PostGoalAction.ShowWishGenie),
            new(7, IterationRole.Hero,   false,false,false, false,true, true,  PostGoalAction.TriggerBossScene),
        };

        // ── Runtime state ───────────────────────────────────────────────────
        int _iterIndex = -1;  // index into _sequence
        IterationDef _def;    // shorthand for current definition

        // Live objects
        GameObject _liveHero;
        GameObject _liveEnemy;   // the enemy the player is currently controlling

        // Ghost objects
        GameObject _ghostHero;
        GameObject _ghostEnemy1;
        GameObject _ghostEnemy2;

        // Recordings (persist across iterations)
        List<FrameData> _heroRecording;
        List<ShootEvent> _heroShooterRecording;
        List<FrameData> _enemy1Recording;
        List<ShootEvent> _enemy1ShooterRecording;
        List<FrameData> _enemy2Recording;
        List<ShootEvent> _enemy2ShooterRecording;

        // Source iteration for each stored recording (0 = none)
        int _heroRecordingIteration;
        int _enemy1RecordingIteration;
        int _enemy2RecordingIteration;

        // Keep hero recordings by source iteration (e.g. iter1 hero path and iter3 hero path)
        readonly Dictionary<int, List<FrameData>> _heroRecordingsByIteration = new();
        readonly Dictionary<int, List<ShootEvent>> _heroShooterRecordingsByIteration = new();

        // Component caches
        MovementRecorder _activeRecorder;
        ProjectileShooter _activeShooter;
        IterationTimer _iterationTimer;

        // ── Public API ──────────────────────────────────────────────────────
        public int CurrentIteration => _def?.number ?? 0;
        public int TotalIterations => _sequence.Count;
        public IterationDef CurrentDef => _def;
        public bool CanRewind => _iterIndex > 0;

        // ─────────────────────────────────────────────────────────────────────
        // Lifecycle
        // ─────────────────────────────────────────────────────────────────────

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            _iterationTimer = GetComponent<IterationTimer>();
            if (_iterationTimer != null)
                _iterationTimer.OnTimerExpired.AddListener(OnIterationTimerExpired);
        }

        /// <summary>Called by RoundManager.Start() (or any bootstrap) to kick off the game.</summary>
        public void BeginGame()
        {
            Debug.Log("[IterationManager] ═══ BEGINNING 7-ITERATION GAME ═══");
            CancelInvoke();

            if (GameManager.Instance != null)
                GameManager.Instance.SetCurrentIteration(0);

            _heroRecording = null;
            _heroShooterRecording = null;
            _enemy1Recording = null;
            _enemy1ShooterRecording = null;
            _enemy2Recording = null;
            _enemy2ShooterRecording = null;
            _heroRecordingIteration = 0;
            _enemy1RecordingIteration = 0;
            _enemy2RecordingIteration = 0;
            _heroRecordingsByIteration.Clear();
            _heroShooterRecordingsByIteration.Clear();

            _iterIndex = -1;
            NotifyProgressionUIChanged();
            StartNextIteration();
        }

        /// <summary>Called by GenieManager after the player picks a wish.</summary>
        public void OnWishApplied()
        {
            Debug.Log("[IterationManager] Wish applied — advancing to next iteration.");
            NotifyProgressionUIChanged();
            StartNextIteration();
        }

        // ─────────────────────────────────────────────────────────────────────
        // Iteration lifecycle
        // ─────────────────────────────────────────────────────────────────────

        void StartNextIteration()
        {
            _iterIndex++;
            if (_iterIndex >= _sequence.Count)
            {
                Debug.LogError("[IterationManager] Tried to start iteration beyond sequence length!");
                return;
            }

            _def = _sequence[_iterIndex];

            if (GameManager.Instance != null)
                GameManager.Instance.SetCurrentIteration(_def.number);

            // Select the latest hero recording that happened BEFORE this iteration.
            // This is crucial when rewinding from iter4->3->2: iter2 must use iter1 hero record.
            SyncHeroRecordingCacheForIteration(_def.number);

            // Safety guard: if an iteration requires replay data that does not exist,
            // timeline is inconsistent (usually from rewind edge cases). Rebuild from start.
            if (!HasRequiredReplayData(_def, out string missingReason))
            {
                Debug.LogWarning($"[IterationManager] Missing replay data for Iteration {_def.number}: {missingReason}. Resetting iterations to rebuild timeline.");
                ResetIterations();
                return;
            }

            Debug.Log($"╔══════════════════════════════════════╗");
            Debug.Log($"║  ITERATION {_def.number} / {_sequence.Count}  —  {_def.role}");
            Debug.Log($"╚══════════════════════════════════════╝");

            // Pause overlay
            if (GameStartPauseManager.Instance != null)
                GameStartPauseManager.Instance.PauseGameAndShowOverlay();

            CleanupAll();

            switch (_def.role)
            {
                case IterationRole.Hero:
                    SetupHeroIteration();
                    break;
                case IterationRole.Enemy1:
                    SetupEnemyIteration(enemy1SpawnPoint);
                    break;
                case IterationRole.Enemy2:
                    SetupEnemyIteration(enemy2SpawnPoint);
                    break;
            }

            // Spawn ghost replays
            if (_def.replayHero)  SpawnGhostHero();
            if (_def.replayEnemy1) SpawnGhostEnemy1();
            if (_def.replayEnemy2) SpawnGhostEnemy2();

            // Timer
            if (_iterationTimer != null)
                _iterationTimer.StartTimer();

            NotifyProgressionUIChanged();
        }

        // ── Restart (death / timer expired / ghost reached flag) ────────────

        public void RestartCurrentIteration()
        {
            Debug.Log($"[IterationManager] ─── RESTARTING Iteration {_def.number} ───");
            CancelInvoke();
            if (_iterationTimer != null) _iterationTimer.StopTimer();

            // Re-run the same index
            _iterIndex--;
            StartNextIteration();
        }

        void DelayedRestart() => RestartCurrentIteration();

        /// <summary>Rewind to a specific earlier iteration, clearing recordings from that point forward.</summary>
        public void RewindToIteration(int targetIteration)
        {
            if (targetIteration < 1 || targetIteration > _sequence.Count)
            {
                Debug.LogError($"[IterationManager] Invalid rewind target: {targetIteration}");
                return;
            }

            Debug.Log($"[IterationManager] ═══ REWINDING TO ITERATION {targetIteration} ═══");
            CancelInvoke();
            if (_iterationTimer != null) _iterationTimer.StopTimer();

            // If rewind is triggered while wish UI is still open, close it to prevent
            // stale clicks from calling OnWishApplied for an old iteration context.
            if (GeniesGambit.Genie.GenieManager.Instance != null)
                GeniesGambit.Genie.GenieManager.Instance.CancelWishSelection();

            var iterationUI = FindFirstObjectByType<GeniesGambit.UI.IterationUI>();
            if (iterationUI != null) iterationUI.ShowRewindBanner(targetIteration);

            // Clear only recordings that were ACTUALLY produced at or after the target.
            // Example: rewinding from Iteration 3 to 2 must keep hero recording from Iteration 1.
            if (_heroRecordingIteration >= targetIteration)
            {
                _heroRecording = null;
                _heroShooterRecording = null;
                _heroRecordingIteration = 0;
            }

            // Remove hero recordings produced at or after rewind target, then select
            // the best remaining hero recording for the target iteration context.
            var heroKeysToRemove = new List<int>();
            foreach (var kvp in _heroRecordingsByIteration)
            {
                if (kvp.Key >= targetIteration) heroKeysToRemove.Add(kvp.Key);
            }
            foreach (var key in heroKeysToRemove)
            {
                _heroRecordingsByIteration.Remove(key);
                _heroShooterRecordingsByIteration.Remove(key);
            }
            SyncHeroRecordingCacheForIteration(targetIteration);

            if (_enemy1RecordingIteration >= targetIteration)
            {
                _enemy1Recording = null;
                _enemy1ShooterRecording = null;
                _enemy1RecordingIteration = 0;
            }

            if (_enemy2RecordingIteration >= targetIteration)
            {
                _enemy2Recording = null;
                _enemy2ShooterRecording = null;
                _enemy2RecordingIteration = 0;
            }

            // Rewind wishes chosen at or after the target iteration
            if (GeniesGambit.Genie.GenieManager.Instance != null)
                GeniesGambit.Genie.GenieManager.Instance.RewindWishesAfterIteration(targetIteration);

            CleanupAll();

            // Jump to the iteration just before the target so StartNextIteration advances to it
            _iterIndex = targetIteration - 2;  // -1 because StartNextIteration does _iterIndex++
            StartNextIteration();
            NotifyProgressionUIChanged();
        }

        /// <summary>Full reset — clear all recordings and restart from Iteration 1.</summary>
        public void ResetIterations()
        {
            Debug.Log("[IterationManager] ═══ RESETTING ALL ITERATIONS ═══");
            CancelInvoke();
            if (_iterationTimer != null) _iterationTimer.StopTimer();

            _heroRecording = null;
            _heroShooterRecording = null;
            _enemy1Recording = null;
            _enemy1ShooterRecording = null;
            _enemy2Recording = null;
            _enemy2ShooterRecording = null;

            CleanupAll();

            _iterIndex = -1;
            StartNextIteration();
        }

        // ─────────────────────────────────────────────────────────────────────
        // Setup helpers
        // ─────────────────────────────────────────────────────────────────────

        void SetupHeroIteration()
        {
            EnsureLiveHero();
            _liveHero.SetActive(true);
            _liveHero.transform.localScale = new Vector3(0.15f, 0.15f, 1f);

            if (heroSpawnPoint != null)
                _liveHero.transform.position = heroSpawnPoint.position;

            // Reset physics — SpikeDamage sets bodyType to Kinematic on death,
            // and residual velocity can carry over between iterations
            var rb = _liveHero.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.linearVelocity = Vector2.zero;
            }

            // Health
            var hp = _liveHero.GetComponent<Health>();
            if (hp != null)
            {
                hp.ResetHealth();
                hp.OnDeath -= OnLiveHeroDied;
                hp.OnDeath += OnLiveHeroDied;
            }

            // Collectibles
            GeniesGambit.Level.KeyCollectible.ResetKey();
            GeniesGambit.Level.CoinCollectible.ResetCoins();

            // Input
            var pi = _liveHero.GetComponent<UnityEngine.InputSystem.PlayerInput>();
            if (pi != null) { pi.enabled = true; pi.ActivateInput(); }

            var pc = _liveHero.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.enabled = true;
                pc.ResetFallGuard();
                // Sync PlayerController's cached start position so RespawnAtStart uses heroSpawnPoint
                if (heroSpawnPoint != null) pc.SetStartPosition(heroSpawnPoint.position);
            }

            // Recording
            if (_def.recordHero)
            {
                _activeRecorder = _liveHero.GetComponent<MovementRecorder>();
                if (_activeRecorder == null) _activeRecorder = _liveHero.AddComponent<MovementRecorder>();
                _activeRecorder.StartRecording();

                _activeShooter = _liveHero.GetComponent<ProjectileShooter>();
                if (_activeShooter != null)
                {
                    _activeShooter.SetTargetTag("Enemy");
                    _activeShooter.StartRecording();
                }
            }
            else
            {
                // Not recording — still set up shooter for gameplay (mouse shooting)
                var shooter = _liveHero.GetComponent<ProjectileShooter>();
                if (shooter != null)
                {
                    shooter.SetTargetTag("Enemy");
                    shooter.EnableShooting(true, false);
                }
                _activeRecorder = null;
                _activeShooter = null;
            }

            if (GameManager.Instance != null)
                GameManager.Instance.SetState(GameState.HeroTurn);

            // Brief invulnerability so stray projectiles can't kill hero during setup
            SetHeroInvulnerableTemporarily();
        }

        void SetupEnemyIteration(Transform spawnPoint)
        {
            // Deactivate live hero
            if (_liveHero != null)
            {
                UnsubHeroDeath();
                var heroInput = _liveHero.GetComponent<UnityEngine.InputSystem.PlayerInput>();
                if (heroInput != null) heroInput.DeactivateInput();
                _liveHero.SetActive(false);
            }

            // Collectibles and key gate state should be refreshed every iteration,
            // not only hero iterations (prevents key wish appearing to "disappear").
            GeniesGambit.Level.KeyCollectible.ResetKey();
            GeniesGambit.Level.CoinCollectible.ResetCoins();

            // Spawn live enemy
            _liveEnemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
            _liveEnemy.name = _def.role == IterationRole.Enemy1
                ? "LiveEnemy1 (Player Controlled)"
                : "LiveEnemy2 (Player Controlled)";
            _liveEnemy.tag = "Enemy";
            _liveEnemy.transform.localScale = new Vector3(0.15f, 0.15f, 1f);
            Debug.Log($"[IterationManager] Spawned {_liveEnemy.name} at {spawnPoint.position}");

            // Health
            var hp = _liveEnemy.GetComponent<Health>();
            if (hp == null) hp = _liveEnemy.AddComponent<Health>();
            hp.ResetHealth();
            hp.OnDeath -= OnLiveEnemyDied;
            hp.OnDeath += OnLiveEnemyDied;

            // Collider
            var col = _liveEnemy.GetComponent<Collider2D>();
            if (col != null) col.enabled = true;
            else
            {
                var cc = _liveEnemy.AddComponent<CapsuleCollider2D>();
                cc.size = new Vector2(1f, 2f);
            }

            // Input
            var pi = _liveEnemy.GetComponent<UnityEngine.InputSystem.PlayerInput>();
            if (pi != null) { pi.enabled = true; }

            var pc = _liveEnemy.GetComponent<PlayerController>();
            if (pc != null) { pc.enabled = true; }

            // Recording
            bool shouldRecord = (_def.role == IterationRole.Enemy1 && _def.recordEnemy1)
                             || (_def.role == IterationRole.Enemy2 && _def.recordEnemy2);
            if (shouldRecord)
            {
                _activeRecorder = _liveEnemy.GetComponent<MovementRecorder>();
                if (_activeRecorder == null) _activeRecorder = _liveEnemy.AddComponent<MovementRecorder>();
                _activeRecorder.StartRecording();

                _activeShooter = _liveEnemy.GetComponent<ProjectileShooter>();
                if (_activeShooter != null)
                {
                    _activeShooter.SetTargetTag("Player");
                    _activeShooter.StartRecording();
                }
            }

            if (GameManager.Instance != null)
                GameManager.Instance.SetState(GameState.MonsterTurn);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Ghost spawning
        // ─────────────────────────────────────────────────────────────────────

        void SpawnGhostHero()
        {
            // Ensure cache points to the correct historical hero recording for this iteration.
            SyncHeroRecordingCacheForIteration(_def.number);

            if (_heroRecording == null || _heroRecording.Count == 0)
            {
                Debug.LogWarning($"[IterationManager] No hero recording to replay in Iteration {_def.number}. Skipping ghost hero spawn.");
                return;
            }

            _ghostHero = Instantiate(heroPrefab, heroSpawnPoint.position, Quaternion.identity);
            _ghostHero.name = "GhostHero (Replay)";
            _ghostHero.tag = "Player";
            _ghostHero.transform.localScale = new Vector3(0.15f, 0.15f, 1f);
            StripLiveComponents(_ghostHero);

            var hp = _ghostHero.GetComponent<Health>();
            if (hp != null)
            {
                hp.ResetHealth();
                hp.OnDeath += OnGhostHeroDied;
            }

            var col = _ghostHero.GetComponent<Collider2D>();
            if (col != null) col.enabled = true;

            var replay = _ghostHero.AddComponent<GhostReplay>();
            replay.StartPlayback(_heroRecording);

            var shooter = _ghostHero.GetComponent<ProjectileShooter>();
            if (shooter != null && _heroShooterRecording != null)
            {
                shooter.SetTargetTag("Enemy");
                shooter.StartReplay(_heroShooterRecording);
            }

            Debug.Log($"[IterationManager] Ghost hero spawned ({_heroRecording.Count} frames)");
        }

        void SpawnGhostEnemy1()
        {
            if (_enemy1Recording == null || _enemy1Recording.Count == 0)
            {
                Debug.LogWarning("[IterationManager] No enemy-1 recording to replay!");
                return;
            }

            _ghostEnemy1 = Instantiate(enemyPrefab, enemy1SpawnPoint.position, Quaternion.identity);
            _ghostEnemy1.name = "GhostEnemy1 (Replay)";
            _ghostEnemy1.tag = "Enemy";
            _ghostEnemy1.transform.localScale = new Vector3(0.15f, 0.15f, 1f);
            StripLiveComponents(_ghostEnemy1);

            var hp = _ghostEnemy1.GetComponent<Health>();
            if (hp == null) hp = _ghostEnemy1.AddComponent<Health>();
            hp.ResetHealth();
            hp.OnDeath += OnGhostEnemy1Died;

            var col = _ghostEnemy1.GetComponent<Collider2D>();
            if (col != null) col.enabled = true;

            var replay = _ghostEnemy1.AddComponent<GhostReplay>();
            replay.StartPlayback(_enemy1Recording);

            var shooter = _ghostEnemy1.GetComponent<ProjectileShooter>();
            if (shooter != null && _enemy1ShooterRecording != null)
            {
                shooter.SetTargetTag("Player");
                shooter.StartReplay(_enemy1ShooterRecording);
            }

            Debug.Log($"[IterationManager] Ghost enemy #1 spawned ({_enemy1Recording.Count} frames)");
        }

        void SpawnGhostEnemy2()
        {
            if (_enemy2Recording == null || _enemy2Recording.Count == 0)
            {
                Debug.LogWarning("[IterationManager] No enemy-2 recording to replay!");
                return;
            }

            _ghostEnemy2 = Instantiate(enemyPrefab, enemy2SpawnPoint.position, Quaternion.identity);
            _ghostEnemy2.name = "GhostEnemy2 (Replay)";
            _ghostEnemy2.tag = "Enemy";
            _ghostEnemy2.transform.localScale = new Vector3(0.15f, 0.15f, 1f);
            StripLiveComponents(_ghostEnemy2);

            var hp = _ghostEnemy2.GetComponent<Health>();
            if (hp == null) hp = _ghostEnemy2.AddComponent<Health>();
            hp.ResetHealth();
            hp.OnDeath += OnGhostEnemy2Died;

            var col = _ghostEnemy2.GetComponent<Collider2D>();
            if (col != null) col.enabled = true;

            var replay = _ghostEnemy2.AddComponent<GhostReplay>();
            replay.StartPlayback(_enemy2Recording);

            var shooter = _ghostEnemy2.GetComponent<ProjectileShooter>();
            if (shooter != null && _enemy2ShooterRecording != null)
            {
                shooter.SetTargetTag("Player");
                shooter.StartReplay(_enemy2ShooterRecording);
            }

            Debug.Log($"[IterationManager] Ghost enemy #2 spawned ({_enemy2Recording.Count} frames)");
        }

        // ─────────────────────────────────────────────────────────────────────
        // Goal reached callbacks (called by FlagTrigger)
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>Hero (live) reached the gate. Valid in hero iterations.</summary>
        public void OnHeroReachedGate()
        {
            if (_def.role != IterationRole.Hero)
            {
                Debug.LogWarning($"[IterationManager] OnHeroReachedGate called but role is {_def.role}!");
                return;
            }

            Debug.Log($"[IterationManager] Hero reached gate in Iteration {_def.number}!");
            if (_iterationTimer != null) _iterationTimer.StopTimer();

            StoreRecordings();
            HandlePostGoal();
        }

        /// <summary>Ghost hero reached the gate — enemy failed to stop it. Restart.</summary>
        public void OnGhostReachedFlag()
        {
            Debug.Log($"[IterationManager] Ghost hero reached gate in Iteration {_def.number}! Enemy failed — restarting.");
            DestroyAllProjectiles();
            StopAllGhostReplays();
            ResetAllChasingMonsters();
            Invoke(nameof(DelayedRestart), 0.1f);
        }

        /// <summary>Enemy killed the ghost hero. Valid in enemy iterations.</summary>
        void OnGhostHeroDied()
        {
            if (_def.role != IterationRole.Enemy1 && _def.role != IterationRole.Enemy2)
                return;

            Debug.Log($"[IterationManager] Ghost hero killed in Iteration {_def.number}! Enemy wins.");
            if (_iterationTimer != null) _iterationTimer.StopTimer();

            StoreRecordings();
            HandlePostGoal();
        }

        // ─────────────────────────────────────────────────────────────────────
        // Death callbacks
        // ─────────────────────────────────────────────────────────────────────

        void OnLiveHeroDied()
        {
            Debug.Log($"[IterationManager] Hero died in Iteration {_def.number}!");

            // Always destroy projectiles immediately to prevent re-kill on respawn
            DestroyAllProjectiles();

            if (_def.recordHero)
            {
                // Recording iteration — restart recording from scratch.
                // IMPORTANT: We must defer this to next frame because Health.Die() calls
                // SetActive(false) AFTER invoking OnDeath. If we call ResetHeroForRerecord
                // synchronously here, its SetActive(true) gets immediately undone by Die().
                Debug.Log("[IterationManager] Deferring hero re-record reset to next frame...");
                StartCoroutine(DeferredResetHeroForRerecord());
            }
            else
            {
                // Non-recording hero iteration (5, 6, 7) — restart iteration.
                // DelayedRestart already uses Invoke with a delay, so Die()'s SetActive(false)
                // runs first, then RestartCurrentIteration re-activates the hero properly.
                StopAllGhostReplays();
                ResetAllChasingMonsters();
                Invoke(nameof(DelayedRestart), 0.1f);
            }
        }

        bool HasRequiredReplayData(IterationDef def, out string reason)
        {
            if (def.replayHero && GetLatestHeroRecordingIterationBefore(def.number) == 0)
            {
                reason = "Hero replay requested but hero recording is empty";
                return false;
            }

            if (def.replayEnemy1 && (_enemy1Recording == null || _enemy1Recording.Count == 0))
            {
                reason = "Enemy1 replay requested but enemy1 recording is empty";
                return false;
            }

            if (def.replayEnemy2 && (_enemy2Recording == null || _enemy2Recording.Count == 0))
            {
                reason = "Enemy2 replay requested but enemy2 recording is empty";
                return false;
            }

            reason = null;
            return true;
        }

        int GetLatestHeroRecordingIterationBefore(int iterationExclusive)
        {
            int best = 0;
            foreach (var kvp in _heroRecordingsByIteration)
            {
                if (kvp.Key < iterationExclusive && kvp.Value != null && kvp.Value.Count > 0 && kvp.Key > best)
                    best = kvp.Key;
            }
            return best;
        }

        void SyncHeroRecordingCacheForIteration(int iterationExclusive)
        {
            int best = GetLatestHeroRecordingIterationBefore(iterationExclusive);
            if (best == 0)
            {
                _heroRecording = null;
                _heroShooterRecording = null;
                _heroRecordingIteration = 0;
                return;
            }

            _heroRecordingIteration = best;
            _heroRecording = _heroRecordingsByIteration[best];
            _heroShooterRecordingsByIteration.TryGetValue(best, out _heroShooterRecording);
        }

        System.Collections.IEnumerator DeferredResetHeroForRerecord()
        {
            // Wait one frame so Health.Die() finishes calling SetActive(false)
            yield return null;
            ResetHeroForRerecord();
        }

        void OnLiveEnemyDied()
        {
            Debug.Log($"[IterationManager] Live enemy died in Iteration {_def.number}! Restarting...");
            if (_iterationTimer != null) _iterationTimer.StopTimer();
            DestroyAllProjectiles();
            StopAllGhostReplays();
            ResetAllChasingMonsters();
            Invoke(nameof(DelayedRestart), 0.1f);
        }

        void OnGhostEnemy1Died()
        {
            Debug.Log("[IterationManager] Ghost enemy #1 killed!");
            _ghostEnemy1 = null;
        }

        void OnGhostEnemy2Died()
        {
            Debug.Log("[IterationManager] Ghost enemy #2 killed!");
            _ghostEnemy2 = null;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Recording storage
        // ─────────────────────────────────────────────────────────────────────

        void StoreRecordings()
        {
            if (_activeRecorder != null)
            {
                _activeRecorder.StopRecording();
                var frames = _activeRecorder.GetRecording();

                if (_def.recordHero)
                {
                    _heroRecording = frames;
                    _heroRecordingIteration = _def.number;
                    _heroRecordingsByIteration[_def.number] = frames;
                    Debug.Log($"[IterationManager] Stored hero recording: {frames.Count} frames");
                }
                else if (_def.recordEnemy1)
                {
                    _enemy1Recording = frames;
                    _enemy1RecordingIteration = _def.number;
                    Debug.Log($"[IterationManager] Stored enemy-1 recording: {frames.Count} frames");
                }
                else if (_def.recordEnemy2)
                {
                    _enemy2Recording = frames;
                    _enemy2RecordingIteration = _def.number;
                    Debug.Log($"[IterationManager] Stored enemy-2 recording: {frames.Count} frames");
                }
            }

            if (_activeShooter != null)
            {
                _activeShooter.StopRecording();
                var shots = _activeShooter.GetRecording();

                if (_def.recordHero)
                {
                    _heroShooterRecording = shots;
                    _heroShooterRecordingsByIteration[_def.number] = shots;
                    Debug.Log($"[IterationManager] Stored hero shooter recording: {shots.Count} shots");
                }
                else if (_def.recordEnemy1)
                {
                    _enemy1ShooterRecording = shots;
                    Debug.Log($"[IterationManager] Stored enemy-1 shooter recording: {shots.Count} shots");
                }
                else if (_def.recordEnemy2)
                {
                    _enemy2ShooterRecording = shots;
                    Debug.Log($"[IterationManager] Stored enemy-2 shooter recording: {shots.Count} shots");
                }
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // Post-goal flow
        // ─────────────────────────────────────────────────────────────────────

        void HandlePostGoal()
        {
            switch (_def.postGoal)
            {
                case PostGoalAction.None:
                    StartNextIteration();
                    break;

                case PostGoalAction.ShowWishGenie:
                    Debug.Log("[IterationManager] Showing Wish Genie screen...");
                    if (GameManager.Instance != null)
                        GameManager.Instance.SetState(GameState.GenieWishScreen);
                    // GenieManager listens for this state and will call OnWishApplied()
                    // when the player picks a wish, which resumes the loop.
                    NotifyProgressionUIChanged();
                    break;

                case PostGoalAction.TriggerBossScene:
                    Debug.Log("[IterationManager] All 7 iterations complete! Triggering Boss Scene...");
                    CleanupAll();

                    if (GameData.Instance != null)
                        GameData.Instance.RoundsCompleted = _sequence.Count;

                    AudioManager.Play(AudioManager.SoundID.GameWin);

                    if (victoryScreen != null)
                        victoryScreen.SetActive(true);

                    if (GameManager.Instance != null)
                        GameManager.Instance.SetState(GameState.BossScene);

                    NotifyProgressionUIChanged();

                    Invoke(nameof(LoadBossScene), bossSceneDelay);
                    break;
            }
        }

        void NotifyProgressionUIChanged()
        {
            var iterationUI = FindFirstObjectByType<GeniesGambit.UI.IterationUI>();
            if (iterationUI != null)
                iterationUI.UpdateProgressionUI();
        }

        void LoadBossScene()
        {
            Debug.Log($"[IterationManager] Loading Boss Intro Video...");
            SceneManager.LoadScene(bossIntroVideoSceneName);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Timer expired
        // ─────────────────────────────────────────────────────────────────────

        void OnIterationTimerExpired()
        {
            Debug.Log($"[IterationManager] Timer expired in Iteration {_def.number}! Restarting...");
            DestroyAllProjectiles();
            StopAllGhostReplays();
            ResetAllChasingMonsters();
            Invoke(nameof(DelayedRestart), 0.1f);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Hero re-record (death during recording iteration)
        // ─────────────────────────────────────────────────────────────────────

        void ResetHeroForRerecord()
        {
            if (_liveHero == null)
            {
                Debug.LogError("[IterationManager] ResetHeroForRerecord: _liveHero is NULL!");
                return;
            }

            Debug.Log($"[IterationManager] ResetHeroForRerecord: hero active BEFORE={_liveHero.activeSelf}");

            // Re-activate the GameObject (Health.Die() called SetActive(false) after OnDeath)
            _liveHero.SetActive(true);

            Debug.Log($"[IterationManager] ResetHeroForRerecord: hero active AFTER SetActive(true)={_liveHero.activeSelf}");

            // Position & velocity
            if (heroSpawnPoint != null)
                _liveHero.transform.position = heroSpawnPoint.position;

            // Reset physics — SpikeDamage sets bodyType to Kinematic on death
            var rb = _liveHero.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.linearVelocity = Vector2.zero;
            }

            // Health (also re-enables collider and clears _isDead)
            var hp = _liveHero.GetComponent<Health>();
            if (hp != null)
            {
                hp.ResetHealth();
                // Re-subscribe OnDeath so the hero can die again
                hp.OnDeath -= OnLiveHeroDied;
                hp.OnDeath += OnLiveHeroDied;
            }

            // Re-enable input (Health/death may have disabled these)
            var pi = _liveHero.GetComponent<UnityEngine.InputSystem.PlayerInput>();
            if (pi != null) { pi.enabled = true; pi.ActivateInput(); }

            var pc = _liveHero.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.enabled = true;
                pc.ResetFallGuard();
                if (heroSpawnPoint != null) pc.SetStartPosition(heroSpawnPoint.position);
            }

            // Re-enable scale (in case anything changed it)
            _liveHero.transform.localScale = new Vector3(0.15f, 0.15f, 1f);

            // Collectibles
            GeniesGambit.Level.KeyCollectible.ResetKey();
            GeniesGambit.Level.CoinCollectible.ResetCoins();

            // Destroy any in-flight projectiles and reset monsters
            DestroyAllProjectiles();
            ResetAllChasingMonsters();

            // Restart ghost replays so they sync with the new recording attempt
            StopAllGhostReplays();
            DestroyTagged(ref _ghostHero);
            DestroyTagged(ref _ghostEnemy1);
            DestroyTagged(ref _ghostEnemy2);
            if (_def.replayHero)   SpawnGhostHero();
            if (_def.replayEnemy1) SpawnGhostEnemy1();
            if (_def.replayEnemy2) SpawnGhostEnemy2();

            // Restart recording and shooter
            if (_activeRecorder != null) _activeRecorder.StartRecording();
            if (_activeShooter != null) _activeShooter.StartRecording();

            // Restart timer
            if (_iterationTimer != null) _iterationTimer.StartTimer();

            // Brief invulnerability so stray damage can't kill hero mid-setup
            SetHeroInvulnerableTemporarily();

            Debug.Log("[IterationManager] Hero recording restarted from scratch after death.");
        }

        // ─────────────────────────────────────────────────────────────────────
        // Utility
        // ─────────────────────────────────────────────────────────────────────

        void EnsureLiveHero()
        {
            // Cached reference is still valid (works even if hero is deactivated by Health.Die)
            if (_liveHero != null) return;

            // Fallback: search scene for the Player tag (only finds active objects)
            _liveHero = GameObject.FindGameObjectWithTag("Player");

            // Make sure we have the real hero, not a ghost
            if (_liveHero != null && _liveHero.name.Contains("Ghost"))
            {
                var allPlayers = GameObject.FindGameObjectsWithTag("Player");
                foreach (var p in allPlayers)
                {
                    if (!p.name.Contains("Ghost"))
                    {
                        _liveHero = p;
                        break;
                    }
                }
            }

            if (_liveHero == null)
            {
                Debug.LogError("[IterationManager] No Player GameObject found!");
            }
        }

        /// <summary>Remove PlayerController, PlayerInput, MovementRecorder, set RB kinematic.</summary>
        void StripLiveComponents(GameObject ghost)
        {
            var pc = ghost.GetComponent<PlayerController>();
            if (pc != null) { pc.enabled = false; Destroy(pc); }

            var pi = ghost.GetComponent<UnityEngine.InputSystem.PlayerInput>();
            if (pi != null) { pi.enabled = false; Destroy(pi); }

            var rec = ghost.GetComponent<MovementRecorder>();
            if (rec != null) { rec.enabled = false; Destroy(rec); }

            var rb = ghost.GetComponent<Rigidbody2D>();
            if (rb != null) rb.bodyType = RigidbodyType2D.Kinematic;
        }

        void UnsubHeroDeath()
        {
            if (_liveHero == null) return;
            var hp = _liveHero.GetComponent<Health>();
            if (hp != null) hp.OnDeath -= OnLiveHeroDied;
        }

        void StopAllGhostReplays()
        {
            StopGhostReplay(_ghostHero);
            StopGhostReplay(_ghostEnemy1);
            StopGhostReplay(_ghostEnemy2);
        }

        void StopGhostReplay(GameObject ghost)
        {
            if (ghost == null) return;
            var replay = ghost.GetComponent<GhostReplay>();
            if (replay != null) replay.Stop();
        }

        void CleanupAll()
        {
            // Destroy all in-flight projectiles so they don't hit the hero on respawn
            DestroyAllProjectiles();

            // Reset chasing monsters to their spawn positions
            ResetAllChasingMonsters();

            // Ghosts
            DestroyTagged(ref _ghostHero);
            DestroyTagged(ref _ghostEnemy1);
            DestroyTagged(ref _ghostEnemy2);

            // Live enemy (spawned each enemy iteration)
            if (_liveEnemy != null)
            {
                var hp = _liveEnemy.GetComponent<Health>();
                if (hp != null) hp.OnDeath -= OnLiveEnemyDied;
                _liveEnemy.SetActive(false);
                Destroy(_liveEnemy);
                _liveEnemy = null;
            }

            // Unsub hero death (hero persists, just unsub)
            UnsubHeroDeath();
        }

        void DestroyTagged(ref GameObject obj)
        {
            if (obj == null) return;
            obj.tag = "Untagged";
            obj.SetActive(false);
            Destroy(obj);
            obj = null;
        }

        /// <summary>Destroy every Projectile currently in the scene (prevents re-kill on respawn).</summary>
        void DestroyAllProjectiles()
        {
            var projectiles = FindObjectsByType<Projectile>(FindObjectsSortMode.None);
            foreach (var p in projectiles)
            {
                if (p != null) Destroy(p.gameObject);
            }
            if (projectiles.Length > 0)
                Debug.Log($"[IterationManager] Destroyed {projectiles.Length} in-flight projectile(s)");
        }

        /// <summary>Reset all ChasingMonsters in the scene to their spawn positions.</summary>
        void ResetAllChasingMonsters()
        {
            var monsters = FindObjectsByType<ChasingMonster>(FindObjectsSortMode.None);
            foreach (var m in monsters)
            {
                if (m != null) m.RespawnGhostPublic();
            }
        }

        /// <summary>Make hero briefly invulnerable so stray damage can't kill it mid-setup.</summary>
        void SetHeroInvulnerableTemporarily()
        {
            if (_liveHero == null) return;
            var hp = _liveHero.GetComponent<Health>();
            if (hp == null) return;
            hp.SetInvulnerable(true);
            StartCoroutine(EndInvulnerability(hp, 0.3f));
        }

        System.Collections.IEnumerator EndInvulnerability(Health hp, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (hp != null) hp.SetInvulnerable(false);
        }

        void OnDestroy()
        {
            CleanupAll();
            _heroRecording?.Clear();
            _heroShooterRecording?.Clear();
            _enemy1Recording?.Clear();
            _enemy1ShooterRecording?.Clear();
            _enemy2Recording?.Clear();
            _enemy2ShooterRecording?.Clear();
        }

        // ─────────────────────────────────────────────────────────────────────
        // Restart game (called from UI)
        // ─────────────────────────────────────────────────────────────────────

        public void RestartGame()
        {
            Debug.Log("[IterationManager] Restarting entire game...");
            CleanupAll();

            if (GeniesGambit.Genie.GenieManager.Instance != null)
                GeniesGambit.Genie.GenieManager.Instance.ResetAllWishes();

            if (GeniesGambit.Level.KeyMechanicManager.Instance != null)
                GeniesGambit.Level.KeyMechanicManager.Instance.ResetKeyMechanic();

            BeginGame();
        }
    }
}
