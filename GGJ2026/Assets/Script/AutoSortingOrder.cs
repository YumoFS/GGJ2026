using UnityEngine;

public class AutoSortingOrder : MonoBehaviour
{
    [Header("排序设置")]
    [SerializeField] private bool useYPosition = true;
    [SerializeField] private float baseY = 0f;  // 基准Y值
    [SerializeField] private int sortOrderMultiplier = 100;  // 排序乘数
    
    private SpriteRenderer spriteRenderer;
    private float lastYPosition;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateSortingOrder();
    }

    void Update()
    {
        if (useYPosition && Mathf.Abs(transform.position.y - lastYPosition) > 0.01f)
        {
            UpdateSortingOrder();
            lastYPosition = transform.position.y;
        }
    }

    void UpdateSortingOrder()
    {
        if (spriteRenderer != null)
        {
            // 基于Y坐标计算排序值（Y值越小，显示越靠前）
            int order = Mathf.RoundToInt((baseY - transform.position.y) * sortOrderMultiplier);
            spriteRenderer.sortingOrder = order;
        }
    }

    // 手动设置排序值
    public void SetSortingOrder(int order)
    {
        if (spriteRenderer != null)
            spriteRenderer.sortingOrder = order;
    }
}