// SimpleMiniMapUI.cs - 简化UI设置
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class MiniMapUI : MonoBehaviour
{
    [Header("小地图UI元素")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private RawImage mapImage;
    [SerializeField] private Image borderImage;
    [SerializeField] private Image playerIcon;
    [SerializeField] private Image hunterIconTemplate;
    
    [Header("外观设置")]
    [SerializeField] private Color backgroundColor = new Color(0.1f, 0.1f, 0.2f, 0.8f);
    [SerializeField] private Color borderColor = new Color(0.3f, 0.3f, 0.4f, 1f);
    [SerializeField] private Color playerIconColor = Color.blue;
    [SerializeField] private Color hunterIconColor = Color.red;
    
    [Header("形状设置")]
    [SerializeField] private bool circularMap = true; // 圆形或方形
    [SerializeField] private float mapRadius = 100f; // 圆形半径
    [SerializeField] private Vector2 mapSize = new Vector2(200, 200); // 方形尺寸
    
    private void Start()
    {
        SetupUI();
    }
    
    private void OnValidate()
    {
        // 在编辑器中实时更新UI
        if (!Application.isPlaying)
        {
            SetupUI();
        }
    }
    
    private void SetupUI()
    {
        if (canvas == null) canvas = GetComponent<Canvas>();
        if (canvas == null) canvas = gameObject.AddComponent<Canvas>();
        
        SetupCanvas();
        SetupMapImage();
        SetupBorder();
        SetupIcons();
    }
    
    private void SetupCanvas()
    {
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100; // 确保在最上层
    }
    
    private void SetupMapImage()
    {
        if (mapImage == null)
        {
            GameObject mapObj = new GameObject("MapImage");
            mapObj.transform.SetParent(transform);
            mapImage = mapObj.AddComponent<RawImage>();
        }
        
        // 设置位置和大小
        RectTransform rt = mapImage.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1); // 左上角
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(10, -10); // 距离左上角10像素
        
        if (circularMap)
        {
            // 圆形小地图
            rt.sizeDelta = new Vector2(mapRadius * 2, mapRadius * 2);
            
            // 添加遮罩使其变成圆形
            AddCircularMask(mapImage.gameObject);
        }
        else
        {
            // 方形小地图
            rt.sizeDelta = mapSize;
        }
        
        mapImage.color = backgroundColor;
    }
    
    private void AddCircularMask(GameObject target)
    {
        // 添加圆形遮罩
        Mask mask = target.GetComponent<Mask>();
        if (mask == null) mask = target.AddComponent<Mask>();
        mask.showMaskGraphic = false;
        
        // 添加圆形Image作为遮罩
        Image maskImage = target.GetComponent<Image>();
        if (maskImage == null) maskImage = target.AddComponent<Image>();
        
        // 创建圆形Sprite（可以通过代码创建，或使用资源）
        // 这里使用Unity的圆形Sprite
        maskImage.sprite = Resources.Load<Sprite>("Circle");
        if (maskImage.sprite == null)
        {
            // 如果没找到圆形Sprite，创建一个简单的
            maskImage.sprite = CreateSimpleCircleSprite();
        }
    }
    
    private Sprite CreateSimpleCircleSprite()
    {
        // 创建一个简单的圆形纹理
        int size = 128;
        Texture2D tex = new Texture2D(size, size);
        
        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
        float radius = size * 0.5f;
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                Color color = distance <= radius ? Color.white : Color.clear;
                tex.SetPixel(x, y, color);
            }
        }
        
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
    
    private void SetupBorder()
    {
        if (borderImage == null)
        {
            GameObject borderObj = new GameObject("Border");
            borderObj.transform.SetParent(mapImage.transform);
            borderImage = borderObj.AddComponent<Image>();
        }
        
        // 设置边框位置和大小
        RectTransform rt = borderImage.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
        
        borderImage.color = borderColor;
        
        // 如果是圆形，边框也应该是圆形
        if (circularMap)
        {
            borderImage.sprite = Resources.Load<Sprite>("Circle");
        }
    }
    
    private void SetupIcons()
    {
        SetupPlayerIcon();
        SetupHunterIconTemplate();
    }
    
    private void SetupPlayerIcon()
    {
        if (playerIcon == null)
        {
            GameObject iconObj = new GameObject("PlayerIcon");
            iconObj.transform.SetParent(mapImage.transform);
            playerIcon = iconObj.AddComponent<Image>();
        }
        
        // 设置玩家图标
        RectTransform rt = playerIcon.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(15, 15); // 图标大小
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        
        playerIcon.color = playerIconColor;
        
        // 设置三角形图标（表示方向）
        playerIcon.sprite = CreateTriangleSprite();
        playerIcon.type = Image.Type.Simple;
    }
    
    private void SetupHunterIconTemplate()
    {
        if (hunterIconTemplate == null)
        {
            GameObject iconObj = new GameObject("HunterIconTemplate");
            iconObj.transform.SetParent(mapImage.transform);
            hunterIconTemplate = iconObj.AddComponent<Image>();
        }
        
        // 设置猎人图标模板（默认隐藏）
        hunterIconTemplate.gameObject.SetActive(false);
        
        RectTransform rt = hunterIconTemplate.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(12, 12); // 图标大小
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        
        hunterIconTemplate.color = hunterIconColor;
        
        // 设置方形图标
        hunterIconTemplate.sprite = CreateSquareSprite();
        hunterIconTemplate.type = Image.Type.Simple;
    }
    
    private Sprite CreateTriangleSprite()
    {
        // 创建三角形纹理
        int size = 32;
        Texture2D tex = new Texture2D(size, size);
        
        // 填充透明
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                tex.SetPixel(x, y, Color.clear);
            }
        }
        
        // 绘制三角形（指向右边）
        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
        
        // 三角形的三个点（指向右边）
        Vector2[] trianglePoints = new Vector2[]
        {
            new Vector2(center.x + size * 0.4f, center.y), // 顶点
            new Vector2(center.x - size * 0.3f, center.y - size * 0.3f), // 左下
            new Vector2(center.x - size * 0.3f, center.y + size * 0.3f)  // 右下
        };
        
        // 简单的三角形填充
        FillTriangle(tex, trianglePoints[0], trianglePoints[1], trianglePoints[2], Color.white);
        
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
    
    private Sprite CreateSquareSprite()
    {
        // 创建方形纹理
        int size = 32;
        Texture2D tex = new Texture2D(size, size);
        
        // 绘制实心方形
        int padding = 2;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                if (x >= padding && x < size - padding && y >= padding && y < size - padding)
                {
                    tex.SetPixel(x, y, Color.white);
                }
                else
                {
                    tex.SetPixel(x, y, Color.clear);
                }
            }
        }
        
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
    
    private void FillTriangle(Texture2D tex, Vector2 p1, Vector2 p2, Vector2 p3, Color color)
    {
        // 简单的三角形填充算法
        Vector2 min = new Vector2(Mathf.Min(p1.x, p2.x, p3.x), Mathf.Min(p1.y, p2.y, p3.y));
        Vector2 max = new Vector2(Mathf.Max(p1.x, p2.x, p3.x), Mathf.Max(p1.y, p2.y, p3.y));
        
        for (int y = (int)min.y; y <= max.y; y++)
        {
            for (int x = (int)min.x; x <= max.x; x++)
            {
                Vector2 p = new Vector2(x, y);
                if (IsPointInTriangle(p, p1, p2, p3))
                {
                    if (x >= 0 && x < tex.width && y >= 0 && y < tex.height)
                    {
                        tex.SetPixel(x, y, color);
                    }
                }
            }
        }
    }
    
    private bool IsPointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        // 使用重心坐标判断点是否在三角形内
        float s1 = c.y - a.y;
        float s2 = c.x - a.x;
        float s3 = b.y - a.y;
        float s4 = p.y - a.y;
        
        float w1 = (a.x * s1 + s4 * s2 - p.x * s1) / (s3 * s2 - (b.x - a.x) * s1);
        float w2 = (s4 - w1 * s3) / s1;
        
        return w1 >= 0 && w2 >= 0 && (w1 + w2) <= 1;
    }
    
    // 获取图标模板用于实例化
    public Image GetHunterIconTemplate()
    {
        return hunterIconTemplate;
    }
    
    // 获取玩家图标
    public Image GetPlayerIcon()
    {
        return playerIcon;
    }
    
    // 获取地图Image
    public RawImage GetMapImage()
    {
        return mapImage;
    }
}