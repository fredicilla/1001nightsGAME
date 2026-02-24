using BossFight;
using UnityEngine;

public class KeyCollectable : MonoBehaviour
{
    [Header("Settings")]
    public float rotationSpeed = 50f;
    public bool autoRotate = true;

    [Header("Effects")]
    public GameObject collectEffect;
    public AudioClip collectSound;

    private bool isCollected = false;

    private void Update()
    {
        if (autoRotate)
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;

        if (other.CompareTag("Player"))
        {
            Debug.Log($"🔑 Key collected by: {other.name}");

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
        {
            Instantiate(collectEffect, transform.position, Quaternion.identity);
        }

        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        }

        Destroy(gameObject, 0.1f);
    }
}
