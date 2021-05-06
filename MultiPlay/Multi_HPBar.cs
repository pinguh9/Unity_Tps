using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class Multi_HPBar : MonoBehaviourPun
{
    public GameObject enemy;
    public Slider hpBar;
    private Transform target;

    // Start is called before the first frame update
    void Start()
    {
        target = Camera.main.gameObject.transform;
    }

    // Update is called once per frame
    void Update()
    {
        var Multi_livingentity = enemy.GetComponent<Multi_LivingEntity>();
        transform.rotation = Quaternion.LookRotation(target.forward, target.up);
        hpBar.value = Multi_livingentity.health / Multi_livingentity.startingHealth; 
    }
}
