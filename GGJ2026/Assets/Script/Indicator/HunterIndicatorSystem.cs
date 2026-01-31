using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HunterIndicatorSystem : MonoBehaviour
{
    [Header("指示器设置")]
    [SerializeField] private GameObject indicatorPrefab; // 三角形指示器预制体
    [SerializeField] private float screenMargin = 50f;   // 距离屏幕边缘的距离
    [SerializeField] private float minScale = 0.5f;     // 最小缩放
    [SerializeField] private float maxScale = 1.5f;      // 最大缩放
    [SerializeField] private float maxDistance = 50f;    // 最大距离（用于缩放计算）
    
    [Header("颜色设置")]
    [SerializeField] private Color indicatorColor = Color.red;
    [SerializeField] private float fadeDistance = 10f;   // 淡入淡出距离
    
    private Transform player;  // 玩家 Transform
    private Camera mainCamera; // 主相机
    private RectTransform canvasRect; // Canvas的RectTransform
    
    // 存储猎人及其指示器
    private Dictionary<Transform, RectTransform> hunterIndicators = new Dictionary<Transform, RectTransform>();
    private Dictionary<Transform, Image> indicatorImages = new Dictionary<Transform, Image>();
    
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        mainCamera = Camera.main;
        
        // 获取Canvas
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            canvasRect = canvas.GetComponent<RectTransform>();
        }
    }
    
    void Update()
    {
        if (player == null || mainCamera == null) return;
        
        // 更新所有猎人的指示器
        foreach (var hunter in hunterIndicators.Keys)
        {
            if (hunter == null) continue;
            
            UpdateIndicatorPosition(hunter);
            UpdateIndicatorScaleAndRotation(hunter);
        }
    }
    
    // 注册猎人
    public void RegisterHunter(Transform hunter)
    {
        if (hunterIndicators.ContainsKey(hunter)) return;
        
        // 实例化指示器
        GameObject indicatorObj = Instantiate(indicatorPrefab, transform);
        RectTransform indicatorRT = indicatorObj.GetComponent<RectTransform>();
        Image indicatorImage = indicatorObj.GetComponent<Image>();
        
        // 设置颜色
        indicatorImage.color = indicatorColor;
        
        // 存储引用
        hunterIndicators[hunter] = indicatorRT;
        indicatorImages[hunter] = indicatorImage;
        
        // 初始隐藏
        indicatorObj.SetActive(false);
    }
    
    // 注销猎人
    public void UnregisterHunter(Transform hunter)
    {
        if (hunterIndicators.ContainsKey(hunter))
        {
            if (hunterIndicators[hunter] != null)
                Destroy(hunterIndicators[hunter].gameObject);
            
            hunterIndicators.Remove(hunter);
            indicatorImages.Remove(hunter);
        }
    }
    
    private void UpdateIndicatorPosition(Transform hunter)
    {
        RectTransform indicatorRT = hunterIndicators[hunter];
        
        // 将猎人世界坐标转换为屏幕坐标
        Vector3 screenPos = mainCamera.WorldToScreenPoint(hunter.position);
        
        // 检查猎人是否在屏幕外
        bool isOnScreen = screenPos.x >= 0 && screenPos.x <= Screen.width &&
                          screenPos.y >= 0 && screenPos.y <= Screen.height &&
                          screenPos.z > 0; // 确保在相机前方
        
        if (isOnScreen)
        {
            // 猎人在屏幕内，隐藏指示器
            indicatorRT.gameObject.SetActive(false);
            return;
        }
        
        // 猎人在屏幕外，显示指示器
        indicatorRT.gameObject.SetActive(true);
        
        // 如果猎人在相机后方，反转屏幕坐标
        if (screenPos.z < 0)
        {
            screenPos *= -1;
        }
        
        // 计算屏幕中心点
        Vector3 screenCenter = new Vector3(Screen.width, Screen.height, 0) / 2;
        
        // 将屏幕坐标置中
        screenPos -= screenCenter;
        
        // 计算屏幕边缘位置
        float angle = Mathf.Atan2(screenPos.y, screenPos.x);
        float slope = Mathf.Tan(angle);
        
        // 计算屏幕边界
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        
        // 根据角度确定指示器应该放置在哪条边上
        Vector3 screenBounds = screenCenter;
        
        if (Mathf.Abs(slope) > screenHeight / screenWidth)
        {
            // 上下边缘
            screenBounds.x = screenCenter.y / Mathf.Abs(slope);
            screenBounds.y = screenCenter.y;
        }
        else
        {
            // 左右边缘
            screenBounds.x = screenCenter.x;
            screenBounds.y = screenCenter.x * Mathf.Abs(slope);
        }
        
        // 根据象限调整符号
        screenBounds.x *= Mathf.Sign(screenPos.x);
        screenBounds.y *= Mathf.Sign(screenPos.y);
        
        // 添加边距
        screenBounds.x = Mathf.Clamp(screenBounds.x, -screenCenter.x + screenMargin, screenCenter.x - screenMargin);
        screenBounds.y = Mathf.Clamp(screenBounds.y, -screenCenter.y + screenMargin, screenCenter.y - screenMargin);
        
        // 转换回Canvas空间
        Vector2 canvasPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, 
            screenBounds + screenCenter, 
            null, 
            out canvasPos
        );
        
        indicatorRT.anchoredPosition = canvasPos;
    }
    
    private void UpdateIndicatorScaleAndRotation(Transform hunter)
    {
        RectTransform indicatorRT = hunterIndicators[hunter];
        Image indicatorImage = indicatorImages[hunter];
        
        // 计算距离
        float distance = Vector3.Distance(player.position, hunter.position);
        
        // 根据距离计算缩放（距离越远，指示器越小）
        float normalizedDistance = Mathf.Clamp01(distance / maxDistance);
        float scale = Mathf.Lerp(maxScale, minScale, normalizedDistance);
        indicatorRT.localScale = Vector3.one * scale;
        
        // 计算旋转角度（指向猎人）
        Vector3 direction = (hunter.position - player.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        indicatorRT.rotation = Quaternion.Euler(0, 0, angle - 90); // -90使三角形指向正确方向
        
        // 根据距离调整透明度（可选）
        if (distance < fadeDistance)
        {
            float alpha = Mathf.Lerp(0.3f, 1f, distance / fadeDistance);
            Color color = indicatorImage.color;
            color.a = alpha;
            indicatorImage.color = color;
        }
    }
    
    // 清空所有指示器
    public void ClearAllIndicators()
    {
        foreach (var indicator in hunterIndicators.Values)
        {
            if (indicator != null)
                Destroy(indicator.gameObject);
        }
        hunterIndicators.Clear();
        indicatorImages.Clear();
    }
}