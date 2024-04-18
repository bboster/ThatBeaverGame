using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GnawHitbox : MonoBehaviour
{
    [Header("Gnaw Explosion Physics")]
    [SerializeField]
    float explosionForce = 5;
    [SerializeField]
    float explosionRadius = 1;
    [SerializeField]
    float playerVelocityMult = 0.3f;
    [SerializeField]
    float upwardsModifier = 0.3f;
    [SerializeField]
    ForceMode forceMode = ForceMode.Impulse;

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

        Fragmenter.FractureCompletedEvent += OnFractureCompletedEvent;

        fracture.CauseFracture(col, Physics.ClosestPoint(collisionPoint.position, other, other.transform.position, other.transform.rotation));
        StartCoroutine(DisableFractureListener());

        OnGnawSuccess();
        col.enabled = false;
    }

    private void OnGnawFail()
    {

    }

    private void OnGnawSuccess()
    {

    }

    private IEnumerator DisableFractureListener()
    {
        yield return new WaitForEndOfFrame();
        Fragmenter.FractureCompletedEvent -= OnFractureCompletedEvent;
    }

    private void OnFractureCompletedEvent(object sender, FractureEventCompleteArgs e)
    {
        e.rigidbody.AddExplosionForce(explosionForce + (parentRb.velocity.magnitude * playerVelocityMult), collisionPoint.position, explosionRadius, upwardsModifier, forceMode);
    }
}
