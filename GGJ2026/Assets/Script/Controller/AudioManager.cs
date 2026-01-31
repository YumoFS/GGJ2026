using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class AudioManager : MonoBehaviour
{
    [Header("音频设置")]
    public AudioClip BGM_1;
    public AudioClip BGM_2;
    public AudioClip femaleChorus;
    public AudioClip footPrint;
    public AudioClip maleChorus;
    // public AudioClip sickleOnGround;
    // public AudioClip sickleSound;
    [Range(0, 1)] public float volume = 0.5f;
    
    [Header("时间设置")]
    public float stage1Duration = 60f;  // 第一段音频播放总时间
    private float timer = 0f;
    
    private AudioSource audioSource;
    private AudioSource stepSource;
    private AudioSource chorusSouece;
    private bool isStage1 = true;
    private Coroutine switchCoroutine;

    void Start()
    {
        // 初始化AudioSource
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = BGM_1;
        audioSource.volume = volume;
        audioSource.loop = true;  // 设置循环

        stepSource = gameObject.AddComponent<AudioSource>();
        stepSource.clip = footPrint;
        stepSource.volume = 0;
        stepSource.loop = true;
        
        // 开始播放第一阶段音频
        audioSource.Play();
        
        // 开始计时
        StartCoroutine(TimerCoroutine());
    }

    private void Update()
    {
        if (PlayerController.Instance.isWalking)
        {
            stepSource.volume = 0.5f;
        }
        else
        {
            stepSource.volume = 0f;
        }
    }

    IEnumerator TimerCoroutine()
    {
        while (timer < stage1Duration)
        {
            timer += Time.deltaTime;
            
            // 显示剩余时间（调试用）
            // Debug.Log($"第一阶段剩余时间: {stage1Duration - timer:F1}秒");
            
            yield return null;
        }
        
        // 60秒后切换到第二阶段
        SwitchToStage2();
    }

    void SwitchToStage2()
    {
        if (!isStage1) return;  // 防止重复切换
        
        isStage1 = false;
        
        // 切换到第二段音频
        audioSource.clip = BGM_2;
        audioSource.Play();
        
        Debug.Log("已切换到第二阶段音频");
    }

}
