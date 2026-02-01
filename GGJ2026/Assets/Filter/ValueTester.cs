using UnityEngine;

// 这是一个测试脚本，用于让 value 在0~100之间循环
public class ValueTester : MonoBehaviour
{
    [Header("模拟数值")]
    public float value = 0f;      // 这个值会被ValueBasedController监控
    public float cycleDuration = 5f; // 完成一次0->100->0的周期时间（秒）

    private float elapsedTime = 0f;

    void Update()
    {
        elapsedTime += Time.deltaTime;

        // 正弦波从0到100
        // sin(-π/2) = -1 -> 0， sin(π/2) = 1 -> 100
        float sinValue = Mathf.Sin((elapsedTime / cycleDuration) * 2f * Mathf.PI - Mathf.PI/2f);
        value = Mathf.Lerp(0f, 100f, (sinValue + 1f) / 2f);
    }
}
