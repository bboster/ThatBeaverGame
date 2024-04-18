using System.Collections;
using UnityEngine;

public class UIButtonShake : MonoBehaviour
{
    public float shakeDuration = 0.5f; // Duration of the shake effect
    public float shakeMagnitude = 1.5f; // Magnitude of the shake effect
    public float shakeSpeed = 5.0f; // Speed of the shake effect

    private Vector3 originalPosition; // Original position of the button
    private bool isShaking = false; // Flag to check if the button is currently shaking

    private Quaternion originalRotation; // Original rotation of the button

    private void Start()
    {
        originalPosition = transform.position; // Store the original position
        originalRotation = transform.rotation; // Store the original rotation
    }

    public void StartShake()
    {
        if (!isShaking)
        {
            StartCoroutine(Shake());
        }
    }

    private IEnumerator Shake()
    {
        isShaking = true;

        float elapsedTime = 0.0f;

        while (elapsedTime < shakeDuration)
        {
            // Calculate the rotation angle using a sine wave
            float rotationAngle = Mathf.Sin(elapsedTime * shakeSpeed) * shakeMagnitude;

            // Scale down the rotation angle
            rotationAngle *= 0.1f; // Adjust this factor to control the rotation angle

            // Apply the rotation effect to the button
            transform.rotation = Quaternion.Euler(originalRotation.x, originalRotation.y, rotationAngle);

            // Increment the elapsed time
            elapsedTime += Time.deltaTime;

            yield return null;
        }

        // Reset the button's rotation to its original rotation
        transform.rotation = originalRotation;

        isShaking = false;
    }
}