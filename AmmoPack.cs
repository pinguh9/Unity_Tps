using UnityEngine;

public class AmmoPack : MonoBehaviour, IItem
{
    public int ammo = 30;

    public void Use(GameObject target)
    {
        var playershooter = target.GetComponent<PlayerShooter>();
       
        if (playershooter != null && playershooter.gun != null) {//총을 쥐고 있으면
            playershooter.gun.ammoRemain += ammo;
        }

        Destroy(gameObject);//자기자신을 파괴
    }
}