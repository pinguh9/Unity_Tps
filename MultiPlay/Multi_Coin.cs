using Photon.Pun;
using UnityEngine;

public class Multi_Coin : MonoBehaviourPun, IItem
{
    public int amount = 100;

    public void Use(GameObject target)
    {
        Multi_GameManager.Instance.AddMoney(amount);
        PhotonNetwork.Destroy(gameObject);
    }
}