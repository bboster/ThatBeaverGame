using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlamHitbox : MonoBehaviour
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
    [Header("Slam Stats")]
    [SerializeField] 
    float slamDurationWhenTriggered = 0.05f;
    [SerializeField]
    float baseForceModifier = 0.3f;

    Collider col;
    Rigidbody parentRb;
    Transform collisionPoint;
    BeaverStats playerStats;

    float force = 1;

    List<FracturedObjectContainer> objectsToFracture = new();

    bool willDisable = false;

    private void Awake()
    {
        col = GetComponent<Collider>();
        parentRb = GetComponentInParent<Rigidbody>();
        collisionPoint = transform.GetChild(0);
        playerStats = GetComponentInParent<BeaverStats>();

        col.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        Fracture fracture = other.GetComponent<Fracture>();
        if (fracture == null)
        {
            if(!willDisable)
                StartCoroutine(SlamDurationTriggered()); 

            return;
        }
            

        if (objectsToFracture.Count == 0)
        {
            force = playerStats.GetStat(ScalableStat.FORCE);

            StartCoroutine(SlamDurationTriggered());
        }

        if (parentRb.velocity.magnitude * force * baseForceModifier >= fracture.fractureableSO.minForceToTrigger)
            objectsToFracture.Add(new(fracture, other));
    }

    private IEnumerator SlamDurationTriggered()
    {
        Animator anim = GetComponentInParent<Animator>();
        willDisable = true;
        yield return new WaitForSeconds(slamDurationWhenTriggered);
        col.enabled = false;

        if (objectsToFracture.Count == 0)
        {
            
        }
        else
        {
            Fragmenter.FractureCompletedEvent += OnFractureCompletedEvent;

            foreach (FracturedObjectContainer f in objectsToFracture)
            {
                if (f.Collider == null)
                    continue;

                f.Fracture.CauseFracture(col, Physics.ClosestPoint(collisionPoint.position, f.Collider, f.Collider.transform.position, f.Collider.transform.rotation));
            }

            Fragmenter.FractureCompletedEvent -= OnFractureCompletedEvent;

            objectsToFracture.Clear();
        }

        anim.ResetTrigger("poundCollided");
        willDisable = false;
    }

    private void OnFractureCompletedEvent(object sender, FractureEventCompleteArgs e)
    {
        e.rigidbody.AddExplosionForce(force * (explosionForce + (parentRb.velocity.magnitude * playerVelocityMult)), collisionPoint.position, explosionRadius, upwardsModifier, forceMode);
    }
}
