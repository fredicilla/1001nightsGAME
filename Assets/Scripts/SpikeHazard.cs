using UnityEngine;

public class SpikeHazard : MonoBehaviour
{
    [Header("Settings")]
    public int damage = 1;
    public bool instantKill = false;
    
    [Header("Effects")]
    public GameObject hitEffect;
    public AudioClip hitSound;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"🌸 Spike hit: {other.name}");
            
            HealthSystem health = other.GetComponent<HealthSystem>();
            if (health != null)
            {
                if (instantKill)
                {
                    Vector3 damageDirection = (other.transform.position - transform.position).normalized;
                    health.Die(damageDirection);
                    Debug.Log($"☠️ {other.name} killed by spike!");
                }
                else
                {
                    health.TakeDamage(damage, transform.position);
                }
            }
            
            if (hitEffect != null)
            {
                Instantiate(hitEffect, transform.position, Quaternion.identity);
            }
            
            if (hitSound != null)
            {
                AudioSource.PlayClipAtPoint(hitSound, transform.position);
            }
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        OnTriggerEnter(collision.collider);
    }
}
