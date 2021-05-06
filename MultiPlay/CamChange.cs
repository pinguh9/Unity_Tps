using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamChange : MonoBehaviour
{
    public GameObject[] cameras;

    private void Start()
    {
        StartCoroutine(changeCamera());
    }

    private IEnumerator changeCamera()
    {
        foreach (var cam in cameras)
        {
            cam.SetActive(true);
            yield return new WaitForSeconds(5f);
            cam.SetActive(false);
        }
        StartCoroutine(changeCamera());
    }

}
