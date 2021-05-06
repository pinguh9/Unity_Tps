using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;

public class Multi_PlayerController : MonoBehaviourPun
{
    public AudioClip itemPickupClip;
    public Transform RespawnPoint;
    public int lifeRemains = 3;
    private AudioSource playerAudioPlayer;
    private Multi_PlayerHealth playerHealth;
    private Multi_PlayerMovement playerMovement;
    private Multi_PlayerShooter playerShooter;

    private void Start()
    {
        playerMovement = GetComponent<Multi_PlayerMovement>();
        playerShooter = GetComponent<Multi_PlayerShooter>();
        playerHealth = GetComponent<Multi_PlayerHealth>();
        playerAudioPlayer = GetComponent<AudioSource>();

        playerHealth.OnDeath += HandleDeath;

        UIManager.Instance.UpdateLifeText(lifeRemains);
        Cursor.visible = false;
    }
    
    private void HandleDeath()
    {
        //플레이어가 사망하면 다른 컴포넌트들 비활성화
        playerMovement.enabled = false;
        playerShooter.enabled = false;

        if (lifeRemains > 0)
        {
            lifeRemains--;
            UIManager.Instance.UpdateLifeText(lifeRemains);
            Invoke("Respawn", 3f);//실행하고 싶은 메소드의 이름과 지연시간
        }
        else
        {
            GameManager.Instance.EndGame();
        }
        Cursor.visible = true;
    }

    public void Respawn()
    {
        if (photonView.IsMine)
        {
            gameObject.SetActive(false);
            transform.position = RespawnPoint.position;
            //랜덤한 위치에서 리스폰될 수 있도록 함
        }
        playerMovement.enabled = true;
        playerShooter.enabled = true;
        gameObject.SetActive(true);//onEnable과 onDisable처리가 자동으로 실행되도록 껐다 켜줌

        playerShooter.gun.ammoRemain = 120;//재장전

        Cursor.visible = false;
    }


    private void OnTriggerEnter(Collider other)
    {//아이템을 먹는 처리
        if (playerHealth.dead)
        {
            return;
        }
        var item = other.GetComponent<IItem>();

        if(item != null)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                item.Use(gameObject);
                playerAudioPlayer.PlayOneShot(itemPickupClip);
            }
                
        }
    }
}