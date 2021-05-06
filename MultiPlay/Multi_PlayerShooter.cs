using Photon.Pun;
using UnityEngine;

public class Multi_PlayerShooter : Multi_Shooter
{
    public enum AimState
    {
        Idle, //기본상태
        HipFire //조준하고 있는 상태 
    }//현재 조준 상태 

    public AimState aimState { get; private set; }
    //조준을 하기 시작하면 해당 방향을 바라봐야 함
    public Multi_Grenade grenadePrefab;

    private Multi_PlayerInput playerInput;
    private Animator playerAnimator;
    private Camera playerCamera;

    private float waitingTimeForReleasingAim = 2.5f;//마지막 발사 입력 시간에서 idle 상태로 되돌아가는데 걸리는 시간
    private float lastFireInputTime;//마지막 발사 시간 기록
    public int Grenadecnt;
    public int maxGrenade = 5;

    private Vector3 aimPoint;//실제로 조준하고 있는 대상이 할당
    private bool linedUp => !(Mathf.Abs(playerCamera.transform.eulerAngles.y - transform.eulerAngles.y) > 1f);
    //플레이어 캐릭터가 바라보는 방향과 카메라가 바라보는 방향 사이에 각도가 너무 벌어지진 않았는가
    private bool hasEnoughDistance => !Physics.Linecast(transform.position + Vector3.up * gun.fireTransform.position.y, gun.fireTransform.position, ~excludeTarget);
    //플레이어 캐릭터가 정면에 총을 발사할 수 있을 만큼의 공간을 가지고 있는가 
    void Awake()
    {
        //게임오브젝트 레이어와 제외 타겟을 합친것과 제외타겟 그 자체가 다르다면
        //(제외타겟에 현재 게임오브젝트의 레이어가 미리 추가되어있지 않음)
        if (excludeTarget != (excludeTarget | (1 << gameObject.layer)))
        {
            excludeTarget |= 1 << gameObject.layer;//제외타겟에 게임오브젝트의 레이어 추가
        }
        //플레이어가 자기 자신을 쏘지 않도록 예외처리.
    }

    private void Start()
    {
        playerCamera = Camera.main;
        playerInput = GetComponent<Multi_PlayerInput>();
        playerAnimator = GetComponent<Animator>();
        Grenadecnt = 3;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        aimState = AimState.Idle;
        gun.gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        aimState = AimState.Idle;
        gun.gameObject.SetActive(false);
        //총을 쏘는 기능이 비활성화되면 gun 게임 오브젝트도 함께 비활성화 
    }

    private void FixedUpdate()
    {
        if (!photonView.IsMine)
        {
            return;
        }
        if (playerInput.fire)
        {
            lastFireInputTime = Time.time;
            Shoot();
        }

    }

    private void Update()
    {
        if (!photonView.IsMine)
        {
            return;
        }
        UpdateAimTarget();

        //카메라 각도에 따른 애니메이션 각도 결정
        //하늘을 볼 때 1.0 땅을 볼 때 0.0
        var angle = playerCamera.transform.eulerAngles.x;
        if (angle > 270f) angle -= 360f;

        angle = angle / -180f + 0.5f;
        playerAnimator.SetFloat("Angle", angle);

        if (!playerInput.fire && playerInput.reload)
        {
            Reload();
        }

        if (!playerInput.fire && Time.time > lastFireInputTime + waitingTimeForReleasingAim)
        {
            aimState = AimState.Idle;
        }//발사를 하지 않고 마지막 발사 시간에서 설정한 시간만큼 흘렀다면 기본상태로 

        if (!playerInput.fire && !playerInput.reload && playerInput.grenade)
        {
            Throw();
        }

        //-90도일때 : 0.5 + 0.5 = 1 90도일 때: 0.5 + 0.5 = 1
        UpdateUI();
    }

    public void Shoot()
    {
        if (aimState == AimState.Idle)
        {
            if (linedUp) aimState = AimState.HipFire;
        }
        else if (aimState == AimState.HipFire)
        {
            if (hasEnoughDistance)
            {
                if (gun.Fire(aimPoint))
                {
                    playerAnimator.SetTrigger("Shoot");//반동에 의한 흔들림 애니메이션
                }
            }
            else
            {
                aimState = AimState.Idle;
            }
        }
    }

    public void Reload()
    {
        if (gun.Reload())
        {
            playerAnimator.SetTrigger("Reload");
        }
    }

    public void Throw()
    {
        if (Grenadecnt == 0) return;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100))
        {
            Vector3 nextVec = hit.point - transform.position;
            nextVec.y = 5;//수류탄이 포물선을 그리도록 함.

            var CreatedGrenade = PhotonNetwork.Instantiate(grenadePrefab.gameObject.name, transform.position, transform.rotation);
            Multi_Grenade grenade = CreatedGrenade.GetComponent<Multi_Grenade>();
            var thrower = photonView.ViewID;

            grenade.photonView.RPC("SetUp", RpcTarget.All, thrower, 100);
            Rigidbody rigidGrenade = grenade.GetComponent<Rigidbody>();
            rigidGrenade.AddForce(nextVec, ForceMode.Impulse);
            rigidGrenade.AddRelativeTorque(Vector3.back * 10, ForceMode.Impulse);//회전

            Grenadecnt--;
        }
    }

    private void UpdateAimTarget()//aimPoint를 플레이어가 조준하고 있는 곳으로 매번 갱신해야함
    {
        RaycastHit hit;
        var ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));//해당 뷰포트를 향해 나아가는 레이캐스트 생성
        if (Physics.Raycast(ray, out hit, gun.fireDistance, ~excludeTarget))
       {
            aimPoint = hit.point;
            if (Physics.Linecast(gun.fireTransform.position, hit.point, out hit, ~excludeTarget))
            {
                aimPoint = hit.point;
            }
        }
        else
        {
            aimPoint = playerCamera.transform.position + playerCamera.transform.forward * gun.fireDistance;
        }
    }

    private void UpdateUI()//남은 탄약과 크로스헤어 갱신
    {
        //UIManager : 싱글톤으로 존재 각종 게임 유아이에 즉시 접근해서 유아이를 갱신하도록 통로를 제공
        if (gun == null || UIManager.Instance == null) return;

        UIManager.Instance.UpdateAmmoText(gun.magAmmo, gun.magCapacity);

        UIManager.Instance.SetActiveCrosshair(hasEnoughDistance);
        UIManager.Instance.UpdateCrossHairPosition(aimPoint);
    }

    private void OnAnimatorIK(int layerIndex)
    {//ik가 갱신될때마다 자동으로 실행
        if (gun == null || gun.state == Multi_Gun.State.Reloading) return;//총이 없거나 재장전 중인 상태에는 왼손 위치를 갱신해주지않음

        playerAnimator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1.0f);
        playerAnimator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1.0f);//왼손의 위치와 회전을 100퍼센트로

        playerAnimator.SetIKPosition(AvatarIKGoal.LeftHand, gun.leftHandMount.position);
        playerAnimator.SetIKRotation(AvatarIKGoal.LeftHand, gun.leftHandMount.rotation);
        //왼손이 항상 왼손손잡이에 위치하게 됨.
    }
}