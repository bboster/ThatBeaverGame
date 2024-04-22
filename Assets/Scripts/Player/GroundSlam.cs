using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GroundSlam : MonoBehaviour
{
    [SerializeField]
    float downwardForce = 5;

    [SerializeField]
    float velocityMult = 0.1f;

    PlayerController playerController;
    Rigidbody playerRb;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        playerRb = GetComponent<Rigidbody>();

        PlayerInput playerInput = GetComponent<PlayerInput>();
        playerInput.currentActionMap.FindAction("GroundSlam").performed += OnGroundSlam;
    }

    private void OnDisable()
    {
        PlayerInput playerInput = GetComponent<PlayerInput>();

        if (playerInput == null || playerInput.currentActionMap == null)
            return;

        playerInput.currentActionMap.FindAction("GroundSlam").performed -= OnGroundSlam;
    }

    private void OnGroundSlam(InputAction.CallbackContext context)
    {
        playerRb.velocity *= velocityMult;

        playerRb.AddForce(Vector3.down * downwardForce, ForceMode.Impulse);
    }
}
