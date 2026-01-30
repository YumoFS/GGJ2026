// PlayerController.cs
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
public class PlayerController : MonoBehaviour
{
    [Header("移动设置")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 10f;
    
    [Header("阵营检测")]
    [SerializeField] private float factionCheckRadius = 0.5f;
    [SerializeField] private LayerMask factionZoneLayer;
    
    [Header("面具设置")]
    [SerializeField] private SpriteRenderer maskRenderer;
    [SerializeField] private float maskTransitionSpeed = 2f;
    
    public bool canMove = true;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Vector2 moveInput;
    private Vector2 currentVelocity;
    
    // 阵营相关
    private FactionZone currentZone = null;
    private float zoneProgressSpeed = 10f; // 每秒增加的进度值
    
    // 面具颜色（双阵营颜色）
    private Color factionAColor = new Color(0.2f, 0.6f, 1f); // 蓝色
    private Color factionBColor = new Color(1f, 0.3f, 0.2f); // 红色
    private Color neutralColor = new Color(0.8f, 0.8f, 0.8f); // 灰色
    
    private PlayerControls playerControls;
    
    [System.Serializable]
    public class FactionMaskData
    {
        public Color colorA;
        public Color colorB;
        public Color currentColor;
        public float factionABlend = 0.5f; // 0=A完全, 1=B完全
    }
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerControls = new PlayerControls();
    }
    
    private void OnEnable()
    {
        playerControls.Enable();
        playerControls.Player.Move.performed += OnMovePerformed;
        playerControls.Player.Move.canceled += OnMoveCanceled;
    }
    
    private void OnDisable()
    {
        playerControls.Player.Move.performed -= OnMovePerformed;
        playerControls.Player.Move.canceled -= OnMoveCanceled;
        playerControls.Disable();
    }
    
    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
    
    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        moveInput = Vector2.zero;
    }
    
    private void FixedUpdate()
    {
        if (!canMove) return;
        
        HandleMovement();
        CheckFactionZone();
        UpdateMaskAppearance();
    }
    
    private void HandleMovement()
    {
        Vector2 targetVelocity = moveInput * moveSpeed;
        
        if (moveInput.magnitude > 0.1f)
        {
            currentVelocity = Vector2.Lerp(currentVelocity, targetVelocity, 
                acceleration * Time.fixedDeltaTime);
        }
        else
        {
            currentVelocity = Vector2.Lerp(currentVelocity, Vector2.zero, 
                deceleration * Time.fixedDeltaTime);
        }
        
        rb.velocity = currentVelocity;
        
        // 简单的动画：根据移动方向调整精灵
        if (moveInput.x != 0)
        {
            spriteRenderer.flipX = moveInput.x < 0;
        }
    }
    
    private void CheckFactionZone()
    {
        Collider2D[] zones = Physics2D.OverlapCircleAll(
            transform.position, factionCheckRadius, factionZoneLayer);
        
        FactionZone nearestZone = null;
        float nearestDistance = float.MaxValue;
        
        foreach (var collider in zones)
        {
            FactionZone zone = collider.GetComponent<FactionZone>();
            if (zone != null)
            {
                float distance = Vector2.Distance(transform.position, zone.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestZone = zone;
                }
            }
        }
        
        // 进入/离开区域
        if (nearestZone != currentZone)
        {
            if (currentZone != null)
            {
                currentZone.OnPlayerExit(this);
            }
            
            currentZone = nearestZone;
            
            if (currentZone != null)
            {
                currentZone.OnPlayerEnter(this);
                GameManager.Instance.playerCurrentFaction = currentZone.faction;
            }
            else
            {
                GameManager.Instance.playerCurrentFaction = GameManager.Faction.Neutral;
            }
        }
        
        // 在区域内增加进度
        if (currentZone != null)
        {
            float progressAmount = zoneProgressSpeed * Time.fixedDeltaTime;
            GameManager.Instance.AddFactionProgress(
                currentZone.faction == GameManager.Faction.FactionA ? 
                GameManager.Faction.FactionA : GameManager.Faction.FactionB,
                progressAmount);
        }
    }
    
    private void UpdateMaskAppearance()
    {
        if (maskRenderer == null) return;
        
        float progressA = GameManager.Instance.maskProgressA;
        float progressB = GameManager.Instance.maskProgressB;
        float totalProgress = progressA + progressB;
        
        if (totalProgress > 0)
        {
            // 计算阵营混合比例
            float blendValue = progressB / totalProgress;
            
            // 渐变混合颜色
            Color targetColor = Color.Lerp(factionAColor, factionBColor, blendValue);
            
            // 根据总进度调整透明度（进度越高，面具越明显）
            float alpha = Mathf.Lerp(0.3f, 1f, totalProgress / 100f);
            targetColor.a = alpha;
            
            maskRenderer.color = Color.Lerp(
                maskRenderer.color, targetColor, maskTransitionSpeed * Time.deltaTime);
        }
        else
        {
            // 无进度时显示中性颜色
            maskRenderer.color = Color.Lerp(
                maskRenderer.color, neutralColor, maskTransitionSpeed * Time.deltaTime);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 被追捕者抓住
        HunterController hunter = other.GetComponent<HunterController>();
        if (hunter != null)
        {
            if (GameManager.Instance.playerCurrentFaction != hunter.faction)
            {
                GameManager.Instance.CaughtByHunter(hunter.faction);
            }
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        // 显示阵营检测范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, factionCheckRadius);
    }
}