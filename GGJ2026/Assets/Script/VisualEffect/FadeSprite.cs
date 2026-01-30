using System.Runtime.InteropServices;
using UnityEngine;

public class FadeSpriteToggleWithStart : MonoBehaviour
{
    public float duration = 1f;          // 渐变时间
    public bool startVisible = true;     // 开局完全不透明 = true，完全透明 = false

    private SpriteRenderer sr;
    private bool isVisible;              // 当前状态
    private bool isFading = false;
    private Coroutine fadeCoroutine;

    [Header("阵营设置")]
    [SerializeField] private GameManager.Faction maskFaction = GameManager.Faction.FactionA;

    [Header("多面具管理")]
    [SerializeField] private bool manageMultipleMasks = false;
    [SerializeField] private FadeSpriteToggleWithStart[] otherMasks; // 其他面具控制器

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        // 根据 startVisible 初始化 alpha
        Color c = sr.color;
        c.a = startVisible ? 1f : 0f;
        sr.color = c;

        isVisible = startVisible;
        UpdateMaskVisibility();
    }

    void Update()
    {
        // if (Input.GetKeyDown(KeyCode.C) && !isFading)
        // {
        //     StartCoroutine(Fade());
        // }
        UpdateMaskVisibility();
    }

    private void UpdateMaskVisibility()
    {
        GameManager.Faction playerFaction = GameManager.Instance.playerCurrentFaction;
        bool shouldShow = playerFaction == maskFaction;
        
        SetMaskVisible(shouldShow);
        
        if (manageMultipleMasks && shouldShow)
        {
            HideOtherMasks();
        }
    }

    private void SetMaskVisible(bool visible)
    {
        if (sr == null) return;
        
        if (isFading)
        {
            // 停止正在进行的渐变
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);
            isFading = false;
        }
        
            // 使用渐变效果
        fadeCoroutine = StartCoroutine(FadeTo(visible ? 1f : 0f));
    }

    private System.Collections.IEnumerator FadeTo(float targetAlpha)
    {
        isFading = true;
        
        float startAlpha = sr.color.a;
        float timer = 0f;
        
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);
            
            Color c = sr.color;
            c.a = Mathf.Lerp(startAlpha, targetAlpha, t);
            sr.color = c;
            
            yield return null;
        }
        
        // 确保最终值准确
        Color finalColor = sr.color;
        finalColor.a = targetAlpha;
        sr.color = finalColor;
        
        isFading = false;
    }
    
    private void HideOtherMasks()
    {
        if (otherMasks == null) return;
        
        foreach (FadeSpriteToggleWithStart otherMask in otherMasks)
        {
            if (otherMask != null && otherMask != this)
            {
                otherMask.SetMaskVisible(false);
            }
        }
    }

/*    System.Collections.IEnumerator Fade()
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
    }*/
}
