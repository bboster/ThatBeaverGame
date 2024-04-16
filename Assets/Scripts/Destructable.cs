using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructable : MonoBehaviour
{
    //public static event EventHandler<FractureEventArgs> fractured;

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
        yield return new WaitForSeconds(5);
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
