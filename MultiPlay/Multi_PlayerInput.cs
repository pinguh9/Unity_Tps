using Photon.Pun;
using UnityEngine;

public class Multi_PlayerInput : MonoBehaviourPun
{
    public string fireButtonName = "Fire1";//총을 발사할때 사용할 버튼
    public string jumpButtonName = "Jump"; // 점프할때 사용할 버튼
    public string moveHorizontalAxisName = "Horizontal";//좌우 수평방향 움직임 감지
    public string moveVerticalAxisName = "Vertical"; // 수직방향(앞,뒤) 움직임 감지
    public string reloadButtonName = "Reload";//재장전할때 사용할 버튼
    public string grenadeButtonName = "Grenade";//수류탄
    public Vector2 moveInput { get; private set; }
    public bool fire { get; private set; }
    public bool reload { get; private set; }
    public bool jump { get; private set; }
    public bool grenade { get; private set; }
    //자동생성 프로퍼티 값을 읽을 때와 값을 쓸 때 액세스를 다르게 함
    //외부 스크립트에서 함부로 값을 수정할 수 없음
    private void Update()
    {
        if (!photonView.IsMine)return;
        if (GameManager.Instance != null
            && GameManager.Instance.isGameover)
        {
            moveInput = Vector2.zero;
            fire = false;
            reload = false;
            jump = false;
            grenade = false;
            return;
        }//게임오버된 경우에는 유저 입력을 모두 무시 

        moveInput = new Vector2(Input.GetAxis(moveHorizontalAxisName), Input.GetAxis(moveVerticalAxisName));
        if (moveInput.sqrMagnitude > 1) moveInput = moveInput.normalized;
        //moveInput의 길이는 1보다 작거나 같아야 함
        //속도를 곱해서 사용하기 때문에 대각선 방향으로 갈때 속도가 더 빨라질 수 있음 
        //magnitude(벡터의 길이를 구하는 연산) : 비쌈 
        jump = Input.GetButtonDown(jumpButtonName);
        fire = Input.GetButton(fireButtonName);
        reload = Input.GetButtonDown(reloadButtonName);
        grenade = Input.GetButtonDown(grenadeButtonName);
    }
}