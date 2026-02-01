using UnityEngine;
using UnityEngine.UI;
using System.Reflection; // 用于反射获取字段
using System.Collections.Generic; // 用于List

public class ValueBasedController : MonoBehaviour
{
    [Header("监控脚本")]
    public MonoBehaviour sourceScript;  // 在Inspector里拖SourceScript组件

    [Header("监控字段名称")]
    public string fieldName = "value"; // Inspector里手动填要监控的字段名

    [Header("滤镜物体")]
    public List<SpriteRenderer> filterSprites = new List<SpriteRenderer>();  // 可选，SpriteRenderer类型列表
    public List<Image> filterImages = new List<Image>();                    // 可选，UI Image类型列表

    [Header("动态物体")]
    public Transform movingObject;

    [Header("效果系数控制")]
    [Range(0f, 2f)]
    public float opacityMultiplier = 1f;      // 不透明度系数，1为原始效果
    [Range(0f, 2f)]
    public float amplitudeMultiplier = 1f;    // 正弦幅度系数
    [Range(0f, 2f)]
    public float periodMultiplier = 1f;       // 正弦周期系数

    private Vector3 startPos;
    private float elapsedTime = 0f;
    private FieldInfo monitoredField;

    void Start()
    {
        if (movingObject != null)
            startPos = movingObject.position;

        // 获取要监控的字段
        if (sourceScript != null && !string.IsNullOrEmpty(fieldName))
        {
            monitoredField = sourceScript.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
            if (monitoredField == null)
            {
                Debug.LogError($"在 {sourceScript.name} 找不到字段 {fieldName}");
            }
        }
    }

    void Update()
    {
        if (monitoredField == null) return;

        // 取得数值
        object valObj = monitoredField.GetValue(sourceScript);
        if (valObj == null) return;

        float value = Mathf.Clamp((float)valObj, 0f, 100f);

        // -------------------------------
        // 1. 控制滤镜不透明度
        // -------------------------------
        float alpha = Mathf.Clamp01((value / 100f) * opacityMultiplier);

        // 遍历SpriteRenderer列表
        foreach (var sprite in filterSprites)
        {
            if (sprite != null)
            {
                Color c = sprite.color;
                c.a = alpha;
                sprite.color = c;
            }
        }

        // 遍历Image列表
        foreach (var img in filterImages)
        {
            if (img != null)
            {
                Color c = img.color;
                c.a = alpha;
                img.color = c;
            }
        }

        // -------------------------------
        // 2. 控制动态物体横向正弦运动
        // -------------------------------
        if (movingObject != null)
        {
            elapsedTime += Time.deltaTime;

            // 将value映射为幅度和周期，并分别乘上对应系数
            float amplitude = Mathf.Lerp(0.5f, 5f, value / 100f) * amplitudeMultiplier;  // 幅度独立系数
            float period = Mathf.Lerp(2f, 0.5f, value / 100f) / Mathf.Max(periodMultiplier, 0.0001f); // 周期独立系数，避免除0
            float omega = 2f * Mathf.PI / period;

            float offsetX = amplitude * Mathf.Sin(omega * elapsedTime);
            movingObject.position = new Vector3(startPos.x + offsetX, startPos.y, startPos.z);
        }
    }
}
