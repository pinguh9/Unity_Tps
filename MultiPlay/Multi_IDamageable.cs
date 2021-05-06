using UnityEngine;

public interface Multi_IDamageable
{
    bool ApplyDamage(float amount, Vector3 hitPoint, Vector3 hitNormal, int damager);
    bool checkDamager(GameObject obj);
}