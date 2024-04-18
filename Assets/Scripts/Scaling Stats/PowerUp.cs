using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour
{
    [SerializeField]
    ScalingStatsSO scalingStats;

    [Tooltip("When set to 0, lasts infinitely.")]
    [SerializeField]
    float duration = 0;

    private void OnTriggerEnter(Collider other)
    {
        BeaverStats playerStats = other.GetComponent<BeaverStats>();
        if (playerStats == null)
            return;

        if(duration <= 0)
            playerStats.AddStats(scalingStats);
        else
            playerStats.TemporaryAddStats(scalingStats, duration);

        Destroy(gameObject);
    }
}
