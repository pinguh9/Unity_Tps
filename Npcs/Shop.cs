using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shop : NpcEntity
{
  
    public override void Talk()
    {
        base.Talk();
        UIManager.Instance.SetActiveShopUI(true);
        CameraManager.Instance.SetActiveShopCam(true);
        
    }

    public override void Exit()
    {
        base.Exit();
        UIManager.Instance.SetActiveShopUI(false);
        CameraManager.Instance.SetActiveShopCam(false);
    }

    public void BuyGrenade()
    {
        var playershooter = target.GetComponent<PlayerShooter>();
        if (playershooter.Grenadecnt < playershooter.maxGrenade)
        {
            playershooter.Grenadecnt += 1;
        }
    }
}
