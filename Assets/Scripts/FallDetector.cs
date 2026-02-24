using UnityEngine;

public class FallDetector : MonoBehaviour
{
    public float fallThreshold = -10f;
    
    private void Update()
    {
        if (transform.position.y < fallThreshold)
        {
            if (gameObject.CompareTag("Player"))
            {
                HealthSystem health = GetComponent<HealthSystem>();
                if (health != null)
                {
                    health.Die(Vector3.down);
                }
            }
        }
    }
}
