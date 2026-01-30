// SmoothCameraFollow.cs
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [Header("跟随目标")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 targetOffset = new Vector3(0, 0, -10);
    
    [Header("平滑设置")]
    [SerializeField] private float smoothTime = 0.3f;
    [SerializeField] private float maxSpeed = Mathf.Infinity;
    
    [Header("边界限制")]
    [SerializeField] private bool enableBounds = false;
    [SerializeField] private Vector2 minBounds = new Vector2(-10, -10);
    [SerializeField] private Vector2 maxBounds = new Vector2(10, 10);
    
    [Header("视角偏移")]
    [SerializeField] private bool enableLookAhead = false;
    [SerializeField] private float lookAheadFactor = 0.5f;
    [SerializeField] private float lookAheadSmooth = 5f;
    
    private Vector3 velocity = Vector3.zero;
    private Vector3 lookAheadPosition;
    private Vector3 lastTargetPosition;

    void Start()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) target = player.transform;
        }
        
        if (target != null)
        {
            lastTargetPosition = target.position;
            lookAheadPosition = transform.position;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;
        
        FollowTarget();
    }

    void FollowTarget()
    {
        // 计算目标位置
        Vector3 targetPosition = target.position + targetOffset;
        
        // 添加预判偏移
        if (enableLookAhead)
        {
            Vector3 moveDirection = target.position - lastTargetPosition;
            
            if (moveDirection.magnitude > 0.01f)
            {
                lookAheadPosition = Vector3.Lerp(
                    lookAheadPosition,
                    targetPosition + moveDirection.normalized * lookAheadFactor,
                    lookAheadSmooth * Time.deltaTime
                );
            }
            else
            {
                lookAheadPosition = Vector3.Lerp(lookAheadPosition, targetPosition, 
                    lookAheadSmooth * Time.deltaTime);
            }
            
            targetPosition = lookAheadPosition;
            lastTargetPosition = target.position;
        }
        
        // 应用边界限制
        if (enableBounds)
        {
            targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.x);
            targetPosition.y = Mathf.Clamp(targetPosition.y, minBounds.y, maxBounds.y);
        }
        
        // 平滑移动摄像机
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocity,
            smoothTime,
            maxSpeed
        );
    }
    
    // 设置跟随目标
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target != null)
        {
            lastTargetPosition = target.position;
        }
    }
    
    // 设置边界
    public void SetBounds(Vector2 min, Vector2 max)
    {
        minBounds = min;
        maxBounds = max;
        enableBounds = true;
    }
    
    // 禁用边界
    public void DisableBounds()
    {
        enableBounds = false;
    }
    
    // 手动设置相机位置（用于过场动画等）
    public void SnapToTarget()
    {
        if (target != null)
        {
            Vector3 targetPosition = target.position + targetOffset;
            if (enableBounds)
            {
                targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.x);
                targetPosition.y = Mathf.Clamp(targetPosition.y, minBounds.y, maxBounds.y);
            }
            transform.position = targetPosition;
            velocity = Vector3.zero;
        }
    }
    
    // 调试：显示边界
    void OnDrawGizmosSelected()
    {
        if (enableBounds)
        {
            Gizmos.color = Color.green;
            Vector3 center = new Vector3(
                (minBounds.x + maxBounds.x) / 2,
                (minBounds.y + maxBounds.y) / 2,
                transform.position.z
            );
            Vector3 size = new Vector3(
                maxBounds.x - minBounds.x,
                maxBounds.y - minBounds.y,
                0.1f
            );
            Gizmos.DrawWireCube(center, size);
        }
    }
}