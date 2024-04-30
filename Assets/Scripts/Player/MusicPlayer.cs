using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    [SerializeField]
    AudioClip musicIntro;
    [SerializeField]
    AudioClip musicLoop;
    AudioSource musicAudio;
    // Start is called before the first frame update
    void Start()
    {
        musicAudio = GetComponent<AudioSource>();
        musicAudio.PlayOneShot(musicIntro);
        StartCoroutine(LoopTheMusic());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator LoopTheMusic()
    {
        yield return new WaitForSeconds(5f);
        musicAudio.clip = musicLoop;
        musicAudio.Play();
    }
}
