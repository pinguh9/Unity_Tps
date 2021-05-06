using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPack : MonoBehaviour, IItem
{
    public float health = 50;
   public void Use(GameObject target)
    {
        var livingEntitiy = target.GetComponent<LivingEntity>();
        if(livingEntitiy != null)
        {
            livingEntitiy.useHealthPack(50);
        }
        Destroy(gameObject);
    }
}
