using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GroundSlam : MonoBehaviour
{
    [SerializeField]
    Collider hitbox;

    [SerializeField]
    float minSlamHeight = 2;

    [SerializeField]
    float floatTime = 0.3f;

    [SerializeField]
    float downwardForce = 5;

    [SerializeField]
    float velocityMult = 0.1f;

    [SerializeField]
    float cooldown = 1;

    InputAction slamAction;
    Rigidbody playerRb;
    PlayerController playerController;

    float currentCooldown = 0;

    private void Awake()
    {
        playerRb = GetComponent<Rigidbody>();
        playerController = GetComponent<PlayerController>();
    }

    private void OnEnable()
    {
        PlayerInput playerInput = GetComponent<PlayerInput>();
        slamAction = playerInput.currentActionMap.FindAction("GroundSlam");
        slamAction.performed += OnGroundSlam;
    }

    private void Update()
    {
        if (currentCooldown > 0)
            currentCooldown -= Time.deltaTime;
    }

    private void OnDisable()
    {
        slamAction.performed -= OnGroundSlam;
    }

    private void OnGroundSlam(InputAction.CallbackContext context)
    {
        if (currentCooldown > 0)
            return;

        if (playerController.IsGrounded() || !IsAboveMinHeight())
            return;

        StartCoroutine(DelayedSlam());
        playerController.anim.SetTrigger("pound");
    }

    private IEnumerator DelayedSlam()
    {
        currentCooldown = cooldown;

        playerRb.velocity *= velocityMult;

        playerController.SetMovementState(PlayerController.MovementState.SLAMMING);

        for(float i = 0; i < floatTime; i += Time.deltaTime)
        {
            playerRb.velocity = VectorUtils.ZeroOutYAxis(playerRb.velocity);

            yield return new WaitForEndOfFrame();
        }

        hitbox.enabled = true;

        playerRb.velocity *= velocityMult;

        playerRb.AddForce(Vector3.down * downwardForce, ForceMode.Impulse);

        while (hitbox.enabled)
        {
            playerRb.AddForce(Vector3.down * downwardForce, ForceMode.Impulse);
            yield return new WaitForEndOfFrame();
        }

        playerController.SetMovementState(PlayerController.MovementState.STATIONARY);
    }

    private bool IsAboveMinHeight()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minSlamHeight);
    }
}
