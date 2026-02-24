using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    
    [Header("Camera Settings")]
    [Tooltip("المسافة خلف اللاعب")]
    public float distanceBehind = 5f;
    
    [Tooltip("الارتفاع فوق اللاعب")]
    public float heightAbove = 2f;
    
    [Tooltip("سرعة متابعة الكاميرا (أعلى = أسرع)")]
    public float followSpeed = 10f;
    
    [Tooltip("سرعة دوران الكاميرا")]
    public float rotationSpeed = 5f;
    
    [Header("Look At Settings")]
    [Tooltip("نقطة النظر فوق اللاعب")]
    public float lookAtHeight = 1.5f;
    
    private void LateUpdate()
    {
        if (target == null)
        {
            // ابحث عن اللاعب الحالي تلقائياً
            FindCurrentPlayer();
            return;
        }
        
        FollowTarget();
    }
    
    private void FindCurrentPlayer()
    {
        // ابحث عن اللاعب الحالي (Player أو Monster)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Monster");
        }
        
        if (player != null)
        {
            target = player.transform;
            Debug.Log($"📹 Camera found target: {target.name}");
        }
    }
    
    private void FollowTarget()
    {
        // احسب الموقع المطلوب خلف اللاعب
        Vector3 targetForward = target.forward;
        Vector3 desiredPosition = target.position - (targetForward * distanceBehind) + (Vector3.up * heightAbove);
        
        // تحريك الكاميرا بسلاسة
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
        
        // انظر نحو اللاعب مع offset للارتفاع
        Vector3 lookAtPosition = target.position + Vector3.up * lookAtHeight;
        Quaternion targetRotation = Quaternion.LookRotation(lookAtPosition - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
    
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        Debug.Log($"📹 Camera target changed to: {newTarget.name}");
    }
}
