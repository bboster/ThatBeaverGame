using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;

public class GnawNet : NetworkBehaviour
{
    [SerializeField] 
    Collider GnawHitBox;

    [SerializeField] 
    float GnawCooldown;
    [SerializeField] 
    float GnawDuration;

    [SerializeField] 
    AudioSource chompAudio;
    [SerializeField] 
    AudioClip beaverChomp;

    bool isGnawing = false;

    Animator anim;
    InputAction gnawAction;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        GnawHitBox.enabled = false;

        if (!isLocalPlayer)
            return;

        PlayerInput playerInput = GetComponent<PlayerInput>();
        gnawAction = playerInput.currentActionMap.FindAction("Attack");
        gnawAction.performed += OnGnaw;
    }

    public override void OnStopClient()
    {
        base.OnStopClient();

        if (!isLocalPlayer)
            return;

        gnawAction.performed -= OnGnaw;
    }

    private void OnGnaw(InputAction.CallbackContext context)
    {
        Debug.Log("Chomp... ");
        if (!isLocalPlayer)
            return;

        Debug.Log("Chomp... isGnawing");
        if (!isGnawing)
        {
            anim.SetTrigger("chomp");

            Debug.Log("Chomp Success!");
            StartCoroutine(JevWaiting());
        }
    }

    public IEnumerator JevWaiting()
    {
        isGnawing = true;
        yield return new WaitForSeconds(GnawCooldown);
        isGnawing = false;
    }
    // this following function is to be used during the Chomp animation as an animation event.

    public void DisableGnawHitbox()
    {
        GnawHitBox.enabled = false;
    }

    public void EnableGnawHitbox()
    {
        GnawHitBox.enabled = true;
        chompAudio.PlayOneShot(beaverChomp);
    }
}
