using BossFight;
using UnityEngine;

public class KeyCollectable : MonoBehaviour
{
    [Header("Bob Settings")]
    public float bobHeight = 0.2f;
    public float bobSpeed = 2f;

    [Header("Glow Settings")]
    public float pulseSpeed = 2f;
    public float minBrightness = 0.6f;
    public float maxBrightness = 1.4f;

    [Header("Effects")]
    public GameObject collectEffect;
    public AudioClip collectSound;

    private bool isCollected = false;
    private Vector3 startPosition;
    private SpriteRenderer sr;
    private Color originalColor;

    private void Start()
    {
        startPosition = transform.position;
        sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            originalColor = sr.color;
    }

    private void Update()
    {
        if (isCollected) return;

        // Bob up and down
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // Glow: pulse the sprite brightness using its own original color
        if (sr != null)
        {
            float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
            float brightness = Mathf.Lerp(minBrightness, maxBrightness, t);
            sr.color = originalColor * brightness;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;

        if (other.CompareTag("Player"))
        {
            CollectKey();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isCollected) return;

        if (other.CompareTag("Player"))
        {
            CollectKey();
        }
    }

    private void CollectKey()
    {
        isCollected = true;

        GameManager gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            gameManager.CollectKey();
            Debug.Log("✅ Key collected! Goal unlocked!");
        }
        else
        {
            Debug.LogError("❌ GameManager not found!");
        }

        if (collectEffect != null)
            Instantiate(collectEffect, transform.position, Quaternion.identity);

        if (collectSound != null)
            AudioSource.PlayClipAtPoint(collectSound, transform.position);

        Destroy(gameObject, 0.1f);
    }
}
