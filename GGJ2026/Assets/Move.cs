using UnityEngine;

public class MoveWithWASD : MonoBehaviour
{
    public float moveSpeed = 5f; // 移动速度（单位：单位/秒）

    private Rigidbody2D rb;
    private Vector2 moveInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if(rb == null)
        {
            // 如果没有 Rigidbody2D 自动添加
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0; // 2D 平面，不受重力影响
        }
    }

    void Update()
    {
        // 获取 WASD / 箭头键输入
        float moveX = Input.GetAxisRaw("Horizontal"); // A/D 或 左/右
        float moveY = Input.GetAxisRaw("Vertical");   // W/S 或 上/下

        moveInput = new Vector2(moveX, moveY).normalized; // 保证对角线速度正常
    }

    void FixedUpdate()
    {
        // 用 Rigidbody2D 移动，物理安全
        rb.velocity = moveInput * moveSpeed;
    }
}
