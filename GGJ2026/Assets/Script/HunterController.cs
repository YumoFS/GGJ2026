// HunterController.cs
using UnityEngine;
using System.Collections.Generic;

public class HunterController : MonoBehaviour
{
    [Header("追捕设置")]
    public GameManager.Faction faction = GameManager.Faction.FactionA;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private float wanderRadius = 5f;
    [SerializeField] private float wanderInterval = 3f;
    
    [Header("视觉设置")]
    [SerializeField] private SpriteRenderer hunterSprite;
    [SerializeField] private Color factionAColor = new Color(0.2f, 0.6f, 1f);
    [SerializeField] private Color factionBColor = new Color(1f, 0.3f, 0.2f);
    [SerializeField] private GameObject detectionVisual;
    
    private Transform player;
    private Vector2 wanderCenter;
    private float wanderTimer;
    private Vector2 targetPosition;
    
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
        wanderTimer = wanderInterval;
        SetRandomWanderTarget();
        
        // 根据阵营设置颜色
        hunterSprite.color = faction == GameManager.Faction.FactionA ? 
            factionAColor : factionBColor;
    }
    
    private void Update()
    {
        if (GameManager.Instance.isGameOver) return;
        
        // 忽略同阵营玩家
        if (player != null && 
            GameManager.Instance.playerCurrentFaction == faction)
        {
            currentState = State.Wandering;
            return;
        }
        
        switch (currentState)
        {
            case State.Wandering:
                UpdateWandering();
                CheckForPlayer();
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
    
    private void UpdateWandering()
    {
        wanderTimer -= Time.deltaTime;
        
        // 移动到徘徊目标
        transform.position = Vector2.MoveTowards(
            transform.position, targetPosition, moveSpeed * Time.deltaTime);
        
        // 到达目标或时间到，设置新目标
        if (Vector2.Distance(transform.position, targetPosition) < 0.1f || wanderTimer <= 0)
        {
            SetRandomWanderTarget();
            wanderTimer = wanderInterval;
        }
    }
    
    private void SetRandomWanderTarget()
    {
        Vector2 randomDirection = Random.insideUnitCircle * wanderRadius;
        targetPosition = wanderCenter + randomDirection;
    }
    
    private void CheckForPlayer()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        if (distanceToPlayer <= detectionRange && 
            GameManager.Instance.playerCurrentFaction != faction)
        {
            currentState = State.Chasing;
            // 播放发现玩家的音效/动画
        }
    }
    
    private void ChasePlayer()
    {
        if (player == null)
        {
            currentState = State.Wandering;
            return;
        }
        
        // 向玩家移动
        transform.position = Vector2.MoveTowards(
            transform.position, player.position, moveSpeed * Time.deltaTime);
        
        // 失去玩家踪迹
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer > detectionRange * 1.5f)
        {
            currentState = State.Wandering;
            wanderCenter = transform.position;
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
            // 这里可以调用GameManager的游戏结束逻辑
            GameManager.Instance.CaughtByHunter(faction);
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
    }
}