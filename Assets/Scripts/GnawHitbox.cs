using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GnawHitbox : MonoBehaviour
{
    [SerializeField] float WaitDuration;
    [SerializeField] Collider col;

    private void Awake()
    {
        col = GetComponent<Collider>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision: " + collision.collider.gameObject);
        col.enabled = false;
    }

    private IEnumerator DisableCollider()
    {
        yield return new WaitForSeconds(WaitDuration);
        col.enabled = false;
    }
}
