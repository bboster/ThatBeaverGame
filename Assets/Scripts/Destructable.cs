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
    

    public void Awake()
    {
        Fragmenter.fractured += fn;
    }

    private void fn(object obj, FractureEventArgs e)
    {
        StartCoroutine(DelayedDestroy(e.obj));
    }

    private IEnumerator DelayedDestroy(GameObject obj)
    {
        yield return new WaitForSeconds(Random.Range(destructionDelayMin, destructionDelayMax));
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
