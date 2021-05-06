using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
public class Multi_GameManager : MonoBehaviourPunCallbacks, IPunObservable 
{
    private static Multi_GameManager instance;
    public static Multi_GameManager Instance
    {
        get
        {
            if (instance == null) instance = FindObjectOfType<Multi_GameManager>();
            
            return instance;
        }
    }

    private int score;
    private int money;
    public string[] username;

    public GameObject playerPrefab;

    public bool isGameover { get; private set; }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(score);
            stream.SendNext(money);
        }
        else
        {
            score = (int)stream.ReceiveNext();
            money = (int)stream.ReceiveNext();
            UIManager.Instance.UpdateScoreText(score);
            UIManager.Instance.UpdateMoneyText(money);
        }
    }

    private void Awake()
    {
        if (Instance != this) Destroy(gameObject);
        //UIManager.Instance.UpdateBestScoreText(DataManager.Instance.playerData.BestScore);
    }

    private void Start()
    {
        Vector3 randomSpwanPos = Random.insideUnitSphere * 5f;
        randomSpwanPos.y = 0f;

        PhotonNetwork.Instantiate(playerPrefab.name, randomSpwanPos, Quaternion.identity);
        PhotonNetwork.NickName = DataManager.Instance.playerData.playername;
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

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Escape))
        //{
        //    PhotonNetwork.LeaveRoom();
        //}
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        SceneManager.LoadScene("Lobby");
    }
}