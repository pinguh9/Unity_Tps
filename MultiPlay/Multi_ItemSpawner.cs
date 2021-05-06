using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Multi_ItemSpawner : MonoBehaviourPun
{
    public GameObject[] items;//실시간 생성할 아이템들의 원형 프리팹
    
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
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }
        if(Time.time>=lastSpawnTime + timeBetSpawn)
        {
            Spawn();
            lastSpawnTime = Time.time;
            timeBetSpawn = Random.Range(timeBetSpawnMin, timeBetSpawnMax);
        }
    }

    private void Spawn()
    {
        var spawnPosition 
            = Utility.GetRandomPointOnNavMesh(Vector3.zero, maxDistance, NavMesh.AllAreas);
        spawnPosition += Vector3.up * 0.5f;
        var Selecteditem = items[Random.Range(0, items.Length)];
        var item = PhotonNetwork.Instantiate(Selecteditem.name, spawnPosition, Quaternion.identity);
        StartCoroutine(DestroyAfter(item, 5f));
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