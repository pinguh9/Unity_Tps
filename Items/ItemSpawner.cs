using UnityEngine;
using UnityEngine.AI;

public class ItemSpawner : MonoBehaviour
{
    public GameObject[] items;//실시간 생성할 아이템들의 원형 프리팹
    public Transform playerTransform;//플레이어를 중심으로 아이템이 배치될 최대 반경
    
    private float lastSpawnTime;
    public float maxDistance = 5f;
    
    private float timeBetSpawn;//다음 생성 시간까지 소요될 시간 간격

    public float timeBetSpawnMax = 7f;
    public float timeBetSpawnMin = 2f;

    private void Start()
    {
        timeBetSpawn = Random.Range(timeBetSpawnMin, timeBetSpawnMax);
        lastSpawnTime = 0f;
    }

    private void Update()
    {
        if(Time.time>=lastSpawnTime + timeBetSpawn && playerTransform != null)
        {
            Spawn();
            lastSpawnTime = Time.time;
            timeBetSpawn = Random.Range(timeBetSpawnMin, timeBetSpawnMax);
        }
    }

    private void Spawn()
    {
        var spawnPosition 
            = Utility.GetRandomPointOnNavMesh(playerTransform.position, maxDistance, NavMesh.AllAreas);
        spawnPosition += Vector3.up * 0.5f;
        var item = Instantiate(items[Random.Range(0, items.Length)], spawnPosition, Quaternion.identity);
        Destroy(item, 5f);//5초동안 먹지 않으면 자동으로 파괴됨
    }
}