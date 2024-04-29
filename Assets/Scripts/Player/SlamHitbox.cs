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

    [Header("Rebound Force")]
    [SerializeField]
    float reboundForce = 5;
    [SerializeField]
    float reboundDelay = 0.08f;

    Collider col;
    Rigidbody parentRb;
    Transform collisionPoint;
    BeaverStats playerStats;
    PlayerController playerController;

    float force = 1;

    List<FracturedObjectContainer> objectsToFracture = new();

    bool willDisable = false;

    [Header("Audio")]
    [SerializeField]
    AudioClip beaverSlam;
    AudioSource slamSource;

    private void Awake()
    {
        col = GetComponent<Collider>();
        parentRb = GetComponentInParent<Rigidbody>();
        collisionPoint = transform.GetChild(0);
        playerStats = GetComponentInParent<BeaverStats>();
        playerController = GetComponentInParent<PlayerController>();

        col.enabled = false;

        slamSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        Fracture fracture = other.GetComponent<Fracture>();
        if (fracture == null)
        {
            if(!willDisable)
                StartCoroutine(SlamDurationTriggered());

            if (other.CompareTag("SoccerBall"))
            {
                Rigidbody otherRb = other.GetComponent<Rigidbody>();
                if (otherRb != null)
                {
                    Vector3 soccerForce = force * (explosionForce + (parentRb.velocity.magnitude * playerVelocityMult)) * transform.parent.forward;
                    soccerForce.y += upwardsModifier * 70;
                    otherRb.AddForce(soccerForce, ForceMode.Impulse);
                }

                //otherRb.AddExplosionForce(, collisionPoint.position, explosionRadius, upwardsModifier, forceMode);

            }

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
        slamSource.PlayOneShot(beaverSlam);

        GetComponentInParent<Animator>().SetTrigger("poundCollided");
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
        willDisable = false;

        yield return new WaitForSeconds(reboundDelay);
        parentRb.AddForce(Vector3.up * reboundForce, ForceMode.Impulse);

        playerController.FallingParticleToggle(0);
    }

    private void OnFractureCompletedEvent(object sender, FractureEventCompleteArgs e)
    {
        e.rigidbody.AddExplosionForce(force * (explosionForce + (parentRb.velocity.magnitude * playerVelocityMult)), collisionPoint.position, explosionRadius, upwardsModifier, forceMode);
    }
}
