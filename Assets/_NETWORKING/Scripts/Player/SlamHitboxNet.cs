using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SlamHitboxNet : NetworkBehaviour
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
    Animator animator;

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
        animator = GetComponentInParent<Animator>();

        slamSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        col.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isLocalPlayer)
            return;

        Fracture fracture = other.GetComponent<Fracture>();
        if (fracture == null)
        {
            if (!willDisable)
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

        animator.SetTrigger("poundCollided");
        willDisable = true;
        yield return new WaitForSeconds(slamDurationWhenTriggered);
        col.enabled = false;

        if (objectsToFracture.Count == 0)
        {

        }
        else
        {
            foreach (FracturedObjectContainer f in objectsToFracture)
            {
                if (f.Collider == null)
                    continue;

                CmdFractureObject(FractureManager.Instance.GetId(f.Fracture), collisionPoint.position);
            }

            objectsToFracture.Clear();
        }
        willDisable = false;

        yield return new WaitForSeconds(reboundDelay);
        parentRb.AddForce(Vector3.up * reboundForce, ForceMode.Impulse);

        playerController.FallingParticleToggle(0);
    }

    [Command]
    private void CmdFractureObject(uint targetId, Vector3 fracturePoint)
    {
        RpcFractureObject(targetId, fracturePoint);
    }

    [ClientRpc]
    private void RpcFractureObject(uint targetId, Vector3 fracturePoint)
    {
        Fracture fracture = FractureManager.Instance.GetFracture(targetId);

        if (fracture == null)
        {
            Debug.LogError("Error: Fracture not found in Manager! ID: " + targetId);
            return;
        }

        Fragmenter.FractureCompletedEvent += OnFractureCompletedEvent;

        Collider collider = fracture.GetComponent<Collider>();

        if (fracture == null || collider == null)
            return;

        fracture.CauseFracture(col, Physics.ClosestPoint(fracturePoint, collider, fracture.transform.position, fracture.transform.rotation));

        Fragmenter.FractureCompletedEvent -= OnFractureCompletedEvent;
    }

    private void OnFractureCompletedEvent(object sender, FractureEventCompleteArgs e)
    {
        e.rigidbody.AddExplosionForce(force * (explosionForce + (parentRb.velocity.magnitude * playerVelocityMult)), collisionPoint.position, explosionRadius, upwardsModifier, forceMode);
    }
}
