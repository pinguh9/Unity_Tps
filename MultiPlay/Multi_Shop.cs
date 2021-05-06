using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Multi_Shop : Multi_NpcEntity
{
  
    public override void Talk()
    {
        base.Talk();
        UIManager.Instance.SetActiveShopUI(true);
        NpcCam.SetActive(true);
        
    }

    public override void Exit()
    {
        base.Exit();
        UIManager.Instance.SetActiveShopUI(false);
        NpcCam.SetActive(false);
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
