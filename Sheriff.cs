using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sheriff : NpcEntity
{
    public EnemySpawner enemySpawner;
    public override void Talk()
    {
        base.Talk();
        UIManager.Instance.SetActiveLevelSelectUI(true);
        CameraManager.Instance.SetActiveSheriffCam(true);
    }

    public override void Exit()
    {
        base.Exit();
        UIManager.Instance.SetActiveLevelSelectUI(false);
        CameraManager.Instance.SetActiveSheriffCam(false);
    }

    public void OnclickNormalLevel()
    {
        Exit();
        enemySpawner.Level_Normal();
    }
    public void OnclickHardLevel()
    {
        Exit();
        enemySpawner.Level_Hard();
    }
    public void OnclickInfLevel()
    {
        Exit();
        enemySpawner.Level_INF();
    }

}
