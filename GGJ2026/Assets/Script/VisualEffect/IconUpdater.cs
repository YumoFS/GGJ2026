// SimpleIconUpdater.cs - 极简的图标更新脚本
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class IconUpdater : MonoBehaviour
{
    [Header("目标设置")]
    [SerializeField] private Transform target; // 跟踪的目标
    [SerializeField] private MiniMapController miniMap; // 小地图控制器
    [SerializeField] private bool isPlayer = false; // 是否是玩家图标
    
    [Header("图标设置")]
    [SerializeField] private bool rotateWithTarget = true; // 是否跟随目标旋转
    [SerializeField] private Color defaultColor = Color.white;
    
    private RectTransform rectTransform;
    private Image iconImage;
    private bool isInitialized = false;
    
    private void Start()
    {
        Initialize();
    }
    
    private void Update()
    {
        if (!isInitialized || target == null) return;
        
        UpdatePosition();
        UpdateRotation();
    }
    
    private void Initialize()
    {
        rectTransform = GetComponent<RectTransform>();
        iconImage = GetComponent<Image>();
        
        if (iconImage != null)
        {
            iconImage.color = defaultColor;
        }
        
        isInitialized = true;
    }
    
    private void UpdatePosition()
    {
        if (miniMap == null || rectTransform == null) return;
        
        // 获取目标在视口的位置
        Vector3 viewportPos = miniMap.WorldToViewportPoint(target.position);
        
        // 转换为小地图坐标
        Vector2 miniMapPos = miniMap.ViewportToMiniMapPosition(viewportPos);
        
        // 更新位置
        rectTransform.anchoredPosition = miniMapPos;
    }
    
    private void UpdateRotation()
    {
        if (!rotateWithTarget || rectTransform == null || target == null) return;
        
        // 获取目标的方向（假设2D游戏中角色朝向上方）
        float angle = GetTargetAngle();
        rectTransform.localEulerAngles = new Vector3(0, 0, angle);
    }
    
    private float GetTargetAngle()
    {
        // 获取目标的旋转角度
        // 根据您的游戏设置，可能需要调整
        return -target.eulerAngles.z; // 2D游戏中通常使用Z轴旋转
    }
    
    // 设置目标
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
    
    // 设置小地图控制器
    public void SetMiniMap(MiniMapController newMiniMap)
    {
        miniMap = newMiniMap;
    }
    
    // 设置图标颜色
    public void SetIconColor(Color color)
    {
        if (iconImage != null)
            iconImage.color = color;
    }
    
    // 设置是否旋转
    public void SetRotateWithTarget(bool rotate)
    {
        rotateWithTarget = rotate;
    }
}