using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    
    public static GameManager Instance
    {
        get
        {
            if (instance == null) instance = FindObjectOfType<GameManager>();
            
            return instance;
        }
    }

    private int score;
    private int money;
    public bool isGameover { get; private set; }

    private void Awake()
    {
        if (Instance != this) Destroy(gameObject);
        UIManager.Instance.UpdateBestScoreText(DataManager.Instance.playerData.BestScore);
    }
    
    public void AddScore(int newScore)
    {
        if (!isGameover)
        {
            score += newScore;
            UIManager.Instance.UpdateScoreText(score);
        }
    }

    public void AddMoney(int amount)
    {
        if (!isGameover)
        {
            money += amount;
            UIManager.Instance.UpdateMoneyText(money);
        }
    }
 
    public void EndGame()
    {
        isGameover = true;
        UIManager.Instance.SetActiveGameoverUI(true);
        if (score > DataManager.Instance.playerData.BestScore)
        {
            DataManager.Instance.playerData.BestScore = score;
            UIManager.Instance.UpdateBestScoreText(score);
        }
    }
}