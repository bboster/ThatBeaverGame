using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GnawHitbox : MonoBehaviour
{
    Collider col;
    Rigidbody parentRb;
    Transform collisionPoint;

    private void Awake()
    {
        col = GetComponent<Collider>();
        parentRb = GetComponentInParent<Rigidbody>();
        collisionPoint = transform.GetChild(0);
    }

    private void OnTriggerEnter(Collider other)
    {
        Fracture fracture = other.GetComponent<Fracture>();
        if (fracture == null)
            return;

        if (parentRb.velocity.magnitude * parentRb.mass < fracture.MinForceForTrigger)
        {
            OnGnawFail();
            return;
        }

        fracture.CauseFracture(col, Physics.ClosestPoint(collisionPoint.position, other, other.transform.position, other.transform.rotation));

        OnGnawSuccess();
        col.enabled = false;
    }

    private void OnGnawFail()
    {

    }

    private void OnGnawSuccess()
    {

    }
}
