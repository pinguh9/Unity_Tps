using UnityEngine;

public class Coin : MonoBehaviour, IItem
{
    public int amount = 100;

    public void Use(GameObject target)
    {
        GameManager.Instance.AddMoney(amount);
        Destroy(gameObject);
    }
}