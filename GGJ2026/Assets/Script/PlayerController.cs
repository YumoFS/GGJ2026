using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("移动参数")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 10f;
    
    [Header("组件引用")]
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    
    private PlayerControls playerControls;
    private Vector2 moveInput;
    private Vector2 currentVelocity;

    public bool canMove = true;

    private void Awake()
    {
        // 获取组件
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // 初始化输入系统
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
        HandleMovement();
        UpdateAnimation();
    }

    private void HandleMovement()
    {
        // 计算目标速度
        Vector2 targetVelocity = moveInput * moveSpeed;
        
        // 平滑插值当前速度
        if (moveInput.magnitude > 0.1f)
        {
            currentVelocity = Vector2.Lerp(currentVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
        }
        else
        {
            currentVelocity = Vector2.Lerp(currentVelocity, Vector2.zero, deceleration * Time.fixedDeltaTime);
        }
        
        // 应用速度
        rb.velocity = currentVelocity;
    }

    private void UpdateAnimation()
    {
        // 如果有Animator组件，更新动画参数
        if (animator != null)
        {
            bool isMoving = moveInput.magnitude > 0.1f;
            animator.SetBool("IsMoving", isMoving);
            
            // 设置移动方向
            if (isMoving)
            {
                animator.SetFloat("MoveX", moveInput.x);
                animator.SetFloat("MoveY", moveInput.y);
            }
        }
        
        // 翻转精灵朝向（如果需要）
        if (spriteRenderer != null && moveInput.x != 0)
        {
            spriteRenderer.flipX = moveInput.x < 0;
        }
    }
}