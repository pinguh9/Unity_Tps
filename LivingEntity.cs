using System;
using UnityEngine;

public class LivingEntity : MonoBehaviour, IDamageable
{
    public float startingHealth = 100f;//초기체력
    public float health { get; protected set; }
    public bool dead { get; protected set; }//외부에서 값을 읽을 순  있지만 덮어쓸 수는 없음
    
    public event Action OnDeath;//LinvingEntity가 사망하는 순간에 실행될 콜백을 외부에서 접근해서 할당할 수 있는 이벤트. 
    
    private const float minTimeBetDamaged = 0.1f;//공격과 공격 사이 최소 대기시간 값이 너무 크면 정상적인 공격도 막히게됨.
    protected float lastDamagedTime;//짧은 시간 안에 공격이 두번 들어가는 예외사항이 가끔 생김

    protected bool IsInvulnerable
    {
        get
        {
            if (Time.time >= lastDamagedTime + minTimeBetDamaged) return false;

            return true;
        }
    }
    
    protected virtual void OnEnable()//자식 클래스에서 기존 기능에 확장해서 사용해야 하기 때문에 거의 가상함수 
    {
        dead = false;
        health = startingHealth;
    }

    public virtual void useHealthPack(float healthPackCap)
    {
        if (dead) return;
        var newHealth = Mathf.Clamp(health + healthPackCap, 0, 100);
        health = newHealth;
       
    }

    public virtual bool ApplyDamage(DamageMessage damageMessage)
    {
        if (IsInvulnerable || damageMessage.damager == gameObject || dead) return false;

        lastDamagedTime = Time.time;
        health -= damageMessage.amount;
        
        if (health <= 0) Die();

        return true;
    }

    public virtual void Die()
    {
        if (OnDeath != null) OnDeath();
        dead = true;
    }
}