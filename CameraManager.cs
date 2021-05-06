using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    private static CameraManager instance;

    public static CameraManager Instance
    {
        get
        {
            if (instance == null) instance = FindObjectOfType<CameraManager>();
            return instance;
        }
    }

    [SerializeField] private GameObject FollowCam;
    [SerializeField] private GameObject SheriffCam;
    [SerializeField] private GameObject ShopCam;


    public void SetActiveFollowCam(bool active)
    {
        FollowCam.SetActive(active);
    }

    public void SetActiveSheriffCam(bool active)
    {
        SheriffCam.SetActive(active);
    }

    public void SetActiveShopCam(bool active)
    {
        ShopCam.SetActive(active);
    }
}
