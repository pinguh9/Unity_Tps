using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


// 적 게임 오브젝트를 주기적으로 생성
public class EnemySpawner : MonoBehaviour
{
    private readonly List<Enemy> enemies = new List<Enemy>();
    //한번 생성되면 덮어쓰기 할 수 없음
    //현재 씬에 존재하는 적들의 리스트

    public float[] damageMin = { 10f, 30f, 10f };
    public float[] damageMax = { 20f, 40f, 20f };
    public float[] enemyHealth = { 100f, 150f, 100f };
    public LivingEntity player;
    public Enemy enemyPrefab;//원본 프리팹
    public Boss bossPrefab;
    public GameObject StartPoint;

    public Transform[] spawnPoints;
    public Transform targetPos;
    public Transform BossSpwanPoint;

    private NpcEntity npcEntity;
    private Boss boss;
    private int wave;//웨이브가 늘어날수록 적의 수가 많아짐
    private int gameNum;
    private float Enemyspeed = 4f;
    private int MaxWave = 3;
    private bool inStartPoint;

    public enum State
    {
        Default,
        BeforeStart,
        Wait,
        Spawning,
    }
    public State state;
    private bool waveCleared => state == State.Wait && enemies.Count <= 0;
    private bool readyToStart => state == State.BeforeStart && inStartPoint;

    private void Start()
    {
        inStartPoint = false;
        state = State.Default;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T)) UIManager.Instance.StartCountDown();
        if (Vector3.Distance(StartPoint.transform.position, player.transform.position) <= 1.3f) inStartPoint = true;
        else inStartPoint = false;

        if (readyToStart)
        {
            state = State.Default;
            InStartPoint();
        }

        if (GameManager.Instance != null && GameManager.Instance.isGameover) return;
        if (gameNum == 2)//무한모드일때
        {
            if (waveCleared)
            {
                if (wave % 4 == 0 && wave != 0)
                {
                    state = State.Spawning;
                    boss = Instantiate(bossPrefab, BossSpwanPoint.position, BossSpwanPoint.rotation);
                    boss.setup(300f, player);
                    boss.OnDeath += () => Destroy(boss.gameObject, 10f);
                    boss.OnDeath += () => GameManager.Instance.AddScore(300);
                    boss.OnDeath += () => state = State.Wait;
                    boss.OnDeath += () => wave++;
                }
                else
                {
                    state = State.Spawning;
                    StartCoroutine(SpawnWave());
                }
            }
        }
        else
        {
            if (waveCleared)
            {
                if (wave < MaxWave)
                {
                    state = State.Spawning;
                    StartCoroutine(SpawnWave());
                }
                else
                {
                    state = State.Spawning;
                    boss = Instantiate(bossPrefab, BossSpwanPoint.position, BossSpwanPoint.rotation);
                    boss.setup(300f, player);
                    boss.OnDeath += () => Destroy(boss.gameObject, 10f);
                    boss.OnDeath += () => GameManager.Instance.AddScore(300);
                }
            }
        }
        UpdateUI();
    }

    private void UpdateUI()
    {
        UIManager.Instance.UpdateWaveText(wave, enemies.Count);
    }

    public void Level_Normal()
    {
        gameNum = 0;
        ToStartPoint();
    }

    public void Level_Hard()
    {
        gameNum = 1;
        ToStartPoint();
    }

    public void Level_INF()
    {
        gameNum = 2;
        UIManager.Instance.SetActiveScoreText(true);
        ToStartPoint();
    }

    private void ToStartPoint()
    {
        string str = "시작지점으로 이동하세요.";
        StartPoint.SetActive(true);
        UIManager.Instance.SetActiveToDoText(true);
        UIManager.Instance.UpdateTodoText(str);
        state = State.BeforeStart;
    }

    private void InStartPoint()
    {
        StartPoint.SetActive(false);
        UIManager.Instance.SetActiveToDoText(false);
        state = State.Wait;
        UIManager.Instance.StartCountDown();
    }

    private IEnumerator SpawnWave()
    {
        yield return new WaitForSeconds(10f);
        wave++;
        var spawnCount = Mathf.RoundToInt(wave * 2f);
        for(var I = 0; I < spawnCount; I++)
        {
            var enemyIntensity = Random.Range(0f, 1f);
            CreateEnemy(enemyIntensity);
            yield return new WaitForSeconds(3f);
        }
        state = State.Wait;
    }

    private void CreateEnemy(float intensity)//적의 강함 정도를 받아옴
    {
        var health = enemyHealth[gameNum];
        var damage = Mathf.Lerp(damageMin[gameNum], damageMax[gameNum], intensity);
        var speed = Enemyspeed;

        var spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        var enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);

        enemy.Setup(health, damage, speed, targetPos, null);

        enemies.Add(enemy);

        enemy.OnDeath += () => enemies.Remove(enemy);
        enemy.OnDeath += () => Destroy(enemy.gameObject, 10f);
        enemy.OnDeath += () => GameManager.Instance.AddScore(100);
    }

}