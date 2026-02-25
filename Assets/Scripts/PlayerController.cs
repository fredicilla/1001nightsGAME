using UnityEngine;
using UnityEngine.InputSystem;

namespace BossFight
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float moveSpeed = 5f;
        public float jumpForce = 8f;
        public float fallMultiplier = 2.5f;
        public float lowJumpMultiplier = 2f;
        public float groundCheckDistance = 0.2f;
        public LayerMask groundLayer;
        public float rotationSpeed = 720f;

        [Header("Shooting Settings")]
        public GameObject applePrefab;
        public Transform shootPoint;
        public float shootForce = 50f;
        public float shootCooldown = 0.5f;
        
        [Header("Projectile Color")]
        public Color projectileColor = new Color(2f, 0f, 0f, 1f);
        public float emissionIntensity = 10f;

        [Header("Speed Modifiers")]
        public float currentSpeedModifier = 1f;

        [Header("Animation")]
        private PlayerAnimationController animationController;
        private Transform characterModel;

        private PlayerAudioManager audioManager;
        private Rigidbody rb;
        private Vector2 moveInput;
        private bool jumpInput;
        private bool shootInput;
        private bool shootInputThisFrame = false;
        private bool isGrounded;
        private float lastShootTime;
        private bool isActive = true;

        public Vector2 MoveInput => moveInput;
        public bool JumpInput => jumpInput;
        public bool ShootInput => shootInputThisFrame;  // Return the flag for recording
        public bool IsGrounded => isGrounded;
        public bool IsActive
        {
            get => isActive;
            set => isActive = value;
        }

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            animationController = GetComponent<PlayerAnimationController>();
            characterModel = transform.Find("CharacterModel");
            audioManager = GetComponent<PlayerAudioManager>();

            isActive = true;

            Debug.Log($"✅ PlayerController Awake: AnimationController = {(animationController != null ? "Found" : "NULL")}");
            Debug.Log($"✅ CharacterModel = {(characterModel != null ? "Found" : "NULL")}");
            Debug.Log($"✅ AudioManager = {(audioManager != null ? "Found" : "NULL")}");

            if (shootPoint == null)
            {
                GameObject sp = new GameObject("ShootPoint");
                sp.transform.SetParent(transform);
                sp.transform.localPosition = new Vector3(0, 1f, 0.5f);
                shootPoint = sp.transform;
            }
        }

        private void FixedUpdate()
        {
            if (!isActive) return;

            // Reset shoot flag at the END of FixedUpdate (after recording happens)
            shootInputThisFrame = false;

            CheckGround();
            HandleMovement();
            ApplyBetterJumpPhysics();
        }

        private void ApplyBetterJumpPhysics()
        {
            // Better jump feel - faster fall
            if (rb.linearVelocity.y < 0)
            {
                // Falling - apply extra gravity
                rb.linearVelocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
            }
            else if (rb.linearVelocity.y > 0 && !jumpInput)
            {
                // Released jump button while going up - fall faster
                rb.linearVelocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
            }
        }

        private void Update()
        {
            if (!isActive) return;

            HandleJump();
            HandleShooting();
        }

        private void CheckGround()
        {
            // FIXED: Much more aggressive ground check!
            float checkRadius = 0.5f;
            float checkDistance = 3.0f; // Very long distance to catch ground!

            isGrounded = Physics.SphereCast(
                transform.position,
                checkRadius,
                Vector3.down,
                out RaycastHit hit,
                checkDistance
            );

            // Visual debug
            Debug.DrawRay(transform.position, Vector3.down * checkDistance, isGrounded ? Color.green : Color.red);

            if (isGrounded)
            {
                Debug.Log($"✅ Ground found at {hit.distance}m below!");
            }
        }

        private void HandleMovement()
        {
            Vector3 movement = new Vector3(moveInput.x, 0, moveInput.y);

            if (movement.magnitude > 0.1f)
            {
                movement = movement.normalized * moveSpeed * currentSpeedModifier;

                // Rotate character model to face movement direction
                if (characterModel != null)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(movement);
                    characterModel.rotation = Quaternion.RotateTowards(
                        characterModel.rotation,
                        targetRotation,
                        rotationSpeed * Time.fixedDeltaTime
                    );
                }
            }

            Vector3 newVelocity = new Vector3(movement.x, rb.linearVelocity.y, movement.z);
            rb.linearVelocity = newVelocity;
        }

        private void HandleJump()
        {
            if (jumpInput && isGrounded)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);

                if (animationController != null)
                {
                    animationController.TriggerJump();
                }

                if (audioManager != null)
                {
                    audioManager.PlayJumpSound();
                }

                jumpInput = false;
            }
        }

        private void HandleShooting()
        {
            if (shootInput && Time.time > lastShootTime + shootCooldown)
            {
                Debug.Log("🔫 HandleShooting: Shooting INSTANTLY!");

                shootInputThisFrame = true;

                Shoot();

                if (animationController != null)
                {
                    animationController.TriggerThrow();
                }

                if (audioManager != null)
                {
                    audioManager.PlayThrowSound();
                }

                lastShootTime = Time.time;
                shootInput = false;
            }
        }

        private void Shoot()
        {
            Debug.Log("🔫 Shoot() CALLED!");

            if (shootPoint == null)
            {
                Debug.LogWarning("⚠️ ShootPoint is NULL! Creating at player position...");
                GameObject shootPointObj = new GameObject("ShootPoint");
                shootPointObj.transform.SetParent(transform);
                shootPointObj.transform.localPosition = new Vector3(0, 1.5f, 0.5f);
                shootPoint = shootPointObj.transform;
            }

            Vector3 shootDirection = characterModel != null ? characterModel.forward : transform.forward;

            Debug.Log($"🍎 Shooting from {shootPoint.position} in direction {shootDirection}");

            GameObject apple = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            apple.name = "Apple";
            apple.tag = "Projectile";

            apple.transform.position = shootPoint.position;
            apple.transform.localScale = Vector3.one * 0.5f;

            var renderer = apple.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = projectileColor;
                Debug.Log($"🎨 Applied simple color: {projectileColor}");
            }

            Debug.Log($"✅ Red sphere apple created at {shootPoint.position}");

            Rigidbody appleRb = apple.GetComponent<Rigidbody>();
            if (appleRb == null)
            {
                appleRb = apple.AddComponent<Rigidbody>();
            }

            appleRb.mass = 0.5f;
            appleRb.useGravity = true;
            appleRb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            Vector3 shootDir = characterModel != null ? characterModel.forward : transform.forward;
            Vector3 launchDirection = shootDir + Vector3.up * 0.1f;
            appleRb.linearVelocity = launchDirection * shootForce;

            Debug.Log($"🚀 Apple launched! Velocity: {appleRb.linearVelocity}");

            ProjectileController projectile = apple.GetComponent<ProjectileController>();
            if (projectile == null)
            {
                projectile = apple.AddComponent<ProjectileController>();
            }
            projectile.owner = gameObject;

            Destroy(apple, 5f);

            Debug.Log($"✅ Apple shot successfully by {gameObject.name}");
        }

        public void OnMove(InputValue value)
        {
            moveInput = value.Get<Vector2>();
        }

        public void OnJump(InputValue value)
        {
            Debug.Log($"Jump pressed! isGrounded: {isGrounded}");
            jumpInput = true;
        }

        public void OnShoot(InputValue value)
        {
            Debug.Log($"🎯 OnShoot called! isActive={isActive}");
            Debug.Log("🔫 Shoot pressed! Setting shootInput=true");
            shootInput = true;
        }

        // Alternative names in case Input Action is named differently
        public void OnFire(InputValue value)
        {
            Debug.Log("🔥 OnFire called! Redirecting to OnShoot...");
            OnShoot(value);
        }

        public void OnAttack(InputValue value)
        {
            Debug.Log("⚔️ OnAttack called! Redirecting to OnShoot...");
            OnShoot(value);
        }

        public void OnThrow(InputValue value)
        {
            Debug.Log("🎾 OnThrow called! Redirecting to OnShoot...");
            OnShoot(value);
        }

        public void ApplySpeedModifier(float modifier)
        {
            currentSpeedModifier = modifier;
        }

        private System.Collections.IEnumerator EnableGravityDelayed(Rigidbody rb, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (rb != null)
            {
                rb.useGravity = true;
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + Vector3.down * (groundCheckDistance + 0.1f));
        }
    }
}
