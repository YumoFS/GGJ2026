using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class MenuController : MonoBehaviour
{
    public void StartGame()
    {
        // "GameScene" 是你游戏主地图场景的名字，请确保与场景文件名一致
        SceneManager.LoadScene("map");
    }
    public void Quitgame()
    {
        Application.Quit();
    }
}
