using BossFight;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    private Animator animator;
    private PlayerController playerController;
    private Rigidbody rb;

    private bool isThrowing = false;
    private bool isDead = false;
    private float throwStartTime;
    private bool throwAppleFired = false;

    [Header("Animation Smoothing")]
    [SerializeField] private float speedSmoothTime = 0.1f;
    private float currentSpeed = 0f;
    private float speedVelocity = 0f;

    private void Awake()
    {
        Debug.Log($"🔍 PlayerAnimationController.Awake on {gameObject.name}");

        // البحث في children
        Animator[] animators = GetComponentsInChildren<Animator>(true);
        Debug.Log($"🔍 Found {animators.Length} Animator(s) in children");

        if (animators.Length > 0)
        {
            for (int i = 0; i < animators.Length; i++)
            {
                Debug.Log($"  - Animator {i}: on {animators[i].gameObject.name}, enabled={animators[i].enabled}");
            }
            animator = animators[0];
        }
        else
        {
            animator = GetComponentInChildren<Animator>();
        }

        playerController = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody>();

        if (animator == null)
        {
            Debug.LogError($"❌ PlayerAnimationController on {gameObject.name}: Animator NOT FOUND in children!");
            Transform characterModel = transform.Find("CharacterModel");
            if (characterModel != null)
            {
                Debug.Log($"  CharacterModel found at: {characterModel.name}");
                Animator cmAnimator = characterModel.GetComponent<Animator>();
                Debug.Log($"  CharacterModel has Animator: {cmAnimator != null}");
            }
            else
            {
                Debug.Log($"  CharacterModel NOT FOUND!");
            }
        }
        else
        {
            Debug.Log($"✅ PlayerAnimationController: Animator found on {animator.gameObject.name}! Controller = {(animator.runtimeAnimatorController != null ? animator.runtimeAnimatorController.name : "NULL")}");
        }

        if (playerController == null)
        {
            Debug.LogWarning($"⚠️ PlayerAnimationController on {gameObject.name}: PlayerController NOT FOUND (this is OK for Ghosts)");
        }
    }

    private void Update()
    {
        if (animator == null || isDead) return;

        // Handle throw animation timing (for BOTH Player and Ghost!)
        if (isThrowing)
        {
            float throwElapsedTime = Time.time - throwStartTime;

            // Complete throw at 0.05 seconds
            if (throwElapsedTime >= 0.05f)
            {
                Debug.Log("✅ Timer: Throw complete (0.05s)!");
                OnThrowComplete();
            }
        }

        // Update animations only for Player (not for Ghosts)
        if (playerController != null)
        {
            UpdateAnimations();
        }
    }

    private void UpdateAnimations()
    {
        if (playerController == null) return;

        float targetSpeed = new Vector3(
            playerController.MoveInput.x,
            0,
            playerController.MoveInput.y
        ).magnitude;

        currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedVelocity, speedSmoothTime);

        animator.SetFloat("Speed", currentSpeed);
        animator.SetBool("IsGrounded", playerController.IsGrounded);

        float verticalVelocity = rb != null ? rb.linearVelocity.y : 0f;
        animator.SetFloat("VerticalVelocity", verticalVelocity);

        if (Time.frameCount % 60 == 0 && currentSpeed > 0)
        {
            Debug.Log($"🎬 Animation Update: Speed={currentSpeed:F2} (Target={targetSpeed:F2}), IsGrounded={playerController.IsGrounded}");
        }
    }

    public void UpdateGhostAnimations(Vector3 velocity)
    {
        if (animator == null)
        {
            Debug.LogWarning($"⚠️ UpdateGhostAnimations: Animator is NULL on {gameObject.name}");
            return;
        }

        if (!animator.enabled)
        {
            animator.enabled = true;
        }

        if (animator.runtimeAnimatorController == null)
        {
            Debug.LogError($"❌ UpdateGhostAnimations: Animator has NO Controller on {gameObject.name}");
            return;
        }

        float targetSpeed = new Vector3(velocity.x, 0, velocity.z).magnitude / 5f;
        targetSpeed = Mathf.Clamp01(targetSpeed);

        // For Ghost: use target speed directly (no smoothing needed since playback is already smooth)
        animator.SetFloat("Speed", targetSpeed);
        animator.SetBool("IsGrounded", true);

        float verticalVelocity = velocity.y;
        animator.SetFloat("VerticalVelocity", verticalVelocity);

        if (Time.frameCount % 60 == 0)
        {
            Debug.Log($"🎬 Ghost Anim: vel={velocity.magnitude:F2}, Speed set to={targetSpeed:F2}");
        }
    }

    public void TriggerJump()
    {
        if (animator != null && !isDead)
        {
            animator.SetTrigger("Jump");
        }
    }

    public void TriggerThrow()
    {
        if (animator != null && !isDead && !isThrowing)
        {
            Debug.Log("🎬 TriggerThrow: Starting throw animation (0.05s)...");

            // Speed up the throw animation to 20x (1.0 / 20 = 0.05s!)
            animator.speed = 20f;

            animator.SetTrigger("Throw");
            isThrowing = true;
            throwStartTime = Time.time;
            throwAppleFired = false;
        }
        else
        {
            Debug.LogWarning($"⚠️ TriggerThrow blocked: animator={animator != null}, isDead={isDead}, isThrowing={isThrowing}");
        }
    }

    public void TriggerDeath(Vector3 damageDirection)
    {
        Debug.Log($"💀 TriggerDeath called on {gameObject.name}! isDead={isDead}, animator={animator != null}");

        if (animator != null && !isDead)
        {
            // Force animator speed to normal for death animation
            animator.speed = 1f;

            Transform characterModel = transform.Find("CharacterModel");
            if (characterModel != null)
            {
                Vector3 knockbackDir = -damageDirection;
                knockbackDir.y = 0;
                if (knockbackDir != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(knockbackDir);
                    characterModel.rotation = targetRotation;
                    Debug.Log($"💀 Character rotated towards knockback: {knockbackDir}");
                }
            }

            animator.SetTrigger("Death");
            isDead = true;
            Debug.Log($"💀 Death animation triggered! Animator state will change to Death.");
        }
        else
        {
            Debug.LogWarning($"⚠️ TriggerDeath failed: animator={animator != null}, isDead={isDead}");
        }
    }

    public void OnThrowComplete()
    {
        string objectName = gameObject.name;
        Debug.Log($"✅ OnThrowComplete on {objectName} - ending throw animation!");

        // Reset speed immediately
        if (animator != null)
        {
            animator.speed = 1f;
            Debug.Log($"⚡ {objectName}: animator.speed reset to 1f");
        }

        isThrowing = false;

        // Force immediate transition to locomotion
        if (animator != null)
        {
            // Clear triggers
            animator.ResetTrigger("Throw");

            // Force update to apply changes NOW
            animator.Update(0f);

            // Jump directly to Locomotion - no transition time
            animator.Play("Locomotion", 0, 0f);

            // Force another update
            animator.Update(0f);

            Debug.Log("⚡ INSTANT transition to Locomotion!");
        }
    }

    public void OnThrowApple()
    {
        // This is no longer needed - PlayerController shoots immediately!
        // Kept for compatibility
        Debug.Log("🍎 OnThrowApple called (deprecated - shooting happens instantly now!)");
    }

    public bool IsDead => isDead;
    public bool IsThrowing => isThrowing;
}
