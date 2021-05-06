using Photon.Pun;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerName : MonoBehaviourPun
{
    public Text playername;
    private Transform target;
    // Update is called once per frame

    [PunRPC]
    public void SetPlayerName()
    {
        if (photonView.IsMine)
        {
            playername.text = PhotonNetwork.NickName;
        }
        else
        {
            playername.text = photonView.Owner.NickName;
        }
    }

    void Start()
    {
        target = Camera.main.gameObject.transform;
        photonView.RPC("SetPlayerName", RpcTarget.All);
    }
    void Update()
    {
        transform.rotation = Quaternion.LookRotation(target.forward, target.up);
    }
}
