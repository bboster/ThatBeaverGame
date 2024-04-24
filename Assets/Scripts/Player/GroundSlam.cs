using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GroundSlam : MonoBehaviour
{
    [SerializeField]
    Collider hitbox;

    [SerializeField]
    float floatTime = 0.3f;

    [SerializeField]
    float floatForce = 2;

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

        if (playerController.IsGrounded())
            return;

        StartCoroutine(DelayedSlam());
        playerController.anim.SetTrigger("pound");
    }

    private IEnumerator DelayedSlam()
    {
        playerRb.velocity *= velocityMult;


        for(float i = 0; i < floatTime; i += 0.05f)
        {
            playerRb.AddForce(Vector3.up * floatForce, ForceMode.Acceleration);

            yield return new WaitForSeconds(0.05f);
        }
            

        playerRb.velocity *= velocityMult;

        playerRb.AddForce(Vector3.down * downwardForce, ForceMode.Impulse);

        hitbox.enabled = true;

        currentCooldown = cooldown;
    }
}
