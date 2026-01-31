// SimpleMiniMapController.cs - 简化小地图控制器
using UnityEngine;
using UnityEngine.UI;

public class MiniMapController : MonoBehaviour
{
    [Header("小地图设置")]
    [SerializeField] private Camera miniMapCamera;
    [SerializeField] private RawImage miniMapDisplay;
    [SerializeField] private Transform player;
    
    [Header("地图边界")]
    [SerializeField] private Vector2 mapMinBounds = new Vector2(-50, -50);
    [SerializeField] private Vector2 mapMaxBounds = new Vector2(50, 50);
    [SerializeField] private bool clampCamera = true; // 是否限制摄像机移动范围
    
    [Header("标记设置")]
    [SerializeField] private RectTransform playerIcon;
    [SerializeField] private RectTransform hunterIconPrefab;
    [SerializeField] private Transform miniMapIconsParent; // 标记的父物体
    
    [Header("小地图大小")]
    [SerializeField] private Vector2 miniMapSize = new Vector2(200, 200);
    [SerializeField] private Vector2 miniMapPosition = new Vector2(20, 20);
    
    private HunterController[] hunters;
    private RectTransform[] hunterIcons;
    
    private void Start()
    {
        InitializeMiniMap();
        InitializeIcons();
    }
    
    private void Update()
    {
        UpdateMiniMapCamera();
        UpdatePlayerIcon();
        UpdateHunterIcons();
    }
    
    private void InitializeMiniMap()
    {
        // 如果没指定玩家，查找Player标签
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }
        
        // 如果没指定小地图摄像机，创建或查找
        if (miniMapCamera == null)
        {
            miniMapCamera = GetComponent<Camera>();
            if (miniMapCamera == null)
            {
                GameObject camObj = new GameObject("MiniMapCamera");
                camObj.transform.SetParent(transform);
                miniMapCamera = camObj.AddComponent<Camera>();
                SetupMiniMapCamera();
            }
        }
        
        // 设置小地图显示位置和大小
        if (miniMapDisplay != null)
        {
            RectTransform rt = miniMapDisplay.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.sizeDelta = miniMapSize;
                rt.anchoredPosition = miniMapPosition;
            }
        }
        
        // 创建Render Texture（如果没设置）
        if (miniMapCamera.targetTexture == null && miniMapDisplay != null)
        {
            CreateRenderTexture();
        }
    }
    
    private void SetupMiniMapCamera()
    {
        miniMapCamera.orthographic = true;
        miniMapCamera.orthographicSize = 20f; // 默认视野大小
        miniMapCamera.clearFlags = CameraClearFlags.SolidColor;
        miniMapCamera.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.7f);
        miniMapCamera.cullingMask = LayerMask.GetMask("MiniMap"); // 只渲染MiniMap层
        miniMapCamera.depth = 1;
    }
    
    private void CreateRenderTexture()
    {
        RenderTexture rt = new RenderTexture(256, 256, 16);
        rt.name = "MiniMap_RT";
        miniMapCamera.targetTexture = rt;
        miniMapDisplay.texture = rt;
    }
    
    private void InitializeIcons()
    {
        // 查找所有猎人
        hunters = FindObjectsOfType<HunterController>();
        hunterIcons = new RectTransform[hunters.Length];
        
        // 为每个猎人创建图标
        for (int i = 0; i < hunters.Length; i++)
        {
            if (hunters[i] != null)
            {
                hunterIcons[i] = CreateHunterIcon(hunters[i].gameObject, i);
            }
        }
        
        Debug.Log($"小地图初始化完成: {hunters.Length} 个猎人");
    }
    
    private RectTransform CreateHunterIcon(GameObject hunter, int index)
    {
        if (hunterIconPrefab == null || miniMapIconsParent == null)
            return null;
        
        RectTransform icon = Instantiate(hunterIconPrefab, miniMapIconsParent);
        icon.name = $"HunterIcon_{index}";
        
        // 根据猎人阵营设置颜色
        HunterController hc = hunter.GetComponent<HunterController>();
        if (hc != null)
        {
            Image iconImage = icon.GetComponent<Image>();
            if (iconImage != null)
            {
                // 根据GameManager.Faction设置颜色
                // 这里需要根据您的GameManager设置具体颜色
                Color factionColor = hc.faction == GameManager.Faction.FactionA ? 
                    new Color(0.2f, 0.6f, 1f) : new Color(1f, 0.3f, 0.2f);
                iconImage.color = factionColor;
            }
        }
        
        return icon;
    }
    
    private void UpdateMiniMapCamera()
    {
        if (miniMapCamera == null || player == null) return;
        
        // 跟随玩家
        Vector3 targetPos = player.position;
        targetPos.z = miniMapCamera.transform.position.z;
        
        // 限制摄像机范围（如果启用）
        if (clampCamera)
        {
            targetPos.x = Mathf.Clamp(targetPos.x, mapMinBounds.x, mapMaxBounds.x);
            targetPos.y = Mathf.Clamp(targetPos.y, mapMinBounds.y, mapMaxBounds.y);
        }
        
        miniMapCamera.transform.position = targetPos;
    }
    
    private void UpdatePlayerIcon()
    {
        if (playerIcon == null || player == null || miniMapDisplay == null) return;
        
        // 如果玩家在边界内，显示图标
        bool isInBounds = IsWithinBounds(player.position);
        playerIcon.gameObject.SetActive(isInBounds);
        
        if (!isInBounds) return;
        
        // 更新玩家图标位置
        Vector2 viewportPos = WorldToViewportPoint(player.position);
        Vector2 screenPos = ViewportToMiniMapPosition(viewportPos);
        playerIcon.anchoredPosition = screenPos;
        
        // 更新玩家图标旋转（如果需要）
        UpdateIconRotation(playerIcon, player);
    }
    
    private void UpdateHunterIcons()
    {
        for (int i = 0; i < hunters.Length; i++)
        {
            if (hunters[i] == null || hunterIcons[i] == null) continue;
            
            // 检查猎人是否有效
            if (!hunters[i].gameObject.activeInHierarchy)
            {
                hunterIcons[i].gameObject.SetActive(false);
                continue;
            }
            
            // 判断是否显示（只显示敌对阵营的猎人）
            bool shouldShow = ShouldShowHunter(hunters[i]);
            hunterIcons[i].gameObject.SetActive(shouldShow);
            
            if (!shouldShow) continue;
            
            // 更新位置
            Vector2 viewportPos = WorldToViewportPoint(hunters[i].transform.position);
            Vector2 screenPos = ViewportToMiniMapPosition(viewportPos);
            hunterIcons[i].anchoredPosition = screenPos;
            
            // 更新旋转
            UpdateIconRotation(hunterIcons[i], hunters[i].transform);
        }
    }
    
    private bool ShouldShowHunter(HunterController hunter)
    {
        if (GameManager.Instance == null) return true;
        
        // 只显示与玩家不同阵营的猎人
        GameManager.Faction playerFaction = GameManager.Instance.playerCurrentFaction;
        return playerFaction != hunter.faction;
    }
    
    private bool IsWithinBounds(Vector3 worldPosition)
    {
        if (!clampCamera) return true;
        
        return worldPosition.x >= mapMinBounds.x && worldPosition.x <= mapMaxBounds.x &&
               worldPosition.y >= mapMinBounds.y && worldPosition.y <= mapMaxBounds.y;
    }
    
    public Vector2 WorldToViewportPoint(Vector3 worldPos)
    {
        if (miniMapCamera == null) return Vector2.zero;
        
        Vector3 viewportPos = miniMapCamera.WorldToViewportPoint(worldPos);
        return new Vector2(viewportPos.x, viewportPos.y);
    }
    
    public Vector2 ViewportToMiniMapPosition(Vector2 viewportPos)
    {
        if (miniMapDisplay == null) return Vector2.zero;
        
        RectTransform rt = miniMapDisplay.GetComponent<RectTransform>();
        if (rt == null) return Vector2.zero;
        
        // 将视口坐标转换为小地图UI坐标
        return new Vector2(
            (viewportPos.x - 0.5f) * rt.rect.width,
            (viewportPos.y - 0.5f) * rt.rect.height
        );
    }
    
    private void UpdateIconRotation(RectTransform icon, Transform target)
    {
        if (icon == null || target == null) return;
        
        // 获取目标面向的方向（2D游戏中通常是transform.right或transform.up）
        Vector3 direction = target.up; // 假设角色朝向上方
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        icon.localEulerAngles = new Vector3(0, 0, angle);
    }
    
    // 公开方法：添加新猎人标记
    public void AddHunterIcon(GameObject hunter)
    {
        if (hunter == null) return;
        
        // 检查是否已经存在
        for (int i = 0; i < hunters.Length; i++)
        {
            if (hunters[i] != null && hunters[i].gameObject == hunter)
                return;
        }
        
        // 添加到数组
        System.Array.Resize(ref hunters, hunters.Length + 1);
        System.Array.Resize(ref hunterIcons, hunterIcons.Length + 1);
        
        hunters[hunters.Length - 1] = hunter.GetComponent<HunterController>();
        hunterIcons[hunterIcons.Length - 1] = CreateHunterIcon(hunter, hunterIcons.Length - 1);
    }
    
    // 公开方法：移除猎人标记
    public void RemoveHunterIcon(GameObject hunter)
    {
        if (hunter == null) return;
        
        for (int i = 0; i < hunters.Length; i++)
        {
            if (hunters[i] != null && hunters[i].gameObject == hunter)
            {
                // 销毁图标
                if (hunterIcons[i] != null)
                    Destroy(hunterIcons[i].gameObject);
                
                // 从数组中移除
                RemoveAt(ref hunters, i);
                RemoveAt(ref hunterIcons, i);
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
        clampCamera = true;
    }
    
    // 公开方法：切换小地图显示
    public void ToggleMiniMap(bool show)
    {
        if (miniMapDisplay != null)
            miniMapDisplay.gameObject.SetActive(show);
        
        if (playerIcon != null)
            playerIcon.gameObject.SetActive(show);
        
        if (miniMapIconsParent != null)
            miniMapIconsParent.gameObject.SetActive(show);
    }
    
    // 绘制边界Gizmo（调试用）
    private void OnDrawGizmosSelected()
    {
        if (!clampCamera) return;
        
        // 绘制地图边界
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
        
        // 标注边界坐标
        #if UNITY_EDITOR
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.yellow;
        
        UnityEditor.Handles.Label(new Vector3(mapMinBounds.x, mapMinBounds.y, 0), 
            $"Min: ({mapMinBounds.x}, {mapMinBounds.y})", style);
        UnityEditor.Handles.Label(new Vector3(mapMaxBounds.x, mapMaxBounds.y, 0), 
            $"Max: ({mapMaxBounds.x}, {mapMaxBounds.y})", style);
        #endif
    }
}