using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeaverGnaw : MonoBehaviour
{
    // Range of raycast - essentially for height detection between objects
    [SerializeField] Collider GnawHitBox;

    [SerializeField] float GnawCooldown;
    [SerializeField] float GnawDuration;

    bool GnawIsTrue = false;
    Animator anim;

    AudioSource chompAudio;
    [SerializeField] AudioClip beaverChomp;

    private void Start()
    {
        GnawHitBox.enabled = false;
        anim = GetComponent<Animator>();
        chompAudio = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            if (GnawIsTrue)
            {
                //Debug.Log("JEV IS ALREADY DESTROYING, DO NOT RUSH!");
            }
            else
            {
                anim.SetTrigger("chomp");
                chompAudio.PlayOneShot(beaverChomp);

                //Debug.Log("JEV'S GNAW HAS ACTIVATED! GNAW AWAY LOVECRAFTIAN BEAST!");
                //GnawIsTrue = true;
                //GnawHitBox.enabled = true;
                StartCoroutine(JevWaiting());
            }
        }
    }

    public IEnumerator JevWaiting()
    {
        GnawIsTrue = true;
        yield return new WaitForSeconds(GnawCooldown);
        GnawIsTrue = false;
    }
    // this following function is to be used during the Chomp animation as an animation event.

    public void DisableGnawHitbox()
    {
        GnawHitBox.enabled = false;
    }

    public void EnableGnawHitbox()
    {
        GnawHitBox.enabled = true;
    }
}
