using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GroundDetectionNet : NetworkBehaviour
{
    [SerializeField]
    List<string> excludedTags = new();

    PlayerControllerNet player;

    bool isCurrentlyGrounded = false;

    private void Start()
    {
        player = GetComponentInParent<PlayerControllerNet>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isLocalPlayer)
            return;

        if (excludedTags.Contains(other.tag))
            return;

        player.SetTouchedGrass(true);
        isCurrentlyGrounded = true;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!isLocalPlayer)
            return;

        if (excludedTags.Contains(other.tag))
            return;

        isCurrentlyGrounded = true;

        if (!player.IsGrounded())
        {
            player.SetTouchedGrass(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!isLocalPlayer)
            return;

        isCurrentlyGrounded = false;
        player.SetTouchedGrass(false);
    }

    public bool IsCurrentlyGrounded()
    {
        return isCurrentlyGrounded;
    }
}
