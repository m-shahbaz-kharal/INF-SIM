using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{

    void Start()
    {
        GetComponent<Canvas>().worldCamera = Camera.main;
    }
    void Update()
    {
        transform.LookAt(Camera.main.transform.position, Vector3.up);
    }
}
