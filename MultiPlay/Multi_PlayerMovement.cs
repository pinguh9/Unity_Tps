using Photon.Pun;
using UnityEngine;

public class Multi_PlayerMovement : MonoBehaviourPun
{
    private CharacterController characterController;
    private Multi_PlayerInput playerInput;//입력값
    private Multi_PlayerShooter playerShooter;
    private Animator animator;//애니메이션 통제
    
    private Camera followCam;//카메라의 방향을 기준으로 움직임 
    
    public float speed = 6f;//초당 6의 속도 
    public float jumpVelocity = 20f;//점프 속도 
    [Range(0.01f, 1f)] public float airControlPercent;//공중에 체류하는 동안 플레이어가 원래
    //속도의 몇 퍼센트 정도를 통제할 수 있는가 0퍼센트일 경우 공중에서 속도 조작 불가 

    public float speedSmoothTime = 0.1f;
    public float turnSmoothTime = 0.1f;
    //스무딩 지연시간 움직임의 변화와 회전의 변화를 부드럽게, 댐핑에 사용

    private float speedSmoothVelocity;
    private float turnSmoothVelocity;
    
    private float currentVelocityY;
    //캐릭터 컨트롤러는 자동으로 중력영향을 받지 않음 따라서 설정해줘야함 

    public float currentSpeed =>//수직방향을 제외한 속도 (지면상에서의 속도)
        new Vector2(characterController.velocity.x, characterController.velocity.z).magnitude;
    
    private void Start()
    {
        playerInput = GetComponent<Multi_PlayerInput>();
        playerShooter = GetComponent<Multi_PlayerShooter>();
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        followCam = Camera.main;
    }

    private void FixedUpdate()
    {
        if (!photonView.IsMine)
        {
            return;
        }
        if (currentSpeed > 0.2f || playerInput.fire || playerShooter.aimState == Multi_PlayerShooter.AimState.HipFire) Rotate();
        //견착상태인 동안에는 카메라를 회전함
        //0.2: 플레이어 캐릭터가 조금이라도 움직이고 있음
        //숫자를 늘릴 경우 캐릭터가 뛰는 순간부터 카메라와 플레이어의 방향이 일치하도록 
        //처리할수도 있음

        Move(playerInput.moveInput);
        
        
    }
    //fixedupdate : 물리 갱신 주기에 맞춰 실행 이동과 관련된 코드는 더 정확하게 
    private void Update()
    {
        if (!photonView.IsMine)
        {
            return;
        }
        if (playerInput.jump) Jump();
        
        UpdateAnimation(playerInput.moveInput);//어떠한 움직임 입력을 넣었냐  
    }
    //물리적으로 정확한 수치를 요구하는 코드를 넣었을 시 오차 발생 가능 
    public void Move(Vector2 moveInput)
    {
        var targetSpeed = speed * moveInput.magnitude;
        //게임패드일 경우 1보다 작은 값이 들어올수 있음 
        var moveDirection =
            Vector3.Normalize(transform.forward * moveInput.y + transform.right * moveInput.x);
        var smoothTime = characterController.isGrounded ? speedSmoothTime : speedSmoothTime /
            airControlPercent;
        //공중에 떠있는 동안에는 지연시간이 길어져 조작이 잘 먹지 않음

        targetSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedSmoothVelocity, smoothTime);

        currentVelocityY += Time.deltaTime * Physics.gravity.y;
        //가속도 = 시간당 속도가 늘어나는 정도 

        var velocity = moveDirection * targetSpeed + Vector3.up * currentVelocityY;
        //앞,뒤,좌우 속도를 먼저 계산한 다음 위의 속도를 따로 계산하고 마지막에 합침
        characterController.Move(velocity * Time.deltaTime);

        if (characterController.isGrounded) currentVelocityY = 0f;
        //아래로 떨어지는 속도값이 계속해서 커질 경우 바닥이 존재해도 뚫고 아래로 떨어짐
    }

    public void Rotate()
    {
        //x나 z방향의 회전값은 사용하지 않음.
        var targetRotation = followCam.transform.eulerAngles.y;

        targetRotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref
            turnSmoothVelocity, turnSmoothTime);
        //smoothDampAngle : 같은 각도가 의도치 않게 더 많이 회전되는 경우를 막아줌
        
        transform.eulerAngles = Vector3.up * targetRotation;//up : y에 대해서만 적용하는 것이 됨
    }

    public void Jump()
    {
        if (!characterController.isGrounded) return;
        currentVelocityY = jumpVelocity;
    }

    private void UpdateAnimation(Vector2 moveInput)
    {
        var animationSpeedPercent = currentSpeed / speed;//현재 속도가 최고 속도 대비 몇인가
        animator.SetFloat("Vertical Move", moveInput.y * animationSpeedPercent, 0.05f, Time.deltaTime);
        animator.SetFloat("Horizontal Move", moveInput.x * animationSpeedPercent, 0.05f, Time.deltaTime);
        //실제 움직임을 기반으로 하므로 벽에 막혀있는데 계속해서 뛰는 현상 없어짐 
    }
}