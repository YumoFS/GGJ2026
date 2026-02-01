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
    
    [Header("区域缩放效果")]
    [SerializeField] private bool enableAreaScaleEffect = false; // 启用进入区域时同时缩放
    [SerializeField] private Vector3 initialScale = Vector3.one; // 初始缩放值
    [SerializeField] private Vector3 targetScale = Vector3.zero; // 目标缩放值

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        // 根据 startVisible 初始化 alpha
        Color c = sr.color;
        c.a = startVisible ? 1f : 0f;
        sr.color = c;
        
        // 初始化缩放
        if (enableAreaScaleEffect)
        {
            // 根据可见状态设置初始缩放
            transform.localScale = startVisible ? targetScale : initialScale;
        }
        else
        {
            transform.localScale = initialScale;
        }

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
        fadeCoroutine = StartCoroutine(FadeTo(visible));
    }

    private System.Collections.IEnumerator FadeTo(bool visible)
    {
        isFading = true;
        
        float startAlpha = sr.color.a;
        float targetAlpha = visible ? 1f : 0f;
        float timer = 0f;
        
        Vector3 startScale = transform.localScale;
        Vector3 endScale = enableAreaScaleEffect ? 
            (visible ? targetScale : initialScale) : 
            startScale;

        // Debug.Log($"{startAlpha}, {targetAlpha}, {startScale}, {targetScale}");
        
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);
            
            Color c = sr.color;
            c.a = Mathf.Lerp(startAlpha, targetAlpha, t);
            sr.color = c;
            
            // 如果启用了缩放效果，同时渐变缩放
            if (enableAreaScaleEffect /*&& endScale == targetScale*/)
            {
                transform.localScale = Vector3.Lerp(startScale, endScale, t);
            }
            
            yield return null;
        }
        
        // 确保最终值准确
        Color finalColor = sr.color;
        finalColor.a = targetAlpha;
        sr.color = finalColor;
        
        if (enableAreaScaleEffect)
        {
            transform.localScale = endScale;
        }
        
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
