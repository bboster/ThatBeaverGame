using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GnawHitboxNet : NetworkBehaviour
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
    [Header("Soccer Ball")]
    [SerializeField]
    float soccerForceMod = 50;
    [SerializeField]
    float soccerUpwards = 1;

    Collider col;
    Rigidbody parentRb;
    Transform collisionPoint;
    BeaverStats playerStats;

    List<FracturedObjectContainer> objectsToFracture = new();

    float force = 1;

    [SerializeField] private AnimationClip plusAnim;
    private Animation anim;

    private void Awake()
    {
        col = GetComponent<Collider>();
        parentRb = GetComponentInParent<Rigidbody>();
        collisionPoint = transform.GetChild(0);
        playerStats = GetComponentInParent<BeaverStats>();
        anim = GetComponent<Animation>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isLocalPlayer)
            return;

        Fracture fracture = other.GetComponent<Fracture>();
        if (fracture == null)
        {
            if (other.CompareTag("SoccerBall"))
            {
                Rigidbody otherRb = other.GetComponent<Rigidbody>();
                if (otherRb != null)
                {
                    Vector3 soccerForce = soccerForceMod * force * (explosionForce + (parentRb.velocity.magnitude * playerVelocityMult)) * transform.parent.forward;
                    soccerForce.y += upwardsModifier * soccerUpwards;
                    otherRb.AddForce(soccerForce, ForceMode.Impulse);
                }

                //otherRb.AddExplosionForce(, collisionPoint.position, explosionRadius, upwardsModifier, forceMode);

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
            foreach (FracturedObjectContainer f in objectsToFracture)
            {
                if (f.Collider == null)
                    continue;

                CmdFractureObject(FractureManager.Instance.GetId(f.Fracture), collisionPoint.position);
            }

            objectsToFracture.Clear();
        }
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

        if(fracture == null)
        {
            Debug.LogError("Error: Fracture not found in Manager! ID: " + targetId);
            return;
        }

        Fragmenter.FractureCompletedEvent += OnFractureCompletedEvent;

        Collider collider = fracture.GetComponent<Collider>();

        if (fracture == null || collider == null)
            return;

        if (fracture.currentRefractureCount == 0)
        {
            anim.clip = plusAnim;
            anim.Play();
        }

        fracture.CauseFracture(col, Physics.ClosestPoint(fracturePoint, collider, fracture.transform.position, fracture.transform.rotation));

        OnGnawSuccess();

        Fragmenter.FractureCompletedEvent -= OnFractureCompletedEvent;
    }

    private void OnFractureCompletedEvent(object sender, FractureEventCompleteArgs e)
    {
        e.rigidbody.AddExplosionForce(force * (explosionForce + (parentRb.velocity.magnitude * playerVelocityMult)), collisionPoint.position, explosionRadius, upwardsModifier, forceMode);
    }
}
