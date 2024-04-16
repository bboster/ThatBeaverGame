using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructable : MonoBehaviour
{
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
