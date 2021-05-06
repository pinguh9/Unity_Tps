using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Multi_Sheriff : Multi_NpcEntity
{
    public override void Talk()
    {
        base.Talk();
        UIManager.Instance.SetActiveLevelSelectUI(true);
        NpcCam.SetActive(true);
    }

    public override void Exit()
    {
        base.Exit();
        UIManager.Instance.SetActiveLevelSelectUI(false);
        NpcCam.SetActive(false);
    }
}
