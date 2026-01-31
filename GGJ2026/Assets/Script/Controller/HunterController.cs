// HunterController.cs - 根据新需求修改版本
using UnityEngine;
using System.Collections.Generic;

public class HunterController : MonoBehaviour
{
    [Header("追捕设置")]
    public GameManager.Faction faction = GameManager.Faction.FactionA;
    [SerializeField] private float baseMoveSpeed = 3f;
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private float wanderRadius = 5f;
    [SerializeField] private float wanderInterval = 3f;
    
    [Header("速度增长系统")]
    [SerializeField] private bool enableSpeedGrowth = true;
    [SerializeField] private float speedGrowthRate = 0.05f;
    [SerializeField] private float maxMoveSpeed = 10f;
    [SerializeField] private float accelerationMultiplier = 1.2f;
    [SerializeField] private float growthStartDelay = 30f;
    
    [Header("不同阵营区域行为")]
    [SerializeField] private float enemyZoneMoveSpeedMultiplier = 0.5f; // 敌对阵营区域移动速度倍数
    [SerializeField] private float neutralZoneMoveSpeedMultiplier = 1.0f; // 中立区域移动速度倍数
    
    [Header("视觉设置")]
    [SerializeField] private SpriteRenderer hunterSprite;
    [SerializeField] private Color factionAColor = new Color(0.2f, 0.6f, 1f);
    [SerializeField] private Color factionBColor = new Color(1f, 0.3f, 0.2f);
    [SerializeField] private GameObject detectionVisual;
    [SerializeField] private ParticleSystem speedEffectParticles;
    
    // 速度相关变量
    private float currentMoveSpeed;
    private float gameTime = 0f;
    private bool growthStarted = false;
    private HunterIndicatorSystem indicatorSystem;
    
    private Transform player;
    private Vector2 wanderCenter;
    private float wanderTimer;
    private Vector2 targetPosition;
    
    // 新增：记录猎人是否在同阵营区域内开始徘徊
    private bool isInSameFactionZone = false;
    private Vector2 sameFactionWanderCenter;
    
    private enum State
    {
        Wandering,
        Chasing,
        Attacking
    }
    
    private State currentState = State.Wandering;
    
    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        wanderCenter = transform.position;
        sameFactionWanderCenter = transform.position;
        wanderTimer = wanderInterval;
        SetRandomWanderTarget();
        
        // 初始化速度
        currentMoveSpeed = baseMoveSpeed;
        
        // 根据阵营设置颜色
        hunterSprite.color = faction == GameManager.Faction.FactionA ? 
            factionAColor : factionBColor;

        indicatorSystem = FindObjectOfType<HunterIndicatorSystem>();
        
        if (indicatorSystem != null)
        {
            indicatorSystem.RegisterHunter(transform);
        }
        else
        {
            Debug.LogWarning("HunterIndicatorSystem not found in scene!");
        }
    }
    
    private void Update()
    {
        if (GameManager.Instance.isGameOver) return;
        
        // 更新游戏时间
        gameTime += Time.deltaTime;
        
        // 延迟后开始加速
        if (!growthStarted && gameTime >= growthStartDelay)
        {
            growthStarted = true;
            Debug.Log($"猎人开始加速！当前速度：{currentMoveSpeed}");
            
            // 播放加速开始特效
            if (speedEffectParticles != null)
            {
                speedEffectParticles.Play();
            }
        }
        
        // 更新速度（随时间增长）
        UpdateSpeedOverTime();
        
        // 根据玩家当前阵营区域决定行为
        UpdateBehaviorBasedOnPlayerZone();
        
        switch (currentState)
        {
            case State.Wandering:
                UpdateWandering();
                break;
                
            case State.Chasing:
                ChasePlayer();
                CheckAttackRange();
                break;
                
            case State.Attacking:
                // 攻击逻辑（如动画播放等）
                break;
        }
    }
    
    void OnDestroy()
    {
        if (indicatorSystem != null)
        {
            indicatorSystem.UnregisterHunter(transform);
        }
    }
    
    void OnEnable()
    {
        if (indicatorSystem != null)
        {
            indicatorSystem.RegisterHunter(transform);
        }
    }
    
    void OnDisable()
    {
        if (indicatorSystem != null)
        {
            indicatorSystem.UnregisterHunter(transform);
        }
    }
    
    private void UpdateSpeedOverTime()
    {
        if (!enableSpeedGrowth || !growthStarted) return;
        
        // 根据时间线性增长速度
        float speedIncrease = speedGrowthRate * Time.deltaTime;
        currentMoveSpeed = Mathf.Min(currentMoveSpeed + speedIncrease, maxMoveSpeed);
    }
    
    private void UpdateBehaviorBasedOnPlayerZone()
    {
        if (player == null) return;
        
        // 获取玩家当前阵营
        GameManager.Faction playerFaction = GameManager.Instance.playerCurrentFaction;
        
        // 情况1: 玩家与猎人同阵营
        if (playerFaction == faction)
        {
            // 进入Wandering状态，在当前位置开始徘徊
            if (currentState != State.Wandering)
            {
                currentState = State.Wandering;
                
                // 设置徘徊中心为当前位置
                sameFactionWanderCenter = transform.position;
                wanderCenter = sameFactionWanderCenter;
                
                // 设置较小的徘徊半径（当前位置附近）
                SetRandomWanderTarget();
                
                isInSameFactionZone = true;
                
                Debug.Log($"猎人进入同阵营区域徘徊模式");
            }
            return;
        }
        
        // 情况2: 玩家在敌对阵营区域
        bool isPlayerInEnemyZone = (playerFaction != GameManager.Faction.Neutral && 
                                    playerFaction != faction);
        
        // 情况3: 玩家在中立区域
        bool isPlayerInNeutralZone = (playerFaction == GameManager.Faction.Neutral);
        
        // 检查玩家是否在检测范围内
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        if (distanceToPlayer <= detectionRange)
        {
            // 玩家在检测范围内，开始追逐
            if (currentState != State.Chasing && currentState != State.Attacking)
            {
                currentState = State.Chasing;
                Debug.Log($"猎人开始追逐玩家");
            }
            
            // 重置徘徊标志
            if (isInSameFactionZone)
            {
                isInSameFactionZone = false;
                wanderCenter = transform.position; // 重置为当前位置
            }
        }
        else
        {
            // 玩家不在检测范围内，恢复徘徊
            if (currentState != State.Wandering)
            {
                currentState = State.Wandering;
                
                if (!isInSameFactionZone)
                {
                    // 如果不是在同阵营区域徘徊，重置徘徊中心为当前位置
                    wanderCenter = transform.position;
                    SetRandomWanderTarget();
                }
            }
        }
    }
    
    private void UpdateWandering()
    {
        wanderTimer -= Time.deltaTime;
        
        // 移动到徘徊目标
        transform.position = Vector2.MoveTowards(
            transform.position, targetPosition, currentMoveSpeed * Time.deltaTime);
        
        // 到达目标或时间到，设置新目标
        if (Vector2.Distance(transform.position, targetPosition) < 0.1f || wanderTimer <= 0)
        {
            SetRandomWanderTarget();
            wanderTimer = wanderInterval;
        }
    }
    
    private void SetRandomWanderTarget()
    {
        // 如果在同阵营区域内，使用较小的徘徊范围
        float currentWanderRadius = isInSameFactionZone ? wanderRadius * 0.5f : wanderRadius;
        
        Vector2 randomDirection = Random.insideUnitCircle * currentWanderRadius;
        targetPosition = wanderCenter + randomDirection;
    }
    
    private void ChasePlayer()
    {
        if (player == null)
        {
            currentState = State.Wandering;
            return;
        }
        
        // 获取玩家当前阵营
        GameManager.Faction playerFaction = GameManager.Instance.playerCurrentFaction;
        
        // 计算追逐速度倍数
        float chaseSpeedMultiplier = accelerationMultiplier; // 默认使用加速倍数
        
        // 根据玩家所在区域调整速度
        if (playerFaction != GameManager.Faction.Neutral && playerFaction != faction)
        {
            // 玩家在敌对阵营区域：较慢速度
            chaseSpeedMultiplier = enemyZoneMoveSpeedMultiplier;
            Debug.Log($"敌对阵营区域，使用慢速追逐: {chaseSpeedMultiplier}");
        }
        else if (playerFaction == GameManager.Faction.Neutral)
        {
            // 玩家在中立区域：全速
            chaseSpeedMultiplier = neutralZoneMoveSpeedMultiplier;
            Debug.Log($"中立区域，使用全速追逐: {chaseSpeedMultiplier}");
        }
        
        // 向玩家移动，根据区域调整速度
        float chaseSpeed = currentMoveSpeed * chaseSpeedMultiplier;
        transform.position = Vector2.MoveTowards(
            transform.position, player.position, chaseSpeed * Time.deltaTime);
        
        // 失去玩家踪迹
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer > detectionRange * 1.5f)
        {
            currentState = State.Wandering;
            wanderCenter = transform.position; // 重置徘徊中心为当前位置
        }
    }
    
    private void CheckAttackRange()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer <= attackRange)
        {
            // 触发攻击
            currentState = State.Attacking;
            GameManager.Instance.CaughtByHunter(faction);
        }
    }
    
    // 公开方法，用于外部调整速度
    public void SetSpeedMultiplier(float multiplier)
    {
        if (multiplier > 0)
        {
            currentMoveSpeed = baseMoveSpeed * multiplier;
            currentMoveSpeed = Mathf.Min(currentMoveSpeed, maxMoveSpeed);
        }
    }
    
    public float GetCurrentSpeed()
    {
        return currentMoveSpeed;
    }
    
    public float GetSpeedRatio()
    {
        return Mathf.InverseLerp(baseMoveSpeed, maxMoveSpeed, currentMoveSpeed);
    }
    
    // 新增：当玩家进入/离开同阵营区域时调用
    public void OnPlayerEnteredSameFactionZone()
    {
        isInSameFactionZone = true;
        sameFactionWanderCenter = transform.position;
        wanderCenter = sameFactionWanderCenter;
        currentState = State.Wandering;
        
        Debug.Log($"玩家进入同阵营区域，猎人开始就地徘徊");
    }
    
    public void OnPlayerExitedSameFactionZone()
    {
        isInSameFactionZone = false;
        
        // 如果当前在徘徊状态，重置为当前位置徘徊
        if (currentState == State.Wandering)
        {
            wanderCenter = transform.position;
            SetRandomWanderTarget();
        }
    }
    
    // 新增：获取当前行为状态描述
    public string GetBehaviorDescription()
    {
        if (player == null) return "等待玩家";
        
        GameManager.Faction playerFaction = GameManager.Instance.playerCurrentFaction;
        
        if (playerFaction == faction)
        {
            return "同阵营区域：原地徘徊";
        }
        else if (playerFaction != GameManager.Faction.Neutral && playerFaction != faction)
        {
            return "敌对阵营区域：慢速追踪";
        }
        else
        {
            return "中立区域：全速追踪";
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        // 显示检测范围
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // 显示攻击范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // 显示徘徊范围
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(wanderCenter, wanderRadius);
        
        // 如果在同阵营区域内，显示特殊的徘徊范围
        if (Application.isPlaying && isInSameFactionZone)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(sameFactionWanderCenter, wanderRadius * 0.5f);
        }
    }
}