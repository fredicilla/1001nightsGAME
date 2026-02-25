using UnityEngine;

public class PlayerAudioManager : MonoBehaviour
{
    [Header("Audio Clips")]
    public AudioClip jumpSound;
    public AudioClip throwSound;
    public AudioClip dieSound;

    [Header("Audio Settings")]
    [Range(0f, 1f)]
    public float jumpVolume = 1f;
    [Range(0f, 1f)]
    public float throwVolume = 1f;
    [Range(0f, 1f)]
    public float dieVolume = 1f;

    private AudioSource audioSource;
    private HealthSystem healthSystem;
    private bool hasPlayedDeathSound = false;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;

        healthSystem = GetComponent<HealthSystem>();
    }

    private void Update()
    {
        if (healthSystem != null && healthSystem.currentHealth <= 0 && !hasPlayedDeathSound)
        {
            PlayDieSound();
            hasPlayedDeathSound = true;
        }
    }

    public void PlayJumpSound()
    {
        if (jumpSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(jumpSound, jumpVolume);
            Debug.Log("🔊 Playing jump sound");
        }
    }

    public void PlayThrowSound()
    {
        if (throwSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(throwSound, throwVolume);
            Debug.Log("🔊 Playing throw sound");
        }
    }

    public void PlayDieSound()
    {
        if (dieSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(dieSound, dieVolume);
            Debug.Log("🔊 Playing die sound");
        }
    }
}
