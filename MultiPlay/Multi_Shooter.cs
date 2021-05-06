using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Multi_Shooter : MonoBehaviourPun
{
    public Multi_Gun gun;
    public LayerMask excludeTarget;//조준에서 제외할 레이어 할당
    protected virtual void OnEnable()
    {
        gun.Setup(this);
    }
}
