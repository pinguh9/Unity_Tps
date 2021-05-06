using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
#endif

public class Grenade : MonoBehaviour
{
    public ParticleSystem effectObj;
    public ParticleSystem RangeEffectBase;
    public ParticleSystem RangeEffect;

    public float attackRadius = 2f;
   
    private Rigidbody rigid;
    private Shooter GrenadeHolder;
    private float damage;
    private LayerMask targetLayer;
    private Vector3 startSize = new Vector3(0f, 0f, 0f);
    private Vector3 targetSize = new Vector3(1f, 1f, 1f);
    // Start is called before the first frame update

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
        Gizmos.DrawSphere(transform.position, attackRadius);
    }

#endif

    public void SetUp(Shooter holder, float Damage, LayerMask targetlayer)
    {
        this.GrenadeHolder = holder;
        this.damage = Damage;
        this.targetLayer = targetlayer;
    }

    void Start()
    {
        rigid = GetComponent<Rigidbody>();
    }

    IEnumerator Explosion()
    {
        RangeEffectBase.gameObject.SetActive(true);
        while (RangeEffect.transform.localScale.x < 1f)
        {
            RangeEffect.transform.localScale += new Vector3(0.02f, 0.02f, 0.02f);
            yield return new WaitForSeconds(0.05f);
        }

        rigid.velocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;
        effectObj.Play();
        yield return new WaitForSeconds(0.5f);
        gameObject.SetActive(false);

        RaycastHit[] hit = Physics.SphereCastAll(transform.position, attackRadius, Vector3.up, 0f, targetLayer);
        foreach(RaycastHit hitobj in hit)
        {
            var target = hitobj.collider.GetComponent<IDamageable>();
            if(target != null)
            {
                DamageMessage damageMessage;

                damageMessage.damager = GrenadeHolder.gameObject;
                damageMessage.amount = damage;
                damageMessage.hitPoint = hitobj.point;
                damageMessage.hitNormal = hitobj.normal;

                target.ApplyDamage(damageMessage);
            }
        }

        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision other)
    {
        StartCoroutine(Explosion());
    }

}
