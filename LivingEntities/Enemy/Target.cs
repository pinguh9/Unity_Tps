using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : LivingEntity
{
    private float TargetstartingHealth = 1000f;

    protected override void OnEnable()
    {
        dead = false;
        health = TargetstartingHealth;
    }

    public override bool ApplyDamage(DamageMessage damageMessage)
    {
        if (base.ApplyDamage(damageMessage)) return false;
        EffectManager.Instance.PlayHitEffect(damageMessage.hitPoint, damageMessage.hitNormal, transform,
            EffectManager.EffectType.Common);
        return true;
    }


    public override void Die()
    {
        base.Die();
        GameManager.Instance.EndGame();
    }
  
}
