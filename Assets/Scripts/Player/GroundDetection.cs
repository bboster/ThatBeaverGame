using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundDetection : MonoBehaviour
{
    [SerializeField]
    List<string> excludedTags = new();

    PlayerController player;

    bool isCurrentlyGrounded = false;

    private void Start()
    {
        player = GetComponentInParent<PlayerController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (excludedTags.Contains(other.tag))
            return;

        player.SetTouchedGrass(true);
        isCurrentlyGrounded = true;
    }

    private void OnTriggerStay(Collider other)
    {
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
        isCurrentlyGrounded = false;
        player.SetTouchedGrass(false);
    }

    public bool IsCurrentlyGrounded()
    {
        return isCurrentlyGrounded;
    }
}
