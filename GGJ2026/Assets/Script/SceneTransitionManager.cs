using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance {get; private set;}
    [SerializeField] private string targetScene;
    private bool canInput = false;
    // Start is called before the first frame update

    void Awake()
    {
        Instance = this;
        
    }
    void Start()
    {
        StartCoroutine(EnableInputAfterDelay(2f));
        
    }
    
    private IEnumerator EnableInputAfterDelay(float delaySeconds)
    {
        Debug.Log($"等待 {delaySeconds} 秒后允许输入...");
        
        // 等待指定的秒数
        yield return new WaitForSeconds(delaySeconds);
        
        // 允许输入
        canInput = true;
        Debug.Log("现在可以输入指令了！");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.anyKeyDown && canInput)
        {
            Application.Quit();
        }
    }

    public void ScheduleTransitionToTitle(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
