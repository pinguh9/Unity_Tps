using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    //다른 스크립트에서 유아이 요소에 쉽게 접근할 수 있도록 통로를 제공하는 스크립트 
    private static UIManager instance;
    private Animator animator;
    public static UIManager Instance
    {//다른 스크립트에서 필요할 때 즉시 접근할 수 있도록 싱글톤으로 구현
        get
        {
            if (instance == null) instance = FindObjectOfType<UIManager>();

            return instance;
        }
    }

    //private이지만 인스펙터 창에서 할당할 수 있도록 시리얼라이즈
    [SerializeField] private GameObject gameoverUI;
    [SerializeField] private Crosshair crosshair;
    [SerializeField] private Marker marker;
    [SerializeField] private GameObject gameplayUI;
    [SerializeField] private GameObject levelselectUI;
    [SerializeField] private GameObject ShopUI;

    [SerializeField] private Text healthText;
    [SerializeField] private Text lifeText;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text ammoText;
    [SerializeField] private Text waveText;
    [SerializeField] private Text countdownText;
    [SerializeField] private Text BestScoreText;
    [SerializeField] private Text Moneytext;
    [SerializeField] private Text TodoText;
    [SerializeField] private GameObject talkNpcButton;
   
    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void UpdateAmmoText(int magAmmo, int remainAmmo)
    {
        ammoText.text = magAmmo + "/" + remainAmmo;
    }
    //유아이 요소를 직접 수정할 수 있는 퍼블릭 메소드 

    public void UpdateScoreText(int newScore)
    {
        scoreText.text = "Score : " + newScore;
    }
    
    public void UpdateWaveText(int waves, int count)
    {
        waveText.text = "Wave : " + waves + "\nEnemy Left : " + count;
    }

    public void UpdateLifeText(int count)
    {
        lifeText.text = "Life : " + count;
    }

    public void UpdateBestScoreText(int score)
    {
        BestScoreText.text = "최고 점수 : " + score;
    }

    public void UpdateCrossHairPosition(Vector3 worldPosition)
    {
        crosshair.UpdatePosition(worldPosition);
    }
    
    public void UpdateHealthText(float health)
    {
        healthText.text = Mathf.Floor(health).ToString();
    }

    public void UpdateMoneyText(float Money)
    {
        Moneytext.text = "Money : " + Money;
    }

    public void UpdateTodoText(string Text)
    {
        TodoText.text = Text;
    }

    public void UpdateCountDownText(float time)
    {
        countdownText.text = Mathf.Floor(time).ToString();
    }
    
    public void SetActiveCrosshair(bool active)
    {
        crosshair.SetActiveCrosshair(active);
    }

    public void SetActivecountDownText(bool active)
    {
        countdownText.gameObject.SetActive(active);
    }    
    public void SetActiveGameoverUI(bool active)
    {
        gameoverUI.SetActive(active);
    }
    
    public void SetActiveGameplayUI(bool active)
    {
        gameplayUI.SetActive(active);
    }

    public void SetActiveTalkButtonUI(bool active)
    {
        talkNpcButton.SetActive(active);
    }

    public void SetActiveLevelSelectUI(bool active)
    {
        levelselectUI.SetActive(active);
    }

    public void SetActiveShopUI(bool active)
    {
        ShopUI.SetActive(active);
    }

    public void SetActiveScoreText(bool active)
    {
        scoreText.gameObject.SetActive(active);
    }

    public void SetActiveToDoText(bool active)
    {
        TodoText.gameObject.SetActive(active);
    }
    public void GameRestart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void StartCountDown()
    {
        StartCoroutine(CountDown());
    }
    
    public IEnumerator CountDown()
    {
        animator.SetBool("IsCountDown", true);
        for(var i = 10; i > 0; i--)
        {
            Debug.Log("!");
            UpdateCountDownText(i);
            yield return new WaitForSeconds(1f);
        }
        animator.SetBool("IsCountDown", false);
    }
}