using System;
using UnityEngine;

public class LivingEntity : MonoBehaviour, IDamageable
{
    public float startingHealth = 100f;//초기체력
    public float health { get; protected set; }
    public bool dead { get; protected set; }
    
    public event Action OnDeath;
    
    private const float minTimeBetDamaged = 0.1f;
    protected float lastDamagedTime;

    protected bool IsInvulnerable
    {
        get
        {
            if (Time.time >= lastDamagedTime + minTimeBetDamaged) return false;

            return true;
        }
    }
    
    protected virtual void OnEnable()
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
