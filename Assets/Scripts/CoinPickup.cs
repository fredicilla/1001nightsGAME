using BossFight;
using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    public float slowdownModifier = 0.5f;
    public float rotationSpeed = 100f;

    private void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.ApplySpeedModifier(slowdownModifier);
            }

            gameObject.SetActive(false);
        }
    }
}
