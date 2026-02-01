using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Components;
using System.Collections;

public class LanguageSystem : MonoBehaviour
{
    private bool isInitialized = false;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private IEnumerator Start()
    {
        // 等待本地化系统初始化
        yield return LocalizationSettings.InitializationOperation;
        isInitialized = true;
        Debug.Log("Localization initialized");
    }
    
    public void ChangeLanguage(string languageCode)
    {
        if (!isInitialized)
        {
            Debug.LogError("Localization not initialized yet!");
            StartCoroutine(ChangeLanguageWhenReady(languageCode));
            return;
        }
        
        StartCoroutine(SetLocaleCoroutine(languageCode));
    }
    
    private IEnumerator ChangeLanguageWhenReady(string languageCode)
    {
        yield return new WaitUntil(() => isInitialized);
        StartCoroutine(SetLocaleCoroutine(languageCode));
    }
    
    private IEnumerator SetLocaleCoroutine(string languageCode)
    {
        // 再次确认初始化
        yield return LocalizationSettings.InitializationOperation;
        
        var locales = LocalizationSettings.AvailableLocales.Locales;
        
        if (locales == null || locales.Count == 0)
        {
            Debug.LogError("No locales available!");
            yield break;
        }
        
        Locale targetLocale = null;
        
        foreach (var locale in locales)
        {
            Debug.Log($"Available locale: {locale.Identifier.Code}");
            if (locale.Identifier.Code == languageCode)
            {
                targetLocale = locale;
                break;
            }
        }
        
        if (targetLocale != null)
        {
            LocalizationSettings.SelectedLocale = targetLocale;
            Debug.Log($"Language switched to: {targetLocale.Identifier.Code}");
            
            // 强制刷新UI
            ForceRefreshUI();
        }
        else
        {
            Debug.LogError($"Locale not found: {languageCode}");
        }
    }
    
    private void ForceRefreshUI()
    {
        // 重新加载当前场景或通知所有UI组件刷新
        var localizeStringEvents = FindObjectsOfType<LocalizeStringEvent>();
        foreach (var lse in localizeStringEvents)
        {
            lse.RefreshString();
        }
    }
}