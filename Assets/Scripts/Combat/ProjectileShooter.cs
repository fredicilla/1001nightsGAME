using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GeniesGambit.Combat
{
    [System.Serializable]
    public struct ShootEvent
    {
        public Vector3 position;
        public Vector3 direction;
        public float timestamp;
    }

    public class ProjectileShooter : MonoBehaviour
    {
        [Header("Shooting Settings")]
        [SerializeField] GameObject projectilePrefab;
        [SerializeField] Transform firePoint;
        [SerializeField] float projectileSpeed = 10f;
        [SerializeField] float fireRate = 0.5f;
        [SerializeField] string projectileTargetTag = "Player";
        [SerializeField] bool invertShootingDirection = false;

        InputAction _shootAction;
        float _nextFireTime;
        bool _canShoot = false;
        bool _isRecording = false;
        bool _isReplaying = false;
        List<ShootEvent> _recordedShots = new List<ShootEvent>();
        List<ShootEvent> _replayShots = new List<ShootEvent>();
        float _recordingStartTime;
        float _replayStartTime;
        int _nextReplayIndex = 0;

        // Replay delay to match GhostReplay's 3-second delay
        const float REPLAY_DELAY = 3f;
        float _replayDelayTimer = 0f;
        bool _replayDelayComplete = false;

        public void StartRecording()
        {
            _isRecording = true;
            _isReplaying = false;
            _recordedShots.Clear();
            _recordingStartTime = Time.time;
            _canShoot = true;

            var playerInput = GetComponent<PlayerInput>();
            if (playerInput == null)
            {
                playerInput = FindFirstObjectByType<PlayerInput>();
            }

            if (playerInput != null)
            {
                try
                {
                    _shootAction = playerInput.actions["Attack"];
                }
                catch (System.Exception)
                {
                    Debug.LogWarning("[ProjectileShooter] 'Attack' action not found in Input Actions. Shooting will be disabled.");
                    _shootAction = null;
                    _canShoot = false;
                }
            }
        }

        public void StopRecording()
        {
            _isRecording = false;
            _canShoot = false;
            _shootAction = null;
        }

        public List<ShootEvent> GetRecording()
        {
            return new List<ShootEvent>(_recordedShots);
        }

        public void StartReplay(List<ShootEvent> shots)
        {
            _isReplaying = true;
            _isRecording = false;
            _canShoot = false;
            _replayShots = new List<ShootEvent>(shots);
            _replayStartTime = Time.time;
            _nextReplayIndex = 0;
            _replayDelayTimer = 0f;
            _replayDelayComplete = false;
            Debug.Log($"[ProjectileShooter] {gameObject.name} starting replay with {shots.Count} shots (3s delay)");
        }

        public void EnableShooting(bool enable, bool recordShots = false)
        {
            _canShoot = enable;
            _isRecording = recordShots;

            if (recordShots)
            {
                _recordedShots.Clear();
                _recordingStartTime = Time.time;
            }

            if (enable)
            {
                var playerInput = FindFirstObjectByType<PlayerInput>();
                if (playerInput != null)
                {
                    try
                    {
                        _shootAction = playerInput.actions["Attack"];
                    }
                    catch (System.Exception)
                    {
                        Debug.LogWarning("[ProjectileShooter] 'Attack' action not found. Shooting disabled.");
                        _shootAction = null;
                        _canShoot = false;
                    }
                }
            }
            else
            {
                _shootAction = null;
            }
        }

        void Update()
        {
            if (_isReplaying)
            {
                ReplayUpdate();
            }
            else if (_canShoot && _shootAction != null)
            {
                if (_shootAction.triggered && Time.time >= _nextFireTime)
                {
                    Shoot();
                    _nextFireTime = Time.time + fireRate;
                }
            }
        }

        void ReplayUpdate()
        {
            if (_nextReplayIndex >= _replayShots.Count)
                return;

            // Wait for delay to match GhostReplay movement delay
            if (!_replayDelayComplete)
            {
                _replayDelayTimer += Time.deltaTime;
                if (_replayDelayTimer >= REPLAY_DELAY)
                {
                    _replayDelayComplete = true;
                    _replayStartTime = Time.time; // Reset start time after delay
                    Debug.Log($"[ProjectileShooter] {gameObject.name} delay complete, can now replay shots");
                }
                return;
            }

            float elapsed = Time.time - _replayStartTime;

            while (_nextReplayIndex < _replayShots.Count)
            {
                ShootEvent shootEvent = _replayShots[_nextReplayIndex];

                if (shootEvent.timestamp <= elapsed)
                {
                    ShootAtPosition(shootEvent.position, shootEvent.direction);
                    _nextReplayIndex++;
                }
                else
                {
                    break;
                }
            }
        }

        void ShootAtPosition(Vector3 position, Vector3 direction)
        {
            if (projectilePrefab == null)
            {
                Debug.LogError("[ProjectileShooter] Missing projectile prefab for replay!");
                return;
            }

            if (firePoint == null)
            {
                Debug.LogError($"[ProjectileShooter] {gameObject.name} has NO firePoint! Using transform.position instead.");
            }

            // Use ghost's current firePoint position instead of recorded world position
            // This ensures projectiles come from where the ghost actually IS, not where it was during recording
            Vector3 spawnPosition = (firePoint != null) ? firePoint.position : transform.position;

            Debug.Log($"[ProjectileShooter] {gameObject.name} REPLAYING shot - Spawn: {spawnPosition}, Dir: {direction}, Target: '{projectileTargetTag}'");

            GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
            projectile.transform.right = direction;

            var projectileScript = projectile.GetComponent<Projectile>();
            if (projectileScript != null)
            {
                var shooterCollider = GetComponent<Collider2D>();
                projectileScript.Initialize(direction, projectileSpeed, projectileTargetTag, shooterCollider);
                Debug.Log($"[ProjectileShooter] Projectile initialized with targetTag='{projectileTargetTag}', speed={projectileSpeed}");
            }
            else
            {
                Debug.LogError($"[ProjectileShooter] Projectile prefab has NO Projectile script!");
            }
        }

        void Shoot()
        {
            if (projectilePrefab == null || firePoint == null)
            {
                Debug.LogWarning("[ProjectileShooter] Missing prefab or fire point!");
                return;
            }

            Vector3 direction = GetFacingDirection();

            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            projectile.transform.right = direction;

            var projectileScript = projectile.GetComponent<Projectile>();
            if (projectileScript != null)
            {
                var shooterCollider = GetComponent<Collider2D>();
                projectileScript.Initialize(direction, projectileSpeed, projectileTargetTag, shooterCollider);
            }

            Debug.Log($"[ProjectileShooter] {gameObject.name} fired projectile in direction {direction}!");

            if (_isRecording)
            {
                _recordedShots.Add(new ShootEvent
                {
                    position = firePoint.position,
                    direction = direction,
                    timestamp = Time.time - _recordingStartTime
                });
            }
        }

        Vector3 GetFacingDirection()
        {
            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                if (invertShootingDirection)
                {
                    return spriteRenderer.flipX ? Vector3.right : Vector3.left;
                }
                else
                {
                    return spriteRenderer.flipX ? Vector3.left : Vector3.right;
                }
            }

            if (transform.localScale.x < 0)
            {
                return Vector3.left;
            }

            return Vector3.right;
        }

        public List<ShootEvent> GetRecordedShots()
        {
            return new List<ShootEvent>(_recordedShots);
        }

        public int GetRecordedShotCount()
        {
            return _recordedShots.Count;
        }

        /// <summary>
        /// Set the tag that projectiles will target
        /// </summary>
        public void SetTargetTag(string tag)
        {
            projectileTargetTag = tag;
            Debug.Log($"[ProjectileShooter] {gameObject.name} now targets: {tag}");
        }

        /// <summary>
        /// Get the current target tag
        /// </summary>
        public string GetTargetTag()
        {
            return projectileTargetTag;
        }
    }
}
