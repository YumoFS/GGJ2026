using UnityEngine;

public class FadeSpriteToggleWithStart : MonoBehaviour
{
    public float duration = 1f;          // 渐变时间
    public bool startVisible = true;     // 开局完全不透明 = true，完全透明 = false

    private SpriteRenderer sr;
    private bool isVisible;              // 当前状态
    private bool isFading = false;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        // 根据 startVisible 初始化 alpha
        Color c = sr.color;
        c.a = startVisible ? 1f : 0f;
        sr.color = c;

        isVisible = startVisible;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C) && !isFading)
        {
            StartCoroutine(Fade());
        }
    }

    System.Collections.IEnumerator Fade()
    {
        isFading = true;

        float startAlpha = sr.color.a;
        float targetAlpha = isVisible ? 0f : 1f;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            Color c = sr.color;
            c.a = Mathf.Lerp(startAlpha, targetAlpha, t);
            sr.color = c;

            yield return null;
        }

        // 保证最终 alpha 精准
        Color finalColor = sr.color;
        finalColor.a = targetAlpha;
        sr.color = finalColor;

        isVisible = !isVisible;
        isFading = false;
    }
}
