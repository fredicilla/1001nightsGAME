using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BossFight
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Settings")]
        public float turnDuration = 15f;

        [Header("References")]
        public GameObject playerPrefab;
        public GameObject monsterPrefab;
        public GameObject ghostPrefab;
        public Transform spawnPoint;
        public LevelModifier levelModifier;

        [Header("Camera")]
        private ThirdPersonCamera thirdPersonCamera;

        [Header("State")]
        public TurnType currentTurn = TurnType.HeroTurn;
        public GameState currentState = GameState.Playing;
        public int turnNumber = 1;

        private GameObject currentPlayer;
        private List<GameObject> activeGhosts = new List<GameObject>();
        private List<List<RecordedFrame>> allRecordings = new List<List<RecordedFrame>>();
        private List<WishType> selectedWishes = new List<WishType>();

        private float turnStartTime;
        private bool hasKey = false;
        private bool requiresKey = false;

        [Header("Freeze Start")]
        private bool waitingForInput = false;
        private bool turnStarted = false;

        public bool HasKey { get => hasKey; set => hasKey = value; }
        public bool RequiresKey { get => requiresKey; set => requiresKey = value; }
        public float TimeRemaining => waitingForInput ? turnDuration : Mathf.Max(0, turnDuration - (Time.time - turnStartTime));
        public bool IsWaitingForInput => waitingForInput;
        public GameObject CurrentPlayer => currentPlayer;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            Debug.Log("🎮 GameManager Started!");
            Debug.Log($"📍 Spawn Point: {(spawnPoint != null ? spawnPoint.position.ToString() : "NULL")}");
            Debug.Log($"👤 Player Prefab (ALAA): {(playerPrefab != null ? playerPrefab.name : "NULL")}");
            Debug.Log($"👹 Monster Prefab (AI alaa dev): {(monsterPrefab != null ? monsterPrefab.name : "NULL")}");
            Debug.Log($"👻 Ghost Prefab: {(ghostPrefab != null ? ghostPrefab.name : "NULL")}");

            // Find camera
            thirdPersonCamera = FindFirstObjectByType<ThirdPersonCamera>();
            if (thirdPersonCamera != null)
            {
                Debug.Log("📹 ThirdPersonCamera found!");
            }
            else
            {
                Debug.LogWarning("⚠️ ThirdPersonCamera not found in scene!");
            }
            StartTurn(TurnType.HeroTurn, 1);
        }

        private void Update()
        {
            // Check for input to start the turn
            if (waitingForInput)
            {
                // Any input = start the turn! (using NEW Input System)
                bool anyInput = Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame;
                bool mouseInput = Mouse.current != null && (Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame);

                if (anyInput || mouseInput)
                {
                    StartTurnTimer();
                }
                return; // Don't check timeout while waiting
            }

            if (currentState != GameState.Playing) return;

            if (currentTurn != TurnType.GenieChoice)
            {
                if (TimeRemaining <= 0)
                {
                    OnTimeOut();
                }
            }
        }

        private void StartTurnTimer()
        {
            waitingForInput = false;
            turnStarted = true;
            turnStartTime = Time.time;

            Debug.Log($"⏱️ Turn started! Timer begins NOW! ({turnDuration}s)");

            // Enable player movement
            if (currentPlayer != null)
            {
                PlayerController playerController = currentPlayer.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    playerController.IsActive = true;
                }
            }

            // Start ghost playback
            foreach (GameObject ghost in activeGhosts)
            {
                if (ghost != null)
                {
                    GhostController ghostController = ghost.GetComponent<GhostController>();
                    if (ghostController != null && !ghostController.IsPlaying)
                    {
                        Debug.Log("👻 Starting ghost playback NOW!");
                        // Ghost already has recording, just unpause it
                    }
                }
            }
        }

        public void StartTurn(TurnType turn, int number)
        {
            currentTurn = turn;
            turnNumber = number;
            currentState = GameState.Playing;
            hasKey = false;

            // Enable freeze start for Turn 1, Turn 2, and Turn 5
            if (turn == TurnType.HeroTurn || turn == TurnType.MonsterTurn || turn == TurnType.SecondMonsterTurn)
            {
                waitingForInput = true;
                turnStarted = false;
                Debug.Log($"⏸️ Turn {number} ({turn}) ready! Press ANY KEY to start...");
            }
            else
            {
                waitingForInput = false;
                turnStarted = true;
                turnStartTime = Time.time;
            }

            ClearActiveEntities();

            // Reapply all selected wishes for the new turn
            if (turn == TurnType.HeroTurn || turn == TurnType.MonsterTurn || turn == TurnType.SecondMonsterTurn)
            {
                ReapplyAllWishes();
            }

            UIManager uiManager = FindFirstObjectByType<UIManager>();
            if (uiManager != null)
            {
                uiManager.UpdateTurnInfo(turnNumber, turn);
            }

            switch (turn)
            {
                case TurnType.HeroTurn:
                    StartHeroTurn();
                    break;
                case TurnType.MonsterTurn:
                    StartMonsterTurn();
                    break;
                case TurnType.GenieChoice:
                    StartGenieChoice();
                    break;
                case TurnType.SecondMonsterTurn:
                    StartSecondMonsterTurn();
                    break;
            }
        }

        private void StartHeroTurn()
        {
            Debug.Log($"🏃 StartHeroTurn() - Turn {turnNumber}, allRecordings.Count = {allRecordings.Count}");

            SpawnPlayer("Player");

            // Disable player at start (frozen until input)
            if (currentPlayer != null)
            {
                PlayerController playerController = currentPlayer.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    playerController.IsActive = false;
                    Debug.Log("🎮 Player frozen! Waiting for input...");
                }
            }

            RecordingManager recorder = currentPlayer.GetComponent<RecordingManager>();
            if (recorder != null)
            {
                recorder.StartRecording();
            }

            // Turn 4: Spawn Monster1 Ghost (from Turn 2)
            if (turnNumber == 4)
            {
                Debug.Log($"🔍 Turn 4: Spawning Monster1 Ghost from Turn 2...");

                if (allRecordings.Count >= 2)
                {
                    Debug.Log("👹 Spawning Monster1 Ghost from Turn 2...");
                    SpawnGhost(allRecordings[1], "Monster", Vector3.right * 3f);
                }
                else
                {
                    Debug.LogError($"❌ Cannot spawn Monster1 Ghost! allRecordings.Count = {allRecordings.Count}");
                }
            }
            // Turn 6: Spawn Monster1 Ghost (Turn 2) + Monster2 Ghost (Turn 5)
            else if (turnNumber == 6)
            {
                Debug.Log($"🔍 Turn 6 (Final): Spawning 2 Monster Ghosts...");

                // Monster1 Ghost من Turn 2
                if (allRecordings.Count >= 2)
                {
                    Debug.Log("👹 Spawning Monster1 Ghost from Turn 2...");
                    SpawnGhost(allRecordings[1], "Monster", Vector3.right * 3f);
                }
                else
                {
                    Debug.LogError($"❌ Cannot spawn Monster1 Ghost! allRecordings.Count = {allRecordings.Count}");
                }

                // Monster2 Ghost من Turn 5
                if (allRecordings.Count >= 4)
                {
                    Debug.Log("👹👹 Spawning Monster2 Ghost from Turn 5 (recording 3)...");
                    SpawnGhost(allRecordings[3], "Monster", Vector3.right * 6f);
                }
                else
                {
                    Debug.LogError($"❌ Cannot spawn Monster2 Ghost! allRecordings.Count = {allRecordings.Count} (need at least 4)");
                }
            }
            else
            {
                Debug.Log($"📋 Turn {turnNumber}: No Monster Ghosts");
            }
        }

        private void StartMonsterTurn()
        {
            Debug.Log("👹 StartMonsterTurn called!");

            // Spawn Monster (AI alaa dev) at spawn point + offset to avoid collision with Ghost!
            SpawnMonster("Monster", Vector3.right * 3f);

            // Disable monster at start (frozen until input)
            if (currentPlayer != null)
            {
                PlayerController playerController = currentPlayer.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    playerController.IsActive = false;
                    Debug.Log("🎮 Monster frozen! Waiting for input...");
                }
            }

            RecordingManager recorder = currentPlayer.GetComponent<RecordingManager>();
            if (recorder != null)
            {
                recorder.StartRecording();
            }

            Debug.Log($"📼 Total recordings available: {allRecordings.Count}");

            if (allRecordings.Count > 0)
            {
                Debug.Log($"📼 Recording 0 has {allRecordings[allRecordings.Count - 1].Count} frames");
                Debug.Log("👻 Spawning Ghost (ALAA) with last recording...");
                SpawnGhost(allRecordings[allRecordings.Count - 1], "Player");
            }
            else
            {
                Debug.LogError("❌ No recordings available to spawn Ghost!");
            }
        }

        private void StartGenieChoice()
        {
            Debug.Log("🧞 StartGenieChoice() - Showing Genie Panel...");

            UIManager uiManager = FindFirstObjectByType<UIManager>();
            if (uiManager != null)
            {
                uiManager.ShowGeniePanel();
            }
            else
            {
                Debug.LogError("❌ UIManager not found!");
            }
        }

        private void StartSecondMonsterTurn()
        {
            Debug.Log("👹👹 StartSecondMonsterTurn (Turn 5) called!");

            // Spawn Monster2 (الوحش الثاني الجديد) - أنت تلعبه
            SpawnMonster("Monster", Vector3.right * 6f);

            // Disable monster at start (frozen until input)
            if (currentPlayer != null)
            {
                PlayerController playerController = currentPlayer.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    playerController.IsActive = false;
                    Debug.Log("🎮 Monster2 frozen! Waiting for input...");
                }
            }

            RecordingManager recorder = currentPlayer.GetComponent<RecordingManager>();
            if (recorder != null)
            {
                recorder.StartRecording();
            }

            Debug.Log($"📼 Total recordings available: {allRecordings.Count}");

            // Spawn Ghost 1: Hero من Turn 4 (allRecordings[2])
            if (allRecordings.Count >= 3)
            {
                Debug.Log("👻 Spawning Hero Ghost from Turn 4 (recording 2)...");
                SpawnGhost(allRecordings[2], "Player");
            }
            else
            {
                Debug.LogError("❌ No Hero recording from Turn 4!");
            }

            // Spawn Ghost 2: Monster1 من Turn 4 (allRecordings[2] - لكن هذا خطأ!)
            // في الحقيقة Monster1 Ghost يجب أن يكون من allRecordings[1] (Turn 2)
            // لكن في Turn 4 المفروض Monster Ghost كان موجود ويتحرك
            // إذن نحتاج نفس Monster من Turn 2
            if (allRecordings.Count >= 2)
            {
                Debug.Log("👹 Spawning Monster1 Ghost from Turn 2 (recording 1)...");
                SpawnGhost(allRecordings[1], "Monster", Vector3.right * 3f);
            }
            else
            {
                Debug.LogError("❌ No Monster recording from Turn 2!");
            }
        }

        private void SpawnPlayer(string tag, Vector3 positionOffset = default)
        {
            if (playerPrefab == null || spawnPoint == null) return;

            Vector3 spawnPosition = spawnPoint.position + positionOffset;
            Debug.Log($"🎭 Spawning Player ({tag}) at {spawnPosition} (offset: {positionOffset})");

            currentPlayer = Instantiate(playerPrefab, spawnPosition, spawnPoint.rotation);
            currentPlayer.tag = tag;

            if (tag == "Monster")
            {
                DeathZone deathZone = currentPlayer.AddComponent<DeathZone>();
            }

            // Update camera to follow new player
            UpdateCameraTarget();
        }

        private void SpawnMonster(string tag, Vector3 positionOffset = default)
        {
            if (monsterPrefab == null || spawnPoint == null)
            {
                Debug.LogWarning("⚠️ MonsterPrefab not set! Using PlayerPrefab instead.");
                SpawnPlayer(tag, positionOffset);
                return;
            }

            Vector3 spawnPosition = spawnPoint.position + positionOffset;
            Debug.Log($"👹 Spawning Monster (AI alaa dev) at {spawnPosition} (offset: {positionOffset})");

            currentPlayer = Instantiate(monsterPrefab, spawnPosition, spawnPoint.rotation);
            currentPlayer.tag = tag;

            DeathZone deathZone = currentPlayer.AddComponent<DeathZone>();

            // Update camera to follow new monster
            UpdateCameraTarget();
        }

        private void UpdateCameraTarget()
        {
            if (thirdPersonCamera != null && currentPlayer != null)
            {
                thirdPersonCamera.SetTarget(currentPlayer.transform);
                Debug.Log($"📹 Camera now following: {currentPlayer.name}");
            }
        }

        private void SpawnGhost(List<RecordedFrame> recording, string tag)
        {
            SpawnGhost(recording, tag, Vector3.zero);
        }

        private void SpawnGhost(List<RecordedFrame> recording, string tag, Vector3 positionOffset)
        {
            // اختر Prefab المناسب بناءً على Tag
            GameObject prefabToUse = (tag == "Monster") ? monsterPrefab : playerPrefab;

            if (prefabToUse == null || spawnPoint == null)
            {
                Debug.LogError($"❌ {tag} prefab or spawn point is NULL!");
                return;
            }

            Vector3 spawnPosition = spawnPoint.position + positionOffset;
            string prefabName = (tag == "Monster") ? "MonsterComplete" : "PlayerComplete";
            Debug.Log($"👻 Spawning Ghost (from {prefabName}) with tag '{tag}' at {spawnPosition} (offset: {positionOffset})");

            GameObject ghost = Instantiate(prefabToUse, spawnPosition, spawnPoint.rotation);
            ghost.name = $"Ghost_{tag}";
            ghost.tag = tag;

            PlayerController playerController = ghost.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.enabled = false;
                Debug.Log("✅ PlayerController disabled on Ghost");
            }

            PlayerInput playerInput = ghost.GetComponent<PlayerInput>();
            if (playerInput != null)
            {
                playerInput.enabled = false;
                Debug.Log("✅ PlayerInput disabled on Ghost");
            }

            RecordingManager recordingManager = ghost.GetComponent<RecordingManager>();
            if (recordingManager != null)
            {
                recordingManager.enabled = false;
                Debug.Log("✅ RecordingManager disabled on Ghost");
            }

            Rigidbody rb = ghost.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
                Debug.Log("✅ Rigidbody set to Kinematic");
            }

            PlayerAnimationController animController = ghost.GetComponent<PlayerAnimationController>();
            if (animController != null)
            {
                Debug.Log("✅ PlayerAnimationController found on Ghost - KEEPING IT ENABLED");
            }
            else
            {
                Debug.LogWarning("⚠️ PlayerAnimationController NOT found on Ghost!");
            }

            Animator animator = ghost.GetComponentInChildren<Animator>();
            if (animator != null)
            {
                Debug.Log($"✅ Animator found on Ghost at: {animator.gameObject.name}, enabled={animator.enabled}");
                if (animator.runtimeAnimatorController != null)
                {
                    Debug.Log($"✅ Animator Controller: {animator.runtimeAnimatorController.name}");
                }
                else
                {
                    Debug.LogError("❌ Animator has NO RuntimeAnimatorController!");
                }
            }
            else
            {
                Debug.LogError("❌ Animator NOT found on Ghost!");
            }

            GhostController ghostController = ghost.AddComponent<GhostController>();

            ghostController.shootForce = 15f;
            Transform shootPoint = ghost.transform.Find("ShootPoint");
            if (shootPoint != null)
            {
                ghostController.shootPoint = shootPoint;
            }

            if (playerController != null && playerController.applePrefab != null)
            {
                ghostController.applePrefab = playerController.applePrefab;
            }

            Debug.Log($"📼 Starting Ghost playback with {recording.Count} frames...");
            ghostController.StartPlayback(recording);
            Debug.Log($"📼 Ghost playback started! IsPlaying: {ghostController.IsPlaying}");

            // Disable FallDetector on Ghost (it should not die from falling)
            FallDetector fallDetector = ghost.GetComponent<FallDetector>();
            if (fallDetector != null)
            {
                fallDetector.enabled = false;
                Debug.Log("✅ FallDetector disabled on Ghost");
            }

            // Don't add DeathZone to Ghost! It kills Players and Ghost is tagged as Player
            // if (tag == "Player")
            // {
            //     DeathZone deathZone = ghost.AddComponent<DeathZone>();
            // }

            activeGhosts.Add(ghost);
            Debug.Log($"✅ Ghost (PlayerComplete) spawned successfully!");
        }

        private void SpawnPreviousGhosts()
        {
            for (int i = 0; i < allRecordings.Count; i++)
            {
                string tag = (i % 2 == 0) ? "Player" : "Monster";
                SpawnGhost(allRecordings[i], tag);
            }
        }

        private void ClearActiveEntities()
        {
            if (currentPlayer != null)
            {
                Destroy(currentPlayer);
                currentPlayer = null;
            }

            foreach (GameObject ghost in activeGhosts)
            {
                if (ghost != null) Destroy(ghost);
            }
            activeGhosts.Clear();
        }

        public void OnGoalReached()
        {
            Debug.Log($"🎯 OnGoalReached called! Turn: {currentTurn}, TurnNumber: {turnNumber}");

            if (currentState != GameState.Playing)
            {
                Debug.Log("⚠️ State is not Playing, ignoring goal...");
                return;
            }

            currentState = GameState.Success;
            SaveCurrentRecording();

            Debug.Log($"✅ Recording saved! Total recordings: {allRecordings.Count}");

            if (currentTurn == TurnType.HeroTurn)
            {
                if (turnNumber == 1)
                {
                    Debug.Log("🔄 Turn 1 complete! Starting Turn 2 (Monster)...");
                    StartTurn(TurnType.MonsterTurn, 2);
                }
                else if (turnNumber == 4)
                {
                    Debug.Log("🎉 Turn 4 complete! Starting Turn 5 (Second Monster)...");
                    StartTurn(TurnType.SecondMonsterTurn, 5);
                }
                else if (turnNumber == 6)
                {
                    Debug.Log("🎉🎉🎉 Turn 6 complete! FINAL VICTORY! 🎉🎉🎉");
                    UIManager uiManager = FindFirstObjectByType<UIManager>();
                    if (uiManager != null)
                    {
                        uiManager.ShowVictoryPanel();
                    }
                }
                else
                {
                    Debug.Log($"🎉 Victory! Game complete after Turn {turnNumber}!");
                    UIManager uiManager = FindFirstObjectByType<UIManager>();
                    if (uiManager != null)
                    {
                        uiManager.ShowVictoryPanel();
                    }
                }
            }
        }

        public void OnMonsterFailed()
        {
            if (currentState != GameState.Playing) return;

            currentState = GameState.Failed;

            Debug.Log("❌ Monster failed! Ghost reached goal. Restarting Turn 2...");

            RestartCurrentTurn();
        }

        public void OnSecondMonsterFailed()
        {
            if (currentState != GameState.Playing) return;

            currentState = GameState.Failed;

            Debug.Log("❌ Monster2 failed! Hero Ghost reached goal. Restarting Turn 5...");

            RestartCurrentTurn();
        }

        public void OnHeroGhostKilled()
        {
            if (currentState != GameState.Playing) return;

            currentState = GameState.Success;

            Debug.Log("✅ Monster2 killed Hero Ghost! Monster2 SUCCESS!");

            // Save Monster2 recording
            SaveCurrentRecording();
            Debug.Log($"📼 Monster2 recording saved! Total recordings: {allRecordings.Count}");

            Debug.Log("🎉 Turn 5 complete! Starting Turn 6 (Final Hero Turn)...");
            StartTurn(TurnType.HeroTurn, 6);
        }

        public void OnPlayerDeath()
        {
            if (currentState != GameState.Playing) return;

            currentState = GameState.Failed;

            Debug.Log($"☠️ Player death in {currentTurn}! Restarting same turn...");

            // Restart the SAME turn (not from Turn 1!)
            RestartCurrentTurn();
        }

        public void RestartCurrentTurn()
        {
            Debug.Log($"🔄 Restarting {currentTurn}, Turn {turnNumber}");

            // حذف اللاعب الحالي
            if (currentPlayer != null)
            {
                Destroy(currentPlayer);
                currentPlayer = null;
            }

            // حذف الأشباح
            foreach (GameObject ghost in activeGhosts)
            {
                if (ghost != null) Destroy(ghost);
            }
            activeGhosts.Clear();

            // إعادة ضبط الحالة
            currentState = GameState.Playing;
            hasKey = false;

            // Restart the same turn
            StartTurn(currentTurn, turnNumber);

            Debug.Log($"✅ Turn {turnNumber} ({currentTurn}) restarted!");
        }

        public void OnGhostKilled()
        {
            if (currentState != GameState.Playing) return;

            currentState = GameState.Success;

            Debug.Log("✅ Monster killed Ghost! Monster SUCCESS!");

            // حفظ تسجيل Monster للاستخدام في Turn 4!
            SaveCurrentRecording();
            Debug.Log($"📼 Monster recording saved! Total recordings: {allRecordings.Count}");

            Debug.Log("🧞 Monster won Turn 2! Starting Genie Choice...");
            StartTurn(TurnType.GenieChoice, turnNumber);
        }

        public void OnTimeOut()
        {
            if (currentState != GameState.Playing) return;

            currentState = GameState.TimeOut;

            Debug.Log("⏰ Time's up! Restarting current turn...");

            UIManager uiManager = FindFirstObjectByType<UIManager>();
            if (uiManager != null)
            {
                uiManager.ShowGameOverPanel("انتهى الوقت!");
            }

            // بعد ثانيتين، أعد نفس المرحلة
            Invoke(nameof(RestartCurrentTurn), 2f);
        }

        public void OnWishSelected(WishType wish)
        {
            Debug.Log($"🎯 GameManager.OnWishSelected({wish}) called!");
            Debug.Log($"✅ Wish selected: {wish}");

            // Add to selected wishes list
            if (!selectedWishes.Contains(wish))
            {
                selectedWishes.Add(wish);
                Debug.Log($"📝 Saved wish: {wish}. Total wishes: {selectedWishes.Count}");
            }

            // Apply the wish immediately
            if (levelModifier != null)
            {
                Debug.Log($"🧞 Applying wish: {wish}");
                levelModifier.ApplyWish(wish);
            }
            else
            {
                Debug.LogError("❌ LevelModifier not found!");
            }

            // Hide Genie Panel
            Debug.Log("🚫 About to hide GeniePanel...");
            UIManager uiManager = FindFirstObjectByType<UIManager>();
            if (uiManager != null)
            {
                Debug.Log("✓ UIManager found, calling HideGeniePanel()");
                uiManager.HideGeniePanel();
            }
            else
            {
                Debug.LogError("❌ UIManager not found!");
            }

            // Start next turn (Hero turn with the new wish active!)
            Debug.Log($"🎮 Starting Turn {turnNumber + 1} (Hero) with {selectedWishes.Count} active wish(es)!");
            StartTurn(TurnType.HeroTurn, turnNumber + 1);
        }

        private void ReapplyAllWishes()
        {
            if (selectedWishes.Count == 0)
            {
                Debug.Log("📋 No wishes to reapply.");
                return;
            }

            Debug.Log($"🔄 Reapplying {selectedWishes.Count} wish(es): {string.Join(", ", selectedWishes)}");

            if (levelModifier != null)
            {
                // Reset first
                levelModifier.ResetAll();

                // Reapply all selected wishes
                foreach (WishType wish in selectedWishes)
                {
                    levelModifier.ApplyWish(wish);
                }

                Debug.Log("✅ All wishes reapplied successfully!");
            }
            else
            {
                Debug.LogError("❌ LevelModifier not found!");
            }
        }

        public void CollectKey()
        {
            hasKey = true;
            Debug.Log("🔑 Key collected! Goal is now unlocked!");

            if (levelModifier != null)
            {
                // Swap locked goal with normal goal
                if (levelModifier.normalGoal != null) levelModifier.normalGoal.SetActive(true);
                if (levelModifier.lockedGoal != null) levelModifier.lockedGoal.SetActive(false);

                Debug.Log("🔓 Goal unlocked!");
            }
        }

        private void SaveCurrentRecording()
        {
            Debug.Log("💾 SaveCurrentRecording called!");

            if (currentPlayer == null)
            {
                Debug.LogError("❌ Current player is NULL!");
                return;
            }

            RecordingManager recorder = currentPlayer.GetComponent<RecordingManager>();
            if (recorder != null && recorder.IsRecording)
            {
                recorder.StopRecording();
                List<RecordedFrame> recording = recorder.GetRecording();
                allRecordings.Add(recording);
                Debug.Log($"✅ Recording saved! Frames: {recording.Count}, Total recordings: {allRecordings.Count}");
            }
            else
            {
                Debug.LogError($"❌ RecordingManager problem! Recorder null: {recorder == null}, IsRecording: {(recorder != null ? recorder.IsRecording.ToString() : "N/A")}");
            }
        }

        public void RestartLevel()
        {
            Debug.Log("🔄 Restarting level...");
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
            );
        }

        public void RewindToTurn(int targetTurn)
        {
            if (targetTurn < 1 || targetTurn > allRecordings.Count) return;

            allRecordings.RemoveRange(targetTurn, allRecordings.Count - targetTurn);

            TurnType turn = (targetTurn % 2 == 1) ? TurnType.HeroTurn : TurnType.MonsterTurn;
            StartTurn(turn, targetTurn);
        }
    }
}
