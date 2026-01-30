// GameManager.cs - 游戏管理器
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("游戏状态")]
    public bool isGameOver = false;
    public GameState currentState = GameState.Playing;
    
    [Header("阵营设置")]
    public Faction playerCurrentFaction = Faction.Neutral;
    // public FactionMaskData maskData;
    
    [Header("UI引用")]
    public Image maskProgressFillA;
    public Image maskProgressFillB;
    public GameObject gameOverPanel;
    public Text gameOverText;
    
    [Header("进度设置")]
    [SerializeField] private float maxMaskProgress = 100f;
    private float maskProgressA = 0f;
    private float maskProgressB = 0f;
    
    private PlayerController player;
    
    public enum GameState
    {
        Playing,
        GameOver,
        Paused
    }
    
    public enum Faction
    {
        Neutral,
        FactionA,
        FactionB
    }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        player = FindObjectOfType<PlayerController>();
        UpdateUI();
    }
    
    // 增加阵营进度
    public void AddFactionProgress(Faction faction, float amount)
    {
        if (isGameOver) return;
        
        if (faction == Faction.FactionA)
        {
            maskProgressA += amount;
            if (maskProgressA >= maxMaskProgress)
            {
                GameOver($"被{Faction.FactionA}完全同化！");
            }
        }
        else if (faction == Faction.FactionB)
        {
            maskProgressB += amount;
            if (maskProgressB >= maxMaskProgress)
            {
                GameOver($"被{Faction.FactionB}完全同化！");
            }
        }
        
        UpdateUI();
    }
    
    // 被追捕者抓住
    public void CaughtByHunter(Faction hunterFaction)
    {
        if (playerCurrentFaction != hunterFaction)
        {
            GameOver($"被{hunterFaction}的追捕者抓住！");
        }
    }
    
    private void GameOver(string reason)
    {
        isGameOver = true;
        currentState = GameState.GameOver;
        gameOverText.text = reason;
        gameOverPanel.SetActive(true);
        
        // 停止玩家输入
        if (player != null)
            player.canMove = false;
    }
    
    private void UpdateUI()
    {
        maskProgressFillA.fillAmount = maskProgressA / maxMaskProgress;
        maskProgressFillB.fillAmount = maskProgressB / maxMaskProgress;
    }
    
    // 重启游戏
    public void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }
}