using System;
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

    [Header("脚步声设置")]
    [SerializeField] private float footstepVolume = 0.5f;
    [SerializeField] private float footstepInterval = 0.3f; // 脚步声音播放间隔
    
    private AudioSource audioSource;
    private AudioSource stepSource;
    private AudioSource chorusSource;
    private bool isStage1 = true;
    private Coroutine chorusCoroutine;
    private Coroutine footstepCoroutine;
    private bool wasWalking = false; // 记录上一帧是否在行走
    private GameManager.Faction lastFaction;

    void Start()
    {
        // 初始化AudioSource
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = BGM_1;
        audioSource.volume = volume;
        audioSource.loop = true;  // 设置循环

        stepSource = gameObject.AddComponent<AudioSource>();
        stepSource.clip = footPrint;
        stepSource.volume = 0.5f;
        stepSource.loop = true;

        chorusSource = gameObject.AddComponent<AudioSource>();
        chorusSource.volume = 1f;
        chorusSource.loop = false;

        lastFaction = GameManager.Instance.playerCurrentFaction;
        
        // 开始播放第一阶段音频
        audioSource.Play();
        
        // 开始计时
        StartCoroutine(TimerCoroutine());
    }

    private void Update()
    {
        // 确保PlayerController实例存在
        if (PlayerController.Instance == null)
        {
            stepSource.volume = 0;
            return;
        }
        
        bool isWalkingNow = PlayerController.Instance.isWalking;
        GameManager.Faction playerCurrentFaction = GameManager.Instance.playerCurrentFaction;

        
        // 状态改变时处理
        if (isWalkingNow != wasWalking)
        {
            if (isWalkingNow)
            {
                // 开始播放脚步声
                if (footstepCoroutine != null)
                {
                    StopCoroutine(footstepCoroutine);
                }
                footstepCoroutine = StartCoroutine(PlayFootstepSounds());
            }
            else
            {
                // 停止播放脚步声
                if (footstepCoroutine != null)
                {
                    StopCoroutine(footstepCoroutine);
                    footstepCoroutine = null;
                }
                stepSource.volume = 0;
            }
            
            wasWalking = isWalkingNow;
        }

        if (playerCurrentFaction != lastFaction)
        {
            if (playerCurrentFaction == GameManager.Faction.FactionA)
            {
                if (chorusCoroutine != null)
                {
                    StopCoroutine(chorusCoroutine);
                }
                chorusCoroutine = StartCoroutine(PlayChorusSounds("female"));
            }
            else if (playerCurrentFaction == GameManager.Faction.FactionB)
            {
                if (chorusCoroutine != null)
                {
                    StopCoroutine(chorusCoroutine);
                }
                chorusCoroutine = StartCoroutine(PlayChorusSounds("male"));
            }
            else
            {
                float volumeTimer = 0f;
                float volumeDuration = 0.8f;
                while(volumeTimer < volumeDuration)
                {
                    volumeTimer += Time.deltaTime;
                    float t = Mathf.Clamp01(volumeTimer / volumeDuration);
                    chorusSource.volume = Mathf.Lerp(1f, 0, t);
                }
                StopCoroutine(chorusCoroutine);
            }

            lastFaction = playerCurrentFaction;
        }
    }

    // 播放脚步声的协程
    IEnumerator PlayFootstepSounds()
    {
        while (true)
        {
            // 播放脚步声
            stepSource.Play();
            stepSource.volume = footstepVolume;
            
            // 等待间隔时间
            yield return new WaitForSeconds(footstepInterval);  
        }
    }

    IEnumerator PlayChorusSounds(string chorusType)
    {
        if (chorusType == "male") chorusSource.clip = maleChorus;
        else if (chorusType == "female") chorusSource.clip = femaleChorus;

        chorusSource.volume = 1f;

        while (true)
        {
            chorusSource.Play();
             
            yield return new WaitForSeconds(15f);
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
