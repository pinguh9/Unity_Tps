using System;
using System.Collections;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public enum State
    {
        Ready,
        Empty,
        Reloading
    }//현재 총의 상태

    public State state { get; private set; }//외부에서 총의 상태를 변경하지 못하게 함
    
    private Shooter gunHolder;
    private LineRenderer bulletLineRenderer;//총알의 궤적을 그림
    
    private AudioSource gunAudioPlayer;
    public AudioClip shotClip;//총알 발사 소리
    public AudioClip reloadClip;//재장전 소리 
    
    public ParticleSystem muzzleFlashEffect;
    public ParticleSystem shellEjectEffect;
    
    public Transform fireTransform;//총알이 나가는 발사 위치와 방향
    public Transform leftHandMount;//왼손의 위치를 알려줌 

    public float damage = 25;
    public float fireDistance = 100f;//총알의 발사 체크를 할 거리 100f 범위로 총알 충돌 체크

    public int ammoRemain = 30;//총 남은 탄약수
    public int magAmmo;//탄창에 들어있는 총알
    public int magCapacity = 30;//탄창 용량

    public float timeBetFire = 0.12f;//총알 발사 사이의 간격
    public float reloadTime = 1.8f;//재장전에 걸리는 시간
    
    [Range(0f, 10f)] public float maxSpread = 3f;//탄착군의 최대 범위 
    [Range(1f, 10f)] public float stability = 1f;//반동이 증가하는 속도(안정성)
    [Range(0.01f, 3f)] public float restoreFromRecoilSpeed = 2f;//연사를 중단한 다음 탄퍼짐이 돌아오기까지 걸리는 시간
    private float currentSpread;//현재 탄퍼짐 반경
    private float currentSpreadVelocity;

    private float lastFireTime;//가장 최근에 발사가 이루어진 시점

    private LayerMask excludeTarget;//총알을 쏘면 안되는 대상을 거르기 위함

    private void Awake()
    {
        gunAudioPlayer = GetComponent<AudioSource>();
        bulletLineRenderer = GetComponent<LineRenderer>();

        bulletLineRenderer.positionCount = 2;//사용할 점의 개수 (총구의 위치 탄의 위치)
        bulletLineRenderer.enabled = false;

    }

    public void Setup(Shooter gunHolder)//총의 초기화 실행 총을 쥐고 있는 사람이 누구인지
    {
        this.gunHolder = gunHolder;
        excludeTarget = gunHolder.excludeTarget;//총을 쏘지 않기로 한 레이어를 가져와서 저장
    }

    private void OnEnable()
    {
        magAmmo = magCapacity;
        currentSpread = 0f;
        lastFireTime = 0f;
        state = State.Ready;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    public bool Fire(Vector3 aimTarget)//shot 메소드를 안전하게 감쌈, 발사 성공/실패 리턴
    {
        if(state == State.Ready && Time.time >= lastFireTime + timeBetFire)//마지막 발사시간에서 발사 간격만큼의 시간이 흘렀으면
        {
            var fireDirection = aimTarget - fireTransform.position;//벡터 연산에서 목표지점 - 시작지점 : 시작지점에서 목표지점으로 향하는 방향과 거리

            var xError = Utility.GedRandomNormalDistribution(0f,currentSpread);
            var yError = Utility.GedRandomNormalDistribution(0f, currentSpread);

            fireDirection = Quaternion.AngleAxis(yError, Vector3.up) * fireDirection;
            fireDirection = Quaternion.AngleAxis(xError, Vector3.right) * fireDirection;
            //yError만큼 조금 더 회전한 방향
            
            currentSpread += 1f / stability;
            //다음번 회차에 반동이 증가하고 정확도가 내려가도록함
            //안정성이 높을수록 반동이 줄어듦.
            
            lastFireTime = Time.time;//마지막 총알을 발사한 시점 갱신
            Shot(fireTransform.position, fireDirection);
            
            return true;//발사 성공
        }
        return false;
    }
    
    private void Shot(Vector3 startPoint, Vector3 direction)
    {
        RaycastHit hit;//충돌정보 저장
        Vector3 hitPosition;

        if(Physics.Raycast(startPoint,direction, out hit, fireDistance, ~excludeTarget))
        {
            //excludeTarget에 있는 대상을 제외하고 실행하도록 ~연산자 붙여줌
            var target = hit.collider.GetComponent<IDamageable>();//대미지를 받을 수 있는 타입
            if (target != null)
            {
                DamageMessage damageMessage;
                
                damageMessage.damager = gunHolder.gameObject;
                damageMessage.amount = damage;
                damageMessage.hitPoint = hit.point;
                damageMessage.hitNormal = hit.normal;

                target.ApplyDamage(damageMessage);//대미지를 입는 타입이라면 ApplyDamage안에 이펙트를 재생하는 기능을 가지고있음
            }
            else
            {//대미지를 입는 타입이 아니라면 직접 이펙트를 재생해줌
                EffectManager.Instance.PlayHitEffect(hit.point, hit.normal,hit.transform);
                //이펙트매니저 싱글톤으로 접근, 레이캐스트를 통해 얻어온 hit 정보
            }
            hitPosition = hit.point;
        }
        else//충돌하지 않았다면
        {
            hitPosition = startPoint + direction * fireDistance;
            //최대 사정거리까지 이동한 거리
        }
        StartCoroutine(ShotEffect(hitPosition));
        magAmmo--;
        if (magAmmo <= 0) state = State.Empty;
    }

    private IEnumerator ShotEffect(Vector3 hitPosition)//시간을 들여서 이뤄지기 때문에 코루틴 사용
    {
        muzzleFlashEffect.Play();
        shellEjectEffect.Play();

        gunAudioPlayer.PlayOneShot(shotClip);//해당 오디오 클립을 즉시 실행, 소리의 중첩이 가능
        bulletLineRenderer.enabled = true;
        bulletLineRenderer.SetPosition(0, fireTransform.position);
        bulletLineRenderer.SetPosition(1, hitPosition);
        
        yield return new WaitForSeconds(0.03f);

        bulletLineRenderer.enabled = false;//대기시간으로 인해 총알이 번쩍하는 효과
    }
    
    public bool Reload()//리로드루틴을 안전하게 감싸는 역할
    {
        if (state == State.Reloading || magAmmo >= magCapacity)
        {//이미 재장전 중, 탄약이 없음, 이미 탄창이 가득 차 있음.
            return false;
        }

        StartCoroutine(ReloadRoutine());

        return true;
    }

    private IEnumerator ReloadRoutine()
    {
        state = State.Reloading;
        gunAudioPlayer.PlayOneShot(reloadClip);

        yield return new WaitForSeconds(reloadTime);

        var ammoToFill = Mathf.Clamp(magCapacity - magAmmo, 0, 30);
        magAmmo += ammoToFill;

        state = State.Ready;
    }

    private void Update()//총알 반동값을 상태에 따라 갱신
    {
        currentSpread = Mathf.Clamp(currentSpread, 0f, maxSpread);
        //아무리 반동이 누적이 되어도 maxSpread이상으로 탄퍼짐이 심해지지 않음.
        currentSpread
            = Mathf.SmoothDamp(currentSpread, 0f, ref currentSpreadVelocity, 1f / restoreFromRecoilSpeed);
        //매 프레임마다 부드럽게 0에 가까워지게 만들어줌
        //restoreFromRecoil 값이 커질수록 지연시간이 줄어들어 빠르게 탄 퍼짐 값이 0에 도달
    }
    
        
}