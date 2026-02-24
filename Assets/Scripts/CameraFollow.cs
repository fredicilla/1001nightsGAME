using UnityEngine;

[ExecuteAlways]
public class CameraFollow : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;
    
    [Header("Camera Offset")]
    public Vector3 offset = new Vector3(0f, 2.3f, -14.05f);
    
    [Header("Camera Rotation")]
    public Vector3 rotation = new Vector3(19.67f, 0f, 0f);
    
    [Header("Follow Settings")]
    public bool smoothFollow = true;
    public float smoothSpeed = 10f;
    
    [Header("Collision Settings")]
    public bool enableCollisionDetection = true;
    public LayerMask collisionLayers = -1;
    public float cameraRadius = 0.3f;
    public float minDistance = 1f;
    public float collisionSmoothSpeed = 15f;
    
    [Header("Dynamic Rotation")]
    public bool enableDynamicRotation = true;
    public float maxRotationAngle = 45f;
    public float rotationSmoothSpeed = 8f;
    
    [Header("Dynamic Height Adjustment")]
    public bool enableDynamicHeight = true;
    public float maxHeightIncrease = 2f;
    public float heightSmoothSpeed = 8f;
    
    [Header("Dynamic FOV (Zoom)")]
    public bool enableDynamicFOV = true;
    public float baseFOV = 60f;
    public float maxFOVIncrease = 20f;
    public float fovSmoothSpeed = 8f;
    
    private float currentDistance;
    private float currentRotationX;
    private float currentHeightOffset;
    private float currentFOV;
    private Camera cameraComponent;
    
    private void OnEnable()
    {
        FindTarget();
        InitializeValues();
    }
    
    private void Start()
    {
        FindTarget();
        InitializeValues();
    }
    
    private void InitializeValues()
    {
        currentDistance = offset.magnitude;
        currentRotationX = rotation.x;
        currentHeightOffset = 0f;
        
        cameraComponent = GetComponent<Camera>();
        if (cameraComponent != null)
        {
            baseFOV = cameraComponent.fieldOfView;
            currentFOV = baseFOV;
        }
    }
    
    private void FindTarget()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }
    }
    
    private void LateUpdate()
    {
        UpdateCameraPosition();
    }
    
    private void OnValidate()
    {
        if (cameraComponent == null)
        {
            cameraComponent = GetComponent<Camera>();
        }
        
        if (!Application.isPlaying)
        {
            FindTarget();
            currentDistance = offset.magnitude;
            currentRotationX = rotation.x;
            currentHeightOffset = 0f;
            if (cameraComponent != null)
            {
                currentFOV = baseFOV;
            }
        }
        
        UpdateCameraPosition();
    }
    
    private void UpdateCameraPosition()
    {
        if (target == null) 
        {
            FindTarget();
            if (target == null) return;
        }
        
        Vector3 targetPosition = target.position;
        Vector3 direction = offset.normalized;
        float desiredDistance = offset.magnitude;
        
        if (Application.isPlaying && enableCollisionDetection)
        {
            RaycastHit[] hits = Physics.SphereCastAll(targetPosition, cameraRadius, direction, desiredDistance, collisionLayers);
            
            float closestDistance = desiredDistance;
            
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.CompareTag("Projectile") || 
                    hit.collider.name.Contains("Apple") || 
                    hit.collider.name.Contains("Projectile"))
                {
                    continue;
                }
                
                if (hit.distance < closestDistance)
                {
                    closestDistance = hit.distance;
                }
            }
            
            float targetDistance = Mathf.Clamp(closestDistance - cameraRadius, minDistance, desiredDistance);
            currentDistance = Mathf.Lerp(currentDistance, targetDistance, collisionSmoothSpeed * Time.deltaTime);
        }
        else
        {
            currentDistance = desiredDistance;
        }
        
        float distanceRatio = 1f - (currentDistance / desiredDistance);
        
        float targetHeightOffset = 0f;
        if (Application.isPlaying && enableDynamicHeight)
        {
            targetHeightOffset = distanceRatio * maxHeightIncrease;
            currentHeightOffset = Mathf.Lerp(currentHeightOffset, targetHeightOffset, heightSmoothSpeed * Time.deltaTime);
        }
        else
        {
            currentHeightOffset = 0f;
        }
        
        Vector3 adjustedOffset = direction * currentDistance;
        adjustedOffset.y = offset.y + currentHeightOffset;
        
        Vector3 desiredPosition = targetPosition + adjustedOffset;
        
        if (Application.isPlaying && smoothFollow)
        {
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        }
        else
        {
            transform.position = desiredPosition;
        }
        
        float targetRotationX = rotation.x;
        
        if (Application.isPlaying && enableDynamicRotation)
        {
            float additionalAngle = distanceRatio * maxRotationAngle;
            targetRotationX = rotation.x + additionalAngle;
            
            currentRotationX = Mathf.Lerp(currentRotationX, targetRotationX, rotationSmoothSpeed * Time.deltaTime);
        }
        else
        {
            currentRotationX = rotation.x;
        }
        
        transform.rotation = Quaternion.Euler(currentRotationX, rotation.y, rotation.z);
        
        if (cameraComponent != null)
        {
            float targetFOV = baseFOV;
            
            if (Application.isPlaying && enableDynamicFOV)
            {
                float fovIncrease = distanceRatio * maxFOVIncrease;
                targetFOV = baseFOV + fovIncrease;
                
                currentFOV = Mathf.Lerp(currentFOV, targetFOV, fovSmoothSpeed * Time.deltaTime);
                cameraComponent.fieldOfView = currentFOV;
            }
            else if (!Application.isPlaying)
            {
                cameraComponent.fieldOfView = baseFOV;
            }
        }
    }
}
