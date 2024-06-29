using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PowerUp : MonoBehaviour
{
    [SerializeField]
    ScalingStatsSO scalingStats;

    [Tooltip("When set to 0 or less, lasts infinitely.")]
    [SerializeField]
    float duration = 0;

    [SerializeField]
    float textDuration = 1;

    Animator anim;

    GameObject visuals;

    TMP_Text text;

    //AudioSource powerupAudio;

    //[SerializeField]
    //AudioClip beaverGulp;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        visuals = transform.GetChild(0).gameObject;
        text = GetComponentInChildren<TMP_Text>();

        text.color = scalingStats.color;
        text.text = scalingStats.text;

        text.enabled = false;

        //powerupAudio = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        BeaverStats playerStats = other.GetComponent<BeaverStats>();
        if (playerStats == null)
            return;

        if(duration <= 0)
            playerStats.AddStats(scalingStats);
        else
            playerStats.TemporaryAddStats(scalingStats, duration);

        visuals.SetActive(false);
        text.enabled = true;

        
        Vector3 targetPostition = new(-other.attachedRigidbody.velocity.x, this.transform.position.y, -other.attachedRigidbody.velocity.z);
        transform.rotation = Quaternion.Euler(new Vector3(0, Quaternion.LookRotation(targetPostition).eulerAngles.y + 180, 0));

        anim.Play("TextCrawl");

        //powerupAudio.PlayOneShot(beaverGulp);

        StartCoroutine(DelayedDestroy());
    }

    private IEnumerator DelayedDestroy()
    {
        yield return new WaitForSeconds(textDuration);
        Destroy(gameObject);
    }
}
