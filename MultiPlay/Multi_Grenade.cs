using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
#endif

public class Multi_Grenade : MonoBehaviourPun
{
    public ParticleSystem effectObj;
    public float attackRadius = 2f;
    private Rigidbody rigid;
    private int GrenadeHolder;
    private int damage;
    private LayerMask targetLayer;
    // Start is called before the first frame update

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
        Gizmos.DrawSphere(transform.position, attackRadius);
    }

#endif
    [PunRPC]
    public void SetUp(int holder, int Damage)
    {
        this.GrenadeHolder = holder;
        this.damage = Damage;
        if (PhotonView.Find(holder).tag == "Player") targetLayer = LayerMask.GetMask("Enemy");
        else targetLayer = LayerMask.GetMask("Player");
    }

    void Start()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        StartCoroutine(Explosion());
        rigid = GetComponent<Rigidbody>();
    }

    IEnumerator Explosion()
    {
        yield return new WaitForSeconds(3f);
        rigid.velocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;

        photonView.RPC("GrenadeEffectProcessOnClients", RpcTarget.All);
        yield return new WaitForSeconds(0.5f);
        gameObject.SetActive(false);

        OnDamage();
       
        PhotonNetwork.Destroy(gameObject);
    }

    [PunRPC]
    private void GrenadeEffectProcessOnClients()
    {
        effectObj.Play();
    }

    private void OnDamage()
    {
        RaycastHit[] hit = Physics.SphereCastAll(transform.position, attackRadius, Vector3.up, 0f, targetLayer);
        foreach (RaycastHit hitobj in hit)
        {
            var target = hitobj.collider.GetComponent<Multi_IDamageable>();
            if (target != null)
            {
                var damager = GrenadeHolder;
                var hitPoint = hitobj.point;
                var hitNormal = hitobj.normal;

                target.ApplyDamage(damage, hitPoint, hitNormal, damager);
            }
        }
    }
}
