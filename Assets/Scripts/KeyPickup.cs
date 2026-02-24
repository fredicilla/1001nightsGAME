using BossFight;
using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    public float rotationSpeed = 100f;

    private void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager gameManager = FindFirstObjectByType<GameManager>();
            if (gameManager != null)
            {
                gameManager.CollectKey();
                gameObject.SetActive(false);
            }
        }
    }
}
