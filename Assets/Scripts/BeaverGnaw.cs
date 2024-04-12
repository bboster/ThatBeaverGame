using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeaverGnaw : MonoBehaviour
{
    // Range of raycast - essentially for height detection between objects
    [SerializeField] float range = 3;

    // Update is called once per frame
    void Update()
    {
        Vector3 direction = Vector3.forward;
        Ray GnawRay = new Ray(transform.position, transform.TransformDirection(direction * range));
        Debug.DrawRay(transform.position, transform.TransformDirection(direction * range));
    }
}
