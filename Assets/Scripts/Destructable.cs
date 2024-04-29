using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructable : MonoBehaviour
{
    //public static event EventHandler<FractureEventArgs> fractured;
    [SerializeField]
    float destructionDelayMax = 12;
    [SerializeField]
    float destructionDelayMin = 5;
    [Space]
    [SerializeField]
    float shrinkDuration = 1;
    [SerializeField]
    float scaleRate = 1;

    //private PointSystem PS;

    AudioSource destructableAudio;
    [SerializeField]
    AudioClip collapseSFX;

    public void Start()
    {
        Fragmenter.fractured += OnFractureEvent;
        destructableAudio = GetComponent<AudioSource>();
    }

    public void OnDestroy()
    {
        Fragmenter.fractured -= OnFractureEvent;
    }

    private void OnFractureEvent(object obj, FractureEventArgs e)
    {
        StartCoroutine(DelayedDestroy(e.obj));
    }

    private IEnumerator DelayedDestroy(GameObject obj)
    {
        destructableAudio.PlayOneShot(collapseSFX);

        Collider col = obj.GetComponent<Collider>();
        yield return new WaitForSeconds(Random.Range(destructionDelayMin, destructionDelayMax));

        col.enabled = false;
        for (float i = 0; i < shrinkDuration; i += 0.025f)
        {
            yield return new WaitForSeconds(0.025f);
            obj.transform.localScale /= scaleRate;
        }
        Destroy(obj);
    }
}

/*public class FractureEventArgs
{
    public GameObject obj;

    public FractureEventArgs(GameObject obj)
    {
        this.obj = obj;
    }
}*/
