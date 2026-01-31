using UnityEngine;
using System.Collections.Generic;

public class AutoSortingOrder : MonoBehaviour
{
    [Header("排序设置")]
    [SerializeField] private bool useYPosition = true;
    [SerializeField] private float baseY = 0f;  // 基准Y值
    [SerializeField] private int sortOrderMultiplier = 100;  // 排序乘数
    [SerializeField] private bool includeInactive = false;  // 是否包含非激活子物体
    
    private List<SpriteRenderer> childSpriteRenderers = new List<SpriteRenderer>();
    private Dictionary<SpriteRenderer, int> originalSortingOrders = new Dictionary<SpriteRenderer, int>();
    private float lastYPosition;

    void Awake()
    {
        // 获取所有子物体的SpriteRenderer
        GetChildSpriteRenderers();
        
        // 记录原始的排序顺序
        foreach (var renderer in childSpriteRenderers)
        {
            originalSortingOrders[renderer] = renderer.sortingOrder;
        }
        
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

    // 获取所有子物体的SpriteRenderer
    void GetChildSpriteRenderers()
    {
        childSpriteRenderers.Clear();
        
        // 获取所有子物体（包括嵌套子物体）的SpriteRenderer
        SpriteRenderer[] allRenderers = GetComponentsInChildren<SpriteRenderer>(includeInactive);
        
        // 排除父物体自身的SpriteRenderer（只保留子物体）
        foreach (var renderer in allRenderers)
        {
            if (renderer.transform != this.transform)  // 不是父物体自身
            {
                childSpriteRenderers.Add(renderer);
            }
        }
        
        Debug.Log($"找到 {childSpriteRenderers.Count} 个子物体的SpriteRenderer");
    }

    void UpdateSortingOrder()
    {
        if (childSpriteRenderers.Count == 0)
            return;
        
        // 基于父物体的Y坐标计算基础排序值
        int baseOrder = Mathf.RoundToInt((baseY - transform.position.y) * sortOrderMultiplier);
        
        // 为每个子物体设置排序值，保持相对顺序
        foreach (var renderer in childSpriteRenderers)
        {
            if (renderer != null)
            {
                // 在基础排序值上加上原始的相对偏移
                int relativeOffset = originalSortingOrders.ContainsKey(renderer) ? 
                    originalSortingOrders[renderer] : renderer.sortingOrder;
                
                renderer.sortingOrder = baseOrder + relativeOffset;
            }
        }
    }

    // 手动设置整体排序值（会保持子物体间的相对顺序）
    public void SetBaseSortingOrder(int baseOrder)
    {
        foreach (var renderer in childSpriteRenderers)
        {
            if (renderer != null)
            {
                int relativeOffset = originalSortingOrders.ContainsKey(renderer) ? 
                    originalSortingOrders[renderer] : renderer.sortingOrder;
                
                renderer.sortingOrder = baseOrder + relativeOffset;
            }
        }
    }
    
    // 刷新子物体列表（当动态添加/删除子物体时调用）
    public void RefreshChildren()
    {
        GetChildSpriteRenderers();
        
        // 更新原始排序顺序字典
        originalSortingOrders.Clear();
        foreach (var renderer in childSpriteRenderers)
        {
            originalSortingOrders[renderer] = renderer.sortingOrder;
        }
        
        UpdateSortingOrder();
    }
    
    // 获取所有子物体SpriteRenderer的数量
    public int GetChildRendererCount()
    {
        return childSpriteRenderers.Count;
    }
    
    // 获取父物体的当前排序值
    public int GetCurrentSortingOrder()
    {
        if (childSpriteRenderers.Count > 0 && childSpriteRenderers[0] != null)
        {
            // 返回第一个子物体的排序值减去其原始偏移
            int firstChildOrder = childSpriteRenderers[0].sortingOrder;
            int firstChildOriginal = originalSortingOrders.ContainsKey(childSpriteRenderers[0]) ? 
                originalSortingOrders[childSpriteRenderers[0]] : 0;
            
            return firstChildOrder - firstChildOriginal;
        }
        return 0;
    }
    
    // 在编辑器中手动排序（适用于非运行时）
    #if UNITY_EDITOR
    [ContextMenu("立即排序")]
    void SortNowInEditor()
    {
        GetChildSpriteRenderers();
        
        // 记录当前排序顺序
        foreach (var renderer in childSpriteRenderers)
        {
            if (!originalSortingOrders.ContainsKey(renderer))
            {
                originalSortingOrders[renderer] = renderer.sortingOrder;
            }
        }
        
        UpdateSortingOrder();
    }
    
    [ContextMenu("重置为原始顺序")]
    void ResetToOriginalOrder()
    {
        foreach (var renderer in childSpriteRenderers)
        {
            if (renderer != null && originalSortingOrders.ContainsKey(renderer))
            {
                renderer.sortingOrder = originalSortingOrders[renderer];
            }
        }
    }
    #endif
}