using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;
using Cinemachine;

public class SpeedLines : MonoBehaviour
{
    [Header("Speed Clamping")]
    [SerializeField]
    float minSpeed = 5;
    [SerializeField]
    float maxSpeed = 50;

    [Header("Emission")]
    [SerializeField]
    float emissionMod = 1;

    [Header("Shape")]
    [SerializeField]
    float radiusFloor = 12;
    [SerializeField]
    float radiusMod = 1;

    [Header("Camera")]
    [SerializeField]
    CinemachineFreeLook mainCamera;
    [SerializeField]
    float focalLengthMod = 1;

    Rigidbody playerRb;
    ParticleSystem particles;

    float focalLengthFloor = 10;

    private void Awake()
    {
        playerRb = GetComponentInParent<Rigidbody>();
        particles = GetComponent<ParticleSystem>();

        focalLengthFloor = mainCamera.m_Lens.FieldOfView;
    }

    private void FixedUpdate()
    {
        UpdateSpeedLines();
    }

    private void UpdateSpeedLines()
    {
        float playerSpeed = VectorUtils.ZeroOutYAxis(playerRb.velocity).magnitude;
        EmissionModule emission = particles.emission;
        ShapeModule shape = particles.shape;

        float clampedSpeed = Mathf.Clamp(playerSpeed, minSpeed, maxSpeed);

        mainCamera.m_Lens.FieldOfView = (clampedSpeed / minSpeed) * focalLengthMod + focalLengthFloor;

        if (playerSpeed < minSpeed)
        {
            if(emission.enabled)
                emission.enabled = false;

            mainCamera.m_Lens.FieldOfView = focalLengthFloor;

            return;
        }

        if (!emission.enabled)
            emission.enabled = true;

        emission.rateOverTime = clampedSpeed * emissionMod;
        shape.radius = (radiusMod / clampedSpeed) + radiusFloor;

        Debug.Log($"Emission Rate: {emission.rateOverTime} | Shape Radius: {shape.radius} | Field of View: {mainCamera.m_Lens.FieldOfView}");
    }
}
