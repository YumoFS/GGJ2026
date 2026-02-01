using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class MenuController : MonoBehaviour
{
    // 在 Inspector 面板中把刚才创建的 CreditsPopup 拖进来
    public GameObject creditsPopup;
    public GameObject SettingCanvas;
    public void StartGame()
    {
        // "map" 是你游戏主地图场景的名字，请确保与场景文件名一致
        SceneManager.LoadScene("BackgroundText");
    }
    public void Quitgame()
    {
        Application.Quit();
    }
    public void ToSettings()
    {
        // SceneManager.LoadScene("Settings");
    }
    // 打开制作人员窗口
    public void OpenCredits()
    {
        creditsPopup.SetActive(true);
    }

    // 关闭窗口
    public void CloseCredits()
    {
        creditsPopup.SetActive(false);
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
