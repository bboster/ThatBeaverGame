using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;

public class GroundSlamNet : NetworkBehaviour
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
    PlayerControllerNet playerController;

    float currentCooldown = 0;

    [SerializeField]
    AudioSource slamAudio;

    [SerializeField]
    AudioClip beaverFlip;

    private void Awake()
    {
        playerRb = GetComponent<Rigidbody>();
        playerController = GetComponent<PlayerControllerNet>();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!isLocalPlayer)
            return;

        PlayerInput playerInput = GetComponent<PlayerInput>();
        slamAction = playerInput.currentActionMap.FindAction("GroundSlam");
        slamAction.performed += OnGroundSlam;
    }

    public override void OnStopClient()
    {
        base.OnStopClient();

        if (!isLocalPlayer)
            return;

        slamAction.performed -= OnGroundSlam;
    }

    private void Update()
    {
        if (!isLocalPlayer)
            return;

        if (currentCooldown > 0)
            currentCooldown -= Time.deltaTime;
    }

    private void OnGroundSlam(InputAction.CallbackContext context)
    {
        Debug.Log("Ground Slam...");

        if (!isLocalPlayer)
            return;

        Debug.Log("Ground Slam... CD");
        if (currentCooldown > 0)
            return;

        Debug.Log("Ground Slam... height");
        if (playerController.IsGrounded() || !IsAboveMinHeight())
            return;

        Debug.Log("Ground Slam Completed!");
        StartCoroutine(DelayedSlam());
        playerController.anim.SetTrigger("pound");

        slamAudio.PlayOneShot(beaverFlip);
    }

    private IEnumerator DelayedSlam()
    {
        currentCooldown = cooldown;

        playerRb.velocity *= velocityMult;

        playerController.SetMovementState(PlayerControllerNet.MovementState.SLAMMING);

        for (float i = 0; i < floatTime; i += Time.deltaTime)
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

        playerController.SetMovementState(PlayerControllerNet.MovementState.STATIONARY);
    }

    private bool IsAboveMinHeight()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minSlamHeight);
    }
}
