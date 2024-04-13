using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundDetection : MonoBehaviour
{
    PlayerController player;

    bool isCurrentlyGrounded = false;

    private void Start()
    {
        player = GetComponentInParent<PlayerController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        player.SetTouchedGround(true);
        isCurrentlyGrounded = true;
    }

    private void OnTriggerStay(Collider other)
    {
        isCurrentlyGrounded = true;

        if (!player.IsGrounded())
        {
            player.SetTouchedGround(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        isCurrentlyGrounded = false;
        player.SetTouchedGround(false);
    }

    public bool IsCurrentlyGrounded()
    {
        return isCurrentlyGrounded;
    }
}
