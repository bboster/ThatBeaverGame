using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeaverGnaw : MonoBehaviour
{
    // Range of raycast - essentially for height detection between objects
    [SerializeField] GameObject GnawHitBox;

    [SerializeField] float GnawCooldown;

    bool GnawIsTrue = false;

    private void Start()
    {
        GnawHitBox.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            if(GnawIsTrue)
            {
                Debug.Log("JEV IS ALREADY DESTROYING, DO NOT RUSH!");
            }
            else
            {
                Debug.Log("JEV'S GNAW HAS ACTIVATED! GNAW AWAY LOVECRAFTIAN BEAST!");
                GnawIsTrue = true;
                GnawHitBox.SetActive(true);
                StartCoroutine("JevWaiting");
            }
        }
    }

    public IEnumerator JevWaiting()
    {
        Debug.Log("Jev is GNAWING");
        yield return new WaitForSeconds(GnawCooldown);

        GnawHitBox.SetActive(false);
        GnawIsTrue = false;
        Debug.Log("Jev is calm");
    }
}
