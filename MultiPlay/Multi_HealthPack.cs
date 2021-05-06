using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Multi_HealthPack : MonoBehaviourPun, IItem
{
    public float health = 50;
   public void Use(GameObject target)
    {
        var livingEntitiy = target.GetComponent<LivingEntity>();
        if(livingEntitiy != null)
        {
            livingEntitiy.useHealthPack(50);
        }
        PhotonNetwork.Destroy(gameObject);
    }
}
