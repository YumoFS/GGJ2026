using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingManager : MonoBehaviour
{
    public GameObject SettingCanvas;

    public void ChangeLanguageToEN()
    {
        LanguageSystem languageSystem = FindObjectOfType<LanguageSystem>();
        
        if (languageSystem != null)
        {
            languageSystem.ChangeLanguage("en");
        }
    }

    public void ChangeLanguageToZH()
    {
        LanguageSystem languageSystem = FindObjectOfType<LanguageSystem>();
        
        if (languageSystem != null)
        {
            languageSystem.ChangeLanguage("zh-Hans");
        }
    }
    public void Quitgame()
    {
        Application.Quit();
    }
    public void ToMenu()
    {
        SceneManager.LoadScene("Startmenu");
    }
    // 打开制作人员窗口
    public void OpenSettings()
    {
        SettingCanvas.SetActive(true);
    }

    // 关闭窗口
    public void CloseSettings()
    {
        SettingCanvas.SetActive(false);
    }
}
