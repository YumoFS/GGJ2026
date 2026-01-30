// FactionZone.cs
using UnityEngine;

public class FactionZone : MonoBehaviour
{
    [Header("阵营设置")]
    public GameManager.Faction faction = GameManager.Faction.FactionA;
    [SerializeField] private Color zoneColor = Color.blue;
    
    [Header("视觉效果")]
    [SerializeField] private SpriteRenderer zoneVisual;
    [SerializeField] private float pulseSpeed = 1f;
    [SerializeField] private float pulseIntensity = 0.2f;
    
    private float pulseTimer = 0f;
    private Color originalColor;
    
    private void Start()
    {
        if (zoneVisual != null)
        {
            originalColor = zoneColor;
            zoneVisual.color = originalColor;
        }
    }
    
    private void Update()
    {
        // 区域脉动效果
        if (zoneVisual != null)
        {
            pulseTimer += Time.deltaTime * pulseSpeed;
            float pulseValue = Mathf.Sin(pulseTimer) * pulseIntensity;
            Color pulseColor = originalColor * (1 + pulseValue);
            pulseColor.a = originalColor.a;
            zoneVisual.color = pulseColor;
        }
    }
    
    public void OnPlayerEnter(PlayerController player)
    {
        // 玩家进入区域
        Debug.Log($"玩家进入{faction}区域");
        
        // 可以在这里添加进入区域的音效/粒子效果
    }
    
    public void OnPlayerExit(PlayerController player)
    {
        // 玩家离开区域
        Debug.Log($"玩家离开{faction}区域");
    }
    
    private void OnDrawGizmos()
    {
        // 显示区域范围
        Gizmos.color = new Color(zoneColor.r, zoneColor.g, zoneColor.b, 0.3f);
        Gizmos.DrawCube(transform.position, transform.localScale);
    }
}