// TopLeftMiniMap.cs - 固定在左上角的小地图系统
using UnityEngine;
using UnityEngine.UI;

public class TopLeftMiniMap : MonoBehaviour
{
    [Header("核心组件")]
    [SerializeField] private Camera miniMapCamera;
    [SerializeField] private RawImage miniMapDisplay;
    [SerializeField] private Transform player;
    
    [Header("小地图设置")]
    [SerializeField] private Vector2 miniMapSize = new Vector2(200, 200);
    [SerializeField] private float cameraSize = 15f;
    [SerializeField] private Color backgroundColor = new Color(0.1f, 0.1f, 0.2f, 0.8f);
    
    [Header("玩家标记")]
    [SerializeField] private RectTransform playerMarker;
    [SerializeField] private Color playerColor = Color.blue;
    
    [Header("追捕者标记")]
    [SerializeField] private RectTransform hunterMarkerPrefab;
    [SerializeField] private Transform markersParent;
    [SerializeField] private Color hunterAColor = new Color(0.2f, 0.6f, 1f); // 阵营A颜色
    [SerializeField] private Color hunterBColor = new Color(1f, 0.3f, 0.2f); // 阵营B颜色
    
    [Header("地图边界")]
    [SerializeField] private Vector2 mapMinBounds = new Vector2(-50, -50);
    [SerializeField] private Vector2 mapMaxBounds = new Vector2(50, 50);
    
    // 私有变量
    private HunterController[] hunters;
    private RectTransform[] hunterMarkers;
    
    void Start()
    {
        InitializeMiniMap();
        InitializeMarkers();
    }
    
    void Update()
    {
        UpdateMiniMapCamera();
        UpdatePlayerMarker();
        UpdateHunterMarkers();
    }
    
    private void InitializeMiniMap()
    {
        // 确保玩家引用存在
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }
        
        // 确保摄像机设置正确
        if (miniMapCamera == null)
        {
            // 尝试查找或创建小地图摄像机
            miniMapCamera = GetComponent<Camera>();
            if (miniMapCamera == null)
            {
                GameObject camObj = new GameObject("MiniMapCamera");
                camObj.transform.SetParent(transform);
                miniMapCamera = camObj.AddComponent<Camera>();
                SetupMiniMapCamera();
            }
        }
        
        // 确保小地图显示设置正确
        if (miniMapDisplay != null)
        {
            SetupMiniMapDisplay();
        }
        
        // 创建Render Texture
        CreateRenderTexture();
        
        Debug.Log("左上角小地图初始化完成");
    }
    
    private void SetupMiniMapCamera()
    {
        miniMapCamera.orthographic = true;
        miniMapCamera.orthographicSize = cameraSize;
        miniMapCamera.clearFlags = CameraClearFlags.SolidColor;
        miniMapCamera.backgroundColor = backgroundColor;
        
        // 只渲染特定层级（如果需要）
        // miniMapCamera.cullingMask = LayerMask.GetMask("MiniMap");
        
        miniMapCamera.depth = 1; // 在主摄像机之上
    }
    
    private void SetupMiniMapDisplay()
    {
        RectTransform rt = miniMapDisplay.GetComponent<RectTransform>();
        
        // 固定在左上角
        rt.anchorMin = new Vector2(0, 1);     // 左上角
        rt.anchorMax = new Vector2(0, 1);     // 左上角
        rt.pivot = new Vector2(0, 1);         // 左上角为轴点
        
        // 设置位置和大小
        rt.anchoredPosition = new Vector2(10, -10); // 距离左上角10像素
        rt.sizeDelta = miniMapSize;
        
        // 添加边框效果（可选）
        AddBorderEffect(rt);
    }
    
    private void AddBorderEffect(RectTransform parent)
    {
        GameObject border = new GameObject("MapBorder");
        border.transform.SetParent(parent, false);
        
        Image borderImage = border.AddComponent<Image>();
        borderImage.color = new Color(0.3f, 0.3f, 0.4f, 1f);
        
        RectTransform borderRT = border.GetComponent<RectTransform>();
        borderRT.anchorMin = Vector2.zero;
        borderRT.anchorMax = Vector2.one;
        borderRT.sizeDelta = new Vector2(4, 4); // 边框宽度
        borderRT.anchoredPosition = Vector2.zero;
    }
    
    private void CreateRenderTexture()
    {
        if (miniMapCamera.targetTexture == null)
        {
            RenderTexture rt = new RenderTexture(256, 256, 16);
            rt.name = "MiniMap_RenderTexture";
            miniMapCamera.targetTexture = rt;
            
            if (miniMapDisplay != null)
            {
                miniMapDisplay.texture = rt;
            }
        }
    }
    
    private void InitializeMarkers()
    {
        // 查找所有追捕者
        hunters = FindObjectsOfType<HunterController>();
        hunterMarkers = new RectTransform[hunters.Length];
        
        // 确保有父物体存放标记
        if (markersParent == null)
        {
            GameObject parentObj = new GameObject("MiniMapMarkers");
            parentObj.transform.SetParent(miniMapDisplay.transform);
            markersParent = parentObj.transform;
        }
        
        // 为每个追捕者创建标记
        for (int i = 0; i < hunters.Length; i++)
        {
            if (hunters[i] != null)
            {
                hunterMarkers[i] = CreateHunterMarker(hunters[i], i);
            }
        }
        
        // 设置玩家标记
        SetupPlayerMarker();
    }
    
    private void SetupPlayerMarker()
    {
        if (playerMarker == null && miniMapDisplay != null)
        {
            GameObject markerObj = new GameObject("PlayerMarker");
            markerObj.transform.SetParent(miniMapDisplay.transform);
            
            playerMarker = markerObj.AddComponent<RectTransform>();
            Image markerImage = markerObj.AddComponent<Image>();
            
            // 设置标记样式
            markerImage.color = playerColor;
            playerMarker.sizeDelta = new Vector2(12, 12);
            
            // 创建三角形箭头（显示方向）
            CreateTriangleSprite(markerImage);
        }
        
        if (playerMarker != null)
        {
            playerMarker.pivot = new Vector2(0.5f, 0.5f);
            playerMarker.anchorMin = new Vector2(0, 0);
            playerMarker.anchorMax = new Vector2(0, 0);
        }
    }
    
    private void CreateTriangleSprite(Image image)
    {
        // 创建简单的三角形纹理
        int size = 32;
        Texture2D tex = new Texture2D(size, size);
        
        // 透明背景
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                tex.SetPixel(x, y, Color.clear);
            }
        }
        
        // 绘制三角形（指向右边）
        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
        
        // 简单三角形绘制（更简单的方法）
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // 简单的三角形区域判断
                float distX = Mathf.Abs(x - center.x);
                float distY = Mathf.Abs(y - center.y);
                
                if (x > center.x + 5) continue; // 右半边
                if (distY < (size * 0.3f) - (distX * 0.3f))
                {
                    tex.SetPixel(x, y, Color.white);
                }
            }
        }
        
        tex.Apply();
        image.sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
    
    private RectTransform CreateHunterMarker(HunterController hunter, int index)
    {
        if (hunterMarkerPrefab == null || markersParent == null)
            return null;
        
        // 实例化标记
        RectTransform marker = Instantiate(hunterMarkerPrefab, markersParent);
        marker.name = $"HunterMarker_{index}";
        
        // 设置样式
        Image markerImage = marker.GetComponent<Image>();
        if (markerImage != null)
        {
            // 根据猎人阵营设置颜色
            Color color = hunter.faction == GameManager.Faction.FactionA ? hunterAColor : hunterBColor;
            markerImage.color = color;
            
            // 创建方形标记
            CreateSquareSprite(markerImage);
        }
        
        // 设置锚点
        marker.pivot = new Vector2(0.5f, 0.5f);
        marker.anchorMin = new Vector2(0, 0);
        marker.anchorMax = new Vector2(0, 0);
        marker.sizeDelta = new Vector2(10, 10);
        
        return marker;
    }
    
    private void CreateSquareSprite(Image image)
    {
        // 创建简单的方形纹理
        int size = 16;
        Texture2D tex = new Texture2D(size, size);
        
        // 实心方形
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                if (x > 1 && x < size - 2 && y > 1 && y < size - 2)
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
        image.sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
    
    private void UpdateMiniMapCamera()
    {
        if (miniMapCamera == null || player == null) return;
        
        // 摄像机跟随玩家
        Vector3 targetPos = player.position;
        targetPos.z = miniMapCamera.transform.position.z;
        
        // 限制摄像机范围
        targetPos.x = Mathf.Clamp(targetPos.x, mapMinBounds.x, mapMaxBounds.x);
        targetPos.y = Mathf.Clamp(targetPos.y, mapMinBounds.y, mapMaxBounds.y);
        
        miniMapCamera.transform.position = targetPos;
    }
    
    private void UpdatePlayerMarker()
    {
        if (playerMarker == null || player == null || miniMapDisplay == null) return;
        
        // 转换玩家位置到小地图坐标
        Vector2 mapPos = WorldToMiniMapPosition(player.position);
        
        // 更新标记位置
        playerMarker.anchoredPosition = mapPos;
        
        // 更新玩家方向
        UpdateMarkerRotation(playerMarker, player);
    }
    
    private void UpdateHunterMarkers()
    {
        if (hunters == null || hunterMarkers == null) return;
        
        for (int i = 0; i < hunters.Length; i++)
        {
            if (hunters[i] == null || hunterMarkers[i] == null) continue;
            
            // 检查是否显示（只显示与玩家不同阵营的追捕者）
            bool shouldShow = ShouldShowHunter(hunters[i]);
            hunterMarkers[i].gameObject.SetActive(shouldShow);
            
            if (!shouldShow) continue;
            
            // 更新位置
            Vector2 mapPos = WorldToMiniMapPosition(hunters[i].transform.position);
            hunterMarkers[i].anchoredPosition = mapPos;
            
            // 更新方向
            UpdateMarkerRotation(hunterMarkers[i], hunters[i].transform);
        }
    }
    
    private bool ShouldShowHunter(HunterController hunter)
    {
        if (GameManager.Instance == null) return true;
        
        // 只显示与玩家不同阵营的追捕者
        GameManager.Faction playerFaction = GameManager.Instance.playerCurrentFaction;
        return playerFaction != hunter.faction;
    }
    
    private Vector2 WorldToMiniMapPosition(Vector3 worldPos)
    {
        if (miniMapCamera == null || miniMapDisplay == null) return Vector2.zero;
        
        // 将世界坐标转换为视口坐标
        Vector3 viewportPos = miniMapCamera.WorldToViewportPoint(worldPos);
        
        // 转换为小地图UI坐标（左上角锚点）
        RectTransform mapRT = miniMapDisplay.GetComponent<RectTransform>();
        
        // 视口坐标的y需要翻转（因为UI的y轴从上到下，而视口坐标的y从下到上）
        float x = viewportPos.x * mapRT.rect.width;
        float y = (1 - viewportPos.y) * mapRT.rect.height;
        
        return new Vector2(x, y);
    }
    
    private void UpdateMarkerRotation(RectTransform marker, Transform target)
    {
        if (marker == null || target == null) return;
        
        // 获取目标旋转角度（2D游戏中通常是Z轴旋转）
        float angle = -target.eulerAngles.z;
        marker.localEulerAngles = new Vector3(0, 0, angle);
    }
    
    // 公开方法：添加新的追捕者标记
    public void AddHunterMarker(GameObject hunter)
    {
        if (hunter == null) return;
        
        HunterController hc = hunter.GetComponent<HunterController>();
        if (hc == null) return;
        
        // 检查是否已存在
        for (int i = 0; i < hunters.Length; i++)
        {
            if (hunters[i] != null && hunters[i].gameObject == hunter)
                return;
        }
        
        // 扩展数组
        System.Array.Resize(ref hunters, hunters.Length + 1);
        System.Array.Resize(ref hunterMarkers, hunterMarkers.Length + 1);
        
        // 添加新追捕者
        hunters[hunters.Length - 1] = hc;
        hunterMarkers[hunterMarkers.Length - 1] = CreateHunterMarker(hc, hunterMarkers.Length - 1);
    }
    
    // 公开方法：移除追捕者标记
    public void RemoveHunterMarker(GameObject hunter)
    {
        if (hunter == null) return;
        
        for (int i = 0; i < hunters.Length; i++)
        {
            if (hunters[i] != null && hunters[i].gameObject == hunter)
            {
                // 销毁标记
                if (hunterMarkers[i] != null)
                    Destroy(hunterMarkers[i].gameObject);
                
                // 从数组中移除
                RemoveAt(ref hunters, i);
                RemoveAt(ref hunterMarkers, i);
                break;
            }
        }
    }
    
    private void RemoveAt<T>(ref T[] array, int index)
    {
        if (index < 0 || index >= array.Length) return;
        
        for (int i = index; i < array.Length - 1; i++)
        {
            array[i] = array[i + 1];
        }
        System.Array.Resize(ref array, array.Length - 1);
    }
    
    // 公开方法：设置地图边界
    public void SetMapBounds(Vector2 min, Vector2 max)
    {
        mapMinBounds = min;
        mapMaxBounds = max;
    }
    
    // 公开方法：切换小地图显示
    public void ToggleMiniMap(bool show)
    {
        if (miniMapDisplay != null)
            miniMapDisplay.gameObject.SetActive(show);
        
        if (playerMarker != null)
            playerMarker.gameObject.SetActive(show);
        
        if (markersParent != null)
            markersParent.gameObject.SetActive(show);
    }
    
    // 公开方法：调整小地图大小
    public void SetMiniMapSize(Vector2 newSize)
    {
        miniMapSize = newSize;
        
        if (miniMapDisplay != null)
        {
            RectTransform rt = miniMapDisplay.GetComponent<RectTransform>();
            rt.sizeDelta = newSize;
        }
    }
    
    // 公开方法：调整摄像机视野
    public void SetCameraSize(float newSize)
    {
        cameraSize = newSize;
        
        if (miniMapCamera != null)
            miniMapCamera.orthographicSize = newSize;
    }
    
    // 调试方法：在场景中绘制地图边界
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        
        Vector3 center = new Vector3(
            (mapMinBounds.x + mapMaxBounds.x) * 0.5f,
            (mapMinBounds.y + mapMaxBounds.y) * 0.5f,
            0
        );
        
        Vector3 size = new Vector3(
            mapMaxBounds.x - mapMinBounds.x,
            mapMaxBounds.y - mapMinBounds.y,
            1
        );
        
        Gizmos.DrawWireCube(center, size);
    }
}