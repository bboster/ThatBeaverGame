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
    [Header("Gnaw Stats")]
    [SerializeField] float gnawDurationWhenTriggered = 0.05f;

    Collider col;
    Rigidbody parentRb;
    Transform collisionPoint;
    BeaverStats playerStats;

    List<FracturedObjectContainer> objectsToFracture = new();

    float force = 1;

    [SerializeField] private PointSystem PS;

    private void Awake()
    {
        col = GetComponent<Collider>();
        parentRb = GetComponentInParent<Rigidbody>();
        collisionPoint = transform.GetChild(0);
        playerStats = GetComponentInParent<BeaverStats>();
    }

    private void OnTriggerEnter(Collider other)
    {
        Fracture fracture = other.GetComponent<Fracture>();
        if (fracture == null)
        {
            if (other.CompareTag("SoccerBall"))
            {
                Rigidbody otherRb = other.GetComponent<Rigidbody>();
                if(otherRb != null)
                    otherRb.AddExplosionForce(30 * force * (explosionForce + (parentRb.velocity.magnitude * playerVelocityMult)), collisionPoint.position, explosionRadius, upwardsModifier, forceMode);

            }

            return;
        }

        if (objectsToFracture.Count == 0)
        {
            force = playerStats.GetStat(ScalableStat.FORCE);

            StartCoroutine(GnawDurationTriggered());
        }
            
        if (parentRb.velocity.magnitude * force >= fracture.fractureableSO.minForceToTrigger)
            objectsToFracture.Add(new(fracture, other));
    }

    private void OnGnawFail()
    {

    }

    private void OnGnawSuccess()
    {

    }

    

    private IEnumerator GnawDurationTriggered()
    {
        yield return new WaitForSeconds(gnawDurationWhenTriggered);
        col.enabled = false;

        if (objectsToFracture.Count == 0)
        {
            OnGnawFail();
        }
        else
        {
            Fragmenter.FractureCompletedEvent += OnFractureCompletedEvent;

            //Debug.Log("Objects to Fracture: " + objectsToFracture.Count + " | Time: " + Time.time);
            foreach (FracturedObjectContainer f in objectsToFracture)
            {
                if(f.Fracture.currentRefractureCount == 0)
                {
                    PS.AddPoint();
                }
                if (f.Collider == null)
                    continue;

                f.Fracture.CauseFracture(col, Physics.ClosestPoint(collisionPoint.position, f.Collider, f.Collider.transform.position, f.Collider.transform.rotation));
            }
                
            
            Fragmenter.FractureCompletedEvent -= OnFractureCompletedEvent;

            OnGnawSuccess();

            objectsToFracture.Clear();
        }
    }

    private void OnFractureCompletedEvent(object sender, FractureEventCompleteArgs e)
    {
        e.rigidbody.AddExplosionForce(force * (explosionForce + (parentRb.velocity.magnitude * playerVelocityMult)), collisionPoint.position, explosionRadius, upwardsModifier, forceMode);
    }
}

public class FracturedObjectContainer
{
    public Fracture Fracture { get; private set; }
    public Collider Collider { get; private set; }

    public FracturedObjectContainer(Fracture f, Collider col)
    {
        Fracture = f;
        Collider = col;
    }
}
