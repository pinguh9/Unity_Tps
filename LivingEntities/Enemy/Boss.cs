using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Boss : LivingEntity
{
    private readonly List<Enemy> summoners = new List<Enemy>();

    public LivingEntity targetEntity;
    public Transform[] spawnPoint;
    public Transform fireTransform;
    public Grenade grenade;
    public Enemy enemyPrefab;
    public Animator animator;
    public AudioClip hitclip;
    public AudioClip deathClip;
    public Gun gun;

    private AudioSource audioPlayer;
    private Shooter shooter;
    private NavMeshAgent agent;
    private LineRenderer aimingLineRenderer;

    private bool hasTarget => targetEntity != null && !targetEntity.dead;

    private float SummonerHealthMin = 100f;
    private float SummonerHealthMax = 200f;
    private float SummonerDamageMin = 20f;
    private float SummonerDamageMax = 40f;
    private float SummonerSpeed = 7f;
    // Start is called before the first frame update

    public void setup(float health, LivingEntity targetEntity)
    {
        this.startingHealth = health;
        this.health = health;
        this.targetEntity = targetEntity;
    }

    private void Start()
    {
        StartCoroutine(ChasePlayer());
        StartCoroutine(Think());
    }

    private void Update()
    {
        if (dead)
        {
            return;
        }
        animator.SetFloat("Speed", agent.desiredVelocity.magnitude);
        //agent.SetDestination(targetEntity.transform.position);
    }

    public void Awake()
    {
        shooter = GetComponent<Shooter>();
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        aimingLineRenderer = GetComponent<LineRenderer>();
        audioPlayer = GetComponent<AudioSource>();

        aimingLineRenderer.positionCount = 2;
        aimingLineRenderer.enabled = false;

        agent.stoppingDistance = Vector3.Distance(transform.position, transform.position) + 6f;
        agent.speed = 3f;
    }

    public override bool ApplyDamage(DamageMessage damageMessage)
    {
        if(!base.ApplyDamage(damageMessage)) return false;
        EffectManager.Instance.PlayHitEffect(damageMessage.hitPoint, damageMessage.hitNormal, transform, EffectManager.EffectType.Flesh);
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
                    if (!targetEntity.dead) agent.SetDestination(targetEntity.transform.position);
                }
            }
            yield return new WaitForSeconds(0.05f);
        }
    }

    private IEnumerator Think()
    {
        if (!dead)
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
        }
    }

    private IEnumerator Summon(float intensity)
    {
        Debug.Log("Summon");
        var health = Mathf.Lerp(SummonerHealthMin, SummonerHealthMax, intensity);//중간 값 사용
        var damage = Mathf.Lerp(SummonerDamageMin, SummonerDamageMax, intensity);

        for (int i = 0; i < 2; i++)
        {
            var enemy = Instantiate(enemyPrefab, spawnPoint[i].position, spawnPoint[i].rotation);
            enemy.Setup(health, damage, SummonerSpeed, null, this.targetEntity);
            enemy.targetEntity = this.targetEntity;
            summoners.Add(enemy);

            enemy.OnDeath += () => summoners.Remove(enemy);
            enemy.OnDeath += () => Destroy(enemy.gameObject, 10f);
            enemy.OnDeath += () => GameManager.Instance.AddScore(100);
        }

        yield return new WaitForSeconds(5f);
        StartCoroutine(Think());
    }

    private IEnumerator Throw()
    {
        Debug.Log("Throw");

        var spawnPosition1 = Utility.GetRandomPointOnNavMesh(targetEntity.transform.position, 4f, NavMesh.AllAreas);
        var instantGrenade1 = Instantiate(grenade, spawnPosition1, targetEntity.transform.rotation);
        instantGrenade1.SetUp(shooter, 100, LayerMask.GetMask("Player"));

        var spawnPosition2 = Utility.GetRandomPointOnNavMesh(targetEntity.transform.position, 4f, NavMesh.AllAreas);
        var instantGrenade2 = Instantiate(grenade, spawnPosition2, targetEntity.transform.rotation);
        instantGrenade2.SetUp(shooter, 100, LayerMask.GetMask("Player"));

        var spawnPosition3 = Utility.GetRandomPointOnNavMesh(targetEntity.transform.position, 4f, NavMesh.AllAreas);
        var instantGrenade3 = Instantiate(grenade, spawnPosition3, targetEntity.transform.rotation);
        instantGrenade3.SetUp(shooter, 100, LayerMask.GetMask("Player"));

        yield return new WaitForSeconds(5f);
        StartCoroutine(Think());
    }

    private IEnumerator Snipe()
    {
        Debug.Log("Snipe");
        agent.isStopped = true;

        aimingLineRenderer.enabled = true;
        for (int i = 0; i < 50; i++)
        {
            aimingLineRenderer.SetPosition(0, fireTransform.position);
            aimingLineRenderer.SetPosition(1, targetEntity.transform.position);
            yield return new WaitForSeconds(0.05f);
        }
        aimingLineRenderer.enabled = false;

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
        Destroy(gameObject, 10f);
    }
}
