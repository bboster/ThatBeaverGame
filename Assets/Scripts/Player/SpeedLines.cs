using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class SpeedLines : MonoBehaviour
{
    [SerializeField]
    float minSpeed = 5;

    [SerializeField]
    float maxSpeed = 50;

    [SerializeField]
    float emissionMod = 1;

    Rigidbody playerRb;
    ParticleSystem particles;

    //Transform cameraTransform;

    private void Awake()
    {
        playerRb = GetComponentInParent<Rigidbody>();
        particles = GetComponent<ParticleSystem>();

        //cameraTransform = Camera.current.transform;
    }

    private void LateUpdate()
    {
        //transform.LookAt(cameraTransform);
    }

    private void FixedUpdate()
    {
        UpdateSpeedLines();
    }

    private void UpdateSpeedLines()
    {
        float playerSpeed = VectorUtils.ZeroOutYAxis(playerRb.velocity).magnitude;
        EmissionModule emission = particles.emission;

        if(playerSpeed < minSpeed)
        {
            if(emission.enabled)
                emission.enabled = false;

            return;
        }

        if (!emission.enabled)
            emission.enabled = true;

        emission.rateOverTime = Mathf.Clamp(playerSpeed, minSpeed, maxSpeed) * emissionMod;
    }
}
