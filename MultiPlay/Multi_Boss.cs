using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Multi_Boss : Multi_LivingEntity
{
    private readonly List<Multi_Enemy> summoners = new List<Multi_Enemy>();

    public Multi_LivingEntity targetEntity;
    public Transform[] spawnPoint;
    public Transform fireTransform;
    public Multi_Grenade grenade;
    public Multi_Enemy enemyPrefab;
    public Animator animator;
    public AudioClip hitclip;
    public AudioClip deathClip;
    public Multi_Gun gun;

    private AudioSource audioPlayer;
    private Multi_Shooter shooter;
    private NavMeshAgent agent;
    private LineRenderer aimingLineRenderer;
    private readonly List<Multi_LivingEntity> players = new List<Multi_LivingEntity>();

    private bool hasTarget => targetEntity != null && !targetEntity.dead;

    private float SummonerHealthMin = 100f;
    private float SummonerHealthMax = 200f;
    private float SummonerDamageMin = 20f;
    private float SummonerDamageMax = 40f;
    private float SummonerSpeed = 7f;
    // Start is called before the first frame update

    [PunRPC]
    public void setup(float health)
    {
        GameObject[] tempobj = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject obj in tempobj)
        {
            Multi_LivingEntity entity = obj.GetComponent<Multi_LivingEntity>();
            players.Add(entity);
        }
        this.startingHealth = health;
        this.health = health;
        this.targetEntity = players[Random.Range(0, players.Count)];
    }

    private void Start()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        StartCoroutine(ChasePlayer());
        StartCoroutine(Think());
    }

    private void Update()
    {
        if (dead || !PhotonNetwork.IsMasterClient)
        {
            return;
        }
        animator.SetFloat("Speed", agent.desiredVelocity.magnitude);
        //agent.SetDestination(targetEntity.transform.position);
    }

    public void Awake()
    {
        shooter = GetComponent<Multi_Shooter>();
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        aimingLineRenderer = GetComponent<LineRenderer>();
        audioPlayer = GetComponent<AudioSource>();

        aimingLineRenderer.positionCount = 2;
        aimingLineRenderer.enabled = false;

        agent.stoppingDistance = Vector3.Distance(transform.position, transform.position) + 6f;
        agent.speed = 3f;
    }

    [PunRPC]
    public override bool ApplyDamage(float amount, Vector3 hitPoint, Vector3 hitNormal, int damager)
    {
        if(!base.ApplyDamage(amount, hitPoint, hitNormal, damager)) return false;
        EffectManager.Instance.PlayHitEffect(hitPoint, hitNormal, transform, EffectManager.EffectType.Flesh);
        audioPlayer.PlayOneShot(hitclip);

        return true;
    }

    private IEnumerator ChasePlayer()
    {
        while (!dead)
        {
            if (hasTarget)
            {
                agent.speed = 5f;
                agent.SetDestination(targetEntity.transform.position);
            }
            else
            {
                if (targetEntity != null)//대상이 사망
                {
                    targetEntity = players[Random.Range(0, players.Count)];//대상 변경
                }
            }
            yield return new WaitForSeconds(0.05f);
        }
    }

    private IEnumerator Think()
    {
        while (!dead)
        {
            yield return new WaitForSeconds(5f);

            int randAction = Random.Range(0, 5);
            switch (randAction)
            {
                case 0:
                    StartCoroutine(Summon(Random.Range(0f, 1f)));
                    break;
                case 1:
                case 2:
                case 3:
                    StartCoroutine(Snipe());
                    break;
                case 4:
                    StartCoroutine(Throw());
                    break;
            }
            targetEntity = players[Random.Range(0, players.Count)];//패턴 한번 실행 뒤 대상 변경
        }
    }

    private IEnumerator Summon(float intensity)
    {
        var health = Mathf.Lerp(SummonerHealthMin, SummonerHealthMax, intensity);//중간 값 사용
        var damage = Mathf.Lerp(SummonerDamageMin, SummonerDamageMax, intensity);

        for (int i = 0; i < 2; i++)
        {
            var CreatedEnemy = PhotonNetwork.Instantiate(enemyPrefab.gameObject.name, spawnPoint[i].position, spawnPoint[i].rotation);
            Multi_Enemy enemy = CreatedEnemy.GetComponent<Multi_Enemy>();
            enemy.photonView.RPC("Setup", RpcTarget.All, health, damage, SummonerSpeed, null, this.targetEntity);
            enemy.targetEntity = players[Random.Range(0,players.Count)];
            summoners.Add(enemy);

            enemy.OnDeath += () => summoners.Remove(enemy);
            enemy.OnDeath += () => StartCoroutine(DestroyAfter(enemy.gameObject, 10f));
            enemy.OnDeath += () => Multi_GameManager.Instance.AddScore(100);
        }

        yield return new WaitForSeconds(5f);
        StartCoroutine(Think());
    }

    private IEnumerator Throw()
    {
        for (int i = 0; i < 3; i++)
        {
            var spawnPosition = Utility.GetRandomPointOnNavMesh(targetEntity.transform.position, 4f, NavMesh.AllAreas);
            var createdGrenade = PhotonNetwork.Instantiate(grenade.gameObject.name, spawnPosition, targetEntity.transform.rotation);
            Multi_Grenade Mgrenade = createdGrenade.GetComponent<Multi_Grenade>();
            Mgrenade.photonView.RPC("SetUp", RpcTarget.All, shooter.photonView.ViewID, 100);
        }

        yield return new WaitForSeconds(5f);
        StartCoroutine(Think());
    }

    private IEnumerator Snipe()
    {
        agent.isStopped = true;

        photonView.RPC("SnipeEffectProcessOnClients", RpcTarget.All);
        if (gun.Fire(targetEntity.transform.position))
        {
            animator.SetTrigger("Shoot");
        }
        agent.isStopped = false;

        yield return new WaitForSeconds(3f);
        StartCoroutine(Think());
    }

    
    public override void Die()
    {
        base.Die();
        animator.applyRootMotion = true;
        animator.SetTrigger("Die");

        audioPlayer.PlayOneShot(deathClip);
    }
    
    [PunRPC]
    private void SnipeEffectProcessOnClients()
    {
        StartCoroutine(SnipeEffect());
    }

    private IEnumerator SnipeEffect()
    {
        aimingLineRenderer.enabled = true;
        for (int i = 0; i < 50; i++)
        {
            aimingLineRenderer.SetPosition(0, fireTransform.position);
            aimingLineRenderer.SetPosition(1, targetEntity.transform.position);
            yield return new WaitForSeconds(0.05f);
        }
        aimingLineRenderer.enabled = false;
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
