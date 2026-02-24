using UnityEngine;

public class WifeAI : MonoBehaviour
{
    [Header("Chase Settings")]
    public float chaseSpeed = 3.5f;
    public float hoverHeight = 0.3f;
    public float rotationSpeed = 5f;
    public float predictionTime = 0.3f;
    public float minDistanceToTarget = 0.3f;
    
    [Header("Detection")]
    public float detectionRange = 50f;
    public string targetTag = "Player";
    
    [Header("Kill Settings")]
    public bool instantKill = true;
    
    [Header("Effects")]
    public GameObject killEffect;
    public AudioClip killSound;
    
    private Transform target;
    private Vector3 lastTargetPosition;
    private Vector3 targetVelocity;
    private bool isActive = false;
    
    private void Update()
    {
        if (!isActive) return;
        
        FindTarget();
        
        if (target != null)
        {
            ChaseTarget();
        }
    }
    
    public void Activate()
    {
        isActive = true;
        Debug.Log("👰 Wife AI activated! Starting chase...");
    }
    
    public void Deactivate()
    {
        isActive = false;
        target = null;
    }
    
    private void FindTarget()
    {
        if (target != null) return;
        
        GameObject[] potentialTargets = GameObject.FindGameObjectsWithTag(targetTag);
        
        float closestDistance = detectionRange;
        Transform closestTarget = null;
        
        foreach (GameObject obj in potentialTargets)
        {
            // تجاهل الـ Ghost
            if (obj.GetComponent<GhostController>() != null) continue;
            
            float distance = Vector3.Distance(transform.position, obj.transform.position);
            
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = obj.transform;
            }
        }
        
        if (closestTarget != null)
        {
            target = closestTarget;
            lastTargetPosition = target.position;
            targetVelocity = Vector3.zero;
            Debug.Log($"👰 Wife found target: {target.name}");
        }
    }
    
    private void ChaseTarget()
    {
        if (target == null) return;
        
        // حساب سرعة الهدف
        Vector3 currentTargetPosition = target.position;
        targetVelocity = (currentTargetPosition - lastTargetPosition) / Time.deltaTime;
        lastTargetPosition = currentTargetPosition;
        
        // التنبؤ بموقع الهدف المستقبلي
        Vector3 predictedPosition = currentTargetPosition + targetVelocity * predictionTime;
        
        // الموقع المستهدف: الموقع المتنبأ + ارتفاع الطيران
        Vector3 targetPosition = predictedPosition + Vector3.up * hoverHeight;
        
        // المسافة للهدف
        float distance = Vector3.Distance(transform.position, targetPosition);
        
        // التحرك نحو الهدف بشكل مستمر (لا نتوقف!)
        Vector3 direction = (targetPosition - transform.position).normalized;
        
        // السرعة ثابتة - أبطأ من اللاعب
        float currentSpeed = chaseSpeed;
        
        // زيادة بسيطة عند القرب
        if (distance < 2f)
        {
            currentSpeed = chaseSpeed * 1.2f; // زيادة 20% فقط
        }
        
        transform.position += direction * currentSpeed * Time.deltaTime;
        
        // الدوران نحو الهدف
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"🔔 Wife.OnTriggerEnter: {other.name}, Tag: {other.tag}, Active: {isActive}");
        
        if (!isActive) 
        {
            Debug.LogWarning("⚠️ Wife not active, ignoring trigger!");
            return;
        }
        
        if (other.CompareTag(targetTag))
        {
            // تجاهل الـ Ghost
            if (other.GetComponent<GhostController>() != null)
            {
                Debug.Log("👻 Ignoring Ghost");
                return;
            }
            
            Debug.Log($"👰 Wife caught: {other.name}!");
            KillTarget(other.gameObject);
        }
        else
        {
            Debug.Log($"❌ Tag mismatch: expected '{targetTag}', got '{other.tag}'");
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (!isActive) return;
        
        if (collision.gameObject.CompareTag(targetTag))
        {
            if (collision.gameObject.GetComponent<GhostController>() != null) return;
            
            Debug.Log($"👰 Wife caught: {collision.gameObject.name}!");
            KillTarget(collision.gameObject);
        }
    }
    
    private void KillTarget(GameObject targetObject)
    {
        HealthSystem health = targetObject.GetComponent<HealthSystem>();
        
        if (health != null)
        {
            if (instantKill)
            {
                Vector3 damageDirection = (targetObject.transform.position - transform.position).normalized;
                health.Die(damageDirection);
                Debug.Log($"☠️ {targetObject.name} killed by Wife!");
            }
        }
        
        if (killEffect != null)
        {
            Instantiate(killEffect, transform.position, Quaternion.identity);
        }
        
        if (killSound != null)
        {
            AudioSource.PlayClipAtPoint(killSound, transform.position);
        }
    }
}
