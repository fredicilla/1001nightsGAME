using System.Collections;
using UnityEngine;

public enum GenieBehaviorState
{
    CirclingPlayer,
    ChasingPlayer,
    IdleWandering,
    Shooting
}

public class GenieBossController : MonoBehaviour
{
    [Header("Boss Settings")]
    public int maxHealth = 5;
    public int currentHealth = 5;
    public int currentPhase = 1;
    public bool isVulnerable = false;
    public bool isAttacking = false;
    
    [Header("Prefabs")]
    public GameObject genieProjectilePrefab;
    public GameObject monsterPrefab;
    public GameObject spikePrefab;
    public GameObject keyPrefab;
    
    [Header("References")]
    public Transform shootPoint;
    public Transform arenaCenter;
    
    [Header("Flying Settings")]
    public float flyHeight = 0.5f;
    public float flySpeed = 3f;
    public float circleRadius = 8f;
    public bool followPlayer = true;
    public float followDistance = 8f;
    public float halfCircleDuration = 2f;
    public float minDistanceFromPlayer = 6f;
    
    private bool isMovingClockwise = true;
    private float movementTimer = 0f;
    
    [Header("Ground Walking Settings")]
    public bool canWalkOnGround = true;
    public float walkSpeed = 2f;
    public float flyDuration = 15f;
    public float walkDuration = 10f;
    
    [Header("AI Behavior Settings")]
    public float behaviorSwitchMinTime = 3f;
    public float behaviorSwitchMaxTime = 7f;
    public float chaseDistance = 12f;
    public float wanderRadius = 8f;
    
    private GenieBehaviorState currentBehaviorState = GenieBehaviorState.CirclingPlayer;
    private float behaviorTimer = 0f;
    private Vector3 wanderTarget;
    
    private bool isFlying = true;
    private float stateTimer = 0f;
    
    [Header("Attack Settings")]
    public float projectileSpeed = 8f;
    public float timeBetweenShots = 1f;
    public float vulnerableWindowDuration = 3f;
    public float autoShootInterval = 10f;
    public int autoShootCount = 1;
    
    [Header("Monster Spawn Settings")]
    public int monstersPerWave = 5;
    public float monsterSpawnInterval = 20f;
    
    [Header("Animation Settings")]
    public float introDuration = 4f;
    public float deathAnimationDuration = 3f;
    
    [Header("Cinemachine Camera References")]
    public GameObject genieIntroCamera;
    public GameObject playerCamera;
    
    private int aliveMonsters = 0;
    private float circleAngle = 0f;
    private HealthSystem healthSystem;
    private Animator genieAnimator;
    private bool hasPlayedIntro = false;
    
    private void Start()
    {
        currentHealth = maxHealth;
        currentPhase = 1;
        isVulnerable = true;
        healthSystem = GetComponent<HealthSystem>();
        
        genieAnimator = GetComponent<Animator>();
        
        if (genieAnimator == null)
        {
            Debug.LogError("⚠️ Animator component not found on Genie!");
            genieAnimator = GetComponentInChildren<Animator>();
            if (genieAnimator != null)
            {
                Debug.Log("✅ Found Animator in children instead");
            }
        }
        else
        {
            Debug.Log("✅ Genie Animator found! Controller: " + (genieAnimator.runtimeAnimatorController != null ? genieAnimator.runtimeAnimatorController.name : "NULL"));
        }
        
        if (healthSystem != null)
        {
            healthSystem.maxHealth = maxHealth;
        }
        
        Debug.Log("🎮 Boss Fight Started!");
        
        SpawnMonsters(monstersPerWave);
        
        isFlying = true;
        stateTimer = flyDuration;
        
        currentBehaviorState = GenieBehaviorState.CirclingPlayer;
        behaviorTimer = Random.Range(behaviorSwitchMinTime, behaviorSwitchMaxTime);
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            wanderTarget = GetRandomPositionNearPlayer(player.transform.position);
        }
        
        if (!hasPlayedIntro)
        {
            Debug.Log("🎬 Starting intro sequence...");
            StartCoroutine(PlayIntroSequence());
        }
        else
        {
            Debug.Log("⏭️ Skipping intro (already played)");
            StartCoroutine(AutoShootLoop());
            StartCoroutine(ContinuousMonsterSpawnLoop());
        }
    }
    
    IEnumerator PlayIntroSequence()
    {
        hasPlayedIntro = true;
        
        Debug.Log("🎬 Playing intro sequence...");
        
        if (genieIntroCamera != null)
        {
            Debug.Log("📹 Activating Genie Intro Camera");
            genieIntroCamera.SetActive(true);
        }
        else
        {
            Debug.LogWarning("⚠️ Genie Intro Camera is NULL - no camera zoom will happen");
        }
        
        if (playerCamera != null)
        {
            Debug.Log("📹 Deactivating Player Camera");
            playerCamera.SetActive(false);
        }
        
        if (genieAnimator != null)
        {
            Debug.Log("🎭 Triggering 'Intro' animation on Genie");
            genieAnimator.SetTrigger("Intro");
            
            // تحقق من وجود الـ parameter
            if (genieAnimator.parameters.Length > 0)
            {
                Debug.Log($"📋 Animator has {genieAnimator.parameters.Length} parameters");
                bool hasIntroParam = false;
                foreach (var param in genieAnimator.parameters)
                {
                    if (param.name == "Intro")
                    {
                        hasIntroParam = true;
                        Debug.Log("✅ Found 'Intro' parameter in Animator!");
                        break;
                    }
                }
                if (!hasIntroParam)
                {
                    Debug.LogError("❌ 'Intro' parameter NOT FOUND in Animator! Please add it.");
                }
            }
            else
            {
                Debug.LogError("❌ Animator has NO parameters! Please add them in Animator Controller.");
            }
        }
        else
        {
            Debug.LogError("❌ genieAnimator is NULL! Cannot play intro animation!");
        }
        
        Debug.Log($"⏱️ Waiting {introDuration} seconds for intro to complete...");
        yield return new WaitForSeconds(introDuration);
        
        if (genieIntroCamera != null)
        {
            Debug.Log("📹 Deactivating Genie Intro Camera");
            genieIntroCamera.SetActive(false);
        }
        
        if (playerCamera != null)
        {
            Debug.Log("📹 Activating Player Camera");
            playerCamera.SetActive(true);
        }
        
        Debug.Log("✅ Intro complete! Starting battle...");
        
        StartCoroutine(AutoShootLoop());
        StartCoroutine(ContinuousMonsterSpawnLoop());
    }
    
    IEnumerator MainBattleLoop()
    {
        while (currentHealth > 0)
        {
            Debug.Log($"🔄 Battle Loop: HP {currentHealth}/{maxHealth}");
            
            yield return StartCoroutine(SummonMonstersPhase());
            
            if (currentHealth <= 0) break;
            
            yield return StartCoroutine(ShootingPhase());
            
            if (currentHealth <= 0) break;
            
            yield return StartCoroutine(MovingPhase());
        }
    }
    
    IEnumerator AutoShootLoop()
    {
        yield return new WaitForSeconds(autoShootInterval);
        
        while (currentHealth > 0)
        {
            Debug.Log($"⏰ Auto-shoot triggered! Shooting {autoShootCount} projectiles (Every {autoShootInterval} seconds)");
            
            for (int i = 0; i < autoShootCount; i++)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null && genieProjectilePrefab != null && shootPoint != null)
                {
                    if (genieAnimator != null)
                    {
                        genieAnimator.SetTrigger("Shoot");
                    }
                    
                    Vector3 targetPosition = player.transform.position;
                    GameObject projectile = Instantiate(genieProjectilePrefab, shootPoint.position, Quaternion.identity);
                    GenieProjectile projScript = projectile.GetComponent<GenieProjectile>();
                    
                    if (projScript != null)
                    {
                        projScript.Initialize(targetPosition, projectileSpeed, gameObject);
                    }
                    
                    Debug.Log($"🔮 Genie auto-shot projectile {i + 1}/{autoShootCount}");
                }
                
                yield return new WaitForSeconds(timeBetweenShots);
            }
            
            yield return new WaitForSeconds(autoShootInterval);
        }
    }
    
    IEnumerator ContinuousMonsterSpawnLoop()
    {
        yield return new WaitForSeconds(monsterSpawnInterval);
        
        while (currentHealth > 0)
        {
            Debug.Log($"👹 Spawning new wave of {monstersPerWave} monsters (Every {monsterSpawnInterval} seconds)");
            
            SpawnMonsters(monstersPerWave);
            
            yield return new WaitForSeconds(monsterSpawnInterval);
        }
    }
    
    IEnumerator SummonMonstersPhase()
    {
        Debug.Log("👹 Phase: Summoning Monsters");
        isVulnerable = false;
        
        int monsterCount = Mathf.Min(currentHealth + 1, 5);
        
        for (int i = 0; i < monsterCount; i++)
        {
            if (monsterPrefab == null) break;
            
            Vector3 spawnPos = GetRandomSpawnPosition();
            GameObject monster = Instantiate(monsterPrefab, spawnPos, Quaternion.identity);
            monster.name = "Monster_" + i;
            
            MonsterAI monsterAI = monster.GetComponent<MonsterAI>();
            if (monsterAI != null)
            {
                monsterAI.genieBoss = this;
            }
            
            aliveMonsters++;
            Debug.Log($"👹 Spawned monster {i + 1}/{monsterCount}");
            
            yield return new WaitForSeconds(0.5f);
        }
        
        Debug.Log($"⏳ Waiting for player to kill all {aliveMonsters} monsters...");
        
        while (aliveMonsters > 0 && currentHealth > 0)
        {
            yield return null;
        }
        
        if (currentHealth > 0)
        {
            Debug.Log("✅ All monsters killed! Moving to shooting phase...");
        }
    }
    
    IEnumerator ShootingPhase()
    {
        Debug.Log("🔮 Phase: Shooting Projectiles");
        isVulnerable = true;
        
        yield return StartCoroutine(ShootProjectiles(4));
        
        yield return new WaitForSeconds(1f);
    }
    
    IEnumerator MovingPhase()
    {
        Debug.Log("🚁 Phase: Moving around arena");
        isVulnerable = true;
        
        float moveDuration = 5f;
        float elapsed = 0f;
        
        while (elapsed < moveDuration && currentHealth > 0)
        {
            FlyInCircle();
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
    
    private void Update()
    {
        if (currentHealth > 0)
        {
            behaviorTimer -= Time.deltaTime;
            
            if (behaviorTimer <= 0f)
            {
                SwitchBehavior();
            }
            
            switch (currentBehaviorState)
            {
                case GenieBehaviorState.CirclingPlayer:
                    if (canWalkOnGround)
                    {
                        stateTimer -= Time.deltaTime;
                        
                        if (stateTimer <= 0f)
                        {
                            isFlying = !isFlying;
                            
                            if (isFlying)
                            {
                                stateTimer = flyDuration;
                                Debug.Log("✈️ Genie is now FLYING!");
                            }
                            else
                            {
                                stateTimer = walkDuration;
                                Debug.Log("🚶 Genie is now WALKING!");
                            }
                        }
                    }
                    
                    if (isFlying)
                    {
                        FlyAroundPlayer();
                    }
                    else
                    {
                        WalkOnGround();
                    }
                    break;
                    
                case GenieBehaviorState.ChasingPlayer:
                    ChasePlayer();
                    break;
                    
                case GenieBehaviorState.IdleWandering:
                    WanderAround();
                    break;
            }
        }
    }
    
    void SwitchBehavior()
    {
        int randomBehavior = Random.Range(0, 3);
        
        switch (randomBehavior)
        {
            case 0:
                currentBehaviorState = GenieBehaviorState.CirclingPlayer;
                Debug.Log("🔄 Genie behavior: Circling Player (180° back and forth)");
                break;
            case 1:
                currentBehaviorState = GenieBehaviorState.ChasingPlayer;
                Debug.Log("🏃 Genie behavior: Chasing Player");
                break;
            case 2:
                currentBehaviorState = GenieBehaviorState.IdleWandering;
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    wanderTarget = GetRandomPositionNearPlayer(player.transform.position);
                }
                Debug.Log("🚶 Genie behavior: Wandering around player");
                break;
        }
        
        behaviorTimer = Random.Range(behaviorSwitchMinTime, behaviorSwitchMaxTime);
    }
    
    void ChasePlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        
        Vector3 playerPos = player.transform.position;
        float distance = Vector3.Distance(transform.position, playerPos);
        
        if (distance < minDistanceFromPlayer)
        {
            Vector3 awayDirection = (transform.position - playerPos).normalized;
            Vector3 targetPos = new Vector3(
                playerPos.x + awayDirection.x * minDistanceFromPlayer, 
                playerPos.y + flyHeight, 
                playerPos.z + awayDirection.z * minDistanceFromPlayer
            );
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * flySpeed);
        }
        else if (distance > chaseDistance)
        {
            Vector3 targetPos = new Vector3(playerPos.x, playerPos.y + flyHeight, playerPos.z);
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * flySpeed * 1.5f);
        }
        else
        {
            Vector3 targetPos = new Vector3(playerPos.x, playerPos.y + flyHeight, playerPos.z);
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * flySpeed * 0.8f);
        }
        
        Vector3 lookDirection = playerPos - transform.position;
        lookDirection.y = 0;
        if (lookDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection), Time.deltaTime * 6f);
        }
        
        if (genieAnimator != null)
        {
            genieAnimator.SetBool("IsGrounded", false);
        }
    }
    
    void WanderAround()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        
        Vector3 targetPos = new Vector3(wanderTarget.x, wanderTarget.y + flyHeight, wanderTarget.z);
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * flySpeed * 0.5f);
        
        if (Vector3.Distance(transform.position, targetPos) < 2f)
        {
            wanderTarget = GetRandomPositionNearPlayer(player.transform.position);
        }
        
        Vector3 lookDirection = wanderTarget - transform.position;
        lookDirection.y = 0;
        if (lookDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection), Time.deltaTime * 3f);
        }
        
        if (genieAnimator != null)
        {
            genieAnimator.SetBool("IsGrounded", false);
        }
    }
    
    Vector3 GetRandomPositionNearPlayer(Vector3 playerPos)
    {
        float randomAngle = Random.Range(0f, Mathf.PI * 2f);
        float randomDistance = Random.Range(wanderRadius * 0.5f, wanderRadius);
        
        float x = playerPos.x + Mathf.Cos(randomAngle) * randomDistance;
        float z = playerPos.z + Mathf.Sin(randomAngle) * randomDistance;
        
        return new Vector3(x, playerPos.y, z);
    }
    
    void FlyAroundPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        
        Vector3 playerPos = player.transform.position;
        
        movementTimer += Time.deltaTime;
        
        if (movementTimer >= halfCircleDuration)
        {
            movementTimer = 0f;
            isMovingClockwise = !isMovingClockwise;
        }
        
        float angleSpeed = (Mathf.PI / halfCircleDuration) * Time.deltaTime;
        if (isMovingClockwise)
        {
            circleAngle += angleSpeed;
        }
        else
        {
            circleAngle -= angleSpeed;
        }
        
        float x = playerPos.x + Mathf.Cos(circleAngle) * followDistance;
        float z = playerPos.z + Mathf.Sin(circleAngle) * followDistance;
        float y = playerPos.y + flyHeight;
        
        Vector3 targetPosition = new Vector3(x, y, z);
        
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * flySpeed);
        
        Vector3 lookDirection = playerPos - transform.position;
        lookDirection.y = 0;
        if (lookDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection), Time.deltaTime * 5f);
        }
        
        if (genieAnimator != null)
        {
            genieAnimator.SetBool("IsGrounded", false);
        }
    }
    
    void WalkOnGround()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        
        Vector3 playerPos = player.transform.position;
        
        circleAngle += walkSpeed * Time.deltaTime * 0.2f;
        
        float x = playerPos.x + Mathf.Cos(circleAngle) * followDistance;
        float z = playerPos.z + Mathf.Sin(circleAngle) * followDistance;
        
        Vector3 targetPosition = new Vector3(x, 1f, z);
        
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * walkSpeed);
        
        Vector3 lookDirection = playerPos - transform.position;
        lookDirection.y = 0;
        if (lookDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection), Time.deltaTime * 4f);
        }
        
        if (genieAnimator != null)
        {
            genieAnimator.SetBool("IsGrounded", true);
            
            int randomAnimIndex = Random.Range(0, 3);
            genieAnimator.SetInteger("GroundAnimIndex", randomAnimIndex);
        }
    }
    
    void FlyInCircle()
    {
        if (arenaCenter == null) return;
        
        circleAngle += flySpeed * Time.deltaTime * 0.2f;
        
        float x = arenaCenter.position.x + Mathf.Cos(circleAngle) * circleRadius;
        float z = arenaCenter.position.z + Mathf.Sin(circleAngle) * circleRadius;
        float y = arenaCenter.position.y + flyHeight;
        
        transform.position = Vector3.Lerp(transform.position, new Vector3(x, y, z), Time.deltaTime * flySpeed);
        
        Vector3 lookDirection = arenaCenter.position - transform.position;
        lookDirection.y = 0;
        if (lookDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection), Time.deltaTime * 5f);
        }
    }
    
    void Die()
    {
        Debug.Log("☠️ Genie defeated! Destroying all monsters...");
        currentPhase = 5;
        currentHealth = 0;
        
        StopAllCoroutines();
        
        if (genieAnimator != null)
        {
            genieAnimator.SetTrigger("Die");
        }
        
        GameObject[] monsters = GameObject.FindGameObjectsWithTag("Monster");
        Debug.Log($"💥 Found {monsters.Length} monsters to destroy");
        
        foreach (GameObject monster in monsters)
        {
            if (monster != null)
            {
                Debug.Log($"💥 Destroying monster: {monster.name}");
                
                MonsterAI monsterAI = monster.GetComponent<MonsterAI>();
                if (monsterAI != null)
                {
                    monsterAI.isDead = true;
                }
                
                Destroy(monster, 0.1f);
            }
        }
        
        aliveMonsters = 0;
        
        if (keyPrefab != null)
        {
            Vector3 keySpawnPos = new Vector3(transform.position.x, 2f, transform.position.z);
            GameObject key = Instantiate(keyPrefab, keySpawnPos, Quaternion.Euler(0, 45, 0));
            key.transform.localScale = Vector3.one * 2f;
            
            Rigidbody keyRb = key.GetComponent<Rigidbody>();
            if (keyRb == null)
            {
                keyRb = key.AddComponent<Rigidbody>();
            }
            keyRb.useGravity = true;
            keyRb.isKinematic = false;
            
            Debug.Log($"🔑 Key dropped at {keySpawnPos}! Scale: 2x");
        }
        else
        {
            Debug.LogWarning("⚠️ Key Prefab is NULL! Assign it in Inspector!");
        }
        
        Debug.Log("✅ Genie and all monsters destroyed!");
        
        BossFightManager bfm = FindFirstObjectByType<BossFightManager>();
        if (bfm != null)
        {
            bfm.OnGenieDefeated();
        }
        
        Destroy(gameObject, deathAnimationDuration);
    }
    
    public void TakeDamage()
    {
        currentHealth--;
        Debug.Log($"💔 Genie took damage! HP: {currentHealth}/{maxHealth}");
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    IEnumerator ShootProjectiles(int count)
    {
        isAttacking = true;
        
        for (int i = 0; i < count; i++)
        {
            if (genieAnimator != null)
            {
                Debug.Log($"🎬 Triggering Shoot animation");
                genieAnimator.SetTrigger("Shoot");
            }
            else
            {
                Debug.LogWarning("⚠️ No Animator component found on Genie or children!");
            }
            
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null && genieProjectilePrefab != null && shootPoint != null)
            {
                Vector3 targetPosition = player.transform.position;
                
                GameObject projectile = Instantiate(genieProjectilePrefab, shootPoint.position, Quaternion.identity);
                GenieProjectile projScript = projectile.GetComponent<GenieProjectile>();
                
                if (projScript != null)
                {
                    projScript.Initialize(targetPosition, projectileSpeed, gameObject);
                }
                
                Debug.Log($"🔮 Genie shot projectile {i + 1}/{count}");
            }
            
            yield return new WaitForSeconds(timeBetweenShots);
        }
        
        isAttacking = false;
    }
    
    void SpawnMonsters(int count)
    {
        if (monsterPrefab == null) return;
        
        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPos = GetRandomSpawnPosition();
            GameObject monster = Instantiate(monsterPrefab, spawnPos, Quaternion.identity);
            monster.name = "Monster_" + i;
            
            MonsterAI ai = monster.GetComponent<MonsterAI>();
            if (ai == null)
            {
                ai = monster.AddComponent<MonsterAI>();
            }
            ai.genieBoss = this;
            
            aliveMonsters++;
            
            Debug.Log($"👹 Spawned monster {i + 1}/{count} at {spawnPos}. Total alive: {aliveMonsters}");
        }
    }
    
    void SpawnSpikes(int count)
    {
        if (spikePrefab == null) return;
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        Vector3 playerPos = playerObj != null ? playerObj.transform.position : Vector3.zero;
        
        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPos = GetSafeSpikePosition(playerPos);
            GameObject spike = Instantiate(spikePrefab, spawnPos, Quaternion.identity);
            spike.name = "Spike_" + i;
            
            SpikeHazard hazard = spike.GetComponent<SpikeHazard>();
            if (hazard != null)
            {
                hazard.damage = 1;
                hazard.instantKill = false;
            }
            
            Debug.Log($"🌸 Spawned spike {i + 1}/{count} at {spawnPos}");
        }
    }
    
    Vector3 GetRandomSpawnPosition()
    {
        if (arenaCenter == null) return Vector3.zero;
        
        float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float randomRadius = Random.Range(5f, 10f);
        
        float x = arenaCenter.position.x + Mathf.Cos(randomAngle) * randomRadius;
        float z = arenaCenter.position.z + Mathf.Sin(randomAngle) * randomRadius;
        
        return new Vector3(x, 1f, z);
    }
    
    Vector3 GetSafeSpikePosition(Vector3 playerPosition)
    {
        Vector3 randomPos;
        int attempts = 0;
        
        do
        {
            float randomX = Random.Range(-12f, 12f);
            float randomZ = Random.Range(-12f, 12f);
            randomPos = new Vector3(randomX, 0f, randomZ);
            attempts++;
            
        } while (Vector3.Distance(randomPos, playerPosition) < 3f && attempts < 20);
        
        return randomPos;
    }
    
    public void OnMonsterKilled()
    {
        aliveMonsters--;
        Debug.Log($"👹 Monster killed! Remaining: {aliveMonsters}");
    }
}
