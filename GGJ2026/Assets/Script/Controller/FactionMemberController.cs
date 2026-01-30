// SwitchedMovementController.cs - 可切换的轨道/随机移动
using UnityEngine;

public class FactionMemberController : MonoBehaviour
{
    public enum MovementMode
    {
        Orbit,      // 绕圈运动
        Random,     // 随机移动
        Mixed       // 混合模式（根据时间或条件切换）
    }
    
    [Header("移动模式")]
    [SerializeField] private MovementMode currentMode = MovementMode.Orbit;
    [SerializeField] private bool allowModeSwitching = true; // 允许运行时切换模式
    [SerializeField] private float modeSwitchInterval = 10f; // 模式切换间隔（混合模式使用）
    
    [Header("轨道运动设置")]
    [SerializeField] private Transform orbitCenter;
    [SerializeField] private float orbitRadius = 3f;
    [SerializeField] private float orbitSpeed = 30f;
    [SerializeField] private bool clockwise = true;
    
    [Header("随机移动设置")]
    [SerializeField] private float randomMoveRadius = 5f; // 随机移动范围半径
    [SerializeField] private float randomMoveSpeed = 2f; // 随机移动速度
    [SerializeField] private float randomTargetChangeInterval = 3f; // 目标点更换间隔
    [SerializeField] private float targetReachThreshold = 0.2f; // 到达目标点的阈值
    
    [Header("随机性设置")]
    [SerializeField] private float radiusVariation = 0.5f;
    [SerializeField] private float speedVariation = 5f;
    [SerializeField] private float randomSpeedVariation = 0.5f;
    
    [Header("视觉设置")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private bool faceMovementDirection = false;
    [SerializeField] private Color orbitColor = Color.blue;
    [SerializeField] private Color randomColor = Color.green;
    
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
    
    // 通用变量
    private float modeSwitchTimer;
    private bool isInMixedMode = false;
    
    private void Start()
    {
        InitializeMovement();
        
        // 如果没有指定精灵渲染器，尝试获取
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }
    
    private void Update()
    {
        // 更新模式切换计时器（混合模式）
        UpdateModeSwitching();
        
        // 根据当前模式执行相应的移动
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
        
        // 更新视觉反馈
        UpdateVisualFeedback();
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
            orbitCenterPos = transform.position; // 如果没有指定中心，使用当前位置
        }
        
        currentOrbitRadius = orbitRadius + Random.Range(-radiusVariation, radiusVariation);
        currentOrbitSpeed = orbitSpeed + Random.Range(-speedVariation, speedVariation);
        currentOrbitAngle = Random.Range(0f, 360f);
        
        // 初始化随机移动
        randomCenterPos = transform.position; // 以当前位置为随机移动中心
        currentRandomSpeed = randomMoveSpeed + Random.Range(-randomSpeedVariation, randomSpeedVariation);
        SetNewRandomTarget();
        
        // 设置混合模式初始状态
        if (currentMode == MovementMode.Mixed)
        {
            isInMixedMode = true;
            modeSwitchTimer = modeSwitchInterval;
            // 随机选择初始子模式
            SwitchToSubMode(Random.Range(0, 2) == 0 ? MovementMode.Orbit : MovementMode.Random);
        }
    }
    
    private void UpdateModeSwitching()
    {
        if (currentMode != MovementMode.Mixed || !allowModeSwitching) return;
        
        modeSwitchTimer -= Time.deltaTime;
        
        if (modeSwitchTimer <= 0f)
        {
            // 切换子模式
            MovementMode newSubMode = (currentMode == MovementMode.Orbit) ? 
                MovementMode.Random : MovementMode.Orbit;
            
            SwitchToSubMode(newSubMode);
            
            // 重置计时器（可以添加随机变化）
            modeSwitchTimer = modeSwitchInterval + Random.Range(-2f, 2f);
        }
    }
    
    private void SwitchToSubMode(MovementMode newMode)
    {
        // 注意：这里不改变currentMode（主模式），而是改变内部状态
        // 对于混合模式，我们通过这个函数切换子模式
        
        if (newMode == MovementMode.Orbit)
        {
            // 切换到轨道模式
            if (orbitCenter != null)
            {
                orbitCenterPos = orbitCenter.position;
            }
            
            // 计算当前位置相对于中心的初始角度
            Vector2 toObject = (Vector2)transform.position - orbitCenterPos;
            currentOrbitAngle = Mathf.Atan2(toObject.y, toObject.x) * Mathf.Rad2Deg;
            
            // 设置轨道半径（保持随机性）
            currentOrbitRadius = orbitRadius + Random.Range(-radiusVariation, radiusVariation);
        }
        else if (newMode == MovementMode.Random)
        {
            // 切换到随机移动模式
            randomCenterPos = transform.position; // 以当前位置为随机移动中心
            SetNewRandomTarget();
        }
        
        // 更新内部状态（这里用currentMode来存储当前的子模式）
        // 注意：在混合模式下，我们实际上是在两个子模式间切换
        // 为了简化，我们在混合模式下直接修改currentMode来切换子模式
        // 但在退出混合模式时，需要恢复到原始模式
        
        Debug.Log($"{gameObject.name} 切换到 {newMode} 子模式");
    }
    
    private void UpdateOrbitMovement()
    {
        // 更新角度
        float direction = clockwise ? -1f : 1f;
        currentOrbitAngle += direction * currentOrbitSpeed * Time.deltaTime;
        currentOrbitAngle %= 360f;
        
        // 计算新位置
        float angleRad = currentOrbitAngle * Mathf.Deg2Rad;
        Vector2 newPosition = orbitCenterPos + new Vector2(
            Mathf.Cos(angleRad) * currentOrbitRadius,
            Mathf.Sin(angleRad) * currentOrbitRadius
        );
        
        // 应用位置
        transform.position = newPosition;
        
        // 更新朝向
        UpdateRotationForOrbit(direction, angleRad);
    }
    
    private void UpdateRotationForOrbit(float direction, float angleRad)
    {
        if (faceMovementDirection && spriteRenderer != null)
        {
            // 计算切线方向（运动方向）
            Vector2 tangentDirection = new Vector2(
                -Mathf.Sin(angleRad),
                Mathf.Cos(angleRad)
            ) * direction;
            
            // 根据切线方向翻转精灵
            spriteRenderer.flipX = tangentDirection.x < 0;
        }
    }
    
    private void UpdateRandomMovement()
    {
        randomMoveTimer -= Time.deltaTime;
        
        // 到达目标点或时间到，设置新目标
        if (Vector2.Distance(transform.position, randomTargetPosition) < targetReachThreshold || 
            randomMoveTimer <= 0)
        {
            SetNewRandomTarget();
        }
        
        // 向目标点移动
        transform.position = Vector2.MoveTowards(
            transform.position, 
            randomTargetPosition, 
            currentRandomSpeed * Time.deltaTime
        );
        
        // 更新朝向
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
        // 混合模式下，currentMode实际上存储当前的子模式
        // 所以直接调用对应的更新函数
        if (currentMode == MovementMode.Orbit)
        {
            UpdateOrbitMovement();
        }
        else if (currentMode == MovementMode.Random)
        {
            UpdateRandomMovement();
        }
    }
    
    private void SetNewRandomTarget()
    {
        // 在随机移动范围内选择一个新目标点
        Vector2 randomDirection = Random.insideUnitCircle * randomMoveRadius;
        randomTargetPosition = randomCenterPos + randomDirection;
        
        // 重置计时器
        randomMoveTimer = randomTargetChangeInterval + Random.Range(-1f, 1f);
        
        // 随机调整速度
        currentRandomSpeed = randomMoveSpeed + Random.Range(-randomSpeedVariation, randomSpeedVariation);
    }
    
    private void UpdateVisualFeedback()
    {
        if (spriteRenderer == null) return;
        
        // 根据模式改变颜色（可选）
        if (currentMode == MovementMode.Orbit)
        {
            // 轨道模式：蓝色调
            spriteRenderer.color = Color.Lerp(spriteRenderer.color, orbitColor, Time.deltaTime * 2f);
        }
        else if (currentMode == MovementMode.Random)
        {
            // 随机模式：绿色调
            spriteRenderer.color = Color.Lerp(spriteRenderer.color, randomColor, Time.deltaTime * 2f);
        }
        else if (currentMode == MovementMode.Mixed)
        {
            // 混合模式：根据当前子模式决定颜色
            Color targetColor = (isInMixedMode && currentMode == MovementMode.Orbit) ? 
                orbitColor : randomColor;
            spriteRenderer.color = Color.Lerp(spriteRenderer.color, targetColor, Time.deltaTime * 2f);
        }
    }
    
    // 公开方法：切换移动模式
    public void SwitchMovementMode(MovementMode newMode)
    {
        if (!allowModeSwitching) return;
        
        // 如果当前是混合模式，退出混合模式
        isInMixedMode = false;
        
        // 切换模式
        currentMode = newMode;
        
        // 初始化新模式
        if (newMode == MovementMode.Orbit)
        {
            InitializeOrbitMode();
        }
        else if (newMode == MovementMode.Random)
        {
            InitializeRandomMode();
        }
        else if (newMode == MovementMode.Mixed)
        {
            InitializeMixedMode();
        }
        
        Debug.Log($"{gameObject.name} 切换到 {newMode} 模式");
    }
    
    private void InitializeOrbitMode()
    {
        // 计算当前位置相对于中心的初始角度
        if (orbitCenter != null)
        {
            orbitCenterPos = orbitCenter.position;
            Vector2 toObject = (Vector2)transform.position - orbitCenterPos;
            currentOrbitAngle = Mathf.Atan2(toObject.y, toObject.x) * Mathf.Rad2Deg;
        }
        else
        {
            // 如果没有指定中心，使用当前位置为中心
            orbitCenterPos = transform.position;
            currentOrbitAngle = 0f;
        }
    }
    
    private void InitializeRandomMode()
    {
        // 设置随机移动中心为当前位置
        randomCenterPos = transform.position;
        SetNewRandomTarget();
    }
    
    private void InitializeMixedMode()
    {
        isInMixedMode = true;
        modeSwitchTimer = modeSwitchInterval;
        
        // 随机选择初始子模式
        SwitchToSubMode(Random.Range(0, 2) == 0 ? MovementMode.Orbit : MovementMode.Random);
    }
    
    // 公开方法：设置轨道中心
    public void SetOrbitCenter(Transform newCenter)
    {
        orbitCenter = newCenter;
        if (orbitCenter != null)
        {
            orbitCenterPos = orbitCenter.position;
        }
    }
    
    // 公开方法：获取当前模式
    public MovementMode GetCurrentMode()
    {
        return currentMode;
    }
    
    // 公开方法：在轨道模式和随机模式之间切换
    public void ToggleMovementMode()
    {
        if (currentMode == MovementMode.Orbit)
        {
            SwitchMovementMode(MovementMode.Random);
        }
        else if (currentMode == MovementMode.Random)
        {
            SwitchMovementMode(MovementMode.Orbit);
        }
        else if (currentMode == MovementMode.Mixed)
        {
            // 混合模式下切换子模式
            MovementMode newSubMode = (currentMode == MovementMode.Orbit) ? 
                MovementMode.Random : MovementMode.Orbit;
            SwitchToSubMode(newSubMode);
        }
    }
    
    // 公开方法：设置随机移动范围
    public void SetRandomMoveRange(float newRadius, Vector2 newCenter)
    {
        randomMoveRadius = newRadius;
        randomCenterPos = newCenter;
        
        // 如果当前是随机模式，重新设置目标
        if (currentMode == MovementMode.Random || 
            (currentMode == MovementMode.Mixed && isInMixedMode && currentMode == MovementMode.Random))
        {
            SetNewRandomTarget();
        }
    }
    
    // 公开方法：设置轨道参数
    public void SetOrbitParameters(float radius, float speed, bool isClockwise = true)
    {
        orbitRadius = radius;
        orbitSpeed = speed;
        clockwise = isClockwise;
        
        // 重新计算当前半径和速度（保持随机变化）
        currentOrbitRadius = orbitRadius + Random.Range(-radiusVariation, radiusVariation);
        currentOrbitSpeed = orbitSpeed + Random.Range(-speedVariation, speedVariation);
    }
    
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;
        
        // 根据模式绘制不同的Gizmo
        switch (currentMode)
        {
            case MovementMode.Orbit:
                DrawOrbitGizmos();
                break;
                
            case MovementMode.Random:
                DrawRandomGizmos();
                break;
                
            case MovementMode.Mixed:
                // 混合模式下根据当前子模式绘制
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
        // 绘制轨道
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(orbitCenterPos, currentOrbitRadius);
        
        // 绘制中心到物体的线
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(orbitCenterPos, transform.position);
        
        // 绘制移动方向指示器
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
        // 绘制随机移动范围
        Gizmos.color = new Color(0f, 1f, 0f, 0.2f); // 半透明绿色
        Gizmos.DrawWireSphere(randomCenterPos, randomMoveRadius);
        
        // 绘制当前目标点
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(randomTargetPosition, 0.2f);
        
        // 绘制到目标点的线
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, randomTargetPosition);
        
        // 绘制移动方向
        if (faceMovementDirection)
        {
            Vector2 direction = (randomTargetPosition - (Vector2)transform.position).normalized;
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, direction * 0.5f);
        }
    }
}