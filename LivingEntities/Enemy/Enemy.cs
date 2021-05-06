using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

#if UNITY_EDITOR //유니티 에디터 내에서만 동작함. 빌드에서는 동작하지 않음
    using UnityEditor;
#endif

public class Enemy : LivingEntity
{
    protected enum State
    {
        Patrol,
        Tracking,
        AttackBegin,
        Attacking,
    }
    
    protected State state;
    
    public NavMeshAgent agent;
    public Animator animator;
    
    public Transform targetPos;
    public Transform attackRoot;//공격을 하는 피봇 포인트. 반지름을 지정해서 해당 반경 내에 있는 물체나 플레이어 공격
    public Transform eyeTransform;//일정 범위 내에서 공격할 물체 감지에 사용할 시야의 기준점
    
    private AudioSource audioPlayer;
    public AudioClip hitClip;
    public AudioClip deathClip;
    
    private Renderer skinRenderer;

    public float speed = 5f;//이동속도 
    [Range(0.01f, 2f)] public float turnSmoothTime = 0.1f;//방향회전에 들어가는 지연시간 
    public float turnSmoothVelocity;
    
    public float damage = 30f;
    public float attackRadius = 2f;//공격 반경
    private float attackDistance;//공격을 시도하는 거리 
    
    public float fieldOfView = 50f;//시야각
    public float viewDistance = 10f;//거리 시야각이 넓어도 거리가 짧으면 플레이어를 감지할 수 없음

    public float TimeBetSetDest = 5f;
    public float LastSearchTime;

    public LivingEntity targetEntity;//적이 추적할 대상 유니티 인스펙터 창에서 보이지 않음
    public LayerMask whatIsTarget;//할당된 레이어에서만 검사해서 성능을 아낌


    public RaycastHit[] hits = new RaycastHit[10];//적의 공격 범위 기반으로 구현 > 여러개의 충돌 포인트가 생김  
    //공격을 새로 시작할때마다 초기화 공격이 두번 이상 같은 대상에 적용되지 않도록 함 
    
    private bool hasTarget => targetEntity != null && !targetEntity.dead;
    //추적할 대상이 존재하는가 

#if UNITY_EDITOR

    private void OnDrawGizmosSelected()
    {
        if (attackRoot != null)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
            Gizmos.DrawSphere(attackRoot.position, attackRadius);
        }
        if (eyeTransform != null)
        {
            var leftEyeRoation = Quaternion.AngleAxis(-fieldOfView * 0.5f, Vector3.up);//적의 시야 표현
            var leftRayDirection = leftEyeRoation * transform.forward;
            Handles.color = new Color(1f, 1f, 1f, 0.2f);
            Handles.DrawSolidArc(eyeTransform.position, Vector3.up, leftRayDirection, fieldOfView, viewDistance);
        }
    }
    
#endif

    public virtual void Awake()
    {
        LastSearchTime = 0f;
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        audioPlayer = GetComponent<AudioSource>();
        skinRenderer = GetComponentInChildren<Renderer>();//좀비게임오브젝트의 자식인 모델에 스킨렌더러가 붙어있음

        var attackPivot = attackRoot.position;
        attackPivot.y = transform.position.y; //수평방향의 거리만 고려 
        attackDistance = Vector3.Distance(transform.position, attackPivot) + attackRadius;

        agent.stoppingDistance = attackDistance;
        agent.speed = speed;
    }

    public void Setup(float health, float damage,
        float speed, Transform targetPos, LivingEntity targetentitiy)//적이 생성될 때 적의 스펙 결정.
    {
        this.startingHealth = health;
        this.health = health;
        
        this.damage = damage;
        this.speed = speed;
        this.targetPos = targetPos;
        this.targetEntity = targetentitiy;

    }

    public virtual void Start()
    {
        StartCoroutine(UpdatePath());
    }

    private void Update()
    {
        if (dead)
        {
            return;//업데이트 메소드 종료
        }
        if(state == State.Tracking)
        {
            var distance = Vector3.Distance(targetEntity.transform.position, transform.position);
            if(distance <= attackDistance)
            {//추적대상과 나 사이의 거리가 공격거리보다 짧다면
                BeginAttack();//공격 실행
            }
        }
        animator.SetFloat("Speed", agent.desiredVelocity.magnitude);
        //desireVelocity : 현재 속도로 설정하고 싶은 값. 실제속도는 아님(중간에 장애물에 부딪힌 경우)
    }

    private IEnumerator UpdatePath()
    {
        while (!dead)
        {
            if (hasTarget)
            {
                if(state == State.Patrol)
                {
                    state = State.Tracking;
                }
                agent.SetDestination(targetEntity.transform.position);//목적지 설정 
            }
            else
            {
                if(targetEntity == null)//아무 대상도 못찾음 (처음 시작)
                {
                    agent.SetDestination(targetPos.transform.position);
                }
                else 
                {
                    targetEntity = null;
                    agent.SetDestination(targetPos.transform.position);
                }
                //targetEntity가 false이지만 null은 아닌경우 : 타겟이 사망함
                //targetEntity에 null을 할당해 후에 추적 대상을 추가할 경우 targetEntitiy가 아니라 다른 대상을 추적하도록 함
                if(state != State.Patrol)
                {
                    state = State.Patrol;//추적할 대상이 없으므로 정찰 상태로 변경
                }
           
                var colliders = Physics.OverlapSphere(eyeTransform.position, viewDistance, whatIsTarget);
                //시야에 있는 모든 콜라이더를 가져와서 필터링으로 whatIsTarget레이어에 포함되어 있는것만 검사
                foreach(var collider in colliders)
                {
                    if (!IsTargetOnSight(collider.transform))//시야 내에서 보이는 타겟인지
                    {
                        continue;
                    }
                    var livingEntity = collider.GetComponent<LivingEntity>();
                    if(livingEntity != null && !livingEntity.dead)
                    {
                        targetEntity = livingEntity;
                        break;
                    }
                }
            }
            yield return new WaitForSeconds(0.05f);
        }
    }
    
    public override bool ApplyDamage(DamageMessage damageMessage)
    {
        if (!base.ApplyDamage(damageMessage)) return false;//공격을 받을 수 없는 상태

        if(targetEntity == null)//추적 대상을 못찾았는데 공격을 받음
        {
            targetEntity = damageMessage.damager.GetComponent<LivingEntity>();
        }
        EffectManager.Instance.PlayHitEffect(damageMessage.hitPoint, damageMessage.hitNormal, transform,
            EffectManager.EffectType.Flesh);

        audioPlayer.PlayOneShot(hitClip);

        return true;
    }

    public virtual void BeginAttack()
    {
        agent.isStopped = true;//추적을 잠시 중단함
    }

    public void DisableAttack()
    {//공격이 끝나는 시점
        if (hasTarget)
        {
            state = State.Tracking;
        }
        else//대상이 죽었을 때
        {
            state = State.Patrol;
        }
        agent.isStopped = false;
    }
    //위의 두 함수는 좀비 애니메이션의 이벤트를 통해서 실행됨.

    private bool IsTargetOnSight(Transform target)
    {
        var direction = target.position - eyeTransform.position;
        direction.y = eyeTransform.forward.y;//수평방향에 대해서만 검사 

        if (Vector3.Angle(direction, eyeTransform.forward) > fieldOfView * 0.5f)//두 방향 벡터 사이의 각도 계산
        {//눈에서 목표까지의 방향과 눈의 앞쪽 방향이 fieldofView의 절반보다 크다면 시야에서 벗어나게 됨
            return false;
        }

        direction = target.position - eyeTransform.position;//장애물을 검사할때는 direction을 원래대로 

        RaycastHit hit;

        if (Physics.Raycast(eyeTransform.position, direction, out hit, viewDistance, whatIsTarget))
        {//레이캐스트를 실행했을때 광선에 닿은 무언가가 있음
            if(hit.transform == target)
            {//상대방이 보임
                return true;
            }
        }
        
        return false;
    }
    
    public override void Die()
    {
        base.Die();//LivingEntity에서의 Die먼저 실행
        GetComponent<Collider>().enabled = false;//죽었을때 부딪히지 않도록 collider 비활성화

        agent.enabled = false;//agent끼리는 서로 피하면서 이동하므로 사망했을 경우엔 완전히 비활성화 
        animator.applyRootMotion = true;
        animator.SetTrigger("Die");

        audioPlayer.PlayOneShot(deathClip);
    }
}