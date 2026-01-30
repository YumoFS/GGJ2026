// FactionMemberController.cs
using UnityEngine;

public class FactionMemberController : MonoBehaviour
{
    [Header("阵营设置")]
    public GameManager.Faction faction = GameManager.Faction.FactionA;
    
    [Header("行为设置")]
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float idleTime = 2f;
    [SerializeField] private float wanderDistance = 3f;
    
    [Header("视觉设置")]
    [SerializeField] private SpriteRenderer memberSprite;
    [SerializeField] private Color factionAColor = new Color(0.2f, 0.6f, 1f, 0.7f);
    [SerializeField] private Color factionBColor = new Color(1f, 0.3f, 0.2f, 0.7f);
    [SerializeField] private GameObject thoughtBubble; // 多邻国风格的想法气泡
    
    private Vector2 startPosition;
    private Vector2 targetPosition;
    private float idleTimer;
    private bool isMoving = false;
    
    private void Start()
    {
        startPosition = transform.position;
        SetRandomTarget();
        
        // 根据阵营设置颜色
        memberSprite.color = faction == GameManager.Faction.FactionA ? 
            factionAColor : factionBColor;
            
        // 随机显示想法气泡
        if (thoughtBubble != null)
        {
            thoughtBubble.SetActive(Random.value > 0.5f);
            InvokeRepeating(nameof(ToggleThoughtBubble), Random.Range(3f, 8f), 
                Random.Range(5f, 12f));
        }
    }
    
    private void Update()
    {
        if (isMoving)
        {
            MoveToTarget();
        }
        else
        {
            idleTimer -= Time.deltaTime;
            if (idleTimer <= 0)
            {
                SetRandomTarget();
                isMoving = true;
            }
        }
    }
    
    private void SetRandomTarget()
    {
        Vector2 randomOffset = Random.insideUnitCircle * wanderDistance;
        targetPosition = startPosition + randomOffset;
        
        // 确保不移动到太远的地方
        if (Vector2.Distance(targetPosition, startPosition) > wanderDistance)
        {
            targetPosition = startPosition + 
                (targetPosition - startPosition).normalized * wanderDistance;
        }
    }
    
    private void MoveToTarget()
    {
        transform.position = Vector2.MoveTowards(
            transform.position, targetPosition, moveSpeed * Time.deltaTime);
        
        // 简单的方向翻转
        if (targetPosition.x > transform.position.x)
        {
            memberSprite.flipX = false;
        }
        else if (targetPosition.x < transform.position.x)
        {
            memberSprite.flipX = true;
        }
        
        // 到达目标
        if (Vector2.Distance(transform.position, targetPosition) < 0.1f)
        {
            isMoving = false;
            idleTimer = idleTime;
        }
    }
    
    private void ToggleThoughtBubble()
    {
        if (thoughtBubble != null)
        {
            thoughtBubble.SetActive(!thoughtBubble.activeSelf);
            
            // 随机改变气泡中的图标
            if (thoughtBubble.activeSelf)
            {
                // 这里可以设置气泡中的图标（不同阵营显示不同图标）
            }
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 与玩家互动（如同行、打招呼等）
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            // 如果是同阵营，可能有特殊互动
            if (GameManager.Instance.playerCurrentFaction == faction)
            {
                // 播放打招呼动画或音效
            }
        }
    }
}