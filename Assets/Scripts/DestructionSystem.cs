using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;


public class DestructionSystem : MonoBehaviour
{
    public Fracture fracture;
    UnityEvent myEvent = new UnityEvent();
    public event Action fractureEvent;
    
    public void Start()
    {
        fractureEvent += isFractured;
    }
    public IEnumerator DeleteFragments()
    {
        yield return new WaitForSeconds(5);
        Destroy(gameObject);
    }

    public void Update()
    {
        isFractured();
    }

    void isFractured()
    {
        Debug.Log(fracture.getFractures().Count);
    }
}

