
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class LanguageScript : MonoBehaviour
{
    // 通过语言代码切换（如 "en", "ja", "zh-CN"）
    public static void ChangeLanguage(string languageCode)
    {
        var locales = LocalizationSettings.AvailableLocales.Locales;
        
        foreach (var locale in locales)
        {
            if (locale.Identifier.Code == languageCode)
            {
                LocalizationSettings.SelectedLocale = locale;
                Debug.Log($"语言已切换至: {locale.Identifier.Code}");
                break;
            }
        }
    }
    
}