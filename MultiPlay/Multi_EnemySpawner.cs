using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 적 게임 오브젝트를 주기적으로 생성
public class Multi_EnemySpawner : MonoBehaviourPun, IPunObservable
 {
    //한번 생성되면 덮어쓰기 할 수 없음
    //현재 씬에 존재하는 적들의 리스트
    private readonly List<Multi_Enemy> enemies = new List<Multi_Enemy>();

    public float[] damageMin = { 10f, 30f ,10f};
    public float[] damageMax = { 20f, 40f , 20f};
    public float[] enemyHealth = { 100f, 150f, 100f };
    public Multi_Enemy enemyPrefab;//원본 프리팹
    public Multi_Boss bossPrefab;

    public Transform[] spawnPoints;
    public Transform BossSpwanPoint;

    private int wave;//웨이브가 늘어날수록 적의 수가 많아짐
    private int gameNum;
    private float Enemyspeed = 4f;
    private int MaxWave = 3;
    private int enemyCount = 0;
    private enum State
    {
        Default,
        Wait,
        Spawning,
    }
    private State state;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(enemies.Count);
            stream.SendNext(wave);
        }
        else
        {
            enemyCount = (int)stream.ReceiveNext();
            wave = (int)stream.ReceiveNext();
        }
    }

    private bool waveCleared => state == State.Wait && enemies.Count <= 0;

    private void Start()
    {
        state = State.Default;
    }

    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (GameManager.Instance != null && GameManager.Instance.isGameover) return;
        if (gameNum == 2)//무한모드일때
        {
            if (waveCleared)
            {
                if (wave % 4 == 0 && wave != 0)
                {
                    CreateBoss();
                }
                else
                {
                    state = State.Spawning;
                    Invoke("SpawnWave", 10f);
                }
            }
        }
        else//나머지 모드
        {
            if (waveCleared)
            {
                if (wave < MaxWave)
                {
                    state = State.Spawning;
                    Invoke("SpawnWave", 10f);
                }
                else
                {
                    CreateBoss();
                }
            }
        }
        UpdateUI();
    }

    private void UpdateUI()
    {
        UIManager.Instance.UpdateWaveText(wave, enemies.Count);
    }

    [PunRPC]
    private void CountDownProcessOnClients()
    {
        StartCoroutine(CountDown());
    }

    private IEnumerator CountDown()
    {
        UIManager.Instance.SetActivecountDownText(true);
        for (int i = 10; i > 0; i--)
        {
            UIManager.Instance.UpdateCountDownText(i);
            yield return new WaitForSeconds(1f);
        }
        UIManager.Instance.SetActivecountDownText(false);
    }

    public void Level_Normal()
    {
        gameNum = 0;
        state = State.Wait;
        photonView.RPC("CountDownProcessOnClients", RpcTarget.All);
    }

    public void Level_Hard()
    {
        gameNum = 1;
        state = State.Wait;
        photonView.RPC("CountDownProcessOnClients", RpcTarget.All);
    }

    public void Level_INF()
    {
        gameNum = 2;
        state = State.Wait;
        UIManager.Instance.SetActiveScoreText(true);
        photonView.RPC("CountDownProcessOnClients", RpcTarget.All);
    }

    private void SpawnWave()
    {
        wave++;
        var spawnCount = Mathf.RoundToInt(wave * 2f);
        for (var i = 0; i < spawnCount; i++)
        {
            var enemyIntensity = Random.Range(0f, 1f);
            CreateEnemy(enemyIntensity);
        }
        state = State.Wait;
    }

    private void CreateEnemy(float intensity)//적의 강함 정도를 받아옴
    {
        var health = enemyHealth[gameNum];
        var damage = Mathf.Lerp(damageMin[gameNum], damageMax[gameNum], intensity);
        var speed = Enemyspeed;

        var spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        var CreatedEnemy = PhotonNetwork.Instantiate(enemyPrefab.gameObject.name, spawnPoint.position, spawnPoint.rotation);
        Multi_Enemy enemy = CreatedEnemy.GetComponent<Multi_Enemy>();

        enemy.photonView.RPC("Setup", RpcTarget.All, health, damage, speed, null);

        enemies.Add(enemy);

        enemy.OnDeath += () => enemies.Remove(enemy);
        enemy.OnDeath += () => StartCoroutine(DestroyAfter(enemy.gameObject, 10f));
        enemy.OnDeath += () => Multi_GameManager.Instance.AddScore(100);
    }

    private void CreateBoss()
    {
        state = State.Spawning;
        var createdBoss = PhotonNetwork.Instantiate(bossPrefab.gameObject.name,
            BossSpwanPoint.position, BossSpwanPoint.rotation);
        Multi_Boss boss = createdBoss.GetComponent<Multi_Boss>();
        boss.photonView.RPC("setup", RpcTarget.All, 300f);
        boss.OnDeath += () => StartCoroutine(DestroyAfter(boss.gameObject, 10f));
        boss.OnDeath += () => Multi_GameManager.Instance.AddScore(300);
        if(gameNum == 2)
        {
            boss.OnDeath += () => Destroy(boss.gameObject, 10f);
            boss.OnDeath += () => Multi_GameManager.Instance.AddScore(300);
        }
    }

    IEnumerator DestroyAfter(GameObject target, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (target != null)
        {
            PhotonNetwork.Destroy(target);
        }
    }

}