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
    float fovMod = 1;
    [SerializeField]
    float fovTransitionSpeed = 0.5f;

    Rigidbody playerRb;
    ParticleSystem particles;

    float fovFloor = 70;

    float playerSpeed = 0;

    private void Awake()
    {
        playerRb = GetComponentInParent<Rigidbody>();
        particles = GetComponent<ParticleSystem>();

        fovFloor = mainCamera.m_Lens.FieldOfView;
    }

    private void FixedUpdate()
    {
        UpdateSpeedLines();
    }

    private void LateUpdate()
    {
        mainCamera.m_Lens.FieldOfView = Mathf.Lerp(mainCamera.m_Lens.FieldOfView, (Mathf.Clamp(playerSpeed, 0, maxSpeed) / minSpeed) * fovMod + fovFloor, fovTransitionSpeed);
    }

    private void UpdateSpeedLines()
    {
        playerSpeed = VectorUtils.ZeroOutYAxis(playerRb.velocity).magnitude;
        EmissionModule emission = particles.emission;
        ShapeModule shape = particles.shape;

        if (playerSpeed < minSpeed)
        {
            if(emission.enabled)
                emission.enabled = false;

            return;
        }

        if (!emission.enabled)
            emission.enabled = true;

        float clampedSpeed = Mathf.Clamp(playerSpeed, minSpeed, maxSpeed);

        emission.rateOverTime = clampedSpeed * emissionMod;
        shape.radius = (radiusMod / clampedSpeed) + radiusFloor;

        //Debug.Log($"Emission Rate: {emission.rateOverTime} | Shape Radius: {shape.radius} | Field of View: {mainCamera.m_Lens.FieldOfView}");
    }
}
