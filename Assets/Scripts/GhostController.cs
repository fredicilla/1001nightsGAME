using BossFight;
using System.Collections.Generic;
using UnityEngine;

public class GhostController : MonoBehaviour
{
    private List<RecordedFrame> recording;
    private int currentFrameIndex = 0;
    private float playbackStartTime;
    private bool isPlaying = false;
    private bool waitingForGameStart = true;

    [Header("Shooting Settings")]
    public GameObject applePrefab;
    public Transform shootPoint;
    public float shootForce = 15f;

    private HashSet<int> shotFrames = new HashSet<int>();
    private PlayerAnimationController animationController;
    private Vector3 lastPosition;
    private Rigidbody rb;
    private Transform characterModel;

    private void Awake()
    {
        if (shootPoint == null)
        {
            GameObject sp = new GameObject("ShootPoint");
            sp.transform.SetParent(transform);
            sp.transform.localPosition = new Vector3(0, 1f, 0.5f);
            shootPoint = sp.transform;
        }

        animationController = GetComponent<PlayerAnimationController>();
        rb = GetComponent<Rigidbody>();
        characterModel = transform.Find("CharacterModel");
        lastPosition = transform.position;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    public void StartPlayback(List<RecordedFrame> recordedFrames)
    {
        recording = recordedFrames;
        currentFrameIndex = 0;
        playbackStartTime = Time.time;
        isPlaying = true;
        waitingForGameStart = true;
        shotFrames.Clear();

        Debug.Log($"📼 GhostController: Playback started! Frames: {recording.Count}, Start time: {playbackStartTime}");
    }

    public void StopPlayback()
    {
        isPlaying = false;
    }

    private void Update()
    {
        GameManager gameManager = GameManager.Instance;
        if (gameManager != null && gameManager.IsWaitingForInput)
        {
            if (waitingForGameStart)
            {
                playbackStartTime = Time.time;
            }
            return;
        }

        if (waitingForGameStart)
        {
            waitingForGameStart = false;
            playbackStartTime = Time.time;
            Debug.Log($"👻 Ghost playback UNFROZEN! Starting from time 0");
        }

        if (!isPlaying || recording == null || recording.Count == 0)
        {
            if (recording != null && recording.Count == 0)
            {
                Debug.LogWarning("⚠️ Ghost has empty recording!");
            }
            return;
        }

        float currentTime = Time.time - playbackStartTime;

        RecordedFrame targetFrame = null;
        RecordedFrame nextFrame = null;
        float interpolationFactor = 0f;

        while (currentFrameIndex < recording.Count && recording[currentFrameIndex].timestamp <= currentTime)
        {
            targetFrame = recording[currentFrameIndex];

            // Debug: Log shoot attempts
            if (targetFrame.shootPressed)
            {
                Debug.Log($"👻 Frame {currentFrameIndex}: shootPressed=TRUE at time {targetFrame.timestamp:F2}");
            }

            if (targetFrame.shootPressed && !shotFrames.Contains(currentFrameIndex))
            {
                Debug.Log($"👻 SHOOTING! Frame {currentFrameIndex}, Direction: {targetFrame.shootDirection}");
                Shoot(targetFrame.shootDirection);
                shotFrames.Add(currentFrameIndex);
            }

            currentFrameIndex++;
        }

        if (targetFrame != null)
        {
            if (currentFrameIndex < recording.Count)
            {
                nextFrame = recording[currentFrameIndex];
                float timeBetweenFrames = nextFrame.timestamp - targetFrame.timestamp;
                if (timeBetweenFrames > 0)
                {
                    float timeSinceFrame = currentTime - targetFrame.timestamp;
                    interpolationFactor = Mathf.Clamp01(timeSinceFrame / timeBetweenFrames);
                }
            }

            Vector3 targetPosition = targetFrame.position;
            Quaternion targetRotation = targetFrame.rotation;

            if (nextFrame != null && interpolationFactor > 0)
            {
                targetPosition = Vector3.Lerp(targetFrame.position, nextFrame.position, interpolationFactor);
                targetRotation = Quaternion.Slerp(targetFrame.rotation, nextFrame.rotation, interpolationFactor);
            }

            if (rb != null)
            {
                rb.MovePosition(targetPosition);
            }
            else
            {
                transform.position = targetPosition;
            }

            // Apply rotation to CharacterModel (not root transform)
            if (characterModel != null)
            {
                characterModel.rotation = targetRotation;
            }
            else
            {
                // Fallback: apply to root
                if (rb != null)
                {
                    rb.MoveRotation(targetRotation);
                }
                else
                {
                    transform.rotation = targetRotation;
                }
            }
        }

        Vector3 velocity = (transform.position - lastPosition) / Time.deltaTime;
        if (animationController != null)
        {
            animationController.UpdateGhostAnimations(velocity);

            // SAFETY CHECK: Force animator speed to normal if not throwing
            if (!animationController.IsThrowing)
            {
                Animator anim = animationController.GetComponent<Animator>();
                if (anim != null && anim.speed != 1f)
                {
                    Debug.LogWarning($"⚠️ Ghost animator.speed was {anim.speed}, forcing to 1f!");
                    anim.speed = 1f;
                }
            }

            if (Time.frameCount % 60 == 0)
            {
                Debug.Log($"👻 Ghost velocity: {velocity.magnitude:F2}, position change: {(transform.position - lastPosition).magnitude:F3}");
            }
        }
        else
        {
            if (Time.frameCount % 120 == 0)
            {
                Debug.LogWarning($"⚠️ GhostController: PlayerAnimationController is NULL on {gameObject.name}!");
            }
        }
        lastPosition = transform.position;

        if (currentFrameIndex >= recording.Count)
        {
            Debug.Log("👻 Ghost playback finished!");
            StopPlayback();
        }
    }

    private void Shoot(Vector3 direction)
    {
        Debug.Log("👻 Ghost shooting apple!");

        // Create red sphere apple (same as PlayerController)
        GameObject apple = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        apple.name = "Apple";
        apple.tag = "Projectile";
        apple.transform.position = shootPoint.position;
        apple.transform.localScale = Vector3.one * 0.3f;

        var renderer = apple.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.red;
        }

        // Add Rigidbody with same settings as PlayerController
        Rigidbody appleRb = apple.GetComponent<Rigidbody>();
        if (appleRb == null)
        {
            appleRb = apple.AddComponent<Rigidbody>();
        }

        appleRb.mass = 0.05f;  // Same as PlayerController
        appleRb.useGravity = true;
        appleRb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        // Launch with same settings as PlayerController
        Vector3 shootDir = direction.normalized;
        Vector3 launchDirection = shootDir + Vector3.up * 0.1f;
        appleRb.linearVelocity = launchDirection * 15f;

        // Set owner to avoid hitting self
        ProjectileController projectile = apple.AddComponent<ProjectileController>();
        projectile.owner = gameObject;

        // Trigger throw animation on Ghost
        if (animationController != null)
        {
            animationController.TriggerThrow();
        }

        Debug.Log($"👻 Ghost shot apple! Velocity: {appleRb.linearVelocity}");
    }

    public bool IsPlaying => isPlaying;
    public bool HasFinished => !isPlaying && currentFrameIndex >= (recording?.Count ?? 0);
}
