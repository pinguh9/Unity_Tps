using Photon.Pun;
using UnityEngine;
using System.Collections;

public class Multi_PlayerHealth : Multi_LivingEntity
{
    private Animator animator;
    private AudioSource playerAudioPlayer;
    private float waitingTimeForRestore = 10f;

    public AudioClip deathClip;//죽을때
    public AudioClip hitClip;//피격당했을때

    private void Start()
    {
        if (photonView.IsMine)
        {
            StartCoroutine(RestoreHealth());
        }
    }
    private void Awake()
    {
        playerAudioPlayer = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();//livingEntity의 OnEnable 확장 / 체력 리셋
        UpdateUI();//체력유아이 갱신
    }
    
    private IEnumerator RestoreHealth()
    {
        while (!dead)
        {
            if (Time.time > lastDamagedTime + waitingTimeForRestore)
            {
                while (health + 1 <= startingHealth)
                {
                    health += 1;
                    UpdateUI();
                    yield return new WaitForSeconds(0.3f);
                }
            }
            yield return new WaitForSeconds(1f);
        }
    }

    private void UpdateUI()
    {
        UIManager.Instance.UpdateHealthText(dead ? 0f : health);//사망 상태가 아니라면 현재 체력
    }

    [PunRPC]
    public override void useHealthPack(float healthPackCap)
    {
        base.useHealthPack(healthPackCap);
        UpdateUI();
    }

    [PunRPC]
    public override bool ApplyDamage(float amount, Vector3 hitPoint, Vector3 hitNormal, int damager)
    {
        if (!base.ApplyDamage(amount, hitPoint, hitNormal, damager)) return false;//대미지 적용 코드를 먼저 실행 
        EffectManager.Instance.PlayHitEffect(hitPoint,
            hitNormal, transform, EffectManager.EffectType.Flesh);
        playerAudioPlayer.PlayOneShot(hitClip);

        UpdateUI();
        
        return true;
    }
    
    public override void Die()
    {
        base.Die();
        playerAudioPlayer.PlayOneShot(deathClip);
        animator.SetTrigger("Die");

        UpdateUI();
    }
}