using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance {get; private set;}
    [SerializeField] private string targetScene;
    // Start is called before the first frame update

    void Awake()
    {
        Instance = this;
        
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.anyKeyDown)
        {
            ScheduleTransitionToTitle(targetScene);
        }
    }

    public void ScheduleTransitionToTitle(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
