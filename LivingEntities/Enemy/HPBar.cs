using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class HPBar : MonoBehaviour
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
        var livingentity = enemy.GetComponent<LivingEntity>();
        transform.rotation = Quaternion.LookRotation(target.forward, target.up);
        hpBar.value = livingentity.health / livingentity.startingHealth; 
    }
}
