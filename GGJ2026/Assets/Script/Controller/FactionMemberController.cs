// FactionMemberController.cs - 可切换的轨道/随机移动，带有返回原范围功能
using UnityEngine;

public class FactionMemberController : MonoBehaviour
{
    public enum MovementMode
    {
        Orbit,      // 绕圈运动
        Random,     // 随机移动
        Mixed,      // 混合模式（根据时间或条件切换）
        Returning   // 返回原范围模式（新增）
    }
    
    [Header("移动模式")]
    [SerializeField] private MovementMode currentMode = MovementMode.Orbit;
    [SerializeField] private bool allowModeSwitching = true;
    [SerializeField] private float modeSwitchInterval = 10f;
    
    [Header("轨道运动设置")]
    [SerializeField] private Transform orbitCenter;
    [SerializeField] private float orbitRadius = 3f;
    [SerializeField] private float orbitSpeed = 30f;
    [SerializeField] private bool clockwise = true;
    
    [Header("随机移动设置")]
    [SerializeField] private float randomMoveRadius = 5f;
    [SerializeField] private float randomMoveSpeed = 2f;
    [SerializeField] private float randomTargetChangeInterval = 3f;
    [SerializeField] private float targetReachThreshold = 0.2f;
    
    [Header("返回原范围设置")]
    [SerializeField] private float returnSpeed = 3f; // 返回原范围的速度
    [SerializeField] private float returnThreshold = 0.1f; // 返回完成的阈值
    [SerializeField] private float returnCheckInterval = 1f; // 检查是否需要返回的间隔
    [SerializeField] private float outOfRangeDistance = 1.5f; // 超过此距离视为离开范围
    
    [Header("随机性设置")]
    [SerializeField] private float radiusVariation = 0.5f;
    [SerializeField] private float speedVariation = 5f;
    [SerializeField] private float randomSpeedVariation = 0.5f;
    
    [Header("视觉设置")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private bool faceMovementDirection = false;
    [SerializeField] private Color orbitColor = Color.blue;
    [SerializeField] private Color randomColor = Color.green;
    [SerializeField] private Color returnColor = Color.yellow; // 返回模式颜色
    
    // 轨道运动变量
    private float currentOrbitRadius;
    private float currentOrbitSpeed;
    private float currentOrbitAngle;
    private Vector2 orbitCenterPos;
    
    // 随机移动变量
    private Vector2 randomCenterPos;
    private Vector2 randomTargetPosition;
    private float randomMoveTimer;
    private float currentRandomSpeed;
    
    // 返回原范围变量
    private Vector2 originalRangeCenter; // 原始范围中心
    private float originalRangeRadius;   // 原始范围半径
    private Vector2 returnTarget;        // 返回目标位置
    private float returnCheckTimer;      // 返回检查计时器
    private bool isReturning = false;    // 是否正在返回
    private MovementMode modeBeforeReturn; // 返回前的模式
    
    // 通用变量
    private float modeSwitchTimer;
    private bool isInMixedMode = false;
    
    [Header("物理设置")]
    [SerializeField] private bool usePhysics = true; // 是否使用物理
    [SerializeField] private float physicsDamping = 0.8f; // 物理阻尼
    [SerializeField] private float returnPhysicsSpeed = 2f; // 返回时的物理速度
    
    // 物理相关变量
    private Vector2 collisionForce;
    private float collisionForceTimer;
    private bool isColliding = false;
    // 物理相关
    private Rigidbody2D rb;
    private Vector2 lastFrameVelocity;
    private float collisionForceThreshold = 2f; // 碰撞力阈值，超过此值视为被撞开
    
    private void Start()
    {
        InitializeMovement();
        
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
            
        // 初始化物理组件
        InitializePhysics();
        
        RecordOriginalRange();
        returnCheckTimer = returnCheckInterval;
    }
    
    private void InitializePhysics()
    {
        // 获取或添加Rigidbody2D
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
        }
        
        // 设置物理属性
        rb.gravityScale = 0f; // 无重力
        rb.drag = 5f; // 线性阻力
        rb.angularDrag = 5f; // 角阻力
        rb.constraints = RigidbodyConstraints2D.FreezeRotation; // 锁定旋转
        
        // 设置碰撞体
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            // 如果没有碰撞体，添加一个
            BoxCollider2D boxCollider = gameObject.AddComponent<BoxCollider2D>();
            boxCollider.size = new Vector2(1f, 1f);
        }
    }
    
    private void Update()
    {
        // 更新模式切换计时器
        UpdateModeSwitching();
        
        // 检查是否需要返回原范围
        CheckReturnToRange();
        
        // 如果不是返回模式，执行正常移动
        if (currentMode != MovementMode.Returning)
        {
            switch (currentMode)
            {
                case MovementMode.Orbit:
                    UpdateOrbitMovement();
                    break;
                    
                case MovementMode.Random:
                    UpdateRandomMovement();
                    break;
                    
                case MovementMode.Mixed:
                    UpdateMixedMovement();
                    break;
            }
        }
        
        // 更新视觉反馈
        UpdateVisualFeedback();
        
        // 更新碰撞力计时器
        if (collisionForceTimer > 0)
        {
            collisionForceTimer -= Time.deltaTime;
            if (collisionForceTimer <= 0)
            {
                collisionForce = Vector2.zero;
                isColliding = false;
            }
        }
    }
    
    private void FixedUpdate()
    {
        // 处理物理更新
        HandlePhysics();
        
        // 如果在返回模式，使用物理方式返回
        if (currentMode == MovementMode.Returning)
        {
            HandleReturnPhysics();
        }
    }
    
    private void HandlePhysics()
    {
        if (!usePhysics || rb == null) return;
        
        // 如果不是返回模式，限制速度避免飞得太远
        if (currentMode != MovementMode.Returning)
        {
            // 限制最大速度
            if (rb.velocity.magnitude > 5f)
            {
                rb.velocity = rb.velocity.normalized * 5f;
            }
            
            // 应用阻尼
            rb.velocity *= physicsDamping;
            
            // 如果速度很小，停止移动
            if (rb.velocity.magnitude < 0.1f)
            {
                rb.velocity = Vector2.zero;
            }
        }
    }
    
    private void HandleReturnPhysics()
    {
        if (!usePhysics || rb == null || !isReturning) return;
        
        // 计算返回方向
        Vector2 directionToTarget = (returnTarget - (Vector2)transform.position).normalized;
        
        // 计算到目标的距离
        float distanceToTarget = Vector2.Distance(transform.position, returnTarget);
        
        if (distanceToTarget > returnThreshold)
        {
            // 使用力或速度移动到目标
            Vector2 desiredVelocity = directionToTarget * returnPhysicsSpeed;
            
            // 计算所需的力（基于当前速度）
            Vector2 force = (desiredVelocity - rb.velocity) * 10f;
            
            // 应用力
            rb.AddForce(force);
            
            // 限制最大速度
            if (rb.velocity.magnitude > returnPhysicsSpeed * 1.5f)
            {
                rb.velocity = rb.velocity.normalized * returnPhysicsSpeed * 1.5f;
            }
            
            // 更新朝向
            if (faceMovementDirection && spriteRenderer != null)
            {
                spriteRenderer.flipX = directionToTarget.x < 0;
            }
        }
        else
        {
            // 接近目标时减速
            rb.velocity *= 0.9f;
            
            // 到达目标点
            if (rb.velocity.magnitude < 0.5f && distanceToTarget < returnThreshold * 2f)
            {
                CompleteReturnToRange();
            }
        }
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 检测是否被玩家撞开
        if (collision.gameObject.CompareTag("Player"))
        {
            // 记录碰撞信息
            isColliding = true;
            collisionForce = collision.relativeVelocity;
            collisionForceTimer = 0.5f; // 记录碰撞力0.5秒
            
            // 计算碰撞力大小
            float collisionForceMagnitude = collisionForce.magnitude;
            
            if (collisionForceMagnitude > collisionForceThreshold)
            {
                // 被玩家撞开，标记需要返回原范围
                StartReturnToRange();
                
                // 可以添加视觉效果
                if (spriteRenderer != null)
                {
                    StartCoroutine(FlashEffect(0.5f));
                }
                
                Debug.Log($"{gameObject.name} 被玩家撞开，碰撞力: {collisionForceMagnitude}");
            }
        }
    }
    
    private void OnCollisionStay2D(Collision2D collision)
    {
        // 持续碰撞时也记录
        if (collision.gameObject.CompareTag("Player"))
        {
            isColliding = true;
            collisionForceTimer = 0.3f;
        }
    }
    
    private void OnCollisionExit2D(Collision2D collision)
    {
        // 碰撞结束
        if (collision.gameObject.CompareTag("Player"))
        {
            isColliding = false;
        }
    }
    
    private void StartReturnToRange()
    {
        // 保存当前模式
        modeBeforeReturn = currentMode;
        
        // 切换到返回模式
        currentMode = MovementMode.Returning;
        isReturning = true;
        
        // 清除当前的物理速度
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        
        // 计算返回目标（在原始范围的边缘）
        Vector2 currentPos = transform.position;
        Vector2 directionToCenter = (originalRangeCenter - currentPos).normalized;
        
        // 确保方向有效
        if (directionToCenter.magnitude < 0.1f)
        {
            // 如果已经在中心，随机选择一个方向
            directionToCenter = Random.insideUnitCircle.normalized;
        }
        
        // 计算返回目标点（在原始范围的80%位置）
        returnTarget = originalRangeCenter - directionToCenter * (originalRangeRadius * 0.8f);
        
        // 确保目标点在范围内
        float distanceToCenter = Vector2.Distance(returnTarget, originalRangeCenter);
        if (distanceToCenter > originalRangeRadius)
        {
            returnTarget = originalRangeCenter + (returnTarget - originalRangeCenter).normalized * originalRangeRadius;
        }
        
        Debug.Log($"{gameObject.name} 开始返回原范围，目标: {returnTarget}");
    }
    
    private void UpdateReturnMovement()
    {
        if (!isReturning) return;
        
        // 如果不是使用物理模式，直接移动
        if (!usePhysics)
        {
            float distanceToTarget = Vector2.Distance(transform.position, returnTarget);
            
            if (distanceToTarget > returnThreshold)
            {
                // 直接向目标点移动
                Vector2 moveDirection = (returnTarget - (Vector2)transform.position).normalized;
                transform.position = Vector2.MoveTowards(
                    transform.position, 
                    returnTarget, 
                    returnSpeed * Time.deltaTime
                );
                
                // 更新朝向
                if (faceMovementDirection && spriteRenderer != null)
                {
                    spriteRenderer.flipX = moveDirection.x < 0;
                }
            }
            else
            {
                CompleteReturnToRange();
            }
        }
    }
    
    private void CompleteReturnToRange()
    {
        // 返回完成，恢复原来的模式
        isReturning = false;
        currentMode = modeBeforeReturn;
        
        // 确保速度清零
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        
        // 根据恢复的模式重新初始化
        switch (currentMode)
        {
            case MovementMode.Orbit:
                InitializeOrbitMode();
                break;
                
            case MovementMode.Random:
                InitializeRandomMode();
                break;
                
            case MovementMode.Mixed:
                InitializeMixedMode();
                break;
        }
        
        Debug.Log($"{gameObject.name} 已返回原范围，恢复{modeBeforeReturn}模式");
    }
    
    private void InitializeMovement()
    {
        // 初始化轨道运动
        if (orbitCenter != null)
        {
            orbitCenterPos = orbitCenter.position;
        }
        else
        {
            orbitCenterPos = transform.position;
        }
        
        currentOrbitRadius = orbitRadius + Random.Range(-radiusVariation, radiusVariation);
        currentOrbitSpeed = orbitSpeed + Random.Range(-speedVariation, speedVariation);
        currentOrbitAngle = Random.Range(0f, 360f);
        
        // 初始化随机移动
        randomCenterPos = transform.position;
        currentRandomSpeed = randomMoveSpeed + Random.Range(-randomSpeedVariation, randomSpeedVariation);
        SetNewRandomTarget();
        
        // 设置混合模式初始状态
        if (currentMode == MovementMode.Mixed)
        {
            isInMixedMode = true;
            modeSwitchTimer = modeSwitchInterval;
            SwitchToSubMode(Random.Range(0, 2) == 0 ? MovementMode.Orbit : MovementMode.Random);
        }
    }
    
    private void RecordOriginalRange()
    {
        // 根据当前模式记录原始范围
        if (currentMode == MovementMode.Orbit)
        {
            originalRangeCenter = orbitCenterPos;
            originalRangeRadius = orbitRadius;
        }
        else if (currentMode == MovementMode.Random || currentMode == MovementMode.Mixed)
        {
            originalRangeCenter = randomCenterPos;
            originalRangeRadius = randomMoveRadius;
        }
    }
    
    private void CheckReturnToRange()
    {
        // 如果不是返回模式，检查是否需要返回
        if (currentMode != MovementMode.Returning)
        {
            returnCheckTimer -= Time.deltaTime;
            
            if (returnCheckTimer <= 0f)
            {
                // 检查是否离开原始范围过远
                float distanceFromCenter = Vector2.Distance(transform.position, originalRangeCenter);
                
                if (distanceFromCenter > originalRangeRadius * outOfRangeDistance)
                {
                    // 离开范围过远，开始返回
                    StartReturnToRange();
                }
                
                // 重置检查计时器
                returnCheckTimer = returnCheckInterval;
            }
        }
    }
    
    
    private void UpdateModeSwitching()
    {
        if (currentMode != MovementMode.Mixed || !allowModeSwitching) return;
        
        modeSwitchTimer -= Time.deltaTime;
        
        if (modeSwitchTimer <= 0f)
        {
            MovementMode newSubMode = (currentMode == MovementMode.Orbit) ? 
                MovementMode.Random : MovementMode.Orbit;
            
            SwitchToSubMode(newSubMode);
            modeSwitchTimer = modeSwitchInterval + Random.Range(-2f, 2f);
        }
    }
    
    private void SwitchToSubMode(MovementMode newMode)
    {
        if (newMode == MovementMode.Orbit)
        {
            if (orbitCenter != null)
                orbitCenterPos = orbitCenter.position;
            
            Vector2 toObject = (Vector2)transform.position - orbitCenterPos;
            currentOrbitAngle = Mathf.Atan2(toObject.y, toObject.x) * Mathf.Rad2Deg;
            currentOrbitRadius = orbitRadius + Random.Range(-radiusVariation, radiusVariation);
        }
        else if (newMode == MovementMode.Random)
        {
            randomCenterPos = transform.position;
            SetNewRandomTarget();
        }
        
        Debug.Log($"{gameObject.name} 切换到 {newMode} 子模式");
    }
    
    private void UpdateOrbitMovement()
    {
        float direction = clockwise ? -1f : 1f;
        currentOrbitAngle += direction * currentOrbitSpeed * Time.deltaTime;
        currentOrbitAngle %= 360f;
        
        float angleRad = currentOrbitAngle * Mathf.Deg2Rad;
        Vector2 newPosition = orbitCenterPos + new Vector2(
            Mathf.Cos(angleRad) * currentOrbitRadius,
            Mathf.Sin(angleRad) * currentOrbitRadius
        );
        
        transform.position = newPosition;
        UpdateRotationForOrbit(direction, angleRad);
    }
    
    private void UpdateRotationForOrbit(float direction, float angleRad)
    {
        if (faceMovementDirection && spriteRenderer != null)
        {
            Vector2 tangentDirection = new Vector2(
                -Mathf.Sin(angleRad),
                Mathf.Cos(angleRad)
            ) * direction;
            
            spriteRenderer.flipX = tangentDirection.x < 0;
        }
    }
    
    private void UpdateRandomMovement()
    {
        randomMoveTimer -= Time.deltaTime;
        
        if (Vector2.Distance(transform.position, randomTargetPosition) < targetReachThreshold || 
            randomMoveTimer <= 0)
        {
            SetNewRandomTarget();
        }
        
        transform.position = Vector2.MoveTowards(
            transform.position, 
            randomTargetPosition, 
            currentRandomSpeed * Time.deltaTime
        );
        
        UpdateRotationForRandomMovement();
    }
    
    private void UpdateRotationForRandomMovement()
    {
        if (faceMovementDirection && spriteRenderer != null)
        {
            Vector2 moveDirection = (randomTargetPosition - (Vector2)transform.position).normalized;
            if (moveDirection.magnitude > 0.1f)
            {
                spriteRenderer.flipX = moveDirection.x < 0;
            }
        }
    }
    
    private void UpdateMixedMovement()
    {
        if (currentMode == MovementMode.Orbit)
            UpdateOrbitMovement();
        else if (currentMode == MovementMode.Random)
            UpdateRandomMovement();
    }
    
    private void SetNewRandomTarget()
    {
        Vector2 randomDirection = Random.insideUnitCircle * randomMoveRadius;
        randomTargetPosition = randomCenterPos + randomDirection;
        randomMoveTimer = randomTargetChangeInterval + Random.Range(-1f, 1f);
        currentRandomSpeed = randomMoveSpeed + Random.Range(-randomSpeedVariation, randomSpeedVariation);
    }
    
    private void UpdateVisualFeedback()
    {
        if (spriteRenderer == null) return;
        
        // 根据模式改变颜色
        switch (currentMode)
        {
            case MovementMode.Orbit:
                spriteRenderer.color = Color.Lerp(spriteRenderer.color, orbitColor, Time.deltaTime * 2f);
                break;
                
            case MovementMode.Random:
                spriteRenderer.color = Color.Lerp(spriteRenderer.color, randomColor, Time.deltaTime * 2f);
                break;
                
            case MovementMode.Returning:
                spriteRenderer.color = Color.Lerp(spriteRenderer.color, returnColor, Time.deltaTime * 5f);
                break;
                
            case MovementMode.Mixed:
                Color targetColor = (isInMixedMode && currentMode == MovementMode.Orbit) ? 
                    orbitColor : randomColor;
                spriteRenderer.color = Color.Lerp(spriteRenderer.color, targetColor, Time.deltaTime * 2f);
                break;
        }
    }
    
    // 闪烁效果协程
    private System.Collections.IEnumerator FlashEffect(float duration)
    {
        Color originalColor = spriteRenderer.color;
        float flashTime = 0f;
        int flashCount = 3;
        
        for (int i = 0; i < flashCount; i++)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(duration / (flashCount * 2));
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(duration / (flashCount * 2));
        }
    }
    
    // 公开方法：切换移动模式
    public void SwitchMovementMode(MovementMode newMode)
    {
        if (!allowModeSwitching) return;
        
        isInMixedMode = false;
        currentMode = newMode;
        
        if (newMode == MovementMode.Orbit)
            InitializeOrbitMode();
        else if (newMode == MovementMode.Random)
            InitializeRandomMode();
        else if (newMode == MovementMode.Mixed)
            InitializeMixedMode();
        else if (newMode == MovementMode.Returning)
            StartReturnToRange();
        
        Debug.Log($"{gameObject.name} 切换到 {newMode} 模式");
    }
    
    private void InitializeOrbitMode()
    {
        if (orbitCenter != null)
        {
            orbitCenterPos = orbitCenter.position;
            Vector2 toObject = (Vector2)transform.position - orbitCenterPos;
            currentOrbitAngle = Mathf.Atan2(toObject.y, toObject.x) * Mathf.Rad2Deg;
        }
        else
        {
            orbitCenterPos = transform.position;
            currentOrbitAngle = 0f;
        }
        
        // 更新原始范围
        originalRangeCenter = orbitCenterPos;
        originalRangeRadius = orbitRadius;
    }
    
    private void InitializeRandomMode()
    {
        randomCenterPos = transform.position;
        SetNewRandomTarget();
        
        // 更新原始范围
        originalRangeCenter = randomCenterPos;
        originalRangeRadius = randomMoveRadius;
    }
    
    private void InitializeMixedMode()
    {
        isInMixedMode = true;
        modeSwitchTimer = modeSwitchInterval;
        SwitchToSubMode(Random.Range(0, 2) == 0 ? MovementMode.Orbit : MovementMode.Random);
        
        // 更新原始范围
        originalRangeCenter = randomCenterPos;
        originalRangeRadius = randomMoveRadius;
    }
    
    // 强制开始返回原范围（可从外部调用）
    public void ForceReturnToRange()
    {
        StartReturnToRange();
    }
    
    // 设置原始范围中心
    public void SetOriginalRangeCenter(Vector2 center)
    {
        originalRangeCenter = center;
    }
    
    // 设置原始范围半径
    public void SetOriginalRangeRadius(float radius)
    {
        originalRangeRadius = radius;
    }
    
    // 设置轨道中心
    public void SetOrbitCenter(Transform newCenter)
    {
        orbitCenter = newCenter;
        if (orbitCenter != null)
        {
            orbitCenterPos = orbitCenter.position;
            originalRangeCenter = orbitCenterPos;
        }
    }
    
    // 获取当前模式
    public MovementMode GetCurrentMode()
    {
        return currentMode;
    }
    
    // 在轨道模式和随机模式之间切换
    public void ToggleMovementMode()
    {
        if (currentMode == MovementMode.Orbit)
            SwitchMovementMode(MovementMode.Random);
        else if (currentMode == MovementMode.Random)
            SwitchMovementMode(MovementMode.Orbit);
        else if (currentMode == MovementMode.Mixed)
        {
            MovementMode newSubMode = (currentMode == MovementMode.Orbit) ? 
                MovementMode.Random : MovementMode.Orbit;
            SwitchToSubMode(newSubMode);
        }
    }
    
    // 设置随机移动范围
    public void SetRandomMoveRange(float newRadius, Vector2 newCenter)
    {
        randomMoveRadius = newRadius;
        randomCenterPos = newCenter;
        
        // 更新原始范围
        originalRangeCenter = randomCenterPos;
        originalRangeRadius = randomMoveRadius;
        
        if (currentMode == MovementMode.Random || 
            (currentMode == MovementMode.Mixed && isInMixedMode && currentMode == MovementMode.Random))
        {
            SetNewRandomTarget();
        }
    }
    
    // 设置轨道参数
    public void SetOrbitParameters(float radius, float speed, bool isClockwise = true)
    {
        orbitRadius = radius;
        orbitSpeed = speed;
        clockwise = isClockwise;
        
        // 更新原始范围
        originalRangeRadius = orbitRadius;
        
        currentOrbitRadius = orbitRadius + Random.Range(-radiusVariation, radiusVariation);
        currentOrbitSpeed = orbitSpeed + Random.Range(-speedVariation, speedVariation);
    }
    
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;
        
        // 绘制原始范围
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f); // 半透明橙色
        Gizmos.DrawWireSphere(originalRangeCenter, originalRangeRadius);
        
        // 绘制离开范围阈值
        Gizmos.color = new Color(1f, 0f, 0f, 0.2f); // 半透明红色
        Gizmos.DrawWireSphere(originalRangeCenter, originalRangeRadius * outOfRangeDistance);
        
        // 根据模式绘制不同的Gizmo
        switch (currentMode)
        {
            case MovementMode.Orbit:
                DrawOrbitGizmos();
                break;
                
            case MovementMode.Random:
                DrawRandomGizmos();
                break;
                
            case MovementMode.Returning:
                DrawReturnGizmos();
                break;
                
            case MovementMode.Mixed:
                if (isInMixedMode)
                {
                    if (currentMode == MovementMode.Orbit)
                        DrawOrbitGizmos();
                    else
                        DrawRandomGizmos();
                }
                break;
        }
    }
    
    private void DrawOrbitGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(orbitCenterPos, currentOrbitRadius);
        
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(orbitCenterPos, transform.position);
        
        Gizmos.color = Color.yellow;
        float angleRad = currentOrbitAngle * Mathf.Deg2Rad;
        Vector2 tangentDirection = new Vector2(
            -Mathf.Sin(angleRad),
            Mathf.Cos(angleRad)
        ) * (clockwise ? -1f : 1f);
        Gizmos.DrawRay(transform.position, tangentDirection * 0.5f);
    }
    
    private void DrawRandomGizmos()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
        Gizmos.DrawWireSphere(randomCenterPos, randomMoveRadius);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(randomTargetPosition, 0.2f);
        
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, randomTargetPosition);
        
        if (faceMovementDirection)
        {
            Vector2 direction = (randomTargetPosition - (Vector2)transform.position).normalized;
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, direction * 0.5f);
        }
    }
    
    private void DrawReturnGizmos()
    {
        // 绘制返回目标点
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(returnTarget, 0.3f);
        
        // 绘制返回路径
        Gizmos.color = new Color(1f, 1f, 0f, 0.5f);
        Gizmos.DrawLine(transform.position, returnTarget);
        
        // 绘制返回方向箭头
        Vector2 direction = (returnTarget - (Vector2)transform.position).normalized;
        Gizmos.DrawRay(transform.position, direction * 0.8f);
        
        // 绘制返回状态指示器
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}