// SimpleMaskSorter.cs - 最简单的实现，一行代码解决问题
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[ExecuteInEditMode]
public class MaskOrder : MonoBehaviour
{
    [SerializeField] private SpriteRenderer parentSprite; // 父级精灵
    [SerializeField] private int offset = 1; // 排序偏移
    
    private SpriteRenderer maskSprite;
    
    private void Awake()
    {
        maskSprite = GetComponent<SpriteRenderer>();
        
        // 如果没有指定父级精灵，自动查找
        if (parentSprite == null)
        {
            Transform parent = transform.parent;
            if (parent != null)
            {
                parentSprite = parent.GetComponent<SpriteRenderer>();
                
                // 如果父级没有精灵，在父级的子对象中查找
                if (parentSprite == null)
                {
                    parentSprite = parent.GetComponentInChildren<SpriteRenderer>();
                }
            }
        }
    }
    
    private void Update()
    {
        UpdateSorting();
    }
    
    private void UpdateSorting()
    {
        if (parentSprite == null || maskSprite == null) return;
        
        // 核心逻辑：设置面具的排序值为父级-1
        maskSprite.sortingLayerName = parentSprite.sortingLayerName;
        maskSprite.sortingOrder = parentSprite.sortingOrder + offset;
    }
    
    [ContextMenu("立即更新")]
    private void ForceUpdate()
    {
        UpdateSorting();
        Debug.Log($"更新面具排序: 父级={parentSprite.sortingOrder}, 面具={maskSprite.sortingOrder}");
    }
    
    #if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying && maskSprite == null)
        {
            maskSprite = GetComponent<SpriteRenderer>();
        }
        
        if (maskSprite != null && parentSprite != null)
        {
            UpdateSorting();
        }
    }
    #endif
}