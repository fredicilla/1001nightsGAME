using UnityEngine;

public class DeathZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            HealthSystem health = other.GetComponent<HealthSystem>();
            if (health != null)
            {
                Vector3 damageDirection = (other.transform.position - transform.position).normalized;
                health.Die(damageDirection);
            }
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            HealthSystem health = collision.gameObject.GetComponent<HealthSystem>();
            if (health != null)
            {
                Vector3 damageDirection = (collision.transform.position - transform.position).normalized;
                health.Die(damageDirection);
            }
        }
    }
}
