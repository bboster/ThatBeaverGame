using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class FractureableSO : ScriptableObject
{
    [Header("Custom Fracture Options")]
    [Tooltip("The force required for GnawAttack to cause fracture.")]
    public float minForceToTrigger = 0;

    public float rigidbodyMass = 10;

    [Header("Base Fracture Options")]
    public TriggerOptions triggerOptions;
    public FractureOptions fractureOptions;
    public RefractureOptions refractureOptions;
    public CallbackOptions callbackOptions;
}
