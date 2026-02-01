using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Events;

public class IntroSequenceController : MonoBehaviour
{
    [Header("Text")]
    // ★ 原 introText 升级为多行
    public TextMeshProUGUI[] introLines;
    public TextMeshProUGUI pressAnyKeyText;

    [Header("Audio")]
    public AudioSource introAudio;
    public AudioSource imageGroupBAudio;

    [Header("Image Groups")]
    public CanvasGroup imageGroupA;
    public CanvasGroup imageGroupB;

    [Header("Timing")]
    // ★ 文本相关时间
    public float lineFadeInTime = 1.2f;   // 每行淡入
    public float[] lineHoldTimes;         // ★ 每行单独 hold 时间
    public float finalLineHoldTime = 2f;  // 四行全出现后的停顿
    public float lineFadeOutTime = 2f;    // 四行一起淡出

    public float imageAFadeInTime = 2f;
    public float imageDelayBetween = 1.5f;
    public float imageBFadeInTime = 2f;
    public float imageHoldTime = 2f;
    public float imageFadeOutTime = 2f;

    public float pressKeyFadeInTime = 1.5f;

    [Header("Scene")]
    public string nextSceneName;

    [Header("Events")]
    public UnityEvent onIntroTextFinished;

    private bool canEnter = false;

    void Start()
    {
        InitVisuals();
        StartCoroutine(PlaySequence());
    }

    void Update()
    {
        if (canEnter && Input.anyKeyDown)
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }

    void InitVisuals()
    {
        // ★ 初始化所有 Intro Lines
        foreach (var line in introLines)
        {
            SetAlpha(line, 0f);
        }

        SetAlpha(pressAnyKeyText, 0f);
        pressAnyKeyText.gameObject.SetActive(false);

        imageGroupA.alpha = 0f;
        imageGroupB.alpha = 0f;
    }

    IEnumerator PlaySequence()
    {
        // ===== Intro Text =====
        if (introAudio != null)
            introAudio.Play();

        // ★ 一行一行淡入 + 独立 hold
        for (int i = 0; i < introLines.Length; i++)
        {
            yield return FadeTMP(introLines[i], 0f, 1f, lineFadeInTime);

            // ★ 防止数组长度不一致导致报错
            float holdTime = (lineHoldTimes != null && i < lineHoldTimes.Length)
                ? lineHoldTimes[i]
                : 1.5f;

            yield return new WaitForSeconds(holdTime);
        }

        // ★ 四行全部出现后的停顿
        yield return new WaitForSeconds(finalLineHoldTime);

        // ★ 四行一起淡出
        foreach (var line in introLines)
        {
            StartCoroutine(FadeTMP(line, 1f, 0f, lineFadeOutTime));
        }

        yield return new WaitForSeconds(lineFadeOutTime);

        // ★ Intro 文本阶段正式结束
        onIntroTextFinished?.Invoke();

        // ===== Image Group A =====
        yield return FadeCanvasGroup(imageGroupA, 0f, 1f, imageAFadeInTime);

        // Delay before Group B
        yield return new WaitForSeconds(imageDelayBetween);

        // ===== Image Group B =====
        if (imageGroupBAudio != null)
        {
            imageGroupBAudio.Play();
        }

        yield return FadeCanvasGroup(imageGroupB, 0f, 1f, imageBFadeInTime);

        // Hold both
        yield return new WaitForSeconds(imageHoldTime);

        // ===== Fade Out Both =====
        StartCoroutine(FadeCanvasGroup(imageGroupA, 1f, 0f, imageFadeOutTime));
        yield return FadeCanvasGroup(imageGroupB, 1f, 0f, imageFadeOutTime);

        // ===== Press Any Key =====
        pressAnyKeyText.gameObject.SetActive(true);
        yield return FadeTMP(pressAnyKeyText, 0f, 1f, pressKeyFadeInTime);

        canEnter = true;
    }

    IEnumerator FadeTMP(TextMeshProUGUI text, float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(from, to, t / duration);
            SetAlpha(text, a);
            yield return null;
        }
        SetAlpha(text, to);
    }

    IEnumerator FadeCanvasGroup(CanvasGroup group, float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            group.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        group.alpha = to;
    }

    void SetAlpha(TextMeshProUGUI text, float alpha)
    {
        Color c = text.color;
        c.a = alpha;
        text.color = c;
    }
}
